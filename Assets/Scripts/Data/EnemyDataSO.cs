using System;
using UnityEngine;

namespace ChickenShooter
{
    [CreateAssetMenu(fileName = "Enemy_", menuName = "Chicken Shooter/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Basic Chicken";
        public Sprite sprite;
        public Color tint = Color.white;

        [Header("Combat")]
        public int maxHealth = 2;
        public int score = 10;
        public float moveSpeed = 3.2f;

        [Header("Eggs")]
        public Vector2 eggInterval = new Vector2(3.5f, 6.5f);
        public float eggSpeed = 4.5f;
        public int eggDamage = 1;
        public GameObject eggPrefab;

        [Header("Dive")]
        [Range(0f, 1f)] public float diveChance = 0.08f;
        public Vector2 diveInterval = new Vector2(5f, 9f);
        public float diveSpeed = 5.5f;

        [Header("Drops")]
        public GameObject foodPrefab;
        public GameObject giftPrefab;
        [Range(0f, 1f)] public float foodChance = 0.15f;
        [Range(0f, 1f)] public float giftChance = 0.035f;
    }
}
