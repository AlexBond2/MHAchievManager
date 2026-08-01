using MHAchievManager.Locale;
using MHAchievManager.Services;
using MHAchievManager.UI;
using OpenCalligraphy.Core.GameData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public partial class LocaleEditorForm : Form
{
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