namespace SIL.Windows.Forms.ClearShare.WinFormsUI
{
	partial class MetadataEditorDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.cancelButton = new System.Windows.Forms.Button();
			this.minimallyCompleteCheckTimer = new System.Windows.Forms.Timer(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this._okButton = new System.Windows.Forms.Button();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._metadataEditorControl = new SIL.Windows.Forms.ClearShare.WinFormsUI.MetadataEditorControl();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._L10NSharpExtender.SetLocalizableToolTip(this.cancelButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this.cancelButton, null);
			this._L10NSharpExtender.SetLocalizationPriority(this.cancelButton, L10NSharp.LocalizationPriority.High);
			this._L10NSharpExtender.SetLocalizingId(this.cancelButton, "Common.CancelButton");
			this.cancelButton.Location = new System.Drawing.Point(380, 10);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 30);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			// 
			// minimallyCompleteCheckTimer
			// 
			this.minimallyCompleteCheckTimer.Enabled = true;
			this.minimallyCompleteCheckTimer.Interval = 500;
			this.minimallyCompleteCheckTimer.Tick += new System.EventHandler(this._minimallyCompleteCheckTimer_Tick);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this._okButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 597);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(472, 50);
			this.panel1.TabIndex = 0;
			// 
			// _okButton
			// 
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._okButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._okButton, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._okButton, L10NSharp.LocalizationPriority.High);
			this._L10NSharpExtender.SetLocalizingId(this._okButton, "Common.OKButton");
			this._okButton.Location = new System.Drawing.Point(289, 10);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 30);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			// 
			// _L10NSharpExtender
			// 
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "MetadataEditor";
			// 
			// _metadataEditorControl
			// 
			this._metadataEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._L10NSharpExtender.SetLocalizableToolTip(this._metadataEditorControl, null);
			this._L10NSharpExtender.SetLocalizationComment(this._metadataEditorControl, null);
			this._L10NSharpExtender.SetLocalizingId(this._metadataEditorControl, "MetadataEditor.MetadataEditorDialog.MetadataEditorControl");
			this._metadataEditorControl.Location = new System.Drawing.Point(1, 12);
			this._metadataEditorControl.Metadata = null;
			this._metadataEditorControl.Name = "_metadataEditorControl";
			this._metadataEditorControl.ShowCreator = false;
			this._metadataEditorControl.Size = new System.Drawing.Size(338, 590);
			this._metadataEditorControl.TabIndex = 0;
			// 
			// MetadataEditorDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(472, 647);
			this.ControlBox = false;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this._metadataEditorControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this, "MetadataEditorDialog.WindowTitle");
			this.Name = "MetadataEditorDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Done at runtime";
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private MetadataEditorControl _metadataEditorControl;
		private System.Windows.Forms.Button _okButton;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Timer minimallyCompleteCheckTimer;
		private System.Windows.Forms.Panel panel1;
	}
}