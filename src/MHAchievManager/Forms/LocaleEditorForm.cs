using MHAchievManager.Locale;
using MHAchievManager.Services;
using MHAchievManager.UI;
using OpenCalligraphy.Core.GameData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class LocaleEditorForm : Form
    {
        private Label lblId;
        private TextBox txtSelectedId;
        private Button btnCreateNew;
        private TabControl tabLanguages;
        private TabPage tabPage1;
        private Button btnSave;
        private Button btnCancel;

        public LocaleStringId SelectedLocaleId { get; private set; }

        private LocaleStringId _currentId;
        private Dictionary<string, RichTextBox> _langInputs = [];
        private string _achievPath;

        public LocaleEditorForm(LocaleStringId currentLocaleId, string achievPath)
        {
            _currentId = currentLocaleId;
            SelectedLocaleId = _currentId;
            _achievPath = achievPath;

            InitializeComponent();

            txtSelectedId.BackColor = Theme.ItemSelectedBg;
            txtSelectedId.ForeColor = Theme.TextSelected;

            SetupDynamicLanguageTabs();
            LoadLocaleData();
        }

        private void InitializeComponent()
        {
            lblId = new Label
            {
                Text = "LocaleStringId:",
                Location = new Point(13, 10),
                AutoSize = true
            };

            txtSelectedId = new TextBox
            {
                Location = new Point(104, 7),
                Size = new Size(400, 23),
                ReadOnly = true,
                TextAlign = HorizontalAlignment.Center,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnCreateNew = new Button
            {
                Text = "...",
                Location = new Point(510, 6),
                Size = new Size(61, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            tabLanguages = new TabControl
            {
                Location = new Point(13, 36),
                Size = new Size(558, 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            tabPage1 = new TabPage(GameLocale.DefaultLocale)
            {
                Padding = new Padding(3),
                UseVisualStyleBackColor = true
            };

            btnSave = new Button
            {
                Text = "Ok",
                Location = new Point(404, 155),
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(490, 155),
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;
            btnCreateNew.Click += btnCreateNew_Click;

            tabLanguages.Controls.Add(tabPage1);

            Text = "Locale Editor";
            ClientSize = new Size(584, 192);
            Padding = new Padding(10);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            Controls.AddRange([tabLanguages, lblId, txtSelectedId, btnCreateNew, btnSave, btnCancel]);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadLocaleData()
        {
            SelectAndLoadId(_currentId);
        }

        private void SetupDynamicLanguageTabs()
        {
            tabLanguages.TabPages.Clear();
            _langInputs.Clear();
            tabLanguages.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabLanguages.DrawItem -= tabLanguages_DrawItem;
            tabLanguages.DrawItem += tabLanguages_DrawItem;

            foreach (var locale in GameLocale.Locales)
            {
                AddLanguageTab(locale.Code);
            }
        }

        private void tabLanguages_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabPage = tabLanguages.TabPages[e.Index];
            string langCode = tabPage.Text.ToLower();

            bool hasContent = _langInputs.TryGetValue(langCode, out var txt) && !string.IsNullOrWhiteSpace(txt.Text);
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color textColor = hasContent ? Color.Black : Color.Gray;

            if (isSelected) textColor = Theme.TextSelected;

            // Fill background
            using (Brush bgBrush = new SolidBrush(isSelected ? Theme.ItemSelectedBg : SystemColors.Control))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            Rectangle textRect = e.Bounds;

            textRect.Y += isSelected ? -1 : 2;

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                    TextFormatFlags.VerticalCenter |
                                    TextFormatFlags.SingleLine |
                                    TextFormatFlags.NoPadding; // Removes GDI text margins

            TextRenderer.DrawText(e.Graphics, tabPage.Text, e.Font, textRect, textColor, flags);
        }

        private void AddLanguageTab(string langCode)
        {
            var tabPage = new TabPage(langCode);
            var txt = new RichTextBox
            {
                Dock = DockStyle.Fill,
                DetectUrls = false
            };
            txt.TextChanged += (s, e) =>
            {
                txt.HighlightCustomTags();
                tabLanguages.Invalidate();
            };
            _langInputs[langCode] = txt;
            tabPage.Controls.Add(txt);

            tabLanguages.TabPages.Add(tabPage);
        }

        private void SelectAndLoadId(LocaleStringId id)
        {
            _currentId = id;
            txtSelectedId.Text = ((ulong)id).ToString();

            var translations = AchievementRepository.Instance.GetTranslationsForId(id);

            tabLanguages.SuspendLayout();
            foreach (var (lang, txtControl) in _langInputs)
            {
                string textValue = string.Empty;

                if (translations != null && translations.TryGetValue(lang, out var val))
                {
                    textValue = val ?? string.Empty;
                }

                txtControl.Text = textValue;
                txtControl.HighlightCustomTags();
            }
            tabLanguages.ResumeLayout();
            tabLanguages.Invalidate();
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            using var dialog = new LocaleSearchForm(_currentId, _achievPath);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _currentId = dialog.SelectedLocaleId;
                LoadLocaleData();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var newTranslations = new Dictionary<string, string>();
            foreach (var (lang, txtControl) in _langInputs)
            {
                newTranslations[lang] = txtControl.Text.Trim();
            }

            if (_currentId != LocaleStringId.Invalid)
                AchievementRepository.Instance.UpdateOrCreateLocale(_currentId, newTranslations);

            SelectedLocaleId = _currentId;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}