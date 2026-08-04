using MHAchievManager.Models;
using MHAchievManager.Services;
using MHAchievManager.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class AchievementListItem(int id, string name, string category)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Category { get; set; } = category;
    }

    public class AchievementSearchForm(int initialId) : GenericSearchForm
    {
        private readonly int _initialId = initialId;

        protected override bool CheckDatabaseLoaded()
        {
            return AchievementRepository.Instance?.AllAchievements != null;
        }

        protected override void SetupForm()
        {
            Text = "Select Achievement";
            txtSearch.PlaceholderText = $"Search by ID or Name (showing top {MaxResults} results)...";
            btnCreateNew.Visible = false;
        }

        protected override void SetInitialSearchText()
        {
            if (_initialId != 0)
            {
                txtSearch.Text = _initialId.ToString();
                txtSearch.SelectionStart = txtSearch.Text.Length;
                txtSearch.SelectionLength = 0;
            }
        }

        protected override void SetupColumns(DataGridView grid)
        {
            grid.Columns.Clear();

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                DataPropertyName = "Id",
                HeaderText = "ID",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.DimGray }
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "Name",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCategory",
                HeaderText = "Category",
                DataPropertyName = "Category",
                Width = 160,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.Gray }
            });
        }

        protected override void RefreshSearchList(string filter)
        {
            filter = filter?.Trim() ?? string.Empty;
            bool isNumber = int.TryParse(filter, out int searchId);

            IEnumerable<AchievementInfo> query = AchievementRepository.Instance.AllAchievements;

            if (!string.IsNullOrEmpty(filter))
            {
                if (isNumber)
                {
                    query = query.Where(a => a.Id == searchId);
                }
                else
                {
                    query = query.Where(a =>
                    {
                        string name = AchievementRepository.Instance.GetLocale(a.Name);
                        return name != null && name.Contains(filter, StringComparison.OrdinalIgnoreCase);
                    });
                }
            }

            var matchedList = query.ToList();
            int totalMatches = matchedList.Count;

            var results = matchedList
                .Take(MaxResults)
                .Select(a => new AchievementListItem(
                    (int)a.Id,
                    AchievementRepository.Instance.GetLocale(a.Name),
                    AchievementRepository.Instance.GetLocale(a.CategoryStr)
                ))
                .ToList();

            gridResults.DataSource = results;
            UpdateStatusLabel(results.Count, totalMatches);
        }

        protected override string GetTextFromItem(object item)
        {
            if (item is AchievementListItem selected)
                return selected.Id.ToString();
            return string.Empty;
        }

        protected override object ParseResult(string searchText, object selectedItem)
        {
            if (int.TryParse(searchText.Trim(), out int parsedId))
                return parsedId;

            if (selectedItem is AchievementListItem selected)
                return selected.Id;

            return _initialId;
        }
    }
}