using Palaso.UI.WindowsForms.ClearShare;
using Palaso.UI.WindowsForms.ClearShare.WinFormsUI;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class ImageToolboxControl
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
			if (disposing)
			{
				_toolImages.Dispose();
				if (components != null)
				{

					components.Dispose();
				}
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
			System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("test", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("testing");
			this._toolListView = new System.Windows.Forms.ListView();
			this._panelForControls = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this._invitationToMetadataPanel = new System.Windows.Forms.Panel();
			this.betterLabel1 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this._editMetadataLink = new System.Windows.Forms.LinkLabel();
			this._metadataDisplayControl = new Palaso.UI.WindowsForms.ClearShare.WinFormsUI.MetadataDisplayControl();
			this._currentImageBox = new System.Windows.Forms.PictureBox();
			this.panel1.SuspendLayout();
			this._invitationToMetadataPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._currentImageBox)).BeginInit();
			this.SuspendLayout();
			//
			// _toolListView
			//
			this._toolListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			listViewGroup1.Header = "test";
			listViewGroup1.Name = "listViewGroup1";
			this._toolListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
			listViewGroup1});
			this._toolListView.HideSelection = false;
			this._toolListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
			listViewItem1});
			this._toolListView.Location = new System.Drawing.Point(0, 0);
			this._toolListView.Name = "_toolListView";
			this._toolListView.ShowGroups = false;
			this._toolListView.Size = new System.Drawing.Size(112, 495);
			this._toolListView.TabIndex = 0;
			this._toolListView.UseCompatibleStateImageBehavior = false;
			this._toolListView.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			//
			// _panelForControls
			//
			this._panelForControls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._panelForControls.BackColor = System.Drawing.SystemColors.Control;
			this._panelForControls.Location = new System.Drawing.Point(118, 0);
			this._panelForControls.Name = "_panelForControls";
			this._panelForControls.Size = new System.Drawing.Size(424, 495);
			this._panelForControls.TabIndex = 3;
			//
			// panel1
			//
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this._invitationToMetadataPanel);
			this.panel1.Controls.Add(this._metadataDisplayControl);
			this.panel1.Controls.Add(this._currentImageBox);
			this.panel1.Location = new System.Drawing.Point(548, 2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(267, 496);
			this.panel1.TabIndex = 5;
			//
			// _invitationToMetadataPanel
			//
			this._invitationToMetadataPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._invitationToMetadataPanel.Controls.Add(this.betterLabel1);
			this._invitationToMetadataPanel.Controls.Add(this._editMetadataLink);
			this._invitationToMetadataPanel.Location = new System.Drawing.Point(7, 349);
			this._invitationToMetadataPanel.Name = "_invitationToMetadataPanel";
			this._invitationToMetadataPanel.Size = new System.Drawing.Size(251, 133);
			this._invitationToMetadataPanel.TabIndex = 8;
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.Location = new System.Drawing.Point(0, 6);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(245, 80);
			this.betterLabel1.TabIndex = 9;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "This image does not know:\r\n\r\nWho created it?\r\nWho can use it?";
			//
			// _editMetadataLink
			//
			this._editMetadataLink.AutoSize = true;
			this._editMetadataLink.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._editMetadataLink.Location = new System.Drawing.Point(0, 100);
			this._editMetadataLink.Name = "_editMetadataLink";
			this._editMetadataLink.Size = new System.Drawing.Size(213, 19);
			this._editMetadataLink.TabIndex = 8;
			this._editMetadataLink.TabStop = true;
			this._editMetadataLink.Text = "Can you answer these questions?";
			this._editMetadataLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnEditMetadataLink_LinkClicked);
			//
			// _metadataDisplayControl
			//
			this._metadataDisplayControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._metadataDisplayControl.Location = new System.Drawing.Point(7, 296);
			this._metadataDisplayControl.Name = "_metadataDisplayControl";
			this._metadataDisplayControl.Size = new System.Drawing.Size(251, 190);
			this._metadataDisplayControl.TabIndex = 6;
			//
			// _currentImageBox
			//
			this._currentImageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._currentImageBox.BackColor = System.Drawing.SystemColors.Control;
			this._currentImageBox.Location = new System.Drawing.Point(7, 0);
			this._currentImageBox.MinimumSize = new System.Drawing.Size(251, 245);
			this._currentImageBox.Name = "_currentImageBox";
			this._currentImageBox.Size = new System.Drawing.Size(251, 245);
			this._currentImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._currentImageBox.TabIndex = 5;
			this._currentImageBox.TabStop = false;
			//
			// ImageToolboxControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this._panelForControls);
			this.Controls.Add(this._toolListView);
			this.Name = "ImageToolboxControl";
			this.Size = new System.Drawing.Size(814, 498);
			this.panel1.ResumeLayout(false);
			this._invitationToMetadataPanel.ResumeLayout(false);
			this._invitationToMetadataPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._currentImageBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView _toolListView;
		private System.Windows.Forms.Panel _panelForControls;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox _currentImageBox;
		private MetadataDisplayControl _metadataDisplayControl;
		private System.Windows.Forms.Panel _invitationToMetadataPanel;
		private Widgets.BetterLabel betterLabel1;
		private System.Windows.Forms.LinkLabel _editMetadataLink;
	}
}
