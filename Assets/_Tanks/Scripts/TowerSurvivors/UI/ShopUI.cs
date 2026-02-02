using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class ShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject m_ShopPanel;
        [SerializeField] private ScrollRect m_ShopScrollRect;
        [SerializeField] private Transform m_WeaponsContent;
        [SerializeField] private Transform m_UpgradesContent;
        [SerializeField] private GameObject m_ShopItemPrefab;
        
        [Header("Tab System")]
        [SerializeField] private Button m_WeaponsTabButton;
        [SerializeField] private Button m_UpgradesTabButton;
        [SerializeField] private GameObject m_WeaponsTab;
        [SerializeField] private GameObject m_UpgradesTab;
        
        [Header("Manager References")]
        [SerializeField] private GoldManager m_GoldManager;
        [SerializeField] private WeaponManager m_WeaponManager;
        
        private List<ShopItemUI> m_WeaponShopItems = new List<ShopItemUI>();
        private List<ShopItemUI> m_UpgradeShopItems = new List<ShopItemUI>();
        private ShopTab m_CurrentTab = ShopTab.Weapons;
        
        public enum ShopTab
        {
            Weapons,
            Upgrades
        }
        
        private void Awake()
        {
            // Auto-find manager references if not assigned
            FindManagerReferences();
            
            // Setup tab buttons
            if (m_WeaponsTabButton != null)
                m_WeaponsTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Weapons));
            
            if (m_UpgradesTabButton != null)
                m_UpgradesTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Upgrades));
        }
        
        private void Start()
        {
            // Subscribe to events
            SubscribeToEvents();
            
            // Initialize shop content
            InitializeShop();
            
            // Set initial tab
            SwitchTab(ShopTab.Weapons);
        }
        
        private void FindManagerReferences()
        {
            if (m_GoldManager == null)
                m_GoldManager = FindObjectOfType<GoldManager>();
            
            if (m_WeaponManager == null)
                m_WeaponManager = FindObjectOfType<WeaponManager>();
        }
        
        private void SubscribeToEvents()
        {
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.AddListener(OnGoldChanged);
            }
            
            // Subscribe to game state changes to show/hide shop
            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
            }
        }
        
        private void InitializeShop()
        {
            // Load available weapons from Resources folder
            PopulateWeapons();
            
            // Load available upgrades (placeholder for now)
            PopulateUpgrades();
            
            // Update initial gold display
            OnGoldChanged(m_GoldManager?.CurrentGold ?? 0f);
        }
        
        private void PopulateWeapons()
        {
            if (m_WeaponsContent == null || m_ShopItemPrefab == null)
            {
                Debug.LogWarning("ShopUI: Missing references for weapons content or shop item prefab");
                return;
            }
            
            // Clear existing items
            ClearShopItems(m_WeaponShopItems);
            
            // Load all WeaponData assets from Resources
            WeaponData[] weaponDataAssets = Resources.LoadAll<WeaponData>("WeaponData");
            
            foreach (WeaponData weaponData in weaponDataAssets)
            {
                CreateWeaponShopItem(weaponData);
            }
        }
        
        private void CreateWeaponShopItem(WeaponData weaponData)
        {
            if (weaponData == null) return;

            GameObject shopItemObj = Instantiate(m_ShopItemPrefab, m_WeaponsContent);
            ShopItemUI shopItemUI = shopItemObj.GetComponent<ShopItemUI>();

            if (shopItemUI == null)
            {
                shopItemUI = shopItemObj.AddComponent<ShopItemUI>();
            }

            // Setup the shop item with WeaponManager reference for owned status checking
            shopItemUI.Initialize(weaponData, OnWeaponPurchase, m_WeaponManager);
            m_WeaponShopItems.Add(shopItemUI);
        }
        
        private void PopulateUpgrades()
        {
            if (m_UpgradesContent == null || m_ShopItemPrefab == null) return;
            
            // Clear existing items
            ClearShopItems(m_UpgradeShopItems);
            
            // TODO: Load upgrade data when UpgradeData and UpgradeManager are implemented
            // For now, create placeholder upgrade items
            CreatePlaceholderUpgrades();
        }
        
        private void CreatePlaceholderUpgrades()
        {
            string[] upgradeNames = { "Max HP +25", "HP Regen +1/sec", "Gold Gen +0.5/sec", "Damage Boost +10%", "Attack Speed +10%" };
            int[] upgradeCosts = { 100, 150, 200, 250, 300 };
            
            for (int i = 0; i < upgradeNames.Length; i++)
            {
                GameObject shopItemObj = Instantiate(m_ShopItemPrefab, m_UpgradesContent);
                ShopItemUI shopItemUI = shopItemObj.GetComponent<ShopItemUI>();
                
                if (shopItemUI == null)
                {
                    shopItemUI = shopItemObj.AddComponent<ShopItemUI>();
                }
                
                // Setup placeholder upgrade
                shopItemUI.InitializePlaceholderUpgrade(upgradeNames[i], upgradeCosts[i], OnUpgradePurchase);
                m_UpgradeShopItems.Add(shopItemUI);
            }
        }
        
        private void ClearShopItems(List<ShopItemUI> itemList)
        {
            foreach (ShopItemUI item in itemList)
            {
                if (item != null && item.gameObject != null)
                {
                    DestroyImmediate(item.gameObject);
                }
            }
            itemList.Clear();
        }
        
        private void SwitchTab(ShopTab newTab)
        {
            m_CurrentTab = newTab;
            
            // Show/hide appropriate tab content
            if (m_WeaponsTab != null)
                m_WeaponsTab.SetActive(newTab == ShopTab.Weapons);
            
            if (m_UpgradesTab != null)
                m_UpgradesTab.SetActive(newTab == ShopTab.Upgrades);
            
            // Update tab button visuals (basic implementation)
            UpdateTabButtons();
        }
        
        private void UpdateTabButtons()
        {
            if (m_WeaponsTabButton != null)
            {
                var colors = m_WeaponsTabButton.colors;
                colors.normalColor = m_CurrentTab == ShopTab.Weapons ? Color.yellow : Color.white;
                m_WeaponsTabButton.colors = colors;
            }
            
            if (m_UpgradesTabButton != null)
            {
                var colors = m_UpgradesTabButton.colors;
                colors.normalColor = m_CurrentTab == ShopTab.Upgrades ? Color.yellow : Color.white;
                m_UpgradesTabButton.colors = colors;
            }
        }
        
        private void OnWeaponPurchase(WeaponData weaponData)
        {
            if (m_GoldManager == null || m_WeaponManager == null || weaponData == null)
            {
                Debug.LogError("ShopUI: Missing manager references for weapon purchase");
                return;
            }
            
            // Check if player can afford the weapon
            if (!m_GoldManager.CanAfford(weaponData.Cost))
            {
                Debug.Log($"Cannot afford {weaponData.WeaponName} - costs {weaponData.Cost} gold");
                return;
            }
            
            // Spend the gold
            if (m_GoldManager.SpendGold(weaponData.Cost))
            {
                // Add weapon to tower
                m_WeaponManager.AddWeapon(weaponData);
                Debug.Log($"Purchased {weaponData.WeaponName} for {weaponData.Cost} gold");
            }
        }
        
        private void OnUpgradePurchase(string upgradeName, int cost)
        {
            if (m_GoldManager == null)
            {
                Debug.LogError("ShopUI: Missing GoldManager reference for upgrade purchase");
                return;
            }
            
            // Check if player can afford the upgrade
            if (!m_GoldManager.CanAfford(cost))
            {
                Debug.Log($"Cannot afford {upgradeName} - costs {cost} gold");
                return;
            }
            
            // Spend the gold (actual upgrade application will be handled by UpgradeManager when implemented)
            if (m_GoldManager.SpendGold(cost))
            {
                Debug.Log($"Purchased upgrade: {upgradeName} for {cost} gold");
                // TODO: Apply upgrade effect when UpgradeManager is implemented
            }
        }
        
        private void OnGoldChanged(float newGoldAmount)
        {
            // Update affordability status for all shop items
            UpdateItemAffordability();
        }
        
        private void UpdateItemAffordability()
        {
            float currentGold = m_GoldManager?.CurrentGold ?? 0f;
            
            // Update weapon items
            foreach (ShopItemUI weaponItem in m_WeaponShopItems)
            {
                if (weaponItem != null)
                {
                    weaponItem.UpdateAffordability(currentGold);
                }
            }
            
            // Update upgrade items
            foreach (ShopItemUI upgradeItem in m_UpgradeShopItems)
            {
                if (upgradeItem != null)
                {
                    upgradeItem.UpdateAffordability(currentGold);
                }
            }
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            // Show shop during Playing and Paused states, hide during MainMenu and GameOver
            bool showShop = newState == GameState.Playing || newState == GameState.Paused;
            
            if (m_ShopPanel != null)
            {
                m_ShopPanel.SetActive(showShop);
            }
            
            // Disable shop interactions when paused (optional enhancement)
            if (newState == GameState.Paused)
            {
                // Could disable button interactivity here if desired
            }
        }
        
        // Public methods for external control
        public void RefreshWeaponList()
        {
            PopulateWeapons();
        }
        
        public void RefreshUpgradeList()
        {
            PopulateUpgrades();
        }
        
        public void SetGoldManagerReference(GoldManager goldManager)
        {
            m_GoldManager = goldManager;
            if (goldManager != null)
            {
                goldManager.OnGoldChanged.AddListener(OnGoldChanged);
            }
        }
        
        public void SetWeaponManagerReference(WeaponManager weaponManager)
        {
            m_WeaponManager = weaponManager;
        }
        
        private void OnDestroy()
        {
            // Cleanup event subscriptions
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.RemoveListener(OnGoldChanged);
            }
            
            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            }
            
            // Cleanup button listeners
            if (m_WeaponsTabButton != null)
                m_WeaponsTabButton.onClick.RemoveAllListeners();
            
            if (m_UpgradesTabButton != null)
                m_UpgradesTabButton.onClick.RemoveAllListeners();
        }
    }
}