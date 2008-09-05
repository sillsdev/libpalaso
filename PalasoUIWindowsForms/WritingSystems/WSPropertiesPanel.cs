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
		private SetupPM _model;

		public WSPropertiesPanel()
		{
			InitializeComponent();
		}

		public void BindToModel(SetupPM model)
		{
			_model = model;
			_buttonBar.BindToModel(_model);
			_picker.BindToModel(_model);
			_propertiesTabControl.BindToModel(_model);
		}
	}
}
