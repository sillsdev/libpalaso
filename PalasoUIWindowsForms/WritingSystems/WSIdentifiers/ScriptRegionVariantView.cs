using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	public partial class ScriptRegionVariantView : UserControl
	{
		private bool _updatingFromModel;
		private readonly WritingSystemSetupPM _model;

		public ScriptRegionVariantView(WritingSystemSetupPM model)
		{
			_model = model;
			InitializeComponent();
			if (model != null)
			{
				model.SelectionChanged += UpdateDisplayFromModel;
			}
			UpdateDisplayFromModel(null, null);
		}

		private void UpdateDisplayFromModel(object sender, EventArgs e)
		{
			if (_model.CurrentDefinition != null)
			{
				_updatingFromModel = true;
				_region.Text= _model.CurrentRegion;
				_variant.Text=_model.CurrentVariant;
				_scriptCombo.SelectedItem = _model.CurrentScriptOption;
				_updatingFromModel = false;
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{

		}

		public string ChoiceName
		{
			get { return "Script/Variant/Region"; }
		}

		private void _variant_TextChanged(object sender, EventArgs e)
		{
			if (_updatingFromModel)
				return;
			_model.CurrentVariant=_variant.Text;
		}

		private void _region_TextChanged(object sender, EventArgs e)
		{
			if (_updatingFromModel)
				return;
			_model.CurrentRegion = _region.Text;
		}

		private void _scriptCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_updatingFromModel)
				return;
			if (_scriptCombo.SelectedItem == null)
			{
				_model.CurrentScriptCode = string.Empty;
			}
			else
			{
				_model.CurrentScriptCode = ((ScriptOption) _scriptCombo.SelectedItem).Code;
			}
		}

	}
}
