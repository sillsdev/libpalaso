using System;
using System.Windows.Forms;

namespace SIL.WritingSystems.WindowsForms.WSIdentifiers
{
	public partial class NothingSpecialView : UserControl, ISelectableIdentifierOptions
	{
		private readonly WritingSystemSetupModel _model;

		public NothingSpecialView(WritingSystemSetupModel model)
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
			if (_model != null)
			{
				_model.IdentifierNothingSelected();
			}
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
