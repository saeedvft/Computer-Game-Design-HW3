using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Anything that can receive damage. Kept deliberately small so bullets,
    /// eggs, player and bosses can all use the same contract.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount, GameObject source = null);
    }

    /// <summary>
    /// Targets that react to the screen-clearing bomb. Normal enemies die
    /// instantly; bosses only lose a percentage of their maximum health.
    /// </summary>
    public interface IBombTarget : IDamageable
    {
        void ApplyBombDamage(float fractionOfMaxHealth);
    }
}
