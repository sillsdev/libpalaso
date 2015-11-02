namespace SIL.Windows.Forms.WritingSystems
{
	partial class DeleteInputSystemDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteInputSystemDialog));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this._selectionTable = new System.Windows.Forms.TableLayoutPanel();
			this._wsSelectionComboBox = new System.Windows.Forms.ComboBox();
			this._helpButton = new System.Windows.Forms.Button();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._radioTable = new System.Windows.Forms.TableLayoutPanel();
			this._deleteRadioButton = new System.Windows.Forms.RadioButton();
			this._mergeRadioButton = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanel1.SuspendLayout();
			this._selectionTable.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this._radioTable.SuspendLayout();
			this.SuspendLayout();
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this._selectionTable, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._radioTable, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(274, 125);
			this.tableLayoutPanel1.TabIndex = 0;
			//
			// _selectionTable
			//
			this._selectionTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._selectionTable.AutoSize = true;
			this._selectionTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._selectionTable.ColumnCount = 2;
			this._selectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._selectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._selectionTable.Controls.Add(this._wsSelectionComboBox, 0, 0);
			this._selectionTable.Controls.Add(this._helpButton, 1, 0);
			this._selectionTable.Location = new System.Drawing.Point(3, 55);
			this._selectionTable.Name = "_selectionTable";
			this._selectionTable.RowCount = 1;
			this._selectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._selectionTable.Size = new System.Drawing.Size(268, 32);
			this._selectionTable.TabIndex = 0;
			//
			// _wsSelectionComboBox
			//
			this._wsSelectionComboBox.FormattingEnabled = true;
			this._wsSelectionComboBox.Location = new System.Drawing.Point(3, 3);
			this._wsSelectionComboBox.Name = "_wsSelectionComboBox";
			this._wsSelectionComboBox.Size = new System.Drawing.Size(230, 21);
			this._wsSelectionComboBox.TabIndex = 2;
			//
			// _helpButton
			//
			this._helpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._helpButton.Image = ((System.Drawing.Image)(resources.GetObject("_helpButton.Image")));
			this._helpButton.Location = new System.Drawing.Point(239, 3);
			this._helpButton.Name = "_helpButton";
			this._helpButton.Size = new System.Drawing.Size(26, 26);
			this._helpButton.TabIndex = 3;
			this._helpButton.UseVisualStyleBackColor = true;
			//
			// tableLayoutPanel3
			//
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.Controls.Add(this._cancelButton, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this._okButton, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 93);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(268, 29);
			this.tableLayoutPanel3.TabIndex = 1;
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(190, 3);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 0;
			this._cancelButton.Text = "Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.Location = new System.Drawing.Point(109, 3);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&Delete";
			this._okButton.UseVisualStyleBackColor = true;
			//
			// _radioTable
			//
			this._radioTable.AutoSize = true;
			this._radioTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._radioTable.ColumnCount = 1;
			this._radioTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._radioTable.Controls.Add(this._deleteRadioButton, 0, 0);
			this._radioTable.Controls.Add(this._mergeRadioButton, 0, 1);
			this._radioTable.Location = new System.Drawing.Point(3, 3);
			this._radioTable.Name = "_radioTable";
			this._radioTable.RowCount = 2;
			this._radioTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._radioTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._radioTable.Size = new System.Drawing.Size(159, 46);
			this._radioTable.TabIndex = 2;
			//
			// _deleteRadioButton
			//
			this._deleteRadioButton.AutoSize = true;
			this._deleteRadioButton.Location = new System.Drawing.Point(3, 3);
			this._deleteRadioButton.Name = "_deleteRadioButton";
			this._deleteRadioButton.Size = new System.Drawing.Size(153, 17);
			this._deleteRadioButton.TabIndex = 0;
			this._deleteRadioButton.TabStop = true;
			this._deleteRadioButton.Text = "Delete all data stored in {0}";
			this._deleteRadioButton.UseVisualStyleBackColor = true;
			//
			// _mergeRadioButton
			//
			this._mergeRadioButton.AutoSize = true;
			this._mergeRadioButton.Location = new System.Drawing.Point(3, 26);
			this._mergeRadioButton.Name = "_mergeRadioButton";
			this._mergeRadioButton.Size = new System.Drawing.Size(134, 17);
			this._mergeRadioButton.TabIndex = 1;
			this._mergeRadioButton.TabStop = true;
			this._mergeRadioButton.Text = "Merge all {0} data with:";
			this._mergeRadioButton.UseVisualStyleBackColor = true;
			//
			// DeleteInputSystemDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(357, 160);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DeleteInputSystemDialog";
			this.Text = "Delete Input System";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this._selectionTable.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this._radioTable.ResumeLayout(false);
			this._radioTable.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel _selectionTable;
		private System.Windows.Forms.ComboBox _wsSelectionComboBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.TableLayoutPanel _radioTable;
		private System.Windows.Forms.RadioButton _deleteRadioButton;
		private System.Windows.Forms.RadioButton _mergeRadioButton;
		private System.Windows.Forms.Button _helpButton;
	}
}