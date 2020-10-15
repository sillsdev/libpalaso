namespace SIL.Windows.Forms.SettingProtection
{
	partial class SettingsPasswordDialog
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
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._explanation = new SIL.Windows.Forms.Widgets.BetterLabel();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.passwordBox = new SIL.Windows.Forms.Widgets.PasswordBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label1, "SettingsProtection.PasswordDialog.FactoryPassword");
			this.label1.Location = new System.Drawing.Point(57, 54);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Factory Password";
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._okButton, "Common.OKButton");
			this._okButton.Location = new System.Drawing.Point(284, 170);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._cancelButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._cancelButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._cancelButton, "Common.CancelButton");
			this._cancelButton.Location = new System.Drawing.Point(365, 170);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 2;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			//
			// pictureBox1
			//
			this.pictureBox1.Image = global::SIL.Windows.Forms.Properties.Resources.lockClosed48x48;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.pictureBox1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.pictureBox1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.pictureBox1, "SettingsProtection.SettingsPasswordDialog.pictureBox1");
			this.pictureBox1.Location = new System.Drawing.Point(3, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(48, 48);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 7;
			this.pictureBox1.TabStop = false;
			//
			// _explanation
			//
			this._explanation.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayoutPanel1.SetColumnSpan(this._explanation, 2);
			this._explanation.Dock = System.Windows.Forms.DockStyle.Fill;
			this._explanation.Enabled = false;
			this._explanation.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._explanation.ForeColor = System.Drawing.SystemColors.ControlText;
			this._explanation.IsTextSelectable = false;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._explanation, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._explanation, null);
			this.l10NSharpExtender1.SetLocalizingId(this._explanation, "SettingsProtection.PasswordDialog.Password.Explanation");
			this._explanation.Location = new System.Drawing.Point(57, 3);
			this._explanation.Multiline = true;
			this._explanation.Name = "_explanation";
			this._explanation.ReadOnly = true;
			this._explanation.Size = new System.Drawing.Size(368, 30);
			this._explanation.TabIndex = 6;
			this._explanation.TabStop = false;
			this._explanation.Text = "To prevent accidental changes which could cause this tool to stop working for you" +
    ", these settings have been locked.";
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Palaso";
			this.l10NSharpExtender1.PrefixForNewItems = "SettingsProtection";
			// 
			// passwordBox
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.passwordBox, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.passwordBox, null);
			this.l10NSharpExtender1.SetLocalizingId(this.passwordBox, "SettingsProtection.SettingsPasswordDialog.passwordBox");
			this.passwordBox.Location = new System.Drawing.Point(154, 57);
			this.passwordBox.Name = "passwordBox";
			this.passwordBox.Size = new System.Drawing.Size(242, 20);
			this.passwordBox.TabIndex = 0;
			this.passwordBox.UseSystemPasswordChar = true;
			this.passwordBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._explanation, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.passwordBox, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.label1, 1, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(428, 146);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// SettingsPasswordDialog
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(464, 205);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "SettingsProtection.PasswordDialog.SettingsPasswordWindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsPasswordDialog";
			this.Text = "Settings Protection Password";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		private Widgets.BetterLabel _explanation;
		private System.Windows.Forms.PictureBox pictureBox1;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private SIL.Windows.Forms.Widgets.PasswordBox passwordBox;
	}
}