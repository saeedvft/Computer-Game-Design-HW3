using UnityEngine;

namespace ChickenShooter.EditorTools
{
    /// <summary>
    /// Tiny software rasteriser used by the asset generator. Everything is drawn
    /// at 4x resolution in float coordinates and box-filtered down, which gives
    /// smooth anti-aliased cartoon art without any external textures.
    /// </summary>
    public sealed class ArtCanvas
    {
        public readonly int Width;
        public readonly int Height;

        private readonly int _scale;
        private readonly Color[] _pixels;

        public ArtCanvas(int width, int height, int scale = 4)
        {
            Width = width;
            Height = height;
            _scale = Mathf.Max(1, scale);
            _pixels = new Color[width * _scale * height * _scale];
        }

        private int HiWidth => Width * _scale;
        private int HiHeight => Height * _scale;

        // ------------------------------------------------------------------
        // Fills
        // ------------------------------------------------------------------

        public void Fill(Color color)
        {
            for (int i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = color;
            }
        }

        public void FillRect(float x, float y, float width, float height, Color color)
        {
            int minX = Mathf.FloorToInt(x * _scale);
            int maxX = Mathf.CeilToInt((x + width) * _scale);
            int minY = Mathf.FloorToInt(y * _scale);
            int maxY = Mathf.CeilToInt((y + height) * _scale);

            for (int py = minY; py < maxY; py++)
            {
                for (int px = minX; px < maxX; px++)
                {
                    BlendAt(px, py, color);
                }
            }
        }

        public void FillRectGradient(float x, float y, float width, float height, Color top, Color bottom)
        {
            int minX = Mathf.FloorToInt(x * _scale);
            int maxX = Mathf.CeilToInt((x + width) * _scale);
            int minY = Mathf.FloorToInt(y * _scale);
            int maxY = Mathf.CeilToInt((y + height) * _scale);

            for (int py = minY; py < maxY; py++)
            {
                float t = Mathf.InverseLerp(minY, maxY, py);
                Color row = Color.Lerp(bottom, top, t);

                for (int px = minX; px < maxX; px++)
                {
                    BlendAt(px, py, row);
                }
            }
        }

        public void FillRoundedRect(float x, float y, float width, float height, float radius, Color color)
        {
            float r = Mathf.Min(radius, Mathf.Min(width, height) * 0.5f);
            FillRect(x + r, y, width - 2f * r, height, color);
            FillRect(x, y + r, r, height - 2f * r, color);
            FillRect(x + width - r, y + r, r, height - 2f * r, color);
            FillCircle(x + r, y + r, r, color);
            FillCircle(x + width - r, y + r, r, color);
            FillCircle(x + r, y + height - r, r, color);
            FillCircle(x + width - r, y + height - r, r, color);
        }

        public void FillRoundedRectGradient(float x, float y, float width, float height, float radius, Color top, Color bottom)
        {
            float r = Mathf.Min(radius, Mathf.Min(width, height) * 0.5f);
            int minX = Mathf.FloorToInt(x * _scale);
            int maxX = Mathf.CeilToInt((x + width) * _scale);
            int minY = Mathf.FloorToInt(y * _scale);
            int maxY = Mathf.CeilToInt((y + height) * _scale);

            for (int py = minY; py < maxY; py++)
            {
                float fy = (py + 0.5f) / _scale;
                float t = Mathf.InverseLerp(y, y + height, fy);
                Color row = Color.Lerp(bottom, top, t);

                for (int px = minX; px < maxX; px++)
                {
                    float fx = (px + 0.5f) / _scale;
                    if (!InsideRoundedRect(fx, fy, x, y, width, height, r))
                    {
                        continue;
                    }

                    BlendAt(px, py, row);
                }
            }
        }

        public void FillCircle(float cx, float cy, float radius, Color color)
        {
            FillEllipse(cx, cy, radius, radius, color);
        }

        public void FillCircleGradient(float cx, float cy, float radius, Color top, Color bottom)
        {
            FillEllipseGradient(cx, cy, radius, radius, top, bottom);
        }

