namespace SIL.Windows.Forms.Privacy
{
	partial class PrivacyDlg
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
            this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._labelDescription = new System.Windows.Forms.Label();
            this._chkProductAnalytics = new System.Windows.Forms.CheckBox();
            this._chkPropagateDecisionGlobally = new System.Windows.Forms.CheckBox();
            this._labelRestartNote = new System.Windows.Forms.Label();
            this._buttonOK = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this.locExtender = new L10NSharp.Windows.Forms.L10NSharpExtender(this.components);
            this._tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.locExtender)).BeginInit();
            this.SuspendLayout();
            // 
            // _tableLayoutPanel
            // 
            this._tableLayoutPanel.AutoSize = true;
            this._tableLayoutPanel.ColumnCount = 3;
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.Controls.Add(this._labelDescription, 0, 0);
            this._tableLayoutPanel.Controls.Add(this._chkProductAnalytics, 0, 1);
            this._tableLayoutPanel.Controls.Add(this._chkPropagateDecisionGlobally, 0, 2);
            this._tableLayoutPanel.Controls.Add(this._labelRestartNote, 0, 3);
            this._tableLayoutPanel.Controls.Add(this._buttonOK, 1, 4);
            this._tableLayoutPanel.Controls.Add(this._buttonCancel, 2, 4);
            this._tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanel.Location = new System.Drawing.Point(20, 20);
            this._tableLayoutPanel.Name = "_tableLayoutPanel";
            this._tableLayoutPanel.RowCount = 5;
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.Size = new System.Drawing.Size(413, 190);
            this._tableLayoutPanel.TabIndex = 0;
            // 
            // _labelDescription
            // 
            this._labelDescription.AutoSize = true;
            this._tableLayoutPanel.SetColumnSpan(this._labelDescription, 3);
            this.locExtender.SetLocalizableToolTip(this._labelDescription, null);
            this.locExtender.SetLocalizationComment(this._labelDescription, "Param 0: product name; Param 1: Organization name (e.g., \"SIL Global\")");
            this.locExtender.SetLocalizingId(this._labelDescription, "DialogBoxes.PrivacyDlg.DescriptionLabel");
            this._labelDescription.Location = new System.Drawing.Point(0, 0);
            this._labelDescription.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this._labelDescription.MaximumSize = new System.Drawing.Size(380, 0);
            this._labelDescription.Name = "_labelDescription";
            this._labelDescription.Size = new System.Drawing.Size(377, 39);
            this._labelDescription.TabIndex = 0;
            this._labelDescription.Text = "{0} normally reports a small amount of anonymous analytics data to {1} in order t" +
    "o enable the developers to make continued improvements to the software. You can " +
    "opt out of analytics reporting below.";
            // 
            // _chkProductAnalytics
            // 
            this._chkProductAnalytics.AutoSize = true;
            this._tableLayoutPanel.SetColumnSpan(this._chkProductAnalytics, 3);
            this.locExtender.SetLocalizableToolTip(this._chkProductAnalytics, null);
            this.locExtender.SetLocalizationComment(this._chkProductAnalytics, "Param 0: product name");
            this.locExtender.SetLocalizingId(this._chkProductAnalytics, "DialogBoxes.PrivacyDlg.ShareProductAnalyticsCheckbox");
            this._chkProductAnalytics.Location = new System.Drawing.Point(0, 59);
            this._chkProductAnalytics.Margin = new System.Windows.Forms.Padding(0, 10, 0, 5);
            this._chkProductAnalytics.Name = "_chkProductAnalytics";
            this._chkProductAnalytics.Size = new System.Drawing.Size(184, 17);
            this._chkProductAnalytics.TabIndex = 1;
            this._chkProductAnalytics.Text = "Share anonymous {0} usage data";
            this._chkProductAnalytics.UseVisualStyleBackColor = true;
            // 
            // _chkPropagateDecisionGlobally
            // 
            this._chkPropagateDecisionGlobally.AutoSize = true;
            this._tableLayoutPanel.SetColumnSpan(this._chkPropagateDecisionGlobally, 3);
            this._chkPropagateDecisionGlobally.Enabled = false;
            this.locExtender.SetLocalizableToolTip(this._chkPropagateDecisionGlobally, null);
            this.locExtender.SetLocalizationComment(this._chkPropagateDecisionGlobally, "Param 0: \"SIL Global\"");
            this.locExtender.SetLocalizingId(this._chkPropagateDecisionGlobally, "DialogBoxes.PrivacyDlg.ApplyGloballyCheckbox");
            this._chkPropagateDecisionGlobally.Location = new System.Drawing.Point(0, 86);
            this._chkPropagateDecisionGlobally.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this._chkPropagateDecisionGlobally.Name = "_chkPropagateDecisionGlobally";
            this._chkPropagateDecisionGlobally.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this._chkPropagateDecisionGlobally.Size = new System.Drawing.Size(264, 17);
            this._chkPropagateDecisionGlobally.TabIndex = 2;
            this._chkPropagateDecisionGlobally.Text = "Apply this change to other {0} desktop software";
            this._chkPropagateDecisionGlobally.UseVisualStyleBackColor = true;
            // 
            // _labelRestartNote
            // 
            this._labelRestartNote.AutoSize = true;
            this._tableLayoutPanel.SetColumnSpan(this._labelRestartNote, 3);
            this._labelRestartNote.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelRestartNote.ForeColor = System.Drawing.SystemColors.GrayText;
            this.locExtender.SetLocalizableToolTip(this._labelRestartNote, null);
            this.locExtender.SetLocalizationComment(this._labelRestartNote, "");
            this.locExtender.SetLocalizingId(this._labelRestartNote, "DialogBoxes.PrivacyDlg.RestartNoteLabel");
            this._labelRestartNote.Location = new System.Drawing.Point(0, 113);
            this._labelRestartNote.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this._labelRestartNote.MaximumSize = new System.Drawing.Size(380, 0);
            this._labelRestartNote.Name = "_labelRestartNote";
            this._labelRestartNote.Padding = new System.Windows.Forms.Padding(14, 0, 0, 8);
            this._labelRestartNote.Size = new System.Drawing.Size(335, 21);
            this._labelRestartNote.TabIndex = 5;
            this._labelRestartNote.Text = "Changes will take effect in other programs when they are restarted.";
            this._labelRestartNote.Visible = false;
            // 
            // _buttonOK
            // 
            this._buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOK.AutoSize = true;
            this.locExtender.SetLocalizableToolTip(this._buttonOK, null);
            this.locExtender.SetLocalizationComment(this._buttonOK, null);
            this.locExtender.SetLocalizingId(this._buttonOK, "Common.OK");
            this._buttonOK.Location = new System.Drawing.Point(255, 164);
            this._buttonOK.Margin = new System.Windows.Forms.Padding(0);
            this._buttonOK.MinimumSize = new System.Drawing.Size(75, 26);
            this._buttonOK.Name = "_buttonOK";
            this._buttonOK.Size = new System.Drawing.Size(75, 26);
            this._buttonOK.TabIndex = 3;
            this._buttonOK.Text = "OK";
            this._buttonOK.UseVisualStyleBackColor = true;
            this._buttonOK.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.AutoSize = true;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.locExtender.SetLocalizableToolTip(this._buttonCancel, null);
            this.locExtender.SetLocalizationComment(this._buttonCancel, null);
            this.locExtender.SetLocalizingId(this._buttonCancel, "Common.Cancel");
            this._buttonCancel.Location = new System.Drawing.Point(338, 164);
            this._buttonCancel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this._buttonCancel.MinimumSize = new System.Drawing.Size(75, 26);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 26);
            this._buttonCancel.TabIndex = 4;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // locExtender
            // 
            this.locExtender.LocalizationManagerId = "SIL.Windows.Forms.Privacy";
            this.locExtender.PrefixForNewItems = null;
            // 
            // PrivacyDlg
            // 
            this.AcceptButton = this._buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(453, 230);
            this.Controls.Add(this._tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.locExtender.SetLocalizableToolTip(this, null);
            this.locExtender.SetLocalizationComment(this, null);
            this.locExtender.SetLocalizingId(this, "DialogBoxes.PrivacyDlg.WindowTitle");
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrivacyDlg";
            this.Padding = new System.Windows.Forms.Padding(20);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Privacy Settings";
            this._tableLayoutPanel.ResumeLayout(false);
            this._tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.locExtender)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
		private System.Windows.Forms.Label _labelDescription;
		private System.Windows.Forms.CheckBox _chkProductAnalytics;
		private System.Windows.Forms.CheckBox _chkPropagateDecisionGlobally;
		private System.Windows.Forms.Button _buttonOK;
		private System.Windows.Forms.Button _buttonCancel;
		private System.Windows.Forms.Label _labelRestartNote;
		private L10NSharp.Windows.Forms.L10NSharpExtender locExtender;
	}
}
