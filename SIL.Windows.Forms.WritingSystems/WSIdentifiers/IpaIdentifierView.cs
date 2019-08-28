using System;
using System.Windows.Forms;
using L10NSharp;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
{
	public partial class IpaIdentifierView : UserControl, ISelectableIdentifierOptions
	{
		private readonly WritingSystemSetupModel _model;

		public IpaIdentifierView(WritingSystemSetupModel model)
		{
			_model = model;
			InitializeComponent();
			_purposeComboBox.Items.AddRange(new object[] {
				LocalizationManager.GetString("WSIdentifiers.IpaIdentifierView.Default", "Default", "Choice for most users"),
				LocalizationManager.GetString("WSIdentifiers.IpaIdentifierView.Etic", "Etic (raw phonetic transcription)",
					"Only translate text inside of the parentheses ()"),
				LocalizationManager.GetString("WSIdentifiers.IpaIdentifierView.Emic",
					"Emic (uses phonology of the language)", "Only translate text inside of the parentheses ()")
			});

			if (model != null)
			{
				model.SelectionChanged += UpdateDisplayFromModel;
			}
		}

		private void UpdateDisplayFromModel(object sender, EventArgs e)
		{
			if (_model.CurrentDefinition != null)
			{
				//minus one because we skip the "not ipa" choice
				_purposeComboBox.SelectedIndex =(int) _model.CurrentIpaStatus-1;
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SIL.Program.Process.SafeStart("http://en.wikipedia.org/wiki/International_Phonetic_Alphabet");
		}

		public string ChoiceName
		{
			get { return "IPA Transcription"; }
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			_model.CurrentIpaStatus =1+(IpaStatusChoices) _purposeComboBox.SelectedIndex;
		}

		#region Implementation of ISelectableIdentifierOptions

		public void Selected()
		{
			if (_model != null)
			{
				_model.IdentifierIpaSelected();
			}
			UpdateDisplayFromModel(null, null);

		}

		public void MoveDataFromViewToModel()
		{
			//do nothing
		}

		public void UnwireBeforeClosing()
		{
			//do nothing
		}

		#endregion
	}
}