        public void FillEllipse(float cx, float cy, float rx, float ry, Color color)
        {
            if (rx <= 0f || ry <= 0f)
            {
                return;
            }

            int minX = Mathf.FloorToInt((cx - rx) * _scale);
            int maxX = Mathf.CeilToInt((cx + rx) * _scale);
            int minY = Mathf.FloorToInt((cy - ry) * _scale);
            int maxY = Mathf.CeilToInt((cy + ry) * _scale);

            float invRx = 1f / rx;
            float invRy = 1f / ry;

            for (int py = minY; py <= maxY; py++)
            {
                float dy = ((py + 0.5f) / _scale - cy) * invRy;

                for (int px = minX; px <= maxX; px++)
                {
                    float dx = ((px + 0.5f) / _scale - cx) * invRx;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        BlendAt(px, py, color);
                    }
                }
            }
        }

        public void FillEllipseGradient(float cx, float cy, float rx, float ry, Color top, Color bottom)
        {
            int minX = Mathf.FloorToInt((cx - rx) * _scale);
            int maxX = Mathf.CeilToInt((cx + rx) * _scale);
            int minY = Mathf.FloorToInt((cy - ry) * _scale);
            int maxY = Mathf.CeilToInt((cy + ry) * _scale);

            float invRx = 1f / rx;
            float invRy = 1f / ry;

            for (int py = minY; py <= maxY; py++)
            {
                float fy = (py + 0.5f) / _scale;
                float dy = (fy - cy) * invRy;
                Color row = Color.Lerp(bottom, top, Mathf.InverseLerp(cy - ry, cy + ry, fy));

                for (int px = minX; px <= maxX; px++)
                {
                    float dx = ((px + 0.5f) / _scale - cx) * invRx;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        BlendAt(px, py, row);
                    }
                }
            }
        }

        public void FillRotatedEllipse(float cx, float cy, float rx, float ry, float degrees, Color color)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            float extent = Mathf.Max(rx, ry);
            float invRx = 1f / rx;
            float invRy = 1f / ry;

            int minX = Mathf.FloorToInt((cx - extent) * _scale);
            int maxX = Mathf.CeilToInt((cx + extent) * _scale);
            int minY = Mathf.FloorToInt((cy - extent) * _scale);
            int maxY = Mathf.CeilToInt((cy + extent) * _scale);

            for (int py = minY; py <= maxY; py++)
            {
                float dy = (py + 0.5f) / _scale - cy;

                for (int px = minX; px <= maxX; px++)
                {
                    float dx = (px + 0.5f) / _scale - cx;
                    float localX = dx * cos + dy * sin;
                    float localY = -dx * sin + dy * cos;

                    if (localX * localX * invRx * invRx + localY * localY * invRy * invRy <= 1f)
                    {
                        BlendAt(px, py, color);
                    }
                }
            }
        }

        public void FillCapsule(float x1, float y1, float x2, float y2, float radius, Color color)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = Mathf.Sqrt(dx * dx + dy * dy);
            int steps = Mathf.Max(1, Mathf.CeilToInt(length * _scale * 1.2f));

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                FillCircle(Mathf.Lerp(x1, x2, t), Mathf.Lerp(y1, y2, t), radius, color);
            }
        }

        public void FillTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            FillPolygon(new[] { a, b, c }, color);
        }

        public void FillPolygon(Vector2[] points, Color color)
        {
            if (points == null || points.Length < 3)
            {
                return;
            }

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                minY = Mathf.Min(minY, points[i].y);
                maxY = Mathf.Max(maxY, points[i].y);
            }

            int y0 = Mathf.Max(0, Mathf.FloorToInt(minY * _scale));
            int y1 = Mathf.Min(HiHeight - 1, Mathf.CeilToInt(maxY * _scale));

            for (int py = y0; py <= y1; py++)
            {
                float scanY = (py + 0.5f) / _scale;
                if (!CollectSpans(points, scanY, out float start, out float end))
                {
                    continue;
                }

                int x0 = Mathf.Max(0, Mathf.FloorToInt(start * _scale));
                int x1 = Mathf.Min(HiWidth - 1, Mathf.CeilToInt(end * _scale));

                for (int px = x0; px <= x1; px++)
                {
                    BlendAt(px, py, color);
                }
            }
        }

        public void FillPolygonGradient(Vector2[] points, Color top, Color bottom)
        {
            if (points == null || points.Length < 3)
            {
                return;
            }

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                minY = Mathf.Min(minY, points[i].y);
                maxY = Mathf.Max(maxY, points[i].y);
            }

            int y0 = Mathf.Max(0, Mathf.FloorToInt(minY * _scale));
            int y1 = Mathf.Min(HiHeight - 1, Mathf.CeilToInt(maxY * _scale));

            for (int py = y0; py <= y1; py++)
            {
                float scanY = (py + 0.5f) / _scale;
                if (!CollectSpans(points, scanY, out float start, out float end))
                {
                    continue;
                }

                Color row = Color.Lerp(bottom, top, Mathf.InverseLerp(minY, maxY, scanY));
                int x0 = Mathf.Max(0, Mathf.FloorToInt(start * _scale));
                int x1 = Mathf.Min(HiWidth - 1, Mathf.CeilToInt(end * _scale));

                for (int px = x0; px <= x1; px++)
                {
                    BlendAt(px, py, row);
                }
            }
        }

        public void StrokePolygon(Vector2[] points, float width, Color color, bool closed = true)
        {
            if (points == null || points.Length < 2)
            {
                return;
            }

            int count = closed ? points.Length : points.Length - 1;
            for (int i = 0; i < count; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[(i + 1) % points.Length];
                FillCapsule(a.x, a.y, b.x, b.y, width * 0.5f, color);
            }
        }

        // ------------------------------------------------------------------
        // Effects
        // ------------------------------------------------------------------

        /// <summary>Soft radial light. Alpha falls off from the centre.</summary>
        public void Radiance(float cx, float cy, float radius, Color color, float power = 2.2f)
        {
            int minX = Mathf.FloorToInt((cx - radius) * _scale);
            int maxX = Mathf.CeilToInt((cx + radius) * _scale);
            int minY = Mathf.FloorToInt((cy - radius) * _scale);
            int maxY = Mathf.CeilToInt((cy + radius) * _scale);

            for (int py = minY; py <= maxY; py++)
            {
                float dy = (py + 0.5f) / _scale - cy;

                for (int px = minX; px <= maxX; px++)
                {
                    float dx = (px + 0.5f) / _scale - cx;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    if (distance >= 1f)
                    {
                        continue;
                    }

                    Color tinted = color;
                    tinted.a *= Mathf.Pow(1f - distance, power);
                    BlendAt(px, py, tinted);
                }
            }
        }

        /// <summary>Additive glow that brightens whatever is underneath.</summary>
        public void AdditiveGlow(float cx, float cy, float radius, Color color, float power = 2f)
        {
            int minX = Mathf.FloorToInt((cx - radius) * _scale);
            int maxX = Mathf.CeilToInt((cx + radius) * _scale);
            int minY = Mathf.FloorToInt((cy - radius) * _scale);
            int maxY = Mathf.CeilToInt((cy + radius) * _scale);

            for (int py = minY; py <= maxY; py++)
            {
                float dy = (py + 0.5f) / _scale - cy;

                for (int px = minX; px <= maxX; px++)
                {
                    float dx = (px + 0.5f) / _scale - cx;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    if (distance >= 1f)
                    {
                        continue;
                    }

                    int index = py * HiWidth + px;
                    if (index < 0 || index >= _pixels.Length)
                    {
                        continue;
                    }

                    float intensity = Mathf.Pow(1f - distance, power) * color.a;
                    Color current = _pixels[index];
                    current.r = Mathf.Min(1f, current.r + color.r * intensity);
                    current.g = Mathf.Min(1f, current.g + color.g * intensity);
                    current.b = Mathf.Min(1f, current.b + color.b * intensity);
                    current.a = Mathf.Max(current.a, intensity);
                    _pixels[index] = current;
                }
            }
        }

        public void AddNoise(float amount, int seed)
        {
            if (amount <= 0f)
            {
                return;
            }

            System.Random random = new System.Random(seed);

            for (int i = 0; i < _pixels.Length; i++)
            {
                Color color = _pixels[i];
                if (color.a <= 0.01f)
                {
                    continue;
                }

                float noise = ((float)random.NextDouble() - 0.5f) * amount;
                color.r = Mathf.Clamp01(color.r + noise);
                color.g = Mathf.Clamp01(color.g + noise);
                color.b = Mathf.Clamp01(color.b + noise);
                _pixels[i] = color;
            }
        }

        public void Vignette(float inner, float outer, Color color)
        {
            int width = HiWidth;
            int height = HiHeight;
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float maxDistance = Mathf.Sqrt(halfW * halfW + halfH * halfH);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - halfW) / maxDistance;
                    float dy = (y - halfH) / maxDistance;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.InverseLerp(inner, outer, distance);
                    if (alpha <= 0f)
                    {
                        continue;
                    }

                    Color tinted = color;
                    tinted.a *= alpha;
                    BlendAt(x, y, tinted);
                }
            }
        }

        public Color Sample(float x, float y)
        {
            int px = Mathf.Clamp(Mathf.RoundToInt(x * _scale), 0, HiWidth - 1);
            int py = Mathf.Clamp(Mathf.RoundToInt(y * _scale), 0, HiHeight - 1);
            return _pixels[py * HiWidth + px];
        }

        // ------------------------------------------------------------------
        // Export
        // ------------------------------------------------------------------

        public Texture2D ToTexture(string name = "Art")
        {
            Texture2D texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] output = new Color[Width * Height];
            int scale = _scale;
            float samples = scale * scale;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    float r = 0f;
                    float g = 0f;
                    float b = 0f;
                    float a = 0f;

                    for (int sy = 0; sy < scale; sy++)
                    {
                        int py = y * scale + sy;

                        for (int sx = 0; sx < scale; sx++)
                        {
                            Color pixel = _pixels[py * HiWidth + x * scale + sx];
                            r += pixel.r * pixel.a;
                            g += pixel.g * pixel.a;
                            b += pixel.b * pixel.a;
                            a += pixel.a;
                        }
                    }

                    Color result;
                    if (a <= 0.0001f)
                    {
                        result = new Color(0f, 0f, 0f, 0f);
                    }
                    else
                    {
                        result = new Color(r / a, g / a, b / a, a / samples);
                    }

                    output[y * Width + x] = result;
                }
            }

            texture.SetPixels(output);
            texture.Apply();
            return texture;
        }

        // ------------------------------------------------------------------
        // Internals
        // ------------------------------------------------------------------

        private static bool InsideRoundedRect(float x, float y, float rx, float ry, float width, float height, float radius)
        {
            if (x < rx || x > rx + width || y < ry || y > ry + height)
            {
                return false;
            }

            float nearestX = Mathf.Clamp(x, rx + radius, rx + width - radius);
            float nearestY = Mathf.Clamp(y, ry + radius, ry + height - radius);
            float dx = x - nearestX;
            float dy = y - nearestY;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static bool CollectSpans(Vector2[] points, float y, out float start, out float end)
        {
            start = float.MaxValue;
            end = float.MinValue;
            bool inside = false;

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[(i + 1) % points.Length];

                if ((a.y <= y && b.y > y) || (b.y <= y && a.y > y))
                {
                    float t = (y - a.y) / (b.y - a.y);
                    float x = a.x + (b.x - a.x) * t;
                    start = Mathf.Min(start, x);
                    end = Mathf.Max(end, x);
                    inside = true;
                }
            }

            return inside;
        }

        private void BlendAt(int x, int y, Color color)
        {
            if (color.a <= 0f || x < 0 || y < 0 || x >= HiWidth || y >= HiHeight)
            {
                return;
            }

            int index = y * HiWidth + x;
            Color destination = _pixels[index];
            float alpha = color.a + destination.a * (1f - color.a);

            if (alpha <= 0.0001f)
            {
                _pixels[index] = Color.clear;
                return;
            }

            float r = (color.r * color.a + destination.r * destination.a * (1f - color.a)) / alpha;
            float g = (color.g * color.a + destination.g * destination.a * (1f - color.a)) / alpha;
            float b = (color.b * color.a + destination.b * destination.a * (1f - color.a)) / alpha;
            _pixels[index] = new Color(r, g, b, alpha);
        }
    }
}
