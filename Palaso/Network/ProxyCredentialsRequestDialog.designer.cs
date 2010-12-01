namespace Palaso.Network
{
	partial class ProxyCredentialsRequestDialog
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
			this._userName = new System.Windows.Forms.TextBox();
			this._password = new System.Windows.Forms.TextBox();
			this._headerLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this._remember = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(109, 173);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 3;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(190, 173);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 4;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			//
			// _userName
			//
			this._userName.Location = new System.Drawing.Point(81, 67);
			this._userName.Name = "_userName";
			this._userName.Size = new System.Drawing.Size(161, 20);
			this._userName.TabIndex = 0;
			//
			// _password
			//
			this._password.Location = new System.Drawing.Point(81, 94);
			this._password.Name = "_password";
			this._password.Size = new System.Drawing.Size(161, 20);
			this._password.TabIndex = 1;
			this._password.UseSystemPasswordChar = true;
			//
			// _headerLabel
			//
			this._headerLabel.AutoSize = true;
			this._headerLabel.Location = new System.Drawing.Point(12, 9);
			this._headerLabel.MaximumSize = new System.Drawing.Size(300, 0);
			this._headerLabel.Name = "_headerLabel";
			this._headerLabel.Size = new System.Drawing.Size(256, 13);
			this._headerLabel.TabIndex = 4;
			this._headerLabel.Text = "The proxy server requires a username and password.";
			this._headerLabel.Click += new System.EventHandler(this._headerLabel_Click);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(17, 67);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Name";
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Location = new System.Drawing.Point(17, 97);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Password";
			//
			// _remember
			//
			this._remember.AutoSize = true;
			this._remember.BackColor = System.Drawing.Color.Transparent;
			this._remember.Checked = true;
			this._remember.CheckState = System.Windows.Forms.CheckState.Checked;
			this._remember.Location = new System.Drawing.Point(81, 120);
			this._remember.Name = "_remember";
			this._remember.Size = new System.Drawing.Size(147, 17);
			this._remember.TabIndex = 2;
			this._remember.Text = "Remember my credentials";
			this._remember.UseVisualStyleBackColor = false;
			//
			// panel1
			//
			this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
			this.panel1.Location = new System.Drawing.Point(12, 52);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(256, 94);
			this.panel1.TabIndex = 7;
			//
			// ProxyCredentialsRequestDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(288, 208);
			this.Controls.Add(this._remember);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._headerLabel);
			this.Controls.Add(this._password);
			this.Controls.Add(this._userName);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProxyCredentialsRequestDialog";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Authentication Required";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.TextBox _userName;
		private System.Windows.Forms.TextBox _password;
		private System.Windows.Forms.Label _headerLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox _remember;
		private System.Windows.Forms.Panel panel1;
	}
}