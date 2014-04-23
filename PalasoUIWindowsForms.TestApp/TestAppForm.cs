﻿// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International. All Rights Reserved.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Palaso.IO;
using Palaso.UI.WindowsForms.ImageGallery;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.ReleaseNotes;
using Palaso.UI.WindowsForms.SIL;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using PalasoUIWindowsForms.TestApp.Properties;

namespace PalasoUIWindowsForms.TestApp
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class TestAppForm : Form
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Default c'tor
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TestAppForm()
		{
			InitializeComponent();
		}

		private void OnFolderBrowserControlClicked(object sender, EventArgs e)
		{
#if __MonoCS__
			MessageBox.Show("FolderBrowserControl not supported on Linux");
#else
			using (var form = new Form())
			{
				var browser = new Palaso.UI.WindowsForms.FolderBrowserControl.FolderBrowserControl();
				browser.Location = new Point(0, 0);
				browser.Width = form.ClientSize.Width;
				browser.Height = form.ClientSize.Height;
				browser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left |
								AnchorStyles.Right;
				browser.ShowOnlyMappedDrives = false;
				browser.ShowAddressbar = true;
				form.Controls.Add(browser);
				form.ShowDialog();
			}
#endif
		}

		private void OnLookupISOCodeDialogClicked(object sender, EventArgs e)
		{
			using (var dialog = new LookupISOCodeDialog())
				dialog.ShowDialog();
		}

		private void OnWritingSystemSetupDialogClicked(object sender, EventArgs e)
		{
			string tempPath = Path.GetTempPath() + "WS-Test";
			Directory.CreateDirectory(tempPath);
			KeyboardController.Initialize();
			try
			{
				var wsRepo = LdmlInFolderWritingSystemRepository.Initialize(tempPath, onMigration, onLoadProblem);
				using (var dialog = new WritingSystemSetupDialog(wsRepo))
				{
					dialog.WritingSystems.LocalKeyboardSettings = Settings.Default.LocalKeyboards;
					dialog.ShowDialog();
					Settings.Default.LocalKeyboards = dialog.WritingSystems.LocalKeyboardSettings;
					Settings.Default.Save();
				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				KeyboardController.Shutdown();
			}
		}

		private void OnArtOfReadingClicked(object sender, EventArgs e)
		{
			using (var dlg = new ArtOfReadingTestForm())
				dlg.ShowDialog();
		}

		private static void onMigration(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
		{
		}

		private static void onLoadProblem(IEnumerable<WritingSystemRepositoryProblem> migrationInfo)
		{
		}

		private void OnSilAboutBoxClicked(object sender, EventArgs e)
		{
			using (var tempfile = new TempFile("<h3>Copyright 2014 <a href=\"http://sil.org\">SIL International</a></h3>" +
				"<p>Testing the <b>about box</b></p>"))
			{
				using (var dlg = new SILAboutBox(tempfile.Path))
					dlg.ShowDialog();
			}
		}

		private void OnShowReleaseNotesClicked(object sender, EventArgs e)
		{
			using (var tempFile = new TempFile(@"
Release Notes Dialog
====================

This dialog takes a [markdown](http://en.wikipedia.org/wiki/Markdown) file
and displays it as HTML.
				"))
			{
				using (var dlg = new ShowReleaseNotesDialog(SystemIcons.WinLogo, tempFile.Path))
					dlg.ShowDialog();
			}
		}
	}
}