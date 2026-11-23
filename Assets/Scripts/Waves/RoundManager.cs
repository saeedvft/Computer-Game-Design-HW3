using System.Collections;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Runs the rounds of one level. Each WaveDataSO is spawned through the pool,
    /// then the manager waits until the registry reports no living enemies.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        private LevelController _levelController;
        private LevelDataSO _level;
        private PlayerShip _player;
        private Coroutine _routine;
        private FormationController _formation;
        private GameObject _formationObject;
        private bool _running;

        public void Initialize(LevelController controller, LevelDataSO level, PlayerShip player)
        {
            _levelController = controller;
            _level = level;
            _player = player;
        }

        public void StartRounds(int startRound)
        {
            StopRounds();
            _running = true;
            _routine = StartCoroutine(RunRounds(Mathf.Max(0, startRound)));
        }

        public void StopRounds()
        {
            _running = false;

            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            ClearFormation();
        }

        private IEnumerator RunRounds(int startRound)
        {
            if (_level == null)
            {
                Debug.LogError("[RoundManager] No LevelDataSO assigned.");
                yield break;
            }

            for (int i = startRound; i < _level.RoundCount; i++)
            {
                if (!_running)
                {
                    yield break;
                }

                RoundDefinition definition = _level.rounds[i];
                if (definition == null || definition.wave == null)
                {
                    Debug.LogWarning($"[RoundManager] Skipping empty round {i + 1}.");
                    continue;
                }

                GameManager manager = GameManager.Instance;
                manager.NotifyRoundStarted(i);
                GameEvents.RaiseRoundStarted(_level.levelNumber, i + 1, _level.RoundCount);
                Debug.Log($"[RoundManager] Level {_level.levelNumber} - Round {i + 1}/{_level.RoundCount}");

                yield return SpawnWave(definition, i);

                float timeout = 600f;
                while (EnemyRegistry.EnemyCount > 0 && timeout > 0f)
                {
                    if (!_running || manager.State.lives <= 0)
                    {
                        yield break;
                    }

                    timeout -= 0.2f;
                    yield return new WaitForSeconds(0.2f);
                }

                if (timeout <= 0f)
                {
                    Debug.LogWarning("[RoundManager] Round timed out with enemies still alive.");
                }

                GameEvents.RaiseRoundCleared(_level.levelNumber, i + 1);
                manager.RequestSave();
                Debug.Log($"[RoundManager] Level {_level.levelNumber} - Round {i + 1} cleared.");

                yield return new WaitForSeconds(1f);
            }

            ClearFormation();
            _running = false;
            _levelController?.OnAllRoundsComplete();
        }

        private IEnumerator SpawnWave(RoundDefinition definition, int roundIndex)
        {
            WaveDataSO wave = definition.wave;
            float difficulty = Mathf.Max(0.5f, definition.difficulty);

            if (wave.kind == WaveKind.AsteroidField)
            {
                yield return SpawnAsteroids(wave, difficulty, roundIndex);
            }
            else
            {
                yield return SpawnChickens(wave, difficulty, roundIndex);
            }
        }

        private IEnumerator SpawnChickens(WaveDataSO wave, float difficulty, int roundIndex)
        {
            if (wave.enemyPrefab == null)
            {
                Debug.LogError($"[RoundManager] Wave '{wave.waveName}' has no chicken prefab.");
                yield break;
            }

            ClearFormation();

            _formationObject = new GameObject($"Formation_R{roundIndex + 1}");
            _formationObject.transform.position = wave.formationCenter;
            _formation = _formationObject.AddComponent<FormationController>();
            _formation.Configure(wave.swayAmplitude, wave.swaySpeed, wave.descentSpeed);

            var slots = FormationBuilder.Build(wave);
            float stagger = wave.entryPath != null ? Mathf.Max(0f, wave.entryPath.stagger) : 0f;
            PoolManager pool = GameManager.Instance.Pool;
            Transform playerTarget = _player != null ? _player.transform : null;

            foreach (Vector2 slot in slots)
            {
                GameObject instance = pool.Spawn(
                    wave.enemyPrefab,
                    _formation.GetSlotWorld(slot),
                    Quaternion.identity);

                if (instance != null)
                {
                    EnemyUnit unit = instance.GetComponent<EnemyUnit>();
                    if (unit != null)
                    {
                        EnemyDataSO data = wave.enemyData;
                        unit.ConfigureChicken(
                            data,
                            difficulty,
                            data != null ? data.foodPrefab : null,
                            data != null ? data.giftPrefab : null);
                    }

                    ChickenBrain brain = instance.GetComponent<ChickenBrain>();
                    if (brain != null)
                    {
                        brain.Initialize(wave.enemyData, wave, _formation, slot, playerTarget, difficulty);
                    }
                }

                if (stagger > 0.001f)
                {
                    yield return new WaitForSeconds(stagger);
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator SpawnAsteroids(WaveDataSO wave, float difficulty, int roundIndex)
        {
            if (wave.asteroidPrefab == null)
            {
                Debug.LogError($"[RoundManager] Wave '{wave.waveName}' has no asteroid prefab.");
                yield break;
            }

            ClearFormation();

            Camera camera = Camera.main;
            Bounds bounds = CameraBounds.GetVisibleBounds(camera, 1f);
            PoolManager pool = GameManager.Instance.Pool;
            int count = Mathf.Max(1, wave.asteroidCount);
            int health = Mathf.Max(1, Mathf.RoundToInt(wave.asteroidHealth * difficulty));

            for (int i = 0; i < count; i++)
            {
                float x = Random.Range(bounds.min.x + 0.8f, bounds.max.x - 0.8f);
                float y = bounds.max.y + 1f;

                GameObject instance = pool.Spawn(
                    wave.asteroidPrefab,
                    new Vector3(x, y, 0f),
                    Quaternion.identity);

                if (instance != null)
                {
                    Asteroid asteroid = instance.GetComponent<Asteroid>();
                    if (asteroid != null)
                    {
                        asteroid.Initialize(health, wave.asteroidScore, wave.asteroidFallSpeed, wave.asteroidSizeRange);
                    }
                }

                yield return new WaitForSeconds(Random.Range(0.25f, 0.6f));
            }
        }

        private void ClearFormation()
        {
            if (_formationObject != null)
            {
                Destroy(_formationObject);
                _formationObject = null;
            }

            _formation = null;
        }
    }
}
