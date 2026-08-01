using MHAchievManager.UI;
using OpenCalligraphy.Core.GameData;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class GuidListItem(long guid, string name)
    {
        public long GuidValue { get; set; } = guid;
        public string Name { get; set; } = name;
    }

    public class GuidPrototypeSearchForm(long initialGuid) : GenericSearchForm
    {
        private readonly long _initialGuid = initialGuid;

        protected override bool CheckDatabaseLoaded()
        {
            return DataDirectory.Instance != null && DataDirectory.Instance.DataChecksum != 0;
        }

        protected override void SetupForm()
        {
            Text = "Select Prototype";

            FormBorderStyle = FormBorderStyle.Sizable;
            Size = new Size(620, 440);

            txtSearch.PlaceholderText = $"Search by GUID or Name (showing top {MaxResults} results)...";
            btnCreateNew.Visible = false;
        }

        protected override void SetInitialSearchText()
        {
            if (_initialGuid != 0)
            {
                txtSearch.Text = _initialGuid.ToString();
                txtSearch.SelectionStart = txtSearch.Text.Length;
                txtSearch.SelectionLength = 0;
            }
        }

        protected override void SetupColumns(DataGridView grid)
        {
            grid.Columns.Clear();

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colGuid",
                DataPropertyName = "GuidValue",
                HeaderText = "Prototype GUID",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.DimGray }
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "Prototype Name",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        protected override void RefreshSearchList(string filter)
        {
            filter = filter?.Trim() ?? string.Empty;
            bool isNumber = long.TryParse(filter, out long searchId);

            var query = DataDirectory.Instance.GetGuidPrototypes();

            if (!string.IsNullOrEmpty(filter))
            {
                if (isNumber)
                {
                    query = query.Where(kvp => ((long)kvp.Key).ToString().Contains(filter));
                }
                else
                {
                    query = query.Where(kvp => kvp.Value.GetName().Contains(filter, StringComparison.OrdinalIgnoreCase));
                }
            }

            int totalMatches = query.Count();

            var results = query
                .Take(MaxResults)
                .Select(kvp => new GuidListItem((long)kvp.Key, kvp.Value.GetName()))
                .ToList();

            gridResults.DataSource = results;

            UpdateStatusLabel(results.Count, totalMatches);
        }

        protected override string GetTextFromItem(object item)
        {
            if (item is GuidListItem selected)
                return selected.GuidValue.ToString();
            return string.Empty;
        }

        protected override object ParseResult(string searchText, object selectedItem)
        {
            if (long.TryParse(searchText.Trim(), out long parsedId))
                return parsedId;

            if (selectedItem is GuidListItem selected)
                return selected.GuidValue;

            return _initialGuid;
        }
    }
}
