using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace TowerSurvivors
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Configuration")]
        [SerializeField] private EnemyData m_EnemyData;
        
        [Header("References")]
        [SerializeField] private GameObject m_DeathEffectPrefab;
        
        [Header("Events")]
        public UnityEvent<float> OnDeath;           // Event triggered on death, passes gold amount
        public UnityEvent<float> OnTakeDamage;      // Event triggered when taking damage
        
        private NavMeshAgent m_NavAgent;
        private Transform m_TowerTarget;
        private float m_CurrentHealth;
        private bool m_IsDead;
        private float m_LastAttackTime;
        private float m_PathUpdateTimer;
        
        // Wave scaling multipliers
        private float m_HealthMultiplier = 1f;
        private float m_DamageMultiplier = 1f;
        private float m_SpeedMultiplier = 1f;
        
        // Cached values for performance
        private TowerHealth m_TowerHealth;
        
        public EnemyData EnemyData => m_EnemyData;
        public float CurrentHealth => m_CurrentHealth;
        public float MaxHealth => m_EnemyData != null ? m_EnemyData.MaxHealth * m_HealthMultiplier : 0f;
        public bool IsDead => m_IsDead;
        
        private void Awake()
        {
            m_NavAgent = GetComponent<NavMeshAgent>();
            
            // Initialize events
            OnDeath ??= new UnityEvent<float>();
            OnTakeDamage ??= new UnityEvent<float>();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (m_IsDead || m_EnemyData == null) return;
            
            UpdatePathfinding();
            UpdateAttack();
        }
        
        public void Initialize(float healthMultiplier = 1f, float damageMultiplier = 1f, float speedMultiplier = 1f)
        {
            if (m_EnemyData == null)
            {
                Debug.LogError($"Enemy {gameObject.name} has no EnemyData assigned!");
                return;
            }
            
            // Store scaling multipliers
            m_HealthMultiplier = healthMultiplier;
            m_DamageMultiplier = damageMultiplier;
            m_SpeedMultiplier = speedMultiplier;
            
            // Initialize health
            m_CurrentHealth = MaxHealth;
            m_IsDead = false;
            m_LastAttackTime = 0f;
            m_PathUpdateTimer = 0f;
            
            // Configure NavMeshAgent
            SetupNavAgent();
            
            // Apply visual scaling
            if (m_EnemyData.Scale != Vector3.one)
            {
                transform.localScale = m_EnemyData.Scale;
            }
            
            // Find tower target
            FindTowerTarget();
        }
        
        private void SetupNavAgent()
        {
            if (m_NavAgent != null && m_EnemyData != null)
            {
                m_NavAgent.speed = m_EnemyData.MoveSpeed * m_SpeedMultiplier;
                m_NavAgent.stoppingDistance = m_EnemyData.StoppingDistance;
                m_NavAgent.angularSpeed = 240f;
                m_NavAgent.acceleration = 8f;
            }
        }
        
        private void FindTowerTarget()
        {
            // Find the tower in the scene (assuming it has the TowerHealth component)
            m_TowerHealth = FindObjectOfType<TowerHealth>();
            if (m_TowerHealth != null)
            {
                m_TowerTarget = m_TowerHealth.transform;
            }
            else
            {
                Debug.LogWarning("Enemy could not find tower target!");
            }
        }
        
        private void UpdatePathfinding()
        {
            if (m_NavAgent == null || m_TowerTarget == null) return;
            
            // Update path periodically to avoid constant recalculation
            m_PathUpdateTimer += Time.deltaTime;
            if (m_PathUpdateTimer >= m_EnemyData.PathUpdateRate)
            {
                m_PathUpdateTimer = 0f;
                m_NavAgent.SetDestination(m_TowerTarget.position);
            }
        }
        
        private void UpdateAttack()
        {
            if (m_TowerTarget == null || m_TowerHealth == null || m_TowerHealth.IsDead) return;
            
            // Check if in attack range
            float distanceToTower = Vector3.Distance(transform.position, m_TowerTarget.position);
            
            if (distanceToTower <= m_EnemyData.AttackRange)
            {
                // Check attack cooldown
                if (Time.time >= m_LastAttackTime + m_EnemyData.AttackCooldown)
                {
                    AttackTower();
                    m_LastAttackTime = Time.time;
                }
            }
        }
        
        private void AttackTower()
        {
            if (m_TowerHealth != null && !m_TowerHealth.IsDead)
            {
                float damage = m_EnemyData.Damage * m_DamageMultiplier;
                m_TowerHealth.TakeDamage(damage);
                Debug.Log($"{gameObject.name} attacked tower for {damage} damage!");
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (m_IsDead || damage <= 0f) return;
            
            m_CurrentHealth -= damage;
            OnTakeDamage?.Invoke(damage);
            
            if (m_CurrentHealth <= 0f)
            {
                Die();
            }
        }
        
        private void Die()
        {
            if (m_IsDead) return;
            
            m_IsDead = true;
            
            // Calculate gold to drop (scaled)
            float goldAmount = m_EnemyData.GoldDrop;
            
            // Trigger death event with gold amount
            OnDeath?.Invoke(goldAmount);
            
            // Play death effect
            PlayDeathEffect();
            
            Debug.Log($"{gameObject.name} died and dropped {goldAmount} gold");
            
            // Destroy the enemy
            Destroy(gameObject, 0.1f); // Small delay to allow effects to play
        }
        
        private void PlayDeathEffect()
        {
            if (m_DeathEffectPrefab != null)
            {
                GameObject effect = Instantiate(m_DeathEffectPrefab, transform.position, transform.rotation);
                
                // Auto-destroy the effect after a few seconds
                ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    Destroy(effect, particles.main.duration + 1f);
                }
                else
                {
                    Destroy(effect, 3f);
                }
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // Handle collision with tower (backup attack method)
            TowerHealth towerHealth = collision.gameObject.GetComponent<TowerHealth>();
            if (towerHealth != null && !towerHealth.IsDead)
            {
                // Only attack if not on cooldown
                if (Time.time >= m_LastAttackTime + m_EnemyData.AttackCooldown)
                {
                    float damage = m_EnemyData.Damage * m_DamageMultiplier;
                    towerHealth.TakeDamage(damage);
                    m_LastAttackTime = Time.time;
                    
                    Debug.Log($"{gameObject.name} collided with tower for {damage} damage!");
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (m_EnemyData != null)
            {
                // Draw attack range
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, m_EnemyData.AttackRange);
                
                // Draw stopping distance
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, m_EnemyData.StoppingDistance);
            }
        }
        
        // For testing purposes
        [ContextMenu("Test Take Damage")]
        private void TestTakeDamage()
        {
            TakeDamage(10f);
        }
    }
}