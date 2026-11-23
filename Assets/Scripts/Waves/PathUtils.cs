using UnityEngine;

namespace ChickenShooter
{
    public static class PathUtils
    {
        /// <summary>
        /// Catmull-Rom evaluation through an arbitrary number of waypoints.
        /// Produces a smooth curved entry path instead of straight lines.
        /// </summary>
        public static Vector3 CatmullRom(Vector3[] points, float t)
        {
            if (points == null || points.Length == 0)
            {
                return Vector3.zero;
            }

            if (points.Length == 1)
            {
                return points[0];
            }

            if (points.Length == 2)
            {
                return Vector3.Lerp(points[0], points[1], Mathf.Clamp01(t));
            }

            float scaled = Mathf.Clamp01(t) * (points.Length - 1);
            int segment = Mathf.Min(Mathf.FloorToInt(scaled), points.Length - 2);
            float localT = scaled - segment;

            Vector3 p0 = points[Mathf.Max(segment - 1, 0)];
            Vector3 p1 = points[segment];
            Vector3 p2 = points[segment + 1];
            Vector3 p3 = points[Mathf.Min(segment + 2, points.Length - 1)];

            float t2 = localT * localT;
            float t3 = t2 * localT;

            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * localT +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }
    }
}
