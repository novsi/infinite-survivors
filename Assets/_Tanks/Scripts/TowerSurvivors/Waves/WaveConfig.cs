using UnityEngine;

namespace TowerSurvivors
{
    [CreateAssetMenu(fileName = "NewWaveConfig", menuName = "Tower Survivors/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Enemy Scaling")]
        [SerializeField] private float m_HealthScalePerWave = 0.1f;         // 10% health increase per wave
        [SerializeField] private float m_DamageScalePerWave = 0.05f;        // 5% damage increase per wave
        [SerializeField] private float m_SpeedScalePerWave = 0.02f;         // 2% speed increase per wave
        [SerializeField] private float m_MaxSpeedScale = 0.5f;              // Cap speed increases at 50%
        [SerializeField] private float m_MaxHealthScale = 2.0f;             // Cap health increases at 200%
        [SerializeField] private float m_MaxDamageScale = 1.0f;             // Cap damage increases at 100%
        
        [Header("Enemy Count Scaling")]
        [SerializeField] private int m_BaseEnemyCount = 5;                  // Starting enemy count
        [SerializeField] private AnimationCurve m_EnemyCountCurve = AnimationCurve.Linear(1, 5, 30, 50); // Enemy count progression
        [SerializeField] private bool m_UseLogarithmicScaling = true;       // Use logarithmic instead of linear scaling
        [SerializeField] private float m_CountScalingFactor = 0.5f;         // Factor for logarithmic scaling
        
        [Header("Gold Scaling")]
        [SerializeField] private float m_GoldDropScalePerWave = 0.05f;      // 5% gold increase per wave
        [SerializeField] private float m_MaxGoldScale = 1.0f;               // Cap gold increases at 100%
        
        [Header("Wave Timing")]
        [SerializeField] private float m_BaseWaveInterval = 30f;            // Base time between waves
        [SerializeField] private float m_WaveIntervalReduction = 0.5f;      // Reduction per wave (in seconds)
        [SerializeField] private float m_MinWaveInterval = 15f;             // Minimum wave interval
        
        [Header("Boss Wave Settings")]
        [SerializeField] private int m_BossWaveInterval = 10;               // Boss appears every X waves
        [SerializeField] private float m_BossHealthMultiplier = 5f;         // Boss has X times normal health
        [SerializeField] private float m_BossDamageMultiplier = 2f;         // Boss does X times normal damage
        [SerializeField] private float m_BossGoldMultiplier = 10f;          // Boss drops X times normal gold
        [SerializeField] private float m_BossScalingFactor = 1.5f;          // Boss scales faster than normal enemies
        
        [Header("Special Wave Types")]
        [SerializeField] private int m_FirstFastWave = 3;                   // Wave when fast enemies start appearing
        [SerializeField] private int m_FirstTankWave = 5;                   // Wave when tank enemies start appearing
        [SerializeField] private int m_FirstRangedWave = 7;                 // Wave when ranged enemies start appearing
        [SerializeField] private int m_FirstMixedWave = 10;                 // Wave when mixed enemy types start
        
        // Public accessors
        public float HealthScalePerWave => m_HealthScalePerWave;
        public float DamageScalePerWave => m_DamageScalePerWave;
        public float SpeedScalePerWave => m_SpeedScalePerWave;
        public float MaxSpeedScale => m_MaxSpeedScale;
        public float MaxHealthScale => m_MaxHealthScale;
        public float MaxDamageScale => m_MaxDamageScale;
        public int BaseEnemyCount => m_BaseEnemyCount;
        public float GoldDropScalePerWave => m_GoldDropScalePerWave;
        public float MaxGoldScale => m_MaxGoldScale;
        public float BaseWaveInterval => m_BaseWaveInterval;
        public float WaveIntervalReduction => m_WaveIntervalReduction;
        public float MinWaveInterval => m_MinWaveInterval;
        public int BossWaveInterval => m_BossWaveInterval;
        public float BossHealthMultiplier => m_BossHealthMultiplier;
        public float BossDamageMultiplier => m_BossDamageMultiplier;
        public float BossGoldMultiplier => m_BossGoldMultiplier;
        public float BossScalingFactor => m_BossScalingFactor;
        public int FirstFastWave => m_FirstFastWave;
        public int FirstTankWave => m_FirstTankWave;
        public int FirstRangedWave => m_FirstRangedWave;
        public int FirstMixedWave => m_FirstMixedWave;
        
        // Calculate health multiplier for a given wave
        public float GetHealthMultiplier(int waveNumber, bool isBoss = false)
        {
            float multiplier = 1f + (m_HealthScalePerWave * (waveNumber - 1));
            
            if (isBoss)
            {
                multiplier *= m_BossHealthMultiplier;
                multiplier *= Mathf.Pow(m_BossScalingFactor, (waveNumber - 1) * 0.1f); // Additional boss scaling
            }
            
            return Mathf.Min(multiplier, 1f + m_MaxHealthScale);
        }
        
        // Calculate damage multiplier for a given wave
        public float GetDamageMultiplier(int waveNumber, bool isBoss = false)
        {
            float multiplier = 1f + (m_DamageScalePerWave * (waveNumber - 1));
            
            if (isBoss)
            {
                multiplier *= m_BossDamageMultiplier;
                multiplier *= Mathf.Pow(m_BossScalingFactor, (waveNumber - 1) * 0.1f); // Additional boss scaling
            }
            
            return Mathf.Min(multiplier, 1f + m_MaxDamageScale);
        }
        
