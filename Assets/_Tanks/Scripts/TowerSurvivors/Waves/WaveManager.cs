using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TowerSurvivors
{
    [System.Serializable]
    public class WaveData
    {
        public int waveNumber;
        public List<GameObject> enemyTypes = new List<GameObject>();
        public List<int> enemyCounts = new List<int>();
        public float delayBeforeWave = 2f;
        public bool isBossWave = false;
    }
    
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private float m_WaveDuration = 30f;                // Time between waves
        [SerializeField] private int m_BaseEnemyCount = 5;                  // Starting enemy count (fallback)
        [SerializeField] private int m_EnemyCountIncrease = 3;              // Enemy count increase per wave (fallback)

        [Header("Wave Config")]
        [SerializeField] private WaveConfig m_WaveConfig;                   // Configurable wave scaling

        [Header("Wave Progression")]
        [SerializeField] private float m_GoldGenerationIncrease = 1f;       // Gold/sec increase per wave
        
        [Header("Enemy Types")]
        [SerializeField] private GameObject m_BasicEnemyPrefab;             // Basic enemy (available from wave 1)
        [SerializeField] private GameObject m_FastEnemyPrefab;              // Fast enemy (available from wave 3)
        [SerializeField] private int m_FastEnemyStartWave = 3;
        [SerializeField] private GameObject m_TankEnemyPrefab;              // Tank enemy (available from wave 5)
        [SerializeField] private int m_TankEnemyStartWave = 5;
        [SerializeField] private GameObject m_RangedEnemyPrefab;            // Ranged enemy (available from wave 7)
        [SerializeField] private int m_RangedEnemyStartWave = 7;

        [Header("Boss Waves")]
        [SerializeField] private int m_BossWaveInterval = 10;               // Boss every X waves
        [SerializeField] private GameObject m_BossPrefab;
        [SerializeField] private float m_BossHPScalePerAppearance = 100f;   // Additional HP per boss appearance
        [SerializeField] private float m_BossGoldScalePerAppearance = 50f;  // Additional gold per boss appearance

        [Header("References")]
        [SerializeField] private EnemySpawner m_EnemySpawner;
        [SerializeField] private GoldManager m_GoldManager;
        
        [Header("Events")]
        public UnityEvent<int> OnWaveStarted;           // Event when wave starts (wave number)
        public UnityEvent<int> OnWaveCompleted;         // Event when wave completes (wave number)
        public UnityEvent<float> OnWaveTimerUpdate;     // Event for wave timer updates (time remaining)
        public UnityEvent OnAllEnemiesCleared;          // Event when all enemies in wave are defeated
        public UnityEvent<int> OnBossSpawned;           // Event when boss spawns (boss appearance number)
        
        private int m_CurrentWave;
        private float m_WaveTimer;
        private bool m_WaveActive;
        private bool m_WaitingForEnemyCleanup;
        private Coroutine m_WaveCoroutine;
        
        public int CurrentWave => m_CurrentWave;
        public float TimeToNextWave => m_WaveDuration - m_WaveTimer;
        public bool IsWaveActive => m_WaveActive;
        public bool IsBossWave => m_CurrentWave > 0 && m_CurrentWave % m_BossWaveInterval == 0;
        
        private void Awake()
        {
            // Initialize events
            OnWaveStarted ??= new UnityEvent<int>();
            OnWaveCompleted ??= new UnityEvent<int>();
            OnWaveTimerUpdate ??= new UnityEvent<float>();
            OnAllEnemiesCleared ??= new UnityEvent();
            OnBossSpawned ??= new UnityEvent<int>();
            
            // Find references if not assigned
            if (m_EnemySpawner == null)
            {
                m_EnemySpawner = FindObjectOfType<EnemySpawner>();
            }
            
            if (m_GoldManager == null)
            {
                m_GoldManager = FindObjectOfType<GoldManager>();
            }
        }
        
        private void Start()
        {
            // Initialize wave system
            m_CurrentWave = 0;
            m_WaveTimer = 0f;
            m_WaveActive = false;
            m_WaitingForEnemyCleanup = false;
        }
        
        private void Update()
        {
            // Only update waves during Playing state
            if (TowerSurvivorsGameManager.Instance == null ||
                TowerSurvivorsGameManager.Instance.CurrentGameState != GameState.Playing)
            {
                return;
            }

            if (!m_WaveActive && !m_WaitingForEnemyCleanup)
            {
                UpdateWaveTimer();
            }
            else if (m_WaitingForEnemyCleanup)
            {
                CheckForEnemyCleanup();
            }
        }
        
        private void UpdateWaveTimer()
        {
            m_WaveTimer += Time.deltaTime;
            OnWaveTimerUpdate?.Invoke(TimeToNextWave);
            
            if (m_WaveTimer >= m_WaveDuration)
            {
                StartNextWave();
            }
        }
        
        private void CheckForEnemyCleanup()
        {
            // Check if all enemies are cleared
            if (m_EnemySpawner != null && m_EnemySpawner.ActiveEnemyCount == 0)
            {
                OnAllEnemiesCleared?.Invoke();
                CompleteWave();
            }
        }
        
        public void StartWaveSystem()
        {
            m_CurrentWave = 0;
            m_WaveTimer = m_WaveDuration - 5f; // Start first wave quickly
            m_WaveActive = false;
            m_WaitingForEnemyCleanup = false;
        }
        
        public void StartNextWave()
        {
            if (m_WaveActive) return;
            
            m_CurrentWave++;
            m_WaveTimer = 0f;
            m_WaveActive = true;
            
            Debug.Log($"Starting Wave {m_CurrentWave}");
            OnWaveStarted?.Invoke(m_CurrentWave);
            
            // Increase gold generation
            if (m_GoldManager != null)
            {
                m_GoldManager.IncreasePassiveGoldGeneration(m_GoldGenerationIncrease);
            }
            
            // Start wave coroutine
            m_WaveCoroutine = StartCoroutine(ExecuteWave());
        }
        
        private IEnumerator ExecuteWave()
        {
            // Check if this is a boss wave
            if (IsBossWave && m_BossPrefab != null)
            {
                yield return StartCoroutine(SpawnBossWave());
            }
            else
            {
                yield return StartCoroutine(SpawnNormalWave());
            }
            
            // Wait for all enemies to be spawned
            if (m_EnemySpawner != null)
            {
                while (m_EnemySpawner.IsSpawning)
                {
                    yield return null;
                }
            }
            
            // Wave spawning complete, now wait for enemies to be cleared
            m_WaveActive = false;
            m_WaitingForEnemyCleanup = true;
        }
        
        private IEnumerator SpawnNormalWave()
        {
            if (m_EnemySpawner == null)
            {
                Debug.LogError("No EnemySpawner assigned to WaveManager!");
                yield break;
            }

            // Calculate total enemy count for this wave (use WaveConfig if available)
            int totalEnemyCount;
            if (m_WaveConfig != null)
            {
                totalEnemyCount = m_WaveConfig.GetEnemyCount(m_CurrentWave);
            }
            else
            {
                totalEnemyCount = m_BaseEnemyCount + ((m_CurrentWave - 1) * m_EnemyCountIncrease);
            }

            // Calculate scaling multipliers from WaveConfig
            float healthMultiplier = m_WaveConfig != null ? m_WaveConfig.GetHealthMultiplier(m_CurrentWave) : 1f;
            float damageMultiplier = m_WaveConfig != null ? m_WaveConfig.GetDamageMultiplier(m_CurrentWave) : 1f;
            float speedMultiplier = m_WaveConfig != null ? m_WaveConfig.GetSpeedMultiplier(m_CurrentWave) : 1f;

            // Build enemy type distribution based on wave number
            List<GameObject> enemyTypes = new List<GameObject>();
            List<int> enemyCounts = new List<int>();

            // Calculate how many of each type to spawn
            int basicCount = totalEnemyCount;
            int fastCount = 0;
            int tankCount = 0;
            int rangedCount = 0;

            // Determine start waves (use WaveConfig if available)
            int fastStartWave = m_WaveConfig != null ? m_WaveConfig.FirstFastWave : m_FastEnemyStartWave;
            int tankStartWave = m_WaveConfig != null ? m_WaveConfig.FirstTankWave : m_TankEnemyStartWave;
            int rangedStartWave = m_WaveConfig != null ? m_WaveConfig.FirstRangedWave : m_RangedEnemyStartWave;

            // Add ranged enemies starting at wave 7
            if (m_CurrentWave >= rangedStartWave && m_RangedEnemyPrefab != null)
            {
                // Ranged enemies make up 15% of the wave
                rangedCount = Mathf.Max(1, Mathf.RoundToInt(totalEnemyCount * 0.15f));
                basicCount -= rangedCount;
            }

            // Add fast enemies starting at wave 3
            if (m_CurrentWave >= fastStartWave && m_FastEnemyPrefab != null)
            {
                // Fast enemies make up 30% of the wave
                fastCount = Mathf.Max(1, Mathf.RoundToInt(totalEnemyCount * 0.3f));
                basicCount -= fastCount;
            }

            // Add tank enemies starting at wave 5
            if (m_CurrentWave >= tankStartWave && m_TankEnemyPrefab != null)
            {
                // Tank enemies make up 20% of the wave
                tankCount = Mathf.Max(1, Mathf.RoundToInt(totalEnemyCount * 0.2f));
                basicCount -= tankCount;
            }

            // Ensure at least some basic enemies
            basicCount = Mathf.Max(1, basicCount);

            // Add basic enemies
            if (m_BasicEnemyPrefab != null && basicCount > 0)
            {
                enemyTypes.Add(m_BasicEnemyPrefab);
                enemyCounts.Add(basicCount);
            }

            // Add fast enemies
            if (m_FastEnemyPrefab != null && fastCount > 0)
            {
                enemyTypes.Add(m_FastEnemyPrefab);
                enemyCounts.Add(fastCount);
            }

            // Add tank enemies
            if (m_TankEnemyPrefab != null && tankCount > 0)
            {
                enemyTypes.Add(m_TankEnemyPrefab);
                enemyCounts.Add(tankCount);
            }

            // Add ranged enemies
            if (m_RangedEnemyPrefab != null && rangedCount > 0)
            {
                enemyTypes.Add(m_RangedEnemyPrefab);
                enemyCounts.Add(rangedCount);
            }

            // Spawn the mixed wave with scaling multipliers
            if (enemyTypes.Count > 0)
            {
                m_EnemySpawner.SpawnMixedWaveWithScaling(totalEnemyCount, enemyTypes, enemyCounts, healthMultiplier, damageMultiplier, speedMultiplier);
            }
            else
            {
                // Fallback to default spawner behavior
                m_EnemySpawner.SpawnWaveWithScaling(totalEnemyCount, null, healthMultiplier, damageMultiplier, speedMultiplier);
            }

            Debug.Log($"Spawning wave {m_CurrentWave}: {basicCount} basic, {fastCount} fast, {tankCount} tank, {rangedCount} ranged enemies (HP:{healthMultiplier:F2}x DMG:{damageMultiplier:F2}x SPD:{speedMultiplier:F2}x)");
            yield break;
        }
        
        private IEnumerator SpawnBossWave()
        {
            if (m_EnemySpawner == null || m_BossPrefab == null)
            {
                Debug.LogError("Cannot spawn boss wave - missing EnemySpawner or BossPrefab!");
                yield break;
            }

            // Calculate which boss appearance this is (1st, 2nd, 3rd, etc.)
            int bossAppearance = m_CurrentWave / m_BossWaveInterval;

            // Trigger boss spawn event for UI effects (screen shake, warning text, etc.)
            OnBossSpawned?.Invoke(bossAppearance);

            // Brief delay for warning to display
            yield return new WaitForSeconds(1f);

            // Boss waves spawn fewer regular enemies + boss
            int regularEnemyCount = Mathf.Max(2, m_BaseEnemyCount / 2);

            // Spawn regular enemies first (use mixed wave if enemy types available)
            if (m_BasicEnemyPrefab != null)
            {
                m_EnemySpawner.SpawnWave(regularEnemyCount, m_BasicEnemyPrefab);
            }
            else
            {
                m_EnemySpawner.SpawnWave(regularEnemyCount);
            }

            // Wait a bit, then spawn the boss
            yield return new WaitForSeconds(2f);

            // Spawn boss with scaling
            SpawnScaledBoss(bossAppearance);

            Debug.Log($"Spawning boss wave {m_CurrentWave} (Boss #{bossAppearance}) with {regularEnemyCount} enemies + 1 scaled boss");
        }

        private void SpawnScaledBoss(int bossAppearance)
        {
            if (m_BossPrefab == null || m_EnemySpawner == null) return;

            // Calculate scaling multipliers based on boss appearance
            // HP scales by +100 per appearance (applied as a multiplier to base HP)
            // Gold scales by +50 per appearance
            float hpMultiplier = 1f + ((bossAppearance - 1) * m_BossHPScalePerAppearance / 500f); // 500 is base boss HP
            float damageMultiplier = 1f;
            float speedMultiplier = 1f;

            // Get spawn position
            Vector3 spawnPosition = GetBossSpawnPosition();

            // Instantiate boss
            GameObject bossInstance = Instantiate(m_BossPrefab, spawnPosition, Quaternion.identity);
            Enemy bossEnemy = bossInstance.GetComponent<Enemy>();

            if (bossEnemy != null)
            {
                // Initialize with scaling
                bossEnemy.Initialize(hpMultiplier, damageMultiplier, speedMultiplier);

                // Register with spawner for tracking (needed for wave completion detection)
                m_EnemySpawner.RegisterEnemy(bossEnemy);

                // Subscribe to death event for scaled gold reward
                float scaledGold = 100f + ((bossAppearance - 1) * m_BossGoldScalePerAppearance);
                bossEnemy.OnDeath.AddListener((baseGold) => OnBossDeath(scaledGold));

                Debug.Log($"Boss #{bossAppearance} spawned with {hpMultiplier:F2}x HP, will drop {scaledGold} gold");
            }
        }

        private Vector3 GetBossSpawnPosition()
        {
            // Spawn boss at a fixed position away from the tower
            TowerHealth tower = FindObjectOfType<TowerHealth>();
            if (tower != null)
            {
                // Spawn 30 units away from tower in a random direction
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                return tower.transform.position + new Vector3(Mathf.Cos(angle) * 30f, 0f, Mathf.Sin(angle) * 30f);
            }
            return Vector3.zero;
        }

        private void OnBossDeath(float scaledGold)
        {
            // Award scaled gold to player
            if (m_GoldManager != null)
            {
                m_GoldManager.AddGold(scaledGold);
                Debug.Log($"Boss defeated! Awarded {scaledGold} gold");
            }
        }
        
        private void CompleteWave()
        {
            m_WaitingForEnemyCleanup = false;
            OnWaveCompleted?.Invoke(m_CurrentWave);
            
            Debug.Log($"Wave {m_CurrentWave} completed!");
            
            // Reset wave timer for next wave
            m_WaveTimer = 0f;
        }
        
        public void ForceStartWave()
        {
            if (!m_WaveActive)
            {
                m_WaveTimer = m_WaveDuration; // This will trigger StartNextWave on next Update
            }
        }
        
        public void SetWaveDuration(float duration)
        {
            m_WaveDuration = Mathf.Max(5f, duration);
        }
        
        public void SetBaseEnemyCount(int count)
        {
            m_BaseEnemyCount = Mathf.Max(1, count);
        }
        
        public void SetEnemyCountIncrease(int increase)
        {
            m_EnemyCountIncrease = Mathf.Max(0, increase);
        }
        
        public void StopWaveSystem()
        {
            if (m_WaveCoroutine != null)
            {
                StopCoroutine(m_WaveCoroutine);
                m_WaveCoroutine = null;
            }
            
            m_WaveActive = false;
            m_WaitingForEnemyCleanup = false;
            
            // Stop enemy spawning
            if (m_EnemySpawner != null)
            {
                m_EnemySpawner.StopCurrentWave();
            }
        }
        
        public void ResetWaveSystem()
        {
            StopWaveSystem();
            m_CurrentWave = 0;
            m_WaveTimer = 0f;
            
            // Clear all enemies
            if (m_EnemySpawner != null)
            {
                m_EnemySpawner.ClearAllEnemies();
            }
        }
        
        public WaveData GetCurrentWaveData()
        {
            WaveData config = new WaveData
            {
                waveNumber = m_CurrentWave,
                isBossWave = IsBossWave,
                delayBeforeWave = 2f
            };
            
            return config;
        }
        
        // For testing purposes
        [ContextMenu("Force Next Wave")]
        private void TestForceWave()
        {
            ForceStartWave();
        }
        
        [ContextMenu("Reset Wave System")]
        private void TestResetWaves()
        {
            ResetWaveSystem();
        }
    }
}