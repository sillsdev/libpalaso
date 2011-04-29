namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	partial class UnlistedLanguageView
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.nonStandardLanguageCode = new System.Windows.Forms.TextBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.betterLabel1 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.nonStandardLanguageName = new System.Windows.Forms.TextBox();
			this.betterLabel2 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.SuspendLayout();
			//
			// nonStandardLanguageCode
			//
			this.nonStandardLanguageCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.nonStandardLanguageCode.Location = new System.Drawing.Point(117, 18);
			this.nonStandardLanguageCode.Name = "nonStandardLanguageCode";
			this.nonStandardLanguageCode.Size = new System.Drawing.Size(52, 20);
			this.nonStandardLanguageCode.TabIndex = 1;
			this.nonStandardLanguageCode.Leave += new System.EventHandler(this.field_OnLeave);
			//
			// linkLabel1
			//
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(1, 78);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(213, 13);
			this.linkLabel1.TabIndex = 2;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Read about language identifiers on the web";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.Location = new System.Drawing.Point(7, 18);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(101, 20);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Language Code:";
			this.betterLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.betterLabel1.TextChanged += new System.EventHandler(this.betterLabel1_TextChanged);
			//
			// nonStandardLanguageName
			//
			this.nonStandardLanguageName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.nonStandardLanguageName.Location = new System.Drawing.Point(115, 44);
			this.nonStandardLanguageName.Name = "nonStandardLanguageName";
			this.nonStandardLanguageName.Size = new System.Drawing.Size(137, 20);
			this.nonStandardLanguageName.TabIndex = 4;
			this.nonStandardLanguageName.Leave += new System.EventHandler(this.field_OnLeave);
			//
			// betterLabel2
			//
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel2.Location = new System.Drawing.Point(1, 44);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(108, 20);
			this.betterLabel2.TabIndex = 3;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "Language Name:";
			this.betterLabel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// UnlistedLanguageView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.nonStandardLanguageName);
			this.Controls.Add(this.betterLabel2);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.nonStandardLanguageCode);
			this.Controls.Add(this.betterLabel1);
			this.Name = "UnlistedLanguageView";
			this.Size = new System.Drawing.Size(270, 117);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel1;
		private System.Windows.Forms.TextBox nonStandardLanguageCode;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.TextBox nonStandardLanguageName;
		private Widgets.BetterLabel betterLabel2;
	}
}
