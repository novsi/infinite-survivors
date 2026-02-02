using UnityEngine;

namespace TowerSurvivors
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Tower Survivors/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Stats")]
        [SerializeField] private float m_MaxHealth = 20f;
        [SerializeField] private float m_MoveSpeed = 5f;
        [SerializeField] private float m_Damage = 10f;
        [SerializeField] private float m_GoldDrop = 5f;
        
        [Header("Combat Settings")]
        [SerializeField] private float m_AttackRange = 2.5f;        // Range for melee attack (must be >= StoppingDistance)
        [SerializeField] private float m_AttackCooldown = 1f;       // Time between attacks

        [Header("AI Settings")]
        [SerializeField] private float m_StoppingDistance = 2f;     // How close enemy gets to tower before stopping
        [SerializeField] private float m_PathUpdateRate = 0.2f;     // How often to recalculate path
        
        [Header("Visual")]
        [SerializeField] private Color m_EnemyColor = Color.red;
        [SerializeField] private Vector3 m_Scale = Vector3.one;
        
        // Public accessors
        public float MaxHealth => m_MaxHealth;
        public float MoveSpeed => m_MoveSpeed;
        public float Damage => m_Damage;
        public float GoldDrop => m_GoldDrop;
        public float AttackRange => m_AttackRange;
        public float AttackCooldown => m_AttackCooldown;
        public float StoppingDistance => m_StoppingDistance;
        public float PathUpdateRate => m_PathUpdateRate;
        public Color EnemyColor => m_EnemyColor;
        public Vector3 Scale => m_Scale;
        
        private void OnValidate()
        {
            // Ensure values stay within reasonable ranges
            m_MaxHealth = Mathf.Max(1f, m_MaxHealth);
            m_MoveSpeed = Mathf.Max(0.1f, m_MoveSpeed);
            m_Damage = Mathf.Max(0f, m_Damage);
            m_GoldDrop = Mathf.Max(0f, m_GoldDrop);
            m_AttackRange = Mathf.Max(0.1f, m_AttackRange);
            m_AttackCooldown = Mathf.Max(0.1f, m_AttackCooldown);
            m_StoppingDistance = Mathf.Max(0.1f, m_StoppingDistance);
            m_PathUpdateRate = Mathf.Max(0.1f, m_PathUpdateRate);
        }
    }
}