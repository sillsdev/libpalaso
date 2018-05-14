using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
{
	public partial class CustomIdentifierView : UserControl
	{
		private readonly WritingSystemSetupModel _model;
		private bool _updatingFromModel;

		public CustomIdentifierView(WritingSystemSetupModel model)
		{
			_model = model;
			InitializeComponent();
			if (model != null)
			{
				model.SelectionChanged += UpdateDisplayFromModel;
			}
			UpdateDisplayFromModel(null,null);
		}

		private void UpdateDisplayFromModel(object sender, EventArgs e)
		{
			if (_model.CurrentDefinition != null)
			{
				_updatingFromModel = true;
				_languageTag.Text = _model.CurrentLanguageTag;
				_updatingFromModel = false;
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SIL.Program.Process.SafeStart("http://www.w3.org/International/articles/language-tags/");
		}

		public string ChoiceName
		{
			get{return "Custom";}
		}

		private void _languageTag_TextChanged(object sender, EventArgs e)
		{
			if(                _updatingFromModel)
				return;
  //          _model.CurrentRFC4646 = _languageTag.Text;
		}

	}
}
