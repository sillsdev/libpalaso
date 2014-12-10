using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SIL.WritingSystems.WindowsForms.Keyboarding;

namespace SIL.WritingSystems.WindowsForms
{
	public partial class WSFontControl : UserControl
	{
		private WritingSystemSetupModel _model;
		private string _defaultFontName;
		private float _defaultFontSize;
		private IKeyboardDefinition _defaultKeyboard;

		public WSFontControl()
		{
			InitializeComponent();
			_defaultFontName = _testArea.Font.Name;
			_defaultFontSize = _testArea.Font.SizeInPoints;
			_fontComboBox.Text = _defaultFontName;
			_fontSizeComboBox.Text = _defaultFontSize.ToString();
			_promptForFontTestArea.SetPrompt(_testArea, "Use this area to type something to test out your font.");
			if (KeyboardController.IsInitialized)
				KeyboardController.Register(_testArea);
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
			}
			_model = model;
			_model.SelectionChanged += ModelSelectionChanged;
			_model.CurrentItemUpdated += ModelCurrentItemUpdated;
			PopulateFontList();
			UpdateFromModel();
			this.Disposed += OnDisposed;
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelSelectionChanged;
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			UpdateFromModel();
		}

		private void ModelCurrentItemUpdated(object sender, EventArgs e)
		{
			UpdateFromModel();
		}

		private void UpdateFromModel()
		{
			if (!_model.HasCurrentSelection)
			{
				Enabled = false;
				return;
			}
			Enabled = true;
			float currentSize;
			if (!float.TryParse(_fontSizeComboBox.Text, out currentSize))
			{
				currentSize = float.NaN;
			}
			if (_model.CurrentDefaultFontName != _fontComboBox.Text)
			{
				if (string.IsNullOrEmpty(_model.CurrentDefaultFontName))
				{
					_fontComboBox.Text = _defaultFontName;
				}
				else
				{
					_fontComboBox.Text = _model.CurrentDefaultFontName;
				}
				_fontComboBox.SelectAll();
			}
			if (_model.CurrentDefaultFontSize != currentSize)
			{
				if (_model.CurrentDefaultFontSize == 0)
				{
					_fontSizeComboBox.Text = _defaultFontSize.ToString();
				}
				else
				{
					_fontSizeComboBox.Text = _model.CurrentDefaultFontSize.ToString();
				}
				_fontSizeComboBox.SelectAll();
			}
			if (_rightToLeftCheckBox.Checked != _model.CurrentRightToLeftScript)
			{
				_rightToLeftCheckBox.Checked = _model.CurrentRightToLeftScript;
			}
			SetTestAreaFont();
		}

		private void PopulateFontList()
		{
			// For clearing the list was resizing the combo box, so we save the original size and then reset it
			Rectangle originalBounds = _fontComboBox.Bounds;
			List<string> fontitems = new List<string>();
			_fontComboBox.Items.Clear();
			if (_model == null)
			{
				return;
			}
			foreach (FontFamily fontFamily in WritingSystemSetupModel.FontFamilies)
			{
				if (!fontFamily.IsStyleAvailable(FontStyle.Regular))
				{
					continue;
				}
				fontitems.Add(fontFamily.Name);
			}
			fontitems.Sort();
			foreach (string fontname in fontitems)
			{
				_fontComboBox.Items.Add(fontname);
			}
			_fontComboBox.Bounds = originalBounds;
		}

		private void SetTestAreaFont()
		{
			float fontSize;
			if (!float.TryParse(_fontSizeComboBox.Text, out fontSize))
			{
				fontSize = _defaultFontSize;
			}
			if (fontSize <= 0 || float.IsNaN(fontSize) || float.IsInfinity(fontSize))
			{
				fontSize = _defaultFontSize;
			}
			string fontName = _fontComboBox.Text;
			if (!_fontComboBox.Items.Contains(fontName))
			{
				fontName = _defaultFontName;
			}
			_testArea.Font = new Font(fontName, fontSize);
			_testArea.ForeColor = Color.Black;
			if (_testArea.Font.Name != _fontComboBox.Text.Trim())
			{
				_testArea.ForeColor = Color.Gray;
			}
			_testArea.RightToLeft = _model.CurrentRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
		}

		private void FontComboBox_TextChanged(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			if (_model.HasCurrentSelection && _model.CurrentDefaultFontName != _fontComboBox.Text)
			{
				_model.CurrentDefaultFontName = _fontComboBox.Text;
			}
			SetTestAreaFont();
		}

		private void FontSizeComboBox_TextChanged(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			float newSize;
			if (!float.TryParse(_fontSizeComboBox.Text, out newSize))
			{
				return;
			}
			if (_model.HasCurrentSelection && _model.CurrentDefaultFontSize != newSize)
			{
				_model.CurrentDefaultFontSize = newSize;
			}
			SetTestAreaFont();
		}

		private void _testArea_Enter(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			_defaultKeyboard = Keyboard.Controller.ActiveKeyboard;
			_model.ActivateCurrentKeyboard();
		}

		private void _testArea_Leave(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			_defaultKeyboard.Activate();
		}

		private void RightToLeftCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			if (_model.HasCurrentSelection && _model.CurrentRightToLeftScript != _rightToLeftCheckBox.Checked)
			{
				_model.CurrentRightToLeftScript = _rightToLeftCheckBox.Checked;
			}
			SetTestAreaFont();
		}
	}
}
