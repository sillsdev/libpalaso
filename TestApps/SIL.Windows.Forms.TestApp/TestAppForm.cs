// Copyright (c) 2013-2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SIL.IO;
using SIL.Lexicon;
using SIL.PlatformUtilities;
using SIL.Reporting;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;
using SIL.Windows.Forms.HtmlBrowser;
using SIL.Windows.Forms.ImageGallery;
using SIL.Windows.Forms.ImageToolbox;
using SIL.Windows.Forms.Keyboarding;
using SIL.Windows.Forms.Miscellaneous;
using SIL.Windows.Forms.ReleaseNotes;
using SIL.Windows.Forms.SettingProtection;
using SIL.Windows.Forms.WritingSystems;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.TestApp
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class TestAppForm : Form
	{
		private bool _KeyboardControllerInitialized;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Default c'tor
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TestAppForm()
		{
			InitializeComponent();
			Text = Platform.DesktopEnvironmentInfoString;
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

		private void OnLanguageLookupDialogClicked(object sender, EventArgs e)
		{
			using (var dialog = new LanguageLookupDialog())
				dialog.ShowDialog();
		}

		private void OnWritingSystemSetupDialogClicked(object sender, EventArgs e)
		{
			string tempPath = Path.GetTempPath() + "WS-Test";
			Directory.CreateDirectory(tempPath);
			if (!_KeyboardControllerInitialized)
			{
				KeyboardController.Initialize();
				_KeyboardControllerInitialized = true;

				foreach (string key in ErrorReport.Properties.Keys)
					Console.WriteLine("{0}: {1}", key, ErrorReport.Properties[key]);
			}
			ICustomDataMapper<WritingSystemDefinition>[] customDataMappers =
			{
				new UserLexiconSettingsWritingSystemDataMapper(new ApplicationSettingsStore(Properties.Settings.Default, "UserSettings")),
				new ProjectLexiconSettingsWritingSystemDataMapper(new ApplicationSettingsStore(Properties.Settings.Default, "ProjectSettings"))
			};
			LdmlInFolderWritingSystemRepository wsRepo = LdmlInFolderWritingSystemRepository.Initialize(tempPath, customDataMappers);
			using (var dialog = new WritingSystemSetupDialog(wsRepo))
				dialog.ShowDialog();
		}

		private void OnImageToolboxClicked(object sender, EventArgs e)
		{
			Application.EnableVisualStyles();
			ThumbnailViewer.UseWebViewer = true;
			using (var dlg = new ImageToolboxDialog(new PalasoImage(), null))
			{
				dlg.ShowDialog();
			}
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

## 2.0
* change one
* change two
## 1.9
* big change 
  + little change
  - other little change
## 1.8

* oldest change
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