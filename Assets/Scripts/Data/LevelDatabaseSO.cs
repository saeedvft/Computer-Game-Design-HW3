using UnityEngine;

namespace ChickenShooter
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Chicken Shooter/Level Database")]
    public class LevelDatabaseSO : ScriptableObject
    {
        public LevelDataSO[] levels;

        public int Count => levels != null ? levels.Length : 0;

        public LevelDataSO Get(int index)
        {
            if (levels == null || index < 0 || index >= levels.Length)
            {
                return null;
            }

            return levels[index];
        }

        public int IndexOf(string sceneName)
        {
            if (levels == null || string.IsNullOrEmpty(sceneName))
            {
                return -1;
            }

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] != null && levels[i].sceneName == sceneName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
