using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WritingSystemSetupView : UserControl
	{
		private WritingSystemSetupModel _model;
		public event EventHandler UserWantsHelpWithDeletingWritingSystems;

		public WritingSystemSetupView()
		{
			InitializeComponent();
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

			var treeModel = new WritingSystemTreeModel(_model);
			treeModel.Suggestor = model.WritingSystemSuggestor;
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
				deleteDialog.HelpWithDeletingWritingSystemsButtonClickedEvent +=
					OnHelpWithDeletingWritingSystemsButtonClicked;
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
			if(UserWantsHelpWithDeletingWritingSystems != null)
			{
				UserWantsHelpWithDeletingWritingSystems(sender, e);
			}
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
			if(_model.CurrentDefinition ==null)
			{
				_rfc4646.Text = "";
				_languageName.Text = "";
			}
			else
			{
				_rfc4646.Text = _model.CurrentDefinition.Bcp47Tag;
				_languageName.Text = _model.CurrentDefinition.ListLabel;
				_languageName.Font = SystemFonts.MessageBoxFont;
				_rfc4646.Font = SystemFonts.MessageBoxFont;
			}
		}

		private static WritingSystemDefinition ShowCreateNewWritingSystemDialog()
		{
			using (var dlg = new LookupISOCodeDialog() { ShowDesiredLanguageNameField =false})
			{
				dlg.ShowDialog();
				if (dlg.DialogResult != DialogResult.OK)
					return null;
				var variant = String.Empty;
				if (dlg.SelectedLanguage.Code == WellKnownSubTags.Unlisted.Language)
				{
					variant = "x-" + "Unlisted";
				}
				return new WritingSystemDefinition(dlg.SelectedLanguage.Code, string.Empty, string.Empty, variant, dlg.SelectedLanguage.Code, false);
			}
		}

		private void _propertiesTabControl_Load(object sender, EventArgs e)
		{

		}
	}
}
