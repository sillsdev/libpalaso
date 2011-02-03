namespace TestAppKeyboard
{
	partial class KeyboardForm
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
			this.testAreaA = new System.Windows.Forms.TextBox();
			this.testAreaB = new System.Windows.Forms.TextBox();
			this.keyboardsA = new System.Windows.Forms.ComboBox();
			this.keyboardsB = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.keyboardsC = new System.Windows.Forms.ComboBox();
			this.testAreaC = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// testAreaA
			//
			this.testAreaA.Location = new System.Drawing.Point(12, 33);
			this.testAreaA.Name = "testAreaA";
			this.testAreaA.Size = new System.Drawing.Size(173, 20);
			this.testAreaA.TabIndex = 0;
			this.testAreaA.Enter += new System.EventHandler(this.testAreaA_Enter);
			//
			// testAreaB
			//
			this.testAreaB.Location = new System.Drawing.Point(12, 87);
			this.testAreaB.Name = "testAreaB";
			this.testAreaB.Size = new System.Drawing.Size(173, 20);
			this.testAreaB.TabIndex = 1;
			this.testAreaB.Enter += new System.EventHandler(this.testAreaB_Enter);
			//
			// keyboardsA
			//
			this.keyboardsA.FormattingEnabled = true;
			this.keyboardsA.Location = new System.Drawing.Point(191, 33);
			this.keyboardsA.Name = "keyboardsA";
			this.keyboardsA.Size = new System.Drawing.Size(174, 21);
			this.keyboardsA.TabIndex = 2;
			//
			// keyboardsB
			//
			this.keyboardsB.FormattingEnabled = true;
			this.keyboardsB.Location = new System.Drawing.Point(191, 87);
			this.keyboardsB.Name = "keyboardsB";
			this.keyboardsB.Size = new System.Drawing.Size(174, 21);
			this.keyboardsB.TabIndex = 3;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Test Area A";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 71);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Test Area B";
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(188, 17);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(77, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Keyboard for A";
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(188, 71);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(77, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Keyboard for B";
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(188, 131);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(77, 13);
			this.label5.TabIndex = 11;
			this.label5.Text = "Keyboard for C";
			//
			// label6
			//
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(9, 131);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(63, 13);
			this.label6.TabIndex = 10;
			this.label6.Text = "Test Area C";
			//
			// keyboardsC
			//
			this.keyboardsC.FormattingEnabled = true;
			this.keyboardsC.Location = new System.Drawing.Point(191, 147);
			this.keyboardsC.Name = "keyboardsC";
			this.keyboardsC.Size = new System.Drawing.Size(174, 21);
			this.keyboardsC.TabIndex = 9;
			//
			// testAreaC
			//
			this.testAreaC.Location = new System.Drawing.Point(12, 147);
			this.testAreaC.Name = "testAreaC";
			this.testAreaC.Size = new System.Drawing.Size(173, 20);
			this.testAreaC.TabIndex = 8;
			this.testAreaC.Enter += new System.EventHandler(this.testAreaC_Enter);
			//
			// KeyboardForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(383, 228);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.keyboardsC);
			this.Controls.Add(this.testAreaC);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.keyboardsB);
			this.Controls.Add(this.keyboardsA);
			this.Controls.Add(this.testAreaB);
			this.Controls.Add(this.testAreaA);
			this.Name = "KeyboardForm";
			this.Text = "Keyboard Test App";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox testAreaA;
		private System.Windows.Forms.TextBox testAreaB;
		private System.Windows.Forms.ComboBox keyboardsA;
		private System.Windows.Forms.ComboBox keyboardsB;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox keyboardsC;
		private System.Windows.Forms.TextBox testAreaC;
	}
}
