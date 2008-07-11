using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSAboutControl : UserControl
	{
		private class WSProxy
		{
			private string _shortName;

			public string ShortName
			{
				get { return _shortName; }
				set { _shortName = value; }
			}

			private string _name;

			public string Name
			{
				get { return _name; }
				set { _name = value; }
			}


		}

		private SetupPM _model;
		private WSProxy _proxy;

		public WSAboutControl()
		{
			InitializeComponent();
			_proxy = new WSProxy();
			_pgAbout.SelectedObject = _proxy;
		}

		public void BindToModel(SetupPM model)
		{
			_model = model;
		}

		private void WSAboutControl_Load(object sender, EventArgs e)
		{

		}
	}
}
