using System;
using UnityEngine;
using Random = System.Random;

namespace ChickenShooter.EditorTools
{
    public static partial class ArtLibrary
    {
        // ------------------------------------------------------------------
        // Parallax backgrounds: 3 themes x 3 depth layers
        // ------------------------------------------------------------------

        private struct BgPalette
        {
            public Color Top;
            public Color Bottom;
            public Color NebulaA;
            public Color NebulaB;
            public Color Star;
            public Color PlanetLight;
            public Color PlanetDark;
            public Color Atmosphere;
            public Color Debris;
        }

        private static BgPalette PaletteFor(int theme)
        {
            switch (theme)
            {
                case 1:
                    return new BgPalette
                    {
                        Top = Hex("#0B0616"),
                        Bottom = Hex("#2A1038"),
                        NebulaA = Hex("#6B2FA0"),
                        NebulaB = Hex("#E8459B"),
                        Star = Hex("#F0D8FF"),
                        PlanetLight = Hex("#B07AC9"),
                        PlanetDark = Hex("#2A1236"),
                        Atmosphere = Hex("#D9A0FF"),
                        Debris = Hex("#3A2440")
                    };
                case 2:
                    return new BgPalette
                    {
                        Top = Hex("#120410"),
                        Bottom = Hex("#2E0A16"),
                        NebulaA = Hex("#A83B2F"),
                        NebulaB = Hex("#FF8A3D"),
                        Star = Hex("#FFE0C2"),
                        PlanetLight = Hex("#E8905A"),
                        PlanetDark = Hex("#3A0F16"),
                        Atmosphere = Hex("#FFB35C"),
                        Debris = Hex("#402028")
                    };
                default:
                    return new BgPalette
                    {
                        Top = Hex("#03081A"),
                        Bottom = Hex("#0B1E3E"),
                        NebulaA = Hex("#1B4FA8"),
                        NebulaB = Hex("#2FA8FF"),
                        Star = Hex("#CFE6FF"),
                        PlanetLight = Hex("#4A8BE8"),
                        PlanetDark = Hex("#0E2A5E"),
                        Atmosphere = Hex("#35D0FF"),
                        Debris = Hex("#24344E")
                    };
            }
        }

        private static Sprite[] BuildBackgrounds(int layer)
        {
            Sprite[] sprites = new Sprite[3];

            for (int theme = 0; theme < 3; theme++)
            {
                ArtCanvas canvas = new ArtCanvas(BackgroundWidth, BackgroundHeight, 2);

                switch (layer)
                {
                    case 0:
                        DrawFarBackground(canvas, theme);
                        break;
                    case 1:
                        DrawMidBackground(canvas, theme);
                        break;
                    default:
                        DrawNearBackground(canvas, theme);
                        break;
                }

                sprites[theme] = Save(
                    canvas,
                    $"BG_{layer}_{theme}",
                    BackgroundPixelsPerUnit,
                    default,
                    TextureWrapMode.Repeat,
                    true);
            }

            return sprites;
        }

        private static void DrawFarBackground(ArtCanvas canvas, int theme)
        {
            BgPalette palette = PaletteFor(theme);
            Random random = new Random(500 + theme * 71);

            canvas.FillRectGradient(0f, 0f, BackgroundWidth, BackgroundHeight, palette.Top, palette.Bottom);

            // Nebula clouds, wrapped so the tile keeps repeating seamlessly.
            for (int i = 0; i < 5; i++)
            {
                float x = (float)random.NextDouble() * BackgroundWidth;
                float y = (float)random.NextDouble() * BackgroundHeight;
                float radius = 210f + (float)random.NextDouble() * 240f;
                Color color = A(i % 2 == 0 ? palette.NebulaA : palette.NebulaB, 0.09f + (float)random.NextDouble() * 0.09f);
                WrapDraw(canvas, x, y, radius, (cx, cy) => canvas.Radiance(cx, cy, radius, color, 2.6f));
            }

            // Milky-way band of tiny stars.
            float bandAngle = (float)random.NextDouble() * Mathf.PI;
            for (int i = 0; i < 260; i++)
            {
                float t = (float)random.NextDouble();
                float distanceFromLine = ((float)random.NextDouble() - 0.5f) * 120f;
                float x = 120f + t * (BackgroundWidth - 240f);
                float y = 140f + (t - 0.5f) * Mathf.Sin(bandAngle) * 300f + Mathf.Cos(bandAngle) * distanceFromLine + 260f;
                float size = 0.7f + (float)random.NextDouble() * 1.3f;
                Color color = A(palette.Star, 0.18f + (float)random.NextDouble() * 0.3f);
                WrapDraw(canvas, x, y, size * 2f, (cx, cy) => canvas.FillCircle(cx, cy, size, color));
            }

            // Foreground stars.
            for (int i = 0; i < 190; i++)
            {
                float x = (float)random.NextDouble() * BackgroundWidth;
                float y = (float)random.NextDouble() * BackgroundHeight;
                float size = 0.9f + (float)random.NextDouble() * 1.9f;
                float alpha = 0.35f + (float)random.NextDouble() * 0.65f;
                Color color = A(palette.Star, alpha);
                WrapDraw(canvas, x, y, size * 2f, (cx, cy) => canvas.FillCircle(cx, cy, size, color));

                if (size > 2.2f)
                {
                    WrapDraw(canvas, x, y, 10f, (cx, cy) => canvas.AdditiveGlow(cx, cy, 9f, A(palette.Star, alpha * 0.5f), 2.2f));
                }
            }

            // Subtle darkening at the very bottom for depth.
            canvas.FillRectGradient(0f, 0f, BackgroundWidth, 150f, Color.clear, A(Hex("#000000"), 0.18f));
        }

