namespace Palaso.Reporting
{
	partial class NonFatalErrorDialog
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
			this._message = new System.Windows.Forms.TextBox();
			this._icon = new System.Windows.Forms.PictureBox();
			this._acceptButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this._icon)).BeginInit();
			this.SuspendLayout();
			//
			// _message
			//
			this._message.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._message.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._message.Location = new System.Drawing.Point(85, 40);
			this._message.Multiline = true;
			this._message.Name = "_message";
			this._message.ReadOnly = true;
			this._message.Size = new System.Drawing.Size(275, 126);
			this._message.TabIndex = 0;
			this._message.Text = "Blah blah";
			//
			// _icon
			//
			this._icon.Location = new System.Drawing.Point(23, 29);
			this._icon.Name = "_icon";
			this._icon.Size = new System.Drawing.Size(45, 36);
			this._icon.TabIndex = 1;
			this._icon.TabStop = false;
			//
			// _acceptButton
			//
			this._acceptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._acceptButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._acceptButton.Location = new System.Drawing.Point(285, 188);
			this._acceptButton.Name = "_acceptButton";
			this._acceptButton.Size = new System.Drawing.Size(75, 23);
			this._acceptButton.TabIndex = 0;
			this._acceptButton.Text = "&OK";
			this._acceptButton.UseVisualStyleBackColor = true;
			this._acceptButton.Click += new System.EventHandler(this._acceptButton_Click);
			//
			// NonFatalErrorDialog
			//
			this.AcceptButton = this._acceptButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this._acceptButton;
			this.ClientSize = new System.Drawing.Size(372, 223);
			this.ControlBox = false;
			this.Controls.Add(this._acceptButton);
			this.Controls.Add(this._icon);
			this.Controls.Add(this._message);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NonFatalErrorDialog";
			this.Text = "NonFatalErrorDialog";
			this.Load += new System.EventHandler(this.NonFatalErrorDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this._icon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.TextBox _message;
		private System.Windows.Forms.PictureBox _icon;
		internal System.Windows.Forms.Button _acceptButton;
	}
}