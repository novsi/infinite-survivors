using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class WeaponInventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform m_WeaponSlotContainer;      // Parent for weapon slots
        [SerializeField] private ScrollRect m_ScrollRect;              // For scrolling when > 8 weapons
        [SerializeField] private GameObject m_WeaponSlotPrefab;        // Prefab for each weapon slot
        [SerializeField] private TextMeshProUGUI m_EquippedLabel;      // "EQUIPPED" label

        [Header("Slot Settings")]
        [SerializeField] private int m_MaxVisibleSlots = 8;
        [SerializeField] private float m_SlotSize = 40f;
        [SerializeField] private float m_SlotSpacing = 6f;
        
        [Header("Pulse Effect")]
        [SerializeField] private Color m_PulseColor = new Color(1f, 1f, 0f, 1f);  // Yellow pulse
        [SerializeField] private float m_PulseDuration = 0.2f;
        [SerializeField] private float m_PulseScale = 1.2f;
        
        [Header("Manager References")]
        [SerializeField] private WeaponManager m_WeaponManager;
        
        // Dictionary to track weapon slots
        private Dictionary<Weapon, WeaponSlot> m_WeaponSlots = new Dictionary<Weapon, WeaponSlot>();
        private List<GameObject> m_PlaceholderSlots = new List<GameObject>();
        
        // Internal class for weapon slot data
        private class WeaponSlot
        {
            public GameObject SlotObject;
            public Image IconImage;
            public Image BackgroundImage;
            public TextMeshProUGUI NameText;
            public Coroutine PulseCoroutine;
            public Color OriginalBackgroundColor;
            public Vector3 OriginalScale;
        }
        
        private void Awake()
        {
            if (m_WeaponManager == null)
            {
                m_WeaponManager = FindObjectOfType<WeaponManager>();
            }

            // Auto-find EquippedLabel if not assigned
            if (m_EquippedLabel == null)
            {
                Transform t = transform.Find("EquippedLabel");
                if (t != null) m_EquippedLabel = t.GetComponent<TextMeshProUGUI>();
            }
        }
        
        private void Start()
        {
            if (m_WeaponManager == null)
            {
                Debug.LogError("WeaponInventoryUI: No WeaponManager found!");
                return;
            }

            // Subscribe to WeaponManager events
            m_WeaponManager.OnWeaponAdded.AddListener(OnWeaponAdded);
            m_WeaponManager.OnWeaponRemoved.AddListener(OnWeaponRemoved);

            // Subscribe to game state changes
            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
            }

            // Initialize UI with existing weapons
            InitializeExistingWeapons();

            // Set correct initial visibility based on current game state
            GameState currentState = TowerSurvivorsGameManager.Instance?.CurrentGameState ?? GameState.MainMenu;
            OnGameStateChanged(currentState);
        }

        private void OnGameStateChanged(GameState newState)
        {
            // Show inventory during Playing and Paused states, hide during MainMenu and GameOver
            bool showInventory = newState == GameState.Playing || newState == GameState.Paused;
            gameObject.SetActive(showInventory);
        }
        
        private void InitializeExistingWeapons()
        {
            if (m_WeaponManager == null) return;

            foreach (Weapon weapon in m_WeaponManager.EquippedWeapons)
            {
                OnWeaponAdded(weapon);
            }

            RefreshPlaceholderSlots();
        }

        private void RefreshPlaceholderSlots()
        {
            // Clear existing placeholders
            foreach (var placeholder in m_PlaceholderSlots)
            {
                if (placeholder != null) Destroy(placeholder);
            }
            m_PlaceholderSlots.Clear();

            // Create placeholder slots to fill up to m_MaxVisibleSlots
            int slotsNeeded = m_MaxVisibleSlots - m_WeaponSlots.Count;
            for (int i = 0; i < slotsNeeded; i++)
            {
                GameObject placeholder = CreatePlaceholderSlot(i);
                if (placeholder != null) m_PlaceholderSlots.Add(placeholder);
            }
        }

        private GameObject CreatePlaceholderSlot(int index)
        {
            if (m_WeaponSlotContainer == null) return null;

            GameObject slotObj = new GameObject($"EmptySlot_{index}");
            slotObj.transform.SetParent(m_WeaponSlotContainer, false);

            RectTransform rectTransform = slotObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(m_SlotSize, m_SlotSize);

            Image bgImage = slotObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.4f); // Dim placeholder

            // Add outline for dim border effect
            var outline = slotObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            outline.effectDistance = new Vector2(1f, 1f);

            LayoutElement layoutElement = slotObj.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = m_SlotSize;
            layoutElement.preferredHeight = m_SlotSize;

            return slotObj;
        }
        
        private void OnWeaponAdded(Weapon weapon)
        {
            if (weapon == null || m_WeaponSlots.ContainsKey(weapon)) return;

            // Create new weapon slot
            WeaponSlot slot = CreateWeaponSlot(weapon);
            if (slot != null)
            {
                m_WeaponSlots.Add(weapon, slot);

                // Subscribe to weapon fire event
                weapon.OnFire.AddListener(() => OnWeaponFired(weapon));
            }

            RefreshPlaceholderSlots();
            UpdateScrollability();
        }
        
        private void OnWeaponRemoved(Weapon weapon)
        {
            if (weapon == null || !m_WeaponSlots.ContainsKey(weapon)) return;
            
            WeaponSlot slot = m_WeaponSlots[weapon];
            
            // Stop any active pulse
            if (slot.PulseCoroutine != null)
            {
                StopCoroutine(slot.PulseCoroutine);
            }
            
            // Unsubscribe from weapon events (weapon might be destroyed)
            if (weapon != null)
            {
                weapon.OnFire.RemoveAllListeners();
            }
            
            // Destroy slot UI
            if (slot.SlotObject != null)
            {
                Destroy(slot.SlotObject);
            }
            
            m_WeaponSlots.Remove(weapon);
            RefreshPlaceholderSlots();
            UpdateScrollability();
        }
        
        private WeaponSlot CreateWeaponSlot(Weapon weapon)
        {
            if (weapon == null || weapon.WeaponData == null) return null;
            
            WeaponSlot slot = new WeaponSlot();
            
            // Create slot GameObject
            if (m_WeaponSlotPrefab != null)
            {
                slot.SlotObject = Instantiate(m_WeaponSlotPrefab, m_WeaponSlotContainer);
            }
            else
            {
                // Create slot dynamically if no prefab
                slot.SlotObject = CreateSlotDynamically(weapon.WeaponData);
            }
            
            if (slot.SlotObject == null) return null;
            
            // Find UI components
            slot.IconImage = slot.SlotObject.transform.Find("WeaponIcon")?.GetComponent<Image>();
            slot.BackgroundImage = slot.SlotObject.GetComponent<Image>();
            slot.NameText = slot.SlotObject.GetComponentInChildren<TextMeshProUGUI>();
            
            // Store original values for pulse reset
            if (slot.BackgroundImage != null)
            {
                slot.OriginalBackgroundColor = slot.BackgroundImage.color;
            }
            slot.OriginalScale = slot.SlotObject.transform.localScale;
            
            // Configure slot with weapon data
            ConfigureSlot(slot, weapon.WeaponData);
            
            return slot;
        }
        
        private GameObject CreateSlotDynamically(WeaponData weaponData)
        {
            // Create the slot container - compact square for horizontal bar
            GameObject slotObj = new GameObject($"WeaponSlot_{weaponData.WeaponName}");
            slotObj.transform.SetParent(m_WeaponSlotContainer, false);

            RectTransform rectTransform = slotObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(m_SlotSize, m_SlotSize);

            Image bgImage = slotObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Add outline border
            var outline = slotObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            outline.effectDistance = new Vector2(1f, 1f);

            LayoutElement layoutElement = slotObj.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = m_SlotSize;
            layoutElement.preferredHeight = m_SlotSize;

            // Create icon image child - fills most of the slot
            GameObject iconObj = new GameObject("WeaponIcon");
            iconObj.transform.SetParent(slotObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;

            // Create name text child - hidden in horizontal mode, used for tooltip
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(slotObj.transform, false);
            nameObj.SetActive(false); // Hidden in horizontal compact layout

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.25f);
            nameRect.offsetMin = new Vector2(2f, 0f);
            nameRect.offsetMax = new Vector2(-2f, 0f);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.fontSize = 8f;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            return slotObj;
        }
        
        private void ConfigureSlot(WeaponSlot slot, WeaponData weaponData)
        {
            if (weaponData == null) return;
            
            // Set icon
            if (slot.IconImage != null)
            {
                if (weaponData.WeaponIcon != null)
                {
                    slot.IconImage.sprite = weaponData.WeaponIcon;
                    slot.IconImage.enabled = true;
                }
                else
                {
                    // Use a placeholder color based on rarity if no icon
                    slot.IconImage.sprite = null;
                    slot.IconImage.color = weaponData.RarityColor;
                }
            }
            
            // Set name
            if (slot.NameText != null)
            {
                slot.NameText.text = weaponData.WeaponName;
                slot.NameText.color = weaponData.RarityColor;
            }
            
            // Set rarity border color
            if (slot.BackgroundImage != null)
            {
                // Create subtle border using rarity color
                Color bgColor = new Color(
                    weaponData.RarityColor.r * 0.3f,
                    weaponData.RarityColor.g * 0.3f,
                    weaponData.RarityColor.b * 0.3f,
                    0.8f
                );
                slot.BackgroundImage.color = bgColor;
                slot.OriginalBackgroundColor = bgColor;
            }
        }
        
        private void OnWeaponFired(Weapon weapon)
        {
            if (weapon == null || !m_WeaponSlots.ContainsKey(weapon)) return;
            
            WeaponSlot slot = m_WeaponSlots[weapon];
            
            // Start pulse effect
            if (slot.PulseCoroutine != null)
            {
                StopCoroutine(slot.PulseCoroutine);
            }
            
            slot.PulseCoroutine = StartCoroutine(PulseSlotCoroutine(slot));
        }
        
        private IEnumerator PulseSlotCoroutine(WeaponSlot slot)
        {
            if (slot.SlotObject == null) yield break;
            
            float elapsed = 0f;
            
            // Pulse up
            while (elapsed < m_PulseDuration / 2f)
            {
                float t = elapsed / (m_PulseDuration / 2f);
                
                // Scale up
                if (slot.SlotObject != null)
                {
                    slot.SlotObject.transform.localScale = Vector3.Lerp(
                        slot.OriginalScale,
                        slot.OriginalScale * m_PulseScale,
                        t
                    );
                }
                
                // Color change
                if (slot.BackgroundImage != null)
                {
                    slot.BackgroundImage.color = Color.Lerp(
                        slot.OriginalBackgroundColor,
                        m_PulseColor,
                        t
                    );
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Reset elapsed for pulse down
            elapsed = 0f;
            
            // Pulse down
            while (elapsed < m_PulseDuration / 2f)
            {
                float t = elapsed / (m_PulseDuration / 2f);
                
                // Scale down
                if (slot.SlotObject != null)
                {
                    slot.SlotObject.transform.localScale = Vector3.Lerp(
                        slot.OriginalScale * m_PulseScale,
                        slot.OriginalScale,
                        t
                    );
                }
                
                // Color restore
                if (slot.BackgroundImage != null)
                {
                    slot.BackgroundImage.color = Color.Lerp(
                        m_PulseColor,
                        slot.OriginalBackgroundColor,
                        t
                    );
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Ensure final state
            if (slot.SlotObject != null)
            {
                slot.SlotObject.transform.localScale = slot.OriginalScale;
            }
            if (slot.BackgroundImage != null)
            {
                slot.BackgroundImage.color = slot.OriginalBackgroundColor;
            }
            
            slot.PulseCoroutine = null;
        }
        
        private void UpdateScrollability()
        {
            if (m_ScrollRect == null) return;

            // Enable/disable horizontal scrolling based on weapon count
            bool needsScroll = m_WeaponSlots.Count > m_MaxVisibleSlots;
            m_ScrollRect.horizontal = needsScroll;
            m_ScrollRect.vertical = false;
        }
        
        // Public methods for external control
        public void SetWeaponManagerReference(WeaponManager weaponManager)
        {
            // Unsubscribe from old manager
            if (m_WeaponManager != null)
            {
                m_WeaponManager.OnWeaponAdded.RemoveListener(OnWeaponAdded);
                m_WeaponManager.OnWeaponRemoved.RemoveListener(OnWeaponRemoved);
            }
            
            // Clear existing slots
            ClearAllSlots();
            
            // Set new reference
            m_WeaponManager = weaponManager;
            
            if (m_WeaponManager != null)
            {
                m_WeaponManager.OnWeaponAdded.AddListener(OnWeaponAdded);
                m_WeaponManager.OnWeaponRemoved.AddListener(OnWeaponRemoved);
                InitializeExistingWeapons();
            }
        }
        
        public void ClearAllSlots()
        {
            foreach (var kvp in m_WeaponSlots)
            {
                if (kvp.Value.PulseCoroutine != null)
                {
                    StopCoroutine(kvp.Value.PulseCoroutine);
                }
                if (kvp.Value.SlotObject != null)
                {
                    Destroy(kvp.Value.SlotObject);
                }
            }
            m_WeaponSlots.Clear();

            foreach (var placeholder in m_PlaceholderSlots)
            {
                if (placeholder != null) Destroy(placeholder);
            }
            m_PlaceholderSlots.Clear();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from WeaponManager events
            if (m_WeaponManager != null)
            {
                m_WeaponManager.OnWeaponAdded.RemoveListener(OnWeaponAdded);
                m_WeaponManager.OnWeaponRemoved.RemoveListener(OnWeaponRemoved);
            }

            // Unsubscribe from game state events
            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            }

            // Clear slots
            ClearAllSlots();
        }
        
        // Context menu for testing
        [ContextMenu("Refresh Weapon Display")]
        private void RefreshWeaponDisplay()
        {
            ClearAllSlots();
            InitializeExistingWeapons();
        }
    }
}
