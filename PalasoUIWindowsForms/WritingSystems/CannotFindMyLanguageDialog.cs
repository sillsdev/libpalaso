using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class CannotFindMyLanguageDialog : Form
	{
		public CannotFindMyLanguageDialog()
		{
			InitializeComponent();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
