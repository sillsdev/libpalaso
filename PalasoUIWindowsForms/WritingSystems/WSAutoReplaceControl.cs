using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSAutoReplaceControl : UserControl
	{
		private SetupPM _model;
		private bool _changingModel;

		public WSAutoReplaceControl()
		{
			InitializeComponent();
		}

		public void BindToModel(SetupPM model)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
			}
			_model = model;
			Enabled = false;
			if (_model != null)
			{
				UpdateFromModel();
				_model.SelectionChanged += ModelSelectionChanged;
				_model.CurrentItemUpdated += ModelCurrentItemUpdated;
			}
		}

		private void UpdateFromModel()
		{
			if (!_model.HasCurrentSelection)
			{
				Enabled = false;
				return;
			}
			Enabled = true;
			_autoReplaceRules.Text = _model.CurrentAutoReplaceRules;
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			UpdateFromModel();
		}

		private void ModelCurrentItemUpdated(object sender, EventArgs e)
		{
			if (!_changingModel)
			{
				UpdateFromModel();
			}
		}

		private void _autoReplaceRules_TextChanged(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			_changingModel = true;
			try
			{
				_model.CurrentAutoReplaceRules = _autoReplaceRules.Text;
			}
			finally
			{
				_changingModel = false;
			}
		}
	}
}
