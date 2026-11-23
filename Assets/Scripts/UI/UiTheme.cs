using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Single source of truth for UI colours so generated scenes, runtime
    /// effects and editor-created sprites stay in sync.
    /// </summary>
    public static class UiTheme
    {
        public static readonly Color Ink = Hex("#060A18");
        public static readonly Color Deep = Hex("#0E1730");
        public static readonly Color Panel = Hex("#121B33");
        public static readonly Color PanelLight = Hex("#1B2947");
        public static readonly Color Accent = Hex("#35D0FF");
        public static readonly Color AccentSoft = Hex("#7FE3FF");
        public static readonly Color Gold = Hex("#FFD166");
        public static readonly Color Danger = Hex("#FF5A5A");
        public static readonly Color Text = Hex("#EAF2FF");
        public static readonly Color TextMuted = Hex("#93A6C4");

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
