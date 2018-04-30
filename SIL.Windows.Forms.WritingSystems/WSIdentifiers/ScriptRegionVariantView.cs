using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
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
				_warningLabel.Visible = model.DisplayScriptRegionVariantWarningLabel;
			}

			_scriptCombo.Items.Add(new ScriptSubtag("blank")); // add a blank item at the top of the list
			_scriptCombo.Items.AddRange(StandardSubtags.RegisteredScripts.Cast<object>().ToArray());
			_scriptCombo.DisplayMember = "Name";
			_regionCombo.Items.Add(new RegionSubtag("blank"));  // add a blank item at the top of the list
			_regionCombo.Items.AddRange(StandardSubtags.RegisteredRegions.Cast<object>().ToArray());
			_regionCombo.DisplayMember = "Name";
		}

		private void UpdateDisplayFromModel(object sender, EventArgs e)
		{
			if (_model.CurrentDefinition != null)
			{
				_updatingFromModel = true;
				_variant.Text=_model.CurrentVariant;
				_regionCombo.SelectedItem = _model.CurrentRegionTag;
				_scriptCombo.SelectedItem = _model.CurrentIso15924Script;
				_updatingFromModel = false;
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.w3.org/International/questions/qa-choosing-language-tags");
		}

		public string ChoiceName
		{
			get { return "Script/Region/Variant"; }
		}

		public void MoveDataFromViewToModel()
		{
			UpdateModelFromView();
		}

		public void UnwireBeforeClosing()
		{
			_variant.Leave -= Variant_OnLeave;
		}

		private void Variant_OnLeave(object sender, EventArgs e)
		{
			UpdateModelFromView();
		}

		private void UpdateModelFromView()
		{
			if (_updatingFromModel)
				return;
			_model.CurrentVariant = _variant.Text;
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
				try
				{
					var scriptSubtag = (ScriptSubtag) _scriptCombo.SelectedItem;
					_model.CurrentScriptCode = scriptSubtag.Code == "blank" ? string.Empty : scriptSubtag.Code;
				}
				catch (ArgumentException error)
				{
					if (originalCode == "Zxxx")
					{
						MessageBox.Show("This Voice input system's script cannot be changed.");
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
				var regionSubtag = (RegionSubtag) _regionCombo.SelectedItem;
				_model.CurrentRegion = regionSubtag.Code == "blank" ? string.Empty : regionSubtag.Code;
			}
		}

		#region Implementation of ISelectableIdentifierOptions

		public void Selected()
		{
			if (_model != null)
			{
				_model.IdentifierScriptRegionVariantSelected();
			}
			UpdateDisplayFromModel(null, null);
		}

		#endregion
	}
}
