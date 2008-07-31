namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSSpellingControl
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
			this.label1 = new System.Windows.Forms.Label();
			this._spellCheckingIdTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Spell Checking ID:";
			//
			// _spellCheckingIdTextBox
			//
			this._spellCheckingIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._spellCheckingIdTextBox.Location = new System.Drawing.Point(114, 3);
			this._spellCheckingIdTextBox.Name = "_spellCheckingIdTextBox";
			this._spellCheckingIdTextBox.Size = new System.Drawing.Size(304, 20);
			this._spellCheckingIdTextBox.TabIndex = 1;
			this._spellCheckingIdTextBox.TextChanged += new System.EventHandler(this._spellCheckingIdTextBox_TextChanged);
			//
			// WSSpellingControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._spellCheckingIdTextBox);
			this.Controls.Add(this.label1);
			this.Name = "WSSpellingControl";
			this.Size = new System.Drawing.Size(421, 250);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _spellCheckingIdTextBox;
	}
}
