using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	public partial class VoiceIdentifierView : UserControl, ISelectableIdentifierOptions
	{
		private readonly WritingSystemSetupModel _model;

		public VoiceIdentifierView(WritingSystemSetupModel model)
		{
			_model = model;
			InitializeComponent();


		}
		public void Selected()
		{
			if (_model != null && _model.CurrentDefinition != null)
			{
				_model.CurrentVariant = string.Empty;
				_model.CurrentRegion = string.Empty;
				_model.CurrentScriptCode = string.Empty;
				_model.CurrentIsVoice = true;
				_model.CurrentIpaStatus = IpaStatusChoices.NotIpa;
			}
		}
		public string ChoiceName
		{
			get { return "Voice"; }
		}
	}


}
