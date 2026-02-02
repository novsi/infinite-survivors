using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TowerSurvivors
{
    /// <summary>
    /// Creates a red screen flash effect when the tower takes damage.
    /// Attach to a full-screen Image UI element.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class DamageScreenEffect : MonoBehaviour
    {
        [Header("Flash Settings")]
        [SerializeField] private Color m_FlashColor = new Color(1f, 0f, 0f, 0.4f);  // Red with 40% opacity
        [SerializeField] private float m_FlashDuration = 0.15f;    // How long flash stays at peak
        [SerializeField] private float m_FadeInDuration = 0.05f;   // How fast flash appears
        [SerializeField] private float m_FadeOutDuration = 0.2f;   // How fast flash fades

        [Header("Intensity Scaling")]
        [SerializeField] private bool m_ScaleWithDamage = true;    // Scale intensity with damage amount
        [SerializeField] private float m_MinDamageForMaxFlash = 50f;  // Damage amount for max flash
        [SerializeField] private float m_MinFlashAlpha = 0.2f;     // Minimum flash opacity
        [SerializeField] private float m_MaxFlashAlpha = 0.6f;     // Maximum flash opacity

        [Header("References")]
        [SerializeField] private TowerHealth m_TowerHealth;        // Reference to tower health

        private Image m_FlashImage;
        private Coroutine m_CurrentFlash;
        private float m_LastHealth;

        private void Awake()
        {
            m_FlashImage = GetComponent<Image>();
            m_FlashImage.color = new Color(m_FlashColor.r, m_FlashColor.g, m_FlashColor.b, 0f);
            m_FlashImage.raycastTarget = false; // Don't block UI interactions
        }

        private void Start()
        {
            // Auto-find tower health if not assigned
            if (m_TowerHealth == null)
            {
                m_TowerHealth = FindObjectOfType<TowerHealth>();
            }

            if (m_TowerHealth != null)
            {
                m_LastHealth = m_TowerHealth.CurrentHealth;
                m_TowerHealth.OnHealthChanged.AddListener(OnHealthChanged);
            }
            else
            {
                Debug.LogWarning("DamageScreenEffect: No TowerHealth found!");
            }
        }

        private void OnDestroy()
        {
            if (m_TowerHealth != null)
            {
                m_TowerHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
            }
        }

        private void OnHealthChanged(float newHealth)
        {
            // Only flash if health decreased (damage taken, not healing)
            float damage = m_LastHealth - newHealth;
            if (damage > 0f)
            {
                TriggerFlash(damage);
            }
            m_LastHealth = newHealth;
        }

        /// <summary>
        /// Triggers the red flash effect.
        /// </summary>
        /// <param name="damageAmount">Amount of damage taken (for intensity scaling)</param>
        public void TriggerFlash(float damageAmount = 0f)
        {
            if (m_CurrentFlash != null)
            {
                StopCoroutine(m_CurrentFlash);
            }
            m_CurrentFlash = StartCoroutine(FlashCoroutine(damageAmount));
        }

        private IEnumerator FlashCoroutine(float damageAmount)
        {
            // Calculate flash intensity based on damage
            float flashAlpha = m_FlashColor.a;
            if (m_ScaleWithDamage && damageAmount > 0f)
            {
                float damageRatio = Mathf.Clamp01(damageAmount / m_MinDamageForMaxFlash);
                flashAlpha = Mathf.Lerp(m_MinFlashAlpha, m_MaxFlashAlpha, damageRatio);
            }

            Color targetColor = new Color(m_FlashColor.r, m_FlashColor.g, m_FlashColor.b, flashAlpha);
            Color clearColor = new Color(m_FlashColor.r, m_FlashColor.g, m_FlashColor.b, 0f);

            // Fade in
            float elapsed = 0f;
            Color startColor = m_FlashImage.color;
            while (elapsed < m_FadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / m_FadeInDuration;
                m_FlashImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            m_FlashImage.color = targetColor;

            // Hold at peak
            yield return new WaitForSecondsRealtime(m_FlashDuration);

            // Fade out
            elapsed = 0f;
            startColor = m_FlashImage.color;
            while (elapsed < m_FadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / m_FadeOutDuration;
                m_FlashImage.color = Color.Lerp(startColor, clearColor, t);
                yield return null;
            }
            m_FlashImage.color = clearColor;

            m_CurrentFlash = null;
        }

        // For testing
        [ContextMenu("Test Flash (Small Damage)")]
        private void TestFlashSmall()
        {
            TriggerFlash(10f);
        }

        [ContextMenu("Test Flash (Large Damage)")]
        private void TestFlashLarge()
        {
            TriggerFlash(50f);
        }
    }
}
