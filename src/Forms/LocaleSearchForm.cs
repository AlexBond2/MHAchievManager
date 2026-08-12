using MHAchievManager.Services;
using OpenCalligraphy.Core.GameData;
using System.Drawing;
using System.Windows.Forms;
using MHAchievManager.UI;

namespace MHAchievManager.Forms
{
    public class LocaleListItem
    {
        public LocaleStringId Id { get; set; }
        public string DisplayId => ((ulong)Id).ToString();
        public string Text { get; set; }
        public int Used { get; set; }

        public override string ToString() => $"[{Id}] {Text}";
    }

    public class LocaleSearchForm(LocaleStringId selectId, string achievPath) : GenericSearchForm
    {
        private readonly string _achievPath = achievPath;
        private readonly LocaleStringId _initialId = selectId;

        // Type-safe wrapper for external access
        public LocaleStringId SelectedLocaleId => SelectedId != null ? (LocaleStringId)SelectedId : _initialId;

        protected override bool CheckDatabaseLoaded()
        {
            return true;
        }

        protected override void SetupForm()
        {
            Text = "Select Locale";
            txtSearch.PlaceholderText = "Enter ID or Text to search...";
            btnCreateNew.Visible = true; // Locales support adding new records
        }

        protected override void SetInitialSearchText()
        {
            txtSearch.Text = ((ulong)_initialId).ToString();
            txtSearch.SelectionStart = txtSearch.Text.Length;
            txtSearch.SelectionLength = 0;
        }

        protected override void SetupColumns(DataGridView grid)
        {
            grid.Columns.Clear();

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                DataPropertyName = "DisplayId",
                HeaderText = "ID",
                Tag = 140,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.DimGray }
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colText",
                HeaderText = "Text",
                DataPropertyName = "Text",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colUsed",
                HeaderText = "Used",
                DataPropertyName = "Used",
                Tag = 40,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
        }

        protected override void RefreshSearchList(string filter)
        {
            var results = AchievementRepository.Instance.GetLocaleItemsForCurrentLanguage(filter, MaxResults, out int totalMatches);
            gridResults.DataSource = results;

            UpdateStatusLabel(results.Count, totalMatches);
        }

        protected override string GetTextFromItem(object item)
        {
            if (item is LocaleListItem selected)
                return selected.DisplayId;
            return string.Empty;
        }

        protected override object ParseResult(string searchText, object selectedItem)
        {
            if (ulong.TryParse(searchText.Trim(), out ulong parsedId))
                return (LocaleStringId)parsedId;

            if (selectedItem is LocaleListItem selected)
                return selected.Id;

            return _initialId; // Fallback
        }

        protected override object GenerateNewItem()
        {
            return AchievementRepository.Instance.GenerateNewLocaleId(_achievPath);
        }
    }
}