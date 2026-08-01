using MHAchievManager.Models;
using MHAchievManager.Services;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MHAchievManager.UI
{
    public class LayerItem
    {
        public string DisplayName { get; set; } = "";
        public string FileName { get; set; } = "";
        public bool IsChecked { get; set; } = true;
        public bool IsBase { get; set; } = false;

        public override string ToString() => DisplayName;
    }

    public static class LayerTreeView
    {
        public static void DrawNode(TreeView treeView, object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null || e.Bounds.Width <= 0 || e.Bounds.Height <= 0)
                return;

            bool isSelected = (treeView.SelectedNode == e.Node);

            Color bg = SystemColors.Window;
            Color fg = SystemColors.WindowText;
            Color ic = Theme.ItemSelectedBg;
            Color inv = Color.FromArgb(160, 160, 160);

            if (isSelected)
            {
                bg = Theme.ItemSelectedBg;
                fg = Theme.TextSelected;
                ic = Theme.TextSelected;
            }
            else
            {
                if (e.Node.Tag is CategoryNode nodeData && !nodeData.HasVisibleAchievements)
                {
                    fg = inv;
                }
                else if (e.Node.Tag is AchievementInfo info)
                {
                    if (!info.Enabled || info.EvaluationType == AchievementEvaluationType.Disabled)
                    {
                        fg = inv;
                    }
                    else if (info.VisibleState == AchievementVisibleState.Invisible ||
                             info.VisibleState == AchievementVisibleState.Complete)
                    {
                        fg = Color.Purple;
                    }
                }
            }

            Rectangle fullRowBounds = new(
                0,
                e.Bounds.Y,
                treeView.ClientSize.Width,
                e.Bounds.Height
            );

            using (var bgBrush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(bgBrush, fullRowBounds);
            }

            if (e.Node.Nodes.Count > 0)
            {
                int glyphSize = 8;
                int indent = e.Node.Level * treeView.Indent;
                int x = indent + 6;
                int y = e.Bounds.Y + (e.Bounds.Height / 2) - (glyphSize / 2);

                if (e.Node.IsExpanded)
                {
                    PointF[] openTriangle =
                    [
                        new (x + glyphSize, y + glyphSize),
                        new (x + glyphSize, y),
                        new (x, y + glyphSize)
                    ];

                    using var fillBrush = new SolidBrush(ic);
                    e.Graphics.FillPolygon(fillBrush, openTriangle);
                }
                else
                {
                    PointF[] closedTriangle =
                    [
                        new (x + 1, y),
                        new (x + glyphSize, y + glyphSize / 2f),
                        new (x + 1, y + glyphSize)
                    ];

                    using var strokePen = new Pen(ic, 1.2f);
                    e.Graphics.DrawPolygon(strokePen, closedTriangle);
                }
            }

            var flags = TextFormatFlags.Left
                      | TextFormatFlags.VerticalCenter
                      | TextFormatFlags.NoPrefix
                      | TextFormatFlags.NoClipping
                      | TextFormatFlags.GlyphOverhangPadding;

            Rectangle textRect = new(
                e.Bounds.X,
                e.Bounds.Y,
                e.Bounds.Width + 10,
                e.Bounds.Height
            );

            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                treeView.Font,
                textRect,
                fg,
                flags);
        }
    }

    public class LayerListBox : ListBox
    {
        public event EventHandler ItemCheckChanged;

        public LayerItem ActiveLayer { get; private set; }

        public LayerListBox()
        {
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 22;
            Font = new(Font.FontFamily, 8.5f, FontStyle.Regular);

            SetStyle(ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
        }

        public void SetActiveLayer(LayerItem item)
        {
            if (item != null && item.IsBase) return;

            ActiveLayer = item;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int index = IndexFromPoint(e.Location);
            if (index < 0 || index >= Items.Count) return;

            if (Items[index] is LayerItem item)
            {
                Rectangle checkRect = new(2, index * ItemHeight + 3, 16, 16);

                if (checkRect.Contains(e.Location))
                {
                    if (item.IsBase) return;

                    item.IsChecked = !item.IsChecked;
                    Invalidate();

                    ItemCheckChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if (SelectedItem is LayerItem item)
            {
                if (item.IsBase) return;

                ActiveLayer = item;
                Invalidate();
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;

            if (Items[e.Index] is not LayerItem item) return;

            bool isActiveEdit = item == ActiveLayer;

            Color currentBg = SystemColors.Window;
            Color color = SystemColors.WindowText;

            if (isActiveEdit)
            {
                currentBg = Theme.ItemSelectedBg;
                color = Theme.TextSelected;
            }
            else if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                currentBg = SystemColors.Highlight;
                color = SystemColors.HighlightText;
            }

            using (var bgBrush = new SolidBrush(currentBg))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            Rectangle checkRect = new(e.Bounds.X + 3, e.Bounds.Y + 3, 14, 14);
            ButtonState checkState = item.IsChecked ? ButtonState.Checked : ButtonState.Normal;

            if (item.IsBase)
            {
                checkState |= ButtonState.Inactive;
            }

            ControlPaint.DrawCheckBox(e.Graphics, checkRect, checkState);

            string textToDraw = item.DisplayName;
            Rectangle textRect = new(e.Bounds.X + 22, e.Bounds.Y + 2, e.Bounds.Width - 24, e.Bounds.Height);

            using var textBrush = new SolidBrush(color);
            using var format = new StringFormat
            {
                FormatFlags = StringFormatFlags.NoWrap
            };

            e.Graphics.DrawString(textToDraw, e.Font ?? Theme.MainFont, textBrush, textRect, format);
        }
    }

    public static class RichTextBoxExtensions
    {
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

                    if (!Theme.TagColors.TryGetValue(tagName, out Color tagColor))
                    {
                        tagColor = Theme.TagColors["purplecard"];
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