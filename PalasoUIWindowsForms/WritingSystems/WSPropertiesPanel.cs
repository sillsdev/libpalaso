using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Palaso.WritingSystems;

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
			_model.MethodToShowUiToBootstrapNewDefinition= ShowCreateNewWritingSystemDialog;

			_buttonBar.BindToModel(_model);
			_propertiesTabControl.BindToModel(_model);

			var treeModel = new WritingSystemTreeModel(_model);
			treeModel.Suggestor = new WritingSystemVariantSuggestor();
			treeModel.OtherKnownWritingSystems = new WritingSystemFromWindowsLocaleProvider();
			_treeView.BindToModel(treeModel);
		}

		private static WritingSystemDefinition ShowCreateNewWritingSystemDialog()
		{
			var dlg= new LookupISOCodeDialog();
			dlg.ShowDialog();
			if(dlg.DialogResult!=DialogResult.OK)
				return null;
			return new WritingSystemDefinition(dlg.ISOCode, string.Empty,string.Empty,string.Empty, dlg.ISOCodeAndName.Name, dlg.ISOCode,false);
		}
	}
}