        private static void DrawMidBackground(ArtCanvas canvas, int theme)
        {
            BgPalette palette = PaletteFor(theme);
            Random random = new Random(900 + theme * 53);

            // Planet with atmosphere.
            float planetX = 220f + (float)random.NextDouble() * (BackgroundWidth - 440f);
            float planetY = 250f + (float)random.NextDouble() * 320f;
            float planetRadius = 110f + (float)random.NextDouble() * 66f;

            canvas.FillCircle(planetX, planetY, planetRadius + 26f, A(palette.Atmosphere, 0.10f));
            canvas.FillCircle(planetX, planetY, planetRadius + 12f, A(palette.Atmosphere, 0.16f));
            canvas.FillCircleGradient(planetX, planetY, planetRadius, palette.PlanetLight, palette.PlanetDark);

            // Cloud bands.
            for (int i = 0; i < 5; i++)
            {
                float bandY = planetY - planetRadius * 0.6f + i * planetRadius * 0.3f;
                float bandRadius = Mathf.Sqrt(Mathf.Max(0f, planetRadius * planetRadius - (bandY - planetY) * (bandY - planetY)));
                canvas.FillEllipse(planetX, bandY, bandRadius * 0.95f, 6f + (float)random.NextDouble() * 9f, A(palette.PlanetDark, 0.22f));
            }

            // Terminator shading.
            canvas.FillCircle(planetX + planetRadius * 0.45f, planetY - planetRadius * 0.35f, planetRadius, A(palette.PlanetDark, 0.35f));
            canvas.FillCircle(planetX - planetRadius * 0.15f, planetY - planetRadius * 0.1f, planetRadius, A(palette.PlanetDark, 0.10f));

            // Rim light.
            DrawArc(canvas, planetX, planetY, planetRadius - 2f, -30f, 150f, 5f, A(palette.Atmosphere, 0.55f));

            // Ring for the asteroid belt theme.
            if (theme == 1)
            {
                DrawEllipseRing(canvas, planetX, planetY, planetRadius * 1.9f, planetRadius * 0.5f, 22f, 6f, A(palette.Atmosphere, 0.35f));
                DrawEllipseRing(canvas, planetX, planetY, planetRadius * 1.72f, planetRadius * 0.44f, 22f, 3f, A(palette.Atmosphere, 0.3f));
            }

            // Moons.
            int moons = 2 + random.Next(2);
            for (int i = 0; i < moons; i++)
            {
                float x = (float)random.NextDouble() * BackgroundWidth;
                float y = (float)random.NextDouble() * BackgroundHeight;
                float radius = 12f + (float)random.NextDouble() * 26f;
                Color light = A(palette.PlanetLight, 0.55f);
                Color dark = A(palette.PlanetDark, 0.65f);

                WrapDraw(canvas, x, y, radius * 2f, (cx, cy) =>
                {
                    canvas.FillCircleGradient(cx, cy, radius, light, dark);
                    canvas.FillCircle(cx - radius * 0.3f, cy + radius * 0.3f, radius * 0.45f, A(Hex("#FFFFFF"), 0.10f));
                });
            }

            // Faint star clusters.
            for (int i = 0; i < 90; i++)
            {
                float x = (float)random.NextDouble() * BackgroundWidth;
                float y = (float)random.NextDouble() * BackgroundHeight;
                float size = 0.8f + (float)random.NextDouble() * 1.4f;
                Color color = A(palette.Star, 0.2f + (float)random.NextDouble() * 0.4f);
                WrapDraw(canvas, x, y, size * 2f, (cx, cy) => canvas.FillCircle(cx, cy, size, color));
            }
        }

