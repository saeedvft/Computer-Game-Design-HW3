using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Pooled straight-line projectile used by the player and later by bosses.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        public Rigidbody2D body;

        [SerializeField] private float lifetime = 6f;

        private int _damage;
        private bool _fromPlayer;
        private float _age;
        private bool _consumed;

        public bool FromPlayer => _fromPlayer;

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
        }

        private void OnDisable()
        {
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        public void Initialize(int damage, float speed, Vector2 direction, bool fromPlayer)
        {
            _damage = damage;
            _fromPlayer = fromPlayer;
            _age = 0f;
            _consumed = false;

            Vector2 normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            body.linearVelocity = normalized * speed;
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

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            if (_fromPlayer && damageable is PlayerShip)
            {
                return;
            }

            _consumed = true;
            damageable.TakeDamage(_damage, gameObject);
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
