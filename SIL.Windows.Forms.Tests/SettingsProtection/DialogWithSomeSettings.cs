using System.Windows.Forms;

namespace SIL.Windows.Forms.Tests.SettingsProtection
{
	public partial class DialogWithSomeSettings : Form
	{
		public DialogWithSomeSettings(bool enableMaybeCheckBox)
		{
			InitializeComponent();
			_chkManageMaybeButton.Enabled = enableMaybeCheckBox;
		}

		public bool ManageTheMaybeButton => _chkManageMaybeButton.Checked;
	}
}
