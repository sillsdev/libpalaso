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

		public string SampleText
		{
			set
			{
				_fontAndKeyboardControl.SampleText = value;
			}
		}

		public string FontFamily
		{
			get
			{
				return _fontAndKeyboardControl.FontFamilyName;
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
				_fontAndKeyboardControl.FontFamilyName = value;
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
				return _fontAndKeyboardControl.KeyboardName;
			}
			set
			{
				  _fontAndKeyboardControl.KeyboardName = value;
			}
		}

		public bool RightToLeftScript
		{
			get
			{
				return _fontAndKeyboardControl.RightToLeftScript;
			}


			set
			{
				_fontAndKeyboardControl.RightToLeftScript = value;
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