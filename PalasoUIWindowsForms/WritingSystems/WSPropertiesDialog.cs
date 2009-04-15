using System;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSPropertiesDialog : Form
	{
		private WritingSystemSetupPM _model;

		public WSPropertiesDialog()
		{
			InitializeComponent();
			_model = new WritingSystemSetupPM(new LdmlInFolderWritingSystemStore());
			_wsPropertiesPanel.BindToModel(_model);
		}

   /* turned out to be hard... so many events are bound to the model, when the dlg
	* closes we'd need to carefully unsubscribe them alll.
	* Better to try again with a weak event model*/
		/// <summary>
		/// Use this one to keep, say, a picker up to date with any change you make
		/// while using the dialog.
		/// </summary>
		/// <param name="writingSystemModel"></param>
		public WSPropertiesDialog(WritingSystemSetupPM writingSystemModel)
		{
			InitializeComponent();
			_model = writingSystemModel;
			_wsPropertiesPanel.BindToModel(_model);
		}

		public WSPropertiesDialog(string writingSystemStorePath)
		{
			InitializeComponent();
			_model = new WritingSystemSetupPM(new LdmlInFolderWritingSystemStore(writingSystemStorePath));
			_wsPropertiesPanel.BindToModel(_model);
		}

		public DialogResult  ShowDialog(string initiallySelectWritingSystemRfc4646)
		{
			_model.SetCurrentIndexFromRfc46464(initiallySelectWritingSystemRfc4646);
			return this.ShowDialog();
		}

		private void _closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			_model.Save();
			base.OnClosed(e);
		}
	}
}