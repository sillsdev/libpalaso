using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	public partial class VoiceIdentifierView : UserControl, ISelectableIdentifierOptions
	{
		private readonly WritingSystemSetupPM _model;

		public VoiceIdentifierView(WritingSystemSetupPM model)
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
			}
		}
		public string ChoiceName
		{
			get { return "Voice"; }
		}
	}


}
