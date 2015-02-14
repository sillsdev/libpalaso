using System;
using System.Diagnostics;
using SIL.WindowsForms.HtmlBrowser;

namespace SIL.WindowsForms.ReleaseNotes
{
	partial class ShowReleaseNotesDialog
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
			}
			_temp = null;
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._browser = new XWebBrowser();
			this._okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _browser
			//
			this._browser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
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
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ShowReleaseNotesDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Release Notes";
			this.Load += new System.EventHandler(this.ShowReleaseNotesDialog_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private XWebBrowser _browser;
		private System.Windows.Forms.Button _okButton;
	}
}