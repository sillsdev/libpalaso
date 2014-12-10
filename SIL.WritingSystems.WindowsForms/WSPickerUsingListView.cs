using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.WritingSystems.WindowsForms
{
	public partial class WSPickerUsingListView : UserControl
	{
		WritingSystemSetupModel _model;
		private bool _changingSelection;

		public event EventHandler SelectedIndexChanged;

		public WSPickerUsingListView()
		{
			InitializeComponent();
			_listView.SelectedIndexChanged += ListViewSelectionChanged;
		}

		public void BindToModel(WritingSystemSetupModel model)
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
			this.Disposed += OnDisposed;
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
			{
				_model.ItemAddedOrDeleted -= ModelItemAddedOrDeleted;
				_model.ListColumnsChanged -= ModelListColumnsChanged;
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
			}
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
			RefreshListViewColumns();
			RefreshListViewItems();
		}

		private void RefreshListViewItems()
		{
			_changingSelection = true;
			try
			{
				_listView.Clear();
				foreach (string[] item in _model.WritingSystemListItems)
				{
					ListViewItem listViewItem = new ListViewItem(item[0]);
					for (int i = 1; i < item.Length; i++)
					{
						listViewItem.SubItems.Add(item[i]);
					}
					_listView.Items.Add(listViewItem);
				}
			}
			finally
			{
				_changingSelection = false;
			}
			RefreshListViewItemBackgrounds();
			ModelSelectionChanged(this, new EventArgs());
		}

		private void RefreshListViewItemBackgrounds()
		{
			bool[] canSave = _model.WritingSystemListCanSave;
			for (int i=0; i < _model.WritingSystemCount; i++)
			{
				_listView.Items[i].BackColor = canSave[i] ? _listView.BackColor : Color.Orange;
			}
		}

		private void RefreshListViewColumns()
		{
			_listView.Columns.Clear();
			foreach (string columnHeader in _model.WritingSystemListColumns)
			{
				_listView.Columns.Add(columnHeader);
			}
			if (_model.WritingSystemListColumns.Length == 1)
			{
				_listView.HeaderStyle = ColumnHeaderStyle.None;
			}
			else
			{
				_listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
			}
		}

		private void ListViewSelectionChanged(object sender, EventArgs e)
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
				_listView.SelectedIndices.Clear();
				if(_model.CurrentIndex > -1 && _model.CurrentIndex < _listView.Items.Count)
					_listView.SelectedIndices.Add(_model.CurrentIndex);
				OnSelectedIndexChanged();
			}
			finally
			{
				_changingSelection = false;
			}
		}

		private void OnSelectedIndexChanged()
		{
			if (SelectedIndexChanged != null)
			{
				SelectedIndexChanged(this, new EventArgs());
			}
		}

		public int SelectedIndex
		{
			get
			{
				if (_listView.SelectedIndices.Count == 0)
					return -1;
				return _listView.SelectedIndices[0];
			}
			set
			{
				if (value < -1 || value >= _listView.Items.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (SelectedIndex == value)
				{
					return;
				}
				_changingSelection = true;
				try
				{
					_listView.SelectedIndices.Clear();
					if (value >= 0)
					{
						_listView.SelectedIndices.Add(value);
					}
					_model.CurrentIndex = SelectedIndex;
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
			if(SelectedIndex<0)
				return;

			string[] currentItem = _model.WritingSystemListCurrentItem;
			ListViewItem listViewItem = _listView.Items[SelectedIndex];
			listViewItem.Text = currentItem[0];
			for (int i = 2; i < currentItem.Length; i++)
			{
				listViewItem.SubItems[i].Text = currentItem[i];
			}
			RefreshListViewItemBackgrounds();
		}

	}
}
