using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SIL.Windows.Forms.WritingSystems.WSTree;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class WritingSystemSetupDialog : Form
	{
		private readonly WritingSystemSetupModel _model;

		//public WritingSystemSetupDialog()
		//{
		//    InitializeComponent();
		//    _model = new WritingSystemSetupModel(new LdmlInFolderWritingSystemRepository());
		//    _writingSystemSetupView.BindToModel(_model);
		//}

		/// <summary>
		/// Use this to set the appropriate kinds of writing systems according to your
		/// application.  For example, is the user of your app likely to want voice? ipa? dialects?
		/// </summary>
		public WritingSystemSuggestor WritingSystemSuggestor
		{
			get { return _model.WritingSystemSuggestor; }
		}

		private bool DisposeRepository { get; set; }

   /* turned out to be hard... so many events are bound to the model, when the dlg
	* closes we'd need to carefully unsubscribe them alll.
	* Better to try again with a weak event model (JH)
	* Or perhaps better yet the passive view model
	*/
		/// <summary>
		/// Use this one to keep, say, a picker up to date with any change you make
		/// while using the dialog.
		/// </summary>
		/// <param name="writingSystemModel"></param>
		public WritingSystemSetupDialog(WritingSystemSetupModel writingSystemModel)
		{
			InitializeComponent();
			_model = writingSystemModel;
			_writingSystemSetupView.BindToModel(_model);
		}

		public WritingSystemSetupDialog(IWritingSystemRepository repository)
		{
			InitializeComponent();
			_model = new WritingSystemSetupModel(repository);
			_writingSystemSetupView.BindToModel(_model);
		}

		public IWritingSystemRepository WritingSystems
		{
			get { return _model.WritingSystems; }
		}

		public DialogResult ShowDialog(string initiallySelectWritingSystemLanguageTag)
		{
			_model.SetCurrentIndexFromIetfLanguageTag(initiallySelectWritingSystemLanguageTag);
			return ShowDialog();
		}

		private void _closeButton_Click(object sender, EventArgs e)
		{
			try
			{
				_model.Save ();
				Close();
			}
			catch (ArgumentException exception)
			{
				MessageBox.Show (
					this, exception.Message, "Input Systems Error",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation
				);
			}
		}

		private void _openDirectory_Click(object sender, EventArgs e)
		{
			var openDir = new FolderBrowserDialog();

			openDir.RootFolder = Environment.SpecialFolder.Personal;

			// Set the help text description for the FolderBrowserDialog.
			openDir.Description = "Select the folder with Writing Systems";

			// Allow the user to create new files via the FolderBrowserDialog.
			openDir.ShowNewFolderButton = true;

			// Display the openFile dialog.
			DialogResult result = openDir.ShowDialog();

			if (result == DialogResult.OK)
			{
				string newDir = openDir.SelectedPath;
				var ldmlRepo = _model.WritingSystems as LdmlInFolderWritingSystemRepository;
				IEnumerable<ICustomDataMapper<WritingSystemDefinition>> customDataMappers = ldmlRepo != null ? ldmlRepo.CustomDataMappers : Enumerable.Empty<ICustomDataMapper<WritingSystemDefinition>>();
				LdmlInFolderWritingSystemRepository repository = LdmlInFolderWritingSystemRepository.Initialize(newDir, customDataMappers);
				var dlg = new WritingSystemSetupDialog(repository);

				dlg.WritingSystemSuggestor.SuggestVoice = true;
				dlg.WritingSystemSuggestor.OtherKnownWritingSystems = null;
				dlg.Text = String.Format("Writing Systems in folder {0}", newDir);

				dlg.Show();
			}
		}

		private void _openGlobal_Click(object sender, EventArgs e)
		{
			var dlg = new WritingSystemSetupDialog(GlobalWritingSystemRepository.Initialize());
			dlg.WritingSystemSuggestor.SuggestVoice = true;
			dlg.WritingSystemSuggestor.OtherKnownWritingSystems = null;
			dlg.DisposeRepository = true;
			dlg.Text = String.Format("Writing Systems for all users of this computer");

			dlg.Show();
		}

	}
}