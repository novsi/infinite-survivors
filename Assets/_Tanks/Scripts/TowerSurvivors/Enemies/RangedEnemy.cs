using UnityEngine;

namespace TowerSurvivors
{
    /// <summary>
    /// A ranged enemy that stops at a distance from the tower and fires projectiles.
    /// Extends the base Enemy behavior with ranged attack capability.
    /// </summary>
    public class RangedEnemy : MonoBehaviour
    {
        [Header("Ranged Attack Settings")]
        [SerializeField] private GameObject m_ProjectilePrefab;
        [SerializeField] private Transform m_FirePoint;
        [SerializeField] private float m_ProjectileSpeed = 15f;

        private Enemy m_Enemy;

        private void Awake()
        {
            m_Enemy = GetComponent<Enemy>();

            // Set fire point to self if not specified
            if (m_FirePoint == null)
            {
                m_FirePoint = transform;
            }
        }

        private void Start()
        {
            // Subscribe to the enemy's attack logic
            // We'll override the attack behavior by hooking into the existing system
        }

        private void Update()
        {
            if (m_Enemy == null || m_Enemy.IsDead) return;

            // Check if we're in range to attack
            TowerHealth tower = FindObjectOfType<TowerHealth>();
            if (tower == null || tower.IsDead) return;

            float distanceToTower = Vector3.Distance(transform.position, tower.transform.position);

            // If we're within attack range (which equals stopping distance for ranged enemies),
            // stop moving and face the tower
            if (m_Enemy.EnemyData != null && distanceToTower <= m_Enemy.EnemyData.AttackRange)
            {
                // Face the tower
                Vector3 directionToTower = (tower.transform.position - transform.position).normalized;
                directionToTower.y = 0;
                if (directionToTower != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(directionToTower);
                }
            }
        }

        /// <summary>
        /// Called by Enemy when performing an attack (via message or direct call).
        /// This intercepts the attack to fire a projectile instead of melee damage.
        /// </summary>
        public void FireProjectile()
        {
            if (m_ProjectilePrefab == null || m_Enemy == null || m_Enemy.EnemyData == null)
            {
                Debug.LogWarning("RangedEnemy: Cannot fire - missing projectile prefab or enemy data");
                return;
            }

            TowerHealth tower = FindObjectOfType<TowerHealth>();
            if (tower == null || tower.IsDead) return;

            // Calculate direction to tower
            Vector3 direction = (tower.transform.position - m_FirePoint.position).normalized;

            // Instantiate projectile
            GameObject projectileObj = Instantiate(m_ProjectilePrefab, m_FirePoint.position, Quaternion.LookRotation(direction));

            // Initialize the projectile
            EnemyProjectile projectile = projectileObj.GetComponent<EnemyProjectile>();
            if (projectile != null)
            {
                // Get the damage multiplier from the enemy (set during wave scaling)
                float damageMultiplier = GetDamageMultiplier();
                projectile.Initialize(m_Enemy.EnemyData.Damage, tower.transform, damageMultiplier);
            }
            else
            {
                // Fallback: basic rigidbody projectile
                Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * m_ProjectileSpeed;
                }
                Destroy(projectileObj, 5f);
            }

            Debug.Log($"RangedEnemy fired projectile at tower");
        }

        private float GetDamageMultiplier()
        {
            return m_Enemy != null ? m_Enemy.DamageMultiplier : 1f;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_Enemy != null && m_Enemy.EnemyData != null)
            {
                // Draw attack range
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, m_Enemy.EnemyData.AttackRange);
            }
        }
    }
}
