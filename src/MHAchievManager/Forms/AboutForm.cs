using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class AboutForm : Form
    {
        private Panel aboutPanel;
        private TableLayoutPanel aboutTableLayoutPanel;
        private PictureBox iconPictureBox;
        private TableLayoutPanel infoTableLayoutPanel;
        private Label titleLabel;
        private Label versionLabel;
        private Label copyrightLabel;
        private Label licenseLabel;
        private LinkLabel websiteLinkLabel;

        public AboutForm()
        {
            InitializeComponent();
            SetupAboutIcon();
        }

        private void SetupAboutIcon()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{assembly.GetName().Name}.Resources.MHAchiev.ico";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var image = Image.FromStream(stream);

            iconPictureBox.Image = new Bitmap(image, 256, 256);
        }

        private void InitializeComponent()
        {
            titleLabel = new Label
            {
                Text = "MH Achievement Manager",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            versionLabel = new Label
            {
                Text = "Version 1.0.0",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            copyrightLabel = new Label
            {
                Text = "Copyright © 2026 AlexBond",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            licenseLabel = new Label
            {
                Text = "Created for the MHServerEmu community.\r\nCustom tool for managing server achievement data.",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            websiteLinkLabel = new LinkLabel
            {
                Text = "https://github.com/AlexBond2/MHAchievManager",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                TabStop = true
            };
            websiteLinkLabel.LinkClicked += WebsiteLinkLabel_LinkClicked;

            iconPictureBox = new PictureBox
            {
                Size = new Size(128, 119),
                SizeMode = PictureBoxSizeMode.Zoom,
                TabStop = false
            };

            infoTableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5
            };
            infoTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            infoTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            infoTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            infoTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            infoTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            infoTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));

            infoTableLayoutPanel.Controls.Add(titleLabel, 0, 0);
            infoTableLayoutPanel.Controls.Add(versionLabel, 0, 1);
            infoTableLayoutPanel.Controls.Add(copyrightLabel, 0, 2);
            infoTableLayoutPanel.Controls.Add(licenseLabel, 0, 3);
            infoTableLayoutPanel.Controls.Add(websiteLinkLabel, 0, 4);

            aboutTableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            aboutTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            aboutTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            aboutTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            aboutTableLayoutPanel.Controls.Add(iconPictureBox, 0, 0);
            aboutTableLayoutPanel.Controls.Add(infoTableLayoutPanel, 1, 0);

            aboutPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            aboutPanel.Controls.Add(aboutTableLayoutPanel);

            ClientSize = new Size(464, 141);
            Controls.Add(aboutPanel);
            Text = "About MH Achievement Manager";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
        }

        private void WebsiteLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {            
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = websiteLinkLabel.Text,
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}