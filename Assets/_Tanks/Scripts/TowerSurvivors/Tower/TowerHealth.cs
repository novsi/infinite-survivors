using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace TowerSurvivors
{
    public class TowerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public float m_MaxHealth = 100f;                    // Maximum health for the tower
        
        [Header("UI References")]
        public Slider m_HealthSlider;                       // Health slider UI element
        public Image m_FillImage;                           // Fill image for the health bar
        
        [Header("Visual Feedback")]
        public Color m_FullHealthColor = Color.green;       // Color when at full health
        public Color m_ZeroHealthColor = Color.red;         // Color when at zero health
        public GameObject m_ExplosionPrefab;                // Explosion effect on death
        
        [Header("Events")]
        public UnityEvent<float> OnHealthChanged;           // Event when health changes
        public UnityEvent OnDeath;                          // Event when tower dies
        
        private float m_CurrentHealth;                      // Current health value
        private bool m_IsDead;                              // Flag to prevent multiple death calls
        private AudioSource m_ExplosionAudio;              // Audio for explosion
        private ParticleSystem m_ExplosionParticles;        // Particles for explosion
        
        public float CurrentHealth => m_CurrentHealth;
        public float MaxHealth => m_MaxHealth;
        public bool IsDead => m_IsDead;
        
        private void Awake()
        {
            // Initialize explosion effect if prefab is assigned
            if (m_ExplosionPrefab != null)
            {
                m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
                m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();
                m_ExplosionParticles.gameObject.SetActive(false);
            }
            
            // Initialize events if not already done
            OnHealthChanged ??= new UnityEvent<float>();
            OnDeath ??= new UnityEvent();
        }
        
        private void Start()
        {
            // Set initial health
            m_CurrentHealth = m_MaxHealth;
            m_IsDead = false;
            
            // Setup health UI
            SetupHealthUI();
            UpdateHealthUI();
        }
        
        private void OnDestroy()
        {
            // Clean up explosion particles
            if (m_ExplosionParticles != null)
                Destroy(m_ExplosionParticles.gameObject);
        }
        
        public void TakeDamage(float damage)
        {
            if (m_IsDead || damage <= 0f) return;
            
            // Reduce health
            m_CurrentHealth = Mathf.Max(0f, m_CurrentHealth - damage);
            
            // Update UI and trigger events
            UpdateHealthUI();
            OnHealthChanged?.Invoke(m_CurrentHealth);
            
            // Check for death
            if (m_CurrentHealth <= 0f && !m_IsDead)
            {
                Die();
            }
        }
        
        public void Heal(float amount)
        {
            if (m_IsDead || amount <= 0f) return;
            
            // Increase health up to max
            m_CurrentHealth = Mathf.Min(m_MaxHealth, m_CurrentHealth + amount);
            
            // Update UI and trigger events
            UpdateHealthUI();
            OnHealthChanged?.Invoke(m_CurrentHealth);
        }
        
        public void SetMaxHealth(float newMaxHealth)
        {
            if (newMaxHealth <= 0f) return;
            
            // Calculate health percentage
            float healthPercentage = m_CurrentHealth / m_MaxHealth;
            
            // Update max health and current health proportionally
            m_MaxHealth = newMaxHealth;
            m_CurrentHealth = m_MaxHealth * healthPercentage;
            
            // Update UI
            SetupHealthUI();
            UpdateHealthUI();
            OnHealthChanged?.Invoke(m_CurrentHealth);
        }
        
        private void Die()
        {
            m_IsDead = true;
            
            // Trigger explosion effect
            if (m_ExplosionParticles != null)
            {
                m_ExplosionParticles.transform.position = transform.position;
                m_ExplosionParticles.gameObject.SetActive(true);
                m_ExplosionParticles.Play();
            }
            
            // Play explosion sound
            if (m_ExplosionAudio != null)
            {
                m_ExplosionAudio.Play();
            }
            
            // Trigger death event
            OnDeath?.Invoke();
            
            Debug.Log("Tower has been destroyed!");
        }
        
        private void SetupHealthUI()
        {
            if (m_HealthSlider != null)
            {
                m_HealthSlider.maxValue = m_MaxHealth;
                m_HealthSlider.value = m_CurrentHealth;
            }
        }
        
        private void UpdateHealthUI()
        {
            if (m_HealthSlider != null)
            {
                m_HealthSlider.value = m_CurrentHealth;
                
                // Update fill color based on health percentage
                if (m_FillImage != null)
                {
                    float healthPercentage = m_CurrentHealth / m_MaxHealth;
                    m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, healthPercentage);
                }
            }
        }
        
        // For testing purposes
        [ContextMenu("Test Take Damage")]
        private void TestTakeDamage()
        {
            TakeDamage(10f);
        }
        
        [ContextMenu("Test Heal")]
        private void TestHeal()
        {
            Heal(10f);
        }
    }
}