using System;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{

	public partial class IpaIdentifierView : UserControl, ISelectableIdentifierOptions
	{
		private readonly WritingSystemSetupModel _model;
		private bool _updatingFromModel;

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
				_updatingFromModel = true;

				//minus one because we skip the "not ipa" choice
				comboBox1.SelectedIndex =(int) _model.CurrentIpaStatus-1;
				_updatingFromModel = false;
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://en.wikipedia.org/wiki/International_Phonetic_Alphabet");
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
			if (_model != null && _model.CurrentDefinition != null)
			{
				_model.CurrentRegion = string.Empty;
				_model.CurrentScriptCode = string.Empty;
				//if we're here, the user wants some kind of ipa
				if (_model.CurrentIpaStatus == IpaStatusChoices.NotIpa)
				{
					_model.CurrentIpaStatus = IpaStatusChoices.Ipa;
				}
			}
			UpdateDisplayFromModel(null, null);

		}

		#endregion
	}
}
