using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TowerSurvivors
{
    [System.Serializable]
    public class WaveConfig
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
        [SerializeField] private int m_BaseEnemyCount = 5;                  // Starting enemy count
        [SerializeField] private int m_EnemyCountIncrease = 3;              // Enemy count increase per wave
        
        [Header("Wave Progression")]
        [SerializeField] private float m_GoldGenerationIncrease = 1f;       // Gold/sec increase per wave
        
        [Header("Boss Waves")]
        [SerializeField] private int m_BossWaveInterval = 10;               // Boss every X waves
        [SerializeField] private GameObject m_BossPrefab;
        
        [Header("References")]
        [SerializeField] private EnemySpawner m_EnemySpawner;
        [SerializeField] private GoldManager m_GoldManager;
        
        [Header("Events")]
        public UnityEvent<int> OnWaveStarted;           // Event when wave starts (wave number)
        public UnityEvent<int> OnWaveCompleted;         // Event when wave completes (wave number)
        public UnityEvent<float> OnWaveTimerUpdate;     // Event for wave timer updates (time remaining)
        public UnityEvent OnAllEnemiesCleared;          // Event when all enemies in wave are defeated
        
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
            
            // Calculate enemy count for this wave
            int enemyCount = m_BaseEnemyCount + ((m_CurrentWave - 1) * m_EnemyCountIncrease);
            
            // For now, spawn basic enemies (this can be expanded for enemy variety)
            m_EnemySpawner.SpawnWave(enemyCount);
            
            Debug.Log($"Spawning normal wave {m_CurrentWave} with {enemyCount} enemies");
        }
        
        private IEnumerator SpawnBossWave()
        {
            if (m_EnemySpawner == null || m_BossPrefab == null)
            {
                Debug.LogError("Cannot spawn boss wave - missing EnemySpawner or BossPrefab!");
                yield break;
            }
            
            // Boss waves spawn fewer regular enemies + boss
            int regularEnemyCount = Mathf.Max(2, m_BaseEnemyCount / 2);
            
            // Spawn regular enemies first
            m_EnemySpawner.SpawnWave(regularEnemyCount);
            
            // Wait a bit, then spawn the boss
            yield return new WaitForSeconds(2f);
            
            // Spawn boss (for now, treat as single enemy spawn)
            m_EnemySpawner.SpawnWave(1, m_BossPrefab);
            
            Debug.Log($"Spawning boss wave {m_CurrentWave} with {regularEnemyCount} enemies + 1 boss");
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
        
        public WaveConfig GetCurrentWaveConfig()
        {
            WaveConfig config = new WaveConfig
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