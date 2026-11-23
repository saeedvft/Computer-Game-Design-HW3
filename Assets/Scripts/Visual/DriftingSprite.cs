using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Slow decorative drift used by the main menu. Wraps around the camera
    /// bounds and bobs vertically, so the menu never feels static.
    /// </summary>
    public class DriftingSprite : MonoBehaviour
    {
        public float speed = 1.4f;
        public float bobAmplitude = 0.4f;
        public float bobSpeed = 1.5f;

        private float _baseY;
        private float _time;
        private float _minX;
        private float _maxX;

        private void Start()
        {
            _baseY = transform.position.y;

            Camera camera = Camera.main;
            if (camera != null)
            {
                Bounds bounds = CameraBounds.GetVisibleBounds(camera, 2.4f);
                _minX = bounds.min.x;
                _maxX = bounds.max.x;
            }
            else
            {
                _minX = -12f;
                _maxX = 12f;
            }

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, _minX, _maxX);
            transform.position = position;
        }

        private void Update()
        {
            _time += Time.deltaTime;

            Vector3 position = transform.position;
            position.x += speed * Time.deltaTime;

            if (speed >= 0f && position.x > _maxX)
            {
                position.x = _minX;
            }
            else if (speed < 0f && position.x < _minX)
            {
                position.x = _maxX;
            }

            position.y = _baseY + Mathf.Sin(_time * bobSpeed) * bobAmplitude;
            transform.position = position;
        }
    }
}
