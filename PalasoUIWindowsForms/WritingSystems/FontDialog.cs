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
				return _FontAndKeyboardControl.FontFamilyName;
			}

			set
			{
				if (String.IsNullOrEmpty(value))
				{
					if (HasFont("Doulos SIL"))
					{
						value = "Doulos SIL";
					}
					else
					{
						value = System.Drawing.FontFamily.GenericSansSerif.Name;
					}
				}
				_FontAndKeyboardControl.FontFamilyName = value;
			}
		}

		private bool HasFont(string name)
		{
			foreach (FontFamily family in System.Drawing.FontFamily.Families)
			{
				if(family.Name == name)
					return true;
			}
			return false;
		}

		public string Keyboard
		{
			get
			{
				return _FontAndKeyboardControl.KeyboardName;
			}
			set
			{
				  _FontAndKeyboardControl.KeyboardName = value;
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