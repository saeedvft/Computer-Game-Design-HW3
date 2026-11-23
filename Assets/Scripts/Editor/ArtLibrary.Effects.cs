using System;
using UnityEngine;
using Random = System.Random;

namespace ChickenShooter.EditorTools
{
    public static partial class ArtLibrary
    {
        // ------------------------------------------------------------------
        // Explosions and effects
        // ------------------------------------------------------------------

        private static Sprite[] BuildExplosion(string name, int count, float maxRadius)
        {
            Sprite[] frames = new Sprite[count];

            for (int i = 0; i < count; i++)
            {
                ArtCanvas canvas = new ArtCanvas(SpriteSize, SpriteSize, 4);
                DrawExplosionFrame(canvas, i, count, maxRadius);
                frames[i] = Save(canvas, $"{name}_{i}");
            }

            return frames;
        }

        private static void DrawExplosionFrame(ArtCanvas canvas, int index, int count, float maxRadius)
        {
            float t = count <= 1 ? 0f : index / (float)(count - 1);
            float grow = 1f - Mathf.Pow(1f - t, 2.2f);
            float radius = Mathf.Lerp(maxRadius * 0.34f, maxRadius, grow);
            float alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1f, t));

            if (alpha <= 0.01f)
            {
                return;
            }

            Random random = new Random(101 + index * 37);
            Vector2 center = V(96f, 96f);

            Vector2[] GenerateBlob(float r, int points, float jitter)
            {
                Vector2[] blob = new Vector2[points];
                for (int i = 0; i < points; i++)
                {
                    float angle = i / (float)points * Mathf.PI * 2f;
                    float wobble = 1f + ((float)random.NextDouble() - 0.5f) * jitter;
                    blob[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r * wobble;
                }

                return blob;
            }

            // Smoke ring behind the fireball.
            if (t > 0.45f)
            {
                float smokeT = Mathf.InverseLerp(0.45f, 1f, t);
                float smokeRadius = Mathf.Lerp(radius * 0.9f, maxRadius * 1.18f, smokeT);
                Color smoke = A(Hex("#58607A"), 0.25f * (1f - smokeT));
                DrawRing(canvas, center.x, center.y, smokeRadius, 10f, smoke);
            }

            // Outer fireball.
            Vector2[] outer = GenerateBlob(radius, 14, 0.38f);
            canvas.FillPolygonGradient(outer, A(Hex("#FF8A3D"), alpha), A(Hex("#B02330"), alpha * 0.9f));
            canvas.Radiance(center.x, center.y, radius * 0.95f, A(Hex("#E8453C"), alpha * 0.85f), 1.4f);

            // Middle flare.
            Vector2[] middle = GenerateBlob(radius * 0.66f, 11, 0.34f);
            canvas.FillPolygonGradient(middle, A(Hex("#FFD166"), alpha), A(Hex("#FF8A3D"), alpha));

            // Hot core.
            Vector2[] core = GenerateBlob(radius * 0.38f, 9, 0.3f);
            canvas.FillPolygonGradient(core, A(Hex("#FFFFFF"), alpha), A(Hex("#FFF3B0"), alpha));

            // Sparks thrown outward.
            int sparks = 12;
            for (int i = 0; i < sparks; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float distance = radius * (1.05f + (float)random.NextDouble() * 0.35f);
                float x = center.x + Mathf.Cos(angle) * distance;
                float y = center.y + Mathf.Sin(angle) * distance;
                float size = 1.6f + (float)random.NextDouble() * 2.8f;
                canvas.FillCircle(x, y, size, A(Hex("#FFD166"), alpha * 0.9f));
                canvas.AdditiveGlow(x, y, size * 3f, A(Hex("#FF8A3D"), alpha * 0.5f), 2f);
            }

            // Central flash.
            canvas.AdditiveGlow(center.x, center.y, radius * 1.2f, A(Hex("#FFF3B0"), alpha * 0.5f), 2.6f);
        }

        private static void DrawRing(ArtCanvas canvas, float cx, float cy, float radius, float width, Color color)
        {
            const int segments = 48;
            Vector2[] ring = new Vector2[segments];

            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                ring[i] = new Vector2(cx + Mathf.Cos(angle) * radius, cy + Mathf.Sin(angle) * radius);
            }

            canvas.StrokePolygon(ring, width, color);
        }

        private static Sprite BuildMuzzle()
        {
            ArtCanvas canvas = new ArtCanvas(96, 96, 4);
            Vector2 center = V(48f, 48f);

            // Eight-point star.
            Vector2[] star = new Vector2[16];
            for (int i = 0; i < star.Length; i++)
            {
                float angle = i / (float)star.Length * Mathf.PI * 2f;
                float radius = i % 2 == 0 ? 40f : 13f;
                star[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            canvas.FillPolygon(star, A(Hex("#35D0FF"), 0.9f));
            canvas.Radiance(48f, 48f, 34f, A(Hex("#BFF4FF"), 0.95f), 1.6f);
            canvas.FillCircle(48f, 48f, 14f, Hex("#FFFFFF"));
            canvas.AdditiveGlow(48f, 48f, 40f, A(Hex("#35D0FF"), 0.8f), 2.2f);

            return Save(canvas, "Muzzle");
        }

        private static Sprite BuildShockwave()
        {
            ArtCanvas canvas = new ArtCanvas(256, 256, 4);

            DrawRing(canvas, 128f, 128f, 100f, 16f, A(Hex("#35D0FF"), 0.18f));
            DrawRing(canvas, 128f, 128f, 100f, 8f, A(Hex("#7FE3FF"), 0.8f));
            DrawRing(canvas, 128f, 128f, 100f, 3f, Hex("#FFFFFF"));
            DrawRing(canvas, 128f, 128f, 78f, 2f, A(Hex("#BFF4FF"), 0.4f));

            canvas.AdditiveGlow(128f, 128f, 124f, A(Hex("#35D0FF"), 0.35f), 2.4f);

            return Save(canvas, "Shockwave");
        }

        private static Sprite BuildSparkle()
        {
            ArtCanvas canvas = new ArtCanvas(64, 64, 4);
            Vector2 center = V(32f, 32f);

            Vector2[] star = new Vector2[8];
            for (int i = 0; i < star.Length; i++)
            {
                float angle = i / (float)star.Length * Mathf.PI * 2f + Mathf.PI * 0.5f;
                float radius = i % 2 == 0 ? 28f : 6f;
                star[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            canvas.FillPolygon(star, Hex("#FFF3B0"));
            canvas.Radiance(32f, 32f, 26f, A(Hex("#FFD166"), 0.85f), 1.6f);
            canvas.AdditiveGlow(32f, 32f, 30f, A(Hex("#FFFFFF"), 0.9f), 2.4f);

            return Save(canvas, "Sparkle");
        }

        private static Sprite BuildStar()
        {
            ArtCanvas canvas = new ArtCanvas(32, 32, 4);

            canvas.Radiance(16f, 16f, 15f, Hex("#FFFFFF"), 1.5f);
            canvas.FillCircle(16f, 16f, 4f, Hex("#FFFFFF"));

            return Save(canvas, "Star");
        }

        private static Sprite BuildGlow()
        {
            ArtCanvas canvas = new ArtCanvas(128, 128, 3);
            canvas.Radiance(64f, 64f, 63f, Hex("#FFFFFF"), 2.4f);
            return Save(canvas, "Glow");
        }
    }
}
