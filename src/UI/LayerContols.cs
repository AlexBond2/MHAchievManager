using MHAchievManager.Models;
using MHAchievManager.Services;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MHAchievManager.UI
{
    public class LayerItem
    {
        public string DisplayName { get; set; } = "";
        public string FileName { get; set; } = "";
        public bool IsChecked { get; set; } = true;
        public bool IsBase { get; set; } = false;
        public bool IsNew { get; set; } = false;

        public override string ToString() => DisplayName;
    }

    public class LayerTreeView : TreeView
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);

        private const int SB_HORZ = 0;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
        private const int TVM_GETEXTENDEDSTYLE = TV_FIRST + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        public LayerTreeView()
        {
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;
            FullRowSelect = true;
            ShowLines = false;
            DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            IntPtr styles = SendMessage(Handle, TVM_GETEXTENDEDSTYLE, IntPtr.Zero, IntPtr.Zero);
            styles = new IntPtr(styles.ToInt64() | TVS_EX_DOUBLEBUFFER);
            SendMessage(Handle, TVM_SETEXTENDEDSTYLE, IntPtr.Zero, styles);

            UpdateIndent();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateIndent();
        }

        public int S(int pixels) => LogicalToDeviceUnits(pixels);

        private void UpdateIndent()
        {
            Indent = S(19);
            ItemHeight = S(24);
        }

        private int GetScrollX() => GetScrollPos(Handle, SB_HORZ);

        private (Rectangle GlyphRect, int TextX) GetNodeLayout(TreeNode node, Rectangle nodeBounds)
        {
            int scrollX = GetScrollX();
            int indent = (node.Level * S(19)) - scrollX;
            int glyphSize = S(8);
            int x = indent + S(6);
            int y = nodeBounds.Y + (nodeBounds.Height / 2) - (glyphSize / 2);

            Rectangle glyphRect = new(x, y, glyphSize, glyphSize);
            int textX = indent + S(20);

            return (glyphRect, textX);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var hit = HitTest(e.Location);
                TreeNode node = hit.Node;

                if (node != null)
                {
                    if (hit.Location == TreeViewHitTestLocations.PlusMinus)
                    {
                        base.OnMouseDown(e);
                        return;
                    }

                    var (glyphRect, textX) = GetNodeLayout(node, node.Bounds);
                    if (node.Nodes.Count > 0 && glyphRect.Contains(e.Location))
                    {
                        node.Toggle();
                        return;
                    }

                    if (e.X >= textX)
                    {
                        SelectedNode = node;
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (e.Node == null || e.Bounds.Width <= 0 || e.Bounds.Height <= 0)
                return;

            bool isSelected = (SelectedNode == e.Node);

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

            int scrollX = GetScrollX();

            Rectangle fullRowBounds = new(
                -scrollX,
                e.Bounds.Y,
                Math.Max(ClientSize.Width + scrollX, e.Bounds.Width + scrollX),
                e.Bounds.Height
            );

            using (var bgBrush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(bgBrush, fullRowBounds);
            }

            var (glyphRect, textX) = GetNodeLayout(e.Node, e.Bounds);

            if (e.Node.Nodes.Count > 0)
            {
                int glyphSize = S(8);
                int x = glyphRect.X;
                int y = glyphRect.Y;

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
                        new (x + S(1), y),
                        new (x + glyphSize, y + glyphSize / 2f),
                        new (x + S(1), y + glyphSize)
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
                textX,
                e.Bounds.Y,
                ClientSize.Width + scrollX - textX,
                e.Bounds.Height
            );

            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                Font,
                textRect,
                fg,
                flags);
        }
    }

    public class LayerItemCheckEventArgs(LayerItem item, bool newValue) : CancelEventArgs
    {
        public LayerItem Item { get; } = item;
        public bool NewValue { get; } = newValue;
    }

    public class LayerListBox : ListBox
    {
        public event EventHandler<LayerItemCheckEventArgs> ItemCheckChanging;
        public event EventHandler ItemCheckChanged;

        public LayerItem ActiveLayer { get; private set; }

        public LayerListBox()
        {
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;
            DrawMode = DrawMode.OwnerDrawFixed;
            SetStyle(ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
        }

        public void SetActiveLayer(LayerItem item)
        {
            if (ActiveLayer == item) return;
            ActiveLayer = item;
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateItemHeight();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateItemHeight();
        }

        public int S(int pixels) => LogicalToDeviceUnits(pixels);

        private void UpdateItemHeight()
        {
            ItemHeight = S(22);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int index = IndexFromPoint(e.Location);
            if (index < 0 || index >= Items.Count) return;

            if (Items[index] is LayerItem item)
            {
                Rectangle itemRect = GetItemRectangle(index);
                Rectangle checkRect = new(itemRect.X + S(3), itemRect.Y + S(3), S(16), S(16));

                if (checkRect.Contains(e.Location))
                {
                    if (item.IsBase || item.IsNew) return;

                    bool newCheckState = !item.IsChecked;

                    if (ItemCheckChanging != null)
                    {
                        var args = new LayerItemCheckEventArgs(item, newCheckState);
                        ItemCheckChanging?.Invoke(this, args);
                        if (args.Cancel) return;
                    }

                    item.IsChecked = newCheckState;
                    Invalidate(itemRect);
                    ItemCheckChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if (SelectedItem is LayerItem item)
            {
                SetActiveLayer(item);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            if (Items[e.Index] is not LayerItem item) return;

            bool isActiveEdit = (item == ActiveLayer);

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

            Rectangle checkRect = new(e.Bounds.X + S(3), e.Bounds.Y + S(3), S(14), S(14));
            ButtonState checkState = item.IsChecked ? ButtonState.Checked : ButtonState.Normal;

            if (item.IsBase || item.IsNew)
            {
                checkState |= ButtonState.Inactive;
            }

            ControlPaint.DrawCheckBox(e.Graphics, checkRect, checkState);

            Rectangle textRect = new(e.Bounds.X + S(22), e.Bounds.Y, e.Bounds.Width - S(24), e.Bounds.Height);

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(e.Graphics, item.DisplayName, Font ?? Theme.MainFont, textRect, color, flags);
        }
    }
}