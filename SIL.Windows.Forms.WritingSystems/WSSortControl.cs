using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.Reporting;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class WSSortControl : UserControl
	{
		private WritingSystemSetupModel _model;
		private readonly Hashtable _sortUsingValueMap;
		private Hashtable _languageOptionMap;
		private bool _changingModel;
		private IKeyboardDefinition _defaultKeyboard;
		private string _defaultFontName;
		private float _defaultFontSize;
		public event EventHandler UserWantsHelpWithCustomSorting;

		public WSSortControl()
		{
			InitializeComponent();
			_sortUsingValueMap = new Hashtable();
			foreach (KeyValuePair<string, string> sortUsingOption in WritingSystemSetupModel.SortUsingOptions)
			{
				int index = _sortUsingComboBox.Items.Add(sortUsingOption.Value);
				_sortUsingValueMap[sortUsingOption.Key] = index;
				_sortUsingValueMap[index] = sortUsingOption.Key;
			}
			_defaultFontName = _sortRulesTextBox.Font.Name;
			_defaultFontSize = _sortRulesTextBox.Font.SizeInPoints;

			// default text for testing the sort rules
			_testSortText.Text = string.Join(Environment.NewLine, "pear", "apple", "orange", "mango", "peach");
		}

		public void BindToModel(WritingSystemSetupModel model)
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
			this.Disposed += OnDisposed;
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
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
			_rulesValidationTimer.Enabled = false;
			if (!_model.HasCurrentSelection)
			{
				_sortrules_panel.Visible = false;
				_languagecombo_panel.Visible = false;
				Enabled = false;
				return;
			}
			Enabled = true;
			LoadLanguageChoicesFromModel();
			if (_sortUsingValueMap.ContainsKey(_model.CurrentCollationRulesType))
			{
				_sortUsingComboBox.SelectedIndex = (int)_sortUsingValueMap[_model.CurrentCollationRulesType];
			}
			else
			{
				_sortUsingComboBox.SelectedIndex = -1;
			}
			SetControlFonts();
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
				_model.CurrentCollationRules = _sortRulesTextBox.Text;
				_rulesValidationTimer.Stop();
				_rulesValidationTimer.Start();
			}
			finally
			{
				_changingModel = false;
			}
		}

		private void _sortUsingComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			_sortrules_panel.Visible = false;
			_languagecombo_panel.Visible = false;
			if (_sortUsingComboBox.SelectedIndex == -1)
			{
				return;
			}
			string newValue = (string)_sortUsingValueMap[_sortUsingComboBox.SelectedIndex];
			_changingModel = true;
			try
			{
				_model.CurrentCollationRulesType = newValue;
			}
			finally
			{
				_changingModel = false;
			}
			if (newValue == "OtherLanguage")
			{
				_sortrules_panel.Visible = true;
				_languagecombo_panel.Visible = true;
				_rulesValidationTimer.Enabled = false;
				if (_languageOptionMap.ContainsKey(_model.CurrentCollationRules))
				{
					_languageComboBox.SelectedIndex = (int)_languageOptionMap[_model.CurrentCollationRules];
				}
			}
			else if (newValue == "CustomSimple" || newValue == "CustomIcu")
			{
				_sortrules_panel.Visible = true;
				_sortRulesTextBox.Text = _model.CurrentCollationRules;
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
				_model.CurrentCollationRules = newValue;
			}
			finally
			{
				_changingModel = false;
			}
		}

		private void _testSortButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (ValidateSortRules())
				{
					_testSortResult.Text = _model.TestSort(_testSortText.Text);
				}
			}
			catch (ApplicationException ex)
			{
				ErrorReport.NotifyUserOfProblem("Unable to sort test text: {0}", ex.Message);
			}
		}

		private void SetControlFonts()
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
			Font customFont = new Font(fontName, fontSize);
			_sortRulesTextBox.Font = customFont;
			// We are not setting the RightToLeft property for the sort rules because the ICU syntax is inherently left-to-right.
			_testSortText.Font = customFont;
			_testSortText.RightToLeft = _model.CurrentRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
			_testSortResult.Font = customFont;
			_testSortResult.RightToLeft = _model.CurrentRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
		}

		private void TextControl_Enter(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			_defaultKeyboard = Keyboard.Controller.ActiveKeyboard;
			_model.ActivateCurrentKeyboard();
		}

		private void TextControl_Leave(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			_defaultKeyboard.Activate();
			if (_rulesValidationTimer.Enabled)
			{
				_rulesValidationTimer.Enabled = false;
				ValidateSortRules();
			}
		}

		private void _rulesValidationTimer_Tick(object sender, EventArgs e)
		{
			_rulesValidationTimer.Enabled = false;
			ValidateSortRules();
		}

		private bool ValidateSortRules()
		{
			string message;
			const string prefixToMessage = "SORT RULES WILL NOT BE SAVED\r\n";
			if (!_model.ValidateCurrentSortRules(out message))
			{
				_testSortResult.Text = prefixToMessage + (message ?? String.Empty);
				_testSortResult.ForeColor = Color.Red;
				return false;
			}
			if (_testSortResult.Text.StartsWith(prefixToMessage))
			{
				_testSortResult.Text = String.Empty;
				_testSortResult.ForeColor = Color.Black;
			}
			return true;
		}

		private void OnHelpLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (UserWantsHelpWithCustomSorting != null)
				UserWantsHelpWithCustomSorting(sender, e);
		}
	}
}
