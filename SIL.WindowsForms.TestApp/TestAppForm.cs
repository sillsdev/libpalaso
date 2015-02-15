// Copyright (c) 2013-2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SIL.IO;
using SIL.LexiconUtils;
using SIL.WindowsForms.ClearShare;
using SIL.WindowsForms.ClearShare.WinFormsUI;
using SIL.WindowsForms.HtmlBrowser;
using SIL.WindowsForms.Keyboarding;
using SIL.WindowsForms.Miscellaneous;
using SIL.WindowsForms.ReleaseNotes;
using SIL.WindowsForms.SettingProtection;
using SIL.WindowsForms.WritingSystems;
using SIL.WritingSystems;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WindowsForms.TestApp
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
				var browser = new FolderBrowserControl.FolderBrowserControl();
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
			using (var dialog = new LookupIsoCodeDialog())
				dialog.ShowDialog();
		}

		private void OnWritingSystemSetupDialogClicked(object sender, EventArgs e)
		{
			string tempPath = Path.GetTempPath() + "WS-Test";
			Directory.CreateDirectory(tempPath);
			KeyboardController.Initialize();
			try
			{
				ICustomDataMapper[] customDataMappers =
				{
					new UserSettingsWritingSystemDataMapper(new ApplicationSettingsStore(Properties.Settings.Default, "UserSettings")),
					new ProjectSettingsWritingSystemDataMapper(new ApplicationSettingsStore(Properties.Settings.Default, "ProjectSettings"))
				};
				LdmlInFolderWritingSystemRepository wsRepo = LdmlInFolderWritingSystemRepository.Initialize(tempPath, customDataMappers, onMigration, onLoadProblem);
				using (var dialog = new WritingSystemSetupDialog(wsRepo))
					dialog.ShowDialog();
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
			ShowSilAboutBox(XWebBrowser.BrowserType.Default, true);
		}

		private void OnSilAboutBoxGeckoClicked(object sender, EventArgs e)
		{
			ShowSilAboutBox(XWebBrowser.BrowserType.GeckoFx, false);
		}

		private static void ShowSilAboutBox(XWebBrowser.BrowserType browserType, bool useFullVersionNumber)
		{
			XWebBrowser.DefaultBrowserType = browserType;
			using (var tempfile = TempFile.WithExtension("html"))
			{
				File.WriteAllText(tempfile.Path,
					@"<html><body><h3>Copyright 2014 <a href=""http://sil.org"">SIL International</a></h3>" +
					@"<p>Testing the <b>about box</b></p></body></html>");
				var uri = new Uri(tempfile.Path);
				using (var dlg = new SILAboutBox(uri.AbsoluteUri, useFullVersionNumber))
				{
					dlg.ShowDialog();
				}
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

		private void OnShowMetaDataEditorClicked(object sender, EventArgs e)
		{
			using (var dlg = new MetadataEditorDialog(new Metadata()))
			{
				dlg.ShowDialog();
			}
		}

		private void OnSelectFileClicked(object sender, System.EventArgs e)
		{
			// Get first file in personal folder
			var fileName = Directory.EnumerateFiles(
				Environment.GetFolderPath(Environment.SpecialFolder.Personal))
				.First(x => !Path.GetFileName(x).StartsWith(".", StringComparison.InvariantCulture));
			PathUtilities.SelectFileInExplorer(fileName);
		}

		private void btnSettingProtectionDialog_Click(object sender, EventArgs e)
		{
			using (var dlg = new SettingProtectionDialog())
				dlg.ShowDialog();
		}
	}
}