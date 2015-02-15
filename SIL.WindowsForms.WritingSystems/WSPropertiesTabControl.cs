using System;
using System.Diagnostics;
using System.Windows.Forms;
using SIL.WindowsForms.Keyboarding;

namespace SIL.WindowsForms.WritingSystems
{
	public partial class WSPropertiesTabControl : UserControl
	{
		private WritingSystemSetupModel _model;
		public event EventHandler UserWantsHelpWithCustomSorting;

		public WSPropertiesTabControl()
		{
			InitializeComponent();
			_sortControl.UserWantsHelpWithCustomSorting += OnHelpWithCustomSorting;

			if (!KeyboardController.IsInitialized)
			{
				// Client applications should call KeyboardController.Initialize(), otherwise
				// we can't display anything on the keyboard tab. Therefore we remove that
				// tab.
				Debug.WriteLine("KeyboardController isn't initialized. Removing Keyboard tab.");
				Debug.WriteLine("Please call KeyboardController.Initialize() if you require the keyboarding functionality!");
				_tabControl.Controls.Remove(_keyboardsPage);
			}
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			 if (_model != null)
			{
				_model.SelectionChanged -= ModelChanged;
				_model.CurrentItemUpdated -= ModelChanged;
			}

			_model = model;
			_identifiersControl.BindToModel(_model);
			_fontControl.BindToModel(_model);
			_keyboardControl.BindToModel(_model);
			_sortControl.BindToModel(_model);
			_spellingControl.BindToModel(_model);


			if (_model != null)
			{
				_model.SelectionChanged+= ModelChanged;
				_model.CurrentItemUpdated += ModelChanged;
			}
			this.Disposed += OnDisposed;
		}

		private void ModelChanged(object sender, EventArgs e)
		{
		   if( !_model.CurrentIsVoice &&
				_tabControl.Controls.Contains(_spellingPage))
		   {
			   return;// don't mess if we really don't need a change
		   }

			_tabControl.Controls.Clear();
			this._tabControl.Controls.Add(this._identifiersPage);

			if( !_model.CurrentIsVoice)
			{
				this._tabControl.Controls.Add(this._spellingPage);
				this._tabControl.Controls.Add(this._fontsPage);
				this._tabControl.Controls.Add(this._keyboardsPage);
				this._tabControl.Controls.Add(this._sortingPage);
			}
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelChanged;
		}

		public void MoveDataFromViewToModel()
		{
			_identifiersControl.MoveDataFromViewToModel();
		}

		public void UnwireBeforeClosing()
		{
			_identifiersControl.UnwireBeforeClosing();
		}

		private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_tabControl.SelectedTab == _keyboardsPage)
				_keyboardControl.Focus();
		}

		private void OnHelpWithCustomSorting(object sender, EventArgs e)
		{
			if (UserWantsHelpWithCustomSorting != null)
				UserWantsHelpWithCustomSorting(sender, e);
		}
	}
}
