using SIL.Windows.Forms.Progress;

namespace SIL.Windows.Forms.Archiving
{
	partial class ArchivingDlg
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
			this._buttonCreatePackage = new System.Windows.Forms.Button();
			this._linkOverview = new System.Windows.Forms.LinkLabel();
			this._progressBar = new System.Windows.Forms.ProgressBar();
			this._logBox = new SIL.Windows.Forms.Progress.LogBox();
			this._buttonLaunchRamp = new System.Windows.Forms.Button();
			this._buttonCancel = new System.Windows.Forms.Button();
			this._flowLayoutExtra = new System.Windows.Forms.FlowLayoutPanel();
			this.locExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._chkMetadataOnly = new System.Windows.Forms.CheckBox();
			this._tableLayoutPanel.SuspendLayout();
			this._flowLayoutExtra.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.locExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// _tableLayoutPanel
			// 
			this._tableLayoutPanel.ColumnCount = 3;
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._tableLayoutPanel.Controls.Add(this._buttonCreatePackage, 0, 4);
			this._tableLayoutPanel.Controls.Add(this._linkOverview, 0, 0);
			this._tableLayoutPanel.Controls.Add(this._progressBar, 0, 2);
			this._tableLayoutPanel.Controls.Add(this._logBox, 0, 1);
			this._tableLayoutPanel.Controls.Add(this._buttonLaunchRamp, 1, 4);
			this._tableLayoutPanel.Controls.Add(this._buttonCancel, 2, 4);
			this._tableLayoutPanel.Controls.Add(this._flowLayoutExtra, 0, 3);
			this._tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tableLayoutPanel.Location = new System.Drawing.Point(12, 12);
			this._tableLayoutPanel.Name = "_tableLayoutPanel";
			this._tableLayoutPanel.RowCount = 5;
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.Size = new System.Drawing.Size(355, 408);
			this._tableLayoutPanel.TabIndex = 0;
			// 
			// _buttonCreatePackage
			// 
			this._buttonCreatePackage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._buttonCreatePackage.AutoSize = true;
			this.locExtender.SetLocalizableToolTip(this._buttonCreatePackage, null);
			this.locExtender.SetLocalizationComment(this._buttonCreatePackage, null);
			this.locExtender.SetLocalizingId(this._buttonCreatePackage, "DialogBoxes.ArchivingDlg._buttonCreatePackage");
			this._buttonCreatePackage.Location = new System.Drawing.Point(58, 382);
			this._buttonCreatePackage.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this._buttonCreatePackage.Name = "_buttonCreatePackage";
			this._buttonCreatePackage.Size = new System.Drawing.Size(106, 26);
			this._buttonCreatePackage.TabIndex = 0;
			this._buttonCreatePackage.Text = "&1) Create Package";
			this._buttonCreatePackage.UseVisualStyleBackColor = true;
			this._buttonCreatePackage.Click += new System.EventHandler(this.CreatePackage);

