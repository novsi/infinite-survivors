using UnityEngine;

namespace TowerSurvivors
{
    public enum WeaponRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }
    
    public enum DamageType
    {
        Normal,
        Piercing,
        Magic,
        Explosive
    }
    
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Tower Survivors/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Stats")]
        [SerializeField] private string m_WeaponName = "New Weapon";
        [SerializeField] private float m_Damage = 10f;
        [SerializeField] private float m_FireRate = 1f;                    // Attacks per second
        [SerializeField] private float m_Range = 15f;
        
        [Header("Projectile")]
        [SerializeField] private GameObject m_ProjectilePrefab;
        [SerializeField] private float m_ProjectileSpeed = 20f;
        [SerializeField] private float m_ProjectileLifetime = 5f;
        
        [Header("Damage Properties")]
        [SerializeField] private DamageType m_DamageType = DamageType.Normal;
        [SerializeField] private bool m_HasAreaDamage = false;
        [SerializeField] private float m_AreaDamageRadius = 2f;
        [SerializeField] private bool m_PierceEnemies = false;
        [SerializeField] private int m_MaxPierceTargets = 3;
        
        [Header("Visual & Audio")]
        [SerializeField] private GameObject m_MuzzleFlashPrefab;
        [SerializeField] private AudioClip m_FireSound;
        [SerializeField] private Sprite m_WeaponIcon;
        
        [Header("Shop Properties")]
        [SerializeField] private WeaponRarity m_Rarity = WeaponRarity.Common;
        [SerializeField] private float m_Cost = 100f;
        [SerializeField] private string m_Description = "A basic weapon";
        [SerializeField] private bool m_AllowMultiple = true;              // Can player buy multiple?
        
        [Header("Special Effects")]
        [SerializeField] private bool m_HasChainLightning = false;
        [SerializeField] private int m_ChainTargets = 3;
        [SerializeField] private float m_ChainRange = 5f;
        [SerializeField] private float m_ChainDamageMultiplier = 0.75f;
        
        // Public accessors
        public string WeaponName => m_WeaponName;
        public float Damage => m_Damage;
        public float FireRate => m_FireRate;
        public float Range => m_Range;
        public GameObject ProjectilePrefab => m_ProjectilePrefab;
        public float ProjectileSpeed => m_ProjectileSpeed;
        public float ProjectileLifetime => m_ProjectileLifetime;
        public DamageType DamageType => m_DamageType;
        public bool HasAreaDamage => m_HasAreaDamage;
        public float AreaDamageRadius => m_AreaDamageRadius;
        public bool PierceEnemies => m_PierceEnemies;
        public int MaxPierceTargets => m_MaxPierceTargets;
        public GameObject MuzzleFlashPrefab => m_MuzzleFlashPrefab;
        public AudioClip FireSound => m_FireSound;
        public Sprite WeaponIcon => m_WeaponIcon;
        public WeaponRarity Rarity => m_Rarity;
        public float Cost => m_Cost;
        public string Description => m_Description;
        public bool AllowMultiple => m_AllowMultiple;
        public bool HasChainLightning => m_HasChainLightning;
        public int ChainTargets => m_ChainTargets;
        public float ChainRange => m_ChainRange;
        public float ChainDamageMultiplier => m_ChainDamageMultiplier;
        
        // Calculated properties
        public float FireCooldown => 1f / Mathf.Max(0.1f, m_FireRate);
        public Color RarityColor
        {
            get
            {
                return m_Rarity switch
                {
                    WeaponRarity.Common => Color.white,
                    WeaponRarity.Uncommon => Color.green,
                    WeaponRarity.Rare => Color.blue,
                    WeaponRarity.Epic => new Color(0.64f, 0.21f, 0.93f), // Purple
                    _ => Color.white
                };
            }
        }
        
        private void OnValidate()
        {
            // Ensure values stay within reasonable ranges
            m_Damage = Mathf.Max(0f, m_Damage);
            m_FireRate = Mathf.Max(0.1f, m_FireRate);
            m_Range = Mathf.Max(0.1f, m_Range);
            m_ProjectileSpeed = Mathf.Max(0.1f, m_ProjectileSpeed);
            m_ProjectileLifetime = Mathf.Max(0.1f, m_ProjectileLifetime);
            m_AreaDamageRadius = Mathf.Max(0f, m_AreaDamageRadius);
            m_MaxPierceTargets = Mathf.Max(1, m_MaxPierceTargets);
            m_Cost = Mathf.Max(0f, m_Cost);
            m_ChainTargets = Mathf.Max(1, m_ChainTargets);
            m_ChainRange = Mathf.Max(0.1f, m_ChainRange);
            m_ChainDamageMultiplier = Mathf.Clamp(m_ChainDamageMultiplier, 0f, 1f);
            
            // Auto-generate name if empty
            if (string.IsNullOrEmpty(m_WeaponName))
            {
                m_WeaponName = name;
            }
        }
    }
}