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
		private WritingSystemSetupModel _wsModel;
		private IWritingSystemRepository _repository;

		public WritingSystemTest()
		{

			InitializeComponent();

			_repository = new LdmlInFolderWritingSystemRepository();
			_wsModel = new WritingSystemSetupModel(_repository);
			this.wsPropertiesPanel1.BindToModel(_wsModel);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			_wsModel.Save();
		}
	}
}
