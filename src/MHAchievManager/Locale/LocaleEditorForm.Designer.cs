partial class LocaleEditorForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        btnSave = new System.Windows.Forms.Button();
        btnCancel = new System.Windows.Forms.Button();
        lblId = new System.Windows.Forms.Label();
        txtSelectedId = new System.Windows.Forms.TextBox();
        btnCreateNew = new System.Windows.Forms.Button();
        tabLanguages = new System.Windows.Forms.TabControl();
        tabPage1 = new System.Windows.Forms.TabPage();
        tabLanguages.SuspendLayout();
        SuspendLayout();
        // 
        // btnSave
        // 
        btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        btnSave.Location = new System.Drawing.Point(404, 155);
        btnSave.Name = "btnSave";
        btnSave.Size = new System.Drawing.Size(80, 25);
        btnSave.TabIndex = 1;
        btnSave.Text = "Save";
        btnSave.UseVisualStyleBackColor = true;
        btnSave.Click += btnSave_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        btnCancel.Location = new System.Drawing.Point(490, 155);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new System.Drawing.Size(80, 25);
        btnCancel.TabIndex = 2;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += btnCancel_Click;
        // 
        // lblId
        // 
        lblId.AutoSize = true;
        lblId.Location = new System.Drawing.Point(13, 10);
        lblId.Name = "lblId";
        lblId.Size = new System.Drawing.Size(85, 15);
        lblId.TabIndex = 0;
        lblId.Text = "LocaleStringId:";
        // 
        // txtSelectedId
        // 
        txtSelectedId.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtSelectedId.Location = new System.Drawing.Point(104, 7);
        txtSelectedId.Name = "txtSelectedId";
        txtSelectedId.ReadOnly = true;
        txtSelectedId.Size = new System.Drawing.Size(400, 23);
        txtSelectedId.TabIndex = 1;
        txtSelectedId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        // 
        // btnCreateNew
        // 
        btnCreateNew.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        btnCreateNew.Location = new System.Drawing.Point(510, 6);
        btnCreateNew.Name = "btnCreateNew";
        btnCreateNew.Size = new System.Drawing.Size(61, 24);
        btnCreateNew.TabIndex = 3;
        btnCreateNew.Text = "...";
        btnCreateNew.UseVisualStyleBackColor = true;
        btnCreateNew.Click += btnCreateNew_Click;
        // 
        // tabLanguages
        // 
        tabLanguages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        tabLanguages.Controls.Add(tabPage1);
        tabLanguages.Location = new System.Drawing.Point(13, 36);
        tabLanguages.Name = "tabLanguages";
        tabLanguages.SelectedIndex = 0;
        tabLanguages.Size = new System.Drawing.Size(558, 110);
        tabLanguages.TabIndex = 4;
        // 
        // tabPage1
        // 
        tabPage1.Location = new System.Drawing.Point(4, 24);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new System.Windows.Forms.Padding(3);
        tabPage1.Size = new System.Drawing.Size(550, 82);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "en_us";
        tabPage1.UseVisualStyleBackColor = true;
        // 
        // LocaleEditorForm
        // 
        AcceptButton = btnSave;
        CancelButton = btnCancel;
        ClientSize = new System.Drawing.Size(584, 192);
        Controls.Add(tabLanguages);
        Controls.Add(btnCreateNew);
        Controls.Add(btnCancel);
        Controls.Add(btnSave);
        Controls.Add(txtSelectedId);
        Controls.Add(lblId);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Name = "LocaleEditorForm";
        Padding = new System.Windows.Forms.Padding(10);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Locale Editor";
        tabLanguages.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnCreateNew;
    private System.Windows.Forms.TextBox txtSelectedId;
    private System.Windows.Forms.Label lblId;
    private System.Windows.Forms.TabControl tabLanguages;
    private System.Windows.Forms.TabPage tabPage1;
}