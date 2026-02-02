using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerSurvivors
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject m_GameOverPanel;
        [SerializeField] private TextMeshProUGUI m_SurvivalTimeText;
        [SerializeField] private TextMeshProUGUI m_WavesSurvivedText;
        [SerializeField] private TextMeshProUGUI m_EnemiesKilledText;
        [SerializeField] private TextMeshProUGUI m_GoldEarnedText;
        [SerializeField] private TextMeshProUGUI m_HighScoreText;
        
        [Header("Buttons")]
        [SerializeField] private Button m_PlayAgainButton;
        [SerializeField] private Button m_MainMenuButton;
        
        private TowerSurvivorsGameManager m_GameManager;
        
        private void Awake()
        {
            // Find game manager reference
            m_GameManager = TowerSurvivorsGameManager.Instance;
            if (m_GameManager == null)
                m_GameManager = FindObjectOfType<TowerSurvivorsGameManager>();
            
            // Setup button listeners
            if (m_PlayAgainButton != null)
                m_PlayAgainButton.onClick.AddListener(OnPlayAgainClicked);
            
            if (m_MainMenuButton != null)
                m_MainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        
        private void Start()
        {
            // Subscribe to game manager events
            if (m_GameManager != null)
            {
                m_GameManager.OnGameStateChanged.AddListener(OnGameStateChanged);
            }
            
            // Initially hide the panel
            if (m_GameOverPanel != null)
                m_GameOverPanel.SetActive(false);
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.GameOver:
                    ShowGameOverScreen();
                    break;
                case GameState.Playing:
                case GameState.MainMenu:
                    HideGameOverScreen();
                    break;
            }
        }
        
        private void ShowGameOverScreen()
        {
            if (m_GameOverPanel != null)
                m_GameOverPanel.SetActive(true);
            
            // Display final stats
            DisplayGameStats();
        }
        
        private void HideGameOverScreen()
        {
            if (m_GameOverPanel != null)
                m_GameOverPanel.SetActive(false);
        }
        
        private void DisplayGameStats()
        {
            if (m_GameManager == null) return;
            
            GameStats stats = m_GameManager.GetCurrentGameStats();
            
            // Format survival time as MM:SS
            float totalSeconds = stats.survivalTime;
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            
            // Update UI text elements
            if (m_SurvivalTimeText != null)
            {
                m_SurvivalTimeText.text = $"Survival Time: {minutes:00}:{seconds:00}";
            }
            
            if (m_WavesSurvivedText != null)
            {
                m_WavesSurvivedText.text = $"Waves Survived: {stats.wavesCompleted}";
            }
            
            if (m_EnemiesKilledText != null)
            {
                m_EnemiesKilledText.text = $"Enemies Killed: {stats.enemiesKilled}";
            }
            
            if (m_GoldEarnedText != null)
            {
                m_GoldEarnedText.text = $"Gold Earned: {stats.goldEarned:F0}";
            }
            
            // Check for high score (will be implemented with HighScoreManager)
            CheckAndDisplayHighScore(stats);
        }
        
        private void CheckAndDisplayHighScore(GameStats stats)
        {
            if (m_HighScoreText != null)
            {
                // For now, just show a placeholder
                // This will be enhanced when HighScoreManager is implemented
                m_HighScoreText.text = "High Score: --:--";
                
                // TODO: Integrate with HighScoreManager when available
                // bool isNewRecord = HighScoreManager.Instance?.IsNewRecord(stats) ?? false;
                // if (isNewRecord)
                // {
                //     m_HighScoreText.text = "NEW RECORD!";
                //     m_HighScoreText.color = Color.yellow;
                // }
                // else
                // {
                //     string bestTime = HighScoreManager.Instance?.GetFormattedBestTime() ?? "--:--";
                //     m_HighScoreText.text = $"Best: {bestTime}";
                //     m_HighScoreText.color = Color.white;
                // }
            }
        }
        
        private void OnPlayAgainClicked()
        {
            if (m_GameManager != null)
            {
                m_GameManager.RestartGame();
            }
        }
        
        private void OnMainMenuClicked()
        {
            if (m_GameManager != null)
            {
                m_GameManager.QuitToMainMenu();
            }
        }
        
        // Public method for testing
        [ContextMenu("Test Show Game Over")]
        private void TestShowGameOver()
        {
            ShowGameOverScreen();
        }
        
        private void OnDestroy()
        {
            // Cleanup button listeners
            if (m_PlayAgainButton != null)
                m_PlayAgainButton.onClick.RemoveListener(OnPlayAgainClicked);
            
            if (m_MainMenuButton != null)
                m_MainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            
            // Unsubscribe from game manager events
            if (m_GameManager != null)
            {
                m_GameManager.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            }
        }
    }
}