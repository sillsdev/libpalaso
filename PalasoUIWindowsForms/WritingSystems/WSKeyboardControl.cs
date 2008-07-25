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
	public partial class WSKeyboardControl : UserControl
	{
		private SetupPM _model;
		private string _defaultKeyboard;
		private string _defaultFontName;
		private float _defaultFontSize;

		public WSKeyboardControl()
		{
			InitializeComponent();
			_defaultFontSize = _testArea.Font.SizeInPoints;
			_defaultFontName = _testArea.Font.Name;
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
			PopulateKeyboardList();
		}

		private void PopulateKeyboardList()
		{
			if (_model == null)
			{
				return;
			}
			Rectangle originalBounds = _keyboardComboBox.Bounds;
			_keyboardComboBox.Items.Clear();
			foreach (string keyboardName in _model.KeyboardNames)
			{
				_keyboardComboBox.Items.Add(keyboardName);
			}
			_keyboardComboBox.Bounds = originalBounds;
		}

		void ModelCurrentItemUpdated(object sender, EventArgs e)
		{
			UpdateFromModel();
		}

		void ModelSelectionChanged(object sender, EventArgs e)
		{
			UpdateFromModel();
		}

		private void UpdateFromModel()
		{
			if (!_model.HasCurrentSelection)
			{
				_keyboardComboBox.Enabled = false;
				_testArea.Enabled = false;
				return;
			}
			_keyboardComboBox.Enabled = true;
			_testArea.Enabled = true;
			if (_model.CurrentKeyboard != _keyboardComboBox.Text)
			{
				_keyboardComboBox.Text = _model.CurrentKeyboard;
			}
			SetTestAreaFont();
		}

		private void SetTestAreaFont()
		{
			float fontSize = _defaultFontSize;
			if (fontSize <= 0 || float.IsNaN(fontSize) || float.IsInfinity(fontSize))
			{
				fontSize = _defaultFontSize;
			}
			string fontName = _model.CurrentDefaultFontName;
			if (string.IsNullOrEmpty(fontName))
			{
				fontName = _defaultFontName;
			}
			_testArea.Font = new Font(fontName, fontSize);
			_testArea.RightToLeft = _model.CurrentRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
		}

		private void _keyboardComboBox_TextChanged(object sender, EventArgs e)
		{
			if (_model.CurrentKeyboard != _keyboardComboBox.Text)
			{
				_model.CurrentKeyboard = _keyboardComboBox.Text;
			}
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
	}
}
