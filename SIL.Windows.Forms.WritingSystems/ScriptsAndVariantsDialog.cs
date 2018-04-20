using System.Windows.Forms;

namespace SIL.Windows.Forms.WritingSystems
{
	/// <summary>
	/// Recommended usage: Bind to a WritingSystemSetupModel that has been locked to only deal with
	/// the one ScriptRegionVariant option in the "Special" combo.
	/// </summary>
	public partial class ScriptsAndVariantsDialog : Form
	{
		public ScriptsAndVariantsDialog()
		{
			InitializeComponent();
			_wsIdentifierView.Selected();
			_wsIdentifierView.Select();
		}

		public void BindToModel(WritingSystemSetupModel wsSetupModel)
		{
			_wsIdentifierView.BindToModel(wsSetupModel);
		}

		public void SelectionForSpecialCombo()
		{
			_wsIdentifierView.Select();
		}

		private void _okButton_Click(object sender, System.EventArgs e)
		{
			_wsIdentifierView.MoveDataFromViewToModel(); // make sure the latest changes are saved
		}
	}
}
