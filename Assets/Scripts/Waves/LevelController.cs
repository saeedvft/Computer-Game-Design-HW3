using System.Collections;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Scene-level coordinator. Resolves the LevelDataSO, wires the round
    /// manager and handles level completion / game over transitions.
    /// </summary>
    [RequireComponent(typeof(RoundManager))]
    public class LevelController : MonoBehaviour
    {
        [Header("Scene Data")]
        public LevelDataSO level;
        public int fallbackLevelIndex;
        public Camera mainCamera;
        public LevelBanner banner;

        private RoundManager _roundManager;
        private bool _ending;

        private void Awake()
        {
            _roundManager = GetComponent<RoundManager>();
            if (_roundManager == null)
            {
                _roundManager = gameObject.AddComponent<RoundManager>();
            }
        }

        private void OnEnable()
        {
            GameEvents.GameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.GameOver -= HandleGameOver;
        }

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureSession();

            LevelDatabaseSO database = manager.Database;
            if (level == null && database != null)
            {
                level = database.Get(fallbackLevelIndex);
            }

            if (level == null)
            {
                Debug.LogError("[LevelController] No LevelDataSO could be resolved.");
                return;
            }

            int index = database != null ? database.IndexOf(level.sceneName) : fallbackLevelIndex;
            if (index < 0)
            {
                index = fallbackLevelIndex;
            }

            if (manager.State.levelIndex != index)
            {
                manager.SetLevelIndex(index);
                manager.SetRoundIndex(0);
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = level.themeColor;
            }

            PlayerShip player = FindAnyObjectByType<PlayerShip>();
            _roundManager.Initialize(this, level, player);

            int startRound = Mathf.Clamp(manager.State.roundIndex, 0, Mathf.Max(0, level.RoundCount - 1));
            _roundManager.StartRounds(startRound);

            if (banner != null)
            {
                banner.Show($"LEVEL {level.levelNumber}", level.displayName.ToUpperInvariant(), 1.9f);
            }

            Debug.Log($"[LevelController] Starting {level.displayName} ({level.sceneName}) at round {startRound + 1}.");
        }

        public void OnAllRoundsComplete()
        {
            if (_ending)
            {
                return;
            }

            _ending = true;
            StartCoroutine(CompleteLevelRoutine());
        }

        private IEnumerator CompleteLevelRoutine()
        {
            GameManager manager = GameManager.Instance;
            bool lastLevel = manager.IsLastLevel;

            GameEvents.RaiseLevelCompleted(level != null ? level.levelNumber : 0);

            if (banner != null)
            {
                banner.Show(
                    lastLevel ? "VICTORY!" : $"LEVEL {level.levelNumber} COMPLETE",
                    lastLevel ? "THE GALAXY IS SAFE" : "GET READY",
                    2.5f);
            }

            Debug.Log($"[LevelController] Level {level.levelNumber} complete. Loading the next scene.");

            yield return new WaitForSeconds(2.5f);
            manager.StartNextLevel();
        }

        private void HandleGameOver()
        {
            if (_ending)
            {
                return;
            }

            _ending = true;
            _roundManager.StopRounds();
            Debug.Log("[LevelController] Game over.");
            StartCoroutine(GameOverRoutine());
        }

        private IEnumerator GameOverRoutine()
        {
            if (banner != null)
            {
                banner.Show("GAME OVER", "THE CHICKENS WIN THIS ONE", 1.8f);
            }

            yield return new WaitForSecondsRealtime(2.2f);
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}
