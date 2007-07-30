namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class FontDialog
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
			this._fontControl = new Palaso.UI.WindowsForms.WritingSystems.FontControl();
			this._CancelButton = new System.Windows.Forms.Button();
			this._OkButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// fontControl1
			//
			this._fontControl.Location = new System.Drawing.Point(12, 36);
			this._fontControl.Name = "_fontControl";
			this._fontControl.Size = new System.Drawing.Size(258, 150);
			this._fontControl.TabIndex = 0;
			//
			// _CancelButton
			//
			this._CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._CancelButton.Location = new System.Drawing.Point(195, 163);
			this._CancelButton.Name = "_CancelButton";
			this._CancelButton.Size = new System.Drawing.Size(75, 23);
			this._CancelButton.TabIndex = 1;
			this._CancelButton.Text = "&Cancel";
			this._CancelButton.UseVisualStyleBackColor = true;
			this._CancelButton.Click += new System.EventHandler(this.OnCancelClick);
			//
			// _OkButton
			//
			this._OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._OkButton.Location = new System.Drawing.Point(114, 163);
			this._OkButton.Name = "_OkButton";
			this._OkButton.Size = new System.Drawing.Size(75, 23);
			this._OkButton.TabIndex = 1;
			this._OkButton.Text = "&OK";
			this._OkButton.UseVisualStyleBackColor = true;
			this._OkButton.Click += new System.EventHandler(this.OnOkClick);
			//
			// FontDialog
			//
			this.AcceptButton = this._OkButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._CancelButton;
			this.ClientSize = new System.Drawing.Size(292, 211);
			this.Controls.Add(this._OkButton);
			this.Controls.Add(this._CancelButton);
			this.Controls.Add(this._fontControl);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FontDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Writing System Default Font";
			this.ResumeLayout(false);

		}

		#endregion

		private FontControl _fontControl;
		private System.Windows.Forms.Button _CancelButton;
		private System.Windows.Forms.Button _OkButton;
	}
}