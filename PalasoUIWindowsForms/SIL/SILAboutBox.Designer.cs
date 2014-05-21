using System.Windows.Forms;
using Palaso.UI.WindowsForms.HtmlBrowser;

namespace Palaso.UI.WindowsForms.SIL
{
	partial class SILAboutBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			this._buildDate = new System.Windows.Forms.Label();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._versionNumber = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._browser = new Palaso.UI.WindowsForms.HtmlBrowser.XWebBrowser();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
			this._buildDate.Location = new System.Drawing.Point(12, 429);
			this._buildDate.Name = "_buildDate";
			this._buildDate.Size = new System.Drawing.Size(60, 13);
			this._buildDate.TabIndex = 1;
			this._buildDate.Text = "build date";
			// 
			// _L10NSharpExtender
			// 
			this._L10NSharpExtender.LocalizationManagerId = "Bloom";
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
			this._versionNumber.Location = new System.Drawing.Point(12, 405);
			this._versionNumber.Name = "_versionNumber";
			this._versionNumber.Size = new System.Drawing.Size(44, 13);
			this._versionNumber.TabIndex = 6;
			this._versionNumber.Text = "version";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Palaso.UI.WindowsForms.Properties.Resources.SILLogoBlue132x184;
			this._L10NSharpExtender.SetLocalizableToolTip(this.pictureBox1, null);
			this._L10NSharpExtender.SetLocalizationComment(this.pictureBox1, null);
			this._L10NSharpExtender.SetLocalizingId(this.pictureBox1, "AboutDialog.pictureBox1");
			this.pictureBox1.Location = new System.Drawing.Point(15, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(101, 144);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 8;
			this.pictureBox1.TabStop = false;
			// 
			// _browser
			// 
			this._browser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._browser.IsWebBrowserContextMenuEnabled = false;
			this._L10NSharpExtender.SetLocalizableToolTip(this._browser, null);
			this._L10NSharpExtender.SetLocalizationComment(this._browser, null);
			this._L10NSharpExtender.SetLocalizingId(this._browser, "AboutDialog.Browser");
			this._browser.Location = new System.Drawing.Point(142, 12);
			this._browser.Name = "_browser";
			this._browser.Size = new System.Drawing.Size(431, 435);
			this._browser.TabIndex = 2;
			this._browser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// SILAboutBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.WhiteSmoke;
			this.ClientSize = new System.Drawing.Size(585, 459);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this._versionNumber);
			this.Controls.Add(this._browser);
			this.Controls.Add(this._buildDate);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.MediumLow);
			this._L10NSharpExtender.SetLocalizingId(this, "AboutDialog.AboutDialogWindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SILAboutBox";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About";
			this.Shown += new System.EventHandler(this.SILAboutBoxShown);
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label _buildDate;
		private XWebBrowser _browser;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
		private System.Windows.Forms.Label _versionNumber;
		private PictureBox pictureBox1;

	}
}
