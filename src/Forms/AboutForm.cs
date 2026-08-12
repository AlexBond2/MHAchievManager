using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class AboutForm : Form
    {
        private PictureBox iconPictureBox;
        private Label titleLabel;
        private Label versionLabel;
        private Label copyrightLabel;
        private Label licenseLabel;
        private LinkLabel websiteLinkLabel;

        public AboutForm()
        {
            InitializeComponent();
            SetupAboutIcon();
            AutoScaleMode = AutoScaleMode.Dpi;
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
            Size = new Size(464, 141);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            iconPictureBox = new PictureBox
            {
                Location = new (11, 11),
                Size = new Size(128, 119),
                SizeMode = PictureBoxSizeMode.Zoom,
                TabStop = false
            };

            titleLabel = new Label
            {
                Location = new (148, 13),
                Text = "MH Achievement Manager",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true
            };

            versionLabel = new Label
            {
                Location = new(148, 33),
                Text = "Version 1.0.1",
                AutoSize = true
            };

            copyrightLabel = new Label
            {
                Location = new(148, 53),
                Text = "Copyright © 2026 AlexBond",
                AutoSize = true
            };

            licenseLabel = new Label
            {
                Location = new(148, 75),
                Text = "Created for the MHServerEmu community.\r\nCustom tool for managing server achievement data.",
                AutoSize = true
            };

            websiteLinkLabel = new LinkLabel
            {
                Location = new(148, 112),
                Text = "https://github.com/AlexBond2/MHAchievManager",
                AutoSize = true,
                TabStop = true
            };
            websiteLinkLabel.LinkClicked += WebsiteLinkLabel_LinkClicked;

            ClientSize = new Size(464, 141);

            Controls.Add(iconPictureBox);

            Controls.Add(titleLabel);
            Controls.Add(versionLabel);
            Controls.Add(copyrightLabel);
            Controls.Add(licenseLabel);
            Controls.Add(websiteLinkLabel);

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