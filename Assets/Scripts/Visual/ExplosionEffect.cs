using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Plays a one-shot sprite animation and removes itself. Used for cartoon
    /// explosions that read better than particle smoke at this art scale.
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        public SpriteRenderer target;
        public Sprite[] frames;
        public float framesPerSecond = 18f;
        public float lifetime = 0.55f;
        public bool spin;

        private float _age;
        private int _index = -1;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            _age = 0f;
            _index = -1;

            if (spin)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            }

            Apply(0f);
        }

        private void Update()
        {
            _age += Time.deltaTime;
            Apply(_age);

            if (_age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void Apply(float age)
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            float t = Mathf.Clamp01(age / Mathf.Max(0.0001f, lifetime));
            int index = Mathf.Min(frames.Length - 1, Mathf.FloorToInt(t * frames.Length));
            if (index != _index)
            {
                _index = index;
                if (target != null)
                {
                    target.sprite = frames[index];
                }
            }
        }
    }
}
