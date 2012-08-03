namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
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
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._minimallyCompleteCheckTimer = new System.Windows.Forms.Timer(this.components);
			this._metadataEditorControl = new Palaso.UI.WindowsForms.ClearShare.WinFormsUI.MetadataEditorControl();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(155, 514);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(246, 514);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 3;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			//
			// _minimallyCompleteCheckTimer
			//
			this._minimallyCompleteCheckTimer.Enabled = true;
			this._minimallyCompleteCheckTimer.Interval = 500;
			this._minimallyCompleteCheckTimer.Tick += new System.EventHandler(this._minimallyCompleteCheckTimer_Tick);
			//
			// _metadataEditorControl
			//
			this._metadataEditorControl.Location = new System.Drawing.Point(-1, 0);
			this._metadataEditorControl.Metadata = null;
			this._metadataEditorControl.Name = "_metadataEditorControl";
			this._metadataEditorControl.ShowCreator = false;
			this._metadataEditorControl.Size = new System.Drawing.Size(338, 505);
			this._metadataEditorControl.TabIndex = 0;
			//
			// MetadataEditorDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(338, 549);
			this.ControlBox = false;
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._metadataEditorControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "MetadataEditorDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Done at runtime";
			this.ResumeLayout(false);

		}

		#endregion

		private MetadataEditorControl _metadataEditorControl;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Timer _minimallyCompleteCheckTimer;
	}
}