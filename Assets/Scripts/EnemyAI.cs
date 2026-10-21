using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace WarriorPlatformer
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class EnemyAI : MonoBehaviour, IDamageable
    {
        [Header("Stats Settings")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 3f;

        [Header("AI Senses")]
        [Tooltip("The range at which the enemy detects the player and starts chasing.")]
        [SerializeField] protected float detectionRadius = 6f;
        [Tooltip("The range at which the enemy stops and attacks the player.")]
        [SerializeField] protected float attackRadius = 1.2f;

        [Header("Combat Settings")]
        [SerializeField] protected float damage = 15f;
        [SerializeField] protected float attackCooldown = 1.5f;
        [SerializeField] protected float knockbackForceX = 4f;
        [SerializeField] protected float knockbackForceY = 3f;

        [Header("UI & Visual Settings")]
        [Tooltip("Height offset above the enemy's center to display the floating health bar.")]
        [SerializeField] protected float healthBarHeightOffset = 1.5f;
        [Tooltip("Width of the floating health bar.")]
        [SerializeField] protected float healthBarWidth = 1.2f;
        [Tooltip("Height of the floating health bar.")]
        [SerializeField] protected float healthBarHeight = 0.15f;

        // Components (Dùng protected để lớp con Kế thừa truy cập)
        protected Rigidbody2D rb;
        protected SpriteRenderer spriteRenderer;
        protected Animator animator;

        // State Tracking
        protected Transform playerTarget;
        protected WarriorController playerController;
        protected float lastAttackTime;
        protected bool isDead;
        protected bool isFacingRight = true;
        protected bool isHurt;

        // Level Management Event
        public static System.Action<EnemyAI> OnEnemyDeath;

        // Programmatic UI Elements
        protected GameObject healthBarCanvasObj;
        protected GameObject healthBarFillObj;
        protected Vector3 originalScale;

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            currentHealth = maxHealth;
            originalScale = transform.localScale;

            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            CreateFloatingHealthBar();
            FindPlayer();
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (playerTarget == null)
            {
                FindPlayer();
                UpdateAnimations(false, false);
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            bool isChasing = false;
            bool isAttacking = false;

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
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    isAttacking = true;

                    FaceTarget(playerTarget.position);

                    if (Time.time >= lastAttackTime + attackCooldown)
                    {
                        StartCoroutine(PerformAttackRoutine());
                    }
                }
                else if (distanceToPlayer <= detectionRadius)
                {
                    isChasing = true;
                    ChasePlayer();
                }
                else
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
            }

            UpdateAnimations(isChasing, isAttacking);
            UpdateHealthBarPosition();
        }

        protected void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                playerController = playerObj.GetComponent<WarriorController>();
            }
        }

        protected void ChasePlayer()
        {
            float directionX = playerTarget.position.x > transform.position.x ? 1f : -1f;
            rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);

            FaceTarget(playerTarget.position);
        }

        protected void FaceTarget(Vector2 targetPos)
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

        protected void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x = isFacingRight ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
            transform.localScale = scale;
        }

        protected IEnumerator PerformAttackRoutine()
        {
            lastAttackTime = Time.time;

            if (animator != null)
            {
                animator.Play("Attack");
            }

            yield return new WaitForSeconds(0.2f);

            if (!isDead && playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= attackRadius + 0.3f)
            {
                if (playerController != null)
                {
                    playerController.TakeDamage(damage, transform.position);
                }
            }
        }

        protected void UpdateAnimations(bool isChasing, bool isAttacking)
        {
            if (animator == null) return;

            if (isAttacking)
            {
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

        public virtual void TakeDamage(float damageAmount, Vector2 damageSourcePosition)
        {
            if (isDead) return;

            currentHealth = Mathf.Max(currentHealth - damageAmount, 0f);

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

            float knockbackDir = transform.position.x > damageSourcePosition.x ? 1f : -1f;
            rb.velocity = new Vector2(knockbackDir * knockbackForceX, knockbackForceY);

            StartCoroutine(HurtRecoveryRoutine());
        }

        protected IEnumerator HurtRecoveryRoutine()
        {
            yield return new WaitForSeconds(0.35f);
            isHurt = false;
        }

        protected virtual void Die()
        {
            isDead = true;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            if (healthBarCanvasObj != null)
            {
                healthBarCanvasObj.SetActive(false);
            }

            if (animator != null)
            {
                animator.Play("Death");
                Destroy(gameObject, 2f);
            }
            else
            {
                StartCoroutine(FadeOutAndDestroyRoutine());
            }

            OnEnemyDeath?.Invoke(this);
            Debug.Log($"[EnemyAI] {gameObject.name} has died.");
        }

        protected IEnumerator FadeOutAndDestroyRoutine()
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

        protected void CreateFloatingHealthBar()
        {
            healthBarCanvasObj = new GameObject("FloatingHealthBarCanvas", typeof(Canvas));
            Canvas canvas = healthBarCanvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            healthBarCanvasObj.transform.SetParent(null);
            healthBarCanvasObj.transform.localScale = Vector3.one;

            GameObject bgObj = new GameObject("Background", typeof(Image));
            bgObj.transform.SetParent(healthBarCanvasObj.transform);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(healthBarWidth, healthBarHeight);
            bgRect.localPosition = Vector3.zero;
            bgRect.localScale = Vector3.one;
            bgObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            healthBarFillObj = new GameObject("Fill", typeof(Image));
            healthBarFillObj.transform.SetParent(healthBarCanvasObj.transform);
            RectTransform fillRect = healthBarFillObj.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(healthBarWidth - 0.04f, healthBarHeight - 0.04f);
            fillRect.localPosition = new Vector3(0f, 0f, -0.1f);
            fillRect.localScale = Vector3.one;
            
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition3D = new Vector3(-healthBarWidth / 2f + 0.02f, 0f, -0.1f);

            healthBarFillObj.GetComponent<Image>().color = new Color(0.85f, 0.15f, 0.15f, 1f);

            healthBarCanvasObj.SetActive(false);
        }

        protected void UpdateHealthBar()
        {
            if (isDead || healthBarCanvasObj == null) return;

            float healthPercentage = currentHealth / maxHealth;

            if (healthPercentage < 1f)
            {
                healthBarCanvasObj.SetActive(true);

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

        protected void UpdateHealthBarPosition()
        {
            if (healthBarCanvasObj == null) return;

            healthBarCanvasObj.transform.position = transform.position + Vector3.up * healthBarHeightOffset;
        }

        protected void OnDestroy()
        {
            if (healthBarCanvasObj != null)
            {
                Destroy(healthBarCanvasObj);
            }
        }

        #endregion

        #region Editor Visualizer

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }

        #endregion
    }
}