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
			if (_model != null)
			{
				_model.IdentifierVoiceSelected();
			}
		}
		public string ChoiceName
		{
			get { return "Voice"; }
		}
	}


}
