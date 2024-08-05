using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using L10NSharp;
using SIL.Archiving;
using SIL.EventsAndDelegates;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reporting;
using SIL.Windows.Forms.Miscellaneous;
using SIL.Windows.Forms.PortableSettingsProvider;
using static System.Environment;
using static System.String;
using static SIL.Archiving.ArchivingDlgViewModel.StringId;

// ReSharper disable InconsistentNaming

namespace SIL.Windows.Forms.Archiving
{
	/// ----------------------------------------------------------------------------------------
	public partial class ArchivingDlg : Form, IArchivingProgressDisplay
	{
		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPosWindows(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		// ReSharper disable InconsistentNaming
		// ReSharper disable IdentifierTypo
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);    // brings window to top and makes it "always on top"
		private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);  // brings window to top but not "always on top"
		private const UInt32 SWP_NOSIZE = 0x0001;
		private const UInt32 SWP_NOMOVE = 0x0002;
		private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
		// ReSharper restore InconsistentNaming
		// ReSharper restore IdentifierTypo

		private readonly FormSettings _settings;
		protected readonly ArchivingDlgViewModel _viewModel;
		protected readonly string _launchButtonTextFormat;
		protected readonly string _appSpecificArchivalProcessInfo;
		protected readonly string _archiveInfoHyperlinkText;
		private Exception _exceptionToNotReport;

		private CancellationTokenSource _cts;

		protected virtual string ArchiveTypeForTitleBar =>
			_viewModel?.ArchiveType == ArchivingDlgViewModel.Standard.REAP ?
			LocalizationManager.GetString("DialogBoxes.ArchivingDlg.RAMPArchiveType",
				"RAMP (SIL Only)") : null;

