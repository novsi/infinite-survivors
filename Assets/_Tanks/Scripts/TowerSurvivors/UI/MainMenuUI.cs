using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject m_MainMenuPanel;
        [SerializeField] private TextMeshProUGUI m_GameTitleText;
        [SerializeField] private Button m_StartGameButton;
        [SerializeField] private Button m_OptionsButton;
        [SerializeField] private Button m_QuitButton;
        
        [Header("High Score Display")]
        [SerializeField] private GameObject m_HighScorePanel;
        [SerializeField] private TextMeshProUGUI m_BestSurvivalTimeText;
        [SerializeField] private TextMeshProUGUI m_BestWaveReachedText;
        [SerializeField] private TextMeshProUGUI m_TotalEnemiesKilledText;
        
        [Header("Credits/Version")]
        [SerializeField] private TextMeshProUGUI m_VersionText;
        [SerializeField] private TextMeshProUGUI m_CreditsText;
        
        private TowerSurvivorsGameManager m_GameManager;
        
        private void Awake()
        {
            // Find game manager reference
            m_GameManager = TowerSurvivorsGameManager.Instance;
            if (m_GameManager == null)
                m_GameManager = FindObjectOfType<TowerSurvivorsGameManager>();
            
            // Setup button listeners
            if (m_StartGameButton != null)
                m_StartGameButton.onClick.AddListener(OnStartGameClicked);
            
            if (m_OptionsButton != null)
                m_OptionsButton.onClick.AddListener(OnOptionsClicked);
            
            if (m_QuitButton != null)
                m_QuitButton.onClick.AddListener(OnQuitClicked);
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
            
            // Load and display high scores
            LoadHighScores();
        }
        
        private void InitializeUI()
        {
            // Set game title
            if (m_GameTitleText != null)
            {
                m_GameTitleText.text = "TOWER SURVIVORS";
            }
            
            // Set version info
            if (m_VersionText != null)
            {
                m_VersionText.text = $"Version: {Application.version}";
            }
            
            // Set credits
            if (m_CreditsText != null)
            {
                m_CreditsText.text = "Created with Unity";
            }
            
            // Initially show main menu if we're in MainMenu state
            bool showMainMenu = m_GameManager?.CurrentGameState == GameState.MainMenu;
            if (m_MainMenuPanel != null)
            {
                m_MainMenuPanel.SetActive(showMainMenu);
            }
        }
        
        private void LoadHighScores()
        {
            // TODO: This will be enhanced when HighScoreManager is implemented
            // For now, use placeholder values or PlayerPrefs directly
            DisplayHighScores(GetPlaceholderHighScores());
        }
        
        private HighScoreData GetPlaceholderHighScores()
        {
            // Load from PlayerPrefs if available, otherwise use defaults
            return new HighScoreData
            {
                bestSurvivalTime = PlayerPrefs.GetFloat("BestSurvivalTime", 0f),
                bestWaveReached = PlayerPrefs.GetInt("BestWaveReached", 0),
                totalEnemiesKilled = PlayerPrefs.GetInt("TotalEnemiesKilled", 0)
            };
        }
        
        private void DisplayHighScores(HighScoreData highScores)
        {
            if (m_BestSurvivalTimeText != null)
            {
                if (highScores.bestSurvivalTime > 0f)
                {
                    int minutes = Mathf.FloorToInt(highScores.bestSurvivalTime / 60f);
                    int seconds = Mathf.FloorToInt(highScores.bestSurvivalTime % 60f);
                    m_BestSurvivalTimeText.text = $"Best Time: {minutes:00}:{seconds:00}";
                }
                else
                {
                    m_BestSurvivalTimeText.text = "Best Time: --:--";
                }
            }
            
            if (m_BestWaveReachedText != null)
            {
                if (highScores.bestWaveReached > 0)
                {
                    m_BestWaveReachedText.text = $"Best Wave: {highScores.bestWaveReached}";
                }
                else
                {
                    m_BestWaveReachedText.text = "Best Wave: --";
                }
            }
            
            if (m_TotalEnemiesKilledText != null)
            {
                m_TotalEnemiesKilledText.text = $"Total Kills: {highScores.totalEnemiesKilled:N0}";
            }
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            // Show main menu only when in MainMenu state
            bool showMainMenu = newState == GameState.MainMenu;
            
            if (m_MainMenuPanel != null)
            {
                m_MainMenuPanel.SetActive(showMainMenu);
            }
            
            // Refresh high scores when returning to main menu
            if (newState == GameState.MainMenu)
            {
                LoadHighScores();
            }
        }
        
        private void OnStartGameClicked()
        {
            if (m_GameManager != null)
            {
                m_GameManager.StartGame();
            }
            else
            {
                Debug.LogError("MainMenuUI: No GameManager reference found!");
            }
        }
        
        private void OnOptionsClicked()
        {
            // TODO: Implement options menu when available
            Debug.Log("Options menu not yet implemented");
            
            // Placeholder for options menu
            // Could open a settings panel for:
            // - Audio volume settings
            // - Graphics quality
            // - Control settings
            // - Gameplay modifiers
        }
        
        private void OnQuitClicked()
        {
            // Quit the application
            Debug.Log("Quitting application...");
            
            #if UNITY_EDITOR
            // Stop playing the scene in editor
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // Quit the application in build
            Application.Quit();
            #endif
        }
        
        // Public method to refresh high scores (called externally when scores change)
        public void RefreshHighScores()
        {
            LoadHighScores();
        }
        
        // Public method for setting game manager reference
        public void SetGameManagerReference(TowerSurvivorsGameManager gameManager)
        {
            m_GameManager = gameManager;
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged.AddListener(OnGameStateChanged);
            }
        }
        
        private void OnDestroy()
        {
            // Cleanup button listeners
            if (m_StartGameButton != null)
                m_StartGameButton.onClick.RemoveListener(OnStartGameClicked);
            
            if (m_OptionsButton != null)
                m_OptionsButton.onClick.RemoveListener(OnOptionsClicked);
            
            if (m_QuitButton != null)
                m_QuitButton.onClick.RemoveListener(OnQuitClicked);
            
            // Unsubscribe from game manager events
            if (m_GameManager != null)
            {
                m_GameManager.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            }
        }
        
        // Helper struct for high score data
        [System.Serializable]
        public struct HighScoreData
        {
            public float bestSurvivalTime;
            public int bestWaveReached;
            public int totalEnemiesKilled;
        }
        
        // Testing methods
        [ContextMenu("Test Start Game")]
        private void TestStartGame()
        {
            OnStartGameClicked();
        }
        
        [ContextMenu("Test Quit")]
        private void TestQuit()
        {
            OnQuitClicked();
        }
    }
}