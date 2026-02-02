using UnityEngine;

namespace TowerSurvivors
{
    public enum UpgradeType
    {
        MaxHealth,          // Increases tower max health
        HealthRegeneration, // Adds health regeneration per second
        GoldGeneration,     // Increases passive gold generation
        DamageMultiplier,   // Multiplies all weapon damage
        AttackSpeedMultiplier, // Multiplies all weapon attack speed
        Range,              // Increases all weapon range
        ProjectileSpeed,    // Increases projectile speed
        CriticalChance,     // Adds critical hit chance
        ExplosiveShells,    // Makes projectiles explode on impact
        ShieldRegeneration  // Adds shield system to tower
    }
    
    [CreateAssetMenu(fileName = "NewUpgradeData", menuName = "Tower Survivors/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private string m_UpgradeName = "New Upgrade";
        [TextArea(3, 6)]
        [SerializeField] private string m_Description = "Upgrade description goes here.";
        [SerializeField] private Sprite m_Icon;
        
        [Header("Upgrade Settings")]
        [SerializeField] private UpgradeType m_UpgradeType = UpgradeType.MaxHealth;
        [SerializeField] private float m_EffectValue = 1f;          // Base effect value
        [SerializeField] private float m_EffectValuePerStack = 1f;  // Additional value per stack
        [SerializeField] private bool m_IsPercentage = false;       // Whether effect is percentage-based
        
        [Header("Purchase Settings")]
        [SerializeField] private int m_BaseCost = 100;
        [SerializeField] private float m_CostMultiplierPerPurchase = 1.5f;  // Cost increase per purchase
        [SerializeField] private int m_MaxStacks = 10;              // Maximum times this can be purchased
        [SerializeField] private bool m_UnlimitedStacks = false;    // Allow unlimited purchases
        
        [Header("Requirements")]
        [SerializeField] private int m_RequiredWave = 1;            // Minimum wave to unlock
        [SerializeField] private UpgradeData[] m_PrerequisiteUpgrades; // Other upgrades needed first
        
        [Header("Visual")]
        [SerializeField] private Color m_UpgradeColor = Color.white;
        [SerializeField] private string m_Rarity = "Common";        // Common, Uncommon, Rare, Epic, Legendary
        
        // Public accessors
        public string UpgradeName => m_UpgradeName;
        public string Description => m_Description;
        public Sprite Icon => m_Icon;
        public UpgradeType Type => m_UpgradeType;
        public float EffectValue => m_EffectValue;
        public float EffectValuePerStack => m_EffectValuePerStack;
        public bool IsPercentage => m_IsPercentage;
        public int BaseCost => m_BaseCost;
        public float CostMultiplierPerPurchase => m_CostMultiplierPerPurchase;
        public int MaxStacks => m_MaxStacks;
        public bool UnlimitedStacks => m_UnlimitedStacks;
        public int RequiredWave => m_RequiredWave;
        public UpgradeData[] PrerequisiteUpgrades => m_PrerequisiteUpgrades;
        public Color UpgradeColor => m_UpgradeColor;
        public string Rarity => m_Rarity;
        
        // Calculate cost for a specific purchase number (1-based)
        public int GetCostForPurchase(int purchaseNumber)
        {
            if (purchaseNumber <= 0) return m_BaseCost;
            
            // Cost increases exponentially: baseCost * (multiplier ^ (purchaseNumber - 1))
            return Mathf.RoundToInt(m_BaseCost * Mathf.Pow(m_CostMultiplierPerPurchase, purchaseNumber - 1));
        }
        
        // Get the total effect value for a given stack count
        public float GetTotalEffectValue(int stackCount)
        {
            if (stackCount <= 0) return 0f;
            
            // First purchase gives base effect, subsequent purchases add effect per stack
            return m_EffectValue + (m_EffectValuePerStack * (stackCount - 1));
        }
        
        // Generate formatted description with current stack info
        public string GetFormattedDescription(int currentStacks = 0)
        {
            string formatted = m_Description;
            
            // Replace placeholders in description
            if (currentStacks > 0)
            {
                float totalEffect = GetTotalEffectValue(currentStacks);
                formatted = formatted.Replace("{EFFECT}", FormatEffectValue(totalEffect));
                formatted = formatted.Replace("{STACKS}", currentStacks.ToString());
            }
            else
            {
                formatted = formatted.Replace("{EFFECT}", FormatEffectValue(m_EffectValue));
                formatted = formatted.Replace("{STACKS}", "0");
            }
            
            return formatted;
        }
        
        private string FormatEffectValue(float value)
        {
            if (m_IsPercentage)
            {
                return $"{value * 100f:F0}%";
            }
            else
            {
                return value.ToString("F1");
            }
        }
        
        // Check if upgrade can be purchased based on requirements
        public bool CanPurchase(int currentWave, int currentStacks, UpgradeManager upgradeManager)
        {
            // Check wave requirement
            if (currentWave < m_RequiredWave)
                return false;
            
            // Check stack limit
            if (!m_UnlimitedStacks && currentStacks >= m_MaxStacks)
                return false;
            
            // Check prerequisites
            if (m_PrerequisiteUpgrades != null && upgradeManager != null)
            {
                foreach (UpgradeData prerequisite in m_PrerequisiteUpgrades)
                {
                    if (prerequisite != null && !upgradeManager.HasUpgrade(prerequisite))
                        return false;
                }
            }
            
            return true;
        }
        
        // Validation in editor
        private void OnValidate()
        {
            // Ensure values stay within reasonable ranges
            m_EffectValue = Mathf.Max(0f, m_EffectValue);
            m_EffectValuePerStack = Mathf.Max(0f, m_EffectValuePerStack);
            m_BaseCost = Mathf.Max(1, m_BaseCost);
            m_CostMultiplierPerPurchase = Mathf.Max(1f, m_CostMultiplierPerPurchase);
            m_MaxStacks = Mathf.Max(1, m_MaxStacks);
            m_RequiredWave = Mathf.Max(1, m_RequiredWave);
            
            // Ensure name is not empty
            if (string.IsNullOrEmpty(m_UpgradeName))
                m_UpgradeName = "Unnamed Upgrade";
        }
    }
}