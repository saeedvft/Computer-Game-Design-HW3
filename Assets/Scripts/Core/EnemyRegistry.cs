using System.Collections.Generic;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Tracks live enemies and eggs. Round completion and the screen-clearing
    /// bomb use this registry instead of expensive scene scans.
    /// </summary>
    public static class EnemyRegistry
    {
        private static readonly List<EnemyUnit> Enemies = new List<EnemyUnit>();
        private static readonly List<EggProjectile> Eggs = new List<EggProjectile>();

        public static int EnemyCount => Enemies.Count;
        public static int EggCount => Eggs.Count;

        public static void RegisterEnemy(EnemyUnit enemy)
        {
            if (enemy != null && !Enemies.Contains(enemy))
            {
                Enemies.Add(enemy);
            }
        }

        public static void UnregisterEnemy(EnemyUnit enemy)
        {
            Enemies.Remove(enemy);
        }

        public static void RegisterEgg(EggProjectile egg)
        {
            if (egg != null && !Eggs.Contains(egg))
            {
                Eggs.Add(egg);
            }
        }

        public static void UnregisterEgg(EggProjectile egg)
        {
            Eggs.Remove(egg);
        }

        public static void ApplyBomb()
        {
            EnemyUnit[] enemies = Enemies.ToArray();
            foreach (EnemyUnit enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.ApplyBombDamage(1f);
                }
            }

            EggProjectile[] eggs = Eggs.ToArray();
            foreach (EggProjectile egg in eggs)
            {
                if (egg != null)
                {
                    egg.TakeDamage(9999);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Enemies.Clear();
            Eggs.Clear();
        }
    }
}
