using MHAchievManager.Services;
using MHAchievManager.UI;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class SavePatchForm : Form
    {
        private TextBox txtInfoPath;
        private TextBox txtStringPath;
        private Label lblInfoAdded;
        private Label lblInfoRemoved;
        private Label lblStringAdded;
        private Label lblStringRemoved;
        private RichTextBox rtbWarnings;
        private Button btnSave;
        private Button btnCancel;

        private readonly SavePatchReport _report;

        private string _infoDirectory;
        private string _stringDirectory;

        public string FinalInfoPath => txtInfoPath.Enabled
            ? Path.Combine(_infoDirectory, txtInfoPath.Text.Trim())
            : _report.TargetInfoFilePath;

        public string FinalStringPath => txtStringPath.Enabled
            ? Path.Combine(_stringDirectory, txtStringPath.Text.Trim())
            : _report.TargetStringFilePath;

        public SavePatchForm(SavePatchReport report)
        {
            _report = report;
            InitializeComponent();
            BindData();

            AutoScaleMode = AutoScaleMode.Dpi;
        }

        private void InitializeComponent()
        {
            Text = "Save Patch Confirmation";
            ClientSize = new Size(584, 261);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // --- BOTTOM: Buttons ---
            btnSave = new Button
            {
                Text = "Save Changes",
                DialogResult = DialogResult.OK,
                Location = new Point(392, 220),
                Size = new Size(100, 30)
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(498, 220),
                Size = new Size(75, 30)
            };

            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            // --- TOP: Paths Panel ---
            var grpPaths = new GroupBox
            {
                Text = "Output Files",
                Location = new Point(12, 12),
                Size = new Size(560, 95)
            };

            var lbl1 = new Label { Text = "InfoMap File:", Location = new Point(15, 25), AutoSize = true };
            txtInfoPath = new TextBox { Location = new Point(105, 22), Size = new Size(350, 20), TextAlign = HorizontalAlignment.Center };
            var flowInfoStats = new FlowLayoutPanel
            {
                Location = new Point(440, 25),
                Size = new Size(110, 20), 
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0)
            };
            lblInfoAdded = new Label
            {
                Text = "+0",
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = Theme.Added,
                Margin = new Padding(0)
            };
            lblInfoRemoved = new Label
            {
                Text = "-0",
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = Theme.Removed,
                Margin = new Padding(6, 0, 0, 0)
            };
            flowInfoStats.Controls.Add(lblInfoRemoved);
            flowInfoStats.Controls.Add(lblInfoAdded);

            var lbl2 = new Label { Text = "StringMap File:", Location = new Point(15, 58), AutoSize = true };
            txtStringPath = new TextBox { Location = new Point(105, 55), Size = new Size(350, 20), TextAlign = HorizontalAlignment.Center };
            var flowStringStats = new FlowLayoutPanel
            {
                Location = new Point(440, 58),
                Size = new Size(110, 20),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0)
            };
            lblStringAdded = new Label
            {
                Text = "+0",
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = Theme.Added,
                Margin = new Padding(0)
            };
            lblStringRemoved = new Label
            {
                Text = "-0",
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = Theme.Removed,
                Margin = new Padding(6, 0, 0, 0)
            };
            flowStringStats.Controls.Add(lblStringRemoved);
            flowStringStats.Controls.Add(lblStringAdded);

            grpPaths.Controls.Add(lbl1);
            grpPaths.Controls.Add(txtInfoPath);
            grpPaths.Controls.Add(flowInfoStats);
            grpPaths.Controls.Add(lbl2);
            grpPaths.Controls.Add(txtStringPath);
            grpPaths.Controls.Add(flowStringStats);
            Controls.Add(grpPaths);

            var grpDetails = new GroupBox
            {
                Text = "Warnings && Layer Conflicts",
                Location = new Point(12, 115),
                Size = new Size(560, 95)
            };

            rtbWarnings = new RichTextBox
            {
                Location = new Point(10, 20),
                Size = new Size(540, 64),
                ReadOnly = true,
                BackColor = SystemColors.Window,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                TabStop = false
            };

            grpDetails.Controls.Add(rtbWarnings);
            Controls.Add(grpDetails);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void BindData()
        {
            _infoDirectory = Path.GetDirectoryName(_report.TargetInfoFilePath);
            _stringDirectory = Path.GetDirectoryName(_report.TargetStringFilePath);

            txtInfoPath.Text = Path.GetFileName(_report.TargetInfoFilePath);
            txtStringPath.Text = Path.GetFileName(_report.TargetStringFilePath);

            bool isNew = _report.IsNewInfoFile && _report.InfoDelta.Count > 0;
            txtInfoPath.ReadOnly = !isNew;
            if (txtInfoPath.ReadOnly && _report.InfoDelta.Count > 0)
            {
                txtInfoPath.BackColor = Theme.ItemSelectedBg;
                txtInfoPath.ForeColor = Theme.TextSelected;
            }

            bool isString = _report.IsNewStringFile && _report.StringDelta.Count > 0;
            txtStringPath.ReadOnly = !isString;
            if (txtStringPath.ReadOnly && _report.StringDelta.Count > 0)
            {
                txtStringPath.BackColor = Theme.ItemSelectedBg;
                txtStringPath.ForeColor = Theme.TextSelected;
            }

            // Summaries
            lblInfoAdded.Text = $"+{_report.InfoAdded}";
            lblInfoRemoved.Text = $"-{_report.InfoRemoved}";

            lblStringAdded.Text = $"+{_report.StringsAdded}";
            lblStringRemoved.Text = $"-{_report.StringsRemoved}";

            // Warnings
            rtbWarnings.Clear();

            bool hasWarnings = _report.InfoWarnings.Count > 0 || _report.StringWarnings.Count > 0;
            bool hasInfoChanges = _report.InfoAdded > 0 || _report.InfoRemoved > 0;
            bool hasStringChanges = _report.StringsAdded > 0 || _report.StringsRemoved > 0;

            btnSave.Enabled = hasInfoChanges || hasStringChanges;

            if (!hasWarnings)
            {
                if (hasInfoChanges || hasStringChanges)
                {
                    rtbWarnings.AppendText("No layer conflicts detected. Safe to save!");
                }
                else
                {
                    rtbWarnings.AppendText("No changes detected in layer merge. Nothing to save.");
                }
            }
            else
            {
                foreach (var warn in _report.InfoWarnings)
                {
                    rtbWarnings.SelectionBackColor = Theme.Warning;
                    rtbWarnings.AppendText("[Info]");
                    rtbWarnings.SelectionBackColor = SystemColors.Window;
                    rtbWarnings.AppendText($" {warn}{Environment.NewLine}");
                }

                foreach (var warn in _report.StringWarnings)
                {
                    rtbWarnings.SelectionBackColor = Theme.Warning;
                    rtbWarnings.AppendText("[String]");
                    rtbWarnings.SelectionBackColor = SystemColors.Window;
                    rtbWarnings.AppendText($" {warn}{Environment.NewLine}");
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInfoPath.Text) || string.IsNullOrWhiteSpace(txtStringPath.Text))
            {
                TaskDialog.ShowDialog(this, new TaskDialogPage
                {
                    Caption = "Validation Error",
                    Text = "File paths cannot be empty!",
                    Icon = TaskDialogIcon.Error,
                    Buttons = { TaskDialogButton.OK }
                });
                DialogResult = DialogResult.None; // Prevent closing
            }
        }
    }
}