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
	public partial class WSIdentifierView : UserControl
	{
		private WritingSystemSetupModel _model;

		public WSIdentifierView()
		{
			InitializeComponent();
		}
		public void BindToModel(WritingSystemSetupModel model)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
			}
			_model = model;
			if (_model != null)
			{
				_model.SelectionChanged += ModelSelectionChanged;
			}
//            UpdateProxyFromModel();
			this.Disposed += OnDisposed;

			AddDetailsControl(new NothingSpecialView(model));
			AddDetailsControl(new IpaIdentifierView(model));
			AddDetailsControl(new VoiceIdentifierView(model));
			AddDetailsControl(new ScriptRegionVariantView(model));
			//AddDetailsControl(new CustomIdentifierView(model));
			comboBox1.DisplayMember = "ChoiceName";
			comboBox1.SelectedIndex = 0;
			UpdateFromModel();
		}

		private void AddDetailsControl(Control view)
		{
			view.Dock = DockStyle.Fill;
			//leave invisible for now
			comboBox1.Items.Add(view);
		}

		private void UpdateFromModel()
		{
			if (_model.CurrentDefinition != null)
			{
				this.Enabled = true;
				_abbreviation.Text = _model.CurrentAbbreviation;
//                _name.Text = _model.CurrentLanguageName;
				//_code.Text=_model.CurrentISO;
				comboBox1.SelectedIndex = (int)_model.SelectionForSpecialCombo;
			}
			else
			{
				this.Enabled = false;
				_abbreviation.Text = string.Empty;
  //              _name.Text = string.Empty;
			   // _code.Text = string.Empty;
				comboBox1.SelectedIndex = 0;
			}
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			UpdateFromModel();
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelSelectionChanged;
		}

		private void _isoSearchButton_Click(object sender, EventArgs e)
		{
			var dlg = new LookupISOCodeDialog();
			dlg.ShowDialog();
			if (dlg.DialogResult == DialogResult.OK)
			{
				_model.CurrentISO = dlg.ISOCode;
				_model.CurrentLanguageName = dlg.ISOCodeAndName.Name;
				_model.CurrentAbbreviation = dlg.ISOCode;
				UpdateFromModel();
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBox1.SelectedItem == null || _model.CurrentDefinition==null)
				return;
//
//            if(_detailPanel.Controls.Count>1)
//            {
//                _detailPanel.Controls[0].Dispose();
//                _detailPanel.Controls.Clear();
//            }
			_detailPanel.Controls.Clear();
			_detailPanel.Controls.Add((Control)comboBox1.SelectedItem);
			((ISelectableIdentifierOptions)comboBox1.SelectedItem).Selected();

//            if (_model.CurrentDefinition != null)
//            {
//                _model.CurrentIsVoice = comboBox1.SelectedItem is VoiceIdentifierView;
//            }

		}
	}

	public interface ISelectableIdentifierOptions
	{
		void Selected();
	}
}
