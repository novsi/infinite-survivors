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
        private string m_UpgradeName;
        private int m_Cost;
        private bool m_IsWeapon;
        private bool m_IsAffordable;
        private bool m_IsOwned;
        
        private Action<WeaponData> m_OnWeaponPurchase;
        private Action<string, int> m_OnUpgradePurchase;
        
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
        
        public void Initialize(WeaponData weaponData, Action<WeaponData> onPurchase)
        {
            if (weaponData == null)
            {
                Debug.LogError("ShopItemUI: WeaponData is null");
                return;
            }
            
            m_WeaponData = weaponData;
            m_OnWeaponPurchase = onPurchase;
            m_IsWeapon = true;
            m_Cost = weaponData.Cost;
            
            UpdateDisplay();
        }
        
        public void InitializePlaceholderUpgrade(string upgradeName, int cost, Action<string, int> onPurchase)
        {
            m_UpgradeName = upgradeName;
            m_Cost = cost;
            m_OnUpgradePurchase = onPurchase;
            m_IsWeapon = false;
            
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (m_IsWeapon && m_WeaponData != null)
            {
                UpdateWeaponDisplay();
            }
            else if (!m_IsWeapon && !string.IsNullOrEmpty(m_UpgradeName))
            {
                UpdateUpgradeDisplay();
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
            if (m_IconImage != null && m_WeaponData.Icon != null)
            {
                m_IconImage.sprite = m_WeaponData.Icon;
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
                m_BuyButton.interactable = m_IsAffordable && !m_IsOwned;
                
                // Update button text
                TextMeshProUGUI buttonText = m_BuyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (m_IsOwned)
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
            if (!m_IsAffordable || m_IsOwned) return;
            
            if (m_IsWeapon && m_WeaponData != null)
            {
                m_OnWeaponPurchase?.Invoke(m_WeaponData);
                
                // For weapons that can only be bought once, mark as owned
                // (This behavior could be customized based on game design)
                SetOwned(true);
            }
            else if (!m_IsWeapon && !string.IsNullOrEmpty(m_UpgradeName))
            {
                m_OnUpgradePurchase?.Invoke(m_UpgradeName, m_Cost);
                
                // For upgrades that can be purchased multiple times,
                // we don't mark as owned but could increase cost here
                // TODO: Implement upgrade stacking and cost scaling
            }
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