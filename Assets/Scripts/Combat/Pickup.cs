using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Fried food and gift boxes dropped by chickens.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        public PickupType type = PickupType.Food;
        public float fallSpeed = 1.6f;
        public float foodValue = 0.12f;
        public int scoreValue = 20;

        private bool _collected;

        private void OnEnable()
        {
            _collected = false;
        }

        private void Update()
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
            transform.Rotate(0f, 0f, 45f * Time.deltaTime);

            Camera camera = Camera.main;
            if (camera != null && transform.position.y < CameraBounds.GetVisibleBounds(camera, 2f).min.y)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected)
            {
                return;
            }

            PlayerShip player = other.GetComponentInParent<PlayerShip>();
            if (player == null)
            {
                return;
            }

            _collected = true;
            GameManager manager = GameManager.Instance;

            if (type == PickupType.Gift)
            {
                manager.AddScore(scoreValue * 2);
                manager.UpgradeWeapon();
                Debug.Log("[Pickup] Weapon upgraded to level " + manager.State.weaponPower);
            }
            else
            {
                manager.AddScore(scoreValue);
                manager.AddFoodProgress(foodValue);
            }

            GameEvents.RaisePickupCollected(type);
            Despawn();
        }

        private void Despawn()
        {
            PooledObject pooled = GetComponent<PooledObject>();
            if (pooled != null)
            {
                pooled.Despawn();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
