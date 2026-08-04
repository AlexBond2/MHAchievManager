using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MHAchievManager.UI
{
    public abstract class GenericSearchForm : Form
    {
        protected virtual int MaxResults => 150;

        // Protected fields so child classes can tweak properties if needed
        protected Label lblSearch;
        protected TextBox txtSearch;
        private Button btnClearSearch;
        protected DataGridView gridResults;
        protected Label lblStatus;
        protected Button btnCreateNew;
        protected Button btnSave;
        protected Button btnCancel;

        // Stores the final selected result
        public object SelectedId { get; protected set; }

        public GenericSearchForm()
        {
            InitializeComponent();
        }

        private void SetupSearchClearButton()
        {
            btnClearSearch = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Width = 23,
                Height = 23,
                Location = new Point(txtSearch.ClientSize.Width - txtSearch.Height, -2),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnClearSearch.FlatAppearance.BorderSize = 0;
            btnClearSearch.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnClearSearch.FlatAppearance.MouseOverBackColor = Theme.ItemSelectedBg;

            btnClearSearch.MouseEnter += (s, e) => btnClearSearch.ForeColor = Theme.TextSelected;
            btnClearSearch.MouseLeave += (s, e) => btnClearSearch.ForeColor = Color.Gray;

            btnClearSearch.Click += (s, e) =>
            {
                txtSearch.Clear();
                txtSearch.Focus();
            };

            txtSearch.Controls.Add(btnClearSearch);
        }

        private void InitializeComponent()
        {
            lblSearch = new Label { Text = "Search:", Location = new Point(12, 15), AutoSize = true };

            txtSearch = new TextBox
            {
                Location = new Point(63, 12),
                Size = new Size(545, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            SetupSearchClearButton();

            gridResults = new DataGridView
            {
                Location = new Point(12, 45),
                Size = new Size(596, 348),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
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

            gridResults.DefaultCellStyle.SelectionBackColor = Theme.ItemSelectedBg;
            gridResults.DefaultCellStyle.SelectionForeColor = Theme.TextSelected;

            lblStatus = new Label
            {
                Text = "",
                Location = new Point(12, 407),
                Size = new Size(320, 17),
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnCreateNew = CreateButton("Add New", new Point(356, 403), btnCreateNew_Click);
            btnSave = CreateButton("Ok", new Point(442, 403), btnSave_Click);
            btnCancel = CreateButton("Cancel", new Point(528, 403), btnCancel_Click);

            txtSearch.TextChanged += txtSearch_TextChanged;
            gridResults.CellClick += gridResults_CellClick; 
            gridResults.CellDoubleClick += gridResults_CellDoubleClick;

            // Suppress standard DataError crash
            gridResults.DataError += (s, e) => { e.ThrowException = false; };

            ClientSize = new Size(620, 440);
            MinimumSize = new Size(553, 342);
            AutoScaleMode = AutoScaleMode.Inherit;
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ShowIcon = false;

            Controls.AddRange([lblSearch, txtSearch, gridResults, lblStatus, btnCreateNew, btnSave, btnCancel]);

            SetupForm();
            SetupColumns(gridResults);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static Button CreateButton(string text, Point location, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                UseVisualStyleBackColor = true
            };
            if (onClick != null) btn.Click += onClick;
            return btn;
        }

        private void EnableDoubleBuffering(DataGridView dgv)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgv, new object[] { true });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Check if database is loaded before proceeding
            if (!CheckDatabaseLoaded())
            {
                MessageBox.Show("Database is not loaded! Please open PakFile before searching.",
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Abort;
                Close();
                return;
            }
            
            EnableDoubleBuffering(gridResults);

            SetInitialSearchText();
            RefreshSearchList(txtSearch.Text);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            btnClearSearch.Visible = !string.IsNullOrEmpty(txtSearch.Text);

            RefreshSearchList(txtSearch.Text);
        }

        private void gridResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var item = gridResults.Rows[e.RowIndex].DataBoundItem;
            if (item != null)
            {
                txtSearch.Text = GetTextFromItem(item);
                txtSearch.SelectionStart = txtSearch.Text.Length;
                txtSearch.SelectionLength = 0;
            }
        }

        private void gridResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnSave_Click(sender, e);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            object selectedItem = gridResults.SelectedRows.Count > 0 ? gridResults.SelectedRows[0].DataBoundItem : null;
            SelectedId = ParseResult(txtSearch.Text, selectedItem);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            SelectedId = GenerateNewItem();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // --- Abstract/Virtual hooks for child implementations ---

        // Return true if DB is loaded, false otherwise
        protected abstract bool CheckDatabaseLoaded();

        // Set up form titles, placeholders, and button visibility
        protected abstract void SetupForm();

        // Define columns for gridResults
        protected abstract void SetupColumns(DataGridView grid);

        // Set the initial text in txtSearch
        protected abstract void SetInitialSearchText();

        // Filter and update data source
        protected abstract void RefreshSearchList(string filter);

        // Extract string representation for the search box when row is clicked
        protected abstract string GetTextFromItem(object item);

        // Parse final ID to store in SelectedId
        protected abstract object ParseResult(string searchText, object selectedItem);

        // Virtual, because not every search needs "Add New"
        protected virtual object GenerateNewItem() => null;

        protected void UpdateStatusLabel(int displayedCount, int totalMatches)
        {
            if (totalMatches > MaxResults)
                lblStatus.Text = $"Showing top {displayedCount} of {totalMatches:N0} matches";
            else
                lblStatus.Text = $"Found {totalMatches:N0} items";
        }
    }
}