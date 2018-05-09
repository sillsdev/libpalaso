using System;
using System.Drawing;
using System.Windows.Forms;
using SIL.PlatformUtilities;

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

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			// On Linux, _wsIdentifierView, which is initially designed to cover the full width
			// of this dialog, ends up narrower than expected, with some of its internal controls
			// scrunched up.  Restoring the full width fixes the appearance.  See the comment and
			// image near the bottom of https://silbloom.myjetbrains.com/youtrack/issue/BL-5736.
			if (Platform.IsMono && _wsIdentifierView.Width < this.ClientSize.Width)
				_wsIdentifierView.Size = new Size(this.ClientSize.Width, _wsIdentifierView.Height);
		}
	}
}
