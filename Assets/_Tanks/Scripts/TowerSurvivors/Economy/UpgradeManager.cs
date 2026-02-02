using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace TowerSurvivors
{
    [System.Serializable]
    public class PurchasedUpgrade
    {
        public UpgradeData upgradeData;
        public int stackCount;
        
        public PurchasedUpgrade(UpgradeData data)
        {
            upgradeData = data;
            stackCount = 0;
        }
    }
    
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Manager References")]
        [SerializeField] private GoldManager m_GoldManager;
        [SerializeField] private TowerHealth m_TowerHealth;
        [SerializeField] private WeaponManager m_WeaponManager;
        
        [Header("Events")]
        public UnityEvent<UpgradeData, int> OnUpgradePurchased;     // Event when upgrade is purchased (upgrade, new stack count)
        public UnityEvent<UpgradeData> OnUpgradeUnlocked;           // Event when upgrade becomes available
        public UnityEvent OnUpgradeEffectsApplied;                  // Event when all upgrades are reapplied
        
        // Singleton pattern for easy access
        public static UpgradeManager Instance { get; private set; }
        
        // Purchased upgrades tracking
        private Dictionary<UpgradeData, PurchasedUpgrade> m_PurchasedUpgrades = new Dictionary<UpgradeData, PurchasedUpgrade>();
        
        // Cached upgrade effects for performance
        private float m_CachedDamageMultiplier = 1f;
        private float m_CachedAttackSpeedMultiplier = 1f;
        private float m_CachedRangeMultiplier = 1f;
        private float m_CachedHealthRegeneration = 0f;
        private float m_CachedGoldGeneration = 0f;
        private float m_CachedMaxHealthBonus = 0f;
        
        // Properties for other systems to access
        public float DamageMultiplier => m_CachedDamageMultiplier;
        public float AttackSpeedMultiplier => m_CachedAttackSpeedMultiplier;
        public float RangeMultiplier => m_CachedRangeMultiplier;
        public float HealthRegenerationPerSecond => m_CachedHealthRegeneration;
        public float GoldGenerationBonus => m_CachedGoldGeneration;
        public float MaxHealthBonus => m_CachedMaxHealthBonus;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                
                // Initialize events
                OnUpgradePurchased ??= new UnityEvent<UpgradeData, int>();
                OnUpgradeUnlocked ??= new UnityEvent<UpgradeData>();
                OnUpgradeEffectsApplied ??= new UnityEvent();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Find manager references if not assigned
            FindManagerReferences();
        }
        
        private void Start()
        {
            // Apply initial upgrade effects
            RecalculateUpgradeEffects();
            
            // Subscribe to wave changes to check for newly unlocked upgrades
            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameStarted.AddListener(OnGameStarted);
            }
        }
        
        private void Update()
        {
            // Apply health regeneration
            if (m_CachedHealthRegeneration > 0f && m_TowerHealth != null)
            {
                float regenAmount = m_CachedHealthRegeneration * Time.deltaTime;
                m_TowerHealth.Heal(regenAmount);
            }
        }
        
        private void FindManagerReferences()
        {
            if (m_GoldManager == null)
                m_GoldManager = FindObjectOfType<GoldManager>();
            
            if (m_TowerHealth == null)
                m_TowerHealth = FindObjectOfType<TowerHealth>();
            
            if (m_WeaponManager == null)
                m_WeaponManager = FindObjectOfType<WeaponManager>();
        }
        
        public bool PurchaseUpgrade(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                Debug.LogError("UpgradeManager: Cannot purchase null upgrade");
                return false;
            }
            
            // Get current stack count
            int currentStacks = GetUpgradeStackCount(upgradeData);
            
            // Check if upgrade can be purchased
            int currentWave = TowerSurvivorsGameManager.Instance?.CurrentWave ?? 1;
            if (!upgradeData.CanPurchase(currentWave, currentStacks, this))
            {
                Debug.Log($"Cannot purchase upgrade {upgradeData.UpgradeName}: requirements not met");
                return false;
            }
            
            // Calculate cost for next purchase
            int cost = upgradeData.GetCostForPurchase(currentStacks + 1);
            
            // Check if player can afford it
            if (m_GoldManager == null || !m_GoldManager.CanAfford(cost))
            {
                Debug.Log($"Cannot afford upgrade {upgradeData.UpgradeName}: costs {cost} gold");
                return false;
            }
            
            // Spend the gold
            if (!m_GoldManager.SpendGold(cost))
            {
                Debug.LogError("Failed to spend gold for upgrade purchase");
                return false;
            }
            
            // Add the upgrade
            if (!m_PurchasedUpgrades.ContainsKey(upgradeData))
            {
                m_PurchasedUpgrades[upgradeData] = new PurchasedUpgrade(upgradeData);
            }
            
            m_PurchasedUpgrades[upgradeData].stackCount++;
            int newStackCount = m_PurchasedUpgrades[upgradeData].stackCount;
            
            // Apply the upgrade effect
            ApplyUpgradeEffect(upgradeData, newStackCount);
            
            // Trigger events
            OnUpgradePurchased?.Invoke(upgradeData, newStackCount);
            
            Debug.Log($"Purchased upgrade: {upgradeData.UpgradeName} (Stack {newStackCount}) for {cost} gold");
            
            return true;
        }
        
        private void ApplyUpgradeEffect(UpgradeData upgradeData, int stackCount)
        {
            float effectValue = upgradeData.GetTotalEffectValue(stackCount);
            
            switch (upgradeData.Type)
            {
                case UpgradeType.MaxHealth:
                    ApplyMaxHealthUpgrade(effectValue);
                    break;
                    
                case UpgradeType.HealthRegeneration:
                    // Regeneration is applied continuously in Update()
                    break;
                    
                case UpgradeType.GoldGeneration:
                    ApplyGoldGenerationUpgrade(effectValue);
                    break;
                    
                case UpgradeType.DamageMultiplier:
                case UpgradeType.AttackSpeedMultiplier:
                case UpgradeType.Range:
                    // These are applied to weapons when they fire
                    break;
                    
                default:
                    Debug.LogWarning($"Upgrade type {upgradeData.Type} not yet implemented");
                    break;
            }
            
            // Recalculate all cached values
            RecalculateUpgradeEffects();
        }
        
        private void ApplyMaxHealthUpgrade(float healthBonus)
        {
            if (m_TowerHealth != null)
            {
                m_TowerHealth.IncreaseMaxHealth(healthBonus);
            }
        }
        
        private void ApplyGoldGenerationUpgrade(float goldBonus)
        {
            if (m_GoldManager != null)
            {
                m_GoldManager.AddPassiveGeneration(goldBonus);
            }
        }
        
        private void RecalculateUpgradeEffects()
        {
            // Reset cached values
            m_CachedDamageMultiplier = 1f;
            m_CachedAttackSpeedMultiplier = 1f;
            m_CachedRangeMultiplier = 1f;
            m_CachedHealthRegeneration = 0f;
            m_CachedGoldGeneration = 0f;
            m_CachedMaxHealthBonus = 0f;
            
            // Recalculate from all purchased upgrades
            foreach (var upgrade in m_PurchasedUpgrades.Values)
            {
                if (upgrade.upgradeData == null || upgrade.stackCount <= 0) continue;
                
                float effectValue = upgrade.upgradeData.GetTotalEffectValue(upgrade.stackCount);
                
                switch (upgrade.upgradeData.Type)
                {
                    case UpgradeType.DamageMultiplier:
                        if (upgrade.upgradeData.IsPercentage)
                            m_CachedDamageMultiplier *= (1f + effectValue);
                        else
                            m_CachedDamageMultiplier += effectValue;
                        break;
                        
                    case UpgradeType.AttackSpeedMultiplier:
                        if (upgrade.upgradeData.IsPercentage)
                            m_CachedAttackSpeedMultiplier *= (1f + effectValue);
                        else
                            m_CachedAttackSpeedMultiplier += effectValue;
                        break;
                        
                    case UpgradeType.Range:
                        if (upgrade.upgradeData.IsPercentage)
                            m_CachedRangeMultiplier *= (1f + effectValue);
                        else
                            m_CachedRangeMultiplier += effectValue;
                        break;
                        
                    case UpgradeType.HealthRegeneration:
                        m_CachedHealthRegeneration += effectValue;
                        break;
                        
                    case UpgradeType.GoldGeneration:
                        m_CachedGoldGeneration += effectValue;
                        break;
                        
                    case UpgradeType.MaxHealth:
                        m_CachedMaxHealthBonus += effectValue;
                        break;
                }
            }
            
            OnUpgradeEffectsApplied?.Invoke();
        }
        
        public bool HasUpgrade(UpgradeData upgradeData)
        {
            return m_PurchasedUpgrades.ContainsKey(upgradeData) && 
                   m_PurchasedUpgrades[upgradeData].stackCount > 0;
        }
        
        public int GetUpgradeStackCount(UpgradeData upgradeData)
        {
            if (m_PurchasedUpgrades.ContainsKey(upgradeData))
            {
                return m_PurchasedUpgrades[upgradeData].stackCount;
            }
            return 0;
        }
        
        public List<PurchasedUpgrade> GetAllPurchasedUpgrades()
        {
            return m_PurchasedUpgrades.Values.Where(u => u.stackCount > 0).ToList();
        }
        
        public int GetTotalUpgradesPurchased()
        {
            return m_PurchasedUpgrades.Values.Sum(u => u.stackCount);
        }
        
        public float GetTotalGoldSpentOnUpgrades()
        {
            float totalSpent = 0f;
            
            foreach (var upgrade in m_PurchasedUpgrades.Values)
            {
                if (upgrade.upgradeData == null || upgrade.stackCount <= 0) continue;
                
                // Calculate total cost for all stacks of this upgrade
                for (int i = 1; i <= upgrade.stackCount; i++)
                {
                    totalSpent += upgrade.upgradeData.GetCostForPurchase(i);
                }
            }
            
            return totalSpent;
        }
        
        private void OnGameStarted()
        {
            // Reset upgrades for new game
            ResetUpgrades();
        }
        
        public void ResetUpgrades()
        {
            m_PurchasedUpgrades.Clear();
            RecalculateUpgradeEffects();
            Debug.Log("All upgrades reset for new game");
        }
        
        // Public methods for UI and external systems
        public List<UpgradeData> GetAvailableUpgrades()
        {
            // Load all upgrade data assets
            UpgradeData[] allUpgrades = Resources.LoadAll<UpgradeData>("");
            
            int currentWave = TowerSurvivorsGameManager.Instance?.CurrentWave ?? 1;
            
            return allUpgrades.Where(upgrade => 
                upgrade != null && 
                upgrade.CanPurchase(currentWave, GetUpgradeStackCount(upgrade), this)
            ).ToList();
        }
        
        public int GetCostForUpgrade(UpgradeData upgradeData)
        {
            if (upgradeData == null) return 0;
            
            int currentStacks = GetUpgradeStackCount(upgradeData);
            return upgradeData.GetCostForPurchase(currentStacks + 1);
        }
        
        // Testing methods
        [ContextMenu("Reset All Upgrades")]
        private void TestResetUpgrades()
        {
            ResetUpgrades();
        }
        
        [ContextMenu("Print Upgrade Stats")]
        private void TestPrintUpgradeStats()
        {
            Debug.Log($"Damage Multiplier: {m_CachedDamageMultiplier:F2}");
            Debug.Log($"Attack Speed Multiplier: {m_CachedAttackSpeedMultiplier:F2}");
            Debug.Log($"Range Multiplier: {m_CachedRangeMultiplier:F2}");
            Debug.Log($"Health Regen/sec: {m_CachedHealthRegeneration:F2}");
            Debug.Log($"Gold Gen Bonus: {m_CachedGoldGeneration:F2}");
            Debug.Log($"Max Health Bonus: {m_CachedMaxHealthBonus:F0}");
            Debug.Log($"Total Upgrades: {GetTotalUpgradesPurchased()}");
            Debug.Log($"Total Gold Spent: {GetTotalGoldSpentOnUpgrades():F0}");
        }
        
        private void OnDestroy()
        {
            // Clear singleton reference
            if (Instance == this)
            {
                Instance = null;
            }
            
            // Unsubscribe from events
            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameStarted.RemoveListener(OnGameStarted);
            }
        }
    }
}