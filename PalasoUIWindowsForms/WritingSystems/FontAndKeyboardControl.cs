using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class FontAndKeyboardControl : UserControl
	{
		private string _sampleText="type here to test the font and keyboard";

		public FontAndKeyboardControl()
		{
			InitializeComponent();
		}

		public bool RightToLeftScript
		{
			get
			{
				return _rightToLeftBox.Checked;
			}

			set
			{
				_rightToLeftBox.Checked = value;
			}
		}

		public string KeyboardName
		{
			get
			{
				if (_keyboardCombo.Text == "(default)")
				{
					return string.Empty;
				}
				return _keyboardCombo.Text;
			}

			set
			{
				if (String.IsNullOrEmpty(value))
				{
					value = "(default)";
				}

				_keyboardCombo.Text = value;
			}
		}

		public string FontFamilyName
		{
			get { return this._fontFamilyCombo.Text; }
			set
			{
				this._fontFamilyCombo.Text = value;
				UpdateDisplay();
			}
		}

		private void UpdateDisplay()
		{

			try
			{
				KeymanLink.KeymanLink link = new KeymanLink.KeymanLink();

				_sampleTextBox.Font = new Font(_fontFamilyCombo.Text, 12);
				_sampleTextBox.ForeColor = Color.Black;
				if (_sampleTextBox.Font.FontFamily.Name != _fontFamilyCombo.Text.Trim())
				{
					 _sampleTextBox.ForeColor=Color.Gray;
				}
			}
			catch(Exception)
			{
				_sampleTextBox.ForeColor=Color.Gray;
			}

			this.Invalidate();
		}

		private void _fontFamilyCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void OnLoad(object sender, EventArgs e)
		{
			_keyboardCombo.Items.Clear();
			foreach (string s in KeyboardNames)
			{
				_keyboardCombo.Items.Add(s);
			}

			_fontFamilyCombo.Items.Clear();
			foreach (FontFamily family in System.Drawing.FontFamily.Families)
			{
				_fontFamilyCombo.Items.Add(family.Name);
			}
				_sampleTextBox.Text =SampleText ;

			UpdateDisplay();
		   // AssignKeyboard();
		}

		private void _fontFamilyCombo_TextChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		public void AssignKeyboard()
		{
			try
			{
				InputLanguage inputLanguage = FindInputLanguage(this.KeyboardName);
				if (inputLanguage != null)
				{
					InputLanguage.CurrentInputLanguage = inputLanguage;
				}
				else
				{
					//set the windows back to default so it doesn't interfere
					//nice idea but is unneeded... perhaps keyman is calling this too
					//InputLanguage.CurrentInputLanguage = InputLanguage.DefaultInputLanguage;
					KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
					if (!string.IsNullOrEmpty(KeyboardName))
					{
						keymanLink.SelectKeymanKeyboard(KeyboardName, true);
					}
				}
			}
			catch (Exception error)
			{
				MessageBox.Show("There was an error trying to set the keyboard. " + error.Message);
			}
		}

		static private InputLanguage FindInputLanguage(string name)
		{
			if (InputLanguage.InstalledInputLanguages != null) // as is the case on Linux
			{
				foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
				{
					if (l.LayoutName == name)
					{
						return l;
					}
				}
			}
			return null;
		}
		private  IEnumerable<String> KeyboardNames
		{
		  get
			{
				List<String> keyboards = new List<string>();
				keyboards.Add("(default)");

				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (keymanLink.Initialize(false))
				{
					foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in keymanLink.Keyboards)
					{
						keyboards.Add(keyboard.KbdName);
					}
				}

				foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
				{
					keyboards.Add(lang.LayoutName);
				}
				return keyboards;
			}
		}

		public string SampleText
		{
			get
			{
				return _sampleText;
			}
			set
			{
				_sampleText = value;
			}
		}

		private void _keyboardCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void _sampleTextBox_Enter(object sender, EventArgs e)
		{
			AssignKeyboard();
		}

		private void _sampleTextBox_Leave(object sender, EventArgs e)
		{
			InputLanguage.CurrentInputLanguage = InputLanguage.DefaultInputLanguage;
		}


	}
}