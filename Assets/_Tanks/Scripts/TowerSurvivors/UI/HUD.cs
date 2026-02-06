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
        [SerializeField] private TextMeshProUGUI m_TowerHealthText;

        [Header("Boss Warning Settings")]
        [SerializeField] private float m_BossWarningDuration = 2f;
        [SerializeField] private Color m_BossWarningColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private bool m_EnableScreenShake = true;
        [SerializeField] private float m_ScreenShakeDuration = 0.5f;
        [SerializeField] private float m_ScreenShakeIntensity = 0.3f;

        [Header("Progress Bars")]
        [SerializeField] private Slider m_WaveProgressSlider;
        [SerializeField] private Slider m_TowerHealthSlider;

        [Header("Level Indicator")]
        [SerializeField] private GameObject m_LevelIndicatorPanel;
        [SerializeField] private TextMeshProUGUI m_LevelText;

        [Header("Buttons")]
        [SerializeField] private Button m_PauseButton;
        [SerializeField] private Button m_AbilityButton;

        [Header("Panels")]
        [SerializeField] private GameObject m_HealthBarPanel;
        [SerializeField] private GameObject m_CurrencyPanel;

        [Header("Manager References")]
        [SerializeField] private GoldManager m_GoldManager;
        [SerializeField] private WaveManager m_WaveManager;
        [SerializeField] private TowerSurvivorsGameManager m_GameManager;
        [SerializeField] private TowerHealth m_TowerHealth;

        private void Awake()
        {
            FindManagerReferences();
            FindUIReferences();
        }

        private void Start()
        {
            SubscribeToEvents();
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

        private void FindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            Transform root = canvas.transform;

            if (m_GoldCounterText == null)
            {
                Transform t = root.Find("CurrencyPanel/GoldText");
                if (t == null) t = root.Find("GoldText");
                if (t != null) m_GoldCounterText = t.GetComponent<TextMeshProUGUI>();
            }

            if (m_WaveNumberText == null)
            {
                Transform t = root.Find("WaveText");
                if (t != null) m_WaveNumberText = t.GetComponent<TextMeshProUGUI>();
            }

            if (m_SurvivalTimeText == null)
            {
                Transform t = root.Find("SurvivalTimeText");
                if (t != null) m_SurvivalTimeText = t.GetComponent<TextMeshProUGUI>();
            }

            if (m_NextWaveTimerText == null)
            {
                Transform t = root.Find("NextWaveTimerText");
                if (t != null) m_NextWaveTimerText = t.GetComponent<TextMeshProUGUI>();
            }

            if (m_EnemiesRemainingText == null)
            {
                Transform t = root.Find("EnemiesRemainingText");
                if (t != null) m_EnemiesRemainingText = t.GetComponent<TextMeshProUGUI>();
            }

            if (m_TowerHealthSlider == null)
            {
                Transform t = root.Find("HealthBarPanel/TowerHealthSlider");
                if (t != null) m_TowerHealthSlider = t.GetComponent<Slider>();
            }

            if (m_TowerHealthText == null)
            {
                Transform t = root.Find("HealthBarPanel/HealthText");
                if (t != null) m_TowerHealthText = t.GetComponent<TextMeshProUGUI>();
            }

            if (m_PauseButton == null)
            {
                Transform t = root.Find("PauseButton");
                if (t != null) m_PauseButton = t.GetComponent<Button>();
            }

            if (m_HealthBarPanel == null)
            {
                Transform t = root.Find("HealthBarPanel");
                if (t != null) m_HealthBarPanel = t.gameObject;
            }

            if (m_CurrencyPanel == null)
            {
                Transform t = root.Find("CurrencyPanel");
                if (t != null) m_CurrencyPanel = t.gameObject;
            }

            if (m_LevelIndicatorPanel == null)
            {
                Transform t = root.Find("LevelIndicatorPanel");
                if (t != null) m_LevelIndicatorPanel = t.gameObject;
            }

            if (m_LevelText == null)
            {
                Transform t = root.Find("LevelIndicatorPanel/LevelText");
                if (t != null) m_LevelText = t.GetComponent<TextMeshProUGUI>();
            }
        }

        private void SubscribeToEvents()
        {
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.AddListener(UpdateGoldDisplay);
            }

            if (m_WaveManager != null)
            {
                m_WaveManager.OnWaveStarted.AddListener(UpdateWaveDisplay);
                m_WaveManager.OnWaveStarted.AddListener(UpdateLevelDisplay);
                m_WaveManager.OnWaveCompleted.AddListener(OnWaveCompleted);
                m_WaveManager.OnBossSpawned.AddListener(OnBossSpawned);
            }

            if (m_GameManager != null)
            {
                m_GameManager.OnSurvivalTimeUpdate.AddListener(UpdateSurvivalTime);
                m_GameManager.OnGameStateChanged.AddListener(OnGameStateChanged);
            }

            if (m_TowerHealth != null)
            {
                m_TowerHealth.OnHealthChanged.AddListener(UpdateTowerHealthDisplay);
            }

            if (m_PauseButton != null)
            {
                m_PauseButton.onClick.AddListener(OnPauseButtonClicked);
            }

            if (m_AbilityButton != null)
            {
                m_AbilityButton.onClick.AddListener(OnAbilityButtonClicked);
            }
        }

        private void InitializeUI()
        {
            UpdateGoldDisplay(m_GoldManager?.CurrentGold ?? 0f);
            UpdateWaveDisplay(m_WaveManager?.CurrentWave ?? 1);
            UpdateSurvivalTime(0f);
            UpdateTowerHealthDisplay(m_TowerHealth?.CurrentHealth ?? 100f);
            UpdateLevelDisplay(m_WaveManager?.CurrentWave ?? 1);

            if (m_WaveProgressSlider != null)
            {
                m_WaveProgressSlider.value = 0f;
            }

            if (m_BossWarningText != null)
            {
                m_BossWarningText.gameObject.SetActive(false);
            }

            GameState currentState = m_GameManager?.CurrentGameState ?? GameState.MainMenu;
            OnGameStateChanged(currentState);
        }

        private void Update()
        {
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
                m_GoldCounterText.text = $"\u2299 {goldAmount:N0}";
                m_GoldCounterText.color = new Color(1f, 0.7f, 0.28f, 1f);
            }
        }

        private void UpdateWaveDisplay(int waveNumber)
        {
            if (m_WaveNumberText != null)
            {
                m_WaveNumberText.text = $"WAVE <b>{waveNumber}</b>";
            }
        }

        private void UpdateLevelDisplay(int waveNumber)
        {
            if (m_LevelText != null)
            {
                m_LevelText.text = $"\u2B06 Lv. {waveNumber}";
            }
        }

        private void UpdateSurvivalTime(float survivalTime)
        {
            if (m_SurvivalTimeText != null)
            {
                int minutes = Mathf.FloorToInt(survivalTime / 60f);
                int seconds = Mathf.FloorToInt(survivalTime % 60f);
                m_SurvivalTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }

        private void UpdateWaveTimer()
        {
            if (m_WaveManager == null) return;

            float timeUntilNextWave = m_WaveManager.TimeToNextWave;
            float waveInterval = 30f;

            if (m_NextWaveTimerText != null && timeUntilNextWave > 0)
            {
                m_NextWaveTimerText.text = $"Next Wave: {timeUntilNextWave:F0}s";
            }
            else if (m_NextWaveTimerText != null)
            {
                m_NextWaveTimerText.text = "Wave Active";
            }

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
                EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
                if (spawner != null)
                {
                    int enemiesRemaining = spawner.ActiveEnemyCount;
                    m_EnemiesRemainingText.text = enemiesRemaining > 0
                        ? $"Enemies: {enemiesRemaining}"
                        : "Enemies: --";
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

                var fillImage = m_TowerHealthSlider.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
                }

                var backgroundImage = m_TowerHealthSlider.GetComponent<Image>();
                if (backgroundImage != null)
                {
                    backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                }
            }

            if (m_TowerHealthText != null && m_TowerHealth != null)
            {
                int currentHp = Mathf.RoundToInt(currentHealth);
                int maxHp = Mathf.RoundToInt(m_TowerHealth.MaxHealth);
                m_TowerHealthText.text = $"{currentHp} / {maxHp}";
            }
        }

        private void OnWaveCompleted(int waveNumber)
        {
            if (m_NextWaveTimerText != null)
            {
                m_NextWaveTimerText.text = "Wave Complete!";
            }
        }

        private void OnPauseButtonClicked()
        {
            if (m_GameManager != null)
            {
                m_GameManager.PauseGame();
            }
        }

        private void OnAbilityButtonClicked()
        {
            Debug.Log("Ability button pressed - coming soon");
        }

        private void OnBossSpawned(int bossAppearance)
        {
            ShowBossWarning(bossAppearance);

            if (m_EnableScreenShake)
            {
                StartCoroutine(ScreenShakeCoroutine());
            }
        }

        private void ShowBossWarning(int bossAppearance)
        {
            if (m_BossWarningText == null) return;

            m_BossWarningText.text = "BOSS INCOMING!";
            m_BossWarningText.color = m_BossWarningColor;
            m_BossWarningText.gameObject.SetActive(true);

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
            bool showGameplayUI = newState == GameState.Playing || newState == GameState.Paused;

            if (m_GoldCounterText != null)
                m_GoldCounterText.gameObject.SetActive(showGameplayUI);

            if (m_WaveNumberText != null)
                m_WaveNumberText.gameObject.SetActive(showGameplayUI);

            // Hide elements not in target design
            if (m_SurvivalTimeText != null)
                m_SurvivalTimeText.gameObject.SetActive(false);

            if (m_NextWaveTimerText != null)
                m_NextWaveTimerText.gameObject.SetActive(false);

            if (m_EnemiesRemainingText != null)
                m_EnemiesRemainingText.gameObject.SetActive(false);

            if (m_BossWarningText != null && !showGameplayUI)
                m_BossWarningText.gameObject.SetActive(false);

            if (m_WaveProgressSlider != null)
                m_WaveProgressSlider.gameObject.SetActive(false);

            if (m_TowerHealthSlider != null)
                m_TowerHealthSlider.gameObject.SetActive(showGameplayUI);

            if (m_TowerHealthText != null)
                m_TowerHealthText.gameObject.SetActive(showGameplayUI);

            if (m_PauseButton != null)
                m_PauseButton.gameObject.SetActive(newState == GameState.Playing);

            if (m_AbilityButton != null)
                m_AbilityButton.gameObject.SetActive(newState == GameState.Playing);

            if (m_LevelIndicatorPanel != null)
                m_LevelIndicatorPanel.SetActive(newState == GameState.Playing);

            if (m_HealthBarPanel != null)
                m_HealthBarPanel.SetActive(showGameplayUI);

            if (m_CurrencyPanel != null)
                m_CurrencyPanel.SetActive(showGameplayUI);
        }

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
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.RemoveListener(UpdateGoldDisplay);
            }

            if (m_WaveManager != null)
            {
                m_WaveManager.OnWaveStarted.RemoveListener(UpdateWaveDisplay);
                m_WaveManager.OnWaveStarted.RemoveListener(UpdateLevelDisplay);
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

            if (m_PauseButton != null)
            {
                m_PauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            }

            if (m_AbilityButton != null)
            {
                m_AbilityButton.onClick.RemoveListener(OnAbilityButtonClicked);
            }
        }
    }
}
