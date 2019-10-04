using System;
using System.Diagnostics;
using SIL.Windows.Forms.HtmlBrowser;

namespace SIL.Windows.Forms.ReleaseNotes
{
	partial class ShowReleaseNotesDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private bool disposed = false;
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			disposed = true;
			if (disposing)
			{
				if (components != null)
					components.Dispose();
				if (_temp != null)
				{
					try
					{
						_temp.Dispose();
					}
					catch (Exception error)
					{
						Debug.Fail(error.Message);
					}
				}
				if (_browser != null)
					_browser.Dispose();
			}
			_temp = null;
			_browser = null;
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
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._browser = new SIL.Windows.Forms.HtmlBrowser.XWebBrowser();
			this._okButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// _L10NSharpExtender
			// 
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "ShowReleaseNotesDialog";
			// 
			// _browser
			// 
			this._browser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._browser, null);
			this._L10NSharpExtender.SetLocalizationComment(this._browser, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._browser, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._browser, "ShowReleaseNotesDialog.ShowReleaseNotesDialog._browser");
			this._browser.Location = new System.Drawing.Point(12, 12);
			this._browser.MinimumSize = new System.Drawing.Size(20, 20);
			this._browser.Name = "_browser";
			this._browser.Size = new System.Drawing.Size(652, 304);
			this._browser.TabIndex = 0;
			this._browser.AllowNavigation = false;
			// 
			// _okButton
			// 
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._L10NSharpExtender.SetLocalizableToolTip(this._okButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._okButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._okButton, "Common.OKButton");
			this._okButton.Location = new System.Drawing.Point(589, 328);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			// 
			// ShowReleaseNotesDialog
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._okButton;
			this.ClientSize = new System.Drawing.Size(676, 363);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._browser);
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizingId(this, "ShowReleaseNotesDialog.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ShowReleaseNotesDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Release Notes";
			this.Load += new System.EventHandler(this.ShowReleaseNotesDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private XWebBrowser _browser;
		private System.Windows.Forms.Button _okButton;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
	}
}