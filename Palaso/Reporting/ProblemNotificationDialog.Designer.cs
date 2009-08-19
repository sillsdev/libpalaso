namespace Palaso.Reporting
{
	partial class ProblemNotificationDialog
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
			this._reoccurenceMessage = new System.Windows.Forms.Label();
			this._alternateButton1 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this._icon)).BeginInit();
			this.SuspendLayout();
			//
			// _message
			//
			this._message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._message.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._message.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._message.Location = new System.Drawing.Point(85, 40);
			this._message.Multiline = true;
			this._message.Name = "_message";
			this._message.ReadOnly = true;
			this._message.Size = new System.Drawing.Size(275, 167);
			this._message.TabIndex = 0;
			this._message.Text = "Blah blah";
			this._message.TextChanged += new System.EventHandler(this._message_TextChanged);
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
			this._acceptButton.Location = new System.Drawing.Point(280, 228);
			this._acceptButton.Name = "_acceptButton";
			this._acceptButton.Size = new System.Drawing.Size(80, 23);
			this._acceptButton.TabIndex = 0;
			this._acceptButton.Text = "&OK";
			this._acceptButton.UseVisualStyleBackColor = true;
			this._acceptButton.Click += new System.EventHandler(this._acceptButton_Click);
			//
			// _reoccurenceMessage
			//
			this._reoccurenceMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._reoccurenceMessage.ForeColor = System.Drawing.Color.Gray;
			this._reoccurenceMessage.Location = new System.Drawing.Point(20, 228);
			this._reoccurenceMessage.MaximumSize = new System.Drawing.Size(250, 300);
			this._reoccurenceMessage.Name = "_reoccurenceMessage";
			this._reoccurenceMessage.Size = new System.Drawing.Size(146, 23);
			this._reoccurenceMessage.TabIndex = 2;
			this._reoccurenceMessage.Text = "Re-occurence message";
			this._reoccurenceMessage.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			// _alternateButton1
			//
			this._alternateButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._alternateButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._alternateButton1.Location = new System.Drawing.Point(172, 228);
			this._alternateButton1.Name = "_alternateButton1";
			this._alternateButton1.Size = new System.Drawing.Size(102, 23);
			this._alternateButton1.TabIndex = 3;
			this._alternateButton1.Text = "&Caller Defined";
			this._alternateButton1.UseVisualStyleBackColor = true;
			this._alternateButton1.Visible = false;
			this._alternateButton1.Click += new System.EventHandler(this.OnAlternateButton1_Click);
			//
			// ProblemNotificationDialog
			//
			this.AcceptButton = this._acceptButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this._acceptButton;
			this.ClientSize = new System.Drawing.Size(372, 263);
			this.ControlBox = false;
			this.Controls.Add(this._alternateButton1);
			this.Controls.Add(this._reoccurenceMessage);
			this.Controls.Add(this._acceptButton);
			this.Controls.Add(this._icon);
			this.Controls.Add(this._message);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProblemNotificationDialog";
			this.Text = "Problem Notification";
			this.Load += new System.EventHandler(this.NonFatalErrorDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this._icon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.TextBox _message;
		private System.Windows.Forms.PictureBox _icon;
		internal System.Windows.Forms.Button _acceptButton;
		private System.Windows.Forms.Label _reoccurenceMessage;
		internal System.Windows.Forms.Button _alternateButton1;
	}
}