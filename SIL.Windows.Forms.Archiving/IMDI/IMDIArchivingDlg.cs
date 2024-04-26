using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace SIL.Windows.Forms.Archiving.IMDI
{
	/// <summary />
	public class IMDIArchivingDlg : ArchivingDlg
	{
		private TableLayoutPanel _destinationFolderTable;
		private Label _destinationFolderLabel;
		private LinkLabel _browseDestinationFolder;

		private TableLayoutPanel _imdiProgramTable;
		private LinkLabel _browseIMDIProgram;
		private ComboBox _selectIMDIPreset;

		/// <summary />
		public IMDIArchivingDlg(ArchivingDlgViewModel model, string localizationManagerId, Font programDialogFont, FormSettings settings) 
			: base(model, localizationManagerId, programDialogFont, settings)
		{
			// DO NOT SHOW THE LAUNCH OPTION AT THIS TIME
			model.PathToProgramToLaunch = null;

			InitializeNewControls();

			// get the saved IMDI program value
			GetSavedValues();

			// set control properties
			SetControlProperties();
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

		protected override void DisableControlsDuringPackageCreation()
		{
			base.DisableControlsDuringPackageCreation();
			_destinationFolderTable.Visible = false;
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
			_buttonCancel.Text = LocalizationManager.GetString("DialogBoxes.IMDIArchivingDlg.CloseButtonLabel", "Close");
			_buttonCreatePackage.Text = LocalizationManager.GetString("DialogBoxes.IMDIArchivingDlg.CreatePackageButtonLabel", "Create Package");
			UpdateOverviewText();
		}

	}
}
