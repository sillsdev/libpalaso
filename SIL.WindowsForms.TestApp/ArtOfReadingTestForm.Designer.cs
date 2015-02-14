namespace SIL.WindowsForms.TestApp
{
	partial class ArtOfReadingTestForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.RootImagePath = new System.Windows.Forms.TextBox();
			this.btnPictureChooser = new System.Windows.Forms.Button();
			this.Result = new System.Windows.Forms.Label();
			this._useGeckoVersion = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(90, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Root Image Path:";
			// 
			// RootImagePath
			// 
			this.RootImagePath.Location = new System.Drawing.Point(110, 13);
			this.RootImagePath.Name = "RootImagePath";
			this.RootImagePath.Size = new System.Drawing.Size(162, 20);
			this.RootImagePath.TabIndex = 1;
			// 
			// btnPictureChooser
			// 
			this.btnPictureChooser.Location = new System.Drawing.Point(16, 39);
			this.btnPictureChooser.Name = "btnPictureChooser";
			this.btnPictureChooser.Size = new System.Drawing.Size(256, 23);
			this.btnPictureChooser.TabIndex = 2;
			this.btnPictureChooser.Text = "Picture Chooser";
			this.btnPictureChooser.UseVisualStyleBackColor = true;
			this.btnPictureChooser.Click += new System.EventHandler(this.OnPictureChooserClicked);
			// 
			// Result
			// 
			this.Result.AutoSize = true;
			this.Result.Location = new System.Drawing.Point(13, 109);
			this.Result.Name = "Result";
			this.Result.Size = new System.Drawing.Size(37, 13);
			this.Result.TabIndex = 3;
			this.Result.Text = "Result";
			// 
			// _useGeckoVersion
			// 
			this._useGeckoVersion.AutoSize = true;
			this._useGeckoVersion.Location = new System.Drawing.Point(16, 68);
			this._useGeckoVersion.Name = "_useGeckoVersion";
			this._useGeckoVersion.Size = new System.Drawing.Size(117, 17);
			this._useGeckoVersion.TabIndex = 4;
			this._useGeckoVersion.Text = "Use Gecko version";
			this._useGeckoVersion.UseVisualStyleBackColor = true;
			// 
			// ArtOfReadingTestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 131);
			this.Controls.Add(this._useGeckoVersion);
			this.Controls.Add(this.Result);
			this.Controls.Add(this.btnPictureChooser);
			this.Controls.Add(this.RootImagePath);
			this.Controls.Add(this.label1);
			this.Name = "ArtOfReadingTestForm";
			this.Text = "ArtOfReadingTestForm";
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox RootImagePath;
		private System.Windows.Forms.Button btnPictureChooser;
		private System.Windows.Forms.Label Result;
		private System.Windows.Forms.CheckBox _useGeckoVersion;
	}
}