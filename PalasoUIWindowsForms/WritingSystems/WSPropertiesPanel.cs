using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSPropertiesPanel : UserControl
	{
		private WritingSystemSetupPM _model;

		public WSPropertiesPanel()
		{
			InitializeComponent();
		}

		public void BindToModel(WritingSystemSetupPM model)
		{
			_model = model;
			_buttonBar.BindToModel(_model);
			_propertiesTabControl.BindToModel(_model);

			var treeModel = new WritingSystemTreeModel(_model);
			treeModel.Suggestor = new WritingSystemVariantSuggestor();
			treeModel.OtherKnownWritingSystems = new WritingSystemFromWindowsLocaleProvider();
			_treeView.BindToModel(treeModel);
		}
	}
}
