using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject m_PauseMenuPanel;
        [SerializeField] private TextMeshProUGUI m_PauseTitleText;
        [SerializeField] private Button m_ResumeButton;
        [SerializeField] private Button m_OptionsButton;
        [SerializeField] private Button m_QuitToMenuButton;
        
        [Header("Current Game Stats")]
        [SerializeField] private GameObject m_GameStatsPanel;
        [SerializeField] private TextMeshProUGUI m_CurrentWaveText;
        [SerializeField] private TextMeshProUGUI m_SurvivalTimeText;
        [SerializeField] private TextMeshProUGUI m_EnemiesKilledText;
        [SerializeField] private TextMeshProUGUI m_GoldEarnedText;
        
        [Header("Tips/Help")]
        [SerializeField] private GameObject m_TipsPanel;
        [SerializeField] private TextMeshProUGUI m_TipsText;
        
        private TowerSurvivorsGameManager m_GameManager;
        
        private void Awake()
        {
            // Find game manager reference
            m_GameManager = TowerSurvivorsGameManager.Instance;
            if (m_GameManager == null)
                m_GameManager = FindObjectOfType<TowerSurvivorsGameManager>();
            
            // Setup button listeners
            if (m_ResumeButton != null)
                m_ResumeButton.onClick.AddListener(OnResumeClicked);
            
            if (m_OptionsButton != null)
                m_OptionsButton.onClick.AddListener(OnOptionsClicked);
            
            if (m_QuitToMenuButton != null)
                m_QuitToMenuButton.onClick.AddListener(OnQuitToMenuClicked);
        }
        
        private void Start()
        {
            // Subscribe to game state changes
            if (m_GameManager != null)
            {
                m_GameManager.OnGameStateChanged.AddListener(OnGameStateChanged);
            }
            
            // Initialize UI elements
            InitializeUI();
            
            // Initially hide pause menu
            if (m_PauseMenuPanel != null)
                m_PauseMenuPanel.SetActive(false);
        }
        
        private void InitializeUI()
        {
            // Set pause menu title
            if (m_PauseTitleText != null)
            {
                m_PauseTitleText.text = "GAME PAUSED";
            }
            
            // Setup gameplay tips
            SetupGameplayTips();
        }
        
        private void SetupGameplayTips()
        {
            if (m_TipsText != null)
            {
                string tips = "GAMEPLAY TIPS:\n\n";
                tips += "• Buy weapons from the shop to defend your tower\n";
                tips += "• Upgrade your tower to survive longer waves\n";
                tips += "• Different enemy types require different strategies\n";
                tips += "• Passive gold generation increases each wave\n";
                tips += "• Boss enemies appear every 10 waves\n";
                tips += "• ESC key pauses/resumes the game\n";
                
                m_TipsText.text = tips;
            }
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            // Show pause menu only when game is paused
            bool showPauseMenu = newState == GameState.Paused;
            
            if (m_PauseMenuPanel != null)
            {
                m_PauseMenuPanel.SetActive(showPauseMenu);
            }
            
            // Update current game stats when pause menu is shown
            if (showPauseMenu)
            {
                UpdateCurrentGameStats();
            }
        }
        
        private void UpdateCurrentGameStats()
        {
            if (m_GameManager == null) return;
            
            GameStats currentStats = m_GameManager.GetCurrentGameStats();
            
            // Update current wave
            if (m_CurrentWaveText != null)
            {
                m_CurrentWaveText.text = $"Wave: {currentStats.wavesCompleted}";
            }
            
            // Update survival time
            if (m_SurvivalTimeText != null)
            {
                float totalSeconds = currentStats.survivalTime;
                int minutes = Mathf.FloorToInt(totalSeconds / 60f);
                int seconds = Mathf.FloorToInt(totalSeconds % 60f);
                m_SurvivalTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            
            // Update enemies killed
            if (m_EnemiesKilledText != null)
            {
                m_EnemiesKilledText.text = $"Kills: {currentStats.enemiesKilled}";
            }
            
            // Update gold earned
            if (m_GoldEarnedText != null)
            {
                m_GoldEarnedText.text = $"Gold: {currentStats.goldEarned:F0}";
            }
        }
        
        private void OnResumeClicked()
        {
            if (m_GameManager != null)
            {
                m_GameManager.ResumeGame();
            }
            else
            {
                Debug.LogError("PauseMenuUI: No GameManager reference found!");
            }
        }
        
        private void OnOptionsClicked()
        {
            // TODO: Implement options menu when available
            Debug.Log("Options menu not yet implemented");
            
            // Placeholder for in-game options menu
            // Could include:
            // - Audio settings
            // - Graphics settings
            // - Control remapping
            // - Gameplay modifiers (speed, difficulty)
        }
        
        private void OnQuitToMenuClicked()
        {
            // Confirm before quitting to menu (optional enhancement)
            if (ShouldConfirmQuit())
            {
                if (m_GameManager != null)
                {
                    m_GameManager.QuitToMainMenu();
                }
                else
                {
                    Debug.LogError("PauseMenuUI: No GameManager reference found!");
                }
            }
        }
        
        private bool ShouldConfirmQuit()
        {
            // For now, always allow quit
            // Could be enhanced with a confirmation dialog
            return true;
        }
        
        // Handle ESC key input for pause/resume
        private void Update()
        {
            // Only handle ESC input if this component is active
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Let the GameManager handle ESC input directly
                // This is just a backup in case GameManager misses it
                if (m_GameManager != null)
                {
                    GameState currentState = m_GameManager.CurrentGameState;
                    if (currentState == GameState.Playing)
                    {
                        m_GameManager.PauseGame();
                    }
                    else if (currentState == GameState.Paused)
                    {
                        m_GameManager.ResumeGame();
                    }
                }
            }
        }
        
        // Public method for external control
        public void SetGameManagerReference(TowerSurvivorsGameManager gameManager)
        {
            m_GameManager = gameManager;
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged.AddListener(OnGameStateChanged);
            }
        }
        
        public void UpdateStatsDisplay()
        {
            UpdateCurrentGameStats();
        }
        
        private void OnDestroy()
        {
            // Cleanup button listeners
            if (m_ResumeButton != null)
                m_ResumeButton.onClick.RemoveListener(OnResumeClicked);
            
            if (m_OptionsButton != null)
                m_OptionsButton.onClick.RemoveListener(OnOptionsClicked);
            
            if (m_QuitToMenuButton != null)
                m_QuitToMenuButton.onClick.RemoveListener(OnQuitToMenuClicked);
            
            // Unsubscribe from game manager events
            if (m_GameManager != null)
            {
                m_GameManager.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            }
        }
        
        // Testing methods
        [ContextMenu("Test Resume")]
        private void TestResume()
        {
            OnResumeClicked();
        }
        
        [ContextMenu("Test Quit to Menu")]
        private void TestQuitToMenu()
        {
            OnQuitToMenuClicked();
        }
        
        [ContextMenu("Update Stats")]
        private void TestUpdateStats()
        {
            UpdateCurrentGameStats();
        }
    }
}