using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSSortControl : UserControl
	{
		private SetupPM _model;
		private readonly Hashtable _sortUsingValueMap;
		private Hashtable _languageOptionMap;
		private bool _changingModel;

		public WSSortControl()
		{
			InitializeComponent();
			_sortUsingValueMap = new Hashtable();
			foreach (KeyValuePair<string, string> sortUsingOption in SetupPM.SortUsingOptions)
			{
				int index = _sortUsingComboBox.Items.Add(sortUsingOption.Value);
				_sortUsingValueMap[sortUsingOption.Key] = index;
				_sortUsingValueMap[index] = sortUsingOption.Key;
			}
		}

		public void BindToModel(SetupPM model)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
			}
			_model = model;
			if (_model != null)
			{
				UpdateFromModel();
				_model.SelectionChanged += ModelSelectionChanged;
				_model.CurrentItemUpdated += ModelCurrentItemUpdated;
			}
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
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
			if (!_model.HasCurrentSelection)
			{
				_sortPanelCustomText.Visible = false;
				_sortPanelOtherLanguage.Visible = false;
				Enabled = false;
				return;
			}
			Enabled = true;
			LoadLanguageChoicesFromModel();
			if (_sortUsingValueMap.ContainsKey(_model.CurrentSortUsing))
			{
				_sortUsingComboBox.SelectedIndex = (int)_sortUsingValueMap[_model.CurrentSortUsing];
			}
			else
			{
				_sortUsingComboBox.SelectedIndex = -1;
			}
		}

		private void LoadLanguageChoicesFromModel()
		{
			_languageComboBox.Items.Clear();
			_languageOptionMap = new Hashtable();
			foreach (KeyValuePair<string, string> languageOption in _model.SortLanguageOptions)
			{
				int index = _languageComboBox.Items.Add(languageOption.Value);
				_languageOptionMap[index] = languageOption.Key;
				_languageOptionMap[languageOption.Key] = index;
			}
			_sortUsingComboBox.SelectedIndex = -1;
		}

		private void _sortRulesTextBox_TextChanged(object sender, EventArgs e)
		{
			_changingModel = true;
			try
			{
				_model.CurrentSortRules = _sortRulesTextBox.Text;
			}
			finally
			{
				_changingModel = false;
			}
		}

		private void _sortUsingComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			_sortPanelCustomText.Visible = false;
			_sortPanelOtherLanguage.Visible = false;
			if (_sortUsingComboBox.SelectedIndex == -1)
			{
				return;
			}
			string newValue = (string) _sortUsingValueMap[_sortUsingComboBox.SelectedIndex];
			if (newValue == "OtherLanguage")
			{
				_sortPanelOtherLanguage.Visible = true;
				if (_languageOptionMap.ContainsKey(_model.CurrentSortRules))
				{
					_languageComboBox.SelectedIndex = (int)_languageOptionMap[_model.CurrentSortRules];
				}
			}
			else
			{
				_sortPanelCustomText.Visible = true;
				_sortRulesTextBox.Text = _model.CurrentSortRules;
			}
			_changingModel = true;
			try
			{
				_model.CurrentSortUsing = newValue;
			}
			finally
			{
				_changingModel = false;
			}
		}

		private void _languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_languageComboBox.SelectedIndex == -1)
			{
				return;
			}
			string newValue = (string) _languageOptionMap[_languageComboBox.SelectedIndex];
			_changingModel = true;
			try
			{
				_model.CurrentSortRules = newValue;
			}
			finally
			{
				_changingModel = false;
			}
		}

		private void _testSortButton_Click(object sender, EventArgs e)
		{
		}
	}
}
