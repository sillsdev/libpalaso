namespace SIL.Windows.Forms.FileSystem
{
	partial class ConfirmFileOverwriteDlg
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
			System.Windows.Forms.Label labelFileExists;
			System.Windows.Forms.Label labelOverwriteConfirmation;
			this.lblFilename = new System.Windows.Forms.Label();
			this.chkApplyToAll = new System.Windows.Forms.CheckBox();
			this.btnYes = new System.Windows.Forms.Button();
			this.btnNo = new System.Windows.Forms.Button();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			labelFileExists = new System.Windows.Forms.Label();
			labelOverwriteConfirmation = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// labelFileExists
			// 
			labelFileExists.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(labelFileExists, null);
			this.l10NSharpExtender1.SetLocalizationComment(labelFileExists, null);
			this.l10NSharpExtender1.SetLocalizationPriority(labelFileExists, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(labelFileExists, "ConfirmFileOverwriteDlg.labelFileExists");
			labelFileExists.Location = new System.Drawing.Point(13, 13);
			labelFileExists.Name = "labelFileExists";
			labelFileExists.Size = new System.Drawing.Size(92, 13);
			labelFileExists.TabIndex = 0;
			labelFileExists.Text = "File already exists:";
			// 
			// labelOverwriteConfirmation
			// 
			labelOverwriteConfirmation.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(labelOverwriteConfirmation, null);
			this.l10NSharpExtender1.SetLocalizationComment(labelOverwriteConfirmation, null);
			this.l10NSharpExtender1.SetLocalizationPriority(labelOverwriteConfirmation, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(labelOverwriteConfirmation, "ConfirmFileOverwriteDlg.labelOverwriteConfirmation");
			labelOverwriteConfirmation.Location = new System.Drawing.Point(13, 43);
			labelOverwriteConfirmation.Name = "labelOverwriteConfirmation";
			labelOverwriteConfirmation.Size = new System.Drawing.Size(139, 13);
			labelOverwriteConfirmation.TabIndex = 2;
			labelOverwriteConfirmation.Text = "Do you want to overwrite it?";
			// 
			// lblFilename
			// 
			this.lblFilename.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.lblFilename, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.lblFilename, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.lblFilename, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.lblFilename, "ConfirmFileOverwriteDlg.lblFilename");
			this.lblFilename.Location = new System.Drawing.Point(13, 28);
			this.lblFilename.Name = "lblFilename";
			this.lblFilename.Size = new System.Drawing.Size(14, 13);
			this.lblFilename.TabIndex = 1;
			this.lblFilename.Text = "#";
			// 
			// chkApplyToAll
			// 
			this.chkApplyToAll.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.chkApplyToAll, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.chkApplyToAll, "To control which character will be the mnemonic key (underlined when the user presses the ALT key), put the ampersand before the desired character.");
			this.l10NSharpExtender1.SetLocalizingId(this.chkApplyToAll, "ConfirmFileOverwriteDlg.chkApplyToAll");
			this.chkApplyToAll.Location = new System.Drawing.Point(16, 69);
			this.chkApplyToAll.Name = "chkApplyToAll";
			this.chkApplyToAll.Size = new System.Drawing.Size(98, 17);
			this.chkApplyToAll.TabIndex = 3;
			this.chkApplyToAll.Text = "&Apply to all files";
			this.chkApplyToAll.UseVisualStyleBackColor = true;
			// 
			// btnYes
			// 
			this.btnYes.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.btnYes, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.btnYes, "To control which character will be the mnemonic key (underlined when the user presses the ALT key), put the ampersand before the desired character.");
			this.l10NSharpExtender1.SetLocalizingId(this.btnYes, "Common.Yes");
			this.btnYes.Location = new System.Drawing.Point(130, 97);
			this.btnYes.Name = "btnYes";
			this.btnYes.Size = new System.Drawing.Size(75, 23);
			this.btnYes.TabIndex = 4;
			this.btnYes.Text = "&Yes";
			this.btnYes.UseVisualStyleBackColor = true;
			// 
			// btnNo
			// 
			this.btnNo.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.btnNo, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.btnNo, "To control which character will be the mnemonic key (underlined when the user presses the ALT key), put the ampersand before the desired character.");
			this.l10NSharpExtender1.SetLocalizingId(this.btnNo, "Common.No");
			this.btnNo.Location = new System.Drawing.Point(211, 97);
			this.btnNo.Name = "btnNo";
			this.btnNo.Size = new System.Drawing.Size(75, 23);
			this.btnNo.TabIndex = 5;
			this.btnNo.Text = "&No";
			this.btnNo.UseVisualStyleBackColor = true;
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Palaso";
			this.l10NSharpExtender1.PrefixForNewItems = null;
			// 
			// ConfirmFileOverwriteDlg
			// 
			this.AcceptButton = this.btnNo;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnNo;
			this.ClientSize = new System.Drawing.Size(416, 131);
			this.Controls.Add(this.btnNo);
			this.Controls.Add(this.btnYes);
			this.Controls.Add(this.chkApplyToAll);
			this.Controls.Add(labelOverwriteConfirmation);
			this.Controls.Add(this.lblFilename);
			this.Controls.Add(labelFileExists);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "ConfirmFileOverwriteDlg.WindowTitle");
			this.MaximumSize = new System.Drawing.Size(6000, 170);
			this.MinimumSize = new System.Drawing.Size(16, 170);
			this.Name = "ConfirmFileOverwriteDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Overwrite Existing File?";
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblFilename;
		private System.Windows.Forms.CheckBox chkApplyToAll;
		private System.Windows.Forms.Button btnYes;
		private System.Windows.Forms.Button btnNo;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}