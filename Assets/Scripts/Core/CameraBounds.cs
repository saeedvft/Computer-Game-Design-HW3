using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Helper for clamping gameplay to the visible orthographic camera area.
    /// </summary>
    public static class CameraBounds
    {
        public static Bounds GetVisibleBounds(Camera camera, float padding = 0f)
        {
            if (camera == null)
            {
                return new Bounds(Vector3.zero, new Vector3(20f, 12f, 1f));
            }

            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * camera.aspect;
            Vector3 center = camera.transform.position;
            Vector3 size = new Vector3(
                Mathf.Max(0.1f, halfWidth * 2f + padding * 2f),
                Mathf.Max(0.1f, halfHeight * 2f + padding * 2f),
                1f);
            return new Bounds(center, size);
        }

        public static bool IsOutside(Camera camera, Vector3 position, float margin)
        {
            Bounds bounds = GetVisibleBounds(camera, margin);
            return position.x < bounds.min.x ||
                   position.x > bounds.max.x ||
                   position.y < bounds.min.y ||
                   position.y > bounds.max.y;
        }
    }
}
