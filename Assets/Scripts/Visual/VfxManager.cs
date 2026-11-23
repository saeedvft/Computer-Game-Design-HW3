using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Central one-shot visual effects spawner. Lives on the persistent
    /// GameManager and reacts to gameplay events so gameplay code stays clean.
    /// </summary>
    public class VfxManager : MonoBehaviour
    {
        private static VfxManager _instance;

        [Header("Prefabs")]
        public GameObject explosionPrefab;
        public GameObject smallExplosionPrefab;
        public GameObject muzzlePrefab;
        public GameObject shockwavePrefab;
        public GameObject sparklePrefab;
        public GameObject damagePrefab;

        [Header("Limits")]
        public int maxLiveEffects = 96;

        private Transform _root;

        public static VfxManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
            _root = new GameObject("VfxRoot").transform;
            _root.SetParent(transform, false);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnEnable()
        {
            GameEvents.EnemyDestroyed += HandleEnemyDestroyed;
            GameEvents.EggDestroyed += HandleEggDestroyed;
            GameEvents.BombUsed += HandleBombUsed;
            GameEvents.PlayerDamaged += HandlePlayerDamaged;
            GameEvents.PickupCollected += HandlePickupCollected;
        }

        private void OnDisable()
        {
            GameEvents.EnemyDestroyed -= HandleEnemyDestroyed;
            GameEvents.EggDestroyed -= HandleEggDestroyed;
            GameEvents.BombUsed -= HandleBombUsed;
            GameEvents.PlayerDamaged -= HandlePlayerDamaged;
            GameEvents.PickupCollected -= HandlePickupCollected;
        }

        public static void PlayExplosion(Vector3 position, float size = 1f)
        {
            if (_instance != null)
            {
                _instance.Spawn(_instance.explosionPrefab, position, size);
            }
        }

        public static void PlayMuzzle(Vector3 position)
        {
            if (_instance != null)
            {
                _instance.Spawn(_instance.muzzlePrefab, position, 1f);
            }
        }

        private void HandleEnemyDestroyed(Vector3 position, float size)
        {
            Spawn(explosionPrefab, position, size);
        }

        private void HandleEggDestroyed(Vector3 position)
        {
            Spawn(smallExplosionPrefab, position, 0.6f);
        }

        private void HandleBombUsed()
        {
            Camera camera = Camera.main;
            Vector3 center = camera != null ? camera.transform.position : Vector3.zero;
            center.z = 0f;
            Spawn(shockwavePrefab, center, 1f);
            GameEvents.RaiseShakeRequested(0.3f, 0.55f);
        }

        private void HandlePlayerDamaged(int remainingLives)
        {
            PlayerShip player = FindAnyObjectByType<PlayerShip>();
            if (player != null)
            {
                Spawn(damagePrefab, player.transform.position, 1f);
            }

            GameEvents.RaiseShakeRequested(0.34f, 0.4f);
        }

        private void HandlePickupCollected(PickupType type)
        {
            PlayerShip player = FindAnyObjectByType<PlayerShip>();
            if (player != null)
            {
                Spawn(sparklePrefab, player.transform.position, type == PickupType.Gift ? 1f : 0.8f);
            }
        }

        private void Spawn(GameObject prefab, Vector3 position, float scale)
        {
            if (prefab == null || _root == null || _root.childCount >= maxLiveEffects)
            {
                return;
            }

            position.z = 0f;
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            instance.transform.SetParent(_root, true);
            instance.transform.localScale = Vector3.one * Mathf.Max(0.05f, scale);
            Destroy(instance, 2f);
        }
    }
}
