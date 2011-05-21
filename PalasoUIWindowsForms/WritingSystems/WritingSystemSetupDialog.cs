using System;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
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

   /* turned out to be hard... so many events are bound to the model, when the dlg
	* closes we'd need to carefully unsubscribe them alll.
	* Better to try again with a weak event model*/
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

		public WritingSystemSetupDialog(string writingSystemRepositoryPath)
		{
			InitializeComponent();
			_model = new WritingSystemSetupModel(new LdmlInFolderWritingSystemRepository(writingSystemRepositoryPath));
			_writingSystemSetupView.BindToModel(_model);
		}

		public DialogResult  ShowDialog(string initiallySelectWritingSystemRfc4646)
		{
			_model.SetCurrentIndexFromRfc46464(initiallySelectWritingSystemRfc4646);
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
					this, exception.Message, "Writing Systems Error",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation
				);
			}
		}

	}
}