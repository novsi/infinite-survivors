using UnityEngine;
using UnityEngine.Events;

namespace TowerSurvivors
{
    public class GoldManager : MonoBehaviour
    {
        [Header("Gold Settings")]
        [SerializeField] private float m_StartingGold = 50f;
        [SerializeField] private float m_PassiveGoldPerSecond = 1f;
        
        [Header("Events")]
        public UnityEvent<float> OnGoldChanged;     // Event when gold amount changes
        
        private float m_CurrentGold;
        private float m_GoldTimer;
        
        // Singleton pattern for easy access
        public static GoldManager Instance { get; private set; }
        
        public float CurrentGold => m_CurrentGold;
        public float PassiveGoldPerSecond => m_PassiveGoldPerSecond;
        
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
            OnGoldChanged ??= new UnityEvent<float>();
        }
        
        private void Start()
        {
            // Initialize gold
            m_CurrentGold = m_StartingGold;
            m_GoldTimer = 0f;
            
            // Trigger initial gold update event
            OnGoldChanged?.Invoke(m_CurrentGold);
        }
        
        private void Update()
        {
            // Handle passive gold generation
            GeneratePassiveGold();
        }
        
        private void GeneratePassiveGold()
        {
            if (m_PassiveGoldPerSecond <= 0f) return;

            // Only generate gold during Playing state
            if (TowerSurvivorsGameManager.Instance == null ||
                TowerSurvivorsGameManager.Instance.CurrentGameState != GameState.Playing)
            {
                return;
            }

            m_GoldTimer += Time.deltaTime;

            // Generate gold every second
            if (m_GoldTimer >= 1f)
            {
                m_GoldTimer -= 1f;
                AddGold(m_PassiveGoldPerSecond);
            }
        }
        
        public void AddGold(float amount)
        {
            if (amount <= 0f) return;
            
            m_CurrentGold += amount;
            OnGoldChanged?.Invoke(m_CurrentGold);
            
            Debug.Log($"Added {amount} gold. Current gold: {m_CurrentGold}");
        }
        
        public bool CanAfford(float cost)
        {
            return m_CurrentGold >= cost;
        }
        
        public bool SpendGold(float amount)
        {
            if (!CanAfford(amount))
            {
                Debug.LogWarning($"Not enough gold! Need {amount}, have {m_CurrentGold}");
                return false;
            }
            
            m_CurrentGold -= amount;
            OnGoldChanged?.Invoke(m_CurrentGold);
            
            Debug.Log($"Spent {amount} gold. Current gold: {m_CurrentGold}");
            return true;
        }
        
        public void SetPassiveGoldGeneration(float goldPerSecond)
        {
            m_PassiveGoldPerSecond = Mathf.Max(0f, goldPerSecond);
            Debug.Log($"Passive gold generation set to {m_PassiveGoldPerSecond} per second");
        }
        
        public void IncreasePassiveGoldGeneration(float increaseAmount)
        {
            m_PassiveGoldPerSecond += Mathf.Max(0f, increaseAmount);
            Debug.Log($"Passive gold generation increased by {increaseAmount}. New rate: {m_PassiveGoldPerSecond} per second");
        }
        
        public void ResetGold()
        {
            m_CurrentGold = m_StartingGold;
            m_PassiveGoldPerSecond = 1f; // Reset to base value
            m_GoldTimer = 0f;
            OnGoldChanged?.Invoke(m_CurrentGold);
        }
        
        // Static convenience methods for easy access
        public static void AddGoldStatic(float amount)
        {
            Instance?.AddGold(amount);
        }
        
        public static bool SpendGoldStatic(float amount)
        {
            return Instance?.SpendGold(amount) ?? false;
        }
        
        public static bool CanAffordStatic(float cost)
        {
            return Instance?.CanAfford(cost) ?? false;
        }
        
        public static float GetCurrentGold()
        {
            return Instance?.CurrentGold ?? 0f;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        // For testing purposes
        [ContextMenu("Add 100 Gold")]
        private void TestAddGold()
        {
            AddGold(100f);
        }
        
        [ContextMenu("Spend 50 Gold")]
        private void TestSpendGold()
        {
            SpendGold(50f);
        }
        
        [ContextMenu("Double Passive Generation")]
        private void TestDoublePassiveGeneration()
        {
            IncreasePassiveGoldGeneration(m_PassiveGoldPerSecond);
        }
    }
}