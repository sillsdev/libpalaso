using System;
using System.Drawing;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSKeyboardControl : UserControl
	{

		private class KeyboardAdapter
		{
			private KeyboardController.KeyboardDescriptor _descriptor;

			public KeyboardAdapter(KeyboardController.KeyboardDescriptor descriptor)
			{
				_descriptor = descriptor;
			}

			public string Id { get { return _descriptor.Id; } }

			public override string ToString()
			{
				return _descriptor.LongName;
			}
		}

		private WritingSystemSetupModel _model;
		private string _defaultKeyboard;
		private string _defaultFontName;
		private float _defaultFontSize;

		public WSKeyboardControl()
		{
			InitializeComponent();
			_defaultFontSize = _testArea.Font.SizeInPoints;
			_defaultFontName = _testArea.Font.Name;
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
			}
			_model = model;
			Enabled = false;
			if (_model != null)
			{
				_model.SelectionChanged += ModelSelectionChanged;
				_model.CurrentItemUpdated += ModelCurrentItemUpdated;
				PopulateKeyboardList();
				UpdateFromModel();
			}
			this.Disposed += OnDisposed;
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelSelectionChanged;
		}

		private void PopulateKeyboardList()
		{
			if (_model == null)
			{
				return;
			}
			Rectangle originalBounds = _keyboardComboBox.Bounds;
			_keyboardComboBox.Items.Clear();
			foreach (var keyboard in WritingSystemSetupModel.KeyboardNames)
			{
				_keyboardComboBox.Items.Add(new KeyboardAdapter(keyboard));
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
				Enabled = false;
				return;
			}
			Enabled = true;
			var currentKeyboard = _keyboardComboBox.SelectedItem as KeyboardAdapter;

			if (currentKeyboard == null || _model.CurrentKeyboard != currentKeyboard.Id)
			{
				foreach(var item in _keyboardComboBox.Items)
				{
					var keyboard = item as KeyboardAdapter;
					if (keyboard.Id == _model.CurrentKeyboard)
					{
						_keyboardComboBox.SelectedItem = keyboard;
						break;
					}
				}
			}
			SetTestAreaFont();
		}

		private void SetTestAreaFont()
		{
			float fontSize = _model.CurrentDefaultFontSize;
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

		private void _keyboardComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			var currentKeyboard = _keyboardComboBox.SelectedItem as KeyboardAdapter;
			if (_model.CurrentKeyboard != currentKeyboard.Id)
			{
				_model.CurrentKeyboard = currentKeyboard.Id;
			}

		}

		private void _testArea_Enter(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
#if!MONO
			_defaultKeyboard = KeyboardController.GetActiveKeyboard();
			_model.ActivateCurrentKeyboard();
#endif
		}

		private void _testArea_Leave(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
#if !MONO
			KeyboardController.ActivateKeyboard(_defaultKeyboard);
#endif
		}

	}
}
