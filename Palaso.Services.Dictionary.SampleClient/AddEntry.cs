using System;
using System.Windows.Forms;
using Palaso.DictionaryService.Client;
using Palaso.DictionaryService.SampleClient.Properties;

namespace Palaso.DictionaryService.SampleClient
{
	public partial class AddEntry : UserControl
	{
		private DictionaryAccessor _dictionaryAccessor;

		public AddEntry()
		{
			InitializeComponent();
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

		private void OnAddButton_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				MainWindow.Logger.Log("Adding");
				string id =
					_dictionaryAccessor.AddEntry(Settings.Default.WritingSystemIdForWords, this._word.Text,
									 Settings.Default.WritingSystemIdForDefinitions,
									 _definition.Text.Trim(),
									 Settings.Default.WritingSystemIdForWords, _example.Text.Trim());


				if (string.IsNullOrEmpty(id))
				{
					MainWindow.Logger.Log("Error adding");
				}
				else
				{
					MainWindow.Logger.Log("Entry Added");
				}
				_word.Text = "";
				_definition.Text = "";
				_example.Text = "";
				_word.Focus();
			}
			catch (Exception error)
			{
				MainWindow.Logger.Log(error.Message);
			}
			Cursor.Current = Cursors.Default;

		}
	}
}
