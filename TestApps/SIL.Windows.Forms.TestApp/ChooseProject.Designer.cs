namespace SIL.Windows.Forms.TestApp
{
    partial class ChooseProject
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
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._projectsList = new SIL.Windows.Forms.TestApp.ExistingProjectsList();
            this._tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this._tableLayoutPanelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(213, 296);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(69, 24);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this._okButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(288, 296);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(69, 24);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "&Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _projectsList
            // 
            this._projectsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._projectsList.IncludeHiddenProjects = false;
            this._projectsList.Location = new System.Drawing.Point(3, 3);
            this._projectsList.Name = "_projectsList";
            this._projectsList.Size = new System.Drawing.Size(339, 262);
            this._projectsList.TabIndex = 0;
            this._projectsList.SelectedProjectChanged += new System.EventHandler(this._projectsList_SelectedProjectChanged);
            this._projectsList.DoubleClick += new System.EventHandler(this._projectsList_DoubleClick);
            // 
            // _tableLayoutPanelMain
            // 
            this._tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tableLayoutPanelMain.ColumnCount = 1;
            this._tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelMain.Controls.Add(this._projectsList, 0, 0);
            this._tableLayoutPanelMain.Location = new System.Drawing.Point(12, 12);
            this._tableLayoutPanelMain.Name = "_tableLayoutPanelMain";
            this._tableLayoutPanelMain.RowCount = 1;
            this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelMain.Size = new System.Drawing.Size(345, 268);
            this._tableLayoutPanelMain.TabIndex = 4;
            // 
            // ChooseProject
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(369, 332);
            this.Controls.Add(this._tableLayoutPanelMain);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(385, 370);
            this.Name = "ChooseProject";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Choose Project";
            this._tableLayoutPanelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

		private ExistingProjectsList _projectsList;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.TableLayoutPanel _tableLayoutPanelMain;
	}
}
