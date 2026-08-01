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
        private Label lblSearch;
        private TextBox txtSearch;
        private DataGridView gridLocales;
        private Button btnCreateNew;
        private Button btnSave;
        private Button btnCancel;

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

        private void InitializeComponent()
        {

            lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(12, 15),
                Size = new Size(45, 15),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new Point(63, 12),
                Size = new Size(478, 23),
                PlaceholderText = "Enter ID or Text to search...",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            gridLocales = new DataGridView
            {
                Location = new Point(12, 45),
                Size = new Size(529, 250),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(240, 240, 240),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnCreateNew = new Button
            {
                Text = "Add New",
                Location = new Point(289, 305),
                Size = new Size(80, 25),
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnSave = new Button
            {
                Text = "Ok",
                Location = new Point(375, 305),
                Size = new Size(80, 25),
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(461, 305),
                Size = new Size(80, 25),
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            txtSearch.TextChanged += txtSearch_TextChanged;
            gridLocales.CellClick += gridLocales_CellClick;
            btnCreateNew.Click += btnCreateNew_Click;
            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;

            Text = "Select Locale";
            ClientSize = new Size(553, 342);
            AutoScaleMode = AutoScaleMode.Inherit;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            Controls.AddRange(
            [
                lblSearch,
                txtSearch,
                gridLocales,
                btnCreateNew,
                btnSave,
                btnCancel
            ]);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
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
