namespace ClipboardTestApp
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
            this.btnCopyText = new System.Windows.Forms.Button();
            this.btnPasteText = new System.Windows.Forms.Button();
            this.btnCopyImage = new System.Windows.Forms.Button();
            this.btnPasteImage = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelContainsText = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelContainsImage = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            //
            // btnCopyText
            //
            this.btnCopyText.Location = new System.Drawing.Point(13, 11);
            this.btnCopyText.Name = "btnCopyText";
            this.btnCopyText.Size = new System.Drawing.Size(75, 23);
            this.btnCopyText.TabIndex = 0;
            this.btnCopyText.Text = "Copy Text";
            this.btnCopyText.UseVisualStyleBackColor = true;
            this.btnCopyText.Click += new System.EventHandler(this.OnCopyText);
            //
            // btnPasteText
            //
            this.btnPasteText.Location = new System.Drawing.Point(94, 11);
            this.btnPasteText.Name = "btnPasteText";
            this.btnPasteText.Size = new System.Drawing.Size(75, 23);
            this.btnPasteText.TabIndex = 1;
            this.btnPasteText.Text = "Paste Text";
            this.btnPasteText.UseVisualStyleBackColor = true;
            this.btnPasteText.Click += new System.EventHandler(this.OnPasteText);
            //
            // btnCopyImage
            //
            this.btnCopyImage.Location = new System.Drawing.Point(175, 11);
            this.btnCopyImage.Name = "btnCopyImage";
            this.btnCopyImage.Size = new System.Drawing.Size(75, 23);
            this.btnCopyImage.TabIndex = 2;
            this.btnCopyImage.Text = "Copy Image";
            this.btnCopyImage.UseVisualStyleBackColor = true;
            this.btnCopyImage.Click += new System.EventHandler(this.OnCopyImage);
            //
            // btnPasteImage
            //
            this.btnPasteImage.Location = new System.Drawing.Point(256, 11);
            this.btnPasteImage.Name = "btnPasteImage";
            this.btnPasteImage.Size = new System.Drawing.Size(75, 23);
            this.btnPasteImage.TabIndex = 1;
            this.btnPasteImage.Text = "Paste Image";
            this.btnPasteImage.UseVisualStyleBackColor = true;
            this.btnPasteImage.Click += new System.EventHandler(this.OnPasteImage);
            //
            // textBox1
            //
            this.textBox1.Location = new System.Drawing.Point(16, 57);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(223, 100);
            this.textBox1.TabIndex = 3;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Text:";
            //
            // pictureBox1
            //
            this.pictureBox1.Location = new System.Drawing.Point(19, 180);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(220, 135);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 164);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Image:";
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(253, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Clipboard contains text:";
            //
            // labelContainsText
            //
            this.labelContainsText.AutoSize = true;
            this.labelContainsText.Location = new System.Drawing.Point(256, 58);
            this.labelContainsText.Name = "labelContainsText";
            this.labelContainsText.Size = new System.Drawing.Size(25, 13);
            this.labelContainsText.TabIndex = 8;
            this.labelContainsText.Text = "???";
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(256, 180);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Clipboard contains Image:";
            //
            // labelContainsImage
            //
            this.labelContainsImage.AutoSize = true;
            this.labelContainsImage.Location = new System.Drawing.Point(256, 197);
            this.labelContainsImage.Name = "labelContainsImage";
            this.labelContainsImage.Size = new System.Drawing.Size(25, 13);
            this.labelContainsImage.TabIndex = 10;
            this.labelContainsImage.Text = "???";
            //
            // btnClose
            //
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(322, 335);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 11;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += OnClose;
            // Form1
            //
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 370);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.labelContainsImage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelContainsText);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnCopyImage);
            this.Controls.Add(this.btnPasteImage);
            this.Controls.Add(this.btnPasteText);
            this.Controls.Add(this.btnCopyText);
            this.Name = "Form1";
            this.Text = "Clipboard Test App";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private System.Windows.Forms.Button btnCopyText;

		#endregion

		private System.Windows.Forms.Button btnPasteText;
		private System.Windows.Forms.Button btnCopyImage;
		private System.Windows.Forms.Button btnPasteImage;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label labelContainsText;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label labelContainsImage;
		private System.Windows.Forms.Button btnClose;
	}
}