using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Additive camera shake driven by gameplay events. Never fights the level
    /// controller because it only offsets local position and always returns to
    /// its captured origin.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        private static CameraShake _instance;

        private Vector3 _origin;
        private float _strength;
        private float _duration;
        private float _remaining;
        private float _seed;

        private void Awake()
        {
            _instance = this;
            _origin = transform.localPosition;
            _seed = Random.value * 128f;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnEnable()
        {
            GameEvents.ShakeRequested += HandleShake;
        }

        private void OnDisable()
        {
            GameEvents.ShakeRequested -= HandleShake;
        }

        public static void Shake(float strength, float duration)
        {
            if (_instance != null)
            {
                _instance.HandleShake(strength, duration);
            }
        }

        private void HandleShake(float strength, float duration)
        {
            if (strength <= 0f || duration <= 0f)
            {
                return;
            }

            _strength = Mathf.Max(_strength, strength);
            _duration = Mathf.Max(_duration, duration);
            _remaining = Mathf.Max(_remaining, duration);
        }

        private void LateUpdate()
        {
            if (_remaining <= 0f)
            {
                transform.localPosition = _origin;
                _strength = 0f;
                return;
            }

            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
            float falloff = _duration > 0f ? _remaining / _duration : 0f;
            float time = Time.time * 26f;

            float x = (Mathf.PerlinNoise(_seed, time) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(_seed + 31f, time) - 0.5f) * 2f;

            transform.localPosition = _origin + new Vector3(x, y, 0f) * (_strength * falloff * falloff);
        }
    }
}
