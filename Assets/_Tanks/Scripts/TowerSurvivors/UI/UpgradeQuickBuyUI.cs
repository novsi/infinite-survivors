using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class UpgradeQuickBuyUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform m_SlotContainer;
        [SerializeField] private GoldManager m_GoldManager;
        [SerializeField] private UpgradeManager m_UpgradeManager;

        private UpgradeData[] m_UpgradeDataAssets;
        private List<UpgradeSlot> m_Slots = new List<UpgradeSlot>();

        private class UpgradeSlot
        {
            public UpgradeData data;
            public Button button;
            public Image icon;
            public TextMeshProUGUI costText;
            public CanvasGroup canvasGroup;
        }

        private void Awake()
        {
            if (m_GoldManager == null)
                m_GoldManager = FindObjectOfType<GoldManager>();
            if (m_UpgradeManager == null)
                m_UpgradeManager = FindObjectOfType<UpgradeManager>();
        }

        private void Start()
        {
            m_UpgradeDataAssets = Resources.LoadAll<UpgradeData>("UpgradeData");

            BuildSlots();

            if (m_GoldManager != null)
                m_GoldManager.OnGoldChanged.AddListener(OnGoldChanged);

            if (m_UpgradeManager != null)
                m_UpgradeManager.OnUpgradePurchased.AddListener(OnUpgradePurchased);

            if (TowerSurvivorsGameManager.Instance != null)
                TowerSurvivorsGameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);

            GameState currentState = TowerSurvivorsGameManager.Instance?.CurrentGameState ?? GameState.MainMenu;
            OnGameStateChanged(currentState);
        }

        private void BuildSlots()
        {
            if (m_SlotContainer == null || m_UpgradeDataAssets == null) return;

            int count = Mathf.Min(3, m_UpgradeDataAssets.Length);
            for (int i = 0; i < count; i++)
            {
                UpgradeData data = m_UpgradeDataAssets[i];
                GameObject slotObj = CreateSlotObject(data, i);
                slotObj.transform.SetParent(m_SlotContainer, false);

                Button btn = slotObj.GetComponent<Button>();
                Image icon = slotObj.transform.Find("Icon")?.GetComponent<Image>();
                TextMeshProUGUI cost = slotObj.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
                CanvasGroup cg = slotObj.GetComponent<CanvasGroup>();

                UpgradeSlot slot = new UpgradeSlot
                {
                    data = data,
                    button = btn,
                    icon = icon,
                    costText = cost,
                    canvasGroup = cg
                };
                m_Slots.Add(slot);

                int index = i;
                btn.onClick.AddListener(() => OnSlotClicked(index));

                RefreshSlot(slot);
            }
        }

        private GameObject CreateSlotObject(UpgradeData data, int index)
        {
            GameObject slot = new GameObject($"UpgradeSlot_{index}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup));
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(70, 90);

            Image bg = slot.GetComponent<Image>();
            bg.color = new Color(0.102f, 0.102f, 0.18f, 0.9f);

            // Button color states
            Button btn = slot.GetComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.102f, 0.102f, 0.18f, 0.9f);
            cb.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            cb.pressedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
            cb.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
            btn.colors = cb;

            // Icon (colored square placeholder)
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(slot.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 1f);
            iconRt.anchorMax = new Vector2(0.5f, 1f);
            iconRt.pivot = new Vector2(0.5f, 1f);
            iconRt.anchoredPosition = new Vector2(0, -5);
            iconRt.sizeDelta = new Vector2(50, 50);
            Image iconImg = iconObj.GetComponent<Image>();
            if (data.Icon != null)
                iconImg.sprite = data.Icon;
            else
                iconImg.color = data.UpgradeColor;

            // Cost text
            GameObject costObj = new GameObject("CostText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            costObj.transform.SetParent(slot.transform, false);
            RectTransform costRt = costObj.GetComponent<RectTransform>();
            costRt.anchorMin = new Vector2(0, 0);
            costRt.anchorMax = new Vector2(1, 0);
            costRt.pivot = new Vector2(0.5f, 0);
            costRt.anchoredPosition = new Vector2(0, 3);
            costRt.sizeDelta = new Vector2(0, 25);
            TextMeshProUGUI costTmp = costObj.GetComponent<TextMeshProUGUI>();
            costTmp.fontSize = 12;
            costTmp.alignment = TextAlignmentOptions.Center;
            costTmp.color = new Color(1f, 0.7f, 0.28f, 1f);

            return slot;
        }

        private void OnSlotClicked(int index)
        {
            if (index < 0 || index >= m_Slots.Count) return;
            UpgradeSlot slot = m_Slots[index];
            if (slot.data == null || m_UpgradeManager == null) return;

            bool success = m_UpgradeManager.PurchaseUpgrade(slot.data);
            if (success)
            {
                RefreshSlot(slot);
            }
        }

        private void RefreshSlot(UpgradeSlot slot)
        {
            if (slot.data == null || m_UpgradeManager == null) return;

            int stacks = m_UpgradeManager.GetUpgradeStackCount(slot.data);
            int cost = slot.data.GetCostForPurchase(stacks + 1);
            bool canAfford = m_GoldManager != null && m_GoldManager.CanAfford(cost);
            bool isMaxed = !slot.data.UnlimitedStacks && stacks >= slot.data.MaxStacks;

            if (slot.costText != null)
                slot.costText.text = isMaxed ? "MAX" : $"{cost}g";

            if (slot.canvasGroup != null)
                slot.canvasGroup.alpha = (canAfford && !isMaxed) ? 1f : 0.4f;

            if (slot.button != null)
                slot.button.interactable = canAfford && !isMaxed;
        }

        private void OnGoldChanged(float newGold)
        {
            foreach (var slot in m_Slots)
                RefreshSlot(slot);
        }

        private void OnUpgradePurchased(UpgradeData data, int stacks)
        {
            foreach (var slot in m_Slots)
            {
                if (slot.data == data)
                    RefreshSlot(slot);
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Playing);
        }

        private void OnDestroy()
        {
            if (m_GoldManager != null)
                m_GoldManager.OnGoldChanged.RemoveListener(OnGoldChanged);
            if (m_UpgradeManager != null)
                m_UpgradeManager.OnUpgradePurchased.RemoveListener(OnUpgradePurchased);
            if (TowerSurvivorsGameManager.Instance != null)
                TowerSurvivorsGameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            foreach (var slot in m_Slots)
            {
                if (slot.button != null)
                    slot.button.onClick.RemoveAllListeners();
            }
        }
    }
}
