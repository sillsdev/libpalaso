namespace Palaso.Email.TestApp
{
	partial class Form1
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
			System.Windows.Forms.Label label1;
			System.Windows.Forms.Label label2;
			System.Windows.Forms.Label label3;
			System.Windows.Forms.Label label4;
			System.Windows.Forms.Label label6;
			System.Windows.Forms.Button btnPreferred;
			System.Windows.Forms.Button btnAlternate;
			this.to = new System.Windows.Forms.TextBox();
			this.cc = new System.Windows.Forms.TextBox();
			this.bcc = new System.Windows.Forms.TextBox();
			this.subject = new System.Windows.Forms.TextBox();
			this.body = new System.Windows.Forms.TextBox();
			this.files = new System.Windows.Forms.TextBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.status = new System.Windows.Forms.TextBox();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label6 = new System.Windows.Forms.Label();
			btnPreferred = new System.Windows.Forms.Button();
			btnAlternate = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// label1
			//
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(24, 26);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(23, 13);
			label1.TabIndex = 0;
			label1.Text = "To:";
			//
			// to
			//
			this.to.Location = new System.Drawing.Point(53, 26);
			this.to.Name = "to";
			this.to.Size = new System.Drawing.Size(219, 20);
			this.to.TabIndex = 1;
			//
			// label2
			//
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(24, 52);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(23, 13);
			label2.TabIndex = 0;
			label2.Text = "Cc:";
			//
			// cc
			//
			this.cc.Location = new System.Drawing.Point(53, 52);
			this.cc.Name = "cc";
			this.cc.Size = new System.Drawing.Size(219, 20);
			this.cc.TabIndex = 2;
			//
			// label3
			//
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(24, 78);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(29, 13);
			label3.TabIndex = 0;
			label3.Text = "Bcc:";
			//
			// bcc
			//
			this.bcc.Location = new System.Drawing.Point(53, 78);
			this.bcc.Name = "bcc";
			this.bcc.Size = new System.Drawing.Size(219, 20);
			this.bcc.TabIndex = 3;
			//
			// label4
			//
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(24, 104);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(46, 13);
			label4.TabIndex = 0;
			label4.Text = "Subject:";
			//
			// subject
			//
			this.subject.Location = new System.Drawing.Point(76, 104);
			this.subject.Name = "subject";
			this.subject.Size = new System.Drawing.Size(196, 20);
			this.subject.TabIndex = 4;
			//
			// body
			//
			this.body.Location = new System.Drawing.Point(27, 130);
			this.body.Multiline = true;
			this.body.Name = "body";
			this.body.Size = new System.Drawing.Size(245, 53);
			this.body.TabIndex = 5;
			//
			// label6
			//
			label6.AutoSize = true;
			label6.Location = new System.Drawing.Point(24, 189);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(69, 13);
			label6.TabIndex = 0;
			label6.Text = "Attachments:";
			//
			// files
			//
			this.files.Location = new System.Drawing.Point(99, 189);
			this.files.Name = "files";
			this.files.Size = new System.Drawing.Size(173, 20);
			this.files.TabIndex = 6;
			//
			// btnPreferred
			//
			btnPreferred.Location = new System.Drawing.Point(27, 215);
			btnPreferred.Name = "btnPreferred";
			btnPreferred.Size = new System.Drawing.Size(75, 23);
			btnPreferred.TabIndex = 7;
			btnPreferred.Text = "Preferred";
			this.toolTip1.SetToolTip(btnPreferred, "Send email with preferred email provider");
			btnPreferred.UseVisualStyleBackColor = true;
			btnPreferred.Click += new System.EventHandler(this.OnPreferredClicked);
			//
			// btnAlternate
			//
			btnAlternate.Location = new System.Drawing.Point(109, 215);
			btnAlternate.Name = "btnAlternate";
			btnAlternate.Size = new System.Drawing.Size(75, 23);
			btnAlternate.TabIndex = 8;
			btnAlternate.Text = "Alternate";
			this.toolTip1.SetToolTip(btnAlternate, "Send email through alternate email provider");
			btnAlternate.UseVisualStyleBackColor = true;
			btnAlternate.Click += new System.EventHandler(this.OnAlternateClicked);
			//
			// status
			//
			this.status.Location = new System.Drawing.Point(27, 244);
			this.status.Multiline = true;
			this.status.Name = "status";
			this.status.ReadOnly = true;
			this.status.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.status.Size = new System.Drawing.Size(245, 91);
			this.status.TabIndex = 11;
			this.status.TabStop = false;
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 347);
			this.Controls.Add(this.status);
			this.Controls.Add(btnAlternate);
			this.Controls.Add(btnPreferred);
			this.Controls.Add(this.files);
			this.Controls.Add(label6);
			this.Controls.Add(this.body);
			this.Controls.Add(this.subject);
			this.Controls.Add(label4);
			this.Controls.Add(this.bcc);
			this.Controls.Add(label3);
			this.Controls.Add(this.cc);
			this.Controls.Add(label2);
			this.Controls.Add(this.to);
			this.Controls.Add(label1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox to;
		private System.Windows.Forms.TextBox cc;
		private System.Windows.Forms.TextBox bcc;
		private System.Windows.Forms.TextBox subject;
		private System.Windows.Forms.TextBox body;
		private System.Windows.Forms.TextBox files;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TextBox status;

	}
}
