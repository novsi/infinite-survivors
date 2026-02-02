using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TowerSurvivors
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float m_SpawnRadius = 30f;
        [SerializeField] private float m_SpawnDelayBetweenEnemies = 0.5f;
        [SerializeField] private Transform m_TowerTransform;              // Center point for spawning circle
        
        [Header("Enemy Prefabs")]
        [SerializeField] private List<GameObject> m_EnemyPrefabs = new List<GameObject>();
        
        [Header("Events")]
        public UnityEvent<int> OnWaveSpawnStarted;      // Event when wave spawn starts (enemy count)
        public UnityEvent OnWaveSpawnCompleted;         // Event when all enemies in wave are spawned
        public UnityEvent<Enemy> OnEnemySpawned;        // Event when individual enemy spawns
        
        private Coroutine m_CurrentSpawnCoroutine;
        private List<Enemy> m_ActiveEnemies = new List<Enemy>();
        private int m_EnemiesSpawnedThisWave;
        
        public int ActiveEnemyCount => m_ActiveEnemies.Count;
        public List<Enemy> ActiveEnemies => new List<Enemy>(m_ActiveEnemies); // Return copy for safety
        public bool IsSpawning => m_CurrentSpawnCoroutine != null;
        
        private void Awake()
        {
            // Initialize events
            OnWaveSpawnStarted ??= new UnityEvent<int>();
            OnWaveSpawnCompleted ??= new UnityEvent();
            OnEnemySpawned ??= new UnityEvent<Enemy>();
            
            // If no tower transform assigned, try to find it
            if (m_TowerTransform == null)
            {
                TowerHealth tower = FindObjectOfType<TowerHealth>();
                if (tower != null)
                {
                    m_TowerTransform = tower.transform;
                }
            }
        }
        
        private void Start()
        {
            // Set tower as center if not assigned
            if (m_TowerTransform == null)
            {
                m_TowerTransform = transform; // Use spawner position as fallback
                Debug.LogWarning("EnemySpawner: No tower transform found, using spawner position as center");
            }
        }
        
        // Scaling multipliers for current wave
        private float m_CurrentHealthMultiplier = 1f;
        private float m_CurrentDamageMultiplier = 1f;
        private float m_CurrentSpeedMultiplier = 1f;

        public void SpawnWave(int enemyCount, GameObject enemyPrefab = null)
        {
            SpawnWaveWithScaling(enemyCount, enemyPrefab, 1f, 1f, 1f);
        }

        public void SpawnWaveWithScaling(int enemyCount, GameObject enemyPrefab, float healthMultiplier, float damageMultiplier, float speedMultiplier)
        {
            if (IsSpawning)
            {
                Debug.LogWarning("Wave spawn already in progress!");
                return;
            }

            // Store scaling multipliers
            m_CurrentHealthMultiplier = healthMultiplier;
            m_CurrentDamageMultiplier = damageMultiplier;
            m_CurrentSpeedMultiplier = speedMultiplier;

            // Use first available prefab if none specified
            if (enemyPrefab == null && m_EnemyPrefabs.Count > 0)
            {
                enemyPrefab = m_EnemyPrefabs[0];
            }

            if (enemyPrefab == null)
            {
                Debug.LogError("No enemy prefab specified for spawn wave!");
                return;
            }

            m_CurrentSpawnCoroutine = StartCoroutine(SpawnWaveCoroutine(enemyCount, enemyPrefab));
        }
        
        public void SpawnMixedWave(int enemyCount, List<GameObject> enemyTypes = null, List<int> enemyCounts = null)
        {
            SpawnMixedWaveWithScaling(enemyCount, enemyTypes, enemyCounts, 1f, 1f, 1f);
        }

        public void SpawnMixedWaveWithScaling(int enemyCount, List<GameObject> enemyTypes, List<int> enemyCounts, float healthMultiplier, float damageMultiplier, float speedMultiplier)
        {
            if (IsSpawning)
            {
                Debug.LogWarning("Wave spawn already in progress!");
                return;
            }

            // Store scaling multipliers
            m_CurrentHealthMultiplier = healthMultiplier;
            m_CurrentDamageMultiplier = damageMultiplier;
            m_CurrentSpeedMultiplier = speedMultiplier;

            // Use default enemy prefabs if none specified
            if (enemyTypes == null || enemyTypes.Count == 0)
            {
                enemyTypes = new List<GameObject>(m_EnemyPrefabs);
            }

            if (enemyTypes.Count == 0)
            {
                Debug.LogError("No enemy types available for mixed wave!");
                return;
            }

            m_CurrentSpawnCoroutine = StartCoroutine(SpawnMixedWaveCoroutine(enemyCount, enemyTypes, enemyCounts));
        }
        
        private IEnumerator SpawnWaveCoroutine(int enemyCount, GameObject enemyPrefab)
        {
            m_EnemiesSpawnedThisWave = 0;
            OnWaveSpawnStarted?.Invoke(enemyCount);
            
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnSingleEnemy(enemyPrefab);
                m_EnemiesSpawnedThisWave++;
                
                // Wait before spawning next enemy
                if (i < enemyCount - 1) // Don't wait after the last enemy
                {
                    yield return new WaitForSeconds(m_SpawnDelayBetweenEnemies);
                }
            }
            
            m_CurrentSpawnCoroutine = null;
            OnWaveSpawnCompleted?.Invoke();
        }
        
        private IEnumerator SpawnMixedWaveCoroutine(int totalEnemies, List<GameObject> enemyTypes, List<int> enemyCounts)
        {
            m_EnemiesSpawnedThisWave = 0;
            OnWaveSpawnStarted?.Invoke(totalEnemies);
            
            // If specific counts provided, use them
            if (enemyCounts != null && enemyCounts.Count == enemyTypes.Count)
            {
                for (int typeIndex = 0; typeIndex < enemyTypes.Count; typeIndex++)
                {
                    int countForThisType = enemyCounts[typeIndex];
                    for (int i = 0; i < countForThisType; i++)
                    {
                        SpawnSingleEnemy(enemyTypes[typeIndex]);
                        m_EnemiesSpawnedThisWave++;
                        
                        yield return new WaitForSeconds(m_SpawnDelayBetweenEnemies);
                    }
                }
            }
            else
            {
                // Distribute enemies randomly among types
                for (int i = 0; i < totalEnemies; i++)
                {
                    GameObject randomEnemyType = enemyTypes[Random.Range(0, enemyTypes.Count)];
                    SpawnSingleEnemy(randomEnemyType);
                    m_EnemiesSpawnedThisWave++;
                    
                    if (i < totalEnemies - 1)
                    {
                        yield return new WaitForSeconds(m_SpawnDelayBetweenEnemies);
                    }
                }
            }
            
            m_CurrentSpawnCoroutine = null;
            OnWaveSpawnCompleted?.Invoke();
        }
        
        private void SpawnSingleEnemy(GameObject enemyPrefab)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // Instantiate enemy
            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // Get Enemy component and set it up
            Enemy enemyComponent = enemyInstance.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                // Initialize enemy with scaling multipliers
                enemyComponent.Initialize(m_CurrentHealthMultiplier, m_CurrentDamageMultiplier, m_CurrentSpeedMultiplier);

                // Subscribe to enemy death event
                enemyComponent.OnDeath.AddListener(OnEnemyDied);

                // Add to active enemies list
                m_ActiveEnemies.Add(enemyComponent);

                // Face towards tower
                if (m_TowerTransform != null)
                {
                    Vector3 directionToTower = (m_TowerTransform.position - spawnPosition).normalized;
                    enemyInstance.transform.rotation = Quaternion.LookRotation(directionToTower);
                }

                // Trigger spawn event
                OnEnemySpawned?.Invoke(enemyComponent);

                Debug.Log($"Spawned {enemyPrefab.name} at {spawnPosition} (HP:{m_CurrentHealthMultiplier:F2}x DMG:{m_CurrentDamageMultiplier:F2}x SPD:{m_CurrentSpeedMultiplier:F2}x)");
            }
            else
            {
                Debug.LogError($"Enemy prefab {enemyPrefab.name} does not have Enemy component!");
            }
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            // Generate random angle
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            
            // Calculate position on circle
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * m_SpawnRadius,
                0f,
                Mathf.Sin(angle) * m_SpawnRadius
            );
            
            Vector3 centerPosition = m_TowerTransform != null ? m_TowerTransform.position : Vector3.zero;
            Vector3 spawnPosition = centerPosition + offset;
            
            // Ensure spawn position is on the ground (raycast down)
            if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                spawnPosition = hit.point;
            }
            
            return spawnPosition;
        }
        
        private void OnEnemyDied(float goldAmount)
        {
            // Note: The actual gold addition should be handled by the system that listens to this event
            // This method is called by the Enemy's OnDeath event
            
            // Find and remove the enemy from active list
            for (int i = m_ActiveEnemies.Count - 1; i >= 0; i--)
            {
                if (m_ActiveEnemies[i] == null || m_ActiveEnemies[i].IsDead)
                {
                    m_ActiveEnemies.RemoveAt(i);
                }
            }
        }
        
        public void StopCurrentWave()
        {
            if (m_CurrentSpawnCoroutine != null)
            {
                StopCoroutine(m_CurrentSpawnCoroutine);
                m_CurrentSpawnCoroutine = null;
            }
        }
        
        public void ClearAllEnemies()
        {
            StopCurrentWave();
            
            // Destroy all active enemies
            for (int i = m_ActiveEnemies.Count - 1; i >= 0; i--)
            {
                if (m_ActiveEnemies[i] != null)
                {
                    Destroy(m_ActiveEnemies[i].gameObject);
                }
            }
            
            m_ActiveEnemies.Clear();
        }
        
        public void SetSpawnRadius(float radius)
        {
            m_SpawnRadius = Mathf.Max(1f, radius);
        }
        
        public void SetSpawnDelay(float delay)
        {
            m_SpawnDelayBetweenEnemies = Mathf.Max(0f, delay);
        }

        /// <summary>
        /// Register an externally spawned enemy with the spawner for tracking.
        /// Used for boss enemies spawned with custom initialization.
        /// </summary>
        public void RegisterEnemy(Enemy enemy)
        {
            if (enemy == null || m_ActiveEnemies.Contains(enemy)) return;

            enemy.OnDeath.AddListener(OnEnemyDied);
            m_ActiveEnemies.Add(enemy);
            OnEnemySpawned?.Invoke(enemy);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw spawn radius
            Vector3 center = m_TowerTransform != null ? m_TowerTransform.position : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, m_SpawnRadius);
            
            // Draw spawn points preview (8 points around circle)
            Gizmos.color = Color.red;
            for (int i = 0; i < 8; i++)
            {
                float angle = (i * 45f) * Mathf.Deg2Rad;
                Vector3 spawnPoint = center + new Vector3(
                    Mathf.Cos(angle) * m_SpawnRadius,
                    0f,
                    Mathf.Sin(angle) * m_SpawnRadius
                );
                Gizmos.DrawWireSphere(spawnPoint, 1f);
            }
        }
        
        // For testing purposes
        [ContextMenu("Test Spawn 5 Enemies")]
        private void TestSpawn()
        {
            if (m_EnemyPrefabs.Count > 0)
            {
                SpawnWave(5, m_EnemyPrefabs[0]);
            }
        }
    }
}