		protected virtual string InformativeText =>
			_viewModel?.ArchiveType == ArchivingDlgViewModel.Standard.REAP ?
			Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.RAMPOverviewText",
					"{0} is a utility for entering metadata and uploading submissions to SIL's internal archive, " +
					"REAP. If you have access to this archive, this tool will help you use {0} to archive your " +
					"{1} data. {2} When the {0} package has been created, you can  launch {0} and enter any " +
					"additional information before doing the actual submission.",
					"Parameter 0  is the word 'RAMP' (the first one will be turned into a hyperlink); " +
					"Parameter 1 is the name of the calling (host) program (SayMore, FLEx, etc.); " +
					"Parameter 2 is additional app-specific information."), _viewModel.NameOfProgramToLaunch, _viewModel.AppName,
				_appSpecificArchivalProcessInfo) : _appSpecificArchivalProcessInfo;

		/// ------------------------------------------------------------------------------------
		/// <summary>Caller can use this to retrieve and persist form settings (typically
		/// after form is closed).</summary>
		/// ------------------------------------------------------------------------------------
		public FormSettings FormSettings => _settings;

		/// ------------------------------------------------------------------------------------
		/// <param name="model">View model</param>
		/// <param name="appSpecificArchivalProcessInfo">Application can use this to pass
		/// additional information that will be displayed to the user in the dialog to explain
		/// any application-specific details about the archival process. For archive systems
		/// (<see cref="ArchivingPrograms.Standard"/> that this "generic" archiving dialog box
		/// does not specifically know what to show as informative text, the value passed in
		/// to this parameter will be displayed.</param>
		/// <param name="localizationManagerId">The ID of the localization manager for the
		/// calling application.</param>
		/// <param name="programDialogFont">Application can set this to ensure a consistent look
		/// in the UI (especially useful for when a localization requires a particular font).</param>
		/// <param name="settings">Location, size, and state where the client would like the
		/// dialog box to appear (can be null)</param>
		/// <param name="archiveInfoHyperlinkText">Text in the InformativeText that will be
		/// marked as a hyperlink to ArchiveInfoUrl (first occurrence only). Defaults to
		/// <see cref="ArchivingDlgViewModel.NameOfProgramToLaunch"/>></param>
		/// ------------------------------------------------------------------------------------
		public ArchivingDlg(ArchivingDlgViewModel model, string appSpecificArchivalProcessInfo,
			string localizationManagerId = null, Font programDialogFont = null,
			FormSettings settings = null, string archiveInfoHyperlinkText = null)
		{
			_settings = settings ?? FormSettings.Create(this);

			_viewModel = model;
			_appSpecificArchivalProcessInfo = appSpecificArchivalProcessInfo;
			_archiveInfoHyperlinkText = archiveInfoHyperlinkText ??
				_viewModel.NameOfProgramToLaunch;

			InitializeComponent();

			if (!IsNullOrEmpty(localizationManagerId))
				locExtender.LocalizationManagerId = localizationManagerId;

			_launchButtonTextFormat = _buttonLaunchRamp.Text;
			_progressBar.Visible = false;
			_chkMetadataOnly.Visible = _viewModel is ISupportMetadataOnly;

			_logBox.Tag = false;

			model.OnReportMessage += DisplayMessage;
			model.OnError += DisplayError;
			model.OnExceptionDuringLaunch += HandleLaunchException;

			if (programDialogFont != null)
			{
				_linkOverview.Font = programDialogFont;
				_logBox.Font = FontHelper.MakeFont(programDialogFont, FontStyle.Bold);
				_buttonCancel.Font = programDialogFont;
				_buttonCreatePackage.Font = programDialogFont;
				_buttonLaunchRamp.Font = programDialogFont;
				Font = programDialogFont;
			}

			_buttonLaunchRamp.Click += (s, e) => model.LaunchArchivingProgram();

			_buttonCancel.MouseLeave += delegate
			{
				if (_cts != null)
					WaitCursor.Show();
			};

			_buttonCancel.MouseEnter += delegate
			{
				if (_cts != null)
					WaitCursor.Hide();
			};

		}

		private void HandleButtonCancelClick(object sender, EventArgs args)
		{
			try
			{
				_cts?.Cancel();
				// British spelling for the ID. American spelling for the string :-)
				DisplayMessage(NewLine + LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CancellingMsg", "Canceling..."), ArchivingDlgViewModel.MessageType.Error);
			}
			catch (Exception e)
			{
				// Probably a race condition - process just ended.
				Console.WriteLine(e);
			}
		}

		private void HandleLaunchException(EventArgs<Exception> args)
		{
			if (_viewModel.ArchiveType == ArchivingDlgViewModel.Standard.REAP &&
			    args.Item is InvalidOperationException)
			{
				_exceptionToNotReport = args.Item;
				EnsureRampHasFocusAndWaitForPackageToUnlock();
			}
		}

		/// ------------------------------------------------------------------------------------
		private void EnsureRampHasFocusAndWaitForPackageToUnlock()
		{
			if (ArchivingDlgViewModel.IsMono)
				BringToFrontMono();
			else
				BringToFrontWindows();
		}

		private static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx,
			int cy, uint uFlags)
		{
			// on Linux simply return true
			return !Platform.IsWindows || SetWindowPosWindows(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);
		}

		private static void BringToFrontWindows()
		{
			var processes = Process.GetProcessesByName(RampArchivingDlgViewModel.kRampProcessName);
			if (processes.Length < 1) return;

			// First, make the window topmost: this puts it in front of all other windows
			// and sets it as "always on top."
			SetWindowPos(processes[0].MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

			// Second, make the window not top-most: this removes the "always on top" behavior
			// and positions the window on top of all other "not always on top" windows.
			SetWindowPos(processes[0].MainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
		}

		private static void BringToFrontMono()
		{
			// On mono this requires xdotool or wmctrl
			string args = null;
			if (!IsNullOrEmpty(FileLocationUtilities.LocateInProgramFiles("xdotool", true)))      /* try to find xdotool first */
				args = "-c \"for pid in `xdotool search --name RAMP`; do xdotool windowactivate $pid; done\"";
			else if (!IsNullOrEmpty(FileLocationUtilities.LocateInProgramFiles("wmctrl", true)))  /* if xdotool is not installed, look for wmctrl */
				args = "-c \"wmctrl -a RAMP\"";

			if (IsNullOrEmpty(args)) return;

			var prs = new Process
			{
				StartInfo =
				{
					FileName = "bash",
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardError = true
				}
			};

			prs.Start();
		}

		private void CreatePackage(object sender, EventArgs eventArgs)
		{
			Focus();
			DisableControlsDuringPackageCreation();
			_progressBar.Visible = true;
			WaitCursor.Show();
			_logBox.Clear();

			Task.Run(async () => await CreatePackageAsync());
		}

		private async Task CreatePackageAsync()
		{
			_cts = new CancellationTokenSource();

			try
			{
				var result = await _viewModel.CreatePackage(_cts.Token);

				if (result != null)
					PackageCreationComplete(result);
			}
			catch (OperationCanceledException)
			{
				CompleteCancellation();
			}
			catch (Exception ex)
			{
				// Handle any other exceptions
				DisplayMessage(GetMessage(ErrorCreatingArchive) +
					NewLine + ex.Message, ArchivingDlgViewModel.MessageType.Error);
			}
			finally
			{
				if (InvokeRequired)
					Invoke(new Action (ResetUIForUserInteraction));
				else
					ResetUIForUserInteraction();

				try
				{
					_cts.Dispose();
				}
				catch (Exception e)
				{
					Logger.WriteError(e);
				}
				_cts = null;
			}
		}

		private void ResetUIForUserInteraction()
		{
			_progressBar.Visible = false;
			WaitCursor.Hide();
		}

		private void CompleteCancellation()
		{
			// American spelling for the ID. British spelling for the string :-)
			DisplayMessage(LocalizationManager.GetString(
					"DialogBoxes.ArchivingDlg.CanceledMsg", "Cancelled..."),
				ArchivingDlgViewModel.MessageType.Error);

			Thread.Sleep(500);
			DialogResult = DialogResult.Cancel;
			Close();
		}

		protected virtual void PackageCreationComplete(string result)
		{
			
			if (InvokeRequired)
				Invoke(new Action(() => { _buttonLaunchRamp.Enabled = true; }));
			else
				_buttonLaunchRamp.Enabled = true;
		}

		protected virtual void DisableControlsDuringPackageCreation()
		{
			_buttonCreatePackage.Enabled = false;
			_chkMetadataOnly.Enabled = false;
		}

		/// ------------------------------------------------------------------------------------
		protected void UpdateLaunchButtonText()
		{
			_buttonLaunchRamp.Text = Format(_launchButtonTextFormat, _viewModel.NameOfProgramToLaunch);
		}

		/// ------------------------------------------------------------------------------------
		protected void UpdateOverviewText()
		{
			_linkOverview.Text = InformativeText;
		}

		#region Implementation of IArchivingProgressDisplay

		public void IncrementProgress()
		{
			if (InvokeRequired)
				Invoke(new Action(() => { _progressBar.Increment(1); }));
			else
				_progressBar.Increment(1);
		}

		/// ------------------------------------------------------------------------------------
		public virtual string GetMessage(ArchivingDlgViewModel.StringId msgId)
		{
			switch (msgId)
			{
				case PreArchivingStatus:
					return LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.PrearchivingStatusMsg",
						"The following files will be added to the archive:");
				case SearchingForArchiveUploadingProgram:
					return Format(LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.SearchingForRampMsg",
						"Searching for the {0} program...",
						"Parameter is the path to the auxiliary archive upload program (RAMP, etc.)."),
						_viewModel.PathToProgramToLaunch);
				case ArchiveUploadingProgramNotFound:
					return Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.RampNotFoundMsg",
						"The {0} program cannot be found!",
						"Parameter is the path to the auxiliary archive upload program (RAMP, etc.)."),
						_viewModel.PathToProgramToLaunch);
				case ErrorStartingArchivalProgram:
					return Format(LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.StartingRampErrorMsg",
						"There was an error attempting to open the archive package in {0}.",
						"Parameter is the path to the auxiliary archive upload program (RAMP, etc.)."),
						_viewModel.PathToProgramToLaunch);
				case PreparingFiles:
					return LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.PreparingFilesMsg",
						"Analyzing component files");
				case SavingFilesInPackage:
					return Format(LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.SavingFilesInPackageMsg",
						"Saving files in {0} package",
						"Parameter is the type of archive (e.g., RAMP/IMDI)"), ArchiveTypeName);
				case FileExcludedFromPackage:
					return Format(LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.FileExcludedFromPackage",
						"File excluded from {0} package: ",
						"Parameter is the type of archive (e.g., RAMP/IMDI)"), ArchiveTypeName);
				case PathNotWritable:
					return LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.PathNotWritableMsg",
						"The path is not writable: {0}");
				case ReadyToCallRampMsg:
					return LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.ReadyToCallRampMsg",
						"Ready to hand the package to RAMP");
				case CopyingFiles:
					return LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.CopyingFilesMsg",
						"Copying files");
				case FailedToMakePackage:
					return Format(LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.FailedToMakePackage",
						"Failed to make the {0} package: {1}",
						"Param 0: The type of archive (e.g., RAMP); " +
						"Param 1: Path to the package that was supposed to get created"),
						ArchiveTypeName, _viewModel.PackagePath);
				case ErrorCreatingMetsFile:
					return Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CreatingInternalReapMetsFileErrorMsg",
						"There was an error attempting to create the {0}/{1} {2} file.",
						"Param 0: \"RAMP\" (program for uploading to REAP)" +
						"Param 1: \"REAP\" (SIL's corporate archive repository)" +
						"Param 2: \"METS\" (Acronym for the Metadata Encoding & Transmission Standard used by the U.S. Library of Congress)"),
						RampArchivingDlgViewModel.kRampProcessName, "REAP", "METS");
				case RampPackageRemoved:
					return Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.",
						"{0} package prematurely removed: {1}",
						"Param 0: \"RAMP\" (program for uploading to REAP)" +
						"Param 1: Path to the missing package"),
						_viewModel.PackagePath);
				case ErrorCreatingArchive:
					return Format(LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.CreatingArchiveErrorMsg",
						"There was an error attempting to create the {0} package: {1}",
						"Parameter is the type of archive (e.g., RAMP/IMDI)"),
						ArchiveTypeName, _viewModel.PackagePath);
				default:
					throw new ArgumentOutOfRangeException(nameof(msgId), msgId, null);
			}
		}

		public virtual string ArchiveTypeName =>
			_viewModel?.ArchiveType == ArchivingDlgViewModel.Standard.REAP
				? RampArchivingDlgViewModel.kRampProcessName :
				_viewModel?.ArchiveType.ToString() ?? "Other";

		#endregion

		/// ------------------------------------------------------------------------------------
		protected void DisplayMessage(string msg, ArchivingDlgViewModel.MessageType type)
		{
			if (InvokeRequired)
				Invoke(new Action(() => { DisplayMessageOnUIThread(msg, type); }));

			DisplayMessageOnUIThread(msg, type);
		}

		/// ------------------------------------------------------------------------------------
		private void DisplayMessageOnUIThread(string msg, ArchivingDlgViewModel.MessageType type)
		{
			if ((bool) _logBox.Tag)
			{
				_logBox.Clear();
				_logBox.Tag = false;
			}
			switch (type)
			{
				case ArchivingDlgViewModel.MessageType.Normal:
					_logBox.WriteMessage(msg);
					break;
				case ArchivingDlgViewModel.MessageType.Indented:
					_logBox.WriteMessage(NewLine + "	" + msg);
					break;
				case ArchivingDlgViewModel.MessageType.Detail:
					_logBox.WriteMessageWithFontStyle(FontStyle.Regular, "\t" + msg);
					break;
				case ArchivingDlgViewModel.MessageType.Bullet:
					_logBox.WriteMessageWithFontStyle(FontStyle.Regular, "		  \u00B7 {0}", msg);
					break;
				case ArchivingDlgViewModel.MessageType.Progress:
					_logBox.WriteMessage(NewLine + msg);
					break;
				case ArchivingDlgViewModel.MessageType.Warning:
					_logBox.WriteWarning(msg);
					break;
				case ArchivingDlgViewModel.MessageType.Error:
					_logBox.WriteMessageWithColor("Red", msg + NewLine);
					break;
				case ArchivingDlgViewModel.MessageType.Success:
					_logBox.WriteMessageWithColor(Color.DarkGreen, NewLine + msg);
					break;
				case ArchivingDlgViewModel.MessageType.Volatile:
					_logBox.WriteMessage(msg);
					_logBox.Tag = true;
					break;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void DisplayError(string msg, string packageTitle, Exception e)
		{
			if (e == _exceptionToNotReport)
			{
				_exceptionToNotReport = null;
				return;
			}

			if (InvokeRequired)
				Invoke(new Action(() => { DisplayErrorOnUIThread(msg, packageTitle, e); }));

			DisplayErrorOnUIThread(msg, packageTitle, e);
		}

		/// ------------------------------------------------------------------------------------
		private void DisplayErrorOnUIThread(string msg, string packageTitle, Exception e)
		{
			if (_logBox.IsHandleCreated)
			{
				WaitCursor.Hide();
				_logBox.WriteError(msg, packageTitle);
				if (e != null)
					_logBox.WriteException(e);
			}
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			_settings.InitializeForm(this);
			base.OnLoad(e);

			Text = Format(Text, _viewModel.AppName, ArchiveTypeForTitleBar ?? ArchiveTypeName);

			UpdateLaunchButtonText();

			_linkOverview.Text = InformativeText;
			_linkOverview.Links.Clear();

			if (!IsNullOrEmpty(_viewModel.ArchiveInfoUrl) && !IsNullOrEmpty(_archiveInfoHyperlinkText))
			{
				int i = _linkOverview.Text.IndexOf(_archiveInfoHyperlinkText, StringComparison.InvariantCulture);
				if (i >= 0)
					_linkOverview.Links.Add(i, _archiveInfoHyperlinkText.Length, _viewModel.ArchiveInfoUrl);
			}

			// this is for a display problem in mono
			_linkOverview.SizeToContents();
		}
		
		/// ------------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			Initialize();
		}
		
		/// ------------------------------------------------------------------------------------
		protected async void Initialize()
		{
			_cts = new CancellationTokenSource();
			WaitCursor.Show();

			try
			{
				_buttonCreatePackage.Enabled = _chkMetadataOnly.Enabled =
					await _viewModel.Initialize(this, _cts.Token);
				_logBox.ScrollToTop();
				_progressBar.Maximum = _viewModel.CalculateMaxProgressBarValue();
			}
			catch (OperationCanceledException)
			{
				CompleteCancellation();
			}
			catch (Exception ex)
			{
				ErrorReport.ReportNonFatalException(ex);
				Close();
			}
			finally
			{
				if (InvokeRequired)
					Invoke(new Action (ResetUIForUserInteraction));
				else
					ResetUIForUserInteraction();

				try
				{
					_cts.Dispose();
				}
				catch (Exception e)
				{
					Logger.WriteError(e);
				}
				_cts = null;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleRampLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var tgt = e.Link.LinkData as string;

			if (!IsNullOrEmpty(tgt))
			{
				var ps = new ProcessStartInfo(tgt)
				{ 
					UseShellExecute = true, 
					Verb = "open" 
				};
				Process.Start(ps);
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleLogBoxReportErrorLinkClicked(object sender, EventArgs e)
		{
			Close();
		}

		private void _chkMetadataOnly_CheckedChanged(object sender, EventArgs e)
		{
			((ISupportMetadataOnly)_viewModel).MetadataOnly = _chkMetadataOnly.Checked;
		}
	}
}