			// 
			// _linkOverview
			// 
			this._linkOverview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._linkOverview.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._linkOverview, 3);
			this._linkOverview.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
			this.locExtender.SetLocalizableToolTip(this._linkOverview, null);
			this.locExtender.SetLocalizationComment(this._linkOverview, "");
			this.locExtender.SetLocalizationPriority(this._linkOverview, L10NSharp.LocalizationPriority.NotLocalizable);
			this.locExtender.SetLocalizingId(this._linkOverview, "DialogBoxes.ArchivingDlg.OverviewText");
			this._linkOverview.Location = new System.Drawing.Point(3, 0);
			this._linkOverview.Name = "_linkOverview";
			this._linkOverview.Size = new System.Drawing.Size(349, 13);
			this._linkOverview.TabIndex = 3;
			this._linkOverview.Text = "#";
			this._linkOverview.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleRampLinkClicked);
			// 
			// _progressBar
			// 
			this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanel.SetColumnSpan(this._progressBar, 3);
			this._progressBar.Location = new System.Drawing.Point(0, 326);
			this._progressBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
			this._progressBar.Name = "_progressBar";
			this._progressBar.Size = new System.Drawing.Size(355, 15);
			this._progressBar.TabIndex = 4;
			// 
			// _logBox
			// 
			this._logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._logBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._logBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
			this._logBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this._logBox.CancelRequested = false;
			this._tableLayoutPanel.SetColumnSpan(this._logBox, 3);
			this._logBox.ErrorEncountered = false;
			this._logBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._logBox.GetDiagnosticsMethod = null;
			this.locExtender.SetLocalizableToolTip(this._logBox, null);
			this.locExtender.SetLocalizationComment(this._logBox, null);
			this.locExtender.SetLocalizingId(this._logBox, "LogBox");
			this._logBox.Location = new System.Drawing.Point(0, 18);
			this._logBox.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
			this._logBox.Name = "_logBox";
			this._logBox.ProgressIndicator = null;
			this._logBox.ShowCopyToClipboardMenuItem = false;
			this._logBox.ShowDetailsMenuItem = false;
			this._logBox.ShowDiagnosticsMenuItem = false;
			this._logBox.ShowFontMenuItem = false;
			this._logBox.ShowMenu = false;
			this._logBox.Size = new System.Drawing.Size(355, 303);
			this._logBox.TabIndex = 5;
			this._logBox.TabStop = false;
			this._logBox.ReportErrorLinkClicked += new System.EventHandler(this.HandleLogBoxReportErrorLinkClicked);
			// 
			// _buttonLaunchRamp
			// 
			this._buttonLaunchRamp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._buttonLaunchRamp.AutoSize = true;
			this._buttonLaunchRamp.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.locExtender.SetLocalizableToolTip(this._buttonLaunchRamp, null);
			this.locExtender.SetLocalizationComment(this._buttonLaunchRamp, null);
			this.locExtender.SetLocalizingId(this._buttonLaunchRamp, "DialogBoxes.ArchivingDlg._buttonLaunchRamp");
			this._buttonLaunchRamp.Location = new System.Drawing.Point(172, 382);
			this._buttonLaunchRamp.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this._buttonLaunchRamp.Name = "_buttonLaunchRamp";
			this._buttonLaunchRamp.Size = new System.Drawing.Size(100, 26);
			this._buttonLaunchRamp.TabIndex = 1;
			this._buttonLaunchRamp.Text = "&2) Launch {0}";
			this._buttonLaunchRamp.UseVisualStyleBackColor = true;
			// 
			// _buttonCancel
			// 
			this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._buttonCancel.AutoSize = true;
			this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.locExtender.SetLocalizableToolTip(this._buttonCancel, null);
			this.locExtender.SetLocalizationComment(this._buttonCancel, null);
			this.locExtender.SetLocalizingId(this._buttonCancel, "DialogBoxes.ArchivingDlg._buttonCancel");
			this._buttonCancel.Location = new System.Drawing.Point(280, 382);
			this._buttonCancel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this._buttonCancel.Name = "_buttonCancel";
			this._buttonCancel.Size = new System.Drawing.Size(75, 26);
			this._buttonCancel.TabIndex = 2;
			this._buttonCancel.Text = "Cancel";
			this._buttonCancel.UseVisualStyleBackColor = true;
			// 
			// _flowLayoutExtra
			// 
			this._flowLayoutExtra.AutoSize = true;
			this._flowLayoutExtra.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._tableLayoutPanel.SetColumnSpan(this._flowLayoutExtra, 3);
			this._flowLayoutExtra.Controls.Add(this._chkMetadataOnly);
			this._flowLayoutExtra.Dock = System.Windows.Forms.DockStyle.Top;
			this._flowLayoutExtra.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this._flowLayoutExtra.Location = new System.Drawing.Point(3, 356);
			this._flowLayoutExtra.Name = "_flowLayoutExtra";
			this._flowLayoutExtra.Size = new System.Drawing.Size(349, 23);
			this._flowLayoutExtra.TabIndex = 6;
			// 
			// locExtender
			// 
			this.locExtender.LocalizationManagerId = "SIL.Windows.Forms.Archiving";
			this.locExtender.PrefixForNewItems = null;
			// 
			// _chkMetadataOnly
			// 
			this._chkMetadataOnly.AutoSize = true;
			this.locExtender.SetLocalizableToolTip(this._chkMetadataOnly, "Select this to prevent copying actual data files into archive");
			this.locExtender.SetLocalizationComment(this._chkMetadataOnly, null);
			this.locExtender.SetLocalizingId(this._chkMetadataOnly, "ArchivingDlg._chkMetadataOnly");
			this._chkMetadataOnly.Location = new System.Drawing.Point(3, 3);
			this._chkMetadataOnly.Name = "_chkMetadataOnly";
			this._chkMetadataOnly.Size = new System.Drawing.Size(93, 17);
			this._chkMetadataOnly.TabIndex = 0;
			this._chkMetadataOnly.Text = "Metadata only";
			this._chkMetadataOnly.UseVisualStyleBackColor = true;
			this._chkMetadataOnly.Visible = false;
			this._chkMetadataOnly.CheckedChanged += new System.EventHandler(this._chkMetadataOnly_CheckedChanged);
			// 
			// ArchivingDlg
			// 
			this.AcceptButton = this._buttonLaunchRamp;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._buttonCancel;
			this.ClientSize = new System.Drawing.Size(379, 432);
			this.Controls.Add(this._tableLayoutPanel);
			this.locExtender.SetLocalizableToolTip(this, null);
			this.locExtender.SetLocalizationComment(this, "Parameter is application name");
			this.locExtender.SetLocalizingId(this, "DialogBoxes.ArchivingDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(355, 320);
			this.Name = "ArchivingDlg";
			this.Padding = new System.Windows.Forms.Padding(12);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "{0}: Archive using {1}";
			this._tableLayoutPanel.ResumeLayout(false);
			this._tableLayoutPanel.PerformLayout();
			this._flowLayoutExtra.ResumeLayout(false);
			this._flowLayoutExtra.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.locExtender)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.LinkLabel _linkOverview;
		private System.Windows.Forms.ProgressBar _progressBar;
		private L10NSharp.UI.L10NSharpExtender locExtender;
		private LogBox _logBox;
		protected System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
		protected System.Windows.Forms.FlowLayoutPanel _flowLayoutExtra;
		protected System.Windows.Forms.Button _buttonLaunchRamp;
		protected System.Windows.Forms.Button _buttonCreatePackage;
		protected System.Windows.Forms.Button _buttonCancel;
		private System.Windows.Forms.CheckBox _chkMetadataOnly;
	}
}