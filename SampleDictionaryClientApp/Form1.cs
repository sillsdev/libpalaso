using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Palaso.DictionaryService.Client;
using Palaso.Services;
using SampleDictionaryServicesApplication;

namespace SampleDictionaryClientApp
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
				IDictionaryService dictionaryService = GetDictionaryService();
				if (dictionaryService == null)
				{
					_entryViewer.DocumentText = "Failed Get Dictionary Service";
					return;
				}
				string[] ids =
					dictionaryService.GetIdsOfMatchingEntries(_writingSystemId.Text, _word.Text, FindMethods.Exact);
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

		private IDictionaryService GetDictionaryService()
		{
			IDictionaryService dictionaryService = IPCUtils.GetExistingService<IDictionaryService>("net.pipe://localhost/DictionaryServices/qTest");
			if (dictionaryService == null)
			{
				Cursor.Current = Cursors.WaitCursor;
			   // System.Diagnostics.Process.Start("SampleDictionaryServicesApplication.exe", "-server");
				string arguments = '"'+_dictionaryPath.Text +'"'+ " -server";
				System.Diagnostics.Process.Start(@"c:\wesay\output\debug\wesay.app.exe", arguments);
				for (int i = 0; i < 10; i++)
				{
					System.Threading.Thread.Sleep(500);
					dictionaryService = IPCUtils.GetExistingService<IDictionaryService>("net.pipe://localhost/DictionaryServices/"+_writingSystemId.Text);
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
	}
}