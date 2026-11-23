using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace ChickenShooter
{
    /// <summary>
    /// Central object pooling service for bullets, eggs, pickups, asteroids and
    /// chickens. Pools live on the persistent GameManager object.
    /// </summary>
    [DisallowMultipleComponent]
    public class PoolManager : MonoBehaviour
    {
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        private readonly HashSet<GameObject> _active = new HashSet<GameObject>();

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[PoolManager] Tried to spawn a null prefab.");
                return null;
            }

            ObjectPool<GameObject> pool = GetPool(prefab);
            GameObject instance = pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            GameObject instance = Spawn(prefab, position, rotation);
            return instance != null ? instance.GetComponent<T>() : null;
        }

        public void Despawn(PooledObject pooled)
        {
            if (pooled == null || !pooled.IsSpawned)
            {
                return;
            }

            if (_pools.TryGetValue(pooled.Prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(pooled.gameObject);
            }
            else
            {
                pooled.gameObject.SetActive(false);
            }
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            ObjectPool<GameObject> pool = GetPool(prefab);
            for (int i = 0; i < count; i++)
            {
                GameObject instance = pool.Get();
                pool.Release(instance);
            }
        }

        public void ReleaseAll()
        {
            GameObject[] snapshot = new GameObject[_active.Count];
            _active.CopyTo(snapshot);
            foreach (GameObject go in snapshot)
            {
                if (go == null)
                {
                    continue;
                }

                PooledObject pooled = go.GetComponent<PooledObject>();
                if (pooled != null && pooled.IsSpawned)
                {
                    Despawn(pooled);
                }
            }
        }

        private ObjectPool<GameObject> GetPool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                return pool;
            }

            Transform parent = new GameObject($"Pool - {prefab.name}").transform;
            parent.SetParent(transform, false);

            pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject instance = Instantiate(prefab, parent);
                    PooledObject pooled = instance.GetComponent<PooledObject>();
                    if (pooled == null)
                    {
                        pooled = instance.AddComponent<PooledObject>();
                    }

                    pooled.Bind(this, prefab);
                    instance.SetActive(false);
                    return instance;
                },
                actionOnGet: instance =>
                {
                    instance.SetActive(true);
                    PooledObject pooled = instance.GetComponent<PooledObject>();
                    if (pooled != null)
                    {
                        pooled.OnSpawned();
                    }

                    _active.Add(instance);
                },
                actionOnRelease: instance =>
                {
                    _active.Remove(instance);
                    PooledObject pooled = instance.GetComponent<PooledObject>();
                    if (pooled != null)
                    {
                        pooled.OnDespawned();
                    }

                    instance.SetActive(false);
                },
                actionOnDestroy: instance =>
                {
                    _active.Remove(instance);
                    if (instance != null)
                    {
                        Destroy(instance);
                    }
                },
                collectionCheck: false,
                defaultCapacity: 16,
                maxSize: 512);

            _pools[prefab] = pool;
            return pool;
        }

        private void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            ReleaseAll();
        }
    }
}
