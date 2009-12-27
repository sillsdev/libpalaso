namespace Palaso.UI.WindowsForms.WritingSystems.WSTree
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
			System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("English");
			System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Edolo (IPA)");
			System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Add Edolo (Voice)");
			System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Edolo", new System.Windows.Forms.TreeNode[] {
			treeNode7,
			treeNode8});
			System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Tok Pisin");
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			//
			// treeView1
			//
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView1.HideSelection = false;
			this.treeView1.Location = new System.Drawing.Point(0, 0);
			this.treeView1.Name = "treeView1";
			treeNode6.Name = "Node0";
			treeNode6.NodeFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode6.Text = "English";
			treeNode7.Name = "Node2";
			treeNode7.NodeFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode7.Text = "Edolo (IPA)";
			treeNode8.ForeColor = System.Drawing.Color.Navy;
			treeNode8.Name = "Node3";
			treeNode8.NodeFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode8.Text = "Add Edolo (Voice)";
			treeNode9.Name = "Node1";
			treeNode9.NodeFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode9.Text = "Edolo";
			treeNode10.Name = "Node4";
			treeNode10.NodeFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			treeNode10.Text = "Tok Pisin";
			this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
			treeNode6,
			treeNode9,
			treeNode10});
			this.treeView1.ShowLines = false;
			this.treeView1.ShowNodeToolTips = true;
			this.treeView1.ShowPlusMinus = false;
			this.treeView1.ShowRootLines = false;
			this.treeView1.Size = new System.Drawing.Size(150, 365);
			this.treeView1.TabIndex = 0;
			//
			// WritingSystemTreeView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.treeView1);
			this.Name = "WritingSystemTreeView";
			this.Size = new System.Drawing.Size(150, 365);
			this.Load += new System.EventHandler(this.WritingSystemTreeView_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView treeView1;
	}
}
