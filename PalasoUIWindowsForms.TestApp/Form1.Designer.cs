namespace PalasoUIWindowsForms.TestApp
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
			this.dialog = new Palaso.UI.WindowsForms.WritingSystems.WSPropertiesDialog();
			this.SuspendLayout();
			//
			// dialog
			//
			this.dialog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dialog.Location = new System.Drawing.Point(0, 0);
			this.dialog.Name = "dialog";
			this.dialog.Size = new System.Drawing.Size(703, 413);
			this.dialog.TabIndex = 0;
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(703, 413);
			this.Controls.Add(this.dialog);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private Palaso.UI.WindowsForms.WritingSystems.WSPropertiesDialog dialog;
	}
}
