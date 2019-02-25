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
			System.Windows.Forms.Button cancelButton;
			System.Windows.Forms.Timer minimallyCompleteCheckTimer;
			System.Windows.Forms.Panel panel1;

			this.components = new System.ComponentModel.Container();
			this._okButton = new System.Windows.Forms.Button();
			cancelButton = new System.Windows.Forms.Button();
			minimallyCompleteCheckTimer = new System.Windows.Forms.Timer(this.components);
			panel1 = new System.Windows.Forms.Panel();
			panel1.SuspendLayout();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._metadataEditorControl = new MetadataEditorControl();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._okButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._okButton, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._okButton, L10NSharp.LocalizationPriority.High);
			this._L10NSharpExtender.SetLocalizingId(this._okButton, "Common.OKButton");
			this._okButton.Location = new System.Drawing.Point(164, 10);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 30);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _cancelButton
			//
			cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._L10NSharpExtender.SetLocalizableToolTip(cancelButton, null);
			this._L10NSharpExtender.SetLocalizationComment(cancelButton, null);
			this._L10NSharpExtender.SetLocalizationPriority(cancelButton, L10NSharp.LocalizationPriority.High);
			this._L10NSharpExtender.SetLocalizingId(cancelButton, "Common.CancelButton");
			cancelButton.Location = new System.Drawing.Point(255, 10);
			cancelButton.Name = "_cancelButton";
			cancelButton.Size = new System.Drawing.Size(75, 30);
			cancelButton.TabIndex = 3;
			cancelButton.Text = "&Cancel";
			cancelButton.UseVisualStyleBackColor = true;
			cancelButton.Click += new System.EventHandler(_cancelButton_Click);
			//
			// _minimallyCompleteCheckTimer
			//
			minimallyCompleteCheckTimer.Enabled = true;
			minimallyCompleteCheckTimer.Interval = 500;
			minimallyCompleteCheckTimer.Tick += new System.EventHandler(_minimallyCompleteCheckTimer_Tick);
			//
			// _L10NSharpExtender
			//
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "MetadataEditor";
			//
			// _metadataEditorControl
			//
			this._L10NSharpExtender.SetLocalizableToolTip(this._metadataEditorControl, null);
			this._L10NSharpExtender.SetLocalizationComment(this._metadataEditorControl, null);
			this._L10NSharpExtender.SetLocalizingId(this._metadataEditorControl, "MetadataEditor.MetadataEditorDialog.MetadataEditorControl");
			this._metadataEditorControl.Location = new System.Drawing.Point(1, 12);
			this._metadataEditorControl.Metadata = null;
			this._metadataEditorControl.Name = "_metadataEditorControl";
			this._metadataEditorControl.ShowCreator = false;
			this._metadataEditorControl.Size = new System.Drawing.Size(338, 590);
			this._metadataEditorControl.TabIndex = 0;
			this._metadataEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			//
			// panel1
			//
			panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			panel1.ClientSize = new System.Drawing.Size(347, 50);
			panel1.Controls.Add(cancelButton);
			panel1.Controls.Add(this._okButton);
			//
			// MetadataEditorDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = cancelButton;
			this.ClientSize = new System.Drawing.Size(472, 640);
			this.ControlBox = false;
			this.Controls.Add(panel1);
			this.Controls.Add(this._metadataEditorControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this, "MetadataEditorDialog.WindowTitle");
			this.Name = "MetadataEditorDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Done at runtime";
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private MetadataEditorControl _metadataEditorControl;
		private System.Windows.Forms.Button _okButton;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
	}
}