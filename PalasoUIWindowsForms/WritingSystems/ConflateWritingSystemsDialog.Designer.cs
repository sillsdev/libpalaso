namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class ConflateWritingSystemsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConflateWritingSystemsDialog));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this._wsLabel = new System.Windows.Forms.Label();
			this._toLabel = new System.Windows.Forms.Label();
			this._wsSelectionComboBox = new System.Windows.Forms.ComboBox();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._infoTextLabel = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._infoTextLabel, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(314, 146);
			this.tableLayoutPanel1.TabIndex = 0;
			//
			// tableLayoutPanel2
			//
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this._wsLabel, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this._toLabel, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this._wsSelectionComboBox, 2, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 81);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(308, 27);
			this.tableLayoutPanel2.TabIndex = 0;
			//
			// _wsLabel
			//
			this._wsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this._wsLabel.AutoSize = true;
			this._wsLabel.Location = new System.Drawing.Point(3, 0);
			this._wsLabel.Name = "_wsLabel";
			this._wsLabel.Size = new System.Drawing.Size(131, 27);
			this._wsLabel.TabIndex = 0;
			this._wsLabel.Text = "lang-scpt-RE-vari-x-private";
			this._wsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// _toLabel
			//
			this._toLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this._toLabel.AutoSize = true;
			this._toLabel.Location = new System.Drawing.Point(140, 0);
			this._toLabel.Name = "_toLabel";
			this._toLabel.Size = new System.Drawing.Size(16, 27);
			this._toLabel.TabIndex = 1;
			this._toLabel.Text = "to";
			this._toLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// _wsSelectionComboBox
			//
			this._wsSelectionComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._wsSelectionComboBox.FormattingEnabled = true;
			this._wsSelectionComboBox.Location = new System.Drawing.Point(162, 3);
			this._wsSelectionComboBox.Name = "_wsSelectionComboBox";
			this._wsSelectionComboBox.Size = new System.Drawing.Size(143, 21);
			this._wsSelectionComboBox.TabIndex = 2;
			//
			// tableLayoutPanel3
			//
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Controls.Add(this._cancelButton, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this._okButton, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 114);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(308, 29);
			this.tableLayoutPanel3.TabIndex = 1;
			//
			// _cancelButton
			//
			this._cancelButton.Location = new System.Drawing.Point(157, 3);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 0;
			this._cancelButton.Text = "Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(76, 3);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "OK";
			this._okButton.UseVisualStyleBackColor = true;
			//
			// _infoTextLabel
			//
			this._infoTextLabel.AutoSize = true;
			this._infoTextLabel.Location = new System.Drawing.Point(3, 0);
			this._infoTextLabel.MaximumSize = new System.Drawing.Size(320, 0);
			this._infoTextLabel.Name = "_infoTextLabel";
			this._infoTextLabel.Size = new System.Drawing.Size(308, 78);
			this._infoTextLabel.TabIndex = 2;
			this._infoTextLabel.Text = resources.GetString("_infoTextLabel.Text");
			//
			// ConflateWritingSystemsDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(337, 168);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "ConflateWritingSystemsDialog";
			this.Text = "Conflate Writing Systems";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label _wsLabel;
		private System.Windows.Forms.Label _toLabel;
		private System.Windows.Forms.ComboBox _wsSelectionComboBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Label _infoTextLabel;
	}
}