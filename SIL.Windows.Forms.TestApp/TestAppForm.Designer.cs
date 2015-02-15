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
using SIL.Windows.Forms.SuperToolTip;

namespace SIL.Windows.Forms.TestApp
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
			this.components = new System.ComponentModel.Container();
			SuperToolTipInfoWrapper superToolTipInfoWrapper1 = new SuperToolTipInfoWrapper();
			SuperToolTipInfo superToolTipInfo1 = new SuperToolTipInfo();
			this.btnFolderBrowserControl = new System.Windows.Forms.Button();
			this.btnLookupISOCodeDialog = new System.Windows.Forms.Button();
			this.btnWritingSystemSetupDialog = new System.Windows.Forms.Button();
			this.btnArtOfReading = new System.Windows.Forms.Button();
			this.btnSilAboutBox = new System.Windows.Forms.Button();
			this.btnShowReleaseNotes = new System.Windows.Forms.Button();
			this.superToolTip1 = new SuperToolTip.SuperToolTip(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.btnMetaDataEditor = new System.Windows.Forms.Button();
			this.btnSelectFile = new System.Windows.Forms.Button();
			this._silAboutBoxGecko = new System.Windows.Forms.Button();
			this.btnSettingProtectionDialog = new System.Windows.Forms.Button();
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
			this.btnShowReleaseNotes.Location = new System.Drawing.Point(12, 185);
			this.btnShowReleaseNotes.Name = "btnShowReleaseNotes";
			this.btnShowReleaseNotes.Size = new System.Drawing.Size(157, 23);
			this.btnShowReleaseNotes.TabIndex = 0;
			this.btnShowReleaseNotes.Text = "Show Release Notes";
			this.btnShowReleaseNotes.UseVisualStyleBackColor = true;
			this.btnShowReleaseNotes.Click += new System.EventHandler(this.OnShowReleaseNotesClicked);
			// 
			// superToolTip1
			// 
			this.superToolTip1.FadingInterval = 10;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 298);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(149, 13);
			superToolTipInfo1.BackgroundGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			superToolTipInfo1.BackgroundGradientEnd = System.Drawing.Color.Blue;
			superToolTipInfo1.BackgroundGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(251)))));
			superToolTipInfo1.BodyText = "This is the body text";
			superToolTipInfo1.FooterForeColor = System.Drawing.Color.Lime;
			superToolTipInfo1.FooterText = "And this is the footer";
			superToolTipInfo1.HeaderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			superToolTipInfo1.HeaderText = "The header can serve as a title";
			superToolTipInfo1.OffsetForWhereToDisplay = new System.Drawing.Point(0, 0);
			superToolTipInfo1.ShowFooter = true;
			superToolTipInfo1.ShowFooterSeparator = true;
			superToolTipInfoWrapper1.SuperToolTipInfo = superToolTipInfo1;
			superToolTipInfoWrapper1.UseSuperToolTip = true;
			this.superToolTip1.SetSuperStuff(this.label1, superToolTipInfoWrapper1);
			this.label1.TabIndex = 1;
			this.label1.Text = "Hover over me to see a tooltip";
			// 
			// btnMetaDataEditor
			// 
			this.btnMetaDataEditor.Location = new System.Drawing.Point(12, 214);
			this.btnMetaDataEditor.Name = "btnMetaDataEditor";
			this.btnMetaDataEditor.Size = new System.Drawing.Size(157, 23);
			this.btnMetaDataEditor.TabIndex = 0;
			this.btnMetaDataEditor.Text = "Meta Data Editor";
			this.btnMetaDataEditor.UseVisualStyleBackColor = true;
			this.btnMetaDataEditor.Click += new System.EventHandler(this.OnShowMetaDataEditorClicked);
			// 
			// btnSelectFile
			// 
			this.btnSelectFile.Location = new System.Drawing.Point(12, 243);
			this.btnSelectFile.Name = "btnSelectFile";
			this.btnSelectFile.Size = new System.Drawing.Size(157, 23);
			this.btnSelectFile.TabIndex = 0;
			this.btnSelectFile.Text = "Select File";
			this.btnSelectFile.UseVisualStyleBackColor = true;
			this.btnSelectFile.Click += new System.EventHandler(this.OnSelectFileClicked);
			// 
			// _silAboutBoxGecko
			// 
			this._silAboutBoxGecko.Location = new System.Drawing.Point(12, 157);
			this._silAboutBoxGecko.Name = "_silAboutBoxGecko";
			this._silAboutBoxGecko.Size = new System.Drawing.Size(157, 23);
			this._silAboutBoxGecko.TabIndex = 2;
			this._silAboutBoxGecko.Text = "SIL AboutBox (Gecko)";
			this._silAboutBoxGecko.UseVisualStyleBackColor = true;
			this._silAboutBoxGecko.Click += new System.EventHandler(this.OnSilAboutBoxGeckoClicked);
			// 
			// btnSettingProtectionDialog
			// 
			this.btnSettingProtectionDialog.Location = new System.Drawing.Point(12, 272);
			this.btnSettingProtectionDialog.Name = "btnSettingProtectionDialog";
			this.btnSettingProtectionDialog.Size = new System.Drawing.Size(157, 23);
			this.btnSettingProtectionDialog.TabIndex = 3;
			this.btnSettingProtectionDialog.Text = "SettingProtectionDialog";
			this.btnSettingProtectionDialog.UseVisualStyleBackColor = true;
			this.btnSettingProtectionDialog.Click += new System.EventHandler(this.btnSettingProtectionDialog_Click);
			// 
			// TestAppForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(187, 326);
			this.Controls.Add(this.btnSettingProtectionDialog);
			this.Controls.Add(this._silAboutBoxGecko);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnSelectFile);
			this.Controls.Add(this.btnMetaDataEditor);
			this.Controls.Add(this.btnShowReleaseNotes);
			this.Controls.Add(this.btnSilAboutBox);
			this.Controls.Add(this.btnArtOfReading);
			this.Controls.Add(this.btnWritingSystemSetupDialog);
			this.Controls.Add(this.btnLookupISOCodeDialog);
			this.Controls.Add(this.btnFolderBrowserControl);
			this.Name = "TestAppForm";
			this.Text = "PalasoUIWindowsForms.TestApp";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnFolderBrowserControl;
		private System.Windows.Forms.Button btnLookupISOCodeDialog;
		private System.Windows.Forms.Button btnWritingSystemSetupDialog;
		private System.Windows.Forms.Button btnArtOfReading;
		private System.Windows.Forms.Button btnSilAboutBox;
		private System.Windows.Forms.Button btnShowReleaseNotes;
		private SuperToolTip.SuperToolTip superToolTip1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnMetaDataEditor;
		private System.Windows.Forms.Button btnSelectFile;
		private System.Windows.Forms.Button _silAboutBoxGecko;
		private System.Windows.Forms.Button btnSettingProtectionDialog;
	}
}
