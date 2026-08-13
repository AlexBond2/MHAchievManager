using MHAchievManager.Services;
using MHAchievManager.UI;
using OpenCalligraphy.Core.GameData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class AssetListItem
    {
        public AssetId Id { get; set; }
        public ulong NumericId => (ulong)Id;
        public string Name { get; set; }
        public string TypeName { get; set; }
    }

    public class AssetIdSearchForm(AssetId initialId) : GenericSearchForm
    {
        private readonly AssetId _initialId = initialId;

        private PictureBox picPreview;

        protected override bool CheckDatabaseLoaded()
        {
            return DataDirectory.Instance != null && DataDirectory.Instance.DataChecksum != 0;
        }

        protected override void SetupForm()
        {
            Text = "Select Asset";
            txtSearch.PlaceholderText = $"Search Asset by ID or Name (showing top {MaxResults})...";
            btnCreateNew.Visible = false;

            picPreview = new PictureBox
            {
                Location = new Point(12, 10),
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Theme.ItemSelectedBg
            };

            Controls.Add(picPreview);

            lblSearch.Location = new Point(60, 21);

            SearchLocation = new Point(115, 18);
            GridLocation = new Point(12, 58);
        }

        protected override void SetInitialSearchText()
        {
            ulong numericVal = (ulong)_initialId;
            if (numericVal != 0)
            {
                txtSearch.Text = numericVal.ToString();
            }
        }

        protected override void SetupColumns(DataGridView grid)
        {
            grid.Columns.Clear();

            // 1. Asset ID
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                DataPropertyName = "NumericId",
                HeaderText = "Asset ID",
                Tag = 140,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.DimGray }
            });

            // 2. Name / Path
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName",
                DataPropertyName = "Name",
                HeaderText = "Name / Path",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // 3. Type
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colType",
                DataPropertyName = "TypeName",
                HeaderText = "Type",
                Tag = 130,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.DarkSlateGray }
            });
        }

        private static readonly HashSet<AssetTypeId> ValidIconTypeIds =
        [
            (AssetTypeId)626889096647806287, // UI/Types/AchievementIconPath.type
            (AssetTypeId)11490316223117528695, // Powers/Types/PowerIconPathType.type
            (AssetTypeId)6803875633425420004 // Entity/Types/EntityIconPathType.type
        ];

        protected override void RefreshSearchList(string filter)
        {
            filter = filter?.Trim() ?? string.Empty;

            var dict = AssetDirectory.Instance.GetAssetsType();

            var results = new List<AssetListItem>();
            int totalMatches = 0;

            foreach (var kvp in dict)
            {
                AssetId assetId = kvp.Key;
                AssetTypeId typeId = kvp.Value;

                if (!ValidIconTypeIds.Contains(typeId)) continue;

                string name = assetId.GetName();
                if (!UpkRepository.Instance.HasAsset(name)) continue;

                string typeName = Path.GetFileNameWithoutExtension(typeId.GetName());
                string idStr = ((ulong)assetId).ToString();

                if (string.IsNullOrEmpty(filter) ||
                    idStr.Contains(filter) ||
                    name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    typeName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    totalMatches++;

                    if (MaxResults <= 0 || results.Count < MaxResults)
                    {
                        results.Add(new AssetListItem
                        {
                            Id = assetId,
                            Name = name,
                            TypeName = typeName
                        });
                    }
                }
            }

            gridResults.DataSource = results;
            UpdateStatusLabel(results.Count, totalMatches);
        }

        private async Task UpdateIconPreviewAsync(AssetId icon)
        {
            picPreview.Image = UpkRepository.Instance.GetBlankIcon();

            if (icon == AssetId.Invalid)
                return;

            Image iconImage = await UpkRepository.Instance.GetIconImageAsync(icon);
            picPreview.Image = iconImage;
        }

        protected override async Task OnSelectedItemChangedAsync(object item)
        {
            if (item is AssetListItem selected)
            {
                await UpdateIconPreviewAsync(selected.Id);
            }
        }

        protected override string GetTextFromItem(object item)
        {
            if (item is AssetListItem selected) 
            {
                return selected.NumericId.ToString();
            }
            return string.Empty;
        }

        protected override object ParseResult(string searchText, object selectedItem)
        {
            if (ulong.TryParse(searchText.Trim(), out ulong parsedId))
                return parsedId;

            if (selectedItem is AssetListItem selected)
                return selected.NumericId;

            return (ulong)_initialId;
        }
    }
}
