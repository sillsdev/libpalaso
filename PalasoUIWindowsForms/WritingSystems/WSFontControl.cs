using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSFontControl : UserControl
	{
		private SetupPM _model;
		private string _defaultFontName;
		private float _defaultFontSize;
		private string _defaultKeyboard;

		public WSFontControl()
		{
			InitializeComponent();
			_defaultFontName = _testArea.Font.Name;
			_defaultFontSize = _testArea.Font.SizeInPoints;
		}

		public void BindToModel(SetupPM model)
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
				_fontComboBox.Enabled = false;
				_fontSizeComboBox.Enabled = false;
				_rightToLeftCheckBox.Enabled = false;
				_testArea.Enabled = false;
				return;
			}
			_fontComboBox.Enabled = true;
			_fontSizeComboBox.Enabled = true;
			_rightToLeftCheckBox.Enabled = true;
			_testArea.Enabled = true;
			float currentSize;
			if (!float.TryParse(_fontSizeComboBox.Text, out currentSize))
			{
				currentSize = float.NaN;
			}
			if (_model.CurrentDefaultFontName != _fontComboBox.Text)
			{
				_fontComboBox.Text = _model.CurrentDefaultFontName;
			}
			if (_model.CurrentDefaultFontSize != currentSize)
			{
				_fontSizeComboBox.Text = _model.CurrentDefaultFontSize.ToString();
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
			_fontComboBox.Items.Clear();
			if (_model == null)
			{
				return;
			}
			foreach (FontFamily fontFamily in _model.FontFamilies)
			{
				if (!fontFamily.IsStyleAvailable(FontStyle.Regular))
				{
					continue;
				}
				_fontComboBox.Items.Add(fontFamily.Name);
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
			if (_model.HasCurrentSelection && _model.CurrentDefaultFontName != _fontComboBox.Text)
			{
				_model.CurrentDefaultFontName = _fontComboBox.Text;
			}
			SetTestAreaFont();
		}

		private void FontSizeComboBox_TextChanged(object sender, EventArgs e)
		{
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
			_defaultKeyboard = KeyboardController.GetActiveKeyboard();
			_model.ActivateCurrentKeyboard();
		}

		private void _testArea_Leave(object sender, EventArgs e)
		{
			KeyboardController.ActivateKeyboard(_defaultKeyboard);
		}

		private void RightToLeftCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (_model.HasCurrentSelection && _model.CurrentRightToLeftScript != _rightToLeftCheckBox.Checked)
			{
				_model.CurrentRightToLeftScript = _rightToLeftCheckBox.Checked;
			}
			SetTestAreaFont();
		}
	}
}
