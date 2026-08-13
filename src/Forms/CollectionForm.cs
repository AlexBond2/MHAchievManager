using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class CollectionForm<T> : Form where T : new()
    {
        private readonly int MaxItems;

        public CollectionForm(IList list, int maxItems)
        {
            MaxItems = maxItems;

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;

            Text = $"Collection {typeof(T).Name}";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize = new (280, 260);

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false
            };

            var btnAdd = new Button { Text = "Add", AutoSize = true, MinimumSize = new (70, 24) };
            var btnDel = new Button { Text = "Delete", AutoSize = true, MinimumSize = new (70, 24) };
            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true, MinimumSize = new (70, 24) };

            void refreshList()
            {
                int selected = listBox.SelectedIndex;
                listBox.BeginUpdate();
                listBox.Items.Clear();

                for (int i = 0; i < list.Count; i++)
                {
                    listBox.Items.Add($"{typeof(T).Name} [{i}]");
                }

                listBox.EndUpdate();

                if (selected >= 0 && selected < listBox.Items.Count)
                    listBox.SelectedIndex = selected;
                else if (listBox.Items.Count > 0)
                    listBox.SelectedIndex = listBox.Items.Count - 1;

                btnAdd.Enabled = list.Count < MaxItems;
                btnDel.Enabled = listBox.SelectedIndex >= 0;
            }

            btnAdd.Click += (s, e) =>
            {
                if (list.Count >= MaxItems) return;
                list.Add(new T());
                refreshList();
            };

            btnDel.Click += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    list.RemoveAt(listBox.SelectedIndex);
                    refreshList();
                }
            };

            listBox.SelectedIndexChanged += (s, e) =>
            {
                btnDel.Enabled = listBox.SelectedIndex >= 0;
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(LogicalToDeviceUnits(8)),
                WrapContents = false
            };

            panel.Controls.AddRange([btnOk, btnDel, btnAdd]);

            Controls.Add(listBox);
            Controls.Add(panel);

            AcceptButton = btnOk;

            refreshList();
        }
    }
}
