using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Lightweight frame-swap animation. Used for wing flaps, engine flames and
    /// explosions without paying for Animator controllers or animation clips.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpriteAnimator : MonoBehaviour
    {
        public SpriteRenderer target;
        public Sprite[] frames;
        public float framesPerSecond = 8f;
        public bool pingPong;
        public bool randomizeStart = true;
        public bool useUnscaledTime;

        private float _timer;
        private int _index;
        private int _direction = 1;

        public bool HasFrames => frames != null && frames.Length > 0;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            _timer = 0f;
            _index = 0;
            _direction = 1;

            if (randomizeStart && frames != null && frames.Length > 1)
            {
                _index = Random.Range(0, frames.Length);
            }

            Apply();
        }

        private void Update()
        {
            if (target == null || frames == null || frames.Length < 2 || framesPerSecond <= 0f)
            {
                return;
            }

            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += delta * framesPerSecond;

            while (_timer >= 1f)
            {
                _timer -= 1f;
                Advance();
            }
        }

        private void Advance()
        {
            if (pingPong)
            {
                _index += _direction;
                if (_index >= frames.Length - 1)
                {
                    _index = frames.Length - 1;
                    _direction = -1;
                }
                else if (_index <= 0)
                {
                    _index = 0;
                    _direction = 1;
                }
            }
            else
            {
                _index = (_index + 1) % frames.Length;
            }

            Apply();
        }

        private void Apply()
        {
            if (target != null && frames != null && frames.Length > 0)
            {
                target.sprite = frames[Mathf.Clamp(_index, 0, frames.Length - 1)];
            }
        }
    }
}
