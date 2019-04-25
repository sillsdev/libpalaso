using System.Windows.Forms;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
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
			if (_model != null)
			{
				_model.IdentifierVoiceSelected();
			}
		}

		public void MoveDataFromViewToModel()
		{
			//do nothing
		}

		public void UnwireBeforeClosing()
		{
			//Do nothing
		}

		public string ChoiceName
		{
			get { return "Voice"; }
		}
	}


}
