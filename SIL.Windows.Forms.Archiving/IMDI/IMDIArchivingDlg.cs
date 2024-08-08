using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using L10NSharp;
using SIL.Archiving;
using SIL.Archiving.IMDI;
using SIL.Windows.Forms.PortableSettingsProvider;
// ReSharper disable InconsistentNaming

namespace SIL.Windows.Forms.Archiving.IMDI
{
	/// <summary />
	[PublicAPI]
	public class IMDIArchivingDlg : ArchivingDlg
	{
		private TableLayoutPanel _destinationFolderTable;
		private Label _destinationFolderLabel;
		private LinkLabel _browseDestinationFolder;

		private TableLayoutPanel _imdiProgramTable;
		private LinkLabel _browseIMDIProgram;
		private ComboBox _selectIMDIPreset;

		/// ------------------------------------------------------------------------------------
		/// <param name="model">View model</param>
		/// <param name="appSpecificArchivalProcessInfo">Application can use this to pass
		/// additional information that will be displayed to the user in the dialog to explain
		/// any application-specific details about the archival process.</param>
		/// <param name="localizationManagerId">The ID of the localization manager for the
		/// calling application.</param>
		/// <param name="programDialogFont">Application can set this to ensure a consistent look
		/// in the UI (especially useful for when a localization requires a particular font).</param>
		/// <param name="settings">Location, size, and state where the client would like the
		/// dialog box to appear (can be null)</param>
		/// ------------------------------------------------------------------------------------
		public IMDIArchivingDlg(IMDIArchivingDlgViewModel model,
			string appSpecificArchivalProcessInfo, string localizationManagerId = null,
			Font programDialogFont = null, FormSettings settings = null)
			: base(model, appSpecificArchivalProcessInfo, localizationManagerId,
				programDialogFont, settings, LocalizationManager.GetString(
					"DialogBoxes.ArchivingDlg.IsleMetadataInitiative", "Isle Metadata Initiative",
					"Typically this probably does not need to be localized."))
		{
			// DO NOT SHOW THE LAUNCH OPTION AT THIS TIME
			model.PathToProgramToLaunch = null;
			model.InitializationFailed += Model_InitializationFailed;

			InitializeNewControls();

			// get the saved IMDI program value
			model.GetSavedValues();

			// set control properties
			SetControlProperties();
		}

		private void Model_InitializationFailed(object sender, EventArgs e)
		{
			_browseDestinationFolder.Enabled = false;
		}

		private void InitializeNewControls()
		{
			AddDestinationFolder();
			AddIMDIProgram();
		}

