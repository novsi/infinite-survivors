using System.Collections.Generic;
using UnityEngine;

namespace TowerSurvivors
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private LayerMask m_EnemyLayerMask = -1;
        [SerializeField] private bool m_DestroyOnHit = true;
        
        private WeaponData m_WeaponData;
        private float m_Damage;
        private Transform m_Target;
        private Rigidbody m_Rigidbody;
        private float m_LifeTimer;
        private bool m_IsInitialized;
        private List<Enemy> m_HitEnemies = new List<Enemy>();
        
        // Tracking for guided projectiles
        private bool m_IsGuided;
        private float m_GuidanceStrength = 2f;
        
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }
        
        private void Start()
        {
            // Auto-initialize if not done manually
            if (!m_IsInitialized)
            {
                // Try to get reasonable defaults
                float defaultSpeed = 20f;
                Vector3 forward = transform.forward * defaultSpeed;
                m_Rigidbody.linearVelocity = forward;
                
                m_LifeTimer = 5f; // Default 5 second lifetime
                m_Damage = 10f; // Default damage
            }
        }
        
        private void Update()
        {
            if (!m_IsInitialized) return;
            
            UpdateLifetime();
            UpdateGuidance();
        }
        
        public void Initialize(WeaponData weaponData, float damage, Transform target = null)
        {
            m_WeaponData = weaponData;
            m_Damage = damage;
            m_Target = target;
            m_IsInitialized = true;
            
            // Set up projectile physics
            if (weaponData != null)
            {
                m_LifeTimer = weaponData.ProjectileLifetime;
                
                // Set initial velocity
                Vector3 direction = target != null 
                    ? (target.position - transform.position).normalized 
                    : transform.forward;
                
                m_Rigidbody.linearVelocity = direction * weaponData.ProjectileSpeed;
                
                // Enable guidance for certain weapon types
                m_IsGuided = target != null && (weaponData.DamageType == DamageType.Magic);
            }
            
            // Schedule destruction
            Destroy(gameObject, m_LifeTimer);
        }
        
        public void Initialize(float damage, Vector3 direction, float speed, float lifetime = 5f)
        {
            m_Damage = damage;
            m_LifeTimer = lifetime;
            m_IsInitialized = true;
            
            m_Rigidbody.linearVelocity = direction.normalized * speed;
            
            Destroy(gameObject, m_LifeTimer);
        }
        
        private void UpdateLifetime()
        {
            m_LifeTimer -= Time.deltaTime;
            
            if (m_LifeTimer <= 0f)
            {
                DestroyProjectile();
            }
        }
        
        private void UpdateGuidance()
        {
            if (!m_IsGuided || m_Target == null) return;
            
            // Simple homing behavior
            Vector3 targetDirection = (m_Target.position - transform.position).normalized;
            Vector3 currentVelocity = m_Rigidbody.linearVelocity;
            
            // Gradually adjust velocity towards target
            Vector3 desiredVelocity = targetDirection * currentVelocity.magnitude;
            Vector3 steerForce = (desiredVelocity - currentVelocity) * m_GuidanceStrength;
            
            m_Rigidbody.AddForce(steerForce, ForceMode.Acceleration);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other.gameObject);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision.gameObject);
        }
        
        private void HandleCollision(GameObject hitObject)
        {
            // Check if we hit an enemy
            Enemy enemy = hitObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                HandleEnemyHit(enemy);
                return;
            }
            
            // Check if we hit the tower (friendly fire prevention)
            TowerHealth tower = hitObject.GetComponent<TowerHealth>();
            if (tower != null)
            {
                return; // Don't damage tower or destroy projectile
            }
            
            // Hit something else (wall, ground, etc.)
            if (m_DestroyOnHit)
            {
                CreateImpactEffect();
                DestroyProjectile();
            }
        }
        
        private void HandleEnemyHit(Enemy enemy)
        {
            if (enemy == null || enemy.IsDead) return;
            
            // Deal damage to primary target
            enemy.TakeDamage(m_Damage);
            m_HitEnemies.Add(enemy);
            
            // Handle weapon-specific effects
            if (m_WeaponData != null)
            {
                // Piercing projectiles
                if (m_WeaponData.PierceEnemies && m_HitEnemies.Count < m_WeaponData.MaxPierceTargets)
                {
                    // Don't destroy on hit, let it continue
                    return;
                }
                
                // Area damage
                if (m_WeaponData.HasAreaDamage)
                {
                    HandleAreaDamage(enemy.transform.position);
                }
                
                // Chain lightning (if projectile somehow has it)
                if (m_WeaponData.HasChainLightning)
                {
                    HandleChainLightning(enemy);
                }
            }
            
            // Create impact effect
            CreateImpactEffect();
            
            // Destroy projectile if it should be destroyed on hit
            if (m_DestroyOnHit)
            {
                DestroyProjectile();
            }
        }
        
        private void HandleAreaDamage(Vector3 explosionCenter)
        {
            if (m_WeaponData == null || !m_WeaponData.HasAreaDamage) return;
            
            Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, m_WeaponData.AreaDamageRadius, m_EnemyLayerMask);
            
            foreach (var collider in hitColliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead && !m_HitEnemies.Contains(enemy))
                {
                    // Calculate damage falloff based on distance
                    float distance = Vector3.Distance(explosionCenter, enemy.transform.position);
                    float damageMultiplier = 1f - (distance / m_WeaponData.AreaDamageRadius);
                    damageMultiplier = Mathf.Clamp01(damageMultiplier);
                    
                    float areaDamage = m_Damage * damageMultiplier;
                    enemy.TakeDamage(areaDamage);
                }
            }
            
            // Create explosion effect
            CreateExplosionEffect(explosionCenter);
        }
        
        private void HandleChainLightning(Enemy firstTarget)
        {
            if (m_WeaponData == null || !m_WeaponData.HasChainLightning) return;
            
            List<Enemy> chainTargets = new List<Enemy> { firstTarget };
            float currentDamage = m_Damage * m_WeaponData.ChainDamageMultiplier; // Reduced for subsequent targets
            
            Enemy lastTarget = firstTarget;
            for (int i = 1; i < m_WeaponData.ChainTargets; i++)
            {
                Enemy nextTarget = FindChainTarget(lastTarget, chainTargets);
                if (nextTarget == null) break;
                
                chainTargets.Add(nextTarget);
                nextTarget.TakeDamage(currentDamage);
                currentDamage *= m_WeaponData.ChainDamageMultiplier;
                
                lastTarget = nextTarget;
            }
            
            // Create chain lightning visual effect
            CreateChainLightningEffect(chainTargets);
        }
        
        private Enemy FindChainTarget(Enemy fromEnemy, List<Enemy> excludeTargets)
        {
            Enemy chainTarget = null;
            float nearestDistance = float.MaxValue;
            
            Collider[] nearbyColliders = Physics.OverlapSphere(fromEnemy.transform.position, m_WeaponData.ChainRange, m_EnemyLayerMask);
            
            foreach (var collider in nearbyColliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead && !excludeTargets.Contains(enemy))
                {
                    float distance = Vector3.Distance(fromEnemy.transform.position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        chainTarget = enemy;
                    }
                }
            }
            
            return chainTarget;
        }
        
        private void CreateImpactEffect()
        {
            // Create basic impact effect
            // TODO: Replace with proper effect based on weapon type
            GameObject effect = new GameObject("Impact Effect");
            effect.transform.position = transform.position;
            
            // Add particle effect if available
            // For now, just log the impact
            Debug.Log($"Projectile impact at {transform.position}");
            
            Destroy(effect, 1f);
        }
        
        private void CreateExplosionEffect(Vector3 position)
        {
            // Create explosion effect
            // TODO: Replace with proper explosion prefab
            GameObject explosion = new GameObject("Explosion Effect");
            explosion.transform.position = position;
            
            Debug.Log($"Explosion at {position} with radius {m_WeaponData.AreaDamageRadius}");
            
            Destroy(explosion, 2f);
        }
        
        private void CreateChainLightningEffect(List<Enemy> targets)
        {
            // Create chain lightning visual effect
            // TODO: Replace with proper lightning effect
            Debug.Log($"Chain lightning connected {targets.Count} targets");
        }
        
        private void DestroyProjectile()
        {
            // Final cleanup before destruction
            Destroy(gameObject);
        }
        
        private void OnDrawGizmos()
        {
            if (m_WeaponData != null && m_WeaponData.HasAreaDamage)
            {
                // Draw area damage radius
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, m_WeaponData.AreaDamageRadius);
            }
        }
    }
}