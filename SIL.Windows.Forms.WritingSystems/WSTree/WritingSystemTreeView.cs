﻿using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.WritingSystems.WSTree
{
	public partial class WritingSystemTreeView : UserControl
	{
		private  WritingSystemTreeModel _model;

		public WritingSystemTreeView()
		{
			InitializeComponent();
		}
		public void BindToModel(WritingSystemTreeModel model)
		{
			  _model = model;
			_model.UpdateDisplay += OnUpdateDisplay;
		}
		public WritingSystemTreeView(WritingSystemTreeModel model)
		{
		   InitializeComponent();
		   BindToModel(model);
		}

		void OnUpdateDisplay(object sender, EventArgs e)
		{
			treeView1.AfterSelect -= treeView1_AfterSelect;
			treeView1.BeginUpdate();
			treeView1.SuspendLayout();
			treeView1.Nodes.Clear();
			var items = _model.GetTreeItems();
			foreach (var item in items)
			{
				var node = item.MakeTreeNode();
				treeView1.Nodes.Add(node);
//                if(item.Selected)
//                    treeView1.SelectedNode = node;
			}
			treeView1.ExpandAll();
			var selectedItem = _model.GetSelectedItem(items);
			foreach (TreeNode topLevelNode in treeView1.Nodes)
			{
				if(topLevelNode.Tag == selectedItem)
				{
					treeView1.SelectedNode = topLevelNode;
					break;
				}
				foreach (TreeNode node in topLevelNode.Nodes)
				{
					if (node.Tag == selectedItem)
					{
						//This will not actually work under a number of circumstances
						//and caused a crash in WeSay and some really wierd behavior in the palaso writing systems UI test app
						//see http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/4843bab1-a7fb-48d6-a66a-80b2e7232e73
						treeView1.SelectedNode = node;
						break;
					}
				}
			}
			//if there is no current selection, need to create a dummy invisible node
			//to be selected, else the tree view control selects the first one
			if(selectedItem ==null)
			{
				treeView1.SelectedNode =  treeView1.Nodes.Add(string.Empty);
			}
			treeView1.ResumeLayout(false);
			treeView1.EndUpdate();
			if (treeView1.SelectedNode != null)
			{
				treeView1.SelectedNode.EnsureVisible();
			}
			treeView1.AfterSelect += treeView1_AfterSelect;
		}

		private void WritingSystemTreeView_Load(object sender, EventArgs e)
		{
			if(_model!=null)
				_model.ViewLoaded();
		}

		private void treeView1_Click(object sender, EventArgs e)
		{
//            if (treeView1.SelectedNode == null)
//                return;
//
//            ((WritingSystemTreeItem) treeView1.SelectedNode.Tag).Clicked();
//            OnUpdateDisplay(this,null);
//
		}

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (treeView1.SelectedNode == null ||
				treeView1.SelectedNode.Tag==null)//hack
				return;

			((WritingSystemTreeItem) treeView1.SelectedNode.Tag).Clicked();
		   // OnUpdateDisplay(this, null);

		}

		private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			e.Cancel =e.Node.Tag!=null && !((WritingSystemTreeItem)e.Node.Tag).CanSelect;
		}
	}
}
