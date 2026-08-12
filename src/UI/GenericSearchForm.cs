using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MHAchievManager.UI
{
    public abstract class GenericSearchForm : Form
    {
        protected virtual int MaxResults => 150; 
        public Point SearchLocation = new (63, 12);
        public Point GridLocation = new (12, 45);
        private bool _isProgrammaticChange = false;

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
            AutoScaleMode = AutoScaleMode.Dpi;
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
                Location = new Point(txtSearch.ClientSize.Width - 23, -2),
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
            lblSearch = new Label { 
                Text = "Search:",
                Location = new Point(12, 15),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            SetupSearchClearButton();

            gridResults = new DataGridView
            {
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
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnCreateNew = CreateButton("Add New", btnCreateNew_Click);
            btnSave = CreateButton("Ok", btnSave_Click);
            btnCancel = CreateButton("Cancel", btnCancel_Click);

            txtSearch.TextChanged += txtSearch_TextChanged;
            gridResults.CellClick += gridResults_CellClick;
            gridResults.SelectionChanged += gridResults_SelectionChanged;
            gridResults.CellDoubleClick += gridResults_CellDoubleClick;

            // Suppress standard DataError crash
            gridResults.DataError += (s, e) => { e.ThrowException = false; };

            ClientSize = new Size(620, 440);
            MinimumSize = new Size(553, 342);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ShowIcon = false;

            Controls.AddRange([lblSearch, txtSearch, gridResults, lblStatus, btnCreateNew, btnSave, btnCancel]);

            SetupForm();
            SetupColumns(gridResults);
            LayoutControls();

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            LayoutControls();
        }

        private int S(int px) => LogicalToDeviceUnits(px);

        private void UpdateColumnMinimumWidths()
        {
            if (gridResults == null || gridResults.IsDisposed) return;

            gridResults.RowTemplate.Height = S(22);
            gridResults.SuspendLayout();

            try
            {
                foreach (DataGridViewColumn col in gridResults.Columns)
                {
                    if (col.Tag is int baseMinWidth && baseMinWidth > 0)
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        col.MinimumWidth = S(baseMinWidth);
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                    }
                }

                if (gridResults.DataSource != null)
                {
                    var ds = gridResults.DataSource;
                    gridResults.DataSource = null;
                    gridResults.DataSource = ds;
                }
                else if (gridResults.Rows.Count > 0)
                {
                    gridResults.Invalidate();
                }
            }
            finally
            {
                gridResults.ResumeLayout(true);
            }
        }

        private void LayoutControls()
        {
            int margin = S(12);
            int spacing = S(6);
            int bottomButtonHeight = S(25);
            int bottomRowHeight = S(35);

            txtSearch.Location = new Point(S(SearchLocation.X), S(SearchLocation.Y));
            txtSearch.Height = S(23);
            txtSearch.Width = ClientSize.Width - txtSearch.Left - margin;

            gridResults.Location = new Point(S(GridLocation.X), S(GridLocation.Y));
            gridResults.Size = new Size(
                ClientSize.Width - margin * 2,
                ClientSize.Height - S(GridLocation.Y) - bottomRowHeight - margin
            );

            UpdateColumnMinimumWidths();

            int bottomY = ClientSize.Height - margin - bottomButtonHeight;

            btnCancel.Location = new Point(ClientSize.Width - margin - btnCancel.Width, bottomY);
            btnSave.Location = new Point(btnCancel.Left - spacing - btnSave.Width, bottomY);
            btnCreateNew.Location = new Point(btnSave.Left - spacing - btnCreateNew.Width, bottomY);

            lblStatus.Location = new Point(margin, bottomY + S(1));
            lblStatus.Width = btnCreateNew.Left - margin - S(10);
        }

        private static Button CreateButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
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
                TaskDialog.ShowDialog(this, new TaskDialogPage
                {
                    Caption = "Warning",
                    Text = "Database is not loaded! Please open Game Folder before searching.",
                    Icon = TaskDialogIcon.Warning,
                    Buttons = { TaskDialogButton.OK }
                });
                DialogResult = DialogResult.Abort;
                Close();
                return;
            }
            
            EnableDoubleBuffering(gridResults);

            SetInitialSearchText();
            RefreshSearchList(txtSearch.Text);

            UpdateSearchFromSelection();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            btnClearSearch.Visible = !string.IsNullOrEmpty(txtSearch.Text);
            if (_isProgrammaticChange) return;
            RefreshSearchList(txtSearch.Text);
        }

        private void gridResults_SelectionChanged(object sender, EventArgs e)
        {
            if (gridResults.ContainsFocus)
            {
                UpdateSearchFromSelection();
            }
        }

        private void gridResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                UpdateSearchFromSelection();
            }
        }

        private async void UpdateSearchFromSelection()
        {
            var item = gridResults.CurrentRow?.DataBoundItem;
            if (item == null) return;

            _isProgrammaticChange = true;
            try
            {
                await OnSelectedItemChangedAsync(item);
                txtSearch.Text = GetTextFromItem(item);
                txtSearch.SelectionStart = txtSearch.Text.Length;
                txtSearch.SelectionLength = 0;
            }
            finally
            {
                _isProgrammaticChange = false;
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

        protected virtual Task OnSelectedItemChangedAsync(object item)
        {
            return Task.CompletedTask;
        }

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