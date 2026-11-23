using System.Collections.Generic;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Pure layout math for formations. Keeping this static makes the patterns
    /// testable and independent from spawning, pooling and AI.
    /// </summary>
    public static class FormationBuilder
    {
        public static List<Vector2> Build(WaveDataSO wave)
        {
            if (wave == null || wave.count <= 0)
            {
                return new List<Vector2>();
            }

            switch (wave.formation)
            {
                case FormationType.VShape:
                    return BuildVShape(wave.count, wave.spacing);
                case FormationType.Diamond:
                    return BuildDiamond(wave.count, wave.spacing);
                case FormationType.Arc:
                    return BuildArc(wave.count, wave.spacing);
                case FormationType.DenseGrid:
                    return BuildGrid(wave.count, wave.columns, wave.spacing * 0.88f);
                default:
                    return BuildGrid(wave.count, wave.columns, wave.spacing);
            }
        }

        private static List<Vector2> BuildGrid(int count, int columns, Vector2 spacing)
        {
            columns = Mathf.Clamp(columns, 1, count);
            var result = new List<Vector2>(count);

            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int column = i % columns;
                int rowCount = Mathf.Min(columns, count - row * columns);
                float x = (column - (rowCount - 1) * 0.5f) * spacing.x;
                float y = -row * spacing.y;
                result.Add(new Vector2(x, y));
            }

            CenterVertically(result, spacing.y);
            return result;
        }

        private static List<Vector2> BuildVShape(int count, Vector2 spacing)
        {
            var result = new List<Vector2>(count);
            int pairs = (count + 1) / 2;

            for (int i = 0; i < count; i++)
            {
                int arm = i % 2 == 0 ? -1 : 1;
                int step = i / 2;
                float x = arm * (step + 0.5f) * spacing.x * 0.6f;
                float y = step * spacing.y * 0.8f;
                result.Add(new Vector2(x, y));
            }

            CenterVertically(result, spacing.y);
            return result;
        }

        private static List<Vector2> BuildDiamond(int count, Vector2 spacing)
        {
            var result = new List<Vector2> { Vector2.zero };
            int radius = 1;

            while (result.Count < count)
            {
                for (int x = -radius; x <= radius && result.Count < count; x++)
                {
                    int remainder = radius - Mathf.Abs(x);
                    result.Add(new Vector2(x, remainder) * spacing);
                    if (remainder != 0 && result.Count < count)
                    {
                        result.Add(new Vector2(x, -remainder) * spacing);
                    }
                }

                radius++;
            }

            return result;
        }

        private static List<Vector2> BuildArc(int count, Vector2 spacing)
        {
            var result = new List<Vector2>(count);
            float radius = Mathf.Max(2f, count * spacing.x * 0.22f);

            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                float angle = Mathf.Lerp(-70f, 70f, t) * Mathf.Deg2Rad;
                float x = Mathf.Sin(angle) * radius;
                float y = Mathf.Cos(angle) * radius - radius + 1.2f;
                result.Add(new Vector2(x, y));
            }

            return result;
        }

        private static void CenterVertically(List<Vector2> points, float spacingY)
        {
            if (points.Count == 0)
            {
                return;
            }

            float minY = float.MaxValue;
            float maxY = float.MinValue;
            foreach (Vector2 point in points)
            {
                minY = Mathf.Min(minY, point.y);
                maxY = Mathf.Max(maxY, point.y);
            }

            float offset = -(minY + maxY) * 0.5f;
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Vector2(points[i].x, points[i].y + offset);
            }
        }
    }
}
