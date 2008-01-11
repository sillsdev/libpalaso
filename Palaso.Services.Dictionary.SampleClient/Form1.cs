using System;
using System.Windows.Forms;
using Palaso.Services.Dictionary;
using SampleDictionaryServicesApplication;

namespace Palaso.Services.Dictionary.SampleClient
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void _lookupButton_Click(object sender, EventArgs e)
		{
			try
			{
				IDictionaryService dictionaryService = GetDictionaryService(_dictionaryPath.Text);
				if (dictionaryService == null)
				{
					_entryViewer.DocumentText = "Failed Get Dictionary Service";
					return;
				}
				string[] ids;
				string[] forms;
				dictionaryService.GetMatchingEntries(_writingSystemId.Text, _word.Text, FindMethods.Exact, out ids, out forms);
				if (ids.Length == 0)
				{
					_entryViewer.DocumentText = "Not Found";
				}
				else
				{
					_entryViewer.DocumentText = dictionaryService.GetHmtlForEntry(ids[0]);
				}
			}
			catch (Exception error)
			{
				_entryViewer.DocumentText = error.Message;
			}
		}

		private static string GetServiceAddress(string liftPath)
		{
			return "net.pipe://localhost/DictionaryServices/"
				   + Uri.EscapeDataString(liftPath);
		}

		private IDictionaryService GetDictionaryService(string pathToLift)
		{
			IDictionaryService dictionaryService = IPCUtils.GetExistingService<IDictionaryService>(GetServiceAddress(pathToLift));
			if (dictionaryService == null)
			{
				Cursor.Current = Cursors.WaitCursor;
			   // System.Diagnostics.Process.Start("SampleDictionaryServicesApplication.exe", "-server");
				string arguments = '"'+_dictionaryPath.Text +'"'+ " -server";
				System.Diagnostics.Process.Start(@"c:\wesay\output\debug\wesay.app.exe", arguments);
				for (int i = 0; i < 20; i++)
				{
					System.Threading.Thread.Sleep(500);
					dictionaryService = IPCUtils.GetExistingService<IDictionaryService>(GetServiceAddress(pathToLift));
					if (dictionaryService != null)
					{
						break;
					}
				}
				Cursor.Current = Cursors.Default;
			}
			return dictionaryService;
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void _jumpButton_Click(object sender, EventArgs e)
		{
			try
			{
				IDictionaryService dictionaryService = GetDictionaryService(_dictionaryPath.Text);
				if (dictionaryService == null)
				{
					_entryViewer.DocumentText = "Failed Get Dictionary Service";
					return;
				}
				string[] ids;
				string[] forms;
			   dictionaryService.GetMatchingEntries(_writingSystemId.Text, _word.Text, FindMethods.Exact, out ids, out forms);
				if (ids.Length == 0)
				{
					_entryViewer.DocumentText = "Not Found";
				}
				else
				{
					_entryViewer.DocumentText = "jumping to "+ids[0];
					dictionaryService.JumpToEntry(ids[0]);
				}
			}
			catch (Exception error)
			{
				_entryViewer.DocumentText = error.Message;
			}
		}

		private void _addEntryButton_Click(object sender, EventArgs e)
		{
			IDictionaryService dictionaryService = GetDictionaryService(_dictionaryPath.Text);
			if (dictionaryService == null)
			{
				_entryViewer.DocumentText = "Failed Get Dictionary Service";
				return;
			}

			_entryViewer.DocumentText = "Adding";
			string id =
				dictionaryService.AddEntry(_writingSystemId.Text, _word.Text, "en",
										   "Pretend definition of " + _word.Text,
										   _writingSystemId.Text, "So I said '" + _word.Text + "'!");
			if (string.IsNullOrEmpty(id))
			{
				_entryViewer.DocumentText = "Error adding";
			}
			else
			{
				_entryViewer.DocumentText = "New Entry id is " + id;
			}
		}

		private void _findSimilarButton_Click(object sender, EventArgs e)
		{
			_choicesList.Items.Clear();
			try
			{
				IDictionaryService dictionaryService = GetDictionaryService(_dictionaryPath.Text);
				if (dictionaryService == null)
				{
					_entryViewer.DocumentText = "Failed Get Dictionary Service";
					return;
				}
				string[] ids;
				string[] forms;
				dictionaryService.GetMatchingEntries(_writingSystemId.Text, _word.Text, FindMethods.DefaultApproximate, out ids, out forms);
				if (ids.Length == 0)
				{
					_entryViewer.DocumentText = "Not Found";
				}
				else
				{
					_entryViewer.DocumentText = "Found " + ids.Length;
//                    string[] forms =
//                        dictionaryService.GetFormsFromIds(_writingSystemId.Text, ids);
					foreach (string form in forms)
					{
							_choicesList.Items.Add(form);
					}
				}
			}
			catch (Exception error)
			{
				_entryViewer.DocumentText = error.Message;
			}

		}
	}
}