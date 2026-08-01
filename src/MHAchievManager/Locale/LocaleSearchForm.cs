using MHAchievManager.Models;
using MHAchievManager.Services;
using OpenCalligraphy.Core.GameData;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MHAchievManager.Locale
{
    public partial class LocaleSearchForm : Form
    {
        public LocaleStringId SelectedLocaleId { get; private set; }
        private string _achievPath;

        public LocaleSearchForm(LocaleStringId selectId, string achievPath)
        {
            InitializeComponent();
            InitLocalesGrid();
            SelectedLocaleId = selectId;
            _achievPath = achievPath;
            txtSearch.Text = ((ulong)SelectedLocaleId).ToString();
            txtSearch.SelectionStart = txtSearch.Text.Length;
            txtSearch.SelectionLength = 0;
        }

        private void EnableDoubleBuffering(DataGridView dgv)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgv, new object[] { true });
        }

        private void InitLocalesGrid()
        {
            gridLocales.Columns.Clear();

            gridLocales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                DataPropertyName = "DisplayId",
                HeaderText = "ID",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.DimGray }                
            });

            gridLocales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colText",
                HeaderText = "Text",
                DataPropertyName = "Text",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            gridLocales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colUsed",
                HeaderText = "Used",
                DataPropertyName = "Used",
                Width = 40,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            gridLocales.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 167, 69);
            gridLocales.DefaultCellStyle.SelectionForeColor = Color.White;
            gridLocales.DataError += (s, e) =>
            {
                e.ThrowException = false;
            };

            EnableDoubleBuffering(gridLocales);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshSearchList(txtSearch.Text);
        }

        private void RefreshSearchList(string filter = "")
        {
            gridLocales.AutoGenerateColumns = false;
            gridLocales.DataSource = null;
            gridLocales.DataSource = AchievementRepository.Instance.GetLocaleItemsForCurrentLanguage(filter).ToList();
        }

        private void gridLocales_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            if (gridLocales.Rows[e.RowIndex].DataBoundItem is LocaleListItem selected)
            {
                SelectedLocaleId = selected.Id;
                txtSearch.Text = selected.DisplayId;
                txtSearch.SelectionStart = txtSearch.Text.Length;
                txtSearch.SelectionLength = 0;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ulong.TryParse(txtSearch.Text.Trim(), out ulong parsedId))
            {
                SelectedLocaleId = (LocaleStringId)parsedId;
            }
            else if (gridLocales.SelectedRows.Count > 0 &&
                     gridLocales.SelectedRows[0].DataBoundItem is LocaleListItem selected)
            {
                SelectedLocaleId = selected.Id;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            LocaleStringId newId = AchievementRepository.Instance.GenerateNewLocaleId(_achievPath);

            SelectedLocaleId = newId;
            txtSearch.Text = ((ulong)newId).ToString();

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
