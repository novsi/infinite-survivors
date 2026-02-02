using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerSurvivors
{
    /// <summary>
    /// Tesla Coil weapon that fires chain lightning at enemies.
    /// Uses LineRenderer to create visual lightning effects between targets.
    /// </summary>
    public class TeslaCoil : MonoBehaviour
    {
        [Header("Lightning Settings")]
        [SerializeField] private LineRenderer m_LightningPrefab;
        [SerializeField] private float m_LightningDuration = 0.2f;
        [SerializeField] private int m_LightningSegments = 8;
        [SerializeField] private float m_LightningAmplitude = 0.3f;
        [SerializeField] private Material m_LightningMaterial;
        [SerializeField] private Color m_LightningColor = new Color(0.5f, 0.8f, 1f, 1f);
        [SerializeField] private float m_LightningWidth = 0.15f;

        private Weapon m_Weapon;
        private List<LineRenderer> m_ActiveLightning = new List<LineRenderer>();

        private void Awake()
        {
            m_Weapon = GetComponent<Weapon>();
            
            // Subscribe to weapon fire event
            if (m_Weapon != null)
            {
                m_Weapon.OnFire.AddListener(OnWeaponFire);
            }
        }

        private void OnDestroy()
        {
            if (m_Weapon != null)
            {
                m_Weapon.OnFire.RemoveListener(OnWeaponFire);
            }

            // Clean up any active lightning
            foreach (var lr in m_ActiveLightning)
            {
                if (lr != null)
                {
                    Destroy(lr.gameObject);
                }
            }
            m_ActiveLightning.Clear();
        }

        private void OnWeaponFire()
        {
            // Get the weapon data to check if it has chain lightning
            if (m_Weapon == null || m_Weapon.WeaponData == null || !m_Weapon.WeaponData.HasChainLightning)
                return;

            // Get enemies in range
            List<Enemy> chainTargets = GetChainTargets();
            if (chainTargets.Count > 0)
            {
                StartCoroutine(CreateLightningChain(chainTargets));
            }
        }

        private List<Enemy> GetChainTargets()
        {
            List<Enemy> targets = new List<Enemy>();

            // Start with the current target
            Enemy currentTarget = m_Weapon.CurrentTarget;
            if (currentTarget == null || currentTarget.IsDead) return targets;

            targets.Add(currentTarget);

            // Find chain targets
            Enemy lastTarget = currentTarget;
            int maxChains = m_Weapon.WeaponData.ChainTargets;
            float chainRange = m_Weapon.WeaponData.ChainRange;

            for (int i = 1; i < maxChains; i++)
            {
                Enemy nextTarget = FindNearestEnemyInRange(lastTarget.transform.position, chainRange, targets);
                if (nextTarget == null) break;

                targets.Add(nextTarget);
                lastTarget = nextTarget;
            }

            return targets;
        }

        private Enemy FindNearestEnemyInRange(Vector3 position, float range, List<Enemy> excludeTargets)
        {
            Enemy nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            Collider[] colliders = Physics.OverlapSphere(position, range);
            foreach (var collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead && !excludeTargets.Contains(enemy))
                {
                    float distance = Vector3.Distance(position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }

            return nearestEnemy;
        }

        private IEnumerator CreateLightningChain(List<Enemy> targets)
        {
            // Create lightning between weapon and first target
            if (targets.Count > 0)
            {
                CreateLightningSegment(transform.position, targets[0].transform.position);
            }

            // Create lightning between each pair of targets
            for (int i = 0; i < targets.Count - 1; i++)
            {
                CreateLightningSegment(targets[i].transform.position, targets[i + 1].transform.position);
            }

            // Wait for lightning duration
            yield return new WaitForSeconds(m_LightningDuration);

            // Clean up lightning
            foreach (var lr in m_ActiveLightning)
            {
                if (lr != null)
                {
                    Destroy(lr.gameObject);
                }
            }
            m_ActiveLightning.Clear();
        }

        private void CreateLightningSegment(Vector3 start, Vector3 end)
        {
            // Create a new LineRenderer for this segment
            GameObject lightningObj = new GameObject("Lightning");
            lightningObj.transform.SetParent(transform);

            LineRenderer lr = lightningObj.AddComponent<LineRenderer>();
            
            // Configure LineRenderer
            if (m_LightningMaterial != null)
            {
                lr.material = m_LightningMaterial;
            }
            else
            {
                // Use default unlit material
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }
            
            lr.startColor = m_LightningColor;
            lr.endColor = m_LightningColor;
            lr.startWidth = m_LightningWidth;
            lr.endWidth = m_LightningWidth * 0.5f;
            lr.positionCount = m_LightningSegments;
            lr.useWorldSpace = true;

            // Generate jagged lightning path
            GenerateLightningPath(lr, start, end);

            m_ActiveLightning.Add(lr);
        }

        private void GenerateLightningPath(LineRenderer lr, Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            float segmentLength = distance / (m_LightningSegments - 1);

            // Get perpendicular directions for displacement
            Vector3 perpX = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpX.magnitude < 0.1f)
            {
                perpX = Vector3.Cross(direction, Vector3.forward).normalized;
            }
            Vector3 perpY = Vector3.Cross(direction, perpX).normalized;

            for (int i = 0; i < m_LightningSegments; i++)
            {
                float t = (float)i / (m_LightningSegments - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);

                // Add random displacement (less at the endpoints)
                float displacement = m_LightningAmplitude * Mathf.Sin(t * Mathf.PI); // More in middle
                float offsetX = Random.Range(-displacement, displacement);
                float offsetY = Random.Range(-displacement, displacement);

                Vector3 pos = basePos + perpX * offsetX + perpY * offsetY;

                // Ensure endpoints are exact
                if (i == 0) pos = start;
                if (i == m_LightningSegments - 1) pos = end;

                lr.SetPosition(i, pos);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_Weapon != null && m_Weapon.WeaponData != null)
            {
                // Draw chain range
                Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, m_Weapon.WeaponData.ChainRange);
            }
        }
    }
}
