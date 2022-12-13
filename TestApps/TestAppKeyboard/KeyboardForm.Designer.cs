using SIL.Windows.Forms.Keyboarding;

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
			KeyboardController.Shutdown();
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
			System.Windows.Forms.Label label8;
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cbOnActivate = new System.Windows.Forms.CheckBox();
			this.cbOnEnter = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.currentKeyboard = new System.Windows.Forms.ComboBox();
			this.lblCurrentKeyboard = new System.Windows.Forms.Label();
			label8 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Location = new System.Drawing.Point(97, 276);
			label8.Name = "label8";
			label8.Size = new System.Drawing.Size(88, 13);
			label8.TabIndex = 12;
			label8.Text = "Current keyboard";
			label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
			this.keyboardsA.TabIndex = 4;
			// 
			// keyboardsB
			// 
			this.keyboardsB.FormattingEnabled = true;
			this.keyboardsB.Location = new System.Drawing.Point(191, 87);
			this.keyboardsB.Name = "keyboardsB";
			this.keyboardsB.Size = new System.Drawing.Size(174, 21);
			this.keyboardsB.TabIndex = 5;
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
			this.label5.Location = new System.Drawing.Point(188, 126);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(77, 13);
			this.label5.TabIndex = 11;
			this.label5.Text = "Keyboard for C";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(9, 126);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(63, 13);
			this.label6.TabIndex = 10;
			this.label6.Text = "Test Area C";
			// 
			// keyboardsC
			// 
			this.keyboardsC.FormattingEnabled = true;
			this.keyboardsC.Location = new System.Drawing.Point(191, 142);
			this.keyboardsC.Name = "keyboardsC";
			this.keyboardsC.Size = new System.Drawing.Size(174, 21);
			this.keyboardsC.TabIndex = 6;
			// 
			// testAreaC
			// 
			this.testAreaC.Location = new System.Drawing.Point(12, 142);
			this.testAreaC.Name = "testAreaC";
			this.testAreaC.Size = new System.Drawing.Size(173, 20);
			this.testAreaC.TabIndex = 2;
			this.testAreaC.Enter += new System.EventHandler(this.testAreaC_Enter);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cbOnActivate);
			this.groupBox1.Controls.Add(this.cbOnEnter);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.currentKeyboard);
			this.groupBox1.Location = new System.Drawing.Point(12, 174);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(353, 99);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Controls";
			// 
			// cbOnActivate
			// 
			this.cbOnActivate.AutoSize = true;
			this.cbOnActivate.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cbOnActivate.Location = new System.Drawing.Point(47, 69);
			this.cbOnActivate.Name = "cbOnActivate";
			this.cbOnActivate.Size = new System.Drawing.Size(145, 17);
			this.cbOnActivate.TabIndex = 2;
			this.cbOnActivate.Text = "Set keyboard on activate";
			this.cbOnActivate.UseVisualStyleBackColor = true;
			// 
			// cbOnEnter
			// 
			this.cbOnEnter.AutoSize = true;
			this.cbOnEnter.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cbOnEnter.Location = new System.Drawing.Point(61, 46);
			this.cbOnEnter.Name = "cbOnEnter";
			this.cbOnEnter.Size = new System.Drawing.Size(131, 17);
			this.cbOnEnter.TabIndex = 1;
			this.cbOnEnter.Text = "Set keyboard on enter";
			this.cbOnEnter.UseVisualStyleBackColor = true;
			this.cbOnEnter.Checked = true;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(67, 22);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(106, 13);
			this.label7.TabIndex = 1;
			this.label7.Text = "Set current keyboard";
			// 
			// currentKeyboard
			// 
			this.currentKeyboard.FormattingEnabled = true;
			this.currentKeyboard.Location = new System.Drawing.Point(179, 19);
			this.currentKeyboard.Name = "currentKeyboard";
			this.currentKeyboard.Size = new System.Drawing.Size(168, 21);
			this.currentKeyboard.TabIndex = 0;
			// 
			// lblCurrentKeyboard
			// 
			this.lblCurrentKeyboard.AutoSize = true;
			this.lblCurrentKeyboard.Location = new System.Drawing.Point(192, 276);
			this.lblCurrentKeyboard.Name = "lblCurrentKeyboard";
			this.lblCurrentKeyboard.Size = new System.Drawing.Size(0, 13);
			this.lblCurrentKeyboard.TabIndex = 13;
			this.lblCurrentKeyboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// KeyboardForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(383, 302);
			this.Controls.Add(this.lblCurrentKeyboard);
			this.Controls.Add(label8);
			this.Controls.Add(this.groupBox1);
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
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
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
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox currentKeyboard;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox cbOnEnter;
		private System.Windows.Forms.CheckBox cbOnActivate;
		private System.Windows.Forms.Label lblCurrentKeyboard;
	}
}
