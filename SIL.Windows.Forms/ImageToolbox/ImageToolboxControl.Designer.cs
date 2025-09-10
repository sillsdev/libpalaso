using SIL.Windows.Forms.ClearShare.WinFormsUI;
using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.ImageToolbox
{
	partial class ImageToolboxControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("test", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("testing");
			this._toolListView = new System.Windows.Forms.ListView();
			this._panelForControls = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this._editLink = new System.Windows.Forms.LinkLabel();
			this._invitationToMetadataPanel = new System.Windows.Forms.Panel();
			this._copyExemplarMetadata = new BetterLinkLabel();
			this.betterLabel1 = new BetterLabel();
			this._editMetadataLink = new System.Windows.Forms.LinkLabel();
			this._metadataDisplayControl = new MetadataDisplayControl();
			this._currentImageBox = new System.Windows.Forms.PictureBox();
			this._toolTip = new System.Windows.Forms.ToolTip(this.components);
			this._l10nSharpExtender = new L10NSharp.Windows.Forms.L10NSharpExtender(this.components);
			this.panel1.SuspendLayout();
			this._invitationToMetadataPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._currentImageBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._l10nSharpExtender)).BeginInit();
			this.SuspendLayout();
			//
			// _toolListView
			//
			this._toolListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this._toolListView.BackColor = System.Drawing.SystemColors.Control;
			this._toolListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
			this.panel1.Controls.Add(this._editLink);
			this.panel1.Controls.Add(this._invitationToMetadataPanel);
			this.panel1.Controls.Add(this._metadataDisplayControl);
			this.panel1.Controls.Add(this._currentImageBox);
			this.panel1.Location = new System.Drawing.Point(548, 2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(267, 496);
			this.panel1.TabIndex = 5;
			//
			// _editLink
			//
			this._editLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._editLink.AutoSize = true;
			this._editLink.LinkColor = System.Drawing.Color.Black;
			this._l10nSharpExtender.SetLocalizableToolTip(this._editLink, null);
			this._l10nSharpExtender.SetLocalizationComment(this._editLink, null);
			this._l10nSharpExtender.SetLocalizingId(this._editLink, "ImageToolbox.EditMetadataLink");
			this._editLink.Location = new System.Drawing.Point(12, 473);
			this._editLink.Name = "_editLink";
			this._editLink.Size = new System.Drawing.Size(34, 13);
			this._editLink.TabIndex = 9;
			this._editLink.TabStop = true;
			this._editLink.Text = "Edit...";
			this._editLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnEditMetadataLink_LinkClicked);
			//
			// _invitationToMetadataPanel
			//
			this._invitationToMetadataPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._invitationToMetadataPanel.Controls.Add(this._copyExemplarMetadata);
			this._invitationToMetadataPanel.Controls.Add(this.betterLabel1);
			this._invitationToMetadataPanel.Controls.Add(this._editMetadataLink);
			this._invitationToMetadataPanel.Location = new System.Drawing.Point(9, 271);
			this._invitationToMetadataPanel.Name = "_invitationToMetadataPanel";
			this._invitationToMetadataPanel.Size = new System.Drawing.Size(251, 195);
			this._invitationToMetadataPanel.TabIndex = 8;
			//
			// _copyExemplarMetadata
			//
			this._copyExemplarMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._copyExemplarMetadata.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._copyExemplarMetadata.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
			this._copyExemplarMetadata.ForeColor = System.Drawing.Color.Blue;
			this._l10nSharpExtender.SetLocalizableToolTip(this._copyExemplarMetadata, null);
			this._l10nSharpExtender.SetLocalizationComment(this._copyExemplarMetadata, null);
			this._l10nSharpExtender.SetLocalizationPriority(this._copyExemplarMetadata, L10NSharp.LocalizationPriority.NotLocalizable);
			this._l10nSharpExtender.SetLocalizingId(this._copyExemplarMetadata, "ImageToolbox.CopyExemplar");
			this._copyExemplarMetadata.Location = new System.Drawing.Point(5, 104);
			this._copyExemplarMetadata.Multiline = true;
			this._copyExemplarMetadata.Name = "_copyExemplarMetadata";
			this._copyExemplarMetadata.ReadOnly = true;
			this._copyExemplarMetadata.Size = new System.Drawing.Size(237, 17);
			this._copyExemplarMetadata.TabIndex = 11;
			this._copyExemplarMetadata.TabStop = false;
			this._copyExemplarMetadata.Text = "Copy Exemplar that is really long";
			this._copyExemplarMetadata.URL = null;
			this._copyExemplarMetadata.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnCopyExemplar_MouseClick);
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Enabled = false;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._l10nSharpExtender.SetLocalizableToolTip(this.betterLabel1, null);
			this._l10nSharpExtender.SetLocalizationComment(this.betterLabel1, null);
			this._l10nSharpExtender.SetLocalizingId(this.betterLabel1, "ImageToolbox.PromptForMissingMetadata");
			this.betterLabel1.Location = new System.Drawing.Point(0, 6);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(245, 65);
			this.betterLabel1.TabIndex = 9;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "This image does not know:\r\n\r\nWho created it?\r\nWho can use it?";
			//
			// _editMetadataLink
			//
			this._editMetadataLink.AutoSize = true;
			this._editMetadataLink.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._l10nSharpExtender.SetLocalizableToolTip(this._editMetadataLink, null);
			this._l10nSharpExtender.SetLocalizationComment(this._editMetadataLink, null);
			this._l10nSharpExtender.SetLocalizingId(this._editMetadataLink, "ImageToolbox.SetUpMetadataLink");
			this._editMetadataLink.Location = new System.Drawing.Point(4, 82);
			this._editMetadataLink.Name = "_editMetadataLink";
			this._editMetadataLink.Size = new System.Drawing.Size(119, 19);
			this._editMetadataLink.TabIndex = 8;
			this._editMetadataLink.TabStop = true;
			this._editMetadataLink.Text = "Set up metadata...";
			this._editMetadataLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnEditMetadataLink_LinkClicked);
			//
			// _metadataDisplayControl
			//
			this._metadataDisplayControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._l10nSharpExtender.SetLocalizableToolTip(this._metadataDisplayControl, null);
			this._l10nSharpExtender.SetLocalizationComment(this._metadataDisplayControl, null);
			this._l10nSharpExtender.SetLocalizingId(this._metadataDisplayControl, "ImageToolboxControl.MetadataDisplayControl");
			this._metadataDisplayControl.Location = new System.Drawing.Point(7, 298);
			this._metadataDisplayControl.Name = "_metadataDisplayControl";
			this._metadataDisplayControl.Size = new System.Drawing.Size(253, 172);
			this._metadataDisplayControl.TabIndex = 6;
			//
			// _currentImageBox
			//
			this._currentImageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._currentImageBox.BackColor = System.Drawing.SystemColors.Control;
			this._l10nSharpExtender.SetLocalizableToolTip(this._currentImageBox, null);
			this._l10nSharpExtender.SetLocalizationComment(this._currentImageBox, null);
			this._l10nSharpExtender.SetLocalizingId(this._currentImageBox, "ImageToolboxControl._currentImageBox");
			this._currentImageBox.Location = new System.Drawing.Point(7, 0);
			this._currentImageBox.MinimumSize = new System.Drawing.Size(251, 245);
			this._currentImageBox.Name = "_currentImageBox";
			this._currentImageBox.Size = new System.Drawing.Size(253, 245);
			this._currentImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._currentImageBox.TabIndex = 5;
			this._currentImageBox.TabStop = false;
			//
			// _l10nSharpExtender
			//
			this._l10nSharpExtender.LocalizationManagerId = "Palaso";
			this._l10nSharpExtender.PrefixForNewItems = "ImageToolbox";
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
			this._l10nSharpExtender.SetLocalizableToolTip(this, null);
			this._l10nSharpExtender.SetLocalizationComment(this, null);
			this._l10nSharpExtender.SetLocalizingId(this, "ImageToolboxControl.ImageToolboxControl");
			this.Name = "ImageToolboxControl";
			this.Size = new System.Drawing.Size(814, 498);
			this.Load += new System.EventHandler(this.OnLoad);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this._invitationToMetadataPanel.ResumeLayout(false);
			this._invitationToMetadataPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._currentImageBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._l10nSharpExtender)).EndInit();
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
		private System.Windows.Forms.LinkLabel _editLink;
		private Widgets.BetterLinkLabel _copyExemplarMetadata;
		private System.Windows.Forms.ToolTip _toolTip;
		private L10NSharp.Windows.Forms.L10NSharpExtender _l10nSharpExtender;
	}
}
