namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSPropertiesPanel
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this._treeView = new Palaso.UI.WindowsForms.WritingSystems.WSTree.WritingSystemTreeView();
			this._propertiesTabControl = new Palaso.UI.WindowsForms.WritingSystems.WSPropertiesTabControl();
			this._buttonBar = new Palaso.UI.WindowsForms.WritingSystems.WSAddDuplicateMoreButtonBar();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			//
			// splitContainer1
			//
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// splitContainer1.Panel1
			//
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			//
			// splitContainer1.Panel2
			//
			this.splitContainer1.Panel2.Controls.Add(this._buttonBar);
			this.splitContainer1.Size = new System.Drawing.Size(841, 461);
			this.splitContainer1.SplitterDistance = 422;
			this.splitContainer1.TabIndex = 2;
			//
			// splitContainer2
			//
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			//
			// splitContainer2.Panel1
			//
			this.splitContainer2.Panel1.Controls.Add(this._treeView);
			//
			// splitContainer2.Panel2
			//
			this.splitContainer2.Panel2.Controls.Add(this._propertiesTabControl);
			this.splitContainer2.Size = new System.Drawing.Size(841, 422);
			this.splitContainer2.SplitterDistance = 303;
			this.splitContainer2.SplitterWidth = 10;
			this.splitContainer2.TabIndex = 0;
			//
			// _treeView
			//
			this._treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._treeView.Location = new System.Drawing.Point(0, 0);
			this._treeView.Name = "_treeView";
			this._treeView.Size = new System.Drawing.Size(303, 422);
			this._treeView.TabIndex = 1;
			//
			// _propertiesTabControl
			//
			this._propertiesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._propertiesTabControl.Location = new System.Drawing.Point(0, 0);
			this._propertiesTabControl.Name = "_propertiesTabControl";
			this._propertiesTabControl.Size = new System.Drawing.Size(528, 422);
			this._propertiesTabControl.TabIndex = 0;
			//
			// _buttonBar
			//
			this._buttonBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._buttonBar.Location = new System.Drawing.Point(0, 4);
			this._buttonBar.Name = "_buttonBar";
			this._buttonBar.Size = new System.Drawing.Size(841, 31);
			this._buttonBar.TabIndex = 0;
			//
			// WSPropertiesPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "WSPropertiesPanel";
			this.Size = new System.Drawing.Size(841, 461);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private WSAddDuplicateMoreButtonBar _buttonBar;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private WSPropertiesTabControl _propertiesTabControl;
		private Palaso.UI.WindowsForms.WritingSystems.WSTree.WritingSystemTreeView _treeView;

	}
}
