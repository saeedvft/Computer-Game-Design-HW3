using System;

namespace ChickenShooter
{
    /// <summary>
    /// The complete run state that will later be serialized to savegame.json.
    /// Keeping it as a plain serializable DTO makes the save/load phase trivial.
    /// </summary>
    [Serializable]
    public class GameState
    {
        public int levelIndex;
        public int roundIndex;
        public int score;
        public int lives;
        public int bombs;
        public int weaponPower;
        public float foodProgress;

        public GameState Clone()
        {
            return new GameState
            {
                levelIndex = levelIndex,
                roundIndex = roundIndex,
                score = score,
                lives = lives,
                bombs = bombs,
                weaponPower = weaponPower,
                foodProgress = foodProgress
            };
        }
    }
}
