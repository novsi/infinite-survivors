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
        [SerializeField] private Color m_AffordableColor = Color.white;
        [SerializeField] private Color m_UnaffordableColor = Color.gray;
        [SerializeField] private Color m_OwnedColor = Color.green;
        
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

        private Action<WeaponData> m_OnWeaponPurchase;
        private Action<UpgradeData> m_OnUpgradeDataPurchase;
        private Action<string, int> m_OnUpgradePurchase;
        private UpgradeManager m_UpgradeManager;
        
        private void Awake()
        {
            // Auto-find UI references if not assigned
            FindUIReferences();
            
            // Setup button listener
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

            // Check if weapon is already owned (for non-multiple weapons)
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

            // Get current stack count and cost
            m_CurrentStacks = upgradeManager != null ? upgradeManager.GetUpgradeStackCount(upgradeData) : 0;
            m_Cost = upgradeData.GetCostForPurchase(m_CurrentStacks + 1);

            // Check if maxed out
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
            if (m_IsWeapon && m_WeaponData != null)
            {
                UpdateWeaponDisplay();
            }
            else if (m_IsUpgrade && m_UpgradeData != null)
            {
                UpdateUpgradeDataDisplay();
            }
            else if (!m_IsWeapon && !string.IsNullOrEmpty(m_UpgradeName))
            {
                UpdateUpgradeDisplay();
            }
        }

        private void UpdateUpgradeDataDisplay()
        {
            // Display upgrade information from UpgradeData
            if (m_NameText != null)
            {
                string stackInfo = m_UpgradeData.UnlimitedStacks ?
                    $" ({m_CurrentStacks})" :
                    $" ({m_CurrentStacks}/{m_UpgradeData.MaxStacks})";
                m_NameText.text = m_UpgradeData.UpgradeName + stackInfo;
            }

            if (m_DescriptionText != null)
            {
                m_DescriptionText.text = m_UpgradeData.GetFormattedDescription(m_CurrentStacks);
            }

            if (m_CostText != null)
            {
                if (m_IsMaxed)
                    m_CostText.text = "MAX";
                else
                    m_CostText.text = $"{m_Cost} Gold";
            }

            // Set icon if available
            if (m_IconImage != null && m_UpgradeData.Icon != null)
            {
                m_IconImage.sprite = m_UpgradeData.Icon;
                m_IconImage.gameObject.SetActive(true);
            }
            else if (m_IconImage != null)
            {
                // Use upgrade color as background if no icon
                m_IconImage.color = m_UpgradeData.UpgradeColor;
                m_IconImage.gameObject.SetActive(true);
            }
        }
        
        private void UpdateWeaponDisplay()
        {
            // Display weapon information
            if (m_NameText != null)
                m_NameText.text = m_WeaponData.WeaponName;
            
            if (m_DescriptionText != null)
            {
                string description = $"Damage: {m_WeaponData.Damage:F0}\n";
                description += $"Fire Rate: {1f / m_WeaponData.FireRate:F1}/sec\n";
                description += $"Range: {m_WeaponData.Range:F0}\n";
                description += $"Rarity: {m_WeaponData.Rarity}";
                m_DescriptionText.text = description;
            }
            
            if (m_CostText != null)
                m_CostText.text = $"{m_Cost} Gold";
            
            // Set icon if available
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
        
        private void UpdateUpgradeDisplay()
        {
            // Display upgrade information
            if (m_NameText != null)
                m_NameText.text = m_UpgradeName;
            
            if (m_DescriptionText != null)
            {
                // Generate description based on upgrade name
                string description = GenerateUpgradeDescription(m_UpgradeName);
                m_DescriptionText.text = description;
            }
            
            if (m_CostText != null)
                m_CostText.text = $"{m_Cost} Gold";
            
            // Hide icon for upgrades (or set a generic upgrade icon)
            if (m_IconImage != null)
            {
                m_IconImage.gameObject.SetActive(false);
            }
        }
        
        private string GenerateUpgradeDescription(string upgradeName)
        {
            // Simple description generation based on upgrade name
            if (upgradeName.Contains("Max HP"))
                return "Increases tower maximum health by 25 points. Can be purchased multiple times.";
            else if (upgradeName.Contains("HP Regen"))
                return "Tower regenerates 1 health per second. Can stack with multiple purchases.";
            else if (upgradeName.Contains("Gold Gen"))
                return "Increases passive gold generation by 0.5 per second.";
            else if (upgradeName.Contains("Damage Boost"))
                return "Increases all weapon damage by 10%. Stacks multiplicatively.";
            else if (upgradeName.Contains("Attack Speed"))
                return "Increases all weapon attack speed by 10%. Stacks multiplicatively.";
            else
                return "Permanent upgrade to improve your tower's capabilities.";
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
            // Update button interactability
            if (m_BuyButton != null)
            {
                m_BuyButton.interactable = m_IsAffordable && !m_IsOwned && !m_IsMaxed;

                // Update button text
                TextMeshProUGUI buttonText = m_BuyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (m_IsMaxed)
                        buttonText.text = "Maxed";
                    else if (m_IsOwned)
                        buttonText.text = "Owned";
                    else if (m_IsAffordable)
                        buttonText.text = "Buy";
                    else
                        buttonText.text = "Can't Afford";
                }
            }
            
            // Update background color
            if (m_BackgroundImage != null)
            {
                Color targetColor;
                if (m_IsOwned)
                    targetColor = m_OwnedColor;
                else if (m_IsAffordable)
                    targetColor = m_AffordableColor;
                else
                    targetColor = m_UnaffordableColor;
                
                m_BackgroundImage.color = targetColor;
            }
            
            // Update text alpha for visual feedback
            float alpha = m_IsAffordable || m_IsOwned ? 1f : 0.6f;
            
            if (m_NameText != null)
            {
                Color nameColor = m_NameText.color;
                nameColor.a = alpha;
                m_NameText.color = nameColor;
            }
            
            if (m_DescriptionText != null)
            {
                Color descColor = m_DescriptionText.color;
                descColor.a = alpha;
                m_DescriptionText.color = descColor;
            }
        }
        
        private void OnBuyButtonClicked()
        {
            if (!m_IsAffordable || m_IsOwned || m_IsMaxed) return;

            if (m_IsWeapon && m_WeaponData != null)
            {
                m_OnWeaponPurchase?.Invoke(m_WeaponData);

                // Only mark as owned if weapon doesn't allow multiple purchases
                if (!m_WeaponData.AllowMultiple)
                {
                    SetOwned(true);
                }
            }
            else if (m_IsUpgrade && m_UpgradeData != null)
            {
                m_OnUpgradeDataPurchase?.Invoke(m_UpgradeData);

                // Refresh display after purchase to update stack count and cost
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

            // Update stack count and cost
            m_CurrentStacks = m_UpgradeManager.GetUpgradeStackCount(m_UpgradeData);
            m_Cost = m_UpgradeData.GetCostForPurchase(m_CurrentStacks + 1);
            m_IsMaxed = !m_UpgradeData.UnlimitedStacks && m_CurrentStacks >= m_UpgradeData.MaxStacks;

            UpdateDisplay();
            UpdateVisuals();
        }
        
        // Public method for testing
        [ContextMenu("Test Buy")]
        private void TestBuy()
        {
            OnBuyButtonClicked();
        }
        
        private void OnDestroy()
        {
            // Cleanup button listener
            if (m_BuyButton != null)
            {
                m_BuyButton.onClick.RemoveListener(OnBuyButtonClicked);
            }
        }
    }
}