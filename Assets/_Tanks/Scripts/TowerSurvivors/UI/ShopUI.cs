using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class ShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject m_ShopPanel;
        [SerializeField] private Transform m_GridContainer;
        [SerializeField] private GameObject m_ShopItemPrefab;

        [Header("Legacy References (kept for compatibility)")]
        [SerializeField] private ScrollRect m_ShopScrollRect;
        [SerializeField] private Transform m_WeaponsContent;
        [SerializeField] private Transform m_UpgradesContent;
        [SerializeField] private Button m_WeaponsTabButton;
        [SerializeField] private Button m_UpgradesTabButton;
        [SerializeField] private GameObject m_WeaponsTab;
        [SerializeField] private GameObject m_UpgradesTab;

        [Header("Manager References")]
        [SerializeField] private GoldManager m_GoldManager;
        [SerializeField] private WeaponManager m_WeaponManager;
        [SerializeField] private UpgradeManager m_UpgradeManager;

        private List<ShopItemUI> m_AllShopItems = new List<ShopItemUI>();
        private const int MAX_GRID_SLOTS = 12;

        private void Awake()
        {
            FindManagerReferences();
            FindUIReferences();
        }

        private void Start()
        {
            SubscribeToEvents();
            InitializeShop();

            GameState currentState = TowerSurvivorsGameManager.Instance?.CurrentGameState ?? GameState.MainMenu;
            OnGameStateChanged(currentState);
        }

        private void FindManagerReferences()
        {
            if (m_GoldManager == null)
                m_GoldManager = FindObjectOfType<GoldManager>();

            if (m_WeaponManager == null)
                m_WeaponManager = FindObjectOfType<WeaponManager>();

            if (m_UpgradeManager == null)
                m_UpgradeManager = FindObjectOfType<UpgradeManager>();
        }

        private void FindUIReferences()
        {
            if (m_GridContainer == null)
            {
                Transform t = transform.Find("ShopGridContainer");
                if (t != null) m_GridContainer = t;
            }

            if (m_ShopPanel == null)
            {
                m_ShopPanel = gameObject;
            }

            // Hide legacy tab elements if they exist
            if (m_WeaponsTabButton != null)
                m_WeaponsTabButton.gameObject.SetActive(false);
            if (m_UpgradesTabButton != null)
                m_UpgradesTabButton.gameObject.SetActive(false);
            if (m_ShopScrollRect != null)
                m_ShopScrollRect.gameObject.SetActive(false);

            // Hide legacy tab containers
            Transform tabButtons = transform.Find("TabButtons");
            if (tabButtons != null)
                tabButtons.gameObject.SetActive(false);
            Transform shopTitle = transform.Find("ShopTitle");
            if (shopTitle != null)
                shopTitle.gameObject.SetActive(false);
            Transform scrollView = transform.Find("ShopScrollView");
            if (scrollView != null)
                scrollView.gameObject.SetActive(false);
        }

        private void SubscribeToEvents()
        {
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.AddListener(OnGoldChanged);
            }

            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
            }
        }

        private void InitializeShop()
        {
            PopulateGrid();
            OnGoldChanged(m_GoldManager?.CurrentGold ?? 0f);
        }

        private void PopulateGrid()
        {
            if (m_GridContainer == null || m_ShopItemPrefab == null)
            {
                Debug.LogWarning("ShopUI: Missing grid container or shop item prefab reference");
                return;
            }

            ClearShopItems();

            WeaponData[] weaponDataAssets = Resources.LoadAll<WeaponData>("WeaponData");
            UpgradeData[] upgradeDataAssets = Resources.LoadAll<UpgradeData>("UpgradeData");

            int slotIndex = 0;

            foreach (WeaponData weaponData in weaponDataAssets)
            {
                if (slotIndex >= MAX_GRID_SLOTS) break;
                CreateGridItem(weaponData);
                slotIndex++;
            }

            foreach (UpgradeData upgradeData in upgradeDataAssets)
            {
                if (slotIndex >= MAX_GRID_SLOTS) break;
                CreateGridItem(upgradeData);
                slotIndex++;
            }
        }

        private void CreateGridItem(WeaponData weaponData)
        {
            if (weaponData == null) return;

            GameObject shopItemObj = Instantiate(m_ShopItemPrefab, m_GridContainer);
            ShopItemUI shopItemUI = shopItemObj.GetComponent<ShopItemUI>();

            if (shopItemUI == null)
                shopItemUI = shopItemObj.AddComponent<ShopItemUI>();

            shopItemUI.Initialize(weaponData, OnWeaponPurchase, m_WeaponManager);
            m_AllShopItems.Add(shopItemUI);
        }

        private void CreateGridItem(UpgradeData upgradeData)
        {
            if (upgradeData == null) return;

            GameObject shopItemObj = Instantiate(m_ShopItemPrefab, m_GridContainer);
            ShopItemUI shopItemUI = shopItemObj.GetComponent<ShopItemUI>();

            if (shopItemUI == null)
                shopItemUI = shopItemObj.AddComponent<ShopItemUI>();

            shopItemUI.Initialize(upgradeData, OnUpgradeDataPurchase, m_UpgradeManager);
            m_AllShopItems.Add(shopItemUI);
        }

        private void ClearShopItems()
        {
            foreach (ShopItemUI item in m_AllShopItems)
            {
                if (item != null && item.gameObject != null)
                {
                    DestroyImmediate(item.gameObject);
                }
            }
            m_AllShopItems.Clear();
        }

        private void OnWeaponPurchase(WeaponData weaponData)
        {
            if (m_GoldManager == null || m_WeaponManager == null || weaponData == null)
            {
                Debug.LogError("ShopUI: Missing manager references for weapon purchase");
                return;
            }

            if (!m_GoldManager.CanAfford(weaponData.Cost))
            {
                Debug.Log($"Cannot afford {weaponData.WeaponName} - costs {weaponData.Cost} gold");
                return;
            }

            if (m_GoldManager.SpendGold(weaponData.Cost))
            {
                m_WeaponManager.AddWeapon(weaponData);
                Debug.Log($"Purchased {weaponData.WeaponName} for {weaponData.Cost} gold");
            }
        }

        private void OnUpgradeDataPurchase(UpgradeData upgradeData)
        {
            if (m_UpgradeManager == null || upgradeData == null)
            {
                Debug.LogError("ShopUI: Missing UpgradeManager reference for upgrade purchase");
                return;
            }

            bool success = m_UpgradeManager.PurchaseUpgrade(upgradeData);

            if (success)
            {
                Debug.Log($"Purchased upgrade: {upgradeData.UpgradeName}");
                RefreshUpgradeItems();
            }
        }

        private void RefreshUpgradeItems()
        {
            foreach (ShopItemUI item in m_AllShopItems)
            {
                if (item != null)
                {
                    item.RefreshUpgradeDisplay();
                }
            }
        }

        private void OnGoldChanged(float newGoldAmount)
        {
            UpdateItemAffordability();
        }

        private void UpdateItemAffordability()
        {
            float currentGold = m_GoldManager?.CurrentGold ?? 0f;

            foreach (ShopItemUI item in m_AllShopItems)
            {
                if (item != null)
                {
                    item.UpdateAffordability(currentGold);
                }
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            bool showShop = newState == GameState.Playing || newState == GameState.Paused;

            if (m_ShopPanel != null)
            {
                m_ShopPanel.SetActive(showShop);
            }
        }

        public void RefreshWeaponList()
        {
            PopulateGrid();
        }

        public void RefreshUpgradeList()
        {
            PopulateGrid();
        }

        public void RefreshShop()
        {
            if (m_GridContainer == null || m_ShopItemPrefab == null) return;

            ClearShopItems();

            WeaponData[] weaponDataAssets = Resources.LoadAll<WeaponData>("WeaponData");
            UpgradeData[] upgradeDataAssets = Resources.LoadAll<UpgradeData>("UpgradeData");

            // Combine and shuffle
            var allItems = new List<object>();
            allItems.AddRange(weaponDataAssets);
            allItems.AddRange(upgradeDataAssets);
            var shuffled = allItems.OrderBy(x => Random.value).ToList();

            int slotIndex = 0;
            foreach (var item in shuffled)
            {
                if (slotIndex >= MAX_GRID_SLOTS) break;
                if (item is WeaponData wd)
                    CreateGridItem(wd);
                else if (item is UpgradeData ud)
                    CreateGridItem(ud);
                slotIndex++;
            }

            OnGoldChanged(m_GoldManager?.CurrentGold ?? 0f);
            Debug.Log("Shop refreshed with randomized items");
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
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.RemoveListener(OnGoldChanged);
            }

            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            }
        }
    }
}
