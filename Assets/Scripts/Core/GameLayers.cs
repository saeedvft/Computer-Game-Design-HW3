using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Layer names and the 2D collision matrix. Configured once at startup so
    /// the project works even if a fresh checkout has default physics settings.
    /// </summary>
    public static class GameLayers
    {
        public const int Player = 6;
        public const int PlayerProjectile = 7;
        public const int Enemy = 8;
        public const int EnemyProjectile = 9;
        public const int Pickup = 10;

        public static readonly int[] All = { Player, PlayerProjectile, Enemy, EnemyProjectile, Pickup };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ConfigureCollisionMatrix()
        {
            // Start with every pair between our layers disabled.
            for (int i = 0; i < All.Length; i++)
            {
                for (int j = 0; j < All.Length; j++)
                {
                    Physics2D.IgnoreLayerCollision(All[i], All[j], true);
                }
            }

            // Only these interactions are allowed.
            Enable(Player, Enemy);
            Enable(Player, EnemyProjectile);
            Enable(Player, Pickup);
            Enable(PlayerProjectile, Enemy);
            Enable(PlayerProjectile, EnemyProjectile);
        }

        private static void Enable(int a, int b)
        {
            Physics2D.IgnoreLayerCollision(a, b, false);
            Physics2D.IgnoreLayerCollision(b, a, false);
        }
    }
}
