using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TowerSurvivors
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private int m_MaxWeapons = 8;
        [SerializeField] private Transform m_WeaponParent;               // Parent transform for weapon instances
        
        [Header("Positioning")]
        [SerializeField] private float m_WeaponRadius = 2f;             // Distance from center for weapon placement
        [SerializeField] private bool m_AutoArrangeWeapons = true;      // Auto-arrange weapons in circle
        
        [Header("Events")]
        public UnityEvent<Weapon> OnWeaponAdded;            // Event when weapon is added
        public UnityEvent<Weapon> OnWeaponRemoved;          // Event when weapon is removed
        public UnityEvent<List<Weapon>> OnWeaponsChanged;   // Event when weapon list changes
        
        private List<Weapon> m_EquippedWeapons = new List<Weapon>();
        private Dictionary<WeaponData, int> m_WeaponCounts = new Dictionary<WeaponData, int>();
        
        // Upgrade multipliers applied to all weapons
        private float m_GlobalDamageMultiplier = 1f;
        private float m_GlobalFireRateMultiplier = 1f;
        private float m_GlobalRangeMultiplier = 1f;
        
        public List<Weapon> EquippedWeapons => new List<Weapon>(m_EquippedWeapons); // Return copy for safety
        public int WeaponCount => m_EquippedWeapons.Count;
        public int MaxWeapons => m_MaxWeapons;
        public bool CanAddMoreWeapons => m_EquippedWeapons.Count < m_MaxWeapons;
        public Dictionary<WeaponData, int> WeaponCounts => new Dictionary<WeaponData, int>(m_WeaponCounts);
        
        private void Awake()
        {
            // Initialize events
            OnWeaponAdded ??= new UnityEvent<Weapon>();
            OnWeaponRemoved ??= new UnityEvent<Weapon>();
            OnWeaponsChanged ??= new UnityEvent<List<Weapon>>();
            
            // Set weapon parent to this transform if not assigned
            if (m_WeaponParent == null)
                m_WeaponParent = transform;
        }
        
        private void Start()
        {
            // Clean up any existing weapons in the scene that aren't tracked
            CleanupUnmanagedWeapons();
        }
        
        private void CleanupUnmanagedWeapons()
        {
            Weapon[] existingWeapons = m_WeaponParent.GetComponentsInChildren<Weapon>();
            foreach (Weapon weapon in existingWeapons)
            {
                if (!m_EquippedWeapons.Contains(weapon))
                {
                    Debug.Log($"Found unmanaged weapon {weapon.name}, adding to manager");
                    m_EquippedWeapons.Add(weapon);
                    
                    // Update weapon count
                    if (weapon.WeaponData != null)
                    {
                        UpdateWeaponCount(weapon.WeaponData, 1);
                    }
                }
            }
            
            if (existingWeapons.Length > 0)
            {
                ArrangeWeapons();
                OnWeaponsChanged?.Invoke(m_EquippedWeapons);
            }
        }
        
        public bool AddWeapon(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogError("Cannot add weapon: WeaponData is null");
                return false;
            }
            
            // Check if we can add more weapons
            if (!CanAddMoreWeapons)
            {
                Debug.LogWarning($"Cannot add weapon: Maximum weapons ({m_MaxWeapons}) already equipped");
                return false;
            }
            
            // Check if weapon allows multiple instances
            if (!weaponData.AllowMultiple && GetWeaponCount(weaponData) > 0)
            {
                Debug.LogWarning($"Cannot add weapon: {weaponData.WeaponName} does not allow multiple instances");
                return false;
            }
            
            // Create weapon prefab (for now, create empty GameObject with Weapon component)
            GameObject weaponObject = CreateWeaponObject(weaponData);
            if (weaponObject == null)
            {
                Debug.LogError($"Failed to create weapon object for {weaponData.WeaponName}");
                return false;
            }
            
            // Get the Weapon component
            Weapon weaponComponent = weaponObject.GetComponent<Weapon>();
            if (weaponComponent == null)
            {
                Debug.LogError($"Weapon object for {weaponData.WeaponName} does not have Weapon component");
                Destroy(weaponObject);
                return false;
            }
            
            // Add to equipped weapons
            m_EquippedWeapons.Add(weaponComponent);
            UpdateWeaponCount(weaponData, 1);
            
            // Apply global upgrades
            ApplyGlobalUpgradesToWeapon(weaponComponent);
            
            // Arrange weapons
            ArrangeWeapons();
            
            // Trigger events
            OnWeaponAdded?.Invoke(weaponComponent);
            OnWeaponsChanged?.Invoke(m_EquippedWeapons);
            
            Debug.Log($"Added weapon: {weaponData.WeaponName}. Total weapons: {m_EquippedWeapons.Count}");
            return true;
        }
        
        public bool AddWeapon(GameObject weaponPrefab)
        {
            if (weaponPrefab == null)
            {
                Debug.LogError("Cannot add weapon: Weapon prefab is null");
                return false;
            }
            
            if (!CanAddMoreWeapons)
            {
                Debug.LogWarning($"Cannot add weapon: Maximum weapons ({m_MaxWeapons}) already equipped");
                return false;
            }
            
            // Instantiate weapon prefab
            GameObject weaponInstance = Instantiate(weaponPrefab, m_WeaponParent);
            
            Weapon weaponComponent = weaponInstance.GetComponent<Weapon>();
            if (weaponComponent == null)
            {
                Debug.LogError($"Weapon prefab {weaponPrefab.name} does not have Weapon component");
                Destroy(weaponInstance);
                return false;
            }
            
            // Check weapon data
            if (weaponComponent.WeaponData == null)
            {
                Debug.LogError($"Weapon {weaponPrefab.name} has no WeaponData assigned");
                Destroy(weaponInstance);
                return false;
            }
            
            // Check if weapon allows multiple instances
            if (!weaponComponent.WeaponData.AllowMultiple && GetWeaponCount(weaponComponent.WeaponData) > 0)
            {
                Debug.LogWarning($"Cannot add weapon: {weaponComponent.WeaponData.WeaponName} does not allow multiple instances");
                Destroy(weaponInstance);
                return false;
            }
            
            // Add to equipped weapons
            m_EquippedWeapons.Add(weaponComponent);
            UpdateWeaponCount(weaponComponent.WeaponData, 1);
            
            // Apply global upgrades
            ApplyGlobalUpgradesToWeapon(weaponComponent);
            
            // Arrange weapons
            ArrangeWeapons();
            
            // Trigger events
            OnWeaponAdded?.Invoke(weaponComponent);
            OnWeaponsChanged?.Invoke(m_EquippedWeapons);
            
            Debug.Log($"Added weapon: {weaponComponent.WeaponData.WeaponName}. Total weapons: {m_EquippedWeapons.Count}");
            return true;
        }
        
        public bool RemoveWeapon(Weapon weapon)
        {
            if (weapon == null || !m_EquippedWeapons.Contains(weapon))
            {
                return false;
            }
            
            // Remove from equipped weapons
            m_EquippedWeapons.Remove(weapon);
            
            // Update weapon count
            if (weapon.WeaponData != null)
            {
                UpdateWeaponCount(weapon.WeaponData, -1);
            }
            
            // Destroy weapon object
            Destroy(weapon.gameObject);
            
            // Arrange remaining weapons
            ArrangeWeapons();
            
            // Trigger events
            OnWeaponRemoved?.Invoke(weapon);
            OnWeaponsChanged?.Invoke(m_EquippedWeapons);
            
            return true;
        }
        
        public void RemoveAllWeapons()
        {
            for (int i = m_EquippedWeapons.Count - 1; i >= 0; i--)
            {
                RemoveWeapon(m_EquippedWeapons[i]);
            }
        }
        
        private GameObject CreateWeaponObject(WeaponData weaponData)
        {
            // Create weapon GameObject
            GameObject weaponObject = new GameObject($"Weapon_{weaponData.WeaponName}");
            weaponObject.transform.SetParent(m_WeaponParent);
            
            // Add Weapon component and configure it
            Weapon weaponComponent = weaponObject.AddComponent<Weapon>();
            
            // Use reflection to set the weapon data (since it's private)
            // This is a workaround - ideally the Weapon class would have a public setter
            var weaponDataField = typeof(Weapon).GetField("m_WeaponData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            weaponDataField?.SetValue(weaponComponent, weaponData);
            
            // Add AudioSource if weapon has fire sound
            if (weaponData.FireSound != null)
            {
                weaponObject.AddComponent<AudioSource>();
            }
            
            return weaponObject;
        }
        
        private void ArrangeWeapons()
        {
            if (!m_AutoArrangeWeapons || m_EquippedWeapons.Count == 0) return;
            
            // Arrange weapons in a circle around the tower
            for (int i = 0; i < m_EquippedWeapons.Count; i++)
            {
                if (m_EquippedWeapons[i] == null) continue;
                
                float angle = (360f / m_EquippedWeapons.Count) * i;
                float radians = angle * Mathf.Deg2Rad;
                
                Vector3 position = new Vector3(
                    Mathf.Cos(radians) * m_WeaponRadius,
                    0f,
                    Mathf.Sin(radians) * m_WeaponRadius
                );
                
                m_EquippedWeapons[i].transform.localPosition = position;
                m_EquippedWeapons[i].transform.localRotation = Quaternion.identity;
            }
        }
        
        private void UpdateWeaponCount(WeaponData weaponData, int change)
        {
            if (!m_WeaponCounts.ContainsKey(weaponData))
            {
                m_WeaponCounts[weaponData] = 0;
            }
            
            m_WeaponCounts[weaponData] += change;
            
            if (m_WeaponCounts[weaponData] <= 0)
            {
                m_WeaponCounts.Remove(weaponData);
            }
        }
        
        public int GetWeaponCount(WeaponData weaponData)
        {
            return m_WeaponCounts.TryGetValue(weaponData, out int count) ? count : 0;
        }
        
        public void SetMaxWeapons(int maxWeapons)
        {
            m_MaxWeapons = Mathf.Max(1, maxWeapons);
        }
        
        public void ApplyGlobalDamageUpgrade(float multiplier)
        {
            m_GlobalDamageMultiplier *= multiplier;
            
            foreach (Weapon weapon in m_EquippedWeapons)
            {
                if (weapon != null)
                {
                    weapon.ApplyDamageUpgrade(multiplier);
                }
            }
        }
        
        public void ApplyGlobalFireRateUpgrade(float multiplier)
        {
            m_GlobalFireRateMultiplier *= multiplier;
            
            foreach (Weapon weapon in m_EquippedWeapons)
            {
                if (weapon != null)
                {
                    weapon.ApplyFireRateUpgrade(multiplier);
                }
            }
        }
        
        public void ApplyGlobalRangeUpgrade(float multiplier)
        {
            m_GlobalRangeMultiplier *= multiplier;
            
            foreach (Weapon weapon in m_EquippedWeapons)
            {
                if (weapon != null)
                {
                    weapon.ApplyRangeUpgrade(multiplier);
                }
            }
        }
        
        private void ApplyGlobalUpgradesToWeapon(Weapon weapon)
        {
            if (weapon == null) return;
            
            weapon.ApplyDamageUpgrade(m_GlobalDamageMultiplier);
            weapon.ApplyFireRateUpgrade(m_GlobalFireRateMultiplier);
            weapon.ApplyRangeUpgrade(m_GlobalRangeMultiplier);
        }
        
        public void ResetAllUpgrades()
        {
            m_GlobalDamageMultiplier = 1f;
            m_GlobalFireRateMultiplier = 1f;
            m_GlobalRangeMultiplier = 1f;
            
            foreach (Weapon weapon in m_EquippedWeapons)
            {
                if (weapon != null)
                {
                    weapon.ResetUpgrades();
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw weapon placement circle
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_WeaponRadius);
            
            // Draw weapon positions
            if (m_AutoArrangeWeapons && m_EquippedWeapons.Count > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < m_MaxWeapons; i++)
                {
                    float angle = (360f / m_MaxWeapons) * i;
                    float radians = angle * Mathf.Deg2Rad;
                    
                    Vector3 position = transform.position + new Vector3(
                        Mathf.Cos(radians) * m_WeaponRadius,
                        0f,
                        Mathf.Sin(radians) * m_WeaponRadius
                    );
                    
                    Gizmos.DrawWireSphere(position, 0.5f);
                }
            }
        }
        
        // For testing purposes
        [ContextMenu("Test Add Weapon")]
        private void TestAddWeapon()
        {
            // This would need a weapon data asset to test with
            Debug.Log("Test Add Weapon - needs WeaponData asset");
        }
    }
}