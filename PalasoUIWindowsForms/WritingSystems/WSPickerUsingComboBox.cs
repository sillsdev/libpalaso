using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WritingSystems;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSPickerUsingComboBox : ComboBox
	{
		WritingSystemSetupPM _model;
		private bool _changingSelection;

		public event EventHandler SelectedComboIndexChanged;

		public WSPickerUsingComboBox()
		{
			InitializeComponent();
			SelectedIndexChanged += ComboSelectionChanged;
		}

		public void BindToModel(WritingSystemSetupPM model)
		{
			Debug.Assert(model != null);
			if (_model != null)
			{
				_model.ItemAddedOrDeleted -= ModelItemAddedOrDeleted;
				_model.ListColumnsChanged -= ModelListColumnsChanged;
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
			}
			_model = model;
			RefreshListView();
			_model.ItemAddedOrDeleted += ModelItemAddedOrDeleted;
			_model.ListColumnsChanged += ModelListColumnsChanged;
			_model.SelectionChanged += ModelSelectionChanged;
			_model.CurrentItemUpdated += ModelCurrentItemUpdated;
		}

		private void ModelItemAddedOrDeleted(object sender, EventArgs e)
		{
			RefreshListViewItems();
		}

		private void ModelListColumnsChanged(object sender, EventArgs e)
		{
			RefreshListView();
		}

		private void RefreshListView()
		{
			RefreshListViewItems();
		}

		private void RefreshListViewItems()
		{
			_changingSelection = true;
			try
			{
				Items.Clear();
				foreach (string[] item in _model.WritingSystemListItems)
				{
					Items.Add(item[0]);//hack
				}
			}
			finally
			{
				_changingSelection = false;
			}
			ModelSelectionChanged(this, new EventArgs());
		}


	  private void ComboSelectionChanged(object sender, EventArgs e)
		{
			// ensures that the SelectedIndexChanged event is only raised once
			// per actual change of the index
			if (_changingSelection)
				return;
			if (SelectedIndex == _model.CurrentIndex)
				return;
			_changingSelection = true;
			try
			{
				_model.CurrentIndex = SelectedIndex;
				OnSelectedIndexChanged();
			}
			finally
			{
				_changingSelection = false;
			}
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			if (_changingSelection)
				return;
			if (SelectedIndex == _model.CurrentIndex)
				return;
			_changingSelection = true;
			try
			{
				SelectedIndex = _model.CurrentIndex;
				OnSelectedIndexChanged();
			}
			finally
			{
				_changingSelection = false;
			}
		}

		private void OnSelectedIndexChanged()
		{
			if (SelectedComboIndexChanged != null)
			{
				SelectedComboIndexChanged(this, new EventArgs());
			}
		}

		public override int SelectedIndex
		{
			get
			{
				return base.SelectedIndex;
			}
			set
			{
				if (value < -1 || value >= Items.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (base.SelectedIndex == value)
				{
					return;
				}
				_changingSelection = true;
				try
				{
					if (value >= 0)
					{
						base.SelectedIndex=value;
					}
					_model.CurrentIndex = base.SelectedIndex;
					OnSelectedIndexChanged();
				}
				finally
				{
					_changingSelection = false;
				}
			}
		}

		private void ModelCurrentItemUpdated(object sender, EventArgs e)
		{

		}
	}
}
