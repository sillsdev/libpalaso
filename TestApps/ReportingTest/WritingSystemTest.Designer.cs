namespace TestApp
{
	partial class WritingSystemTest
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
			this.wsPropertiesPanel1 = new SIL.WritingSystems.WindowsForms.WritingSystemSetupView();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// wsPropertiesPanel1
			//
			this.wsPropertiesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wsPropertiesPanel1.Location = new System.Drawing.Point(0, 0);
			this.wsPropertiesPanel1.Name = "wsPropertiesPanel1";
			this.wsPropertiesPanel1.Size = new System.Drawing.Size(692, 460);
			this.wsPropertiesPanel1.TabIndex = 1;
			//
			// button1
			//
			this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
			this.button1.Location = new System.Drawing.Point(549, 425);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(131, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "Save All Ws Changes";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			//
			// WritingSystemTest
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(692, 460);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.wsPropertiesPanel1);
			this.Name = "WritingSystemTest";
			this.Text = "WritingSystemTest";
			this.ResumeLayout(false);

		}

		#endregion

		private SIL.WritingSystems.WindowsForms.WritingSystemSetupView wsPropertiesPanel1;
		private System.Windows.Forms.Button button1;

	}
}