        // Calculate speed multiplier for a given wave
        public float GetSpeedMultiplier(int waveNumber, bool isBoss = false)
        {
            float multiplier = 1f + (m_SpeedScalePerWave * (waveNumber - 1));
            
            if (isBoss)
            {
                // Bosses move slower typically, so don't multiply by boss factor
                multiplier *= 0.8f; // Bosses are 20% slower
            }
            
            return Mathf.Min(multiplier, 1f + m_MaxSpeedScale);
        }
        
        // Calculate gold drop multiplier for a given wave
        public float GetGoldMultiplier(int waveNumber, bool isBoss = false)
        {
            float multiplier = 1f + (m_GoldDropScalePerWave * (waveNumber - 1));
            
            if (isBoss)
            {
                multiplier *= m_BossGoldMultiplier;
            }
            
            return Mathf.Min(multiplier, 1f + m_MaxGoldScale);
        }
        
        // Calculate enemy count for a given wave
        public int GetEnemyCount(int waveNumber)
        {
            if (IsBossWave(waveNumber))
            {
                return 1; // Boss waves have only one enemy (the boss)
            }
            
            if (m_UseLogarithmicScaling)
            {
                // Logarithmic scaling: slower growth over time
                float scaledWave = Mathf.Log(waveNumber + 1) * m_CountScalingFactor;
                return m_BaseEnemyCount + Mathf.FloorToInt(scaledWave * (waveNumber - 1));
            }
            else
            {
                // Use animation curve for custom scaling
                return Mathf.FloorToInt(m_EnemyCountCurve.Evaluate(waveNumber));
            }
        }
        
        // Calculate wave interval for a given wave
        public float GetWaveInterval(int waveNumber)
        {
            float interval = m_BaseWaveInterval - (m_WaveIntervalReduction * (waveNumber - 1));
            return Mathf.Max(interval, m_MinWaveInterval);
        }
        
        // Check if a wave is a boss wave
        public bool IsBossWave(int waveNumber)
        {
            return waveNumber > 0 && waveNumber % m_BossWaveInterval == 0;
        }
        
        // Get available enemy types for a given wave
        public EnemyType[] GetAvailableEnemyTypes(int waveNumber)
        {
            System.Collections.Generic.List<EnemyType> availableTypes = new System.Collections.Generic.List<EnemyType>();
            
            // Basic enemies always available
            availableTypes.Add(EnemyType.Basic);
            
            // Add enemy types based on wave progression
            if (waveNumber >= m_FirstFastWave)
                availableTypes.Add(EnemyType.Fast);
            
            if (waveNumber >= m_FirstTankWave)
                availableTypes.Add(EnemyType.Tank);
            
            if (waveNumber >= m_FirstRangedWave)
                availableTypes.Add(EnemyType.Ranged);
            
            return availableTypes.ToArray();
        }
        
        // Get enemy type distribution for a wave (returns weights for each available type)
        public float[] GetEnemyTypeWeights(int waveNumber)
        {
            EnemyType[] availableTypes = GetAvailableEnemyTypes(waveNumber);
            
            if (waveNumber < m_FirstMixedWave)
            {
                // Before mixed waves, just use the latest unlocked type
                float[] weights = new float[availableTypes.Length];
                weights[weights.Length - 1] = 1f; // Last (newest) type gets full weight
                return weights;
            }
            else
            {
                // After mixed waves, use balanced distribution
                float[] weights = new float[availableTypes.Length];
                for (int i = 0; i < weights.Length; i++)
                {
                    // Newer enemy types get slightly higher weight
                    weights[i] = 0.5f + (i * 0.25f);
                }
                return weights;
            }
        }
        
        // Validate configuration values
        private void OnValidate()
        {
            m_HealthScalePerWave = Mathf.Max(0f, m_HealthScalePerWave);
            m_DamageScalePerWave = Mathf.Max(0f, m_DamageScalePerWave);
            m_SpeedScalePerWave = Mathf.Max(0f, m_SpeedScalePerWave);
            m_MaxSpeedScale = Mathf.Max(0f, m_MaxSpeedScale);
            m_MaxHealthScale = Mathf.Max(0f, m_MaxHealthScale);
            m_MaxDamageScale = Mathf.Max(0f, m_MaxDamageScale);
            m_BaseEnemyCount = Mathf.Max(1, m_BaseEnemyCount);
            m_GoldDropScalePerWave = Mathf.Max(0f, m_GoldDropScalePerWave);
            m_MaxGoldScale = Mathf.Max(0f, m_MaxGoldScale);
            m_BaseWaveInterval = Mathf.Max(1f, m_BaseWaveInterval);
            m_MinWaveInterval = Mathf.Max(1f, m_MinWaveInterval);
            m_BossWaveInterval = Mathf.Max(2, m_BossWaveInterval);
            m_BossHealthMultiplier = Mathf.Max(1f, m_BossHealthMultiplier);
            m_BossDamageMultiplier = Mathf.Max(1f, m_BossDamageMultiplier);
            m_BossGoldMultiplier = Mathf.Max(1f, m_BossGoldMultiplier);
            m_BossScalingFactor = Mathf.Max(1f, m_BossScalingFactor);
            
            // Ensure wave progression makes sense
            m_FirstTankWave = Mathf.Max(m_FirstFastWave, m_FirstTankWave);
            m_FirstRangedWave = Mathf.Max(m_FirstTankWave, m_FirstRangedWave);
            m_FirstMixedWave = Mathf.Max(m_FirstRangedWave, m_FirstMixedWave);
        }
    }
    
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank,
        Ranged,
        Boss
    }
}