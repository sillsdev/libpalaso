using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;

namespace TestApp
{
	public partial class WritingSystemTest : Form
	{
		private WritingSystemSetupPM _wsModel;
		private IWritingSystemStore _store;

		public WritingSystemTest()
		{

			InitializeComponent();

			_store = new LdmlInFolderWritingSystemStore();
			_wsModel = new WritingSystemSetupPM(_store);
			this.wsPropertiesPanel1.BindToModel(_wsModel);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			_wsModel.Save();
		}
	}
}
