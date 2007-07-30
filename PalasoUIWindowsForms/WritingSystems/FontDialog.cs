using System;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class FontDialog : Form
	{
		public FontDialog()
		{
			InitializeComponent();
		}

		public string FontFamily
		{
			get
			{
				return _fontControl.FontFamilyName;
			}

			set
			{
				_fontControl.FontFamilyName = value;
			}
		}
		private void OnCancelClick(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close();
		}

		private void OnOkClick(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}
	}
}