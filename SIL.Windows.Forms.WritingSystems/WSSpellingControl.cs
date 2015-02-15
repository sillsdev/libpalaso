using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class WSSpellingControl : UserControl
	{
		private WritingSystemSetupModel _model;
		private bool _updatingFromModel;
		private bool _changingModel;

		public WSSpellingControl()
		{
			InitializeComponent();
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			if (_model != null)
			{
				model.CurrentItemUpdated -= ModelCurrentItemUpdated;
				model.SelectionChanged -= ModelSelectionChanged;
			}
			_model = model;
			if (_model != null)
			{
				UpdateFromModel();
				model.CurrentItemUpdated += ModelCurrentItemUpdated;
				model.SelectionChanged += ModelSelectionChanged;
			}
			this.Disposed += OnDisposed;
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
			{
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
				_model.SelectionChanged -= ModelSelectionChanged;
			}
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			_spellCheckingIdComboBox.Items.Clear();
			_spellCheckingIdComboBox.Items.AddRange(_model.GetSpellCheckComboBoxItems().ToArray());

			UpdateFromModel();
		}

		private void ModelCurrentItemUpdated(object sender, EventArgs e)
		{
			if (_changingModel)
			{
				return;
			}
			UpdateFromModel();
		}

		private void UpdateFromModel()
		{
			_updatingFromModel = true;
			if (!_model.HasCurrentSelection)
			{
				Enabled = false;
				return;
			}
			Enabled = true;
			_spellCheckingIdComboBox.SelectedItem = _model.CurrentSpellChecker;
			_updatingFromModel = false;
		}

		private void _spellCheckingIdComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_updatingFromModel)
			{
				return;
			}
			try
			{
				_changingModel = true;
				_model.CurrentSpellChecker = (WritingSystemSetupModel.SpellCheckInfo) _spellCheckingIdComboBox.SelectedItem;
			}
			finally
			{
				_changingModel = false;
			}
		}


	}
}
