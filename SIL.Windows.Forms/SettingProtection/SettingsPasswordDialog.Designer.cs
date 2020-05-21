using SIL.Windows.Forms.Widgets;

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
			this._explanation = new BetterLabel();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
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
			this.l10NSharpExtender1.SetLocalizableToolTip(this._explanation, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._explanation, null);
			this.l10NSharpExtender1.SetLocalizingId(this._explanation, "SettingsProtection.PasswordDialog.Password.Explanation");
			this._explanation.Location = new System.Drawing.Point(57, 3);
			this._explanation.Multiline = true;
			this._explanation.Name = "_explanation";
			this._explanation.ReadOnly = true;
			this._explanation.Size = new System.Drawing.Size(368, 48);
			this._explanation.TabIndex = 6;
			this._explanation.TabStop = false;
			this._explanation.Text = "To prevent accidental changes which could cause this program to stop working for you" +
	", these settings have been locked.";
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Palaso";
			this.l10NSharpExtender1.PrefixForNewItems = "SettingsProtection";
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._explanation, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.checkBox1, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.textBox1, 2, 1);
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
			// checkBox1
			//
			this.checkBox1.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.checkBox1, 2);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.checkBox1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.checkBox1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.checkBox1, "SettingsProtection.PasswordDialog.ShowCharactersCheckbox");
			this.checkBox1.Location = new System.Drawing.Point(57, 83);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(107, 17);
			this.checkBox1.TabIndex = 3;
			this.checkBox1.Text = "Show Characters";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			//
			// textBox1
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this.textBox1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.textBox1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.textBox1, "SettingsProtection.SettingsPasswordDialog.textBox1");
			this.textBox1.Location = new System.Drawing.Point(154, 57);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(242, 20);
			this.textBox1.TabIndex = 0;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
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
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.TextBox textBox1;
	}
}