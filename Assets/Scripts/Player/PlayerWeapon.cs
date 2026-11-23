using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Data-driven primary weapon. Reads the current power level from the
    /// GameManager and applies the matching WeaponDataSO fire pattern.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("References")]
        public PlayerInputReader input;
        public WeaponDataSO[] weaponLevels = new WeaponDataSO[3];

        private float _nextFireTime;

        private void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            if (input == null || !input.FireHeld || Time.time < _nextFireTime)
            {
                return;
            }

            if (GameManager.Instance.State.lives <= 0)
            {
                return;
            }

            WeaponDataSO weapon = GetCurrentWeapon();
            if (weapon == null || weapon.projectilePrefab == null)
            {
                return;
            }

            Fire(weapon);
        }

        public WeaponDataSO GetCurrentWeapon()
        {
            if (weaponLevels == null || weaponLevels.Length == 0)
            {
                return null;
            }

            int index = Mathf.Clamp(GameManager.Instance.State.weaponPower - 1, 0, weaponLevels.Length - 1);
            return weaponLevels[index];
        }

        private void Fire(WeaponDataSO weapon)
        {
            _nextFireTime = Time.time + weapon.fireInterval;

            int count = Mathf.Max(1, weapon.projectileCount);
            float spread = weapon.spreadAngle;

            for (int i = 0; i < count; i++)
            {
                float angle = 0f;
                if (weapon.pattern == FirePattern.Spread && count > 1)
                {
                    angle = Mathf.Lerp(-spread, spread, i / (float)(count - 1));
                }

                Vector2 offset = GetMuzzleOffset(weapon, i, count);
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
                Vector2 direction = rotation * Vector2.up;

                VfxManager.PlayMuzzle(transform.position + (Vector3)offset);

                Projectile projectile = GameManager.Instance.Pool.Spawn<Projectile>(
                    weapon.projectilePrefab,
                    transform.position + (Vector3)offset,
                    rotation);

                if (projectile != null)
                {
                    projectile.Initialize(weapon.damage, weapon.projectileSpeed, direction, true);
                }
            }
        }

        private static Vector2 GetMuzzleOffset(WeaponDataSO weapon, int index, int count)
        {
            if (weapon.muzzleOffsets != null && index < weapon.muzzleOffsets.Length)
            {
                return weapon.muzzleOffsets[index];
            }

            float centered = index - (count - 1) * 0.5f;
            switch (weapon.pattern)
            {
                case FirePattern.Parallel:
                    return new Vector2(centered * 0.24f, 0.5f);
                case FirePattern.Spread:
                    return new Vector2(centered * 0.28f, 0.5f - Mathf.Abs(centered) * 0.05f);
                default:
                    return new Vector2(0f, 0.55f);
            }
        }
    }
}
