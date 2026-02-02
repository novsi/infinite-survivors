using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TowerSurvivors
{
    public class Weapon : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [SerializeField] private WeaponData m_WeaponData;
        
        [Header("Firing Points")]
        [SerializeField] private Transform m_FirePoint;
        [SerializeField] private Transform m_MuzzleFlashPoint;
        
        [Header("Events")]
        public UnityEvent OnFire;                       // Event when weapon fires
        public UnityEvent<Enemy> OnTargetAcquired;      // Event when new target found
        
        private Enemy m_CurrentTarget;
        private float m_LastFireTime;
        private List<Enemy> m_EnemiesInRange = new List<Enemy>();
        private AudioSource m_AudioSource;
        
        // Upgrade multipliers (can be modified by upgrade system)
        private float m_DamageMultiplier = 1f;
        private float m_FireRateMultiplier = 1f;
        private float m_RangeMultiplier = 1f;
        
        public WeaponData WeaponData => m_WeaponData;
        public Enemy CurrentTarget => m_CurrentTarget;
        public bool HasTarget => m_CurrentTarget != null && !m_CurrentTarget.IsDead;
        public float NextFireTime => m_LastFireTime + CalculatedFireCooldown;
        public bool CanFire => Time.time >= NextFireTime;
        
        // Calculated properties with multipliers
        public float CalculatedDamage => m_WeaponData != null ? m_WeaponData.Damage * m_DamageMultiplier : 0f;
        public float CalculatedFireCooldown => m_WeaponData != null ? m_WeaponData.FireCooldown / m_FireRateMultiplier : 1f;
        public float CalculatedRange => m_WeaponData != null ? m_WeaponData.Range * m_RangeMultiplier : 0f;
        
        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            
            // Initialize events
            OnFire ??= new UnityEvent();
            OnTargetAcquired ??= new UnityEvent<Enemy>();
            
            // Set fire point to weapon transform if not assigned
            if (m_FirePoint == null)
                m_FirePoint = transform;
            
            if (m_MuzzleFlashPoint == null)
                m_MuzzleFlashPoint = m_FirePoint;
        }
        
        private void Start()
        {
            if (m_WeaponData == null)
            {
                Debug.LogError($"Weapon {gameObject.name} has no WeaponData assigned!");
                enabled = false;
            }
        }
        
        private void Update()
        {
            if (m_WeaponData == null) return;
            
            UpdateTarget();
            
            if (HasTarget && CanFire)
            {
                Fire();
            }
        }
        
        private void UpdateTarget()
        {
            // Find enemies in range
            FindEnemiesInRange();
            
            // Check if current target is still valid
            if (!IsValidTarget(m_CurrentTarget))
            {
                m_CurrentTarget = null;
            }
            
            // Find new target if we don't have one
            if (m_CurrentTarget == null)
            {
                m_CurrentTarget = FindNearestEnemy();
                
                if (m_CurrentTarget != null)
                {
                    OnTargetAcquired?.Invoke(m_CurrentTarget);
                }
            }
        }
        
        private void FindEnemiesInRange()
        {
            m_EnemiesInRange.Clear();
            
            // Find all enemies in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, CalculatedRange);
            
            foreach (var collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    m_EnemiesInRange.Add(enemy);
                }
            }
        }
        
        private bool IsValidTarget(Enemy target)
        {
            if (target == null || target.IsDead) return false;
            
            float distance = Vector3.Distance(transform.position, target.transform.position);
            return distance <= CalculatedRange;
        }
        
        private Enemy FindNearestEnemy()
        {
            Enemy nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (Enemy enemy in m_EnemiesInRange)
            {
                if (enemy == null || enemy.IsDead) continue;
                
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
            
            return nearestEnemy;
        }
        
        public void Fire()
        {
            if (!CanFire || !HasTarget || m_WeaponData == null) return;
            
            m_LastFireTime = Time.time;
            
            // Aim at target
            Vector3 targetDirection = (m_CurrentTarget.transform.position - m_FirePoint.position).normalized;
            
            // Handle different weapon types
            if (m_WeaponData.HasChainLightning)
            {
                FireChainLightning();
            }
            else if (m_WeaponData.ProjectilePrefab != null)
            {
                FireProjectile(targetDirection);
            }
            else
            {
                // Instant hit weapon (laser, etc.)
                FireInstantHit();
            }
            
            // Play effects
            PlayFireEffects();
            
            // Trigger fire event
            OnFire?.Invoke();
        }
        
        private void FireProjectile(Vector3 direction)
        {
            if (m_WeaponData.ProjectilePrefab == null) return;
            
            // Instantiate projectile
            GameObject projectileObj = Instantiate(m_WeaponData.ProjectilePrefab, m_FirePoint.position, Quaternion.LookRotation(direction));
            
            // Setup projectile
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(m_WeaponData, CalculatedDamage, m_CurrentTarget.transform);
            }
            else
            {
                // Fallback: basic rigidbody projectile
                Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * m_WeaponData.ProjectileSpeed;
                }
                
                // Destroy after lifetime
                Destroy(projectileObj, m_WeaponData.ProjectileLifetime);
            }
        }
        
        private void FireInstantHit()
        {
            // Deal damage directly to current target
            if (m_CurrentTarget != null)
            {
                m_CurrentTarget.TakeDamage(CalculatedDamage);
                
                // Handle piercing
                if (m_WeaponData.PierceEnemies)
                {
                    HandlePiercing();
                }
                
                // Handle area damage
                if (m_WeaponData.HasAreaDamage)
                {
                    HandleAreaDamage(m_CurrentTarget.transform.position);
                }
            }
        }
        
        private void FireChainLightning()
        {
            if (m_CurrentTarget == null) return;
            
            List<Enemy> chainTargets = new List<Enemy> { m_CurrentTarget };
            float currentDamage = CalculatedDamage;
            
            // Deal damage to primary target
            m_CurrentTarget.TakeDamage(currentDamage);
            
            // Find chain targets
            Enemy lastTarget = m_CurrentTarget;
            for (int i = 1; i < m_WeaponData.ChainTargets; i++)
            {
                Enemy nextTarget = FindChainTarget(lastTarget, chainTargets);
                if (nextTarget == null) break;
                
                chainTargets.Add(nextTarget);
                currentDamage *= m_WeaponData.ChainDamageMultiplier;
                nextTarget.TakeDamage(currentDamage);
                
                lastTarget = nextTarget;
            }
            
            // TODO: Create visual lightning effect between targets
            CreateLightningEffect(chainTargets);
        }
        
        private Enemy FindChainTarget(Enemy fromEnemy, List<Enemy> excludeTargets)
        {
            Enemy chainTarget = null;
            float nearestDistance = float.MaxValue;
            
            foreach (Enemy enemy in m_EnemiesInRange)
            {
                if (enemy == null || enemy.IsDead || excludeTargets.Contains(enemy)) continue;
                
                float distance = Vector3.Distance(fromEnemy.transform.position, enemy.transform.position);
                if (distance <= m_WeaponData.ChainRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    chainTarget = enemy;
                }
            }
            
            return chainTarget;
        }
        
        private void HandlePiercing()
        {
            // Find enemies in line of fire for piercing
            Vector3 direction = (m_CurrentTarget.transform.position - m_FirePoint.position).normalized;
            RaycastHit[] hits = Physics.RaycastAll(m_FirePoint.position, direction, CalculatedRange);
            
            int pierceCount = 0;
            foreach (var hit in hits)
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null && enemy != m_CurrentTarget && !enemy.IsDead && pierceCount < m_WeaponData.MaxPierceTargets)
                {
                    enemy.TakeDamage(CalculatedDamage);
                    pierceCount++;
                }
            }
        }
        
        private void HandleAreaDamage(Vector3 center)
        {
            Collider[] hitColliders = Physics.OverlapSphere(center, m_WeaponData.AreaDamageRadius);
            
            foreach (var collider in hitColliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && enemy != m_CurrentTarget && !enemy.IsDead)
                {
                    // Calculate damage falloff based on distance
                    float distance = Vector3.Distance(center, enemy.transform.position);
                    float damageMultiplier = 1f - (distance / m_WeaponData.AreaDamageRadius);
                    float areaDamage = CalculatedDamage * damageMultiplier;
                    
                    enemy.TakeDamage(areaDamage);
                }
            }
        }
        
        private void CreateLightningEffect(List<Enemy> targets)
        {
            // TODO: Implement visual lightning effect
            // For now, just log the chain
            Debug.Log($"Chain lightning hit {targets.Count} targets");
        }
        
        private void PlayFireEffects()
        {
            // Play muzzle flash
            if (m_WeaponData.MuzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(m_WeaponData.MuzzleFlashPrefab, m_MuzzleFlashPoint.position, m_MuzzleFlashPoint.rotation);
                Destroy(flash, 1f);
            }
            
            // Play fire sound
            if (m_WeaponData.FireSound != null && m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(m_WeaponData.FireSound);
            }
        }
        
        public void ApplyDamageUpgrade(float multiplier)
        {
            m_DamageMultiplier *= multiplier;
        }
        
        public void ApplyFireRateUpgrade(float multiplier)
        {
            m_FireRateMultiplier *= multiplier;
        }
        
        public void ApplyRangeUpgrade(float multiplier)
        {
            m_RangeMultiplier *= multiplier;
        }
        
        public void ResetUpgrades()
        {
            m_DamageMultiplier = 1f;
            m_FireRateMultiplier = 1f;
            m_RangeMultiplier = 1f;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (m_WeaponData != null)
            {
                // Draw weapon range
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, CalculatedRange);
                
                // Draw line to current target
                if (HasTarget)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(m_FirePoint.position, m_CurrentTarget.transform.position);
                }
            }
        }
        
        // For testing purposes
        [ContextMenu("Test Fire")]
        private void TestFire()
        {
            if (HasTarget)
            {
                Fire();
            }
        }
    }
}