using System.Collections;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Player ship movement, screen clamping, lives, invincibility and damage
    /// feedback. Input is delegated entirely to PlayerInputReader.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerShip : MonoBehaviour, IDamageable
    {
        [Header("References")]
        public SpriteRenderer spriteRenderer;
        public Rigidbody2D body;
        public PlayerInputReader input;
        public PlayerWeapon weapon;
        public BombSystem bombSystem;

        [Header("Movement")]
        public float moveSpeed = 7f;
        public float invincibilityDuration = 2f;
        public float blinkInterval = 0.1f;

        private Camera _camera;
        private Coroutine _invincibilityRoutine;
        private bool _dead;

        public bool IsDead => _dead;
        public bool IsInvincible { get; private set; }

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (weapon == null) weapon = GetComponent<PlayerWeapon>();
            if (bombSystem == null) bombSystem = GetComponent<BombSystem>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Start()
        {
            _camera = Camera.main;
            GameManager.Instance.EnsureSession();
        }

        private void FixedUpdate()
        {
            if (_dead || input == null || GameManager.Instance.State.lives <= 0)
            {
                return;
            }

            Vector2 move = Vector2.ClampMagnitude(input.Move, 1f);
            Vector2 next = body.position + move * (moveSpeed * Time.fixedDeltaTime);
            body.MovePosition(ClampToCamera(next));
        }

        private Vector2 ClampToCamera(Vector2 position)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                return position;
            }

            Bounds bounds = CameraBounds.GetVisibleBounds(_camera);
            Vector2 extents = spriteRenderer != null
                ? (Vector2)spriteRenderer.bounds.extents
                : new Vector2(0.25f, 0.25f);

            position.x = Mathf.Clamp(position.x, bounds.min.x + extents.x, bounds.max.x - extents.x);
            position.y = Mathf.Clamp(position.y, bounds.min.y + extents.y, bounds.max.y - extents.y);
            return position;
        }

        public void TakeDamage(int amount, GameObject source = null)
        {
            if (_dead || IsInvincible || amount <= 0)
            {
                return;
            }

            GameManager manager = GameManager.Instance;
            if (manager.State.lives <= 0)
            {
                return;
            }

            manager.LoseLife();
            manager.DowngradeWeapon();
            GameEvents.RaisePlayerDamaged(manager.State.lives);

            if (manager.State.lives <= 0)
            {
                Die();
                return;
            }

            if (_invincibilityRoutine != null)
            {
                StopCoroutine(_invincibilityRoutine);
            }

            _invincibilityRoutine = StartCoroutine(InvincibilityRoutine());
        }

        private IEnumerator InvincibilityRoutine()
        {
            IsInvincible = true;
            float elapsed = 0f;
            bool visible = true;

            while (elapsed < invincibilityDuration)
            {
                elapsed += blinkInterval;
                visible = !visible;
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = visible;
                }

                yield return new WaitForSeconds(blinkInterval);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            IsInvincible = false;
            _invincibilityRoutine = null;
        }

        private void Die()
        {
            _dead = true;
            IsInvincible = true;
            if (weapon != null) weapon.enabled = false;
            if (bombSystem != null) bombSystem.enabled = false;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (input != null) input.enabled = false;
            Debug.Log("[PlayerShip] Player lost all lives.");
        }
    }
}
