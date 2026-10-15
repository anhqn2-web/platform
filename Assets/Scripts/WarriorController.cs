using UnityEngine;
using System.Collections;

namespace WarriorPlatformer
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
    public class WarriorController : MonoBehaviour
    {
        [Header("Health & Stamina")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina;
        [SerializeField] private float staminaRegenRate = 25f;

        [Header("Stamina Costs")]
        [SerializeField] private float jumpStaminaCost = 10f;
        [SerializeField] private float attackStaminaCost = 15f;
        [SerializeField] private float slideStaminaCost = 20f;

        [Header("Movement Settings")]
        [Tooltip("Standard walking/running speed.")]
        [SerializeField] private float moveSpeed = 8f;
        [Tooltip("The vertical force applied during a jump.")]
        [SerializeField] private float jumpForce = 12f;
        [Tooltip("Scale multiplier for gravity when falling. High values make the jump feel less floaty.")]
        [SerializeField] private float fallGravityMultiplier = 3f;
        [Tooltip("Scale multiplier for gravity when the jump button is released early.")]
        [SerializeField] private float lowJumpGravityMultiplier = 2.5f;

        [Header("Ground Check Settings")]
        [Tooltip("LayerMask containing ground/platform geometry.")]
        [SerializeField] private LayerMask groundLayer;
        [Tooltip("How far below the collider to check for the ground.")]
        [SerializeField] private float groundCheckExtraDistance = 0.05f;

        [Header("Coyote Time & Buffer Settings")]
        [Tooltip("How long (seconds) after leaving a ledge a player can still jump.")]
        [SerializeField] private float coyoteTime = 0.15f;
        [Tooltip("How long (seconds) before landing a jump input is cached.")]
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("Combat Settings")]
        [Tooltip("Damage dealt by standard attack.")]
        [SerializeField] private float attackDamage = 25f;
        [Tooltip("Amount of health restored by healing.")]
        [SerializeField] private float healAmount = 50f;
        [Tooltip("Cooldown between heals (seconds).")]
        [SerializeField] private float healCooldown = 10f;
        [Tooltip("Number of attacks allowed per second.")]
        [SerializeField] private float attackRate = 2f;
        [Tooltip("Radius of the attack circle.")]
        [SerializeField] private float attackRange = 0.8f;
        [Tooltip("Offset of the attack point relative to the character center.")]
        [SerializeField] private Vector2 attackOffset = new Vector2(0.6f, 0f);
        [Tooltip("Should horizontal movement be disabled during the attack swing?")]
        [SerializeField] private bool stopMovementDuringAttack = true;
        [Tooltip("Forward push force during attack (creates a satisfying lunge).")]
        [SerializeField] private float attackLungeForce = 4f;
        [Tooltip("Time delay (seconds) from attack start to damage application (matches visual sword swing).")]
        [SerializeField] private float attackDamageDelay = 0.15f;

        [Header("Crouch Settings")]
        [Tooltip("Speed multiplier while crouching.")]
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;
        [Tooltip("How much to shrink the collider height when crouching.")]
        [SerializeField] private float crouchColliderHeightMultiplier = 0.5f;

        [Header("Slide & Dash Settings")]
        [Tooltip("Is the slide/dash mechanic enabled?")]
        [SerializeField] private bool isSlideEnabled = true;
        [Tooltip("Horizontal speed during a slide/dash.")]
        [SerializeField] private float slideSpeed = 14f;
        [Tooltip("Duration of the slide (seconds).")]
        [SerializeField] private float slideDuration = 0.4f;
        [Tooltip("Cooldown between slides (seconds).")]
        [SerializeField] private float slideCooldown = 1.0f;

        [Header("Wall Interaction Settings")]
        [Tooltip("Enable sliding on walls?")]
        [SerializeField] private bool isWallSlideEnabled = true;
        [Tooltip("Max falling speed when sliding down a wall.")]
        [SerializeField] private float wallSlideSpeed = 2f;
        [Tooltip("Detection distance for wall checks.")]
        [SerializeField] private float wallCheckDistance = 0.1f;
        [Tooltip("Force pushed away from the wall during a wall jump.")]
        [SerializeField] private Vector2 wallJumpForce = new Vector2(8f, 12f);
        [Tooltip("Duration (seconds) that wall jump horizontal control is locked.")]
        [SerializeField] private float wallJumpControlLockTime = 0.15f;

        // Public getters for UI
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float MaxStamina => maxStamina;
        public float CurrentStamina => currentStamina;
        public float HealCooldownRemaining => Mathf.Max(0, (lastHealTime + healCooldown) - Time.time);
        public float HealCooldownRatio => Mathf.Clamp01(HealCooldownRemaining / healCooldown);

        // Components
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Collider2D mainCollider;

        // Input & State Tracking
        private float horizontalInput;
        private bool isFacingRight = true;
        private bool isGrounded;
        private bool isCrouching;
        private bool isSliding;
        private bool isWallSliding;
        private bool isAttacking;
        private bool isHurt;
        private bool isDead;

        // Timers & Counters
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float nextAttackTime;
        private float lastSlideTime = -999f;
        private float lastHealTime = -999f;
        private float wallJumpControlTimeLeft;
        private float originalGravityScale;

        // Collider Sizing
        private Vector2 originalColliderSize;
        private Vector2 originalColliderOffset;
        private bool isBoxCollider;
        private bool isCapsuleCollider;

        // Animator State Strings
        private const string ANIM_IDLE = "Idle";
        private const string ANIM_RUN = "Run";
        private const string ANIM_JUMP = "jump"; // Note: lower case 'j' in Warrior free set
        private const string ANIM_FALL = "Fall";
        private const string ANIM_CROUCH = "Croush"; // Note: spelling in asset pack
        private const string ANIM_SLIDE = "Slide";
        private const string ANIM_DASH = "Dash";
        private const string ANIM_ATTACK = "Dash-Attack NoDust";
        private const string ANIM_HURT = "Hurt";
        private const string ANIM_DEATH = "Death";
        private const string ANIM_WALL_SLIDE = "Wall-Slide";

        private string currentAnimationState;

        private void Awake()
        {
            // Auto-tag Player to ensure Enemy AI can find it with zero editor setup
            gameObject.tag = "Player";
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            mainCollider = GetComponent<Collider2D>();

            originalGravityScale = rb.gravityScale;

            currentHealth = maxHealth;
            currentStamina = maxStamina;

            // Cache collider dimensions to dynamically shrink during crouching
            if (mainCollider is BoxCollider2D box)
            {
                isBoxCollider = true;
                originalColliderSize = box.size;
                originalColliderOffset = box.offset;
            }
            else if (mainCollider is CapsuleCollider2D capsule)
            {
                isCapsuleCollider = true;
                originalColliderSize = capsule.size;
                originalColliderOffset = capsule.offset;
            }
        }

        private void Update()
        {
            if (isDead) return;

            // Gather Input
            horizontalInput = Input.GetAxisRaw("Horizontal");

            // Timers & Stats Regen
            UpdateTimers();
            RegenerateStats();

            // Ground & Wall Checks
            CheckSurroundings();

            // Action Triggers
            HandleInputs();

            // Handle Animation Selection
            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            if (isSliding)
            {
                PerformSlideMovement();
            }
            else if (isWallSliding)
            {
                PerformWallSlideMovement();
            }
            else
            {
                PerformStandardMovement();
            }

            ApplyCustomPhysics();
        }

        #region Inputs & State Handling

        private void UpdateTimers()
        {
            // Coyote Time
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // Jump Buffer
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // Wall Jump Control Lock
            if (wallJumpControlTimeLeft > 0)
            {
                wallJumpControlTimeLeft -= Time.deltaTime;
            }
        }

        private void RegenerateStats()
        {
            // Stamina regeneration (only when not sliding or actively attacking)
            if (!isSliding && !isAttacking)
            {
                currentStamina = Mathf.Clamp(currentStamina + staminaRegenRate * Time.deltaTime, 0f, maxStamina);
            }
        }

        private void CheckSurroundings()
        {
            if (mainCollider == null) return;

            // 1. Bulletproof Ground Check using a BoxCast downwards from collider base
            Bounds bounds = mainCollider.bounds;
            Vector2 boxOrigin = new Vector2(bounds.center.x, bounds.min.y);
            Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, 0.05f);

            RaycastHit2D groundHit = Physics2D.BoxCast(boxOrigin, boxSize, 0f, Vector2.down, groundCheckExtraDistance, groundLayer);
            isGrounded = groundHit.collider != null;

            // 2. Wall Check using a lateral check in the direction facing
            isWallSliding = false;
            if (isWallSlideEnabled && !isGrounded)
            {
                Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
                RaycastHit2D wallHit = Physics2D.Raycast(bounds.center, direction, (bounds.size.x / 2f) + wallCheckDistance, groundLayer);

                // Slide down if hugging wall and pressing move button towards it
                if (wallHit.collider != null && ((horizontalInput > 0 && isFacingRight) || (horizontalInput < 0 && !isFacingRight)))
                {
                    isWallSliding = true;
                }
            }
        }

        private void HandleInputs()
        {
            // 1. Crouch Handling (Holding S or Down Arrow)
            bool wantToCrouch = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && isGrounded && !isSliding && !isAttacking;
            if (wantToCrouch != isCrouching)
            {
                SetCrouch(wantToCrouch);
            }

            // 2. Jump Input
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isCrouching && !isAttacking)
            {
                if (currentStamina >= jumpStaminaCost)
                {
                    Jump();
                }
            }
            // Wall Jump Input
            else if (jumpBufferCounter > 0f && isWallSliding)
            {
                if (currentStamina >= jumpStaminaCost)
                {
                    WallJump();
                }
            }

            // 3. Attack Input (Left Click or J Key)
            if ((Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.J)) && !isSliding)
            {
                if (Time.time >= nextAttackTime && currentStamina >= attackStaminaCost)
                {
                    StartCoroutine(AttackRoutine());
                    nextAttackTime = Time.time + (1f / attackRate);
                }
            }

            // 4. Slide/Dash Input (Left Shift)
            if (isSlideEnabled && Input.GetKeyDown(KeyCode.LeftShift) && !isSliding && !isAttacking && isGrounded)
            {
                if (Time.time >= lastSlideTime + slideCooldown && currentStamina >= slideStaminaCost)
                {
                    StartCoroutine(SlideRoutine());
                }
            }

            // 5. Heal Input (Q Key)
            if (Input.GetKeyDown(KeyCode.Q) && !isDead)
            {
                if (Time.time >= lastHealTime + healCooldown)
                {
                    Heal();
                }
            }
        }

        #endregion

        #region Movement Implementations

        private void PerformStandardMovement()
        {
            // Lock horizontal control briefly during a wall jump push
            if (wallJumpControlTimeLeft > 0) return;

            // Scale speed if crouching
            float currentSpeed = moveSpeed;
            if (isCrouching)
            {
                currentSpeed *= crouchSpeedMultiplier;
            }

            // Freeze horizontal motion if attacking (and option is toggled) - lunge force is handled separately
            if (isAttacking && stopMovementDuringAttack)
            {
                return;
            }

            rb.velocity = new Vector2(horizontalInput * currentSpeed, rb.velocity.y);

            // Flip character model
            if (horizontalInput > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && isFacingRight)
            {
                Flip();
            }
        }

        private void PerformWallSlideMovement()
        {
            // Slide down at a constant speed
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }

        private void PerformSlideMovement()
        {
            float direction = isFacingRight ? 1f : -1f;
            rb.velocity = new Vector2(direction * slideSpeed, rb.velocity.y);
        }

        private void ApplyCustomPhysics()
        {
            // Custom gravity scale adjustments for satisfying game-feel
            if (isSliding || isWallSliding)
            {
                rb.gravityScale = originalGravityScale;
                return;
            }

            if (rb.velocity.y < 0)
            {
                // Falling
                rb.gravityScale = originalGravityScale * fallGravityMultiplier;
            }
            else if (rb.velocity.y > 0 && !(Input.GetButton("Jump") || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)))
            {
                // Released Jump Key early
                rb.gravityScale = originalGravityScale * lowJumpGravityMultiplier;
            }
            else
            {
                rb.gravityScale = originalGravityScale;
            }
        }

        private void Jump()
        {
            currentStamina -= jumpStaminaCost;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        private void WallJump()
        {
            currentStamina -= jumpStaminaCost;
            float jumpDirection = isFacingRight ? -1f : 1f;
            rb.velocity = new Vector2(jumpDirection * wallJumpForce.x, wallJumpForce.y);

            // Flip direction immediately on wall kick
            Flip();

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            wallJumpControlTimeLeft = wallJumpControlLockTime;
        }

        private void SetCrouch(bool active)
        {
            isCrouching = active;

            if (isCrouching)
            {
                // Shrink collider
                if (isBoxCollider)
                {
                    BoxCollider2D box = (BoxCollider2D)mainCollider;
                    box.size = new Vector2(originalColliderSize.x, originalColliderSize.y * crouchColliderHeightMultiplier);
                    box.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - (originalColliderSize.y - box.size.y) / 2f);
                }
                else if (isCapsuleCollider)
                {
                    CapsuleCollider2D capsule = (CapsuleCollider2D)mainCollider;
                    capsule.size = new Vector2(originalColliderSize.x, originalColliderSize.y * crouchColliderHeightMultiplier);
                    capsule.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - (originalColliderSize.y - capsule.size.y) / 2f);
                }
            }
            else
            {
                // Restore collider size
                if (isBoxCollider)
                {
                    BoxCollider2D box = (BoxCollider2D)mainCollider;
                    box.size = originalColliderSize;
                    box.offset = originalColliderOffset;
                }
                else if (isCapsuleCollider)
                {
                    CapsuleCollider2D capsule = (CapsuleCollider2D)mainCollider;
                    capsule.size = originalColliderSize;
                    capsule.offset = originalColliderOffset;
                }
            }
        }

        private IEnumerator SlideRoutine()
        {
            isSliding = true;
            lastSlideTime = Time.time;
            currentStamina -= slideStaminaCost;

            // Trigger slide/dash animation
            ChangeAnimationState(ANIM_SLIDE);

            yield return new WaitForSeconds(slideDuration);

            isSliding = false;
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }

        private void Heal()
        {
            lastHealTime = Time.time;
            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
            
            float amountHealed = currentHealth - previousHealth;
            Debug.Log($"[WarriorController] Healed for {amountHealed} HP. Current Health: {currentHealth}/{maxHealth}");

            // Visual feedback: Play a quick effect or color flash
            StartCoroutine(HealVisualRoutine());
        }

        private IEnumerator HealVisualRoutine()
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }

        #endregion

        #region Combat Interactions

        private IEnumerator AttackRoutine()
        {
            isAttacking = true;
            currentStamina -= attackStaminaCost;

            // Attack Lunge: Apply a forward boost when swinging to give attacks real movement and punchiness
            float lungeDirection = isFacingRight ? 1f : -1f;
            rb.velocity = new Vector2(lungeDirection * attackLungeForce, rb.velocity.y);

            // Play the Attack animation state
            ChangeAnimationState(ANIM_ATTACK);

            // Wait until the specific swing moment to apply physics-based hit detection
            yield return new WaitForSeconds(attackDamageDelay);

            ApplyAttackHitbox();

            // Wait for remaining visual animation length (or simple hardcoded swing duration)
            yield return new WaitForSeconds(0.15f);

            isAttacking = false;
        }

        private void ApplyAttackHitbox()
        {
            // Calculate real-world attack origin in front of the player
            Vector3 facingOffset = new Vector3(isFacingRight ? attackOffset.x : -attackOffset.x, attackOffset.y, 0f);
            Vector2 hitboxCenter = transform.position + facingOffset;

            // Find all colliders within the attack radius
            Collider2D[] hitTargets = Physics2D.OverlapCircleAll(hitboxCenter, attackRange);

            bool hitAtLeastOneEnemy = false;

            foreach (Collider2D target in hitTargets)
            {
                // Prevent damaging self
                if (target.gameObject == gameObject) continue;

                // Check for IDamageable interface
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage, transform.position);
                    hitAtLeastOneEnemy = true;
                }
            }

            // Screen Shake Feedback: Shake the camera slightly on a successful hit! This makes combat feel extremely satisfying.
            if (hitAtLeastOneEnemy)
            {
                CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                if (cam != null)
                {
                    cam.TriggerShake(0.12f, 0.15f);
                }
            }
        }

        /// <summary>
        /// Call this method to apply damage to the player character.
        /// </summary>
        public void TakeDamage(float damage, Vector2 damageSourcePosition)
        {
            if (isDead) return;

            currentHealth = Mathf.Max(currentHealth - damage, 0f);
            
            // Trigger UI camera shake on taking damage too (heavier shake)
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.TriggerShake(0.2f, 0.25f);
            }

            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            isHurt = true;
            ChangeAnimationState(ANIM_HURT);

            // Calculate directional knockback away from damage source
            float forceDirection = transform.position.x > damageSourcePosition.x ? 1f : -1f;
            rb.velocity = new Vector2(forceDirection * 5f, 6f);

            // Simple coroutine/timer to return from Hurt state after a brief moment
            StartCoroutine(HurtRecoveryRoutine());
        }

        private IEnumerator HurtRecoveryRoutine()
        {
            yield return new WaitForSeconds(0.3f);
            isHurt = false;
        }

        /// <summary>
        /// Kills the player character.
        /// </summary>
        public void Die()
        {
            if (isDead) return;

            isDead = true;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // Turn off physics to freeze body on ground
            mainCollider.enabled = false; // Disable collisions

            ChangeAnimationState(ANIM_DEATH);
            Debug.Log("[WarriorController] Character has perished.");
        }

        #endregion

        #region Animation Controller

        private void UpdateAnimations()
        {
            // Priority list of override animation states
            if (isDead)
            {
                ChangeAnimationState(ANIM_DEATH);
                return;
            }
            if (isHurt)
            {
                ChangeAnimationState(ANIM_HURT);
                return;
            }
            if (isAttacking)
            {
                ChangeAnimationState(ANIM_ATTACK);
                return;
            }
            if (isSliding)
            {
                ChangeAnimationState(ANIM_SLIDE);
                return;
            }
            if (isWallSliding)
            {
                ChangeAnimationState(ANIM_WALL_SLIDE);
                return;
            }

            // Standard locational state machine
            if (isGrounded)
            {
                if (isCrouching)
                {
                    ChangeAnimationState(ANIM_CROUCH);
                }
                else if (Mathf.Abs(horizontalInput) > 0.05f)
                {
                    ChangeAnimationState(ANIM_RUN);
                }
                else
                {
                    ChangeAnimationState(ANIM_IDLE);
                }
            }
            else
            {
                // In Air
                if (rb.velocity.y > 0.1f)
                {
                    ChangeAnimationState(ANIM_JUMP);
                }
                else if (rb.velocity.y < -0.1f)
                {
                    ChangeAnimationState(ANIM_FALL);
                }
            }
        }

        /// <summary>
        /// Safe wrapper to trigger animator states.
        /// Prevents resetting/stuttering an animation if it's already actively playing.
        /// </summary>
        private void ChangeAnimationState(string newState)
        {
            if (currentAnimationState == newState) return;

            animator.Play(newState);
            currentAnimationState = newState;
        }

        #endregion

        #region Editor Visualization Helpers

        private void OnDrawGizmosSelected()
        {
            // 1. Draw Ground Check Area
            if (mainCollider != null)
            {
                Gizmos.color = Color.green;
                Bounds bounds = mainCollider.bounds;
                Vector3 boxOrigin = new Vector3(bounds.center.x, bounds.min.y - groundCheckExtraDistance, bounds.center.z);
                Vector3 boxSize = new Vector3(bounds.size.x * 0.9f, 0.05f, 0.1f);
                Gizmos.DrawWireCube(boxOrigin, boxSize);
            }

            // 2. Draw Attack Hitbox Range
            Gizmos.color = Color.red;
            Vector3 facingOffset = new Vector3(isFacingRight ? attackOffset.x : -attackOffset.x, attackOffset.y, 0f);
            Gizmos.DrawWireSphere(transform.position + facingOffset, attackRange);
        }

        #endregion
    }
}
