using System;
using UnityEngine;

namespace ChickenShooter
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "Chicken Shooter/Weapon Data")]
    public class WeaponDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Laser";
        public int powerLevel = 1;
        public Sprite icon;

        [Header("Projectile")]
        public GameObject projectilePrefab;
        public int damage = 1;
        public float projectileSpeed = 14f;

        [Header("Firing")]
        [Min(0.01f)]
        public float fireInterval = 0.22f;
        public FirePattern pattern = FirePattern.Single;
        public int projectileCount = 1;
        public float spreadAngle = 12f;

        [Tooltip("Local muzzle offsets relative to the ship.")]
        public Vector2[] muzzleOffsets = { new Vector2(0f, 0.55f) };
    }
}
