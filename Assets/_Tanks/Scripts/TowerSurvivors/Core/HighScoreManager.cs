using UnityEngine;
using UnityEngine.Events;

namespace TowerSurvivors
{
    public class HighScoreManager : MonoBehaviour
    {
        [Header("High Score Settings")]
        [SerializeField] private bool m_SaveHighScores = true;
        [SerializeField] private bool m_ShowNewRecordNotifications = true;
        
        [Header("Events")]
        public UnityEvent<HighScoreType> OnNewRecord;           // Event when a new record is set
        public UnityEvent<HighScoreData> OnHighScoresLoaded;    // Event when high scores are loaded
        
        // Singleton pattern for easy access
        public static HighScoreManager Instance { get; private set; }
        
        // Current high score data
        private HighScoreData m_HighScores;
        
        // Property accessors
        public float BestSurvivalTime => m_HighScores.bestSurvivalTime;
        public int BestWaveReached => m_HighScores.bestWaveReached;
        public int TotalEnemiesKilled => m_HighScores.totalEnemiesKilled;
        public float TotalGoldEarned => m_HighScores.totalGoldEarned;
        public int TotalGamesPlayed => m_HighScores.totalGamesPlayed;
        
        public enum HighScoreType
        {
            SurvivalTime,
            WaveReached,
            EnemiesKilled,
            GoldEarned,
            GamesPlayed
        }
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Initialize events
                OnNewRecord ??= new UnityEvent<HighScoreType>();
                OnHighScoresLoaded ??= new UnityEvent<HighScoreData>();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            // Load high scores on start
            LoadHighScores();
            
            // Subscribe to game manager events
            SubscribeToGameEvents();
        }
        
