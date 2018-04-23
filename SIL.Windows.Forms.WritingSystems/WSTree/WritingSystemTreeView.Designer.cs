namespace SIL.Windows.Forms.WritingSystems.WSTree
{
	partial class WritingSystemTreeView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("English");
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Edolo (IPA)");
			System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Add Edolo (Voice)");
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Edolo", new System.Windows.Forms.TreeNode[] {
			treeNode2,
			treeNode3});
			System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Tok Pisin");
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			//
			// treeView1
			//
			this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.treeView1.HideSelection = false;
			this.treeView1.Location = new System.Drawing.Point(10, 10);
			this.treeView1.Margin = new System.Windows.Forms.Padding(113, 113, 3, 3);
			this.treeView1.Name = "treeView1";
			treeNode1.Name = "Node0";
			treeNode1.NodeFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode1.Text = "English";
			treeNode2.Name = "Node2";
			treeNode2.NodeFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode2.Text = "Edolo (IPA)";
			treeNode3.ForeColor = System.Drawing.Color.Navy;
			treeNode3.Name = "Node3";
			treeNode3.NodeFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode3.Text = "Add Edolo (Voice)";
			treeNode4.Name = "Node1";
			treeNode4.NodeFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode4.Text = "Edolo";
			treeNode5.Name = "Node4";
			treeNode5.NodeFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode5.Text = "Tok Pisin";
			this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
			treeNode1,
			treeNode4,
			treeNode5});
			this.treeView1.ShowLines = false;
			this.treeView1.ShowNodeToolTips = true;
			this.treeView1.ShowPlusMinus = false;
			this.treeView1.ShowRootLines = false;
			this.treeView1.Size = new System.Drawing.Size(138, 353);
			this.treeView1.TabIndex = 0;
			this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
			this.treeView1.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeSelect);
			this.treeView1.Click += new System.EventHandler(this.treeView1_Click);
			//
			// WritingSystemTreeView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.treeView1);
			this.Name = "WritingSystemTreeView";
			this.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
			this.Size = new System.Drawing.Size(148, 363);
			this.Load += new System.EventHandler(this.WritingSystemTreeView_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView treeView1;
	}
}
