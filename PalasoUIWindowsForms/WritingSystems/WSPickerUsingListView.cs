using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSPickerUsingListView : UserControl
	{
		SetupPM _model;

		public WSPickerUsingListView()
		{
			InitializeComponent();
		}

		public void BindToModel(SetupPM model)
		{
			_model = model;
		}
	}
}
