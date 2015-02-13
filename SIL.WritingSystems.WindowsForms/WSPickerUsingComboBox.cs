using System;
using System.Windows.Forms;
using SIL.Code;

namespace SIL.WritingSystems.WindowsForms
{
	public partial class WSPickerUsingComboBox : ComboBox
	{
		WritingSystemSetupModel _model;
		private bool _changingSelection;

		public event EventHandler SelectedComboIndexChanged;

		/// <summary>
		/// This one is for supporting the designer, and must be followed by a call to BindToModel
		/// </summary>
		public WSPickerUsingComboBox()
		{
			InitializeComponent();
			SelectedIndexChanged += ComboSelectionChanged;
		}

		public WSPickerUsingComboBox(WritingSystemSetupModel model):this()
		{
		   BindToModel(model);
		}

		/// <summary>
		/// Call this explicitly if using the constructor which does not set the model
		/// </summary>
		/// <param name="model"></param>
		public void BindToModel(WritingSystemSetupModel model)
		{
			Guard.AgainstNull(model,"model");
			_model = model;
			//in case this is called twice, don't double subscribe
			_model.ItemAddedOrDeleted -= ModelItemAddedOrDeleted;
			_model.ListColumnsChanged -= ModelListColumnsChanged;
			_model.SelectionChanged -= ModelSelectionChanged;
			_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
			this.Disposed -= OnDisposed;

			ReloadItems();
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
			ReloadItems();
		}

		private void ModelListColumnsChanged(object sender, EventArgs e)
		{
			ReloadItems();
		}


		private void ReloadItems()
		{
			var previous = SelectedIndex;
			_changingSelection = true;
			try
			{
				Items.Clear();
				foreach (string[] item in _model.WritingSystemListItems)
				{
					Items.Add(item[0]);//hack
				}
				if(previous < Items.Count)
					SelectedIndex = previous;
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
			// Some views have more indexes than we do, so do nothing if we can't do anything useful.
			// review: shouldn't these controls have their own model.
			if (_model.CurrentIndex >= Items.Count)
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
				 //this if() was messing us up when the last item was deleted, s.t. value is -1
					//see http://projects.palaso.org/issues/show/265  if (value >= 0)
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
			ReloadItems();

		}
	}
}
