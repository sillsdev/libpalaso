﻿using System;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	public partial class UnlistedLanguageView : UserControl, ISelectableIdentifierOptions
	{
		private readonly WritingSystemSetupModel _model;
		private bool _updatingFromModel;
		//public const string DefaultName = "~Unlisted Language";
		//public const string DefaultCode = "v";
		//public const string DefaultAbbreviation = DefaultCode;

		public UnlistedLanguageView(WritingSystemSetupModel model)
		{
			_model = model;
			InitializeComponent();
			if (model != null)
			{
				model.SelectionChanged += UpdateDisplayFromModel;
			}
			UpdateDisplayFromModel(null, null);
			//Selected();
		}

		private void UpdateDisplayFromModel(object sender, EventArgs e)
		{
			if (_model.CurrentDefinition != null)
			{
				_updatingFromModel = true;
				nonStandardLanguageName.Text = _model.CurrentLanguageName;
				_updatingFromModel = false;
			}
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

		private void OnLeave_NonStandardLanguageName(object sender, EventArgs e)
		{
			MoveDataFromViewToModel();
		}

		public void Selected()
		{
			_model.IdentifierUnlistedLanguageSelected();
			UpdateDisplayFromModel(null, null);
		}

		public void MoveDataFromViewToModel()
		{
			if (_updatingFromModel)
				return;
			//ResetFieldsIfNecessary();
			//_model.SetCurrentVariant = "x-" + nonStandardLanguageCode.Text;
			// NO! We don't want to change the language code (variant section) just because
			// the language name changes.  The non-standard language code should be invariant
			// once it is set.  Changing it can cause major problems in sharing data with
			// other people.
			//_model.SetCurrentVariantFromUnlistedLanguageName(nonStandardLanguageName.Text);
			_model.CurrentLanguageName = nonStandardLanguageName.Text;
		}

		public void UnwireBeforeClosing()
		{
			nonStandardLanguageName.Leave -= OnLeave_NonStandardLanguageName;
		}

		private void betterLabel1_TextChanged(object sender, EventArgs e)
		{

		}
	}
}
