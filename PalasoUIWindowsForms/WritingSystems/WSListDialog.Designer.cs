using Palaso.UI.WindowsForms.WritingSystems;

namespace Palaso.UI.WritingSystems
{
	partial class WSListDialog
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
			this._writingSystemListControl = new WSListControl();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._okButton.Location = new System.Drawing.Point(412, 290);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// wsListControl1
			//
			this._writingSystemListControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._writingSystemListControl.Location = new System.Drawing.Point(7, 12);
			this._writingSystemListControl.Name = "_writingSystemListControl";
			this._writingSystemListControl.Size = new System.Drawing.Size(568, 272);
			this._writingSystemListControl.TabIndex = 2;
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(493, 290);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 1;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			//
			// WSListDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(587, 325);
			this.ControlBox = false;
			this.Controls.Add(this._writingSystemListControl);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.Name = "WSListDialog";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Setup Writing Systems...";
			this.Load += new System.EventHandler(this.WSListDialog_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _okButton;
		private WSListControl _writingSystemListControl;
		private System.Windows.Forms.Button _cancelButton;
	}
}