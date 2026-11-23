using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Chicken egg. Falls downward, can be destroyed in the air by the player's
    /// bullets, and damages the ship on contact.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EggProjectile : MonoBehaviour, IDamageable
    {
        public Rigidbody2D body;

        [SerializeField] private float lifetime = 10f;

        private int _damage = 1;
        private float _age;
        private bool _consumed;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
        }

        private void OnEnable()
        {
            _age = 0f;
            _consumed = false;
            EnemyRegistry.RegisterEgg(this);
        }

        private void OnDisable()
        {
            EnemyRegistry.UnregisterEgg(this);
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        public void Initialize(Vector2 velocity, int damage)
        {
            _damage = Mathf.Max(1, damage);
            _age = 0f;
            _consumed = false;
            body.linearVelocity = velocity;
        }

        private void FixedUpdate()
        {
            _age += Time.fixedDeltaTime;
            if (_age >= lifetime)
            {
                Despawn();
                return;
            }

            Camera camera = Camera.main;
            if (camera != null && CameraBounds.IsOutside(camera, transform.position, 2f))
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed)
            {
                return;
            }

            PlayerShip player = other.GetComponentInParent<PlayerShip>();
            if (player == null)
            {
                return;
            }

            _consumed = true;
            player.TakeDamage(_damage, gameObject);
            Despawn();
        }

        public void TakeDamage(int amount, GameObject source = null)
        {
            if (_consumed)
            {
                return;
            }

            _consumed = true;
            GameEvents.RaiseEggDestroyed(transform.position);
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
