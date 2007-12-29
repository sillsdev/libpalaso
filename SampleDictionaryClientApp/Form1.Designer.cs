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
			this._dictionaryPath = new System.Windows.Forms.TextBox();
			this._writingSystemId = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _lookupButton
			//
			this._lookupButton.Location = new System.Drawing.Point(205, 59);
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
			this._entryViewer.Location = new System.Drawing.Point(8, 95);
			this._entryViewer.MinimumSize = new System.Drawing.Size(20, 20);
			this._entryViewer.Name = "_entryViewer";
			this._entryViewer.Size = new System.Drawing.Size(272, 202);
			this._entryViewer.TabIndex = 1;
			//
			// _word
			//
			this._word.Location = new System.Drawing.Point(102, 62);
			this._word.Name = "_word";
			this._word.Size = new System.Drawing.Size(97, 20);
			this._word.TabIndex = 2;
			this._word.Text = "mango";
			//
			// _dictionaryPath
			//
			this._dictionaryPath.Location = new System.Drawing.Point(8, 12);
			this._dictionaryPath.Name = "_dictionaryPath";
			this._dictionaryPath.Size = new System.Drawing.Size(291, 20);
			this._dictionaryPath.TabIndex = 2;
			this._dictionaryPath.Text = "E:\\Users\\John\\Documents\\WeSay\\NooSupu\\noosupu.lift";
			//
			// _writingSystemId
			//
			this._writingSystemId.Location = new System.Drawing.Point(102, 33);
			this._writingSystemId.Name = "_writingSystemId";
			this._writingSystemId.Size = new System.Drawing.Size(97, 20);
			this._writingSystemId.TabIndex = 2;
			this._writingSystemId.Text = "xgs";
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 39);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(89, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Writing System Id";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 65);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(33, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Word";
			//
			// Form1
			//
			this.AcceptButton = this._lookupButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(297, 322);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._dictionaryPath);
			this.Controls.Add(this._writingSystemId);
			this.Controls.Add(this._word);
			this.Controls.Add(this._entryViewer);
			this.Controls.Add(this._lookupButton);
			this.Name = "Form1";
			this.Text = "Sample Dictionary Client";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _lookupButton;
		private System.Windows.Forms.WebBrowser _entryViewer;
		private System.Windows.Forms.TextBox _word;
		private System.Windows.Forms.TextBox _dictionaryPath;
		private System.Windows.Forms.TextBox _writingSystemId;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
	}
}
