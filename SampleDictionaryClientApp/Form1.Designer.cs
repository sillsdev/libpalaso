namespace SampleDictionaryClientApp
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
			this._lookupButton = new System.Windows.Forms.Button();
			this._entryViewer = new System.Windows.Forms.WebBrowser();
			this._word = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// _lookupButton
			//
			this._lookupButton.Location = new System.Drawing.Point(205, 14);
			this._lookupButton.Name = "_lookupButton";
			this._lookupButton.Size = new System.Drawing.Size(75, 23);
			this._lookupButton.TabIndex = 0;
			this._lookupButton.Text = "Lookup";
			this._lookupButton.UseVisualStyleBackColor = true;
			this._lookupButton.Click += new System.EventHandler(this._lookupButton_Click);
			//
			// _entryViewer
			//
			this._entryViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._entryViewer.Location = new System.Drawing.Point(8, 60);
			this._entryViewer.MinimumSize = new System.Drawing.Size(20, 20);
			this._entryViewer.Name = "_entryViewer";
			this._entryViewer.Size = new System.Drawing.Size(272, 179);
			this._entryViewer.TabIndex = 1;
			//
			// _word
			//
			this._word.Location = new System.Drawing.Point(8, 17);
			this._word.Name = "_word";
			this._word.Size = new System.Drawing.Size(191, 20);
			this._word.TabIndex = 2;
			this._word.Text = "mango";
			//
			// Form1
			//
			this.AcceptButton = this._lookupButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(297, 264);
			this.Controls.Add(this._word);
			this.Controls.Add(this._entryViewer);
			this.Controls.Add(this._lookupButton);
			this.Name = "Form1";
			this.Text = "Sample Dictionary Client";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _lookupButton;
		private System.Windows.Forms.WebBrowser _entryViewer;
		private System.Windows.Forms.TextBox _word;
	}
}
