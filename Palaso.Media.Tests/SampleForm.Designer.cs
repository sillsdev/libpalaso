namespace Palaso.Media.Tests
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
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.soundFieldControl1 = new Palaso.Media.SoundFieldControl();
			this.shortSoundFieldControl1 = new Palaso.Media.ShortSoundFieldControl();
			this.shortSoundFieldControl2 = new Palaso.Media.ShortSoundFieldControl();
			this.SuspendLayout();
			//
			// soundFieldControl1
			//
			this.soundFieldControl1.Location = new System.Drawing.Point(44, 21);
			this.soundFieldControl1.Name = "soundFieldControl1";
			this.soundFieldControl1.Path = null;
			this.soundFieldControl1.Size = new System.Drawing.Size(150, 34);
			this.soundFieldControl1.TabIndex = 0;
			//
			// shortSoundFieldControl1
			//
			this.shortSoundFieldControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.shortSoundFieldControl1.Location = new System.Drawing.Point(44, 92);
			this.shortSoundFieldControl1.Name = "shortSoundFieldControl1";
			this.shortSoundFieldControl1.Path = null;
			this.shortSoundFieldControl1.Size = new System.Drawing.Size(328, 22);
			this.shortSoundFieldControl1.TabIndex = 1;
			this.shortSoundFieldControl1.Load += new System.EventHandler(this.shortSoundFieldControl1_Load);
			//
			// shortSoundFieldControl2
			//
			this.shortSoundFieldControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.shortSoundFieldControl2.Location = new System.Drawing.Point(44, 150);
			this.shortSoundFieldControl2.Name = "shortSoundFieldControl2";
			this.shortSoundFieldControl2.Path = null;
			this.shortSoundFieldControl2.Size = new System.Drawing.Size(328, 25);
			this.shortSoundFieldControl2.TabIndex = 2;
			this.shortSoundFieldControl2.Load += new System.EventHandler(this.shortSoundFieldControl2_Load);
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(461, 264);
			this.Controls.Add(this.shortSoundFieldControl2);
			this.Controls.Add(this.shortSoundFieldControl1);
			this.Controls.Add(this.soundFieldControl1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.Text = "Palaso.Media Sample App";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timer1;
		private SoundFieldControl soundFieldControl1;
		private ShortSoundFieldControl shortSoundFieldControl1;
		private ShortSoundFieldControl shortSoundFieldControl2;
	}
}