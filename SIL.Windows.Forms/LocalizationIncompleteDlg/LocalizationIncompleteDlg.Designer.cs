namespace SIL.Windows.Forms.LocalizationIncompleteDlg
{
	partial class LocalizationIncompleteDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocalizationIncompleteDlg));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this._lblLocalizationIncomplete = new System.Windows.Forms.Label();
			this._txtUserEmailAddress = new System.Windows.Forms.TextBox();
			this._lblEmailAddress = new System.Windows.Forms.Label();
			this._lblUsers = new System.Windows.Forms.Label();
			this._numUsers = new System.Windows.Forms.NumericUpDown();
			this._linkCrowdinAndEmailInstructions = new System.Windows.Forms.LinkLabel();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._chkAbleToHelp = new System.Windows.Forms.CheckBox();
			this._lblMoreInformationEmail = new System.Windows.Forms.Label();
			this._btnCopy = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._numUsers)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 3, 7);
			this.tableLayoutPanel1.Controls.Add(this._lblLocalizationIncomplete, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._txtUserEmailAddress, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this._lblEmailAddress, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._lblUsers, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._numUsers, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this._chkAbleToHelp, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this._lblMoreInformationEmail, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this._btnCopy, 2, 6);
			this.tableLayoutPanel1.Controls.Add(this.btnOk, 2, 7);
			this.tableLayoutPanel1.Controls.Add(this._linkCrowdinAndEmailInstructions, 1, 5);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 8;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(576, 290);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.btnCancel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.btnCancel, null);
			this.l10NSharpExtender1.SetLocalizingId(this.btnCancel, "Common.CancelButton");
			this.btnCancel.Location = new System.Drawing.Point(498, 264);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 0;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.btnOk, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.btnOk, null);
			this.l10NSharpExtender1.SetLocalizingId(this.btnOk, "Common.OKButton");
			this.btnOk.Location = new System.Drawing.Point(417, 264);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// _lblLocalizationIncomplete
			// 
			this._lblLocalizationIncomplete.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._lblLocalizationIncomplete, 4);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._lblLocalizationIncomplete, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._lblLocalizationIncomplete, "Param 0: Localization Manager name (usually the same as the application name); Pa" +
        "ram 1: Name of the selected user interface language.");
			this.l10NSharpExtender1.SetLocalizingId(this._lblLocalizationIncomplete, "DialogBoxes.LocalizationIncompleteDlg._lblLocalizationI" +
        "ncomplete");
			this._lblLocalizationIncomplete.Location = new System.Drawing.Point(3, 0);
			this._lblLocalizationIncomplete.Name = "_lblLocalizationIncomplete";
			this._lblLocalizationIncomplete.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
			this._lblLocalizationIncomplete.Size = new System.Drawing.Size(562, 51);
			this._lblLocalizationIncomplete.TabIndex = 2;
			this._lblLocalizationIncomplete.Text = resources.GetString("_lblLocalizationIncomplete.Text");
			// 
			// _txtUserEmailAddress
			// 
			this._txtUserEmailAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this._txtUserEmailAddress, 3);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._txtUserEmailAddress, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._txtUserEmailAddress, null);
			this.l10NSharpExtender1.SetLocalizingId(this._txtUserEmailAddress, "DialogBoxes.LocalizationIncompleteDlg.textBox1");
			this._txtUserEmailAddress.Location = new System.Drawing.Point(84, 54);
			this._txtUserEmailAddress.Name = "_txtUserEmailAddress";
			this._txtUserEmailAddress.Size = new System.Drawing.Size(489, 20);
			this._txtUserEmailAddress.TabIndex = 4;
			// 
			// _lblEmailAddress
			// 
			this._lblEmailAddress.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this._lblEmailAddress.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._lblEmailAddress, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._lblEmailAddress, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._lblEmailAddress, L10NSharp.LocalizationPriority.Medium);
			this.l10NSharpExtender1.SetLocalizingId(this._lblEmailAddress, "DialogBoxes.LocalizationIncompleteDlg._lblEmailAddress");
			this._lblEmailAddress.Location = new System.Drawing.Point(3, 57);
			this._lblEmailAddress.Name = "_lblEmailAddress";
			this._lblEmailAddress.Size = new System.Drawing.Size(75, 13);
			this._lblEmailAddress.TabIndex = 3;
			this._lblEmailAddress.Text = "Email address:";
			// 
			// _lblUsers
			// 
			this._lblUsers.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._lblUsers, 4);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._lblUsers, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._lblUsers, "Param 0: Selected user interface language");
			this.l10NSharpExtender1.SetLocalizationPriority(this._lblUsers, L10NSharp.LocalizationPriority.Medium);
			this.l10NSharpExtender1.SetLocalizingId(this._lblUsers, "DialogBoxes.LocalizationIncompleteDlg._lblUsers");
			this._lblUsers.Location = new System.Drawing.Point(3, 77);
			this._lblUsers.Name = "_lblUsers";
			this._lblUsers.Padding = new System.Windows.Forms.Padding(0, 12, 0, 8);
			this._lblUsers.Size = new System.Drawing.Size(566, 33);
			this._lblUsers.TabIndex = 5;
			this._lblUsers.Text = "Approximately how many users or teams do you know of who would benefit from havin" +
    "g this software localized into {0}?";
			// 
			// _numUsers
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this._numUsers, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._numUsers, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._numUsers, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this._numUsers, "DialogBoxes.LocalizationIncompleteDlg._numUsers");
			this._numUsers.Location = new System.Drawing.Point(84, 113);
			this._numUsers.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this._numUsers.Name = "_numUsers";
			this._numUsers.Size = new System.Drawing.Size(120, 20);
			this._numUsers.TabIndex = 6;
			this._numUsers.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// _linkCrowdinAndEmailInstructions
			// 
			this._linkCrowdinAndEmailInstructions.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._linkCrowdinAndEmailInstructions.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._linkCrowdinAndEmailInstructions, 3);
			this._linkCrowdinAndEmailInstructions.LinkArea = new System.Windows.Forms.LinkArea(0, 39);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._linkCrowdinAndEmailInstructions, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._linkCrowdinAndEmailInstructions, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._linkCrowdinAndEmailInstructions, L10NSharp.LocalizationPriority.Medium);
			this.l10NSharpExtender1.SetLocalizingId(this._linkCrowdinAndEmailInstructions, "DialogBoxes.LocalizationIncompleteDlg._linkCrowdinAndEm" +
        "ailInstructions");
			this._linkCrowdinAndEmailInstructions.Location = new System.Drawing.Point(389, 175);
			this._linkCrowdinAndEmailInstructions.Name = "_linkCrowdinAndEmailInstructions";
			this._linkCrowdinAndEmailInstructions.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
			this._linkCrowdinAndEmailInstructions.Size = new System.Drawing.Size(184, 21);
			this._linkCrowdinAndEmailInstructions.TabIndex = 7;
			this._linkCrowdinAndEmailInstructions.TabStop = true;
			this._linkCrowdinAndEmailInstructions.Text = "Help with localization or see progress.";
			this._linkCrowdinAndEmailInstructions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._lblCrowdinAndEmailInstructions_LinkClicked);
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = null;
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes";
			// 
			// _chkAbleToHelp
			// 
			this._chkAbleToHelp.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._chkAbleToHelp, 3);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._chkAbleToHelp, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._chkAbleToHelp, "Param 0: Localization Manager name (usually the same as the application name); Pa" +
        "ram 1: Name of the selected user interface language.");
			this.l10NSharpExtender1.SetLocalizingId(this._chkAbleToHelp, "DialogBoxes.LocalizationIncompleteDlg.checkBox1");
			this._chkAbleToHelp.Location = new System.Drawing.Point(84, 139);
			this._chkAbleToHelp.Name = "_chkAbleToHelp";
			this._chkAbleToHelp.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
			this._chkAbleToHelp.Size = new System.Drawing.Size(428, 33);
			this._chkAbleToHelp.TabIndex = 8;
			this._chkAbleToHelp.Text = "I am able to help with localization of {0} into {1}, or I can help  find someone " +
    "who can.";
			this._chkAbleToHelp.UseVisualStyleBackColor = true;
			// 
			// _lblMoreInromationEmail
			// 
			this._lblMoreInformationEmail.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this._lblMoreInformationEmail.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._lblMoreInformationEmail, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._lblMoreInformationEmail, "Param 0: email address");
			this.l10NSharpExtender1.SetLocalizationPriority(this._lblMoreInformationEmail, L10NSharp.LocalizationPriority.Medium);
			this.l10NSharpExtender1.SetLocalizingId(this._lblMoreInformationEmail, "DialogBoxes.LocalizationIncompleteDlg._lblMoreInformationEmail");
			this._lblMoreInformationEmail.Location = new System.Drawing.Point(84, 202);
			this._lblMoreInformationEmail.Name = "_lblMoreInformationEmail";
			this._lblMoreInformationEmail.Size = new System.Drawing.Size(149, 13);
			this._lblMoreInformationEmail.TabIndex = 9;
			this._lblMoreInformationEmail.Text = "For more information, email {0}";
			// 
			// _btnCopy
			// 
			this._btnCopy.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this._btnCopy.AutoSize = true;
			this._btnCopy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._btnCopy.FlatAppearance.BorderSize = 0;
			this._btnCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._btnCopy.Image = global::SIL.Windows.Forms.Properties.Resources.copy_icon;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._btnCopy, "Copy email address to clipboard");
			this.l10NSharpExtender1.SetLocalizationComment(this._btnCopy, null);
			this.l10NSharpExtender1.SetLocalizingId(this._btnCopy, "DialogBoxes.LocalizationIncompleteDlg._btnCopy");
			this._btnCopy.Location = new System.Drawing.Point(236, 196);
			this._btnCopy.Margin = new System.Windows.Forms.Padding(0);
			this._btnCopy.Name = "_btnCopy";
			this._btnCopy.Size = new System.Drawing.Size(26, 26);
			this._btnCopy.TabIndex = 10;
			this._btnCopy.UseVisualStyleBackColor = true;
			this._btnCopy.Click += new System.EventHandler(this._btnCopy_Click);
			// 
			// LocalizationIncompleteDlg
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(600, 314);
			this.Controls.Add(this.tableLayoutPanel1);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, "This will be replaced by a localization manager name.");
			this.l10NSharpExtender1.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this, "LocalizationIncompleteDlg.WindowTitle");
			this.Name = "LocalizationIncompleteDlg";
			this.Padding = new System.Windows.Forms.Padding(12);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Localization Incomplete";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._numUsers)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label _lblLocalizationIncomplete;
		private System.Windows.Forms.TextBox _txtUserEmailAddress;
		private System.Windows.Forms.Label _lblEmailAddress;
		private System.Windows.Forms.Label _lblUsers;
		private System.Windows.Forms.NumericUpDown _numUsers;
		private System.Windows.Forms.LinkLabel _linkCrowdinAndEmailInstructions;
		private System.Windows.Forms.CheckBox _chkAbleToHelp;
		private System.Windows.Forms.Label _lblMoreInformationEmail;
		private System.Windows.Forms.Button _btnCopy;
	}
}