using UnityEngine;

namespace WarriorPlatformer
{
    public class SpikeTrap : MonoBehaviour
    {
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private float damageInterval = 1f;

        private float nextDamageTime;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (Time.time >= nextDamageTime)
                {
                    WarriorController player = other.GetComponent<WarriorController>();
                    if (player != null)
                    {
                        // Truyền lượng sát thương và vị trí của bẫy gai
                        player.TakeDamage(damageAmount, transform.position);
                        nextDamageTime = Time.time + damageInterval;
                    }
                }
            }
        }
    }
}