using System;
using UnityEngine;

namespace ChickenShooter
{
    [Serializable]
    public class RoundDefinition
    {
        public WaveDataSO wave;

        [Range(0.5f, 3f)]
        public float difficulty = 1f;
    }

    [CreateAssetMenu(fileName = "Level_", menuName = "Chicken Shooter/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Identity")]
        public int levelNumber = 1;
        public string displayName = "Earth Orbit";
        public string sceneName = "Level1";

        [Header("Presentation")]
        public LevelTheme theme = LevelTheme.EarthOrbit;
        public Color themeColor = new Color(0.02f, 0.04f, 0.10f, 1f);
        public AudioClip backgroundMusic;

        [Header("Rounds")]
        public RoundDefinition[] rounds = Array.Empty<RoundDefinition>();

        public int RoundCount => rounds != null ? rounds.Length : 0;
    }
}
