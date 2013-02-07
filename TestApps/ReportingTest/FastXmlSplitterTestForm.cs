using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Palaso.Xml;

namespace TestApp
{
	public partial class FastXmlSplitterTestForm : Form
	{
		public FastXmlSplitterTestForm()
		{
			InitializeComponent();
		}

		private void LoadDataFile(object sender, EventArgs e)
		{
			try
			{
				string selectedPathname = null;
				using (var fileDlg = new OpenFileDialog())
				{
					if (fileDlg.ShowDialog(this) == DialogResult.OK)
						selectedPathname = fileDlg.FileName;
				}
				if (!string.IsNullOrEmpty(selectedPathname))
				{
					var extension = Path.GetExtension(selectedPathname).ToLowerInvariant();
					string firstElementMarker = null;
					string recordMarker = null;
					switch (extension)
					{
						case ".lift":
							firstElementMarker = "header";
							recordMarker = "entry";
							break;
						case ".chorusnotes":
							recordMarker = "annotation";
							break;
						case ".fwdata":
							firstElementMarker = "AdditionalFields";
							recordMarker = "rt";
							break;
					}
					using (var splitter = new FastXmlElementSplitter(selectedPathname))
					{
						bool foundOptionalFirstElement;
						var results = splitter.GetSecondLevelElementBytes(firstElementMarker, recordMarker, out foundOptionalFirstElement);
						Console.WriteLine("Records: " + results.Count());
					}
				}
				Close();
			}
			catch (Exception err)
			{
				var msg = err.Message;
				Console.WriteLine(msg);
				throw;
			}
		}
	}
}
