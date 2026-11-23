using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Endless vertical scroller. Owns two stacked copies of a seamless tile and
    /// recycles them, so the camera never sees an edge. Horizontal offset follows
    /// the camera slightly to add depth.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxLayer : MonoBehaviour
    {
        public float scrollSpeed = 0.15f;
        public float horizontalFactor = 0.01f;
        public float tileHeight = 15f;

        private SpriteRenderer _renderer;
        private Camera _camera;
        private Vector3 _start;
        private float _offsetX;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _start = transform.position;

            GameObject copy = new GameObject("Wrap");
            copy.transform.SetParent(transform, false);
            copy.transform.localPosition = new Vector3(0f, tileHeight, 0f);

            SpriteRenderer copyRenderer = copy.AddComponent<SpriteRenderer>();
            copyRenderer.sprite = _renderer.sprite;
            copyRenderer.sortingOrder = _renderer.sortingOrder;
            copyRenderer.color = _renderer.color;
            copyRenderer.sharedMaterial = _renderer.sharedMaterial;
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            float delta = Time.deltaTime;
            Vector3 position = transform.position;
            position.y -= scrollSpeed * delta;

            if (position.y <= _start.y - tileHeight)
            {
                position.y += tileHeight;
            }

            if (_camera != null && (horizontalFactor > 0.0001f || Mathf.Abs(_offsetX) > 0.0001f))
            {
                float targetX = _camera.transform.position.x * horizontalFactor;
                _offsetX = Mathf.Lerp(_offsetX, targetX, 1f - Mathf.Exp(-4f * delta));
                position.x = _start.x + _offsetX;
            }

            transform.position = position;
        }
    }
}
