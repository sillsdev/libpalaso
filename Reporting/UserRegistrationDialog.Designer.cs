namespace Palaso.Reporting
{
	partial class UserRegistrationDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserRegistrationDialog));
			this._emailAddress = new System.Windows.Forms.TextBox();
			this._noticeLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this._okButton = new System.Windows.Forms.Button();
			this._welcomeLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _emailAddress
			//
			this._emailAddress.Location = new System.Drawing.Point(26, 77);
			this._emailAddress.Name = "_emailAddress";
			this._emailAddress.Size = new System.Drawing.Size(228, 20);
			this._emailAddress.TabIndex = 0;
			this._emailAddress.TextChanged += new System.EventHandler(this._emailAddress_TextChanged);
			//
			// _noticeLabel
			//
			this._noticeLabel.AutoSize = true;
			this._noticeLabel.Location = new System.Drawing.Point(23, 117);
			this._noticeLabel.MaximumSize = new System.Drawing.Size(230, 0);
			this._noticeLabel.Name = "_noticeLabel";
			this._noticeLabel.Size = new System.Drawing.Size(225, 143);
			this._noticeLabel.TabIndex = 1;
			this._noticeLabel.Text = resources.GetString("_noticeLabel.Text");
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(23, 61);
			this.label2.MaximumSize = new System.Drawing.Size(200, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(73, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Email Address";
			//
			// _okButton
			//
			this._okButton.Location = new System.Drawing.Point(178, 291);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&Register";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this.OnOkButton);
			//
			// _welcomeLabel
			//
			this._welcomeLabel.AutoSize = true;
			this._welcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._welcomeLabel.Location = new System.Drawing.Point(23, 18);
			this._welcomeLabel.MaximumSize = new System.Drawing.Size(200, 0);
			this._welcomeLabel.Name = "_welcomeLabel";
			this._welcomeLabel.Size = new System.Drawing.Size(103, 13);
			this._welcomeLabel.TabIndex = 3;
			this._welcomeLabel.Text = "Welcome To {0}!";
			//
			// UserRegistrationDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(291, 327);
			this.Controls.Add(this._welcomeLabel);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._noticeLabel);
			this.Controls.Add(this._emailAddress);
			this.Name = "UserRegistrationDialog";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Please Register";
			this.Load += new System.EventHandler(this.UserRegistrationDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _emailAddress;
		private System.Windows.Forms.Label _noticeLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Label _welcomeLabel;
	}
}