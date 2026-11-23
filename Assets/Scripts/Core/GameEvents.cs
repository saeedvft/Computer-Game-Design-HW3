using System;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Lightweight event bus. Gameplay systems raise events; UI, audio and VFX
    /// subscribe without the gameplay code knowing about them.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<int> ScoreChanged;
        public static event Action<int> LivesChanged;
        public static event Action<int> BombsChanged;
        public static event Action<int> WeaponPowerChanged;
        public static event Action<float> FoodProgressChanged;
        public static event Action<int, int, int> RoundStarted;   // level (1-based), round (1-based), total rounds
        public static event Action<int, int> RoundCleared;         // level, round
        public static event Action<int> LevelCompleted;
        public static event Action<int> PlayerDamaged;             // remaining lives
        public static event Action BombUsed;
        public static event Action GameOver;
        public static event Action Victory;
        public static event Action<bool> PauseChanged;             // true = paused
        public static event Action<GameState> SaveRequested;
        public static event Action<PickupType> PickupCollected;
        public static event Action<Vector3, float> EnemyDestroyed;   // world position, size multiplier
        public static event Action<Vector3> EggDestroyed;
        public static event Action<float, float> ShakeRequested;     // strength, duration

        public static void RaiseScoreChanged(int value) => ScoreChanged?.Invoke(value);
        public static void RaiseLivesChanged(int value) => LivesChanged?.Invoke(value);
        public static void RaiseBombsChanged(int value) => BombsChanged?.Invoke(value);
        public static void RaiseWeaponPowerChanged(int value) => WeaponPowerChanged?.Invoke(value);
        public static void RaiseFoodProgressChanged(float value) => FoodProgressChanged?.Invoke(value);
        public static void RaiseRoundStarted(int level, int round, int total) => RoundStarted?.Invoke(level, round, total);
        public static void RaiseRoundCleared(int level, int round) => RoundCleared?.Invoke(level, round);
        public static void RaiseLevelCompleted(int level) => LevelCompleted?.Invoke(level);
        public static void RaisePlayerDamaged(int remainingLives) => PlayerDamaged?.Invoke(remainingLives);
        public static void RaiseBombUsed() => BombUsed?.Invoke();
        public static void RaiseGameOver() => GameOver?.Invoke();
        public static void RaiseVictory() => Victory?.Invoke();
        public static void RaisePauseChanged(bool paused) => PauseChanged?.Invoke(paused);
        public static void RaiseSaveRequested(GameState state) => SaveRequested?.Invoke(state);
        public static void RaisePickupCollected(PickupType type) => PickupCollected?.Invoke(type);
        public static void RaiseEnemyDestroyed(Vector3 position, float size) => EnemyDestroyed?.Invoke(position, size);
        public static void RaiseEggDestroyed(Vector3 position) => EggDestroyed?.Invoke(position);
        public static void RaiseShakeRequested(float strength, float duration) => ShakeRequested?.Invoke(strength, duration);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ScoreChanged = null;
            LivesChanged = null;
            BombsChanged = null;
            WeaponPowerChanged = null;
            FoodProgressChanged = null;
            RoundStarted = null;
            RoundCleared = null;
            LevelCompleted = null;
            PlayerDamaged = null;
            BombUsed = null;
            GameOver = null;
            Victory = null;
            PauseChanged = null;
            SaveRequested = null;
            PickupCollected = null;
            EnemyDestroyed = null;
            EggDestroyed = null;
            ShakeRequested = null;
        }
    }
}
