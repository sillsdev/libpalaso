// Copyright (c) 2013-2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel.Security.Tokens;
using System.Threading;
using System.Windows.Forms;
using Gecko.Net;
using SIL.IO;
using SIL.Lexicon;
using SIL.PlatformUtilities;
using SIL.Reporting;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;
using SIL.Windows.Forms.HtmlBrowser;
using SIL.Windows.Forms.ImageToolbox;
using SIL.Windows.Forms.ImageToolbox.ImageGallery;
using SIL.Windows.Forms.Keyboarding;
using SIL.Windows.Forms.Miscellaneous;
using SIL.Windows.Forms.ReleaseNotes;
using SIL.Windows.Forms.SettingProtection;
using SIL.Windows.Forms.WritingSystems;
using SIL.WritingSystems;
using SIL.Media;
using SIL.Windows.Forms.Extensions;
using SIL.Windows.Forms.LocalizationIncompleteDlg;

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
		private LocalizationIncompleteViewModel _localizationIncompleteViewModel;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Default c'tor
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TestAppForm()
		{
			InitializeComponent();
			Text = Platform.DesktopEnvironmentInfoString;
			_localizationIncompleteViewModel = new LocalizationIncompleteViewModel(
				Program.PrimaryL10NManager, "testapp",
				IssueAnalyticsRequest);
			_uiLanguageMenu.InitializeWithAvailableUILocales(l => true, Program.PrimaryL10NManager,
				_localizationIncompleteViewModel, additionalNamedLocales:new Dictionary<string, string> {
					{ "Some untranslated language", WellKnownSubtags.UnlistedLanguage } });
		}

		private void IssueAnalyticsRequest()
		{
			if (InvokeRequired)
				Invoke(new Action(IssueAnalyticsRequest));
			else
			{
				var msg = "Request issued for localization into " +
					_localizationIncompleteViewModel.StandardAnalyticsInfo["Requested language"];
				if (!string.IsNullOrWhiteSpace(_localizationIncompleteViewModel.UserEmailAddress))
					msg += Environment.NewLine + "by " +
						_localizationIncompleteViewModel.StandardAnalyticsInfo["User email"];
				msg +=  Environment.NewLine + $"for {_localizationIncompleteViewModel.NumberOfUsers} users";
				MessageBox.Show(msg, Text);
			}
		}

		private void OnFolderBrowserControlClicked(object sender, EventArgs e)
		{
			if (!Platform.IsWindows)
			{
				MessageBox.Show("FolderBrowserControl not supported on Linux");
				return;
			}

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
		}

		private void OnLanguageLookupDialogClicked(object sender, EventArgs e)
		{
			using (var dialog = new LanguageLookupDialog())
			{
				dialog.SetLanguageAlias("zh-Hans", "Simplified Chinese (简体中文)");
				dialog.MatchingLanguageFilter = info => info.LanguageTag != "cmn";

				dialog.IsScriptAndVariantLinkVisible = true;
				dialog.ShowDialog();
			}
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
					@"<html><head><meta charset='UTF-8' /></head><body>" +
					@"<h3>Copyright 2014 <a href=""http://sil.org"">SIL International</a></h3>" +
					@"<p>Testing the <b>about box</b></p><ul>#DependencyAcknowledgements#</ul></body></html>");
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

		/// <summary>
		/// Displays the FlexibleMessageBox using the contents of the clipboard (if text is available on it) or
		/// some standard text. The localization (French vs. a custom pseudo-localization) and the version of the
		/// Show method (which buttons, icon, etc.) are randomized. This makes it easy to see fairly quickly what
		/// it would look like with several different options, but it makes it impossible to repeatedly test a
		/// particular option over and over. I thought this was probably faster and more useful than designing a
		/// whole UI that would allow the tester to configure which options would be used, but that could certainly
		/// be done later if it proved expedient.
		/// </summary>
		private void btnFlexibleMessageBox_Click(object sender, EventArgs e)
		{
			var random = new Random(DateTime.Now.Millisecond);

			if (random.Next(0, 6) == 5)
			{
				var LocalizedButtonText = FlexibleMessageBox.GetButtonTextLocalizationKeys.ToDictionary(k => k.Key, v => v.Value + " Loc");
				FlexibleMessageBox.GetButtonText = id => LocalizedButtonText[id];
			}
			else
			{
				FlexibleMessageBox.GetButtonText = null;
			}

			string caption;
			string msg;
			try
			{
				string clipboardText = PortableClipboard.GetText();
				if (clipboardText == String.Empty)
					throw new ApplicationException("This is fine. It will display the default caption and message");
				var data = clipboardText.Split(new [] {'\n'}, 2);
				if (data.Length == 1)
				{
					caption = "Flexible Message Box test caption";
					msg = clipboardText;
				}
				else
				{
					caption = data[0];
					msg = data[1];
				}
			}
			catch (Exception)
			{
				caption = Text;
				msg = "If you don't like this message, copy some text to your clipboard. If it's more than\n" +
					"one line, the first line will be used as the caption of the flexible message box.";
			}

			LinkClickedEventHandler handler = msg.Contains("http:") ? (LinkClickedEventHandler)((s, args) =>
			{
				FlexibleMessageBox.Show("Link clicked: " + args.LinkText);
			}) : null;

			while (ShowFlexibleMessageBox(random.Next(0, 10), msg, caption, GetDefaultButton(random.Next(0, 3)), handler));
		}

		private bool ShowFlexibleMessageBox(int option, string msg, string caption, MessageBoxDefaultButton defaultButton, LinkClickedEventHandler handler)
		{
			switch (option)
			{
				case 0:
					msg += "\nThis message box is always on top!";
					FlexibleMessageBox.Show(this, msg, handler, FlexibleMessageBoxOptions.AlwaysOnTop);
					return true;
				case 1:
					FlexibleMessageBox.Show(this, msg, caption, handler);
					break;
				case 2:
					FlexibleMessageBox.Show(this, msg, caption, MessageBoxButtons.OKCancel, handler);
					break;
				case 3:
					msg += "\nClick Retry to display another version of the message box.";
					return FlexibleMessageBox.Show(this, msg, caption, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning, handler) == DialogResult.Retry;
				case 4:
					msg += "\nWould you like to display another version of the message box?";
					return FlexibleMessageBox.Show(this, msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
							defaultButton, handler) == DialogResult.Yes;
				case 5:
					FlexibleMessageBox.Show(msg, handler);
					break;
				case 6:
					FlexibleMessageBox.Show(msg, caption, handler);
					break;
				case 7:
					msg += "\nClick Retry to display another version of the message box.";
					return FlexibleMessageBox.Show(msg, caption, MessageBoxButtons.RetryCancel, handler) == DialogResult.Retry;
				case 8:
					msg += "\nThis message box is always on top!";
					FlexibleMessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Stop, handler, FlexibleMessageBoxOptions.AlwaysOnTop);
					break;
				default:
					msg += "\nWould you like to display another version of the message box?";
					return FlexibleMessageBox.Show(msg, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
						defaultButton, handler) == DialogResult.Yes;
			}
			return false;
		}

		private MessageBoxDefaultButton GetDefaultButton(int defaultButton)
		{
			switch (defaultButton)
			{
				default: return MessageBoxDefaultButton.Button1;
				case 1: return MessageBoxDefaultButton.Button2;
				case 2: return MessageBoxDefaultButton.Button3;
			}
		}

		private bool _mouseDownFlag = false;

		private void recordPlayButton_MouseUp(object sender, MouseEventArgs e)
		{
			_mouseDownFlag = false;
		}

		/// <summary>
		/// Used for a practical validation of audio session record and playback.
		/// Hold the button down while saying something; release to hear playback.
		/// Should see a "play started" dialog immediately after releasing mouse,
		/// and "play stopped" when it has all played.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void recordPlayButton_MouseDown(object sender, MouseEventArgs e)
		{
			_mouseDownFlag = true;
			using (var tempFile = TempFile.WithExtension(".wav"))
			using (var session = AudioFactory.CreateAudioSession(tempFile.Path))
			{
				session.StartRecording();
				while (_mouseDownFlag)
				{
					Thread.Sleep(10);
					Application.DoEvents();
				}
				session.StopRecordingAndSaveAsWav();
				if (session is ISimpleAudioWithEvents)
				{
					(session as ISimpleAudioWithEvents).PlaybackStopped += (o, args) =>
					{
						this.Invoke((Action) (() => MessageBox.Show("play stopped")));
					};
				}

				session.Play();
				MessageBox.Show("play started");
			}
		}

		private void btnTestContributorsList_Click(object sender, EventArgs e)
		{
			using (var dlg = new ContributorsForm())
				dlg.ShowDialog();
		}

		private void btnShowFormWithModalChild_Click(object sender, EventArgs e)
		{
			var parent = new ParentOfModalChild();
			parent.Show();
		}

		private void btnThrowException_Click(object sender, EventArgs e)
		{
			throw new Exception("This is a test of the error reporting window!");
		}
	}
}