using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TowerSurvivors
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
    
    public class TowerSurvivorsGameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private GameState m_InitialState = GameState.MainMenu;
        
        [Header("Manager References")]
        [SerializeField] private GoldManager m_GoldManager;
        [SerializeField] private WaveManager m_WaveManager;
        [SerializeField] private EnemySpawner m_EnemySpawner;
        [SerializeField] private TowerHealth m_TowerHealth;
        
        [Header("Events")]
        public UnityEvent<GameState> OnGameStateChanged;
        public UnityEvent<float> OnSurvivalTimeUpdate;
        public UnityEvent OnGameStarted;
        public UnityEvent OnGameOver;
        public UnityEvent OnGamePaused;
        public UnityEvent OnGameResumed;
        
        private GameState m_CurrentGameState;
        private float m_SurvivalTime;
        private int m_EnemiesKilled;
        private float m_GoldEarned;
        private bool m_GameInitialized;
        
        // Singleton pattern for easy access
        public static TowerSurvivorsGameManager Instance { get; private set; }
        
        public GameState CurrentGameState => m_CurrentGameState;
        public float SurvivalTime => m_SurvivalTime;
        public int EnemiesKilled => m_EnemiesKilled;
        public float GoldEarned => m_GoldEarned;
        public int CurrentWave => m_WaveManager != null ? m_WaveManager.CurrentWave : 0;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Initialize events
            OnGameStateChanged ??= new UnityEvent<GameState>();
            OnSurvivalTimeUpdate ??= new UnityEvent<float>();
            OnGameStarted ??= new UnityEvent();
            OnGameOver ??= new UnityEvent();
            OnGamePaused ??= new UnityEvent();
            OnGameResumed ??= new UnityEvent();
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void Update()
        {
            HandleInput();
            
            if (m_CurrentGameState == GameState.Playing)
            {
                UpdateSurvivalTime();
            }
        }
        
        private void InitializeGame()
        {
            if (m_GameInitialized) return;
            
            // Find manager references if not assigned
            FindManagerReferences();
            
            // Subscribe to events
            SubscribeToEvents();
            
            // Set initial game state
            ChangeGameState(m_InitialState);
            
            m_GameInitialized = true;
        }
        
        private void FindManagerReferences()
        {
            if (m_GoldManager == null)
                m_GoldManager = FindObjectOfType<GoldManager>();
            
            if (m_WaveManager == null)
                m_WaveManager = FindObjectOfType<WaveManager>();
            
            if (m_EnemySpawner == null)
                m_EnemySpawner = FindObjectOfType<EnemySpawner>();
            
            if (m_TowerHealth == null)
                m_TowerHealth = FindObjectOfType<TowerHealth>();
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to tower death
            if (m_TowerHealth != null)
            {
                m_TowerHealth.OnDeath.AddListener(OnTowerDestroyed);
            }
            
            // Subscribe to enemy spawner events
            if (m_EnemySpawner != null)
            {
                m_EnemySpawner.OnEnemySpawned.AddListener(OnEnemySpawned);
            }
            
            // Subscribe to gold manager events
            if (m_GoldManager != null)
            {
                m_GoldManager.OnGoldChanged.AddListener(OnGoldChanged);
            }
        }
        
        private void HandleInput()
        {
            // ESC key for pause/resume
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (m_CurrentGameState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (m_CurrentGameState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }
        
        private void UpdateSurvivalTime()
        {
            m_SurvivalTime += Time.deltaTime;
            OnSurvivalTimeUpdate?.Invoke(m_SurvivalTime);
        }
        
        public void StartGame()
        {
            if (m_CurrentGameState == GameState.Playing) return;
            
            // Reset game stats
            ResetGameStats();
            
            // Initialize all managers
            InitializeGameSystems();
            
            // Change to playing state
            ChangeGameState(GameState.Playing);
            
            OnGameStarted?.Invoke();
            Debug.Log("Game Started!");
        }
        
        private void ResetGameStats()
        {
            m_SurvivalTime = 0f;
            m_EnemiesKilled = 0;
            m_GoldEarned = 0f;
        }
        
        private void InitializeGameSystems()
        {
            // Reset gold manager
            if (m_GoldManager != null)
            {
                m_GoldManager.ResetGold();
            }
            
            // Reset wave manager
            if (m_WaveManager != null)
            {
                m_WaveManager.ResetWaveSystem();
                m_WaveManager.StartWaveSystem();
            }
            
            // Clear any existing enemies
            if (m_EnemySpawner != null)
            {
                m_EnemySpawner.ClearAllEnemies();
            }
        }
        
        public void PauseGame()
        {
            if (m_CurrentGameState != GameState.Playing) return;
            
            Time.timeScale = 0f;
            ChangeGameState(GameState.Paused);
            OnGamePaused?.Invoke();
        }
        
        public void ResumeGame()
        {
            if (m_CurrentGameState != GameState.Paused) return;
            
            Time.timeScale = 1f;
            ChangeGameState(GameState.Playing);
            OnGameResumed?.Invoke();
        }
        
        public void TogglePause()
        {
            if (m_CurrentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (m_CurrentGameState == GameState.Paused)
            {
                ResumeGame();
            }
        }
        
        private void OnTowerDestroyed()
        {
            // Tower has been destroyed, trigger game over
            EndGame();
        }
        
        private void EndGame()
        {
            if (m_CurrentGameState == GameState.GameOver) return;
            
            // Stop all game systems
            if (m_WaveManager != null)
            {
                m_WaveManager.StopWaveSystem();
            }
            
            if (m_EnemySpawner != null)
            {
                m_EnemySpawner.StopCurrentWave();
            }
            
            // Change to game over state
            ChangeGameState(GameState.GameOver);
            OnGameOver?.Invoke();
            
            Debug.Log($"Game Over! Survived {m_SurvivalTime:F1}s, Waves: {CurrentWave}, Enemies Killed: {m_EnemiesKilled}");
        }
        
        public void RestartGame()
        {
            Time.timeScale = 1f; // Ensure time scale is reset
            StartGame();
        }
        
        public void QuitToMainMenu()
        {
            Time.timeScale = 1f; // Ensure time scale is reset
            
            // Stop all game systems
            if (m_WaveManager != null)
            {
                m_WaveManager.StopWaveSystem();
            }
            
            if (m_EnemySpawner != null)
            {
                m_EnemySpawner.ClearAllEnemies();
            }
            
            ChangeGameState(GameState.MainMenu);
        }
        
        private void ChangeGameState(GameState newState)
        {
            if (m_CurrentGameState == newState) return;
            
            GameState previousState = m_CurrentGameState;
            m_CurrentGameState = newState;
            
            OnGameStateChanged?.Invoke(newState);
            
            Debug.Log($"Game State changed from {previousState} to {newState}");
        }
        
        private void OnEnemySpawned(Enemy enemy)
        {
            // Subscribe to enemy death event to track kills
            if (enemy != null)
            {
                enemy.OnDeath.AddListener(OnEnemyKilled);
            }
        }
        
        private void OnEnemyKilled(float goldAmount)
        {
            m_EnemiesKilled++;
            
            // Add gold to manager
            if (m_GoldManager != null)
            {
                m_GoldManager.AddGold(goldAmount);
            }
        }
        
        private void OnGoldChanged(float newGoldAmount)
        {
            // Track total gold earned
            // Note: This is a simple approximation - for precise tracking,
            // we'd need to track gold sources separately
            if (newGoldAmount > m_GoldEarned)
            {
                m_GoldEarned = newGoldAmount;
            }
        }
        
        public GameStats GetCurrentGameStats()
        {
            return new GameStats
            {
                survivalTime = m_SurvivalTime,
                wavesCompleted = CurrentWave,
                enemiesKilled = m_EnemiesKilled,
                goldEarned = m_GoldEarned
            };
        }
        
        private void OnDestroy()
        {
            // Ensure time scale is reset when destroyed
            Time.timeScale = 1f;
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        // For testing purposes
        [ContextMenu("Start Game")]
        private void TestStartGame()
        {
            StartGame();
        }
        
        [ContextMenu("End Game")]
        private void TestEndGame()
        {
            EndGame();
        }
        
        [ContextMenu("Toggle Pause")]
        private void TestTogglePause()
        {
            TogglePause();
        }
    }
    
    [System.Serializable]
    public struct GameStats
    {
        public float survivalTime;
        public int wavesCompleted;
        public int enemiesKilled;
        public float goldEarned;
    }
}