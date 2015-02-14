namespace SIL.WindowsForms.WritingSystems
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
			this._spellCheckingIdComboBox = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Spell Checking ID:";
			//
			// _spellCheckingIdComboBox
			//
			this._spellCheckingIdComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._spellCheckingIdComboBox.FormattingEnabled = true;
			this._spellCheckingIdComboBox.Location = new System.Drawing.Point(114, 22);
			this._spellCheckingIdComboBox.Name = "_spellCheckingIdComboBox";
			this._spellCheckingIdComboBox.Size = new System.Drawing.Size(294, 21);
			this._spellCheckingIdComboBox.TabIndex = 2;
			this._spellCheckingIdComboBox.SelectedIndexChanged += new System.EventHandler(this._spellCheckingIdComboBox_SelectedIndexChanged);
			//
			// WSSpellingControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._spellCheckingIdComboBox);
			this.Controls.Add(this.label1);
			this.Name = "WSSpellingControl";
			this.Size = new System.Drawing.Size(421, 250);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox _spellCheckingIdComboBox;
	}
}
