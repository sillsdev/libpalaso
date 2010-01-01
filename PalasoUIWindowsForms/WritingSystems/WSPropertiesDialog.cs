using System;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSPropertiesDialog : Form
	{
		private readonly WritingSystemSetupModel _model;

		public WSPropertiesDialog()
		{
			InitializeComponent();
			_model = new WritingSystemSetupModel(new LdmlInFolderWritingSystemStore());
			_writingSystemSetupView.BindToModel(_model);
		}

   /* turned out to be hard... so many events are bound to the model, when the dlg
	* closes we'd need to carefully unsubscribe them alll.
	* Better to try again with a weak event model*/
		/// <summary>
		/// Use this one to keep, say, a picker up to date with any change you make
		/// while using the dialog.
		/// </summary>
		/// <param name="writingSystemModel"></param>
		public WSPropertiesDialog(WritingSystemSetupModel writingSystemModel)
		{
			InitializeComponent();
			_model = writingSystemModel;
			_writingSystemSetupView.BindToModel(_model);
		}

		public WSPropertiesDialog(string writingSystemStorePath)
		{
			InitializeComponent();
			_model = new WritingSystemSetupModel(new LdmlInFolderWritingSystemStore(writingSystemStorePath));
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