        private void SubscribeToGameEvents()
        {
            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameOver.AddListener(OnGameOver);
            }
        }
        
        public void LoadHighScores()
        {
            if (!m_SaveHighScores)
            {
                m_HighScores = new HighScoreData(); // Default values
                OnHighScoresLoaded?.Invoke(m_HighScores);
                return;
            }
            
            // Load from PlayerPrefs
            m_HighScores = new HighScoreData
            {
                bestSurvivalTime = PlayerPrefs.GetFloat("HighScore_BestSurvivalTime", 0f),
                bestWaveReached = PlayerPrefs.GetInt("HighScore_BestWaveReached", 0),
                totalEnemiesKilled = PlayerPrefs.GetInt("HighScore_TotalEnemiesKilled", 0),
                totalGoldEarned = PlayerPrefs.GetFloat("HighScore_TotalGoldEarned", 0f),
                totalGamesPlayed = PlayerPrefs.GetInt("HighScore_TotalGamesPlayed", 0)
            };
            
            OnHighScoresLoaded?.Invoke(m_HighScores);
            
            Debug.Log($"High scores loaded - Best Time: {GetFormattedTime(m_HighScores.bestSurvivalTime)}, Best Wave: {m_HighScores.bestWaveReached}");
        }
        
        public void SaveHighScores()
        {
            if (!m_SaveHighScores) return;
            
            PlayerPrefs.SetFloat("HighScore_BestSurvivalTime", m_HighScores.bestSurvivalTime);
            PlayerPrefs.SetInt("HighScore_BestWaveReached", m_HighScores.bestWaveReached);
            PlayerPrefs.SetInt("HighScore_TotalEnemiesKilled", m_HighScores.totalEnemiesKilled);
            PlayerPrefs.SetFloat("HighScore_TotalGoldEarned", m_HighScores.totalGoldEarned);
            PlayerPrefs.SetInt("HighScore_TotalGamesPlayed", m_HighScores.totalGamesPlayed);
            
            PlayerPrefs.Save();
            
            Debug.Log("High scores saved to PlayerPrefs");
        }
        
        private void OnGameOver()
        {
            if (TowerSurvivorsGameManager.Instance == null) return;
            
            GameStats currentGameStats = TowerSurvivorsGameManager.Instance.GetCurrentGameStats();
            UpdateHighScores(currentGameStats);
        }
        
        public void UpdateHighScores(GameStats gameStats)
        {
            bool hasNewRecord = false;
            
            // Update total games played
            m_HighScores.totalGamesPlayed++;
            OnNewRecord?.Invoke(HighScoreType.GamesPlayed);
            
            // Check survival time record
            if (gameStats.survivalTime > m_HighScores.bestSurvivalTime)
            {
                m_HighScores.bestSurvivalTime = gameStats.survivalTime;
                hasNewRecord = true;
                OnNewRecord?.Invoke(HighScoreType.SurvivalTime);
                
                if (m_ShowNewRecordNotifications)
                {
                    Debug.Log($"NEW RECORD! Best Survival Time: {GetFormattedTime(gameStats.survivalTime)}");
                }
            }
            
            // Check wave record
            if (gameStats.wavesCompleted > m_HighScores.bestWaveReached)
            {
                m_HighScores.bestWaveReached = gameStats.wavesCompleted;
                hasNewRecord = true;
                OnNewRecord?.Invoke(HighScoreType.WaveReached);
                
                if (m_ShowNewRecordNotifications)
                {
                    Debug.Log($"NEW RECORD! Best Wave Reached: {gameStats.wavesCompleted}");
                }
            }
            
            // Update cumulative stats (these always increase)
            m_HighScores.totalEnemiesKilled += gameStats.enemiesKilled;
            m_HighScores.totalGoldEarned += gameStats.goldEarned;
            
            if (gameStats.enemiesKilled > 0)
                OnNewRecord?.Invoke(HighScoreType.EnemiesKilled);
            
            if (gameStats.goldEarned > 0)
                OnNewRecord?.Invoke(HighScoreType.GoldEarned);
            
            // Save updated scores
            SaveHighScores();
            
            // Log summary
            if (hasNewRecord && m_ShowNewRecordNotifications)
            {
                Debug.Log("New personal record achieved!");
            }
        }
        
        public bool IsNewSurvivalTimeRecord(float survivalTime)
        {
            return survivalTime > m_HighScores.bestSurvivalTime;
        }
        
        public bool IsNewWaveRecord(int waveNumber)
        {
            return waveNumber > m_HighScores.bestWaveReached;
        }
        
        public bool IsNewRecord(GameStats gameStats)
        {
            return IsNewSurvivalTimeRecord(gameStats.survivalTime) ||
                   IsNewWaveRecord(gameStats.wavesCompleted);
        }
        
        public string GetFormattedBestTime()
        {
            return GetFormattedTime(m_HighScores.bestSurvivalTime);
        }
        
        public string GetFormattedTime(float totalSeconds)
        {
            if (totalSeconds <= 0f)
                return "--:--";
            
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        
        public HighScoreData GetHighScoreData()
        {
            return m_HighScores;
        }
        
        public void ResetHighScores()
        {
            m_HighScores = new HighScoreData();
            SaveHighScores();
            OnHighScoresLoaded?.Invoke(m_HighScores);
            Debug.Log("High scores reset to default values");
        }
        
        // Export high scores to JSON (for external backup/sharing)
        public string ExportHighScoresToJson()
        {
            return JsonUtility.ToJson(m_HighScores, true);
        }
        
        // Import high scores from JSON (for external restore)
        public bool ImportHighScoresFromJson(string jsonData)
        {
            try
            {
                HighScoreData importedData = JsonUtility.FromJson<HighScoreData>(jsonData);
                
                // Validate imported data
                if (importedData.bestSurvivalTime >= 0f && 
                    importedData.bestWaveReached >= 0 &&
                    importedData.totalEnemiesKilled >= 0 &&
                    importedData.totalGoldEarned >= 0f &&
                    importedData.totalGamesPlayed >= 0)
                {
                    m_HighScores = importedData;
                    SaveHighScores();
                    OnHighScoresLoaded?.Invoke(m_HighScores);
                    Debug.Log("High scores imported successfully");
                    return true;
                }
                else
                {
                    Debug.LogError("Invalid high score data in JSON");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to import high scores: {e.Message}");
                return false;
            }
        }
        
        private void OnDestroy()
        {
            // Save scores before destroying
            SaveHighScores();
            
            // Clear singleton reference
            if (Instance == this)
            {
                Instance = null;
            }
            
            // Unsubscribe from events
            if (TowerSurvivorsGameManager.Instance != null)
            {
                TowerSurvivorsGameManager.Instance.OnGameOver.RemoveListener(OnGameOver);
            }
        }
        
        // Testing methods
        [ContextMenu("Reset High Scores")]
        private void TestResetHighScores()
        {
            ResetHighScores();
        }
        
        [ContextMenu("Print High Scores")]
        private void TestPrintHighScores()
        {
            Debug.Log(ExportHighScoresToJson());
        }
        
        [ContextMenu("Simulate New Record")]
        private void TestSimulateNewRecord()
        {
            GameStats testStats = new GameStats
            {
                survivalTime = m_HighScores.bestSurvivalTime + 60f, // Add 1 minute
                wavesCompleted = m_HighScores.bestWaveReached + 5,    // Add 5 waves
                enemiesKilled = 50,
                goldEarned = 1000f
            };
            
            UpdateHighScores(testStats);
        }
    }
    
    [System.Serializable]
    public struct HighScoreData
    {
        public float bestSurvivalTime;      // Best survival time in seconds
        public int bestWaveReached;         // Highest wave number reached
        public int totalEnemiesKilled;      // Total enemies killed across all games
        public float totalGoldEarned;       // Total gold earned across all games
        public int totalGamesPlayed;        // Total number of games played
    }
}