using UnityEngine;

namespace TowerSurvivors
{
    /// <summary>
    /// Projectile fired by ranged enemies at the tower.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float m_Speed = 15f;
        [SerializeField] private float m_Lifetime = 5f;

        private float m_Damage;
        private Transform m_Target;
        private Rigidbody m_Rigidbody;
        private float m_DamageMultiplier = 1f;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.useGravity = false;
            m_Rigidbody.isKinematic = false;
        }

        public void Initialize(float damage, Transform target, float damageMultiplier = 1f)
        {
            m_Damage = damage;
            m_Target = target;
            m_DamageMultiplier = damageMultiplier;

            // Calculate direction to target
            if (m_Target != null)
            {
                Vector3 direction = (m_Target.position - transform.position).normalized;
                m_Rigidbody.linearVelocity = direction * m_Speed;

                // Face the direction of travel
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Destroy after lifetime
            Destroy(gameObject, m_Lifetime);
        }

        public void Initialize(float damage, Vector3 direction, float damageMultiplier = 1f)
        {
            m_Damage = damage;
            m_DamageMultiplier = damageMultiplier;

            m_Rigidbody.linearVelocity = direction.normalized * m_Speed;
            transform.rotation = Quaternion.LookRotation(direction);

            Destroy(gameObject, m_Lifetime);
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
            // Check if we hit the tower
            TowerHealth tower = hitObject.GetComponent<TowerHealth>();
            if (tower != null && !tower.IsDead)
            {
                float scaledDamage = m_Damage * m_DamageMultiplier;
                tower.TakeDamage(scaledDamage);
                Debug.Log($"Enemy projectile hit tower for {scaledDamage} damage");
                Destroy(gameObject);
                return;
            }

            // Ignore enemies (don't destroy on enemy collision)
            Enemy enemy = hitObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                return;
            }

            // Hit something else (walls, ground, etc.)
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            // Draw projectile direction
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}
