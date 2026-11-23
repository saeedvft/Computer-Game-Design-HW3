using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ChickenShooter.EditorTools
{
    /// <summary>
    /// TMP UI construction for the main menu, the round banner and the shared
    /// button/label styling. Gameplay HUD, pause and end screens belong to the
    /// next phase. All sprites come from ArtLibrary.
    /// </summary>
    public static partial class ProjectSetup
    {
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 BottomRight = new Vector2(1f, 0f);

        // ------------------------------------------------------------------
        // Generic builders
        // ------------------------------------------------------------------

        private static Canvas CreateCanvas(string name, int sortingOrder)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static RectTransform NewRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Image NewImage(Transform parent, string name, Sprite sprite, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size,
            Image.Type type = Image.Type.Simple)
        {
            RectTransform rect = NewRect(parent, name, anchorMin, anchorMax, pivot, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = type;
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI NewLabel(Transform parent, string name, string text, float fontSize,
            Color color, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size, bool bold = false)
        {
            RectTransform rect = NewRect(parent, name, anchorMin, anchorMax, pivot, position, size);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;

            if (bold)
            {
                label.fontStyle = FontStyles.Bold;
            }

            return label;
        }

        private static Material GetOutlineMaterial(Color outline, float width = 0.2f)
        {
            string key = ColorUtility.ToHtmlStringRGB(outline);
            string path = $"{ArtLibrary.ArtDir}/TMP_Outline_{key}.mat";

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            material = new Material(font.material)
            {
                name = $"TMP_Outline_{key}"
            };

            material.EnableKeyword("OUTLINE_ON");
            material.SetFloat("_OutlineWidth", width);
            material.SetFloat("_OutlineSoftness", 0.05f);
            material.SetColor("_OutlineColor", outline);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void StyleTitle(TextMeshProUGUI label, Color top, Color bottom, Color outline, float outlineWidth = 0.2f)
        {
            label.fontStyle = FontStyles.Bold;
            label.enableVertexGradient = true;
            label.colorGradient = new VertexGradient(top, top, bottom, bottom);
            label.fontSharedMaterial = GetOutlineMaterial(outline, outlineWidth);
        }

        private static Button NewButton(Transform parent, string name, string text, ArtLibrary.SpriteSet sprites,
            Vector2 position, Vector2 size, float fontSize = 32)
        {
            RectTransform rect = NewRect(parent, name, Center, Center, Center, position, size);

            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprites.Button;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.82f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.62f, 0.86f, 1f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.45f, 0.5f, 0.6f, 0.55f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            TextMeshProUGUI label = NewLabel(rect, "Label", text, fontSize, UiTheme.Text,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero, true);
            RectTransform labelRect = label.rectTransform;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            rect.gameObject.AddComponent<UiButtonJuice>();
            return button;
        }

        private static Button FindButton(Transform root, string name)
        {
            Transform child = root.Find(name);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private static InputSystemUIInputModule CreateEventSystem()
        {
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            return go.AddComponent<InputSystemUIInputModule>();
        }

        // ------------------------------------------------------------------
        // Main menu
        // ------------------------------------------------------------------

        private static void BuildMainMenu(Transform canvas, ArtLibrary.SpriteSet sprites)
        {
            NewImage(canvas, "LogoBadge", sprites.LogoBadge, Color.white, Center, Center, Center,
                new Vector2(0f, 330f), new Vector2(250f, 250f));

            TextMeshProUGUI title = NewLabel(canvas, "Title", "CHICKEN SHOOTER", 108f, Color.white,
                TextAlignmentOptions.Center, Center, Center, Center, new Vector2(0f, 152f), new Vector2(1700f, 150f), true);
            StyleTitle(title, UiTheme.Gold, UiTheme.Hex("#FF8A3D"), UiTheme.Ink, 0.22f);
            title.characterSpacing = 6f;

            TextMeshProUGUI subtitle = NewLabel(canvas, "Subtitle", "HOMEWORK 3  -  GRAPHICS PASS", 26f,
                UiTheme.Accent, TextAlignmentOptions.Center, Center, Center, Center,
                new Vector2(0f, 66f), new Vector2(1100f, 44f), true);
            subtitle.characterSpacing = 10f;

            Button newGame = NewButton(canvas, "NewGameButton", "NEW GAME", sprites, new Vector2(0f, -8f), new Vector2(440f, 88f));
            Button quitButton = NewButton(canvas, "QuitButton", "EXIT", sprites, new Vector2(0f, -118f), new Vector2(440f, 88f));

            TextMeshProUGUI hint = NewLabel(canvas, "Hint",
                "WASD / ARROWS  MOVE      SPACE  FIRE      X  BOMB      ESC  PAUSE", 22f,
                UiTheme.TextMuted, TextAlignmentOptions.Center, Center, Center, Center,
                new Vector2(0f, -240f), new Vector2(1500f, 40f));

            hint.characterSpacing = 2f;

            TextMeshProUGUI version = NewLabel(canvas, "Version", "v0.2  -  graphics pass", 18f,
                UiTheme.WithAlpha(UiTheme.TextMuted, 0.7f), TextAlignmentOptions.BottomRight,
                BottomRight, BottomRight, BottomRight, new Vector2(-34f, 26f), new Vector2(500f, 30f));

            // Keep references alive for the controller wiring step.
            newGame.name = "NewGameButton";
            quitButton.name = "QuitButton";
        }

        // ------------------------------------------------------------------
        // Banner
        // ------------------------------------------------------------------

        private static LevelBanner CreateBanner()
        {
            Canvas canvas = CreateCanvas("BannerCanvas", 20);
            CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            TextMeshProUGUI title = NewLabel(canvas.transform, "BannerLabel", string.Empty, 96f, Color.white,
                TextAlignmentOptions.Center, Center, Center, Center, new Vector2(0f, 46f), new Vector2(1700f, 170f), true);
            StyleTitle(title, UiTheme.Gold, UiTheme.Hex("#FF8A3D"), UiTheme.Ink, 0.22f);
            title.characterSpacing = 6f;

            TextMeshProUGUI subtitle = NewLabel(canvas.transform, "BannerSub", string.Empty, 30f, UiTheme.Accent,
                TextAlignmentOptions.Center, Center, Center, Center, new Vector2(0f, -62f), new Vector2(1500f, 60f), true);
            subtitle.characterSpacing = 8f;

            LevelBanner banner = canvas.gameObject.AddComponent<LevelBanner>();
            banner.label = title;
            banner.sublabel = subtitle;
            banner.group = group;
            return banner;
        }
    }
}
