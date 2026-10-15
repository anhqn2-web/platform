using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace WarriorPlatformer
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class EnemyAI : MonoBehaviour, IDamageable
    {
        [Header("Stats Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float moveSpeed = 3f;

        [Header("AI Senses")]
        [Tooltip("The range at which the enemy detects the player and starts chasing.")]
        [SerializeField] private float detectionRadius = 6f;
        [Tooltip("The range at which the enemy stops and attacks the player.")]
        [SerializeField] private float attackRadius = 1.2f;

        [Header("Combat Settings")]
        [SerializeField] private float damage = 15f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float knockbackForceX = 4f;
        [SerializeField] private float knockbackForceY = 3f;

        [Header("UI & Visual Settings")]
        [Tooltip("Height offset above the enemy's center to display the floating health bar.")]
        [SerializeField] private float healthBarHeightOffset = 1.5f;
        [Tooltip("Width of the floating health bar.")]
        [SerializeField] private float healthBarWidth = 1.2f;
        [Tooltip("Height of the floating health bar.")]
        [SerializeField] private float healthBarHeight = 0.15f;

        // Components
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        // State Tracking
        private Transform playerTarget;
        private WarriorController playerController;
        private float lastAttackTime;
        private bool isDead;
        private bool isFacingRight = true;
        private bool isHurt;

        // Level Management Event
        public static System.Action<EnemyAI> OnEnemyDeath;

        // Programmatic UI Elements (Created at runtime for zero-setup ease of use)
        private GameObject healthBarCanvasObj;
        private GameObject healthBarFillObj;
        private Vector3 originalScale;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>(); // Optional component

            currentHealth = maxHealth;
            originalScale = transform.localScale;

            // Rigidbody constraints setup
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Create the floating health bar
            CreateFloatingHealthBar();

            // Find the player in the scene
            FindPlayer();
        }

        private void Update()
        {
            if (isDead) return;

            // Make sure we have a player target reference
            if (playerTarget == null)
            {
                FindPlayer();
                UpdateAnimations(false, false);
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            bool isChasing = false;
            bool isAttacking = false;

            // If player is dead, idle
            if (playerController != null && playerController.CurrentHealth <= 0)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                UpdateAnimations(false, false);
                return;
            }

            if (!isHurt)
            {
                if (distanceToPlayer <= attackRadius)
                {
                    // Within Attack Range
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    isAttacking = true;

                    // Face the player
                    FaceTarget(playerTarget.position);

                    // Perform Attack if cooldown is finished
                    if (Time.time >= lastAttackTime + attackCooldown)
                    {
                        StartCoroutine(PerformAttackRoutine());
                    }
                }
                else if (distanceToPlayer <= detectionRadius)
                {
                    // Within Chase Range
                    isChasing = true;
                    ChasePlayer();
                }
                else
                {
                    // Idle (outside senses)
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
            }

            UpdateAnimations(isChasing, isAttacking);
            UpdateHealthBarPosition();
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                playerController = playerObj.GetComponent<WarriorController>();
            }
        }

        private void ChasePlayer()
        {
            float directionX = playerTarget.position.x > transform.position.x ? 1f : -1f;
            rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);

            FaceTarget(playerTarget.position);
        }

        private void FaceTarget(Vector2 targetPos)
        {
            if (targetPos.x > transform.position.x && !isFacingRight)
            {
                Flip();
            }
            else if (targetPos.x < transform.position.x && isFacingRight)
            {
                Flip();
            }
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x = isFacingRight ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
            transform.localScale = scale;
        }

        private IEnumerator PerformAttackRoutine()
        {
            lastAttackTime = Time.time;

            if (animator != null)
            {
                animator.Play("Attack");
            }

            // Yield briefly to sync with physical sword hit moment (common in anims)
            yield return new WaitForSeconds(0.2f);

            // Double check player is still in range before damaging
            if (!isDead && playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= attackRadius + 0.3f)
            {
                if (playerController != null)
                {
                    playerController.TakeDamage(damage, transform.position);
                }
            }
        }

        private void UpdateAnimations(bool isChasing, bool isAttacking)
        {
            if (animator == null) return;

            if (isAttacking)
            {
                // Attack animation is triggered directly in the coroutine via Play()
                return;
            }

            if (isChasing)
            {
                animator.Play("Run");
            }
            else
            {
                animator.Play("Idle");
            }
        }

        #region IDamageable Implementation

        public void TakeDamage(float damageAmount, Vector2 damageSourcePosition)
        {
            if (isDead) return;

            currentHealth = Mathf.Max(currentHealth - damageAmount, 0f);

            // Update Floating Health Bar visibility & ratio
            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            isHurt = true;
            if (animator != null)
            {
                animator.Play("Hurt");
            }

            // Apply slight knockback away from attack
            float knockbackDir = transform.position.x > damageSourcePosition.x ? 1f : -1f;
            rb.velocity = new Vector2(knockbackDir * knockbackForceX, knockbackForceY);

            StartCoroutine(HurtRecoveryRoutine());
        }

        private IEnumerator HurtRecoveryRoutine()
        {
            yield return new WaitForSeconds(0.35f);
            isHurt = false;
        }

        private void Die()
        {
            isDead = true;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            // Turn off collider so player can walk through dead enemy
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Disable Health Bar
            if (healthBarCanvasObj != null)
            {
                healthBarCanvasObj.SetActive(false);
            }

            if (animator != null)
            {
                animator.Play("Death");
                // Destroy after a short delay to allow animation to play
                Destroy(gameObject, 2f);
            }
            else
            {
                // Fallback: fade out and destroy if no animator
                StartCoroutine(FadeOutAndDestroyRoutine());
            }

            OnEnemyDeath?.Invoke(this);
            Debug.Log($"[EnemyAI] {gameObject.name} has died.");
        }

        private IEnumerator FadeOutAndDestroyRoutine()
        {
            float elapsed = 0f;
            Color startCol = spriteRenderer.color;

            while (elapsed < 1.5f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / 1.5f);
                spriteRenderer.color = new Color(startCol.r, startCol.g, startCol.b, alpha);
                yield return null;
            }

            Destroy(gameObject);
        }

        #endregion

        #region Programmatic Floating Health Bar

        private void CreateFloatingHealthBar()
        {
            // 1. Create World Space Canvas Object
            healthBarCanvasObj = new GameObject("FloatingHealthBarCanvas", typeof(Canvas));
            Canvas canvas = healthBarCanvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            // Unparent from scales, but maintain position tracking in Update
            healthBarCanvasObj.transform.SetParent(null);
            healthBarCanvasObj.transform.localScale = Vector3.one;

            // 2. Background container
            GameObject bgObj = new GameObject("Background", typeof(Image));
            bgObj.transform.SetParent(healthBarCanvasObj.transform);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(healthBarWidth, healthBarHeight);
            bgRect.localPosition = Vector3.zero;
            bgRect.localScale = Vector3.one;
            bgObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark Charcoal

            // 3. Health fill container
            healthBarFillObj = new GameObject("Fill", typeof(Image));
            healthBarFillObj.transform.SetParent(healthBarCanvasObj.transform);
            RectTransform fillRect = healthBarFillObj.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(healthBarWidth - 0.04f, healthBarHeight - 0.04f); // slight border padding
            fillRect.localPosition = Vector3.zero;
            fillRect.localScale = Vector3.one;
            
            // Align anchor to the Left so scaling down looks like a draining bar
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = new Vector2(-healthBarWidth / 2f + 0.02f, 0f);

            healthBarFillObj.GetComponent<Image>().color = new Color(0.85f, 0.15f, 0.15f, 1f); // Rich Red

            // Hide initially until damaged (health < 100%)
            healthBarCanvasObj.SetActive(false);
        }

        private void UpdateHealthBar()
        {
            if (isDead || healthBarCanvasObj == null) return;

            float healthPercentage = currentHealth / maxHealth;

            if (healthPercentage < 1f)
            {
                // Ensure it is visible now that they took damage
                healthBarCanvasObj.SetActive(true);

                // Dynamically adjust scale on the X axis to represent draining health
                if (healthBarFillObj != null)
                {
                    RectTransform fillRect = healthBarFillObj.GetComponent<RectTransform>();
                    fillRect.localScale = new Vector3(healthPercentage, 1f, 1f);
                }
            }
            else
            {
                healthBarCanvasObj.SetActive(false);
            }
        }

        private void UpdateHealthBarPosition()
        {
            if (healthBarCanvasObj == null) return;

            // Anchor the world space Canvas above the enemy's current transform position
            healthBarCanvasObj.transform.position = transform.position + Vector3.up * healthBarHeightOffset;
        }

        private void OnDestroy()
        {
            // Clean up programmatic canvas when the enemy is destroyed
            if (healthBarCanvasObj != null)
            {
                Destroy(healthBarCanvasObj);
            }
        }

        #endregion

        #region Editor Visualizer

        private void OnDrawGizmosSelected()
        {
            // 1. Draw detection range (Yellow)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // 2. Draw attack range (Red)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }

        #endregion
    }
}
