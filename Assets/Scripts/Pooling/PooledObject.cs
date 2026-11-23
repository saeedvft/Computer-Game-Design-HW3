using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Bookkeeping component attached to pooled instances so they can return
    /// themselves to the right pool without knowing where they came from.
    /// </summary>
    [DisallowMultipleComponent]
    public class PooledObject : MonoBehaviour
    {
        public PoolManager Manager { get; private set; }
        public GameObject Prefab { get; private set; }
        public bool IsSpawned { get; private set; }

        internal void Bind(PoolManager manager, GameObject prefab)
        {
            Manager = manager;
            Prefab = prefab;
        }

        internal void OnSpawned()
        {
            IsSpawned = true;
        }

        internal void OnDespawned()
        {
            IsSpawned = false;
        }

        public void Despawn()
        {
            if (Manager != null)
            {
                Manager.Despawn(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
