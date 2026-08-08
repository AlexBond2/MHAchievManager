using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MHAchievManager.UI
{
    public static class Theme
    {    
        public static readonly Color ItemSelectedBg = Color.FromArgb(2, 151, 55);
        public static readonly Color TextMain = Color.FromArgb(220, 220, 220);
        public static readonly Color TextSelected = Color.White;
        public static readonly Color ItemHoverBg = Color.FromArgb(45, 45, 48);
        public static readonly Color Warning = Color.FromArgb(252, 225, 0);
        public static readonly Color Added = Color.FromArgb(34, 134, 58);
        public static readonly Color Removed = Color.FromArgb(203, 36, 49);
        public static readonly Font MainFont = new("Segoe UI", 9f, FontStyle.Regular);
    }

    public static class RichTextBoxExtensions
    {        
        public static readonly Dictionary<string, Color> TagColors = new(StringComparer.OrdinalIgnoreCase)
        {
            { "purplecard", ColorTranslator.FromHtml("#CC0099") },
            { "green",      ColorTranslator.FromHtml("#28AA00") },
            { "redcard",    ColorTranslator.FromHtml("#FF0000") },
            { "yellow",     ColorTranslator.FromHtml("#FCDB1C") },
            { "emphasis",   ColorTranslator.FromHtml("#3C96E6") },
            { "orange",   ColorTranslator.FromHtml("#FF9900") }
        };

        private static readonly Regex TagRegex = new(@"#(\w+)#(.*?)#/\1#", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void HighlightCustomTags(this RichTextBox rtb)
        {
            if (string.IsNullOrEmpty(rtb.Text)) return;

            int start = rtb.SelectionStart;
            int len = rtb.SelectionLength;

            rtb.SuspendLayout();

            try
            {
                rtb.SelectAll();
                rtb.SelectionColor = SystemColors.WindowText;
                rtb.SelectionFont = new Font(rtb.Font, FontStyle.Regular);

                var matches = TagRegex.Matches(rtb.Text);

                foreach (Match match in matches)
                {
                    string tagName = match.Groups[1].Value.ToLower();
                    var innerGroup = match.Groups[2];

                    if (!TagColors.TryGetValue(tagName, out Color tagColor))
                    {
                        tagColor = TagColors["purplecard"];
                    }
                    rtb.Select(match.Index, match.Length);
                    rtb.SelectionColor = tagColor;
                    rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
                }
            }
            finally
            {
                rtb.Select(start, len);
                rtb.ResumeLayout();
            }
        }
    }
}
