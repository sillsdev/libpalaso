namespace FastSplitterTest
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
			this.button1 = new System.Windows.Forms.Button();
			this._tbXmlFile = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this._tbOptionFirstElementMarker = new System.Windows.Forms.TextBox();
			this._tbRepeatingElementMarker = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// button1
			//
			this.button1.Location = new System.Drawing.Point(13, 69);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(157, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Browse For Xml File...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.BrowseClicked);
			//
			// _tbXmlFile
			//
			this._tbXmlFile.Enabled = false;
			this._tbXmlFile.Location = new System.Drawing.Point(13, 99);
			this._tbXmlFile.Name = "_tbXmlFile";
			this._tbXmlFile.Size = new System.Drawing.Size(259, 20);
			this._tbXmlFile.TabIndex = 1;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(143, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Optional First element marker";
			//
			// _tbOptionFirstElementMarker
			//
			this._tbOptionFirstElementMarker.Location = new System.Drawing.Point(163, 4);
			this._tbOptionFirstElementMarker.Name = "_tbOptionFirstElementMarker";
			this._tbOptionFirstElementMarker.Size = new System.Drawing.Size(207, 20);
			this._tbOptionFirstElementMarker.TabIndex = 3;
			//
			// _tbRepeatingElementMarker
			//
			this._tbRepeatingElementMarker.Location = new System.Drawing.Point(163, 36);
			this._tbRepeatingElementMarker.Name = "_tbRepeatingElementMarker";
			this._tbRepeatingElementMarker.Size = new System.Drawing.Size(207, 20);
			this._tbRepeatingElementMarker.TabIndex = 5;
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(131, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Repeating element marker";
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 174);
			this.Controls.Add(this._tbRepeatingElementMarker);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._tbOptionFirstElementMarker);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._tbXmlFile);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "Fast Xml Splitter Test app";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox _tbXmlFile;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _tbOptionFirstElementMarker;
		private System.Windows.Forms.TextBox _tbRepeatingElementMarker;
		private System.Windows.Forms.Label label2;
	}
}
