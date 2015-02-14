using SIL.WindowsForms.Widgets;

namespace SIL.WritingSystems.WindowsForms.WSIdentifiers
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
			this.nonStandardLanguageName = new System.Windows.Forms.TextBox();
			this.betterLabel2 = new BetterLabel();
			this.SuspendLayout();
			//
			// nonStandardLanguageName
			//
			this.nonStandardLanguageName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.nonStandardLanguageName.Location = new System.Drawing.Point(115, 34);
			this.nonStandardLanguageName.Name = "nonStandardLanguageName";
			this.nonStandardLanguageName.Size = new System.Drawing.Size(137, 20);
			this.nonStandardLanguageName.TabIndex = 4;
			this.nonStandardLanguageName.Leave += new System.EventHandler(this.OnLeave_NonStandardLanguageName);
			//
			// betterLabel2
			//
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel2.Location = new System.Drawing.Point(7, 34);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(108, 20);
			this.betterLabel2.TabIndex = 3;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "Language Name:";
			//
			// UnlistedLanguageView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.nonStandardLanguageName);
			this.Controls.Add(this.betterLabel2);
			this.Name = "UnlistedLanguageView";
			this.Size = new System.Drawing.Size(270, 117);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox nonStandardLanguageName;
		private BetterLabel betterLabel2;
	}
}
