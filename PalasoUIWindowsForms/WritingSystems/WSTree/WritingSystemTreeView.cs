using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems.WSTree
{
	public partial class WritingSystemTreeView : UserControl
	{
		private readonly WritingSystemTreeModel _model;
		public event EventHandler UpdateDisplay;

		public WritingSystemTreeView()
		{
			InitializeComponent();
		}
		public WritingSystemTreeView(WritingSystemTreeModel model)
		{
			_model = model;
			InitializeComponent();
			UpdateDisplay += new EventHandler(OnUpdateDisplay);
		}

		void OnUpdateDisplay(object sender, EventArgs e)
		{
			treeView1.SuspendLayout();
			treeView1.Nodes.Clear();
			foreach (var item in _model.GetTopLevelItems())
			{
				treeView1.Nodes.Add(item.MakeTreeNode());
			}
			treeView1.ResumeLayout();
		}

		private void WritingSystemTreeView_Load(object sender, EventArgs e)
		{
			_model.ViewLoaded(UpdateDisplay);
		}
	}
}
