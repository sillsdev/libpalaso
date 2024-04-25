using System;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms;
using SIL.Windows.Forms.Miscellaneous;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace SIL.Windows.Forms.Archiving
{
	/// ----------------------------------------------------------------------------------------
	public partial class ArchivingDlg : Form
	{
		private readonly FormSettings _settings;
		protected readonly ArchivingDlgViewModel _viewModel;
		protected readonly string _launchButtonTextFormat;

		/// ------------------------------------------------------------------------------------
		/// <summary>Caller can use this to retrieve and persist form settings (typically
		/// after form is closed).</summary>
		/// ------------------------------------------------------------------------------------
		public FormSettings FormSettings
		{
			get { return _settings; }
		}

		/// ------------------------------------------------------------------------------------
		/// <param name="model">View model</param>
		/// <param name="localizationManagerId">The ID of the localization manager for the
		/// calling application.</param>
		/// <param name="programDialogFont">Application can set this to ensure a consistent look
		/// in the UI (especially useful for when a localization requires a particular font).</param>
		/// <param name="settings">Location, size, and state where the client would like the
		/// dialog box to appear (can be null)</param>
		/// ------------------------------------------------------------------------------------
		public ArchivingDlg(ArchivingDlgViewModel model, string localizationManagerId,
			Font programDialogFont, FormSettings settings)
		{
			_settings = settings ?? FormSettings.Create(this);

			_viewModel = model;

			InitializeComponent();

			if (!string.IsNullOrEmpty(localizationManagerId))
				locExtender.LocalizationManagerId = localizationManagerId;

			Text = string.Format(Text, model.AppName, model.ArchiveType);
			_progressBar.Visible = false;

			// remember this because we will need it later in a derived class
			_launchButtonTextFormat = _buttonLaunchRamp.Text;

			UpdateLaunchButtonText();
			_buttonLaunchRamp.Enabled = false; //!string.IsNullOrEmpty(model.PathToProgramToLaunch);
			_chkMetadataOnly.Visible = _viewModel is ISupportMetadataOnly;

			_linkOverview.Text = model.InformativeText;
			_linkOverview.Links.Clear();

			if (!string.IsNullOrEmpty(model.ArchiveInfoUrl) && !string.IsNullOrEmpty(model.ArchiveInfoHyperlinkText))
			{
				int i = _linkOverview.Text.IndexOf(model.ArchiveInfoHyperlinkText, StringComparison.InvariantCulture);
				if (i >= 0)
					_linkOverview.Links.Add(i, model.ArchiveInfoHyperlinkText.Length, model.ArchiveInfoUrl);
			}

			// this is for a display problem in mono
			_linkOverview.SizeToContents();
			_logBox.Tag = false;

			model.OnDisplayMessage += DisplayMessage;
			model.OnDisplayError += new ArchivingDlgViewModel.DisplayErrorEventHandler(model_DisplayError);

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
				if (model.IsBusy)
					WaitCursor.Show();
			};

			_buttonCancel.MouseEnter += delegate
			{
				if (model.IsBusy)
					WaitCursor.Hide();
			};

			_buttonCancel.Click += delegate
			{
				model.Cancel();
				WaitCursor.Hide();
			};
		}

		private void CreatePackage(object sender, EventArgs eventArgs)
		{
			Focus();
			DisableControlsDuringPackageCreation();
			_progressBar.Visible = true;
			WaitCursor.Show();
			_logBox.Clear();
			_buttonLaunchRamp.Enabled = _viewModel.CreatePackage();
			_progressBar.Visible = false;
			WaitCursor.Hide();
		}

		protected virtual void DisableControlsDuringPackageCreation()
		{
			_buttonCreatePackage.Enabled = false;
			_chkMetadataOnly.Enabled = false;
		}

		/// ------------------------------------------------------------------------------------
		protected void UpdateLaunchButtonText()
		{
			_buttonLaunchRamp.Text = string.Format(_launchButtonTextFormat, _viewModel.NameOfProgramToLaunch);
		}

		/// ------------------------------------------------------------------------------------
		protected void UpdateOverviewText()
		{
			_linkOverview.Text = _viewModel.InformativeText;
		}

		/// ------------------------------------------------------------------------------------
		void DisplayMessage(string msg, ArchivingDlgViewModel.MessageType type)
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
					_logBox.WriteMessage(Environment.NewLine + "	" + msg);
					break;
				case ArchivingDlgViewModel.MessageType.Detail:
					_logBox.WriteMessageWithFontStyle(FontStyle.Regular, "\t" + msg);
					break;
				case ArchivingDlgViewModel.MessageType.Bullet:
					_logBox.WriteMessageWithFontStyle(FontStyle.Regular, "		  \u00B7 {0}", msg);
					break;
				case ArchivingDlgViewModel.MessageType.Progress:
					_logBox.WriteMessage(Environment.NewLine + msg);
					break;
				case ArchivingDlgViewModel.MessageType.Warning:
					_logBox.WriteWarning(msg);
					break;
				case ArchivingDlgViewModel.MessageType.Error:
					_logBox.WriteMessageWithColor("Red", msg + Environment.NewLine);
					break;
				case ArchivingDlgViewModel.MessageType.Success:
					_logBox.WriteMessageWithColor(Color.DarkGreen, Environment.NewLine + msg);
					break;
				case ArchivingDlgViewModel.MessageType.Volatile:
					_logBox.WriteMessage(msg);
					_logBox.Tag = true;
					break;
			}
		}

		/// ------------------------------------------------------------------------------------
		void model_DisplayError(string msg, string packageTitle, Exception e)
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
		}
		
		/// ------------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			try
			{
				WaitCursor.Show();
				_viewModel.IncrementProgressBarAction = () => _progressBar.Increment(1);
				_buttonCreatePackage.Enabled = _viewModel.Initialize();
				_logBox.ScrollToTop();
				_progressBar.Maximum = _viewModel.CalculateMaxProgressBarValue();
				WaitCursor.Hide();
			}
			catch
			{
				WaitCursor.Hide();
				throw;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleRampLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var tgt = e.Link.LinkData as string;

			if (!string.IsNullOrEmpty(tgt))
				System.Diagnostics.Process.Start(tgt);
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
