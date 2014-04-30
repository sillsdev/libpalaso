// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International. All Rights Reserved.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
namespace PalasoUIWindowsForms.TestApp
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	partial class TestAppForm
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
			this.btnFolderBrowserControl = new System.Windows.Forms.Button();
			this.btnLookupISOCodeDialog = new System.Windows.Forms.Button();
			this.btnWritingSystemSetupDialog = new System.Windows.Forms.Button();
			this.btnArtOfReading = new System.Windows.Forms.Button();
			this.btnSilAboutBox = new System.Windows.Forms.Button();
			this.btnShowReleaseNotes = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnFolderBrowserControl
			// 
			this.btnFolderBrowserControl.Location = new System.Drawing.Point(12, 12);
			this.btnFolderBrowserControl.Name = "btnFolderBrowserControl";
			this.btnFolderBrowserControl.Size = new System.Drawing.Size(157, 23);
			this.btnFolderBrowserControl.TabIndex = 0;
			this.btnFolderBrowserControl.Text = "FolderBrowserControl";
			this.btnFolderBrowserControl.UseVisualStyleBackColor = true;
			this.btnFolderBrowserControl.Click += new System.EventHandler(this.OnFolderBrowserControlClicked);
			// 
			// btnLookupISOCodeDialog
			// 
			this.btnLookupISOCodeDialog.Location = new System.Drawing.Point(12, 41);
			this.btnLookupISOCodeDialog.Name = "btnLookupISOCodeDialog";
			this.btnLookupISOCodeDialog.Size = new System.Drawing.Size(157, 23);
			this.btnLookupISOCodeDialog.TabIndex = 0;
			this.btnLookupISOCodeDialog.Text = "LookupISOCodeDialog";
			this.btnLookupISOCodeDialog.UseVisualStyleBackColor = true;
			this.btnLookupISOCodeDialog.Click += new System.EventHandler(this.OnLookupISOCodeDialogClicked);
			// 
			// btnWritingSystemSetupDialog
			// 
			this.btnWritingSystemSetupDialog.Location = new System.Drawing.Point(12, 70);
			this.btnWritingSystemSetupDialog.Name = "btnWritingSystemSetupDialog";
			this.btnWritingSystemSetupDialog.Size = new System.Drawing.Size(157, 23);
			this.btnWritingSystemSetupDialog.TabIndex = 0;
			this.btnWritingSystemSetupDialog.Text = "WritingSystemSetupDialog";
			this.btnWritingSystemSetupDialog.UseVisualStyleBackColor = true;
			this.btnWritingSystemSetupDialog.Click += new System.EventHandler(this.OnWritingSystemSetupDialogClicked);
			// 
			// btnArtOfReading
			// 
			this.btnArtOfReading.Location = new System.Drawing.Point(12, 99);
			this.btnArtOfReading.Name = "btnArtOfReading";
			this.btnArtOfReading.Size = new System.Drawing.Size(157, 23);
			this.btnArtOfReading.TabIndex = 0;
			this.btnArtOfReading.Text = "ArtOfReading";
			this.btnArtOfReading.UseVisualStyleBackColor = true;
			this.btnArtOfReading.Click += new System.EventHandler(this.OnArtOfReadingClicked);
			// 
			// btnSilAboutBox
			// 
			this.btnSilAboutBox.Location = new System.Drawing.Point(12, 128);
			this.btnSilAboutBox.Name = "btnSilAboutBox";
			this.btnSilAboutBox.Size = new System.Drawing.Size(157, 23);
			this.btnSilAboutBox.TabIndex = 0;
			this.btnSilAboutBox.Text = "SIL AboutBox";
			this.btnSilAboutBox.UseVisualStyleBackColor = true;
			this.btnSilAboutBox.Click += new System.EventHandler(this.OnSilAboutBoxClicked);
			// 
			// btnShowReleaseNotes
			// 
			this.btnShowReleaseNotes.Location = new System.Drawing.Point(12, 157);
			this.btnShowReleaseNotes.Name = "btnShowReleaseNotes";
			this.btnShowReleaseNotes.Size = new System.Drawing.Size(157, 23);
			this.btnShowReleaseNotes.TabIndex = 0;
			this.btnShowReleaseNotes.Text = "Show Release Notes";
			this.btnShowReleaseNotes.UseVisualStyleBackColor = true;
			this.btnShowReleaseNotes.Click += new System.EventHandler(this.OnShowReleaseNotesClicked);
			// 
			// TestAppForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.btnShowReleaseNotes);
			this.Controls.Add(this.btnSilAboutBox);
			this.Controls.Add(this.btnArtOfReading);
			this.Controls.Add(this.btnWritingSystemSetupDialog);
			this.Controls.Add(this.btnLookupISOCodeDialog);
			this.Controls.Add(this.btnFolderBrowserControl);
			this.Name = "TestAppForm";
			this.Text = "PalasoUIWindowsForms.TestApp";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnFolderBrowserControl;
		private System.Windows.Forms.Button btnLookupISOCodeDialog;
		private System.Windows.Forms.Button btnWritingSystemSetupDialog;
		private System.Windows.Forms.Button btnArtOfReading;
		private System.Windows.Forms.Button btnSilAboutBox;
		private System.Windows.Forms.Button btnShowReleaseNotes;
	}
}
