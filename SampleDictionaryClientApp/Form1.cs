using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
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
			ILookup lookup = IPCUtils.GetExistingService<ILookup>("DictionaryServiceFor_xyz");
			if (lookup == null)
			{
			}

			_entryViewer.DocumentText = lookup.GetHmtlForWord("mango");
		}
	}
}