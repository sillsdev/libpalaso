using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSPropertiesTabControl : UserControl
	{
		private SetupPM _model;

		public WSPropertiesTabControl()
		{
			InitializeComponent();
			_aboutControl.TextChanged += delegate { _aboutPage.Text = _aboutControl.Text; };
		}

		public void BindToModel(SetupPM model)
		{
			_model = model;
			_aboutControl.BindToModel(_model);
			_fontControl.BindToModel(_model);
			_keyboardControl.BindToModel(_model);
			_sortControl.BindToModel(_model);
			_spellingControl.BindToModel(_model);
		}
	}
}
