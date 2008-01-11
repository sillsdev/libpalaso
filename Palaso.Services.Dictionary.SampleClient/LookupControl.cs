using System;
using System.Windows.Forms;
using Palaso.Services.Dictionary.SampleClient;
using Palaso.Services.Dictionary.SampleClient.Properties;

namespace Palaso.Services.Dictionary.SampleClient
{
	public partial class LookupControl : UserControl
	{
		private bool _pauseListChangeDetection;
		private DictionaryAccessor _dictionaryAccessor;

		public LookupControl()
		{
			InitializeComponent();
		}


		internal class WordListItem
		{
			private readonly string _id;
			private readonly string _form;

			public WordListItem(string id, string form)
			{
				_id = id;
				_form = form;
			}

			public string Id
			{
				get { return _id; }
			}

			public override string ToString()
			{
				return _form;
			}
		}

		public DictionaryAccessor DictionaryAccessor
		{
			get { return _dictionaryAccessor; }
			set
			{
				_dictionaryAccessor = value;
				this.Enabled = (_dictionaryAccessor != null);
			}
		}

		private void OnChoicesList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(_pauseListChangeDetection)
				return;

			UpdateDisplay();
			_entryViewer.DocumentText = "";

			WordListItem item = _choicesList.SelectedItem as WordListItem;
			if (item == null)
				return;

			Cursor.Current = Cursors.WaitCursor;
			string html;
			try
			{
				html = _dictionaryAccessor.GetHmtlForEntry(item.Id);
			}
			catch (Exception error)
			{
				Cursor.Current = Cursors.Default;
				MainWindow.Logger.Log(error.Message);
				return;
			}


			_entryViewer.DocumentText = string.Format("<html><body>{0}</body></html>", html);
			if (string.IsNullOrEmpty(html))
			{
				MainWindow.Logger.Log("Got empty reply for html lookup of {0}", item.Id);
			}
			else
			{
				MainWindow.Logger.Log("");
			}
			Cursor.Current = Cursors.Default;
		}

		private void OnFindSimilarButton_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			_entryViewer.DocumentText = string.Empty;
			string[] forms;
			string[] ids;

			try
			{
				_dictionaryAccessor.GetMatchingEntries(Settings.Default.WritingSystemIdForWords, _word.Text,
													   FindMethods.DefaultApproximate, out ids, out forms);
			}
			catch (Exception error)
			{
				Cursor.Current = Cursors.Default;
				MainWindow.Logger.Log(error.Message);
				return;
			}

			_pauseListChangeDetection = true;
			_choicesList.Items.Clear();
			_choicesList.SuspendLayout();
			for (int i = 0; i < ids.Length; i++)
			{
				int index =_choicesList.Items.Add(new WordListItem(ids[i], forms[i]));
				if (forms[i] == _word.Text)
				{
					_choicesList.SelectedIndex = index;
				}
			}
			_choicesList.ResumeLayout(true);
			_pauseListChangeDetection = false;
			OnChoicesList_SelectedIndexChanged(this, null);
			Cursor.Current = Cursors.Default;
		}


		private void OnJumpLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WordListItem item = _choicesList.SelectedItem as WordListItem;
			if (item == null)
				return;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				_dictionaryAccessor.JumpToEntry(item.Id);
			}
			catch (Exception error)
			{
				MainWindow.Logger.Log(error.Message);
			}

			Cursor.Current = Cursors.Default;

		}

		private void UpdateDisplay()
		{
			WordListItem item = _choicesList.SelectedItem as WordListItem;
			if (item == null)
			{
				_jumpLink.Enabled = false;
				_jumpLink.Text = "";
			}
			else
			{
				_jumpLink.Text = string.Format("Jump to {0} in dictionary application", item);
				_jumpLink.Enabled = true;
			}
			_findSimilarButton.Enabled = !string.IsNullOrEmpty(_word.Text);
		}

		private void LookupControl_Load(object sender, EventArgs e)
		{
			//not working!
			_entryViewer.Refresh();
			_entryViewer.DocumentText = string.Format(@"<html><body></body></html>");
			_entryViewer.Refresh();
			UpdateDisplay();
		}

		private void OnWord_TextChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

	}
}