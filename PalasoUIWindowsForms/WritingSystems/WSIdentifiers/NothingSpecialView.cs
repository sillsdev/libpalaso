using System;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	public partial class NothingSpecialView : UserControl, ISelectableIdentifierOptions
	{
		private readonly WritingSystemSetupPM _model;

		public NothingSpecialView(WritingSystemSetupPM model)
		{
			_model = model;
			InitializeComponent();
		}

		public string ChoiceName
		{
			get { return "None"; }
		}

		#region Implementation of ISelectableIdentifierOptions

		public void Selected()
		{
			if (_model != null && _model.CurrentDefinition != null)
			{
				_model.CurrentVariant = string.Empty;
				_model.CurrentRegion = string.Empty;
				_model.CurrentScriptCode = string.Empty;
			}
		}

		#endregion
	}
}
