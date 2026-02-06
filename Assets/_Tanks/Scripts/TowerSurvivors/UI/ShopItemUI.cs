using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_NameText;
        [SerializeField] private TextMeshProUGUI m_DescriptionText;
        [SerializeField] private TextMeshProUGUI m_CostText;
        [SerializeField] private Button m_BuyButton;
        [SerializeField] private Image m_IconImage;
        [SerializeField] private Image m_BackgroundImage;

        [Header("Visual Settings")]
        [SerializeField] private Color m_AffordableColor = new Color(0.102f, 0.102f, 0.18f, 0.9f);
        [SerializeField] private Color m_UnaffordableColor = new Color(0.102f, 0.102f, 0.18f, 0.9f);
        [SerializeField] private Color m_OwnedColor = new Color(0.1f, 0.3f, 0.1f, 0.9f);

        private WeaponData m_WeaponData;
        private UpgradeData m_UpgradeData;
        private string m_UpgradeName;
        private int m_Cost;
        private bool m_IsWeapon;
        private bool m_IsUpgrade;
        private bool m_IsAffordable;
        private bool m_IsOwned;
        private bool m_IsMaxed;
        private int m_CurrentStacks;
        private CanvasGroup m_CanvasGroup;

        private Action<WeaponData> m_OnWeaponPurchase;
        private Action<UpgradeData> m_OnUpgradeDataPurchase;
        private Action<string, int> m_OnUpgradePurchase;
        private UpgradeManager m_UpgradeManager;

        private void Awake()
        {
            FindUIReferences();
            EnsureCanvasGroup();

            if (m_BuyButton == null)
            {
                // Use the whole card as a button
                m_BuyButton = GetComponent<Button>();
            }

            if (m_BuyButton != null)
            {
                m_BuyButton.onClick.AddListener(OnBuyButtonClicked);
            }
        }

        private void FindUIReferences()
        {
            if (m_NameText == null)
                m_NameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

            if (m_DescriptionText == null)
                m_DescriptionText = transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();

            if (m_CostText == null)
                m_CostText = transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();

            if (m_BuyButton == null)
                m_BuyButton = transform.Find("BuyButton")?.GetComponent<Button>();

            if (m_IconImage == null)
                m_IconImage = transform.Find("Icon")?.GetComponent<Image>();

            if (m_BackgroundImage == null)
                m_BackgroundImage = GetComponent<Image>();
        }

        private void EnsureCanvasGroup()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Initialize(WeaponData weaponData, Action<WeaponData> onPurchase, WeaponManager weaponManager = null)
        {
            if (weaponData == null)
            {
                Debug.LogError("ShopItemUI: WeaponData is null");
                return;
            }

            m_WeaponData = weaponData;
            m_OnWeaponPurchase = onPurchase;
            m_IsWeapon = true;
            m_Cost = (int)weaponData.Cost;

            if (weaponManager != null && !weaponData.AllowMultiple && weaponManager.GetWeaponCount(weaponData) > 0)
            {
                m_IsOwned = true;
            }

            UpdateDisplay();
        }

        public void Initialize(UpgradeData upgradeData, Action<UpgradeData> onPurchase, UpgradeManager upgradeManager)
        {
            if (upgradeData == null)
            {
                Debug.LogError("ShopItemUI: UpgradeData is null");
                return;
            }

            m_UpgradeData = upgradeData;
            m_OnUpgradeDataPurchase = onPurchase;
            m_UpgradeManager = upgradeManager;
            m_IsWeapon = false;
            m_IsUpgrade = true;

            m_CurrentStacks = upgradeManager != null ? upgradeManager.GetUpgradeStackCount(upgradeData) : 0;
            m_Cost = upgradeData.GetCostForPurchase(m_CurrentStacks + 1);
            m_IsMaxed = !upgradeData.UnlimitedStacks && m_CurrentStacks >= upgradeData.MaxStacks;

            UpdateDisplay();
        }

        public void InitializePlaceholderUpgrade(string upgradeName, int cost, Action<string, int> onPurchase)
        {
            m_UpgradeName = upgradeName;
            m_Cost = cost;
            m_OnUpgradePurchase = onPurchase;
            m_IsWeapon = false;
            m_IsUpgrade = false;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // Compact grid card: hide name and description, show only icon + price
            if (m_NameText != null)
                m_NameText.gameObject.SetActive(false);

            if (m_DescriptionText != null)
                m_DescriptionText.gameObject.SetActive(false);

            // Update cost display - price number only
            if (m_CostText != null)
            {
                if (m_IsMaxed)
                    m_CostText.text = "MAX";
                else
                    m_CostText.text = $"{m_Cost}";

                m_CostText.fontSize = 14;
                m_CostText.color = new Color(1f, 0.7f, 0.28f, 1f); // Gold color
            }

            // Update icon
            if (m_IsWeapon && m_WeaponData != null)
            {
                if (m_IconImage != null && m_WeaponData.WeaponIcon != null)
                {
                    m_IconImage.sprite = m_WeaponData.WeaponIcon;
                    m_IconImage.gameObject.SetActive(true);
                }
                else if (m_IconImage != null)
                {
                    m_IconImage.gameObject.SetActive(false);
                }
            }
            else if (m_IsUpgrade && m_UpgradeData != null)
            {
                if (m_IconImage != null && m_UpgradeData.Icon != null)
                {
                    m_IconImage.sprite = m_UpgradeData.Icon;
                    m_IconImage.gameObject.SetActive(true);
                }
                else if (m_IconImage != null)
                {
                    m_IconImage.color = m_UpgradeData.UpgradeColor;
                    m_IconImage.gameObject.SetActive(true);
                }
            }

            // Set dark card background
            if (m_BackgroundImage != null)
            {
                m_BackgroundImage.color = m_AffordableColor;
            }

            // Hide buy button text if it exists (card itself is clickable)
            if (m_BuyButton != null)
            {
                TextMeshProUGUI buttonText = m_BuyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && buttonText != m_CostText && buttonText != m_NameText)
                {
                    buttonText.gameObject.SetActive(false);
                }
            }
        }

        public void UpdateAffordability(float currentGold)
        {
            m_IsAffordable = currentGold >= m_Cost;
            UpdateVisuals();
        }

        public void SetOwned(bool isOwned)
        {
            m_IsOwned = isOwned;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // Dim unaffordable items to alpha 0.4
            if (m_CanvasGroup != null)
            {
                if (m_IsMaxed)
                    m_CanvasGroup.alpha = 0.5f;
                else if (m_IsOwned)
                    m_CanvasGroup.alpha = 0.6f;
                else if (!m_IsAffordable)
                    m_CanvasGroup.alpha = 0.4f;
                else
                    m_CanvasGroup.alpha = 1f;
            }

            // Update button interactability
            if (m_BuyButton != null)
            {
                m_BuyButton.interactable = m_IsAffordable && !m_IsOwned && !m_IsMaxed;
            }

            // Update background
            if (m_BackgroundImage != null)
            {
                if (m_IsOwned)
                    m_BackgroundImage.color = m_OwnedColor;
                else
                    m_BackgroundImage.color = m_AffordableColor;
            }
        }

        private void OnBuyButtonClicked()
        {
            if (!m_IsAffordable || m_IsOwned || m_IsMaxed) return;

            if (m_IsWeapon && m_WeaponData != null)
            {
                m_OnWeaponPurchase?.Invoke(m_WeaponData);

                if (!m_WeaponData.AllowMultiple)
                {
                    SetOwned(true);
                }
            }
            else if (m_IsUpgrade && m_UpgradeData != null)
            {
                m_OnUpgradeDataPurchase?.Invoke(m_UpgradeData);
                RefreshUpgradeDisplay();
            }
            else if (!m_IsWeapon && !string.IsNullOrEmpty(m_UpgradeName))
            {
                m_OnUpgradePurchase?.Invoke(m_UpgradeName, m_Cost);
            }
        }

        public void RefreshUpgradeDisplay()
        {
            if (!m_IsUpgrade || m_UpgradeData == null || m_UpgradeManager == null) return;

            m_CurrentStacks = m_UpgradeManager.GetUpgradeStackCount(m_UpgradeData);
            m_Cost = m_UpgradeData.GetCostForPurchase(m_CurrentStacks + 1);
            m_IsMaxed = !m_UpgradeData.UnlimitedStacks && m_CurrentStacks >= m_UpgradeData.MaxStacks;

            UpdateDisplay();
            UpdateVisuals();
        }

        [ContextMenu("Test Buy")]
        private void TestBuy()
        {
            OnBuyButtonClicked();
        }

        private void OnDestroy()
        {
            if (m_BuyButton != null)
            {
                m_BuyButton.onClick.RemoveListener(OnBuyButtonClicked);
            }
        }
    }
}
