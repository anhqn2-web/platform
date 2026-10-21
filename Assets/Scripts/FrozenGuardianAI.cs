using UnityEngine;
using System.Collections;

namespace WarriorPlatformer
{
    public class FrozenGuardianAI : EnemyAI
    {
        [Header("Boss Attack Ranges")]
        [SerializeField] private float groundSlamRadius = 4f;
        [SerializeField] private float groundSlamDamage = 30f;
        [SerializeField] private float iceProjectileRange = 10f;
        [SerializeField] private float chargeRange = 8f;

        [Header("Boss Cooldowns")]
        [SerializeField] private float groundSlamCooldown = 5f;
        [SerializeField] private float iceProjectileCooldown = 4f;
        [SerializeField] private float chargeCooldown = 6f;

        [Header("Prefabs & Effects")]
        [SerializeField] private GameObject iceProjectilePrefab;
        [SerializeField] private GameObject groundSlamEffect;

        [Header("Charge Settings")]
        [SerializeField] private float chargeSpeed = 12f;
        [SerializeField] private float chargeDuration = 1f;

        private float nextGroundSlamTime;
        private float nextProjectileTime;
        private float nextChargeTime;

        private bool isPerformingSpecialAttack = false;

        protected override void Update()
        {
            if (isDead) return;

            if (playerTarget == null)
            {
                FindPlayer();
                UpdateAnimations(false, false);
                return;
            }

            if (playerController != null && playerController.CurrentHealth <= 0)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                UpdateAnimations(false, false);
                return;
            }

            if (isHurt || isPerformingSpecialAttack)
            {
                UpdateHealthBarPosition();
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer <= groundSlamRadius && Time.time >= nextGroundSlamTime)
            {
                StartCoroutine(GroundSlamRoutine());
            }
            else if (distanceToPlayer <= chargeRange && Time.time >= nextChargeTime)
            {
                StartCoroutine(ChargeRoutine());
            }
            else if (distanceToPlayer <= iceProjectileRange && Time.time >= nextProjectileTime)
            {
                StartCoroutine(ProjectileRoutine());
            }
            else if (distanceToPlayer <= attackRadius)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartCoroutine(PerformAttackRoutine());
                }
                rb.velocity = new Vector2(0f, rb.velocity.y);
                UpdateAnimations(false, true);
            }
            else if (distanceToPlayer <= detectionRadius)
            {
                ChasePlayer();
                UpdateAnimations(true, false);
            }
            else
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                UpdateAnimations(false, false);
            }

            UpdateHealthBarPosition();
        }

        private IEnumerator GroundSlamRoutine()
        {
            isPerformingSpecialAttack = true;
            nextGroundSlamTime = Time.time + groundSlamCooldown;
            
            rb.velocity = Vector2.zero;
            if (animator != null) animator.Play("Attack_Slam");

            yield return new WaitForSeconds(0.5f);

            if (groundSlamEffect != null)
            {
                Instantiate(groundSlamEffect, transform.position, Quaternion.identity);
            }

            if (Vector2.Distance(transform.position, playerTarget.position) <= groundSlamRadius)
            {
                playerController.TakeDamage(groundSlamDamage, transform.position);
            }

            yield return new WaitForSeconds(0.5f);
            isPerformingSpecialAttack = false;
        }

        private IEnumerator ProjectileRoutine()
        {
            isPerformingSpecialAttack = true;
            nextProjectileTime = Time.time + iceProjectileCooldown;

            rb.velocity = Vector2.zero;
            FaceTarget(playerTarget.position);
            if (animator != null) animator.Play("Attack_Range");

            yield return new WaitForSeconds(0.4f);

            if (iceProjectilePrefab != null)
            {
                GameObject proj = Instantiate(iceProjectilePrefab, transform.position + Vector3.up, Quaternion.identity);
                Vector2 dir = (playerTarget.position - (transform.position + Vector3.up)).normalized;
                proj.GetComponent<Rigidbody2D>().velocity = dir * 8f;
            }

            yield return new WaitForSeconds(0.4f);
            isPerformingSpecialAttack = false;
        }

        private IEnumerator ChargeRoutine()
        {
            isPerformingSpecialAttack = true;
            nextChargeTime = Time.time + chargeCooldown;

            FaceTarget(playerTarget.position);
            if (animator != null) animator.Play("Attack_Charge_Start");

            yield return new WaitForSeconds(0.5f);

            float chargeStartTime = Time.time;
            float chargeDir = isFacingRight ? 1f : -1f;

            if (animator != null) animator.Play("Run");

            while (Time.time < chargeStartTime + chargeDuration)
            {
                rb.velocity = new Vector2(chargeDir * chargeSpeed, rb.velocity.y);
                
                if (Vector2.Distance(transform.position, playerTarget.position) < 1.5f)
                {
                    playerController.TakeDamage(damage * 1.5f, transform.position);
                    break;
                }
                yield return null;
            }

            rb.velocity = Vector2.zero;
            yield return new WaitForSeconds(1.0f);
            isPerformingSpecialAttack = false;
        }
    }
}