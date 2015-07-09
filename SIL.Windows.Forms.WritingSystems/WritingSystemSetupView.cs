using System;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.Miscellaneous;
using SIL.Windows.Forms.WritingSystems.WSTree;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class WritingSystemSetupView : UserControl
	{
		private WritingSystemSetupModel _model;
		public event EventHandler UserWantsHelpWithDeletingWritingSystems;
		public event EventHandler UserWantsHelpWithCustomSorting;

		public WritingSystemSetupView()
		{
			InitializeComponent();
			_propertiesTabControl.UserWantsHelpWithCustomSorting += OnHelpWithCustomSorting;
		}

		public WritingSystemSetupView(WritingSystemSetupModel model)
			: this()
		{
			BindToModel(model);
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			_model = model;
			_model.MethodToShowUiToBootstrapNewDefinition= ShowCreateNewWritingSystemDialog;

			_buttonBar.BindToModel(_model);
			_propertiesTabControl.BindToModel(_model);

			var treeModel = new WritingSystemTreeModel(_model) {Suggestor = model.WritingSystemSuggestor};
			_treeView.BindToModel(treeModel);
			_model.SelectionChanged += UpdateHeaders;
			_model.CurrentItemUpdated += UpdateHeaders;
			_model.AskUserWhatToDoWithDataInWritingSystemToBeDeleted += OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted;
			UpdateHeaders(null, null);
		}

		private void OnAskUserWhatToDoWithDataInWritingSystemToBeDeleted(object sender, WhatToDoWithDataInWritingSystemToBeDeletedEventArgs args)
		{
			//If no one is listening for the help button we won't offer it to the user
			bool showHelpButton = UserWantsHelpWithDeletingWritingSystems != null;
			using (var deleteDialog = new DeleteInputSystemDialog(args.WritingSystemIdToDelete, _model.WritingSystemDefinitions, showHelpButton))
			{
				deleteDialog.HelpWithDeletingWritingSystemsButtonClickedEvent += OnHelpWithDeletingWritingSystemsButtonClicked;
				var dialogResult = deleteDialog.ShowDialog();

				if (dialogResult != DialogResult.OK)
				{
					args.WhatToDo = WhatToDos.Nothing;
				}
				else
				{
					switch (deleteDialog.Choice)
					{
						case DeleteInputSystemDialog.Choices.Cancel:
							args.WhatToDo = WhatToDos.Nothing;
							break;
						case DeleteInputSystemDialog.Choices.Merge:
							args.WhatToDo = WhatToDos.Conflate;
							args.WritingSystemIdToConflateWith = deleteDialog.WritingSystemToConflateWith;
							break;
						case DeleteInputSystemDialog.Choices.Delete:
							args.WhatToDo = WhatToDos.Delete;
							break;
					}
				}
			}
		}

		private void OnHelpWithDeletingWritingSystemsButtonClicked(object sender, EventArgs e)
		{
			if (UserWantsHelpWithDeletingWritingSystems != null)
				UserWantsHelpWithDeletingWritingSystems(sender, e);
		}

		private void OnHelpWithCustomSorting(object sender, EventArgs e)
		{
			if (UserWantsHelpWithCustomSorting != null)
				UserWantsHelpWithCustomSorting(sender, e);
		}

		/// <summary>
		/// Use this to set the appropriate kinds of writing systems according to your
		/// application.  For example, is the user of your app likely to want voice? ipa? dialects?
		/// </summary>
		public WritingSystemSuggestor WritingSystemSuggestor
		{
			get { return _model.WritingSystemSuggestor; }
		}

		public int LeftColumnWidth
		{
			get { return splitContainer2.SplitterDistance; }
			set { splitContainer2.SplitterDistance = value; }
		}

		public void SetWritingSystemsInRepo()
		{
			_propertiesTabControl.MoveDataFromViewToModel();
			_model.SetAllPossibleAndRemoveOthers();
		}

		public void UnwireBeforeClosing()
		{
			_propertiesTabControl.UnwireBeforeClosing();
		}

		private void UpdateHeaders(object sender, EventArgs e)
		{
			if (_model.CurrentDefinition == null)
			{
				_ietfLanguageTag.Text = "";
				_languageName.Text = "";
			}
			else
			{
				_ietfLanguageTag.Text = _model.CurrentDefinition.LanguageTag;
				_languageName.Text = _model.CurrentDefinition.ListLabel;
				_languageName.Font = SystemFonts.MessageBoxFont;
				_ietfLanguageTag.Font = SystemFonts.MessageBoxFont;
			}
		}

		private WritingSystemDefinition ShowCreateNewWritingSystemDialog()
		{
			using (var dlg = new LanguageLookupDialog {ShowDesiredLanguageNameField = false})
			{
				dlg.ShowDialog();
				if (dlg.DialogResult != DialogResult.OK)
					return null;

				WaitCursor.Show();
				try
				{
					return _model.WritingSystemFactory.Create(dlg.SelectedLanguage.LanguageTag);
				}
				finally
				{
					WaitCursor.Hide();
				}
			}
		}
	}
}
