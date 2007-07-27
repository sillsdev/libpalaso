using Palaso.UI.Widgets;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSListControl
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
			this._writingSystemList = new Palaso.UI.Widgets.ControlListBox();
			this._linkAddNew = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			//
			// _writingSystemList
			//
			this._writingSystemList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
																					| System.Windows.Forms.AnchorStyles.Left)
																				   | System.Windows.Forms.AnchorStyles.Right)));
			this._writingSystemList.Location = new System.Drawing.Point(0, 3);
			this._writingSystemList.Name = "_writingSystemList";
			this._writingSystemList.Size = new System.Drawing.Size(564, 184);
			this._writingSystemList.TabIndex = 0;
			//
			// _linkAddNew
			//
			this._linkAddNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._linkAddNew.AutoSize = true;
			this._linkAddNew.Location = new System.Drawing.Point(23, 194);
			this._linkAddNew.Name = "_linkAddNew";
			this._linkAddNew.Size = new System.Drawing.Size(117, 13);
			this._linkAddNew.TabIndex = 1;
			this._linkAddNew.TabStop = true;
			this._linkAddNew.Text = "Add new writing system";
			this._linkAddNew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAddNewClicked);
			//
			// WSListControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._linkAddNew);
			this.Controls.Add(this._writingSystemList);
			this.Name = "WSListControl";
			this.Size = new System.Drawing.Size(568, 214);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ControlListBox _writingSystemList;
		private System.Windows.Forms.LinkLabel _linkAddNew;
	}
}