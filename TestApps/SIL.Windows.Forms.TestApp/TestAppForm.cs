// Copyright (c) 2013-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using L10NSharp;
using SIL.Extensions;
using SIL.IO;
using SIL.Lexicon;
using SIL.PlatformUtilities;
using SIL.Reporting;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;
using SIL.Windows.Forms.HtmlBrowser;
using SIL.Windows.Forms.Keyboarding;
using SIL.Windows.Forms.Miscellaneous;
using SIL.Windows.Forms.ReleaseNotes;
using SIL.Windows.Forms.SettingProtection;
using SIL.Windows.Forms.WritingSystems;
using SIL.WritingSystems;
using SIL.Media;
using SIL.Windows.Forms.Extensions;
using SIL.Windows.Forms.FileSystem;
using SIL.Windows.Forms.LocalizationIncompleteDlg;
using static System.Windows.Forms.MessageBoxButtons;

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
		private readonly LocalizationIncompleteViewModel _localizationIncompleteViewModel;

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
			_cboAboutHTML.SelectedIndex = 0;
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
				if ((ModifierKeys & Keys.Shift) == Keys.Shift)
					dialog.Caption = "Add a language, my friend!";

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

		private void OnSilAboutBoxClicked(object sender, EventArgs e)
		{
			ShowSilAboutBox(XWebBrowser.BrowserType.Default, true);
		}

		private void OnSilAboutBoxGeckoClicked(object sender, EventArgs e)
		{
			ShowSilAboutBox(XWebBrowser.BrowserType.GeckoFx, false);
		}

		private void ShowSilAboutBox(XWebBrowser.BrowserType browserType, bool useFullVersionNumber)
		{
			// Long enough text to push the internal link off the screen, so we can confirm that
			// clicking the link takes us to the right place.
			const string internalLinkHtmlContent = @"
						  <p>Testing the about box with an <a href='#internal'>internal link</a>.</p>
						  <p>Some ipsums and maybe a lorum or two.</p>
						  <p>Here is a place where a quick brown fox might jump.</p>
						  <p>He could be running from a hunter.</p>
						  <p>Or it is possible that he would be chasing the cow and calf.</p>
						  <p>It is possible that they would be chasing the pig.</p>
						  <p>And the pig could be chasing the dog.</p>
						  <p>The dog may or may not be chasing the cat.</p>
						  <p>The cat seems to be chasing the frog.</p>
						  <p>And I think we all know how much frogs are tempted to chase flies.</p>
						  <p>So the only surprise is the scary, loud noise that has the otherwise brave hunter on the run.</p>
						  <p>Is the suspense killing you yet?</p>
						  <hr />
						  <h4 id='internal'>This is the internal section</h4>
						  <p>You jumped here using an internal anchor link just to find out it was a lamb with a tin can on its tail.</p>
						  <p><a href='mailto:someone@example.com'>Email Tom</a></p>
						</body></html>";
			XWebBrowser.DefaultBrowserType = browserType;
			string html;
			var handleNavigation = false;
			var allowExtLinksInsideAbout = false;
			var createCss = false;
			switch (_cboAboutHTML.SelectedIndex)
			{
				default: // Links without target attribute
					createCss = true;
					html = @"<html>
						<head>
							<meta charset='UTF-8' />
							<link rel=""stylesheet"" type=""text/css"" href=""aboutBox.css"" />
						</head>
						<body>
						  <h3>Copyright 2025 <a href=""http://sil.org"">SIL Global</a></h3>
						  <p>Testing the <b>about box</b></p>
						  <ul>#DependencyAcknowledgements#</ul>
						</body></html>";
					break;
				case 1: // HTML head includes <base target = "_blank" rel = "noopener noreferrer">
					html = @"<html><head>
						<base target = ""_blank"" rel = ""noopener noreferrer"">
						</head><meta charset='UTF-8' /></head>
						<body>
						  <h3>Copyright 2025 <a href=""http://sil.org"">SIL Global</a></h3>
						  <p>Testing the <b>about box</b></p>
						  <ul>#DependencyAcknowledgements#</ul>
						</body></html>";
					break;
				case 2: // Individual link has target = "_blank"
					html = @"<html>
						<head>
						  <meta charset='UTF-8' />
						</head>
						<body>
						  <h3>Copyright 2025 <a href='http://sil.org' target='_blank'>SIL Global</a></h3>
						  <p>This <a href='https://example.com/'>link</a> is still going to open inside About.</p>
						  <p>This <a name='CurrentFolder' href=''>link</a> is blank and will take you to a folder!</p>
						  <p>This <a href='file://notexist.md'>changelog</a> is a broken link to a local file!</p>" +
						internalLinkHtmlContent;
					break;
				case 3: // Navigating is handled
					handleNavigation = true;
					goto default;
				case 4: // Simple HTML with no external links
					html = @"<html><head><meta charset='UTF-8' /></head>
						<body>
						  <h3>Copyright 2025, SIL Global</a></h3>" +
					       internalLinkHtmlContent;
					break;
				case 5: // Allow external links to open in About dialog
					allowExtLinksInsideAbout = true;
					goto default;
			}

			using var tempFile = TempFile.WithExtension("html");
			File.WriteAllText(tempFile.Path, html);

			TempFile cssFile = null;
			if (createCss)
			{
				cssFile = TempFile.WithFilename("aboutBox.css");
				File.WriteAllText(cssFile.Path, @"
						body {
							font-family: sans-serif;
						}
						a {
							color: orange;
							text-decoration: underline;
						}
						a:visited {
							color: green;
						}
						a:hover {
							text-decoration: none;
						}
					");
			}

			try
			{
				var uri = new Uri(tempFile.Path);
				using (var dlg = new SILAboutBox(uri.AbsoluteUri, useFullVersionNumber))
				{
					bool firstNav = true;
					if (handleNavigation)
						dlg.Navigating += (sender, args) =>
						{
							if (firstNav)
							{
								firstNav = false;
								return;
							}

							var msg = string.Format(LocalizationManager.GetString(
									"About.ExternalNavigationConfirmationMsg",
									"Request to navigate to {0} with target frame {1}",
									"Param 0: URL; Param 1: Target frame name"),
								args.Url,
								args.TargetFrameName);
							var title = LocalizationManager.GetString(
								"About.ExternalNavigationConfirmationTitle",
								"External navigation request");
							var dlgResult = MessageBox.Show(msg, title, OKCancel);
							args.Cancel = DialogResult.Cancel == dlgResult;
						};
					dlg.AllowExternalLinksToOpenInsideAboutBox = allowExtLinksInsideAbout;
					dlg.ShowDialog();
				}
			}
			finally
			{
				cssFile?.Dispose();
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
					FlexibleMessageBox.Show(this, msg, caption, OKCancel, handler);
					break;
				case 3:
					msg += "\nClick Retry to display another version of the message box.";
					return FlexibleMessageBox.Show(this, msg, caption, AbortRetryIgnore, MessageBoxIcon.Warning, handler) == DialogResult.Retry;
				case 4:
					msg += "\nWould you like to display another version of the message box?";
					return FlexibleMessageBox.Show(this, msg, caption, YesNo, MessageBoxIcon.Question,
							defaultButton, handler) == DialogResult.Yes;
				case 5:
					FlexibleMessageBox.Show(msg, handler);
					break;
				case 6:
					FlexibleMessageBox.Show(msg, caption, handler);
					break;
				case 7:
					msg += "\nClick Retry to display another version of the message box.";
					return FlexibleMessageBox.Show(msg, caption, RetryCancel, handler) == DialogResult.Retry;
				case 8:
					msg += "\nThis message box is always on top!";
					FlexibleMessageBox.Show(msg, caption, OK, MessageBoxIcon.Stop, handler, FlexibleMessageBoxOptions.AlwaysOnTop);
					break;
				default:
					msg += "\nWould you like to display another version of the message box?";
					return FlexibleMessageBox.Show(msg, caption, YesNoCancel, MessageBoxIcon.Question,
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
				if (session is ISimpleAudioWithEvents events)
				{
					events.PlaybackStopped += (o, args) =>
					{
						Invoke((Action) (() => MessageBox.Show("play stopped")));
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

		private string GetExtensionsStr(StringCollection extensions)
		{
			var prepend = "*";
			var sb = new StringBuilder();
			foreach (string ext in extensions)
			{
				sb.Append(prepend);
				sb.Append(ext);
				prepend = ";*";
			}

			return sb.ToString();
		}

		private void btnMediaFileInfo_Click(object sender, EventArgs e)
		{
			using (var dlg = new OpenFileDialog())
			{
				dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				dlg.RestoreDirectory = true;
				dlg.CheckFileExists = true;
				dlg.CheckPathExists = true;
				dlg.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}|{4} ({5})|{5}",
					"Audio Files",
					GetExtensionsStr(FileUtils.AudioFileExtensions),
					"Video Files",
					GetExtensionsStr(FileUtils.VideoFileExtensions),
					"All Files",
					"*.*");
				dlg.FilterIndex = 0;
				dlg.Multiselect = false;
				dlg.Title = "Select a media file";
				dlg.ValidateNames = true;
				if (dlg.ShowDialog(this) == DialogResult.OK && File.Exists(dlg.FileName))
				{
					try
					{
						var info = MediaInfo.GetInfo(dlg.FileName);
						var sb = new StringBuilder("File: ");
						sb.Append(Path.GetFileName(dlg.FileName));
						if (info.Audio != null)
						{
							sb.Append(Environment.NewLine);
							sb.Append("Audio Info:");
							sb.Append(Environment.NewLine);
							sb.Append("  ChannelCount: ");
							sb.Append(info.Audio.ChannelCount);
							sb.Append(Environment.NewLine);
							sb.Append("  Duration: ");
							sb.Append(info.Audio.Duration);
							sb.Append(Environment.NewLine);
							sb.Append("  Encoding: ");
							sb.Append(info.Audio.Encoding);
							sb.Append(Environment.NewLine);
							sb.Append("  SamplesPerSecond: ");
							sb.Append(info.Audio.SamplesPerSecond);
							if (info.Audio.BitDepth > 0)
							{
								sb.Append(Environment.NewLine);
								sb.Append("  BitDepth: ");
								sb.Append(info.Audio.BitDepth);
							}
							if (info.AnalysisData.AudioStreams.Count > 1)
							{
								sb.Append(Environment.NewLine);
								sb.Append("  Total number of audio streams:");
								sb.Append(info.AnalysisData.AudioStreams.Count);
							}
						}
						if (info.Video != null)
						{
							sb.Append(Environment.NewLine);
							sb.Append("Video Info:");
							sb.Append(Environment.NewLine);
							sb.Append("  Resolution: ");
							sb.Append(info.Video.Resolution);
							sb.Append(Environment.NewLine);
							sb.Append("  Duration: ");
							sb.Append(info.Video.Duration);
							sb.Append(Environment.NewLine);
							sb.Append("  Encoding: ");
							sb.Append(info.Video.Encoding);
							sb.Append(Environment.NewLine);
							sb.Append("  FrameRate: ");
							sb.Append(info.Video.FrameRate);
							if (info.AnalysisData.VideoStreams.Count > 1)
							{
								sb.Append(Environment.NewLine);
								sb.Append("  Total number of video streams:");
								sb.Append(info.AnalysisData.VideoStreams.Count);
							}
						}
						if (info.Audio == null && info.Video == null)
						{
							sb.Append(Environment.NewLine);
							sb.Append("Not a valid media file!");
						}

						MessageBox.Show(this, sb.ToString(), "Media information");
					}
					catch (Exception exception)
					{
						MessageBox.Show(exception.Message);
					}
				}
			}
		}

		private void btnShowFileOverwriteDlg_Click(object sender, EventArgs e)
		{
			var filenames = new List<string>
			{
				@"c:\folder\file.txt",
				@"My Documents\another.doc",
				@"LastOne.png"
			};
			var filesOverwritten = new List<string>();
			var filesSkipped = new List<string>();
			bool? overwriteAll = null;

			foreach (var file in filenames)
			{
				if (overwriteAll == null)
				{
					using (var dlg = new ConfirmFileOverwriteDlg(file))
					{
						if (dlg.ShowDialog(this) == DialogResult.No)
						{
							filesSkipped.Add(file);
							if (dlg.ApplyToAll)
								overwriteAll = false;
						}
						else
						{
							filesOverwritten.Add(file);
							if (dlg.ApplyToAll)
								overwriteAll = true;
						}
					}
				}
				else if ((bool)overwriteAll)
				{
					filesOverwritten.Add(file);
				}
				else
				{
					filesSkipped.Add(file);
				}
			}

			MessageBox.Show(
				$"Files overwritten:\r\t{filesOverwritten.ToString("\r\t")}\rFiles skipped:\r\t{filesSkipped.ToString("\r\t")}", "Results");
		}

		private void btnOpenProject_Click(object sender, EventArgs e)
		{
			using var dlg = new ChooseProject();
			if (dlg.ShowDialog(this) == DialogResult.OK)
				MessageBox.Show("Got " + dlg.SelectedProject);
		}

		private static int s_FadingMessageCount = 1;
		private void OnShowFadingMessageClicked(object sender, EventArgs e)
		{
			var fadingMsgWindow = new FadingMessageWindow();
			fadingMsgWindow.Show($"{s_FadingMessageCount++}) Fading message.", _btnShowFadingMessage.Location);
		}

		private void btnRefRange_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScrReferenceFilterDlg())
				dlg.ShowDialog();
		}
	}
}