using System;
using System.Windows.Forms;
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
				comboBox1.SelectedIndex =(int) _model.CurrentIpaStatus-1;
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
			_model.CurrentIpaStatus =1+(IpaStatusChoices) comboBox1.SelectedIndex;
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