		private void AddDestinationFolder()
		{
			_destinationFolderTable = new TableLayoutPanel
			{
				ColumnCount = 2,
				RowCount = 1,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};
			_destinationFolderTable.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
			_destinationFolderTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.AutoSize });
			_destinationFolderTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 100 });

			// add the "Change Folder" link
			_browseDestinationFolder = new LinkLabel
			{
				Text = LocalizationManager.GetString("DialogBoxes.IMDIArchivingDlg.ChangeDestinationFolder",
					"Change Folder"),
				Anchor = AnchorStyles.Left,
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft,
			};

			_browseDestinationFolder.Click += _browseDestinationFolder_Click;
			_destinationFolderTable.Controls.Add(_browseDestinationFolder, 0, 0);

			// add the current folder label
			_destinationFolderLabel = new Label
			{
				Anchor = AnchorStyles.Left,
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft
			};
			SetDestinationLabelText();
			_destinationFolderTable.Controls.Add(_destinationFolderLabel, 1, 0);

			_flowLayoutExtra.Controls.Add(_destinationFolderTable);
		}

		public override string ArchiveTypeName =>
			LocalizationManager.GetString("DialogBoxes.ArchivingDlg.IMDIArchiveType", "IMDI",
				"This is the abbreviation for Isle Metadata Initiative (https://www.mpi.nl/imdi/). " +
				"Typically this probably does not need to be localized.");

		/// ------------------------------------------------------------------------------------
		protected override string InformativeText
		{
			get
			{
				string programInfo = string.IsNullOrEmpty(_viewModel.NameOfProgramToLaunch) ?
					string.Format(LocalizationManager.GetString(
							"DialogBoxes.ArchivingDlg.NoIMDIProgramInfoText",
							"The {0} package will be created in {1}.",
							"Parameter 0 is 'IMDI'; " +
							"Parameter 1 is the path where the package is created."),
						_viewModel.ArchiveType, _viewModel.PackagePath)
					:
					string.Format(LocalizationManager.GetString(
							"DialogBoxes.ArchivingDlg.IMDIProgramInfoText",
							"This tool will help you use {0} to archive your {1} data. When the {1} package has been " +
							"created, you can launch {0} and enter any additional information before doing the actual submission.",
							"Parameter 0 is the name of the program that will be launched to further prepare the IMDI data for submission; " +
							"Parameter 1 is the name of the calling (host) program (SayMore, FLEx, etc.)"),
						_viewModel.NameOfProgramToLaunch, _viewModel.AppName);
				return string.Format(LocalizationManager.GetString(
							"DialogBoxes.ArchivingDlg.IMDIOverviewText",
							"{0} ({1}) is a metadata standard to describe multi-media and multi-modal language " +
							"resources. The standard provides interoperability for browsable and searchable " +
							"corpus structures and resource descriptions.",
							"Parameter 0  is 'Isle Metadata Initiative' (the first occurrence will be turned into a hyperlink); " +
							"Parameter 1 is 'IMDI'"),
						_archiveInfoHyperlinkText, _viewModel.ArchiveType) +
					" " + _appSpecificArchivalProcessInfo +
					" " + programInfo;
			}
		}

		protected override void DisableControlsDuringPackageCreation()
		{
			base.DisableControlsDuringPackageCreation();
			_destinationFolderTable.Visible = false;
		}

		protected override void PackageCreationComplete(string result)
		{
			base.PackageCreationComplete(result);

			var mainExportFile = result;

			if (mainExportFile != null)
			{
				void PutIMDIPackagePathOnClipboard()
				{
					// copy the path to the imdi file to the clipboard

					// SP-818: Crash in IMDI export when dialog tries to put string on clipboard
					//   18 FEB 2014, Phil Hopper: I found this possible solution using retries on StackOverflow
					//   https://stackoverflow.com/questions/5707990/requested-clipboard-operation-did-not-succeed
					//Clipboard.SetData(DataFormats.Text, _imdiData.MainExportFile);
					Clipboard.SetDataObject(mainExportFile, true, 3, 500);
				}

				if (InvokeRequired)
					Invoke(new Action(PutIMDIPackagePathOnClipboard));
				else
					PutIMDIPackagePathOnClipboard();

				var successMsg = LocalizationManager.GetString("DialogBoxes.ArchivingDlg.ReadyToCallIMDIMsg",
					"Exported to {0}. This path is now on your clipboard. If you are using Arbil, go to File, Import, then paste this path in.");
				DisplayMessage(string.Format(successMsg, mainExportFile), ArchivingDlgViewModel.MessageType.Success);
			}
		}

		public override string GetMessage(ArchivingDlgViewModel.StringId msgId)
		{
			switch (msgId)
			{
				case ArchivingDlgViewModel.StringId.IMDIPackageInvalid:
					return LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.IMDIPackageInvalid", "The IMDI package is invalid.",
						"This is displayed in the Archive Using IMDI dialog box if the calling " +
						"program fails to initialize the IMDI package with valid settings.");
				case ArchivingDlgViewModel.StringId.IMDIActorsGroup:
					return LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.IMDIActorsGroup", "Actors",
						"This is the heading displayed in the Archive Using IMDI dialog box for the files for the actors/participants");
				default: return base.GetMessage(msgId);
			}
		}

		void SetDestinationLabelText()
		{
			var labelText = ((IMDIArchivingDlgViewModel)_viewModel).OutputFolder;
			if (labelText.Length > 50)
				labelText = labelText.Substring(0, 3) + "..." + labelText.Substring(labelText.Length - 44);

			_destinationFolderLabel.Text = labelText;
		}

		void _browseDestinationFolder_Click(object sender, EventArgs e)
		{
			using (var chooseFolder = new FolderBrowserDialog())
			{
				var previousPath = ((IMDIArchivingDlgViewModel)_viewModel).OutputFolder;
				if (string.IsNullOrEmpty(previousPath))
					previousPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

				chooseFolder.Description = LocalizationManager.GetString(
					"DialogBoxes.IMDIArchivingDlg.ArchivingIMDILocationDescription",
					"Select a base folder where the IMDI directory structure should be created.");
				chooseFolder.ShowNewFolderButton = true;
				chooseFolder.SelectedPath = previousPath;
				if (chooseFolder.ShowDialog() == DialogResult.Cancel)
					return;

				var dir = chooseFolder.SelectedPath;

				// check if the selected path is currently writable by the current user
				if (!_viewModel.IsPathWritable(dir))
					return;

				((IMDIArchivingDlgViewModel)_viewModel).OutputFolder = dir;

				SetDestinationLabelText();
				SetControlProperties();
			}
		}

		private void AddIMDIProgram()
		{
			_imdiProgramTable = new TableLayoutPanel
			{
				ColumnCount = 2,
				RowCount = 1,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};
			_imdiProgramTable.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
			_imdiProgramTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.AutoSize });
			_imdiProgramTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 100 });

			// add the preset combo box
			_selectIMDIPreset = new ComboBox { Anchor = AnchorStyles.Left, DropDownStyle = ComboBoxStyle.DropDownList };
			_selectIMDIPreset.Items.AddRange(new object[] { "Arbil", "Other" });
			SizeComboBox(_selectIMDIPreset);
			_imdiProgramTable.Controls.Add(_selectIMDIPreset, 0, 0);

			// add the "Change Program to Launch" link
			_browseIMDIProgram = new LinkLabel
			{
				Text = LocalizationManager.GetString("DialogBoxes.IMDIArchivingDlg.ChangeProgramToLaunch",
					"Change Program to Launch"),
				Anchor = AnchorStyles.Left,
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft
			};

			_browseIMDIProgram.Click += SelectIMDIProgramOnClick;
			_imdiProgramTable.Controls.Add(_browseIMDIProgram, 1, 0);

			// DO NOT SHOW THE LAUNCH OPTION AT THIS TIME
			//_flowLayoutExtra.Controls.Add(_imdiProgramTable);
		}

		private void SelectIMDIProgramOnClick(object sender, EventArgs eventArgs)
		{
			using (var chooseIMDIProgram = new OpenFileDialog())
			{
				chooseIMDIProgram.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				chooseIMDIProgram.RestoreDirectory = true;
				chooseIMDIProgram.CheckFileExists = true;
				chooseIMDIProgram.CheckPathExists = true;
				chooseIMDIProgram.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}",
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.ProgramsFileTypeLabel", "Programs"),
					"*.exe;*.pif;*.com;*.bat;*.cmd",
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.AllFilesLabel", "All Files"),
					"*.*");
				chooseIMDIProgram.FilterIndex = 0;
				chooseIMDIProgram.Multiselect = false;
				chooseIMDIProgram.Title = LocalizationManager.GetString(
					"DialogBoxes.IMDIArchivingDlg.SelectIMDIProgram", "Select the program to launch after IMDI package is created");
				chooseIMDIProgram.ValidateNames = true;
				if (chooseIMDIProgram.ShowDialog() == DialogResult.OK && File.Exists(chooseIMDIProgram.FileName))
				{
					((IMDIArchivingDlgViewModel)_viewModel).OtherProgramPath = chooseIMDIProgram.FileName;
					SetControlProperties();
				}
			}
		}

		/// <summary>Resize a ComboBox to fit the width of the list items</summary>
		private static void SizeComboBox(ComboBox comboBox)
		{
			var maxWidth = 0;

// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var item in comboBox.Items)
			{
				var itmWidth = TextRenderer.MeasureText(item.ToString(), comboBox.Font).Width;
				if (itmWidth > maxWidth)
					maxWidth = itmWidth;
			}

			comboBox.Width = maxWidth + 30;
		}

		private void GetSavedValues()
		{
			SelectPreset(((IMDIArchivingDlgViewModel)_viewModel).ProgramPreset);
			_selectIMDIPreset.SelectedIndexChanged += SelectIMDIPresetOnSelectedIndexChanged;
		}

		private void SelectIMDIPresetOnSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			((IMDIArchivingDlgViewModel) _viewModel).ProgramPreset = _selectIMDIPreset.SelectedItem.ToString();
			SetControlProperties();
		}

		private void SelectPreset(string preset)
		{
			foreach (var item in _selectIMDIPreset.Items.Cast<object>().Where(item => item.ToString() == preset))
			{
				_selectIMDIPreset.SelectedItem = item;
				return;
			}

			// if you are here, the selected item was not found
			_selectIMDIPreset.SelectedIndex = 0;
		}

		private void SetControlProperties()
		{
			// DO NOT SHOW THE LAUNCH OPTION AT THIS TIME
			//_browseIMDIProgram.Visible = (_selectIMDIPreset.SelectedIndex == (_selectIMDIPreset.Items.Count - 1));
			//UpdateLaunchButtonText();
			_buttonLaunchRamp.Visible = false;
			_tableLayoutPanel.SetColumn(_buttonCreatePackage, 1);
			_buttonCreatePackage.Text = LocalizationManager.GetString("DialogBoxes.IMDIArchivingDlg.CreatePackageButtonLabel", "Create Package");
			UpdateOverviewText();
		}

	}
}
