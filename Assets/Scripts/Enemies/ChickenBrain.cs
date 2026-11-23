using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Chicken behaviour: curved entry, formation idle/egg dropping, suicide
    /// dive toward the player, and return. Implemented as an explicit state
    /// machine to keep the enemy code data-driven and easy to extend.
    /// </summary>
    public class ChickenBrain : MonoBehaviour
    {
        public enum State
        {
            Dormant,
            Entering,
            InFormation,
            Diving,
            Returning
        }

        private EnemyUnit _unit;
        private EnemyDataSO _data;
        private WaveDataSO _wave;
        private FormationController _formation;
        private Transform _player;

        private Vector2 _slot;
        private Vector3 _slotWorld;
        private Vector3[] _path;
        private float _entryDuration;
        private float _entryProgress;
        private float _difficulty;
        private float _moveSpeed;
        private float _diveSpeed;
        private float _bobPhase;
        private float _diveTimer;
        private float _nextEggTime;
        private float _nextDiveCheckTime;
        private bool _canDropEggs;
        private bool _countedAsDiver;
        private static int _activeDivers;

        public State CurrentState { get; private set; } = State.Dormant;
        public bool IsDiving => CurrentState == State.Diving;

        public void Initialize(
            EnemyDataSO data,
            WaveDataSO wave,
            FormationController formation,
            Vector2 slot,
            Transform player,
            float difficulty)
        {
            _unit = GetComponent<EnemyUnit>();
            _data = data;
            _wave = wave;
            _formation = formation;
            _slot = slot;
            _player = player;
            _difficulty = Mathf.Max(0.5f, difficulty);
            _bobPhase = Random.Range(0f, Mathf.PI * 2f);
            _diveTimer = 0f;

            _moveSpeed = (data != null ? data.moveSpeed : 3f) * (0.85f + 0.15f * _difficulty);
            _diveSpeed = (data != null ? data.diveSpeed : 5f) * (0.9f + 0.1f * _difficulty);

            _slotWorld = formation != null ? formation.GetSlotWorld(slot) : transform.position;
            BuildEntryPath();
            CurrentState = State.Entering;
            _entryProgress = 0f;

            // Only part of the formation drops eggs, which keeps the screen
            // readable while still producing a steady stream of hazards.
            _canDropEggs = Random.value < 0.6f;
            _nextEggTime = Time.time + RandomEggDelay() * Random.Range(1.2f, 2.2f);
            _nextDiveCheckTime = Time.time + RandomDiveDelay();
        }

        private void BuildEntryPath()
        {
            Vector2[] waypoints = _wave != null && _wave.entryPath != null
                ? _wave.entryPath.waypoints
                : null;

            float spread = _wave != null && _wave.entryPath != null ? _wave.entryPath.spreadX : 0f;
            float duration = _wave != null && _wave.entryPath != null ? _wave.entryPath.duration : 1.6f;
            _entryDuration = Mathf.Max(0.25f, duration / (0.9f + 0.1f * _difficulty));

            if (waypoints == null || waypoints.Length == 0)
            {
                _path = new[]
                {
                    _slotWorld + new Vector3(-2f, 4f, 0f),
                    _slotWorld + new Vector3(0f, 2f, 0f),
                    _slotWorld
                };
            }
            else
            {
                var points = new System.Collections.Generic.List<Vector3>(waypoints.Length + 1);
                for (int i = 0; i < waypoints.Length; i++)
                {
                    Vector2 point = waypoints[i];
                    if (i == 0 && spread > 0f)
                    {
                        point.x += Random.Range(-spread, spread);
                    }

                    points.Add(_slotWorld + new Vector3(point.x, point.y, 0f));
                }

                if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], _slotWorld) > 0.01f)
                {
                    points.Add(_slotWorld);
                }

                _path = points.ToArray();
            }

            transform.position = _path[0];
        }

        private void Update()
        {
            if (GameManager.Instance.State.lives <= 0)
            {
                return;
            }

            switch (CurrentState)
            {
                case State.Entering:
                    UpdateEntering();
                    break;
                case State.InFormation:
                    UpdateFormation();
                    break;
                case State.Diving:
                    UpdateDiving();
                    break;
                case State.Returning:
                    UpdateReturning();
                    break;
            }
        }

        private void UpdateEntering()
        {
            _entryProgress += Time.deltaTime / _entryDuration;
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_entryProgress));
            transform.position = PathUtils.CatmullRom(_path, eased);

            if (_entryProgress >= 1f)
            {
                CurrentState = State.InFormation;
                _nextEggTime = Time.time + RandomEggDelay();
                _nextDiveCheckTime = Time.time + RandomDiveDelay();
            }
        }

        private void UpdateFormation()
        {
            Vector3 target = GetSlotTarget();
            float blend = 1f - Mathf.Exp(-10f * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, target, blend);

            if (Time.time >= _nextEggTime)
            {
                if (_canDropEggs)
                {
                    DropEgg();
                }

                _nextEggTime = Time.time + RandomEggDelay();
            }

            if (Time.time >= _nextDiveCheckTime)
            {
                _nextDiveCheckTime = Time.time + RandomDiveDelay();
                float chance = (_data != null ? _data.diveChance : 0f) *
                               (_wave != null ? _wave.diveChanceMultiplier : 1f);
                if (Random.value < chance)
                {
                    StartDive();
                }
            }
        }

        private void UpdateDiving()
        {
            _diveTimer += Time.deltaTime;

            if (_player != null)
            {
                Vector3 direction = _player.position - transform.position;
                if (direction.sqrMagnitude > 0.001f)
                {
                    transform.position += direction.normalized * (_diveSpeed * Time.deltaTime);
                }
            }

            Camera camera = Camera.main;
            bool belowScreen = camera != null &&
                               transform.position.y < CameraBounds.GetVisibleBounds(camera, 1.5f).min.y;

            if (_diveTimer > 5f || belowScreen || _player == null)
            {
                EndDive();
                CurrentState = State.Returning;
                _diveTimer = 0f;
            }
        }

        private void UpdateReturning()
        {
            Vector3 target = GetSlotTarget();
            transform.position = Vector3.MoveTowards(transform.position, target, _moveSpeed * 1.6f * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.08f)
            {
                CurrentState = State.InFormation;
                _nextEggTime = Time.time + RandomEggDelay();
            }
        }

        private Vector3 GetSlotTarget()
        {
            if (_formation == null)
            {
                return _slotWorld + Vector3.up * (Mathf.Sin(Time.time * 3f + _bobPhase) * 0.07f);
            }

            Vector3 target = _formation.GetSlotWorld(_slot);
            return target + Vector3.up * (Mathf.Sin(Time.time * 3f + _bobPhase) * 0.07f);
        }

        private void StartDive()
        {
            if (CurrentState != State.InFormation)
            {
                return;
            }

            // Cap simultaneous divers so a large formation cannot swarm the
            // ship all at once. Higher difficulty allows one extra attacker.
            int maxDivers = _difficulty >= 1.8f ? 3 : (_difficulty >= 1.3f ? 2 : 1);
            if (_activeDivers >= maxDivers)
            {
                return;
            }

            _activeDivers++;
            _countedAsDiver = true;
            CurrentState = State.Diving;
            _diveTimer = 0f;
        }

        private void EndDive()
        {
            if (!_countedAsDiver)
            {
                return;
            }

            _countedAsDiver = false;
            _activeDivers = Mathf.Max(0, _activeDivers - 1);
        }

        private void DropEgg()
        {
            if (_data == null || _data.eggPrefab == null || _player == null)
            {
                return;
            }

            PoolManager pool = GameManager.Instance.Pool;
            if (pool == null)
            {
                return;
            }

            EggProjectile egg = pool.Spawn<EggProjectile>(
                _data.eggPrefab,
                transform.position + Vector3.down * 0.4f,
                Quaternion.identity);

            if (egg != null)
            {
                egg.Initialize(Vector2.down * _data.eggSpeed, _data.eggDamage);
            }
        }

        private float RandomEggDelay()
        {
            if (_data == null)
            {
                return 5f;
            }

            float multiplier = _wave != null ? Mathf.Max(0.1f, _wave.fireRateMultiplier) : 1f;
            float interval = Random.Range(_data.eggInterval.x, _data.eggInterval.y);
            return Mathf.Max(0.35f, interval / (multiplier * (0.9f + 0.1f * _difficulty)));
        }

        private float RandomDiveDelay()
        {
            if (_data == null)
            {
                return 8f;
            }

            return Mathf.Max(1.5f, Random.Range(_data.diveInterval.x, _data.diveInterval.y));
        }

        private void OnDisable()
        {
            EndDive();
            CurrentState = State.Dormant;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _activeDivers = 0;
        }
    }
}
