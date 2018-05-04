using SIL.Windows.Forms.SuperToolTip;

namespace SIL.Windows.Forms.Tests.SuperToolTip
{
	partial class SuperToolTipTestForm
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
			SuperToolTipInfoWrapper superToolTipInfoWrapper1 = new SuperToolTipInfoWrapper();
			SuperToolTipInfo superToolTipInfo1 = new SuperToolTipInfo();
			SuperToolTipInfoWrapper superToolTipInfoWrapper2 = new SuperToolTipInfoWrapper();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.superToolTip1 = new Windows.Forms.SuperToolTip.SuperToolTip(this.components);
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// richTextBox1
			//
			this.richTextBox1.Location = new System.Drawing.Point(21, 12);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(235, 167);
			superToolTipInfo1.BackgroundGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			superToolTipInfo1.BackgroundGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(218)))), ((int)(((byte)(239)))));
			superToolTipInfo1.BackgroundGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(251)))));
			superToolTipInfo1.BodyFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			superToolTipInfo1.BodyText = "hello\r\nalsdkjfsaldjk\r\nasdlfjasl\r\n\r\nsalkdjflk\r\n";
			superToolTipInfo1.HeaderFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
			superToolTipInfo1.HeaderText = "Test";
			superToolTipInfo1.OffsetForWhereToDisplay = new System.Drawing.Point(0, 0);
			superToolTipInfoWrapper1.SuperToolTipInfo = superToolTipInfo1;
			superToolTipInfoWrapper1.UseSuperToolTip = true;
			this.superToolTip1.SetSuperStuff(this.richTextBox1, superToolTipInfoWrapper1);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "one\n\n\ntwo\n\n\nthree\n\n\nfour";
			this.richTextBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.richTextBox1_MouseMove);
			//
			// button1
			//
			this.button1.Location = new System.Drawing.Point(146, 198);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			superToolTipInfoWrapper2.SuperToolTipInfo = null;
			this.superToolTip1.SetSuperStuff(this.button1, superToolTipInfoWrapper2);
			this.button1.TabIndex = 1;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			//
			// SuperToolTipTestForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.richTextBox1);
			this.Name = "SuperToolTipTestForm";
			this.Text = "SuperToolTipTestForm";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBox1;
		private Windows.Forms.SuperToolTip.SuperToolTip superToolTip1;
		private System.Windows.Forms.Button button1;
	}
}
