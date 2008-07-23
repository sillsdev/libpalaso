using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSPropertiesDialog : UserControl
	{
		private SetupPM _model;
		private WSAboutControl _aboutControl;
		private TabPage _aboutControlPage;

		public WSPropertiesDialog()
		{
			InitializeComponent();
			AddTabPages();
		}

		public void AddTabPages()
		{
			TabPage page;
			_tabControl.TabPages.Clear();

			// Create and add "About" page
			_aboutControl = new WSAboutControl();
			_aboutControlPage = new TabPage("About");
			_aboutControlPage.Controls.Add(_aboutControl);
			_aboutControl.Dock = DockStyle.Fill;
			_aboutControl.TextChanged += delegate { _aboutControlPage.Text = _aboutControl.Text; };
			_tabControl.TabPages.Add(_aboutControlPage);
		}

		public void BindToModel(SetupPM model)
		{
			_model = model;
			_buttonBar.BindToModel(_model);
			_picker.BindToModel(_model);
			_aboutControl.BindToModel(_model);
		}
	}
}
