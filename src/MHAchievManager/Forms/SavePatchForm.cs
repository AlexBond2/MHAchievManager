using MHAchievManager.Services;
using MHAchievManager.UI;
using System;
using System.Collections.Generic;
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
        private TextBox txtWarnings;
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
                ForeColor = ColorTranslator.FromHtml("#22863A"),
                Margin = new Padding(0)
            };
            lblInfoRemoved = new Label
            {
                Text = "-0",
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#CB2431"),
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
                ForeColor = ColorTranslator.FromHtml("#22863A"),
                Margin = new Padding(0)
            };
            lblStringRemoved = new Label
            {
                Text = "-0",
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#CB2431"),
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

            txtWarnings = new TextBox
            {
                Location = new Point(15, 20),
                Size = new Size(530, 65),
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White, 
                TabStop = false
            };

            grpDetails.Controls.Add(txtWarnings);
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
            var allWarnings = new List<string>();
            foreach (var warn in _report.InfoWarnings) allWarnings.Add($"[Info] {warn}");
            foreach (var warn in _report.StringWarnings) allWarnings.Add($"[String] {warn}");

            if (allWarnings.Count == 0)
            {
                txtWarnings.Text = "No warnings or layer conflicts detected. Safe to save!";
            }
            else
            {
                txtWarnings.Text = string.Join(Environment.NewLine, allWarnings);
            }

            btnSave.Enabled =_report.InfoDelta.Count > 0 || _report.StringDelta.Count > 0;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInfoPath.Text) || string.IsNullOrWhiteSpace(txtStringPath.Text))
            {
                MessageBox.Show("File paths cannot be empty!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None; // Prevent closing
            }
        }
    }
}