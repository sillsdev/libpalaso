using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace TestApp
{
	public partial class WritingSystemTest : Form
	{
		private readonly WritingSystemSetupModel _wsModel;
		private readonly IWritingSystemRepository _repository;

		public WritingSystemTest()
		{

			InitializeComponent();

			_repository = GlobalWritingSystemRepository.Initialize(MigrationHandler);
			_wsModel = new WritingSystemSetupModel(_repository);
			wsPropertiesPanel1.BindToModel(_wsModel);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			_wsModel.Save();
		}

		public void MigrationHandler(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
		{
		}
	}


}
