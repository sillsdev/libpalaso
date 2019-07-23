using System;
using System.Windows.Forms;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
{
	public partial class WSIdentifierView : UserControl, ISelectableIdentifierOptions
	{
		private WritingSystemSetupModel _model;

		public WSIdentifierView()
		{
			InitializeComponent();
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			_specialTypeComboBox.SelectedIndexChanged -= specialTypeComboBox_SelectedIndexChanged;
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				foreach (var item in _specialTypeComboBox.Items)
				{
					(item as IDisposable)?.Dispose();
				}
				_specialTypeComboBox.Items.Clear();
			}
			_model = model;
			if (_model != null)
			{
				_model.SelectionChanged += ModelSelectionChanged;
			}
			this.Disposed += OnDisposed;
			if (_model.IsSpecialComboLocked)
			{
				switch (_model.LockedSpecialCombo)
				{
					case WritingSystemSetupModel.SelectionsForSpecialCombo.Ipa:
						AddDetailsControl(new IpaIdentifierView(model));
						break;
					case WritingSystemSetupModel.SelectionsForSpecialCombo.Voice:
						AddDetailsControl(new VoiceIdentifierView(model));
						break;
					case WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant:
						AddDetailsControl(new ScriptRegionVariantView(model));
						break;
					default:
						throw new ApplicationException("Special Combo is locked to an unknown selection.");
				}
			}
			else
			{
				AddDetailsControl(new NothingSpecialView(model));
				AddDetailsControl(new IpaIdentifierView(model));
				AddDetailsControl(new VoiceIdentifierView(model));
				AddDetailsControl(new ScriptRegionVariantView(model));
			}
			_specialTypeComboBox.DisplayMember = "ChoiceName";
			_specialTypeComboBox.SelectedIndex = 0;
			UpdateFromModel();
			_specialTypeComboBox.SelectedIndexChanged += specialTypeComboBox_SelectedIndexChanged;
			if (_model.IsSpecialComboLocked)
			{
				// Update the display to reflect the only combobox selection available.
				specialTypeComboBox_SelectedIndexChanged(null, null);
			}
		}

		private void AddDetailsControl(Control view)
		{
			view.Dock = DockStyle.Fill;
			//leave invisible for now
			_specialTypeComboBox.Items.Add(view);
		}

		private void UpdateFromModel()
		{
			if (_model.CurrentDefinition != null)
			{
				this.Enabled = true;
				_abbreviation.Text = _model.CurrentAbbreviation;
//                _name.Text = _model.CurrentLanguageName;
				//_code.Text=_model.CurrentISO;
				if (!_model.IsSpecialComboLocked)
				{
					UpdateSpecialComboBox();
					_specialTypeComboBox.SelectedIndex = (int)_model.SelectionForSpecialCombo;
				}
			}
			else
			{
				this.Enabled = false;
				_abbreviation.Text = string.Empty;
				_detailPanel.Controls.Clear();
  //              _name.Text = string.Empty;
			   // _code.Text = string.Empty;
				_specialTypeComboBox.SelectedIndex = 0;
			}
		}

		private void UpdateSpecialComboBox()
		{
			if (_model.CurrentIso == WellKnownSubtags.UnlistedLanguage)
			{
				if (_specialTypeComboBox.Items.Count == 4)
				{
					AddDetailsControl(new UnlistedLanguageView(_model));
				}
			}
			else
			{
				if (_specialTypeComboBox.Items.Count == 5)
				{
					_specialTypeComboBox.Items.RemoveAt(4);
				}
			}
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			UpdateFromModel();
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelSelectionChanged;
		}

		private void specialTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_specialTypeComboBox.SelectedItem == null || _model.CurrentDefinition==null)
				return;

			_detailPanel.Controls.Clear();
			_detailPanel.Controls.Add((Control)_specialTypeComboBox.SelectedItem);
			((ISelectableIdentifierOptions)_specialTypeComboBox.SelectedItem).Selected();
		}

		private void OnVisibleChanged(object sender, EventArgs e)
		{
			//UpdateSpecialComboBox();

		}

		public void Selected()
		{
			specialTypeComboBox_SelectedIndexChanged(null, null);
		}

		private void _abbreviation_TextChanged(object sender, EventArgs e)
		{
			var s = _abbreviation.Text.Trim();
			if(s.Length > 0  && s!= _model.CurrentAbbreviation)
			{
				_model.CurrentAbbreviation = s;
			}
		}

		public void MoveDataFromViewToModel()
		{
			((ISelectableIdentifierOptions)_specialTypeComboBox.SelectedItem).MoveDataFromViewToModel();
		}

		public void UnwireBeforeClosing()
		{
			((ISelectableIdentifierOptions)_specialTypeComboBox?.SelectedItem)?.UnwireBeforeClosing();
		}
	}

	public interface ISelectableIdentifierOptions
	{
		void Selected();
		void MoveDataFromViewToModel();
		void UnwireBeforeClosing();
	}
}
