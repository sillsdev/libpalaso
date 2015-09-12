// Copyright (c) 2013-2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Palaso.IO;
using Palaso.UI.WindowsForms.ClearShare;
using Palaso.UI.WindowsForms.ClearShare.WinFormsUI;
using Palaso.UI.WindowsForms.HtmlBrowser;
using Palaso.UI.WindowsForms.ImageGallery;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.ReleaseNotes;
using Palaso.UI.WindowsForms.SIL;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using PalasoUIWindowsForms.TestApp.Properties;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.ImageToolbox;

namespace PalasoUIWindowsForms.TestApp
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
			Text = Palaso.PlatformUtilities.Platform.DesktopEnvironmentInfoString;
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
			if (!_KeyboardControllerInitialized)
			{
				KeyboardController.Initialize();
				_KeyboardControllerInitialized = true;

				foreach (string key in ErrorReport.Properties.Keys)
					Console.WriteLine("{0}: {1}", key, ErrorReport.Properties[key]);
			}
			var wsRepo = LdmlInFolderWritingSystemRepository.Initialize(tempPath, onMigration, onLoadProblem);
			using (var dialog = new WritingSystemSetupDialog(wsRepo))
			{
				dialog.WritingSystems.LocalKeyboardSettings = Settings.Default.LocalKeyboards;
				dialog.ShowDialog();
				Settings.Default.LocalKeyboards = dialog.WritingSystems.LocalKeyboardSettings;
				Settings.Default.Save();
			}
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
	}
}