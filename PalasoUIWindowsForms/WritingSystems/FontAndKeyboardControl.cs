using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class FontAndKeyboardControl : UserControl
	{
		private string _sampleText="type here to test the font and keyboard";
		private SetupPM _model;

		public FontAndKeyboardControl()
		{
			InitializeComponent();
		}

		internal void BindToModel(SetupPM model)
		{
			_model = model;
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
			if(_rightToLeftBox.Checked)
			{
				_sampleTextBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			}
			else
			{
				_sampleTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
			}

			try
			{
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
			_fontFamilyCombo.Items.Clear();
			if (_model == null)
			{
				UpdateDisplay();
				return;
			}
			foreach (string s in _model.KeyboardNames)
			{
				_keyboardCombo.Items.Add(s);
			}

			foreach (FontFamily family in _model.FontFamilies)
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
			KeyboardController.ActivateKeyboard(KeyboardName);
		}

		static private  IEnumerable<String> KeyboardNames
		{
		  get
			{
				List<String> keyboards = new List<string>();
				keyboards.Add("(default)");
				List<KeyboardController.KeyboardDescriptor> systemKeyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All);
				foreach (KeyboardController.KeyboardDescriptor keyboard in systemKeyboards)
				{
					keyboards.Add(keyboard.Name);
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

		private void _rightToLeftBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}


	}
}