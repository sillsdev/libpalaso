using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.WritingSystems.WSTree
{
	public partial class GetDialectNameDialog : Form
	{
		public GetDialectNameDialog()
		{
			InitializeComponent();
		}

		public string DialectName {
			get
				{
					return _dialectName.Text;
				}
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}
	}
}