using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace ChickenShooter.EditorTools
{
    /// <summary>
    /// Deterministic procedural art. Every sprite is drawn at 4x and filtered
    /// down, so the game has smooth, hand-drawn-looking cartoon art with zero
    /// external assets and zero licensing obligations.
    /// </summary>
    public static partial class ArtLibrary
    {
        public const string ArtDir = "Assets/Art/Generated";
        public const float SpritePixelsPerUnit = 192f;
        public const float BackgroundPixelsPerUnit = 48f;
        public const float UiPixelsPerUnit = 100f;

        public const int SpriteSize = 192;
        public const int BackgroundWidth = 1280;
        public const int BackgroundHeight = 720;

        // Shared palette -----------------------------------------------------
        internal static readonly Color Ink = Hex("#05080F");
        internal static readonly Color Outline = Hex("#1A1230");
        internal static readonly Color Gold = Hex("#FFC24B");
        internal static readonly Color GoldDeep = Hex("#D98A18");
        internal static readonly Color Cyan = Hex("#35D0FF");
        internal static readonly Color CyanLight = Hex("#BFF4FF");
        internal static readonly Color CyanDeep = Hex("#1E8FD6");
        internal static readonly Color Red = Hex("#E8453C");
        internal static readonly Color RedLight = Hex("#FF8272");
        internal static readonly Color Orange = Hex("#FF8A3D");

        public sealed class SpriteSet
        {
            public Sprite[] Ship;
            public Sprite[] ChickenBasic;
            public Sprite[] ChickenPoison;
            public Sprite[] ChickenAlien;
            public Sprite Egg;
            public Sprite EggPoison;
            public Sprite Laser;
            public Sprite[] Asteroid;
            public Sprite Food;
            public Sprite Gift;

            public Sprite[] Explosion;
            public Sprite[] ExplosionSmall;
            public Sprite Muzzle;
            public Sprite Shockwave;
            public Sprite Sparkle;
            public Sprite Star;
            public Sprite Glow;

            public Sprite Panel;
            public Sprite Button;
            public Sprite ButtonHover;
            public Sprite ButtonPressed;
            public Sprite BarBackground;
            public Sprite BarFill;
            public Sprite BarFillFood;
            public Sprite BarFillBoss;
            public Sprite IconLife;
            public Sprite IconBomb;
            public Sprite IconWeapon;
            public Sprite IconFood;
            public Sprite Vignette;
            public Sprite White;
            public Sprite LogoBadge;

            public Sprite[] BackgroundFar;
            public Sprite[] BackgroundMid;
            public Sprite[] BackgroundNear;
        }

        public static SpriteSet Build()
        {
            SpriteSet set = new SpriteSet
            {
                Ship = BuildShip(),
                ChickenBasic = BuildChicken(ChickenVariant.Basic),
                ChickenPoison = BuildChicken(ChickenVariant.Poison),
                ChickenAlien = BuildChicken(ChickenVariant.Alien),
                Egg = BuildEgg(false),
                EggPoison = BuildEgg(true),
                Laser = BuildLaser(),
                Asteroid = BuildAsteroids(),
                Food = BuildFood(),
                Gift = BuildGift(),

                Explosion = BuildExplosion("Explosion", 8, 96),
                ExplosionSmall = BuildExplosion("ExplosionSmall", 6, 58),
                Muzzle = BuildMuzzle(),
                Shockwave = BuildShockwave(),
                Sparkle = BuildSparkle(),
                Star = BuildStar(),
                Glow = BuildGlow(),

                Panel = BuildPanel(),
                Button = BuildButton("Button", Hex("#1B3358"), Hex("#12233F"), Cyan, 1f),
                ButtonHover = BuildButton("ButtonHover", Hex("#27507F"), Hex("#173055"), Hex("#7FE3FF"), 1f),
                ButtonPressed = BuildButton("ButtonPressed", Hex("#0E1B30"), Hex("#0A1424"), Gold, 1f),
                BarBackground = BuildBar("BarBackground", Hex("#0A1224"), Hex("#22314F")),
                BarFill = BuildBar("BarFill", CyanDeep, CyanLight),
                BarFillFood = BuildBar("BarFillFood", Hex("#D98A18"), Gold),
                BarFillBoss = BuildBar("BarFillBoss", Hex("#8C1F2E"), Hex("#FF5A5A")),
                IconLife = BuildLifeIcon(),
                IconBomb = BuildBombIcon(),
                IconWeapon = BuildWeaponIcon(),
                IconFood = BuildFoodIcon(),
                Vignette = BuildVignette(),
                White = BuildWhite(),
                LogoBadge = BuildLogoBadge(),

                BackgroundFar = BuildBackgrounds(0),
                BackgroundMid = BuildBackgrounds(1),
                BackgroundNear = BuildBackgrounds(2)
            };

            return set;
        }

        // ------------------------------------------------------------------
        // Ship
        // ------------------------------------------------------------------

        private static Sprite[] BuildShip()
        {
            Sprite[] frames = new Sprite[2];

            for (int frame = 0; frame < frames.Length; frame++)
            {
                ArtCanvas canvas = new ArtCanvas(SpriteSize, SpriteSize, 4);
                DrawShip(canvas, frame == 0 ? 1f : 0.55f);
                frames[frame] = Save(canvas, frame == 0 ? "Ship_0" : "Ship_1");
            }

            return frames;
        }

        private static void DrawShip(ArtCanvas canvas, float thrust)
        {
            Color hullTop = Hex("#FFFFFF");
            Color hullBottom = Hex("#A9C4E6");
            Color wingTop = Hex("#DCEBFF");
            Color wingBottom = Hex("#7E9EC6");
            Color hullOutline = Hex("#183252");

            // Engine glow behind the hull.
            canvas.AdditiveGlow(78f, 30f, 34f, Hex("#5FD0FF"), 2.4f);
            canvas.AdditiveGlow(114f, 30f, 34f, Hex("#5FD0FF"), 2.4f);

            DrawFlame(canvas, 96f, 24f, thrust, 1f);
            DrawFlame(canvas, 74f, 28f, thrust * 0.72f, 0.55f);
            DrawFlame(canvas, 118f, 28f, thrust * 0.72f, 0.55f);

            // Wings.
            Vector2[] leftWing =
            {
                V(70, 112), V(30, 78), V(16, 44), V(34, 26), V(58, 40), V(66, 62)
            };
            Vector2[] rightWing = Mirror(leftWing);

            canvas.StrokePolygon(leftWing, 9f, hullOutline);
            canvas.FillPolygonGradient(leftWing, wingTop, wingBottom);
            canvas.StrokePolygon(rightWing, 9f, hullOutline);
            canvas.FillPolygonGradient(rightWing, wingTop, wingBottom);

            // Wing accents.
            DrawWingStripe(canvas, new[] { V(52, 96), V(32, 76), V(28, 58), V(38, 54), V(56, 74) }, Gold);
            DrawWingStripe(canvas, new[] { V(140, 96), V(160, 76), V(164, 58), V(154, 54), V(136, 74) }, Gold);
            DrawWingStripe(canvas, new[] { V(46, 62), V(35, 48), V(37, 38), V(46, 44) }, Hex("#FF6B5A"));
            DrawWingStripe(canvas, new[] { V(146, 62), V(157, 48), V(155, 38), V(146, 44) }, Hex("#FF6B5A"));

            // Main hull.
            Vector2[] hull =
            {
                V(96, 184), V(122, 150), V(134, 106), V(126, 62), V(108, 38),
                V(84, 38), V(66, 62), V(58, 106), V(70, 150)
            };

            canvas.StrokePolygon(hull, 10f, hullOutline);
            canvas.FillPolygonGradient(hull, hullTop, hullBottom);

            // Centre spine.
            Vector2[] spine =
            {
                V(96, 176), V(106, 146), V(104, 78), V(96, 66), V(88, 78), V(86, 146)
            };
            canvas.FillPolygonGradient(spine, Hex("#FFFFFF"), Hex("#D7E7FA"));

            // Gold trim.
            canvas.FillPolygonGradient(
                new[] { V(96, 80), V(104, 74), V(96, 62), V(88, 74) },
                Gold, GoldDeep);

            // Cockpit.
            canvas.FillEllipse(96f, 128f, 25f, 30f, hullOutline);
            canvas.FillEllipseGradient(96f, 128f, 22f, 27f, CyanLight, CyanDeep);
            canvas.FillEllipse(88f, 138f, 7f, 11f, A(Hex("#FFFFFF"), 0.85f));
            canvas.FillEllipse(101f, 120f, 3f, 5f, A(Hex("#BFF4FF"), 0.6f));
            canvas.FillEllipse(96f, 128f, 22f, 27f, A(Hex("#0B2A44"), 0.12f));

            // Nose light.
            canvas.FillCircle(96f, 179f, 5f, Hex("#EAFDFF"));
            canvas.AdditiveGlow(96f, 179f, 14f, A(Cyan, 0.85f), 2.6f);

            // Nozzles.
            canvas.FillRoundedRect(68f, 32f, 16f, 18f, 5f, hullOutline);
            canvas.FillRoundedRect(108f, 32f, 16f, 18f, 5f, hullOutline);
            canvas.FillRoundedRect(71f, 35f, 10f, 13f, 3f, Hex("#24344E"));
            canvas.FillRoundedRect(111f, 35f, 10f, 13f, 3f, Hex("#24344E"));

            // Rim light along the left hull edge.
            canvas.StrokePolygon(new[] { V(70, 150), V(60, 108), V(68, 66) }, 3f, A(Hex("#FFFFFF"), 0.55f), false);
            canvas.StrokePolygon(new[] { V(34, 40), V(24, 50) }, 3f, A(Hex("#FFFFFF"), 0.35f), false);

            // Warm engine bloom on top of everything.
            canvas.AdditiveGlow(78f, 24f, 22f, A(Hex("#FFB35C"), 0.65f), 2.2f);
            canvas.AdditiveGlow(114f, 24f, 22f, A(Hex("#FFB35C"), 0.65f), 2.2f);
        }

        private static void DrawFlame(ArtCanvas canvas, float x, float y, float thrust, float scale)
        {
            float length = Mathf.Lerp(10f, 34f, Mathf.Clamp01(thrust)) * scale;

            Vector2[] outer =
            {
                V(x - 9f * scale, y + 6f),
                V(x - 5f * scale, y - length * 0.55f),
                V(x, y - length),
                V(x + 5f * scale, y - length * 0.55f),
                V(x + 9f * scale, y + 6f)
            };

            Vector2[] inner =
            {
                V(x - 5f * scale, y + 4f),
                V(x, y - length * 0.72f),
                V(x + 5f * scale, y + 4f)
            };

            canvas.FillPolygonGradient(outer, Hex("#FF8A3D"), A(Hex("#E8453C"), 0.25f));
            canvas.FillPolygonGradient(inner, Hex("#FFF3B0"), Hex("#FFC24B"));
            canvas.FillCapsule(x - 3f * scale, y, x, y - length * 0.5f, 2.2f * scale, A(Hex("#FFFFFF"), 0.9f));
        }

        private static void DrawWingStripe(ArtCanvas canvas, Vector2[] points, Color color)
        {
            canvas.FillPolygon(points, color);
        }

        // ------------------------------------------------------------------
        // Chickens
        // ------------------------------------------------------------------

        private enum ChickenVariant
        {
            Basic,
            Poison,
            Alien
        }

        private static Sprite[] BuildChicken(ChickenVariant variant)
        {
            Sprite[] frames = new Sprite[2];

            for (int frame = 0; frame < frames.Length; frame++)
            {
                ArtCanvas canvas = new ArtCanvas(SpriteSize, SpriteSize, 4);
                DrawChicken(canvas, variant, frame == 1);
                string name = variant switch
                {
                    ChickenVariant.Poison => "Chicken_Poison",
                    ChickenVariant.Alien => "Chicken_Alien",
                    _ => "Chicken_Basic"
                };

                frames[frame] = Save(canvas, $"{name}_{frame}");
            }

            return frames;
        }

        private static void DrawChicken(ArtCanvas canvas, ChickenVariant variant, bool flap)
        {
            bool poison = variant == ChickenVariant.Poison;
            bool alien = variant == ChickenVariant.Alien;

            Color bodyTop = alien ? Hex("#E3C8FF") : poison ? Hex("#D6FFB0") : Hex("#FFFDF2");
            Color bodyBottom = alien ? Hex("#8A4FD8") : poison ? Hex("#5FBF3F") : Hex("#E8C9A1");
            Color wingColor = alien ? Hex("#9A63E0") : poison ? Hex("#4FA532") : Hex("#D9B77E");
            Color comb = alien ? Hex("#35D0FF") : poison ? Hex("#7A3FD6") : Red;
            Color combLight = alien ? Hex("#BFF4FF") : poison ? Hex("#B27CFF") : RedLight;
            Color beak = poison ? Hex("#9DF26B") : Hex("#FFC24B");
            Color beakLight = poison ? Hex("#D6FFB0") : Hex("#FFE28A");
            Color outline = alien ? Hex("#241040") : poison ? Hex("#173312") : Hex("#3B2A1F");

            float wingLift = flap ? 14f : 0f;
            float wingAngle = flap ? 34f : 12f;

            // Legs.
            canvas.FillCapsule(80f, 52f, 72f, 22f, 5.5f, outline);
            canvas.FillCapsule(112f, 52f, 120f, 22f, 5.5f, outline);
            canvas.FillCapsule(80f, 52f, 72f, 22f, 4f, Hex("#E8A03A"));
            canvas.FillCapsule(112f, 52f, 120f, 22f, 4f, Hex("#E8A03A"));

            // Toes.
            DrawToes(canvas, 72f, 22f, outline, Hex("#E8A03A"));
            DrawToes(canvas, 120f, 22f, outline, Hex("#E8A03A"));

            // Tail feathers.
            canvas.FillRotatedEllipse(42f, 74f, 14f, 30f, 28f, outline);
            canvas.FillRotatedEllipse(42f, 74f, 11f, 26f, 28f, A(wingColor, 0.95f));
            canvas.FillRotatedEllipse(150f, 74f, 14f, 30f, -28f, outline);
            canvas.FillRotatedEllipse(150f, 74f, 11f, 26f, -28f, A(wingColor, 0.95f));

            // Wings (behind the body, drawn at the sides).
            canvas.FillRotatedEllipse(40f, 92f + wingLift, 20f, 34f, wingAngle, outline);
            canvas.FillRotatedEllipse(40f, 92f + wingLift, 17f, 31f, wingAngle, wingColor);
            canvas.FillRotatedEllipse(152f, 92f + wingLift, 20f, 34f, -wingAngle, outline);
            canvas.FillRotatedEllipse(152f, 92f + wingLift, 17f, 31f, -wingAngle, wingColor);

            // Body.
            canvas.FillEllipse(96f, 88f, 55f, 46f, outline);
            canvas.FillEllipseGradient(96f, 88f, 51f, 42f, bodyTop, bodyBottom);

            // Belly highlight.
            canvas.FillEllipse(96f, 70f, 32f, 20f, A(Hex("#FFFFFF"), 0.18f));

            // Spots for poison / alien.
            if (poison || alien)
            {
                Color spot = poison ? Hex("#7A3FD6") : Hex("#5B2FA8");
                canvas.FillEllipse(66f, 92f, 9f, 7f, A(spot, 0.75f));
                canvas.FillEllipse(126f, 82f, 7f, 9f, A(spot, 0.65f));
                canvas.FillEllipse(104f, 62f, 6f, 5f, A(spot, 0.55f));
            }

            // Head.
            canvas.FillCircle(96f, 138f, 39f, outline);
            canvas.FillEllipseGradient(96f, 138f, 35f, 36f, bodyTop, bodyBottom);

            // Comb.
            if (alien)
            {
                DrawAntenna(canvas, 74f, 168f, 62f, 188f, comb, combLight);
                DrawAntenna(canvas, 118f, 168f, 130f, 188f, comb, combLight);
            }
            else
            {
                canvas.FillCircle(78f, 168f, 13f, outline);
                canvas.FillCircle(96f, 174f, 14f, outline);
                canvas.FillCircle(114f, 168f, 13f, outline);
                canvas.FillCircle(78f, 168f, 10f, comb);
                canvas.FillCircle(96f, 174f, 11f, comb);
                canvas.FillCircle(114f, 168f, 10f, comb);
                canvas.FillCircle(76f, 171f, 4f, combLight);
                canvas.FillCircle(94f, 177f, 4f, combLight);
                canvas.FillCircle(112f, 171f, 4f, combLight);
            }

            // Beak.
            Vector2[] beakOuter = { V(76, 118), V(116, 118), V(96, 94) };
            canvas.StrokePolygon(beakOuter, 6f, outline);
            canvas.FillPolygonGradient(beakOuter, beakLight, beak);
            canvas.FillPolygon(new[] { V(82, 116), V(110, 116), V(96, 108) }, A(Hex("#000000"), 0.18f));

            if (poison)
            {
                canvas.FillCapsule(92f, 100f, 90f, 86f, 3f, A(Hex("#D6FFB0"), 0.85f));
            }

            // Eyes.
            DrawEye(canvas, 78f, 148f, 12f, outline, poison, alien);
            DrawEye(canvas, 114f, 148f, 12f, outline, poison, alien);

            if (alien)
            {
                Color visor = A(Hex("#35D0FF"), 0.92f);
                canvas.FillRoundedRect(62f, 138f, 68f, 24f, 11f, Hex("#1A0F33"));
                canvas.FillRoundedRect(65f, 141f, 62f, 18f, 9f, visor);
                canvas.FillRoundedRect(68f, 151f, 22f, 5f, 2.5f, A(Hex("#FFFFFF"), 0.65f));
                canvas.FillEllipse(72f, 150f, 10f, 6f, A(Hex("#FFFFFF"), 0.4f));
            }

            if (poison)
            {
                // Angry brows.
                canvas.FillCapsule(64f, 160f, 88f, 154f, 4f, Hex("#241040"));
                canvas.FillCapsule(128f, 160f, 104f, 154f, 4f, Hex("#241040"));
            }

            // Cheeks.
            canvas.FillEllipse(64f, 130f, 9f, 6f, A(Hex("#FF8A9B"), 0.35f));
            canvas.FillEllipse(128f, 130f, 9f, 6f, A(Hex("#FF8A9B"), 0.35f));

            // Soft top light.
            canvas.FillEllipse(96f, 158f, 22f, 9f, A(Hex("#FFFFFF"), 0.16f));
        }

        private static void DrawToes(ArtCanvas canvas, float x, float y, Color outline, Color color)
        {
            canvas.FillCapsule(x, y, x - 10f, y - 3f, 3.2f, outline);
            canvas.FillCapsule(x, y, x + 10f, y - 3f, 3.2f, outline);
            canvas.FillCapsule(x, y, x, y - 10f, 3.2f, outline);
            canvas.FillCapsule(x, y, x - 8f, y - 2f, 2.2f, color);
            canvas.FillCapsule(x, y, x + 8f, y - 2f, 2.2f, color);
            canvas.FillCapsule(x, y, x, y - 7f, 2.2f, color);
        }

        private static void DrawEye(ArtCanvas canvas, float x, float y, float radius, Color outline, bool poison, bool alien)
        {
            Color sclera = poison ? Hex("#FFF3B0") : Hex("#FFFFFF");
            Color pupil = alien ? Hex("#12304A") : Hex("#1B1420");

            canvas.FillCircle(x, y, radius + 2f, outline);
            canvas.FillCircle(x, y, radius, sclera);
            canvas.FillCircle(x, y - 2f, radius * 0.52f, pupil);
            canvas.FillCircle(x - 2.5f, y + 2.5f, radius * 0.2f, A(Hex("#FFFFFF"), 0.95f));
        }

        private static void DrawAntenna(ArtCanvas canvas, float x1, float y1, float x2, float y2, Color color, Color light)
        {
            canvas.FillCapsule(x1, y1, x2, y2, 3f, Outline);
            canvas.FillCapsule(x1, y1, x2, y2, 1.6f, color);
            canvas.FillCircle(x2, y2, 6f, Outline);
            canvas.FillCircle(x2, y2, 4.5f, light);
            canvas.AdditiveGlow(x2, y2, 12f, A(color, 0.8f), 2.2f);
        }

        // ------------------------------------------------------------------
        // Projectiles and props
        // ------------------------------------------------------------------

        private static Sprite BuildEgg(bool poison)
        {
            ArtCanvas canvas = new ArtCanvas(64, 88, 4);
            Color outline = poison ? Hex("#1E3A16") : Hex("#4A3A28");
            Color top = poison ? Hex("#D6FFB0") : Hex("#FFFDF4");
            Color bottom = poison ? Hex("#7CC24A") : Hex("#E4D2AE");

            canvas.FillEllipse(32f, 44f, 24f, 34f, outline);
            canvas.FillEllipseGradient(32f, 44f, 21f, 31f, top, bottom);

            // Speckles.
            Random random = new Random(poison ? 17 : 5);
            Color speck = poison ? Hex("#4FA532") : Hex("#C9B48A");
            for (int i = 0; i < 7; i++)
            {
                float x = 16f + (float)random.NextDouble() * 32f;
                float y = 24f + (float)random.NextDouble() * 40f;
                canvas.FillEllipse(x, y, 2.4f, 3.2f, A(speck, 0.75f));
            }

            canvas.FillEllipse(23f, 60f, 7f, 11f, A(Hex("#FFFFFF"), 0.75f));
            canvas.FillEllipse(40f, 26f, 8f, 5f, A(Hex("#000000"), 0.08f));

            return Save(canvas, poison ? "Egg_Poison" : "Egg");
        }

        private static Sprite BuildLaser()
        {
            ArtCanvas canvas = new ArtCanvas(32, 112, 4);

            canvas.FillCapsule(16f, 8f, 16f, 100f, 11f, A(Cyan, 0.18f));
            canvas.FillCapsule(16f, 10f, 16f, 98f, 7f, Hex("#59E4FF"));
            canvas.FillCapsule(16f, 14f, 16f, 94f, 3.4f, Hex("#FFFFFF"));
            canvas.FillCircle(16f, 101f, 8f, A(Hex("#BFF4FF"), 0.85f));
            canvas.FillCircle(16f, 101f, 4f, Hex("#FFFFFF"));
            canvas.AdditiveGlow(16f, 101f, 18f, A(Cyan, 0.8f), 2.2f);

            return Save(canvas, "Laser");
        }

        private static Sprite[] BuildAsteroids()
        {
            Sprite[] sprites = new Sprite[3];

            for (int i = 0; i < sprites.Length; i++)
            {
                ArtCanvas canvas = new ArtCanvas(SpriteSize, SpriteSize, 4);
                DrawAsteroid(canvas, 11 + i * 7);
                sprites[i] = Save(canvas, $"Asteroid_{i}");
            }

            return sprites;
        }

        private static void DrawAsteroid(ArtCanvas canvas, int seed)
        {
            Random random = new Random(seed);
            const int points = 16;
            Vector2[] polygon = new Vector2[points];

            for (int i = 0; i < points; i++)
            {
                float angle = i / (float)points * Mathf.PI * 2f;
                float radius = 62f + (float)random.NextDouble() * 24f;
                polygon[i] = new Vector2(96f + Mathf.Cos(angle) * radius, 96f + Mathf.Sin(angle) * radius);
            }

            canvas.StrokePolygon(polygon, 8f, Hex("#0B0F18"));
            canvas.FillPolygonGradient(polygon, Hex("#7A7F8E"), Hex("#33374A"));

            // Craters.
            Color crater = Hex("#2B2F3C");
            Color craterRim = Hex("#9AA1B2");

            for (int i = 0; i < 6; i++)
            {
                float x = 24f + (float)random.NextDouble() * 144f;
                float y = 24f + (float)random.NextDouble() * 144f;
                float rx = 7f + (float)random.NextDouble() * 16f;
                float ry = rx * (0.7f + (float)random.NextDouble() * 0.4f);

                if ((new Vector2(x, y) - new Vector2(96f, 96f)).magnitude > 62f - rx)
                {
                    continue;
                }

                canvas.FillEllipse(x, y, rx, ry, A(crater, 0.85f));
                canvas.FillEllipse(x - rx * 0.25f, y + ry * 0.3f, rx * 0.7f, ry * 0.55f, A(craterRim, 0.35f));
            }

            // Rim light on the lit side.
            canvas.StrokePolygon(
                new[] { polygon[8], polygon[9], polygon[10], polygon[11] },
                4f, A(Hex("#C9D1E2"), 0.55f), false);

            canvas.AddNoise(0.06f, seed);
        }

        private static Sprite BuildFood()
        {
            ArtCanvas canvas = new ArtCanvas(128, 128, 4);
            Color outline = Hex("#3B1D0C");

            // Bone.
            canvas.FillCapsule(86f, 52f, 104f, 86f, 11f, outline);
            canvas.FillCapsule(86f, 52f, 104f, 86f, 8f, Hex("#F6EEDC"));
            canvas.FillCircle(108f, 92f, 12f, outline);
            canvas.FillCircle(108f, 92f, 9f, Hex("#F6EEDC"));
            canvas.FillCircle(98f, 100f, 9f, outline);
            canvas.FillCircle(98f, 100f, 6.5f, Hex("#F6EEDC"));

            // Meat.
            canvas.FillRotatedEllipse(58f, 62f, 42f, 34f, -24f, outline);
            canvas.FillRotatedEllipse(58f, 63f, 37f, 29f, -24f, Hex("#8C4A1E"));
            canvas.FillRotatedEllipse(54f, 70f, 31f, 23f, -24f, Hex("#C97A3F"));
            canvas.FillRotatedEllipse(46f, 78f, 15f, 9f, -24f, A(Hex("#E8A86A"), 0.75f));

            // Crispy edge.
            canvas.FillRotatedEllipse(76f, 48f, 18f, 11f, -24f, A(Hex("#6B3412"), 0.55f));

            return Save(canvas, "Food");
        }

        private static Sprite BuildGift()
        {
            ArtCanvas canvas = new ArtCanvas(128, 128, 4);
            Color outline = Hex("#2A0B16");
            Color boxTop = Hex("#FF6B5A");
            Color boxBottom = Hex("#B02330");

            // Box.
            canvas.FillRoundedRect(24f, 16f, 80f, 72f, 8f, outline);
            canvas.FillRoundedRectGradient(28f, 20f, 72f, 66f, 7f, boxTop, boxBottom);

            // Lid.
            canvas.FillRoundedRect(20f, 80f, 88f, 24f, 8f, outline);
            canvas.FillRoundedRectGradient(24f, 83f, 80f, 18f, 6f, Hex("#FF8272"), Hex("#C0303A"));

            // Ribbon.
            canvas.FillRect(56f, 24f, 16f, 76f, Hex("#FFD166"));
            canvas.FillRect(56f, 24f, 6f, 76f, Hex("#FFE9A8"));
            canvas.FillRect(30f, 54f, 68f, 16f, Hex("#FFD166"));
            canvas.FillRect(30f, 54f, 68f, 6f, Hex("#FFE9A8"));

            // Bow.
            canvas.FillRotatedEllipse(48f, 106f, 18f, 10f, 26f, outline);
            canvas.FillRotatedEllipse(48f, 106f, 15f, 7f, 26f, Hex("#FFD166"));
            canvas.FillRotatedEllipse(80f, 106f, 18f, 10f, -26f, outline);
            canvas.FillRotatedEllipse(80f, 106f, 15f, 7f, -26f, Hex("#FFD166"));
            canvas.FillCircle(64f, 102f, 9f, outline);
            canvas.FillCircle(64f, 102f, 6.5f, Hex("#FFE9A8"));

            // Sparkle.
            canvas.AdditiveGlow(102f, 96f, 16f, A(Hex("#FFFFFF"), 0.8f), 2.4f);

            return Save(canvas, "Gift");
        }

        // ------------------------------------------------------------------
        // Utilities
        // ------------------------------------------------------------------

        internal static Vector2 V(float x, float y) => new Vector2(x, y);

        internal static Color Hex(string hex) => UiTheme.Hex(hex);

        internal static Color A(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        internal static Vector2[] Mirror(Vector2[] points)
        {
            Vector2[] mirrored = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                mirrored[i] = new Vector2(192f - points[i].x, points[i].y);
            }

            return mirrored;
        }

        internal static Sprite Save(ArtCanvas canvas, string name, float ppu = SpritePixelsPerUnit,
            Vector4 border = default, TextureWrapMode wrap = TextureWrapMode.Clamp, bool fullRect = false)
        {
            Texture2D texture = canvas.ToTexture(name);
            string path = $"{ArtDir}/{name}.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = wrap;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.sRGBTexture = true;

                if (border != Vector4.zero || fullRect)
                {
                    TextureImporterSettings settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteBorder = border;
                    settings.spriteMeshType = fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
                    importer.SetTextureSettings(settings);
                }

                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError($"[ChickenShooter] Failed to import sprite {path}.");
            }

            return sprite;
        }
    }
}
