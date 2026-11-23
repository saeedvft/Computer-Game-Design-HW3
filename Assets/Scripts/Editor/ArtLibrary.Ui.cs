using UnityEngine;

namespace ChickenShooter.EditorTools
{
    public static partial class ArtLibrary
    {
        // ------------------------------------------------------------------
        // UI kit (all 9-sliced sprites are 96x96 with a 26px border)
        // ------------------------------------------------------------------

        private static Sprite BuildPanel()
        {
            ArtCanvas canvas = new ArtCanvas(96, 96, 4);
            Color border = A(Hex("#3A4F78"), 0.9f);

            canvas.FillRoundedRect(2f, 2f, 92f, 92f, 20f, border);
            canvas.FillRoundedRectGradient(5f, 5f, 86f, 86f, 17f, A(Hex("#1B2947"), 0.98f), A(Hex("#0C1526"), 0.99f));
            canvas.FillRoundedRect(9f, 78f, 78f, 10f, 5f, A(Hex("#FFFFFF"), 0.06f));

            return Save(canvas, "UI_Panel", UiPixelsPerUnit, new Vector4(28f, 28f, 28f, 28f), TextureWrapMode.Clamp, true);
        }

        private static Sprite BuildButton(string name, Color top, Color bottom, Color border, float borderAlpha)
        {
            ArtCanvas canvas = new ArtCanvas(96, 96, 4);

            canvas.FillRoundedRect(2f, 4f, 92f, 90f, 22f, A(Hex("#05080F"), 0.55f));
            canvas.FillRoundedRect(4f, 6f, 88f, 86f, 20f, A(border, borderAlpha));
            canvas.FillRoundedRectGradient(7f, 9f, 82f, 80f, 17f, top, bottom);
            canvas.FillRoundedRect(11f, 72f, 74f, 12f, 6f, A(Hex("#FFFFFF"), 0.10f));
            canvas.FillRoundedRect(11f, 12f, 74f, 6f, 3f, A(Hex("#000000"), 0.18f));

            return Save(canvas, $"UI_{name}", UiPixelsPerUnit, new Vector4(30f, 30f, 30f, 30f), TextureWrapMode.Clamp, true);
        }

        private static Sprite BuildBar(string name, Color bottom, Color top)
        {
            ArtCanvas canvas = new ArtCanvas(64, 64, 4);

            canvas.FillRoundedRect(1f, 1f, 62f, 62f, 18f, Hex("#22314F"));
            canvas.FillRoundedRectGradient(4f, 4f, 56f, 56f, 15f, top, bottom);
            canvas.FillRoundedRect(8f, 40f, 48f, 14f, 7f, A(Hex("#FFFFFF"), 0.25f));

            return Save(canvas, $"UI_{name}", UiPixelsPerUnit, new Vector4(18f, 18f, 18f, 18f), TextureWrapMode.Clamp, true);
        }

        private static Sprite BuildLifeIcon()
        {
            ArtCanvas canvas = new ArtCanvas(64, 64, 4);
            Vector2[] hull =
            {
                V(32, 58), V(41, 44), V(45, 28), V(40, 14), V(32, 8), V(24, 14), V(19, 28), V(23, 44)
            };

            Vector2[] leftWing = { V(24, 38), V(10, 26), V(8, 14), V(20, 20) };
            Vector2[] rightWing = Mirror64(leftWing);

            canvas.FillPolygon(leftWing, Hex("#FFFFFF"));
            canvas.FillPolygon(rightWing, Hex("#FFFFFF"));
            canvas.FillPolygon(hull, Hex("#FFFFFF"));
            canvas.FillEllipse(32f, 38f, 6f, 8f, A(Hex("#8FA8C8"), 0.9f));

            return Save(canvas, "UI_IconLife", UiPixelsPerUnit);
        }

        private static Sprite BuildBombIcon()
        {
            ArtCanvas canvas = new ArtCanvas(64, 64, 4);

            canvas.FillCircle(30f, 26f, 18f, Hex("#FFFFFF"));
            canvas.FillCircle(30f, 26f, 18f, A(Hex("#C9D6EA"), 0.35f));
            canvas.FillRoundedRect(26f, 40f, 8f, 10f, 3f, Hex("#FFFFFF"));
            canvas.FillCapsule(32f, 48f, 40f, 56f, 2.5f, Hex("#FFFFFF"));
            canvas.FillCircle(42f, 57f, 5f, Hex("#FFFFFF"));
            canvas.FillCircle(20f, 32f, 5f, A(Hex("#FFFFFF"), 0.7f));

            return Save(canvas, "UI_IconBomb", UiPixelsPerUnit);
        }

