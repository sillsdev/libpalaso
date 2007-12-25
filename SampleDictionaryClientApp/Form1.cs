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
			ILookup lookup = GetDictionaryService();
			if (lookup == null)
			{
				_entryViewer.DocumentText = "Failed Get Dictionary Service";
				return;
			}
			_entryViewer.DocumentText = lookup.GetHmtlForWord(_word.Text);
		}

		private ILookup GetDictionaryService()
		{
			ILookup lookup = IPCUtils.GetExistingService<ILookup>("net.pipe://localhost/DictionaryServices/qTest");
			if (lookup == null)
			{
				Cursor.Current = Cursors.WaitCursor;
				System.Diagnostics.Process.Start("SampleDictionaryServicesApplication.exe", "-server");
				for (int i = 0; i < 10; i++)
				{
					System.Threading.Thread.Sleep(500);
					lookup = IPCUtils.GetExistingService<ILookup>("net.pipe://localhost/DictionaryServices/qTest");
					if (lookup != null)
					{
						break;
					}
				}
				Cursor.Current = Cursors.Default;
			}
			return lookup;
		}
	}
}