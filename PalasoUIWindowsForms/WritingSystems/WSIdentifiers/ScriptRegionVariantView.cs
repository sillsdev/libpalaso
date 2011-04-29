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
	public partial class ScriptRegionVariantView : UserControl, ISelectableIdentifierOptions
	{
		private bool _updatingFromModel;
		private readonly WritingSystemSetupModel _model;

		public ScriptRegionVariantView(WritingSystemSetupModel model)
		{
			_model = model;
			InitializeComponent();
			if (model != null)
			{
				model.SelectionChanged += UpdateDisplayFromModel;
			}
			_scriptCombo.Items.AddRange(StandardTags.ValidIso15924Scripts.ToArray());
			_scriptCombo.DisplayMember = "Label";
			_regionCombo.Items.AddRange(StandardTags.ValidIso3166Regions.ToArray());
			_regionCombo.DisplayMember = "Description";
		}

		private void UpdateDisplayFromModel(object sender, EventArgs e)
		{
			if (_model.CurrentDefinition != null)
			{
				_updatingFromModel = true;
				_regionCombo.SelectedItem = _model.CurrentRegion;
				_variant.Text=_model.CurrentVariant;
				_scriptCombo.SelectedItem = _model.CurrentIso15924Script;
				_updatingFromModel = false;
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{

		}

		public string ChoiceName
		{
			get { return "Script/Region/Variant"; }
		}

		private void Variant_OnTextChanged(object sender, EventArgs e)
		{
			if (_updatingFromModel)
				return;
			_model.CurrentVariant=_variant.Text;
			// update the display, since we may have changed the variant text
			UpdateDisplayFromModel(null, null);
		}

		private void ScriptCombo_OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_updatingFromModel)
				return;
			if (_scriptCombo.SelectedItem == null)
			{
				_model.CurrentScriptCode = string.Empty;
			}
			else
			{
				string originalCode = _model.CurrentScriptCode;
				string originalLabel = ((Iso15924Script) _scriptCombo.SelectedItem).Label;
				try
				{
					_model.CurrentScriptCode = ((Iso15924Script)_scriptCombo.SelectedItem).Code;
				}
				catch (ArgumentException error)
				{
					if (originalCode == "Zxxx")
					{
						MessageBox.Show("This Voice writing system's script cannot be changed.");
					}
					else
					{
						MessageBox.Show(error.Message);
					}
					_model.CurrentScriptCode = originalCode;
					UpdateDisplayFromModel(null, null);
				}

			}
		}

		private void RegionCombo_OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_updatingFromModel)
				return;
			if (_regionCombo.SelectedItem == null)
			{
				_model.CurrentRegion = string.Empty;
			}
			else
			{
				_model.CurrentRegion = ((StandardTags.IanaSubtag)_regionCombo.SelectedItem).Subtag;
			}
		}

		#region Implementation of ISelectableIdentifierOptions

		public void Selected()
		{
			if (_model != null && _model.CurrentDefinition != null)
			{
				_model.CurrentVariant = _model.CurrentDefinition.Variant;
				_model.CurrentRegion = _model.CurrentDefinition.Region;
				_model.CurrentScriptCode = _model.CurrentDefinition.Script;
				_model.CurrentIsVoice = _model.CurrentDefinition.IsVoice;
				_model.CurrentIpaStatus = _model.CurrentDefinition.IpaStatus;
			}
			UpdateDisplayFromModel(null, null);
		}

		#endregion
	}
}