        private static Sprite BuildWeaponIcon()
        {
            ArtCanvas canvas = new ArtCanvas(64, 64, 4);

            Vector2[] bolt =
            {
                V(34, 58), V(16, 30), V(29, 30), V(24, 6), V(46, 36), V(34, 36)
            };

            canvas.FillPolygon(bolt, Hex("#FFFFFF"));
            canvas.FillPolygon(new[] { V(31, 48), V(23, 34), V(28, 34) }, A(Hex("#B9D4EE"), 0.5f));

            return Save(canvas, "UI_IconWeapon", UiPixelsPerUnit);
        }

        private static Sprite BuildFoodIcon()
        {
            ArtCanvas canvas = new ArtCanvas(64, 64, 4);

            canvas.FillRotatedEllipse(28f, 32f, 20f, 15f, -22f, Hex("#FFFFFF"));
            canvas.FillCapsule(40f, 28f, 52f, 44f, 6f, Hex("#FFFFFF"));
            canvas.FillCircle(55f, 48f, 7f, Hex("#FFFFFF"));
            canvas.FillCircle(50f, 53f, 5f, Hex("#FFFFFF"));

            return Save(canvas, "UI_IconFood", UiPixelsPerUnit);
        }

        private static Sprite BuildVignette()
        {
            ArtCanvas canvas = new ArtCanvas(512, 512, 1);
            canvas.Vignette(0.32f, 1.02f, Hex("#FFFFFF"));
            return Save(canvas, "UI_Vignette", UiPixelsPerUnit);
        }

        private static Sprite BuildWhite()
        {
            ArtCanvas canvas = new ArtCanvas(8, 8, 1);
            canvas.Fill(Color.white);
            return Save(canvas, "UI_White", UiPixelsPerUnit);
        }

        private static Sprite BuildLogoBadge()
        {
            ArtCanvas canvas = new ArtCanvas(256, 256, 4);
            Vector2 center = V(128f, 128f);

            canvas.AdditiveGlow(128f, 128f, 124f, A(Cyan, 0.4f), 3f);

            // Gold ring.
            canvas.FillCircle(128f, 128f, 118f, A(Hex("#05080F"), 0.6f));
            canvas.FillCircle(128f, 128f, 112f, Hex("#D98A18"));
            canvas.FillCircleGradient(128f, 128f, 106f, Hex("#FFE9A8"), Hex("#B87110"));
            canvas.FillCircle(128f, 128f, 96f, Hex("#0C1526"));
            canvas.FillCircleGradient(128f, 128f, 94f, Hex("#1B3358"), Hex("#0A1224"));

            // Chicken head emblem.
            canvas.FillCircle(118f, 142f, 46f, A(Hex("#05080F"), 0.5f));
            canvas.FillCircle(118f, 144f, 44f, Hex("#E8D2A8"));
            canvas.FillCircleGradient(118f, 144f, 42f, Hex("#FFFDF2"), Hex("#E8C9A1"));

            // Comb.
            canvas.FillCircle(96f, 182f, 14f, Hex("#E8453C"));
            canvas.FillCircle(118f, 190f, 16f, Hex("#E8453C"));
            canvas.FillCircle(140f, 182f, 14f, Hex("#E8453C"));

            // Beak.
            canvas.FillPolygon(new[] { V(96, 126), V(140, 126), V(118, 100) }, Hex("#FFC24B"));
            canvas.FillPolygon(new[] { V(102, 124), V(134, 124), V(118, 113) }, A(Hex("#B87110"), 0.6f));

            // Eyes.
            canvas.FillCircle(102f, 154f, 11f, Hex("#FFFFFF"));
            canvas.FillCircle(134f, 154f, 11f, Hex("#FFFFFF"));
            canvas.FillCircle(102f, 151f, 5.5f, Hex("#1B1420"));
            canvas.FillCircle(134f, 151f, 5.5f, Hex("#1B1420"));
            canvas.FillCircle(100f, 156f, 2f, Hex("#FFFFFF"));

            // Stars.
            DrawEmblemStar(canvas, 62f, 74f, 9f);
            DrawEmblemStar(canvas, 196f, 84f, 7f);
            DrawEmblemStar(canvas, 176f, 58f, 5f);

            return Save(canvas, "UI_LogoBadge", UiPixelsPerUnit);
        }

        private static void DrawEmblemStar(ArtCanvas canvas, float x, float y, float radius)
        {
            Vector2[] star = new Vector2[8];
            for (int i = 0; i < star.Length; i++)
            {
                float angle = i / (float)star.Length * Mathf.PI * 2f;
                float r = i % 2 == 0 ? radius : radius * 0.32f;
                star[i] = new Vector2(x + Mathf.Cos(angle) * r, y + Mathf.Sin(angle) * r);
            }

            canvas.FillPolygon(star, A(Hex("#FFE9A8"), 0.9f));
        }

        private static Vector2[] Mirror64(Vector2[] points)
        {
            Vector2[] mirrored = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                mirrored[i] = new Vector2(64f - points[i].x, points[i].y);
            }

            return mirrored;
        }
    }
}
