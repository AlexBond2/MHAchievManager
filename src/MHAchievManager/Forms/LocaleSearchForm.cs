using MHAchievManager.Models;
using MHAchievManager.Services;
using OpenCalligraphy.Core.GameData;
using System.Drawing;
using System.Windows.Forms;
using MHAchievManager.UI;

namespace MHAchievManager.Forms
{
    public class LocaleSearchForm : GenericSearchForm
    {
        private readonly string _achievPath;
        private readonly LocaleStringId _initialId;

        // Type-safe wrapper for external access
        public LocaleStringId SelectedLocaleId => SelectedId != null ? (LocaleStringId)SelectedId : _initialId;

        public LocaleSearchForm(LocaleStringId selectId, string achievPath)
        {
            _initialId = selectId;
            _achievPath = achievPath;
        }

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
                Width = 130,
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
                Width = 40,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            grid.DefaultCellStyle.SelectionBackColor = Theme.ItemSelectedBg;
            grid.DefaultCellStyle.SelectionForeColor = Theme.TextSelected;
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