using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	public partial class UnlistedLanguageView : UserControl, ISelectableIdentifierOptions
	{
		private readonly WritingSystemSetupModel _model;
		private bool _updatingFromModel;
		private bool _resettingFields;
		public const string DefaultName = "Unlisted Language";
		public const string DefaultCode = "v";
		public const string DefaultAbbreviation = DefaultCode;

		public UnlistedLanguageView(WritingSystemSetupModel model)
		{
			_model = model;
			InitializeComponent();
			if (model != null)
			{
				model.SelectionChanged += UpdateDisplayFromModel;
			}
			//Selected();
		}

		private void UpdateDisplayFromModel(object sender, EventArgs e)
		{
			if (_model.CurrentDefinition != null)
			{
				string name;
				_updatingFromModel = true;
				nonStandardLanguageCode.Text = CodeFromPrivateUse();
				nonStandardLanguageName.Text = _model.CurrentLanguageName;
				_updatingFromModel = false;
			}
		}

		private string CodeFromPrivateUse()
		{
			string code = DefaultCode;
			if (_model.CurrentVariant.Contains("x-"))
			{
				string[] tokens = _model.CurrentVariant.Substring(_model.CurrentVariant.IndexOf("x-") + 2).Split('-');
				if (tokens.Length > 0)
				{
					code = tokens[0];
				}
			}
			return code;
		}


		//private void MakeFieldsFromPrivateUse(out string code, out string name)
		//{
		//    code = DefaultCode;
		//    name = DefaultName;
		//    if (_model.CurrentVariant.Contains("x-"))
		//    {
		//        string[] tokens = _model.CurrentVariant.Substring(_model.CurrentVariant.IndexOf("x-") + 2).Split('-');
		//        if (tokens.Length > 0)
		//        {
		//            code = tokens[0];
		//            if (tokens.Length > 1)
		//            {
		//                name = tokens[1];
		//            }
		//        }
		//    }
		//}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.w3.org/International/articles/language-tags/");
		}

		public string ChoiceName
		{
			get{return "Unlisted Language Details";}
		}

		private void field_OnLeave(object sender, EventArgs e)
		{
			if(_updatingFromModel || _resettingFields)
				return;
			ResetFieldsIfNecessary();
			_model.CurrentVariant = "x-" + nonStandardLanguageCode.Text;
			_model.CurrentLanguageName = nonStandardLanguageName.Text;
		}

		private void ResetFieldsIfNecessary()
		{
			_resettingFields = true;
			string code = nonStandardLanguageCode.Text;
			if (string.IsNullOrEmpty(nonStandardLanguageName.Text))
			{
				nonStandardLanguageName.Text = DefaultName;
			}
			if (string.IsNullOrEmpty(code) || code == "audio" || code == "etic" || code == "emic")
			{
				nonStandardLanguageCode.Text = DefaultCode;
			}
			_resettingFields = false;
		}

		public void Selected()
		{
			field_OnLeave(null, null);
			UpdateDisplayFromModel(null, null);
		}

		private void betterLabel1_TextChanged(object sender, EventArgs e)
		{

		}
	}
}
