using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class HUD : MonoBehaviour
    {
        [Header("UI Text Elements")]
        [SerializeField] private TextMeshProUGUI m_GoldCounterText;
        [SerializeField] private TextMeshProUGUI m_WaveNumberText;
        [SerializeField] private TextMeshProUGUI m_SurvivalTimeText;
        [SerializeField] private TextMeshProUGUI m_NextWaveTimerText;
        [SerializeField] private TextMeshProUGUI m_EnemiesRemainingText;
        [SerializeField] private TextMeshProUGUI m_BossWarningText;

        [Header("Boss Warning Settings")]
        [SerializeField] private float m_BossWarningDuration = 2f;
        [SerializeField] private Color m_BossWarningColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private bool m_EnableScreenShake = true;
        [SerializeField] private float m_ScreenShakeDuration = 0.5f;
        [SerializeField] private float m_ScreenShakeIntensity = 0.3f;
        
        [Header("Progress Bars")]
        [SerializeField] private Slider m_WaveProgressSlider;
        [SerializeField] private Slider m_TowerHealthSlider;
        
        [Header("Manager References")]
        [SerializeField] private GoldManager m_GoldManager;
        [SerializeField] private WaveManager m_WaveManager;
        [SerializeField] private TowerSurvivorsGameManager m_GameManager;
        [SerializeField] private TowerHealth m_TowerHealth;
        
        private void Awake()
        {
            // Auto-find manager references if not assigned
            FindManagerReferences();
        }
        
        private void Start()
        {
            // Subscribe to manager events
            SubscribeToEvents();
            
            // Initialize UI elements
            InitializeUI();
        }
        
        private void FindManagerReferences()
        {
            if (m_GoldManager == null)
                m_GoldManager = FindObjectOfType<GoldManager>();
            
            if (m_WaveManager == null)
                m_WaveManager = FindObjectOfType<WaveManager>();
            
            if (m_GameManager == null)
                m_GameManager = TowerSurvivorsGameManager.Instance ?? FindObjectOfType<TowerSurvivorsGameManager>();
            
            if (m_TowerHealth == null)
                m_TowerHealth = FindObjectOfType<TowerHealth>();
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to gold changes
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.AddListener(UpdateGoldDisplay);
            }
            
            // Subscribe to wave changes
            if (m_WaveManager != null)
            {
                m_WaveManager.OnWaveStarted.AddListener(UpdateWaveDisplay);
                m_WaveManager.OnWaveCompleted.AddListener(OnWaveCompleted);
                m_WaveManager.OnBossSpawned.AddListener(OnBossSpawned);
            }
            
            // Subscribe to game time updates
            if (m_GameManager != null)
            {
                m_GameManager.OnSurvivalTimeUpdate.AddListener(UpdateSurvivalTime);
                m_GameManager.OnGameStateChanged.AddListener(OnGameStateChanged);
            }
            
            // Subscribe to tower health changes
            if (m_TowerHealth != null)
            {
                m_TowerHealth.OnHealthChanged.AddListener(UpdateTowerHealthDisplay);
            }
        }
        
        private void InitializeUI()
        {
            // Initialize all UI elements with default values
            UpdateGoldDisplay(m_GoldManager?.CurrentGold ?? 0f);
            UpdateWaveDisplay(m_WaveManager?.CurrentWave ?? 1);
            UpdateSurvivalTime(0f);
            UpdateTowerHealthDisplay(m_TowerHealth?.CurrentHealth ?? 100f);
            
            // Initialize progress bars
            if (m_WaveProgressSlider != null)
            {
                m_WaveProgressSlider.value = 0f;
            }

            // Hide boss warning initially
            if (m_BossWarningText != null)
            {
                m_BossWarningText.gameObject.SetActive(false);
            }
        }
        
        private void Update()
        {
            // Update wave timer and progress during gameplay
            if (m_GameManager?.CurrentGameState == GameState.Playing)
            {
                UpdateWaveTimer();
                UpdateEnemiesRemaining();
            }
        }
        
        private void UpdateGoldDisplay(float goldAmount)
        {
            if (m_GoldCounterText != null)
            {
                m_GoldCounterText.text = $"Gold: {goldAmount:F0}";
            }
        }
        
        private void UpdateWaveDisplay(int waveNumber)
        {
            if (m_WaveNumberText != null)
            {
                m_WaveNumberText.text = $"Wave {waveNumber}";
            }
        }
        
        private void UpdateSurvivalTime(float survivalTime)
        {
            if (m_SurvivalTimeText != null)
            {
                // Format as MM:SS
                int minutes = Mathf.FloorToInt(survivalTime / 60f);
                int seconds = Mathf.FloorToInt(survivalTime % 60f);
                m_SurvivalTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
        
        private void UpdateWaveTimer()
        {
            if (m_WaveManager == null) return;
            
            float timeUntilNextWave = m_WaveManager.TimeToNextWave;
            float waveInterval = 30f; // Default wave interval - matches WaveManager's m_WaveDuration
            
            // Update next wave timer text
            if (m_NextWaveTimerText != null && timeUntilNextWave > 0)
            {
                m_NextWaveTimerText.text = $"Next Wave: {timeUntilNextWave:F0}s";
            }
            else if (m_NextWaveTimerText != null)
            {
                m_NextWaveTimerText.text = "Wave Active";
            }
            
            // Update wave progress slider
            if (m_WaveProgressSlider != null)
            {
                float progress = 1f - (timeUntilNextWave / waveInterval);
                m_WaveProgressSlider.value = Mathf.Clamp01(progress);
            }
        }
        
        private void UpdateEnemiesRemaining()
        {
            if (m_EnemiesRemainingText != null)
            {
                // Find enemy spawner to get active enemy count
                EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
                if (spawner != null)
                {
                    int enemiesRemaining = spawner.ActiveEnemyCount;
                    if (enemiesRemaining > 0)
                    {
                        m_EnemiesRemainingText.text = $"Enemies: {enemiesRemaining}";
                    }
                    else
                    {
                        m_EnemiesRemainingText.text = "Enemies: --";
                    }
                }
                else
                {
                    m_EnemiesRemainingText.text = "Enemies: --";
                }
            }
        }
        
        private void UpdateTowerHealthDisplay(float currentHealth)
        {
            if (m_TowerHealthSlider != null && m_TowerHealth != null)
            {
                m_TowerHealthSlider.value = currentHealth / m_TowerHealth.MaxHealth;
            }
        }
        
        private void OnWaveCompleted(int waveNumber)
        {
            // Wave completed - could add visual feedback here
            if (m_NextWaveTimerText != null)
            {
                m_NextWaveTimerText.text = "Wave Complete!";
            }
        }

        private void OnBossSpawned(int bossAppearance)
        {
            // Show boss warning text
            ShowBossWarning(bossAppearance);

            // Trigger screen shake
            if (m_EnableScreenShake)
            {
                StartCoroutine(ScreenShakeCoroutine());
            }
        }

        private void ShowBossWarning(int bossAppearance)
        {
            if (m_BossWarningText == null) return;

            m_BossWarningText.text = $"BOSS INCOMING!";
            m_BossWarningText.color = m_BossWarningColor;
            m_BossWarningText.gameObject.SetActive(true);

            // Hide warning after duration
            StartCoroutine(HideBossWarningAfterDelay());
        }

        private System.Collections.IEnumerator HideBossWarningAfterDelay()
        {
            yield return new WaitForSeconds(m_BossWarningDuration);

            if (m_BossWarningText != null)
            {
                m_BossWarningText.gameObject.SetActive(false);
            }
        }

        private System.Collections.IEnumerator ScreenShakeCoroutine()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) yield break;

            Vector3 originalPosition = mainCamera.transform.position;
            float elapsed = 0f;

            while (elapsed < m_ScreenShakeDuration)
            {
                float x = Random.Range(-1f, 1f) * m_ScreenShakeIntensity;
                float y = Random.Range(-1f, 1f) * m_ScreenShakeIntensity;

                mainCamera.transform.position = originalPosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            mainCamera.transform.position = originalPosition;
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            // Show/hide UI elements based on game state
            bool showGameplayUI = newState == GameState.Playing || newState == GameState.Paused;
            gameObject.SetActive(showGameplayUI);
        }
        
        // Public methods for external control
        public void SetGoldManagerReference(GoldManager goldManager)
        {
            m_GoldManager = goldManager;
            if (goldManager != null)
            {
                goldManager.OnGoldChanged.AddListener(UpdateGoldDisplay);
                UpdateGoldDisplay(goldManager.CurrentGold);
            }
        }
        
        public void SetWaveManagerReference(WaveManager waveManager)
        {
            m_WaveManager = waveManager;
            if (waveManager != null)
            {
                waveManager.OnWaveStarted.AddListener(UpdateWaveDisplay);
                waveManager.OnWaveCompleted.AddListener(OnWaveCompleted);
                UpdateWaveDisplay(waveManager.CurrentWave);
            }
        }
        
        public void SetTowerHealthReference(TowerHealth towerHealth)
        {
            m_TowerHealth = towerHealth;
            if (towerHealth != null)
            {
                towerHealth.OnHealthChanged.AddListener(UpdateTowerHealthDisplay);
                UpdateTowerHealthDisplay(towerHealth.CurrentHealth);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from all events to prevent memory leaks
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.RemoveListener(UpdateGoldDisplay);
            }
            
            if (m_WaveManager != null)
            {
                m_WaveManager.OnWaveStarted.RemoveListener(UpdateWaveDisplay);
                m_WaveManager.OnWaveCompleted.RemoveListener(OnWaveCompleted);
                m_WaveManager.OnBossSpawned.RemoveListener(OnBossSpawned);
            }
            
            if (m_GameManager != null)
            {
                m_GameManager.OnSurvivalTimeUpdate.RemoveListener(UpdateSurvivalTime);
                m_GameManager.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            }
            
            if (m_TowerHealth != null)
            {
                m_TowerHealth.OnHealthChanged.RemoveListener(UpdateTowerHealthDisplay);
            }
        }
    }
}