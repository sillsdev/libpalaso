using SIL.Windows.Forms.Widgets;

namespace SIL.Media.SoundFieldControlTestApp
{
	partial class MainForm
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
			System.Drawing.Imaging.ImageAttributes imageAttributes1 = new System.Drawing.Imaging.ImageAttributes();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.soundFieldControl1 = new SIL.Media.SoundFieldControl();
			this.shortSoundFieldControl1 = new SIL.Media.ShortSoundFieldControl();
			this.shortSoundFieldControl2 = new SIL.Media.ShortSoundFieldControl();
			this.bitmapButton1 = new SIL.Windows.Forms.Widgets.BitmapButton();
			this.SuspendLayout();
			// 
			// soundFieldControl1
			// 
			this.soundFieldControl1.Location = new System.Drawing.Point(44, 21);
			this.soundFieldControl1.Name = "soundFieldControl1";
			this.soundFieldControl1.Path = "";
			this.soundFieldControl1.Size = new System.Drawing.Size(150, 34);
			this.soundFieldControl1.TabIndex = 0;
			// 
			// shortSoundFieldControl1
			// 
			this.shortSoundFieldControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.shortSoundFieldControl1.Location = new System.Drawing.Point(44, 92);
			this.shortSoundFieldControl1.Name = "shortSoundFieldControl1";
			this.shortSoundFieldControl1.Path = "";
			this.shortSoundFieldControl1.PlayOnly = false;
			this.shortSoundFieldControl1.Size = new System.Drawing.Size(328, 22);
			this.shortSoundFieldControl1.TabIndex = 1;
			this.shortSoundFieldControl1.Load += new System.EventHandler(this.shortSoundFieldControl1_Load);
			// 
			// shortSoundFieldControl2
			// 
			this.shortSoundFieldControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.shortSoundFieldControl2.Location = new System.Drawing.Point(44, 150);
			this.shortSoundFieldControl2.Name = "shortSoundFieldControl2";
			this.shortSoundFieldControl2.Path = "";
			this.shortSoundFieldControl2.PlayOnly = false;
			this.shortSoundFieldControl2.Size = new System.Drawing.Size(328, 25);
			this.shortSoundFieldControl2.TabIndex = 2;
			this.shortSoundFieldControl2.Load += new System.EventHandler(this.shortSoundFieldControl2_Load);
			// 
			// bitmapButton1
			// 
			this.bitmapButton1.BackColor = System.Drawing.Color.Transparent;
			this.bitmapButton1.BorderColor = System.Drawing.Color.DarkBlue;
			this.bitmapButton1.DisabledTextColor = System.Drawing.Color.DimGray;
			this.bitmapButton1.FlatAppearance.BorderSize = 0;
			this.bitmapButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.bitmapButton1.FocusRectangleEnabled = true;
			this.bitmapButton1.Image = null;
			this.bitmapButton1.ImageAttributes = imageAttributes1;
			this.bitmapButton1.ImageBorderColor = System.Drawing.Color.Transparent;
			this.bitmapButton1.ImageBorderEnabled = false;
			this.bitmapButton1.ImageDropShadow = false;
			this.bitmapButton1.ImageFocused = null;
			this.bitmapButton1.ImageInactive = null;
			this.bitmapButton1.ImageMouseOver = null;
			this.bitmapButton1.ImageNormal = global::SIL.Media.SoundFieldControlTestApp.Properties.Resources.MP3_PLAYER;
			this.bitmapButton1.ImagePressed = null;
			this.bitmapButton1.InnerBorderColor = System.Drawing.Color.Transparent;
			this.bitmapButton1.InnerBorderColor_Focus = System.Drawing.Color.LightBlue;
			this.bitmapButton1.InnerBorderColor_MouseOver = System.Drawing.Color.Gold;
			this.bitmapButton1.Location = new System.Drawing.Point(326, 21);
			this.bitmapButton1.Name = "bitmapButton1";
			this.bitmapButton1.OffsetPressedContent = true;
			this.bitmapButton1.Size = new System.Drawing.Size(37, 34);
			this.bitmapButton1.StretchImage = false;
			this.bitmapButton1.TabIndex = 3;
			this.bitmapButton1.TextDropShadow = true;
			this.bitmapButton1.TextWordWrap = false;
			this.bitmapButton1.UseVisualStyleBackColor = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(461, 264);
			this.Controls.Add(this.bitmapButton1);
			this.Controls.Add(this.shortSoundFieldControl2);
			this.Controls.Add(this.shortSoundFieldControl1);
			this.Controls.Add(this.soundFieldControl1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.Text = "SIL.Media Sample App";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timer1;
		private SoundFieldControl soundFieldControl1;
		private ShortSoundFieldControl shortSoundFieldControl1;
		private ShortSoundFieldControl shortSoundFieldControl2;
		private BitmapButton bitmapButton1;
	}
}