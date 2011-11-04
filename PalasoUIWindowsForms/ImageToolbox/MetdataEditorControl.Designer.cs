namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class MetdataEditorControl
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
			this.label3 = new System.Windows.Forms.Label();
			this._copyright = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this._illustrator = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(29, 90);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(46, 15);
			this.label3.TabIndex = 9;
			this.label3.Text = "License";
			//
			// _copyright
			//
			this._copyright.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._copyright.Location = new System.Drawing.Point(90, 62);
			this._copyright.Name = "_copyright";
			this._copyright.Size = new System.Drawing.Size(163, 23);
			this._copyright.TabIndex = 8;
			this._copyright.Text = "2011 Sago Land";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(28, 63);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 15);
			this.label2.TabIndex = 7;
			this.label2.Text = "Copyright";
			//
			// _illustrator
			//
			this._illustrator.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._illustrator.Location = new System.Drawing.Point(90, 35);
			this._illustrator.Name = "_illustrator";
			this._illustrator.Size = new System.Drawing.Size(163, 23);
			this._illustrator.TabIndex = 6;
			this._illustrator.Text = "Artist Annie";
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(28, 36);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 15);
			this.label1.TabIndex = 5;
			this.label1.Text = "Illustrator";
			//
			// MetdataEditorControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label3);
			this.Controls.Add(this._copyright);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._illustrator);
			this.Controls.Add(this.label1);
			this.Name = "MetdataEditorControl";
			this.Size = new System.Drawing.Size(499, 328);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _copyright;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox _illustrator;
		private System.Windows.Forms.Label label1;
	}
}
