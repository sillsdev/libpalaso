namespace SampleDictionaryServicesApplication
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
			this._logBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// _logBox
			//
			this._logBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._logBox.Location = new System.Drawing.Point(0, 0);
			this._logBox.Multiline = true;
			this._logBox.Name = "_logBox";
			this._logBox.Size = new System.Drawing.Size(284, 264);
			this._logBox.TabIndex = 0;
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 264);
			this.Controls.Add(this._logBox);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _logBox;
	}
}
