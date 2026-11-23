using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Simple drifting, destructible asteroid used by asteroid rounds.
    /// </summary>
    [RequireComponent(typeof(EnemyUnit))]
    public class Asteroid : MonoBehaviour
    {
        public EnemyUnit unit;
        public Sprite[] variants;

        private float _fallSpeed;
        private float _spinSpeed;

        private void Awake()
        {
            if (unit == null) unit = GetComponent<EnemyUnit>();
        }

        public void Initialize(int health, int score, float fallSpeed, Vector2 sizeRange)
        {
            Sprite variant = variants != null && variants.Length > 0
                ? variants[Random.Range(0, variants.Length)]
                : null;

            unit.ConfigureAsteroid(health, score, variant);

            _fallSpeed = fallSpeed * Random.Range(0.85f, 1.2f);
            _spinSpeed = Random.Range(-90f, 90f);

            float size = Random.Range(sizeRange.x, sizeRange.y);
            transform.localScale = Vector3.one * size;
        }

        private void FixedUpdate()
        {
            transform.position += Vector3.down * (_fallSpeed * Time.fixedDeltaTime);
            transform.Rotate(0f, 0f, _spinSpeed * Time.fixedDeltaTime);

            Camera camera = Camera.main;
            if (camera != null && transform.position.y < CameraBounds.GetVisibleBounds(camera, 2f).min.y)
            {
                Despawn();
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
