namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSPropertiesDialog
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
			this._buttonBar = new Palaso.UI.WindowsForms.WritingSystems.WSAddDuplicateMoreButtonBar();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this._picker = new Palaso.UI.WindowsForms.WritingSystems.WSPickerUsingListView();
			this._tabControl = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this._tabControl.SuspendLayout();
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
			// _buttonBar
			//
			this._buttonBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._buttonBar.Location = new System.Drawing.Point(0, 4);
			this._buttonBar.Name = "_buttonBar";
			this._buttonBar.Size = new System.Drawing.Size(841, 31);
			this._buttonBar.TabIndex = 0;
			//
			// splitContainer2
			//
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			//
			// splitContainer2.Panel1
			//
			this.splitContainer2.Panel1.Controls.Add(this._picker);
			//
			// splitContainer2.Panel2
			//
			this.splitContainer2.Panel2.Controls.Add(this._tabControl);
			this.splitContainer2.Size = new System.Drawing.Size(841, 422);
			this.splitContainer2.SplitterDistance = 243;
			this.splitContainer2.SplitterWidth = 10;
			this.splitContainer2.TabIndex = 0;
			//
			// _picker
			//
			this._picker.Dock = System.Windows.Forms.DockStyle.Fill;
			this._picker.Location = new System.Drawing.Point(0, 0);
			this._picker.Name = "_picker";
			this._picker.SelectedIndex = -1;
			this._picker.Size = new System.Drawing.Size(243, 422);
			this._picker.TabIndex = 0;
			//
			// _tabControl
			//
			this._tabControl.Controls.Add(this.tabPage1);
			this._tabControl.Controls.Add(this.tabPage2);
			this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tabControl.Location = new System.Drawing.Point(0, 0);
			this._tabControl.Name = "_tabControl";
			this._tabControl.SelectedIndex = 0;
			this._tabControl.Size = new System.Drawing.Size(588, 422);
			this._tabControl.TabIndex = 0;
			//
			// tabPage1
			//
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(580, 396);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "tabPage1";
			this.tabPage1.UseVisualStyleBackColor = true;
			//
			// tabPage2
			//
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(192, 74);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "tabPage2";
			this.tabPage2.UseVisualStyleBackColor = true;
			//
			// WSPropertiesDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "WSPropertiesDialog";
			this.Size = new System.Drawing.Size(841, 461);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this._tabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private WSAddDuplicateMoreButtonBar _buttonBar;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private WSPickerUsingListView _picker;
		private System.Windows.Forms.TabControl _tabControl;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;

	}
}
