using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChickenShooter
{
    /// <summary>
    /// Persistent owner of the run state. It survives scene changes with
    /// DontDestroyOnLoad so lives, score, bombs, weapon power and food progress
    /// are carried from level to level.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = FindAnyObjectByType<GameManager>();
                if (_instance != null)
                {
                    return _instance;
                }

                GameManager prefab = Resources.Load<GameManager>("GameManager");
                if (prefab != null)
                {
                    _instance = Instantiate(prefab);
                }
                else
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }

                return _instance;
            }
        }

        [Header("Database")]
        public LevelDatabaseSO database;

        [Header("Start Values")]
        public int startingLives = 3;
        public int startingBombs = 2;
        public int maxWeaponPower = 3;

        public GameState State { get; private set; } = new GameState();
        public PoolManager Pool { get; private set; }

        private bool _sessionStarted;

        public LevelDatabaseSO Database
        {
            get
            {
                EnsureDatabase();
                return database;
            }
        }

        public LevelDataSO CurrentLevel
        {
            get
            {
                EnsureDatabase();
                return database != null ? database.Get(State.levelIndex) : null;
            }
        }

        public bool IsLastLevel => Database != null && State.levelIndex >= database.Count - 1;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Pool = GetComponent<PoolManager>();
            if (Pool == null)
            {
                Pool = gameObject.AddComponent<PoolManager>();
            }

            EnsureDatabase();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        private void EnsureDatabase()
        {
            if (database == null)
            {
                database = Resources.Load<LevelDatabaseSO>("LevelDatabase");
            }
        }

        public void EnsureSession()
        {
            if (!_sessionStarted)
            {
                ResetRunState();
            }
        }

        public void StartNewGame()
        {
            ResetRunState();
            LoadLevel(0, 0);
        }

        public void ApplyState(GameState state)
        {
            if (state == null)
            {
                return;
            }

            State = state.Clone();
            ClampState();
            _sessionStarted = true;
        }

        public void LoadLevel(int levelIndex, int roundIndex)
        {
            EnsureDatabase();
            LevelDataSO level = database != null ? database.Get(levelIndex) : null;
            if (level == null)
            {
                Debug.LogError($"[GameManager] Level index {levelIndex} is missing from the database.");
                return;
            }

            State.levelIndex = levelIndex;
            State.roundIndex = Mathf.Max(0, roundIndex);
            _sessionStarted = true;
            Time.timeScale = 1f;
            RequestSave();

            SceneManager.LoadScene(level.sceneName);
        }

        public void StartNextLevel()
        {
            int next = State.levelIndex + 1;
            if (Database != null && next < database.Count)
            {
                LoadLevel(next, 0);
                return;
            }

            GameEvents.RaiseVictory();
            SceneManager.LoadScene("MainMenu");
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void SetLevelIndex(int index)
        {
            State.levelIndex = Mathf.Max(0, index);
        }

        public void SetRoundIndex(int index)
        {
            State.roundIndex = Mathf.Max(0, index);
        }

        public void NotifyRoundStarted(int roundIndex)
        {
            State.roundIndex = Mathf.Max(0, roundIndex);
            RequestSave();
        }

        public void RequestSave()
        {
            GameEvents.RaiseSaveRequested(State.Clone());
        }

        public void AddScore(int amount)
        {
            if (amount == 0)
            {
                return;
            }

            State.score += amount;
            GameEvents.RaiseScoreChanged(State.score);
        }

        public void LoseLife()
        {
            State.lives = Mathf.Max(0, State.lives - 1);
            GameEvents.RaiseLivesChanged(State.lives);

            if (State.lives <= 0)
            {
                GameEvents.RaiseGameOver();
            }
        }

        public void DowngradeWeapon()
        {
            State.weaponPower = Mathf.Max(1, State.weaponPower - 1);
            GameEvents.RaiseWeaponPowerChanged(State.weaponPower);
        }

        public void UpgradeWeapon()
        {
            State.weaponPower = Mathf.Min(maxWeaponPower, State.weaponPower + 1);
            GameEvents.RaiseWeaponPowerChanged(State.weaponPower);
        }

        public void AddBomb(int amount)
        {
            State.bombs = Mathf.Max(0, State.bombs + amount);
            GameEvents.RaiseBombsChanged(State.bombs);
        }

        public bool TryConsumeBomb()
        {
            if (State.bombs <= 0)
            {
                return false;
            }

            State.bombs--;
            GameEvents.RaiseBombsChanged(State.bombs);
            return true;
        }

        public void AddFoodProgress(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            State.foodProgress += amount;
            while (State.foodProgress >= 1f)
            {
                State.foodProgress -= 1f;
                AddBomb(1);
            }

            GameEvents.RaiseFoodProgressChanged(State.foodProgress);
        }

        private void ResetRunState()
        {
            State = new GameState
            {
                levelIndex = 0,
                roundIndex = 0,
                score = 0,
                lives = startingLives,
                bombs = startingBombs,
                weaponPower = 1,
                foodProgress = 0f
            };

            _sessionStarted = true;

            GameEvents.RaiseScoreChanged(State.score);
            GameEvents.RaiseLivesChanged(State.lives);
            GameEvents.RaiseBombsChanged(State.bombs);
            GameEvents.RaiseWeaponPowerChanged(State.weaponPower);
            GameEvents.RaiseFoodProgressChanged(State.foodProgress);
        }

        private void ClampState()
        {
            State.lives = Mathf.Clamp(State.lives, 0, 99);
            State.bombs = Mathf.Max(0, State.bombs);
            State.weaponPower = Mathf.Clamp(State.weaponPower, 1, maxWeaponPower);
            State.score = Mathf.Max(0, State.score);
            State.foodProgress = Mathf.Clamp01(State.foodProgress);
            State.levelIndex = Mathf.Max(0, State.levelIndex);
            State.roundIndex = Mathf.Max(0, State.roundIndex);
        }
    }
}
