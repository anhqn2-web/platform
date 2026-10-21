using UnityEngine;

namespace WarriorPlatformer
{
    public class BossProjectile : MonoBehaviour
    {
        [SerializeField] private float damage = 20f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private GameObject hitEffect;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                WarriorController player = other.GetComponent<WarriorController>();
                if (player != null)
                {
                    player.TakeDamage(damage, transform.position);
                }
                Explode();
            }
            else if (((1 << other.gameObject.layer) & LayerMask.GetMask("Ground")) != 0)
            {
                Explode();
            }
        }

        private void Explode()
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
