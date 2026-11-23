using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Secondary super missile: consumes one bomb and clears every normal
    /// enemy and egg through the EnemyRegistry.
    /// </summary>
    public class BombSystem : MonoBehaviour
    {
        public PlayerInputReader input;

        private void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            if (input == null || !input.BombWasPressed)
            {
                return;
            }

            if (GameManager.Instance.State.lives <= 0)
            {
                return;
            }

            if (!GameManager.Instance.TryConsumeBomb())
            {
                Debug.Log("[BombSystem] No bombs left.");
                return;
            }

            EnemyRegistry.ApplyBomb();
            GameEvents.RaiseBombUsed();
            Debug.Log("[BombSystem] Screen-clearing bomb fired.");
        }
    }
}
