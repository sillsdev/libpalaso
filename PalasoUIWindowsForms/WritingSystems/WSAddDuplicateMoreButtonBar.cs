using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSAddDuplicateMoreButtonBar : UserControl
	{
		SetupPM _model;

		public WSAddDuplicateMoreButtonBar()
		{
			InitializeComponent();
		}

		public void BindToModel(SetupPM model)
		{
			_model = model;
		}

		private void MoreButtonClick(object sender, EventArgs e)
		{
			// Popup context menu
			_contextMenu.Show(_moreButton, 0, 0);
		}

		private void AddButtonClick(object sender, EventArgs e)
		{
			_model.AddNew();
		}

		private void DuplicateButtonClick(object sender, EventArgs e)
		{
			_model.DuplicateCurrent();
		}

		private void DeleteMenuClick(object sender, EventArgs e)
		{
			_model.DeleteCurrent();
		}

		private void LoadMenuClick(object sender, EventArgs e)
		{
			// TODO:
		}
	}
}
