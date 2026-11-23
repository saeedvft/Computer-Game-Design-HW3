using System.Collections;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Shared health/drop behaviour for all normal enemies (chickens and
    /// asteroids). Behaviour-specific movement lives in ChickenBrain/Asteroid.
    /// </summary>
    public class EnemyUnit : MonoBehaviour, IBombTarget
    {
        public SpriteRenderer spriteRenderer;

        public int MaxHealth { get; private set; } = 1;
        public int Health { get; private set; } = 1;
        public int ScoreValue { get; private set; }
        public float FoodChance { get; private set; }
        public float GiftChance { get; private set; }
        public bool IsAsteroid { get; private set; }

        private GameObject _foodPrefab;
        private GameObject _giftPrefab;
        private Color _baseColor = Color.white;
        private bool _dead;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            _dead = false;
            EnemyRegistry.RegisterEnemy(this);
        }

        private void OnDisable()
        {
            EnemyRegistry.UnregisterEnemy(this);

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
        }

        public void ConfigureChicken(EnemyDataSO data, float difficulty, GameObject foodPrefab, GameObject giftPrefab)
        {
            IsAsteroid = false;
            _foodPrefab = foodPrefab;
            _giftPrefab = giftPrefab;

            if (data == null)
            {
                MaxHealth = 1;
                Health = 1;
                ScoreValue = 10;
                FoodChance = 0.1f;
                GiftChance = 0f;
                _baseColor = Color.white;
            }
            else
            {
                float safeDifficulty = Mathf.Max(0.5f, difficulty);
                MaxHealth = Mathf.Max(1, Mathf.RoundToInt(data.maxHealth * safeDifficulty));
                Health = MaxHealth;
                ScoreValue = Mathf.RoundToInt(data.score * (0.85f + 0.15f * safeDifficulty));
                FoodChance = data.foodChance;
                GiftChance = data.giftChance;
                _baseColor = data.tint;

                if (spriteRenderer != null && data.sprite != null)
                {
                    SpriteAnimator animator = GetComponent<SpriteAnimator>();
                    if (animator == null || !animator.HasFrames)
                    {
                        spriteRenderer.sprite = data.sprite;
                    }
                }
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = _baseColor;
            }
        }

        public void ConfigureAsteroid(int health, int score, Sprite sprite = null)
        {
            IsAsteroid = true;
            MaxHealth = Mathf.Max(1, health);
            Health = MaxHealth;
            ScoreValue = Mathf.Max(1, score);
            FoodChance = 0f;
            GiftChance = 0f;
            _foodPrefab = null;
            _giftPrefab = null;
            _baseColor = Color.white;

            if (spriteRenderer != null)
            {
                if (sprite != null)
                {
                    spriteRenderer.sprite = sprite;
                }

                spriteRenderer.color = Color.white;
            }
        }

        public void TakeDamage(int amount, GameObject source = null)
        {
            if (_dead || amount <= 0)
            {
                return;
            }

            Health -= amount;

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(FlashRoutine());

            if (Health <= 0)
            {
                Die();
            }
        }

        public void ApplyBombDamage(float fractionOfMaxHealth)
        {
            TakeDamage(MaxHealth);
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer == null)
            {
                yield break;
            }

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.06f);

            if (!_dead && spriteRenderer != null)
            {
                spriteRenderer.color = _baseColor;
            }

            _flashRoutine = null;
        }

        private void Die()
        {
            if (_dead)
            {
                return;
            }

            _dead = true;

            float size = IsAsteroid ? 1.5f : 1f;
            GameEvents.RaiseEnemyDestroyed(transform.position, size);

            GameManager manager = GameManager.Instance;
            manager.AddScore(ScoreValue);

            PoolManager pool = manager.Pool;
            if (pool != null)
            {
                Vector3 dropPosition = transform.position + (Vector3)(Random.insideUnitCircle * 0.15f);

                if (_foodPrefab != null && Random.value <= FoodChance)
                {
                    pool.Spawn(_foodPrefab, dropPosition, Quaternion.identity);
                }

                if (_giftPrefab != null && Random.value <= GiftChance)
                {
                    pool.Spawn(_giftPrefab, dropPosition, Quaternion.identity);
                }
            }

            Despawn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_dead)
            {
                return;
            }

            PlayerShip player = other.GetComponentInParent<PlayerShip>();
            if (player == null)
            {
                return;
            }

            player.TakeDamage(1, gameObject);

            if (!_dead)
            {
                TakeDamage(MaxHealth);
            }
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
