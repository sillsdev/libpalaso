namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class LookupISOCodeDialog
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
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._lookupISOControl = new Palaso.UI.WindowsForms.WritingSystems.LookupISOControl();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._okButton.Location = new System.Drawing.Point(88, 364);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(169, 364);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 2;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			//
			// _lookupISOControl
			//
			this._lookupISOControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._lookupISOControl.ISOCode = "";
			this._lookupISOControl.Location = new System.Drawing.Point(12, 12);
			this._lookupISOControl.Name = "_lookupISOControl";
			this._lookupISOControl.Size = new System.Drawing.Size(255, 346);
			this._lookupISOControl.TabIndex = 0;
			this._lookupISOControl.ReadinessChanged += new System.EventHandler(this._lookupISOControl_Changed);
			this._lookupISOControl.DoubleClicked += new System.EventHandler(this.OnChooserDoubleClicked);
			//
			// LookupISOCodeDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(279, 399);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._lookupISOControl);
			this.Controls.Add(this._okButton);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LookupISOCodeDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Lookup Language Code...";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _okButton;
		private LookupISOControl _lookupISOControl;
		private System.Windows.Forms.Button _cancelButton;
	}
}