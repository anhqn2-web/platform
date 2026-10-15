using UnityEngine;
using System.Collections;

namespace WarriorPlatformer
{
    /// <summary>
    /// A simple dummy target that can be placed in the scene to test attacks.
    /// It responds to hits by changing color, vibrating, and displaying damage logs.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class DummyTarget : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Visual Effects")]
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private float shakeAmount = 0.1f;
        [SerializeField] private float shakeDuration = 0.2f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Vector3 originalPosition;
        private Coroutine hitEffectsCoroutine;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            originalPosition = transform.position;
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage, Vector2 damageSourcePosition)
        {
            currentHealth -= damage;
            Debug.Log($"[DummyTarget] Hit! Took {damage} damage. Current Health: {currentHealth}/{maxHealth}");

            // Trigger visual feedback
            if (hitEffectsCoroutine != null)
            {
                StopCoroutine(hitEffectsCoroutine);
            }
            hitEffectsCoroutine = StartCoroutine(HitFeedbackRoutine());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator HitFeedbackRoutine()
        {
            float elapsed = 0f;
            spriteRenderer.color = hitColor;

            while (elapsed < shakeDuration)
            {
                // Simple screen shake effect on the dummy
                float offsetX = Random.Range(-shakeAmount, shakeAmount);
                float offsetY = Random.Range(-shakeAmount, shakeAmount);
                transform.position = originalPosition + new Vector3(offsetX, offsetY, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPosition;
            spriteRenderer.color = originalColor;
        }

        private void Die()
        {
            Debug.Log("[DummyTarget] Destoyed! Respawning in 3 seconds...");
            gameObject.SetActive(false);
            Invoke(nameof(Respawn), 3f);
        }

        private void Respawn()
        {
            currentHealth = maxHealth;
            transform.position = originalPosition;
            gameObject.SetActive(true);
            spriteRenderer.color = originalColor;
            Debug.Log("[DummyTarget] Respawned and ready for action!");
        }
    }
}
