using System;
using UnityEngine;

namespace ChickenShooter
{
    [Serializable]
    public class EntryPathConfig
    {
        [Tooltip("Offsets relative to the formation slot. The first point is where the chicken enters the screen.")]
        public Vector2[] waypoints =
        {
            new Vector2(-2.5f, 3.5f),
            new Vector2(-1f, 1.8f),
            Vector2.zero
        };

        [Min(0.1f)]
        public float duration = 1.6f;

        [Min(0f)]
        public float stagger = 0.08f;

        [Min(0f)]
        public float spreadX = 0.45f;
    }

    [CreateAssetMenu(fileName = "Wave_", menuName = "Chicken Shooter/Wave Data")]
    public class WaveDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string waveName = "Wave";
        public WaveKind kind = WaveKind.ChickenFormation;

        [Header("Chicken Wave")]
        public GameObject enemyPrefab;
        public EnemyDataSO enemyData;
        public int count = 15;
        public FormationType formation = FormationType.Grid;
        public int rows = 3;
        public int columns = 5;
        public Vector2 spacing = new Vector2(1.2f, 0.85f);
        public Vector2 formationCenter = new Vector2(0f, 1.9f);

        [Header("Formation Motion")]
        public float swayAmplitude = 0.6f;
        public float swaySpeed = 1.2f;
        public float descentSpeed = 0.03f;

        [Header("Entry Path")]
        public EntryPathConfig entryPath = new EntryPathConfig();

        [Header("Behavior Multipliers")]
        [Min(0.1f)] public float fireRateMultiplier = 1f;
        [Min(0f)] public float diveChanceMultiplier = 1f;

        [Header("Asteroid Wave")]
        public GameObject asteroidPrefab;
        public int asteroidCount = 6;
        public int asteroidHealth = 3;
        public float asteroidFallSpeed = 1.4f;
        public Vector2 asteroidSizeRange = new Vector2(0.8f, 1.5f);
        public int asteroidScore = 15;
    }
}
