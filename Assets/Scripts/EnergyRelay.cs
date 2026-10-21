using UnityEngine;

namespace WarriorPlatformer
{
    public class EnergyRelay : PuzzleMechanism, IDamageable
    {
        [Header("Energy Relay Settings")]
        [SerializeField] private float resetTime = 0f; // 0 means it stays activated

        private float activatedTime;

        public void TakeDamage(float damage, Vector2 sourcePosition)
        {
            // Being hit by the player (or any damage source) activates it
            SetActivated(true);
            activatedTime = Time.time;
        }

        private void Update()
        {
            if (isActivated && resetTime > 0)
            {
                if (Time.time >= activatedTime + resetTime)
                {
                    SetActivated(false);
                }
            }
        }
    }
}
