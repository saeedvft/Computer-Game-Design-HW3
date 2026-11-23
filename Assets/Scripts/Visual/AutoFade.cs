using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Short-lived visual burst: scales and fades a sprite then destroys it.
    /// Used for muzzle flashes, shockwaves and pickup sparkles.
    /// </summary>
    public class AutoFade : MonoBehaviour
    {
        public SpriteRenderer target;
        public float lifetime = 0.2f;
        public float startScale = 1f;
        public float endScale = 1.4f;
        public float spinSpeed;
        public bool useUnscaledTime;

        private Color _color = Color.white;
        private float _age;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<SpriteRenderer>();
            }

            if (target != null)
            {
                _color = target.color;
            }
        }

        private void OnEnable()
        {
            _age = 0f;
            transform.localScale = Vector3.one * startScale;

            if (target != null)
            {
                target.color = _color;
            }
        }

        private void Update()
        {
            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _age += delta;

            float t = Mathf.Clamp01(_age / Mathf.Max(0.0001f, lifetime));
            float eased = 1f - (1f - t) * (1f - t);

            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);

            if (spinSpeed != 0f)
            {
                transform.Rotate(0f, 0f, spinSpeed * delta);
            }

            if (target != null)
            {
                Color color = _color;
                color.a = _color.a * (1f - t);
                target.color = color;
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
