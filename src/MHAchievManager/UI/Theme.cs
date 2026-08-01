using System;
using System.Collections.Generic;
using System.Drawing;

namespace MHAchievManager.UI
{
    public static class Theme
    {    
        public static readonly Color ItemSelectedBg = Color.FromArgb(2, 151, 55);
        public static readonly Color TextMain = Color.FromArgb(220, 220, 220);
        public static readonly Color TextSelected = Color.White;
        public static readonly Color ItemHoverBg = Color.FromArgb(45, 45, 48);
        public static readonly Font MainFont = new("Segoe UI", 9f, FontStyle.Regular);

        public static readonly Dictionary<string, Color> TagColors = new(StringComparer.OrdinalIgnoreCase)
        {
            { "purplecard", ColorTranslator.FromHtml("#CC0099") },
            { "green",      ColorTranslator.FromHtml("#28AA00") },
            { "redcard",    ColorTranslator.FromHtml("#FF0000") },
            { "yellow",     ColorTranslator.FromHtml("#FCDB1C") },
            { "emphasis",   ColorTranslator.FromHtml("#3C96E6") },
            { "orange",   ColorTranslator.FromHtml("#FF9900") }
        };
    }
}