        private static void DrawNearBackground(ArtCanvas canvas, int theme)
        {
            BgPalette palette = PaletteFor(theme);
            Random random = new Random(1300 + theme * 91);

            // Debris rocks drifting through the foreground.
            int rocks = 7 + random.Next(3);
            for (int i = 0; i < rocks; i++)
            {
                float x = (float)random.NextDouble() * BackgroundWidth;
                float y = (float)random.NextDouble() * BackgroundHeight;
                float radius = 12f + (float)random.NextDouble() * 34f;
                float rotation = (float)random.NextDouble() * 360f;
                float stretch = 0.6f + (float)random.NextDouble() * 0.7f;

                WrapDraw(canvas, x, y, radius * 1.6f, (cx, cy) =>
                    DrawDebrisRock(canvas, cx, cy, radius, stretch, rotation, palette, random));
            }

            // Comet streaks.
            for (int i = 0; i < 4; i++)
            {
                float x = (float)random.NextDouble() * BackgroundWidth;
                float y = (float)random.NextDouble() * BackgroundHeight;
                float length = 60f + (float)random.NextDouble() * 120f;
                float angle = -20f + (float)random.NextDouble() * 40f;
                float radians = angle * Mathf.Deg2Rad;
                Vector2 start = new Vector2(x, y);
                Vector2 end = start + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * length;
                Color color = A(palette.Star, 0.08f + (float)random.NextDouble() * 0.08f);

                WrapDraw(canvas, x, y, length, (cx, cy) => canvas.FillCapsule(
                    cx, cy, cx + (end.x - start.x), cy + (end.y - start.y), 1.6f, color));
            }

            // Fine dust.
            for (int i = 0; i < 120; i++)
            {
                float x = (float)random.NextDouble() * BackgroundWidth;
                float y = (float)random.NextDouble() * BackgroundHeight;
                float size = 0.6f + (float)random.NextDouble() * 1.1f;
                Color color = A(palette.Star, 0.08f + (float)random.NextDouble() * 0.2f);
                WrapDraw(canvas, x, y, size * 2f, (cx, cy) => canvas.FillCircle(cx, cy, size, color));
            }
        }

        private static void DrawDebrisRock(
            ArtCanvas canvas, float x, float y, float radius, float stretch,
            float rotation, BgPalette palette, Random random)
        {
            const int points = 9;
            float radians = rotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            Vector2[] polygon = new Vector2[points];

            for (int i = 0; i < points; i++)
            {
                float angle = i / (float)points * Mathf.PI * 2f;
                float r = radius * (0.75f + (float)random.NextDouble() * 0.35f);
                Vector2 local = new Vector2(Mathf.Cos(angle) * r * stretch, Mathf.Sin(angle) * r);
                polygon[i] = new Vector2(
                    x + local.x * cos - local.y * sin,
                    y + local.x * sin + local.y * cos);
            }

            canvas.StrokePolygon(polygon, 4f, A(Hex("#05080F"), 0.85f));
            canvas.FillPolygonGradient(polygon, A(palette.Debris, 0.95f), A(Hex("#05080F"), 0.95f));
            canvas.StrokePolygon(
                new[] { polygon[1], polygon[2], polygon[3] },
                2.5f,
                A(palette.Atmosphere, 0.35f),
                false);
        }

        private static void DrawArc(
            ArtCanvas canvas, float cx, float cy, float radius,
            float startDegrees, float endDegrees, float width, Color color)
        {
            const int segments = 28;
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(startDegrees, endDegrees, i / (float)segments) * Mathf.Deg2Rad;
                points[i] = new Vector2(cx + Mathf.Cos(angle) * radius, cy + Mathf.Sin(angle) * radius);
            }

            canvas.StrokePolygon(points, width, color, false);
        }

        private static void DrawEllipseRing(
            ArtCanvas canvas, float cx, float cy, float rx, float ry, float rotation, float width, Color color)
        {
            const int segments = 64;
            float radians = rotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            Vector2[] points = new Vector2[segments];

            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * rx;
                float y = Mathf.Sin(angle) * ry;
                points[i] = new Vector2(cx + x * cos - y * sin, cy + x * sin + y * cos);
            }

            canvas.StrokePolygon(points, width, color);
        }

        private static void WrapDraw(ArtCanvas canvas, float x, float y, float radius, Action<float, float> draw)
        {
            int minX = x - radius < 0f ? -1 : 0;
            int maxX = x + radius > BackgroundWidth ? 1 : 0;
            int minY = y - radius < 0f ? -1 : 0;
            int maxY = y + radius > BackgroundHeight ? 1 : 0;

            for (int ox = minX; ox <= maxX; ox++)
            {
                for (int oy = minY; oy <= maxY; oy++)
                {
                    draw(x + ox * BackgroundWidth, y + oy * BackgroundHeight);
                }
            }
        }
    }
}
