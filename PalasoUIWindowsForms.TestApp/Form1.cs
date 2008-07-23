using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;
using System.IO;

namespace PalasoUIWindowsForms.TestApp
{
	public partial class Form1 : Form
	{
		private SetupPM _model;
		private string tempPath;

		public Form1()
		{
			InitializeComponent();
			tempPath = Path.GetTempPath() + "WS-Test";
			Directory.CreateDirectory(tempPath);
			_model = new SetupPM(new LdmlInFolderWritingSystemStore(tempPath));
			dialog.BindToModel(_model);
		}

		protected override void OnClosed(EventArgs e)
		{
			_model.Save();
			base.OnClosed(e);
		}
	}
}