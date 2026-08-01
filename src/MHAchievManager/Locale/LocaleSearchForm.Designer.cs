namespace MHAchievManager.Locale
{
    partial class LocaleSearchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCreateNew = new System.Windows.Forms.Button();
            gridLocales = new System.Windows.Forms.DataGridView();
            txtSearch = new System.Windows.Forms.TextBox();
            lblSearch = new System.Windows.Forms.Label();
            btnSave = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // btnCreateNew
            // 
            btnCreateNew.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCreateNew.Location = new System.Drawing.Point(289, 305);
            btnCreateNew.Name = "btnCreateNew";
            btnCreateNew.Size = new System.Drawing.Size(80, 25);
            btnCreateNew.TabIndex = 7;
            btnCreateNew.Text = "Add New";
            btnCreateNew.UseVisualStyleBackColor = true;
            btnCreateNew.Click += btnCreateNew_Click;
            // 
            // gridLocales
            // 
            gridLocales.AllowUserToAddRows = false;
            gridLocales.AllowUserToDeleteRows = false;
            gridLocales.AllowUserToResizeRows = false;
            gridLocales.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridLocales.BackgroundColor = System.Drawing.Color.White;
            gridLocales.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            gridLocales.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            gridLocales.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridLocales.GridColor = System.Drawing.Color.FromArgb(240, 240, 240);
            gridLocales.Location = new System.Drawing.Point(12, 45);
            gridLocales.MultiSelect = false;
            gridLocales.Name = "gridLocales";
            gridLocales.ReadOnly = true;
            gridLocales.RowHeadersVisible = false; // Убираем серую колонку-указатель слева
            gridLocales.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            gridLocales.Size = new System.Drawing.Size(529, 250);
            gridLocales.TabIndex = 5;
            gridLocales.CellClick += gridLocales_CellClick;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtSearch.Location = new System.Drawing.Point(63, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Enter ID or part of the text to search...";
            txtSearch.Size = new System.Drawing.Size(478, 23);
            txtSearch.TabIndex = 4;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new System.Drawing.Point(12, 15);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new System.Drawing.Size(45, 15);
            lblSearch.TabIndex = 3;
            lblSearch.Text = "Search:";
            // 
            // btnSave
            // 
            btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSave.Location = new System.Drawing.Point(375, 305);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(80, 25);
            btnSave.TabIndex = 9;
            btnSave.Text = "Ok";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.Location = new System.Drawing.Point(461, 305);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(80, 25);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // LocaleSearchForm
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            ClientSize = new System.Drawing.Size(553, 342);
            AcceptButton = btnSave;
            CancelButton = btnCancel;
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnCreateNew);
            Controls.Add(gridLocales);
            Controls.Add(txtSearch);
            Controls.Add(lblSearch);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LocaleSearchForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Select Locale";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnCreateNew;
        private System.Windows.Forms.DataGridView gridLocales;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}