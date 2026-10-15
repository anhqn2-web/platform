using UnityEngine;

namespace WarriorPlatformer
{
    /// <summary>
    /// Interface for any object or enemy that can take damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies damage to the object.
        /// </summary>
        /// <param name="damage">Amount of damage to take.</param>
        /// <param name="damageSourcePosition">Position of the damage dealer (useful for knockback calculations).</param>
        void TakeDamage(float damage, Vector2 damageSourcePosition);
    }
}
