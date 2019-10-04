using System.Windows.Forms;
using SIL.Windows.Forms.HtmlBrowser;

namespace SIL.Windows.Forms.Miscellaneous
{
	partial class SILAboutBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private bool disposed = false;
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			disposed = true;
			if (disposing)
			{
				if (components != null)
					components.Dispose();
				if (_browser != null)
				{
					_browser.Dispose();
					_browser = null;
				}
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
			this._buildDate = new System.Windows.Forms.Label();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._versionNumber = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._browser = new SIL.Windows.Forms.HtmlBrowser.XWebBrowser();
			this._checkForUpdates = new System.Windows.Forms.Button();
			this._releaseNotesLabel = new System.Windows.Forms.LinkLabel();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.tableLayoutPanelMain.SuspendLayout();
			this.SuspendLayout();
			//
			// _buildDate
			//
			this._buildDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._buildDate.AutoSize = true;
			this._buildDate.Font = new System.Drawing.Font("Segoe UI", 8F);
			this._L10NSharpExtender.SetLocalizableToolTip(this._buildDate, null);
			this._L10NSharpExtender.SetLocalizationComment(this._buildDate, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._buildDate, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._buildDate, "AboutDialog._versionInfo");
			this._buildDate.Location = new System.Drawing.Point(3, 422);
			this._buildDate.Name = "_buildDate";
			this._buildDate.Size = new System.Drawing.Size(60, 13);
			this._buildDate.TabIndex = 1;
			this._buildDate.Text = "build date";
			//
			// _L10NSharpExtender
			//
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "AboutDialog";
			//
			// _versionNumber
			//
			this._versionNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._versionNumber.AutoSize = true;
			this._versionNumber.Font = new System.Drawing.Font("Segoe UI", 8F);
			this._L10NSharpExtender.SetLocalizableToolTip(this._versionNumber, null);
			this._L10NSharpExtender.SetLocalizationComment(this._versionNumber, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._versionNumber, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._versionNumber, "AboutDialog._versionInfo");
			this._versionNumber.Location = new System.Drawing.Point(3, 403);
			this._versionNumber.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
			this._versionNumber.Name = "_versionNumber";
			this._versionNumber.Size = new System.Drawing.Size(44, 13);
			this._versionNumber.TabIndex = 6;
			this._versionNumber.Text = "version";
			//
			// pictureBox1
			//
			this.pictureBox1.Image = global::SIL.Windows.Forms.Properties.Resources.SILLogoBlue132x184;
			this._L10NSharpExtender.SetLocalizableToolTip(this.pictureBox1, null);
			this._L10NSharpExtender.SetLocalizationComment(this.pictureBox1, null);
			this._L10NSharpExtender.SetLocalizingId(this.pictureBox1, "AboutDialog.pictureBox1");
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(101, 144);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 8;
			this.pictureBox1.TabStop = false;
			//
			// _browser
			//
			this._browser.Dock = System.Windows.Forms.DockStyle.Fill;
			this._L10NSharpExtender.SetLocalizableToolTip(this._browser, null);
			this._L10NSharpExtender.SetLocalizationComment(this._browser, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._browser, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._browser, "AboutDialog.Browser");
			this._browser.Location = new System.Drawing.Point(160, 0);
			this._browser.Margin = new System.Windows.Forms.Padding(0);
			this._browser.Name = "_browser";
			this.tableLayoutPanelMain.SetRowSpan(this._browser, 7);
			this._browser.Size = new System.Drawing.Size(395, 435);
			this._browser.TabIndex = 9;
			this._browser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			this._browser.AllowNavigation = false;
			//
			// _checkForUpdates
			//
			this._L10NSharpExtender.SetLocalizableToolTip(this._checkForUpdates, null);
			this._L10NSharpExtender.SetLocalizationComment(this._checkForUpdates, null);
			this._L10NSharpExtender.SetLocalizingId(this._checkForUpdates, "AboutDialog._checkForUpdates");
			this._checkForUpdates.Location = new System.Drawing.Point(3, 261);
			this._checkForUpdates.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this._checkForUpdates.Name = "_checkForUpdates";
			this._checkForUpdates.Size = new System.Drawing.Size(128, 23);
			this._checkForUpdates.TabIndex = 10;
			this._checkForUpdates.Text = "Check For Updates";
			this._checkForUpdates.UseVisualStyleBackColor = true;
			//
			// _releaseNotesLabel
			//
			this._releaseNotesLabel.AutoSize = true;
			this._releaseNotesLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._releaseNotesLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._L10NSharpExtender.SetLocalizableToolTip(this._releaseNotesLabel, null);
			this._L10NSharpExtender.SetLocalizationComment(this._releaseNotesLabel, null);
			this._L10NSharpExtender.SetLocalizingId(this._releaseNotesLabel, "AboutDialog._releaseNotesLabel");
			this._releaseNotesLabel.Location = new System.Drawing.Point(3, 294);
			this._releaseNotesLabel.Name = "_releaseNotesLabel";
			this._releaseNotesLabel.Size = new System.Drawing.Size(80, 15);
			this._releaseNotesLabel.TabIndex = 11;
			this._releaseNotesLabel.TabStop = true;
			this._releaseNotesLabel.Text = "Release Notes";
			//
			// tableLayoutPanelMain
			//
			this.tableLayoutPanelMain.ColumnCount = 3;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
			this.tableLayoutPanelMain.Controls.Add(this.pictureBox1, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this._versionNumber, 0, 5);
			this.tableLayoutPanelMain.Controls.Add(this._buildDate, 0, 6);
			this.tableLayoutPanelMain.Controls.Add(this._browser, 2, 0);
			this.tableLayoutPanelMain.Controls.Add(this._checkForUpdates, 0, 2);
			this.tableLayoutPanelMain.Controls.Add(this._releaseNotesLabel, 0, 3);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(15, 12);
			this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 7;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(555, 435);
			this.tableLayoutPanelMain.TabIndex = 9;
			//
			// SILAboutBox
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.WhiteSmoke;
			this.ClientSize = new System.Drawing.Size(585, 459);
			this.Controls.Add(this.tableLayoutPanelMain);
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.MediumLow);
			this._L10NSharpExtender.SetLocalizingId(this, "AboutDialog.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(591, 487);
			this.Name = "SILAboutBox";
			this.Padding = new System.Windows.Forms.Padding(15, 12, 15, 12);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About {0}";
			this.Shown += new System.EventHandler(this.SILAboutBoxShown);
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label _buildDate;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
		private System.Windows.Forms.Label _versionNumber;
		private PictureBox pictureBox1;
		private TableLayoutPanel tableLayoutPanelMain;
		private XWebBrowser _browser;
		private Button _checkForUpdates;
		private LinkLabel _releaseNotesLabel;

	}
}
