namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class FontControl
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
			this._fontDialog = new System.Windows.Forms.FontDialog();
			this.label1 = new System.Windows.Forms.Label();
			this._sampleTextBox = new System.Windows.Forms.TextBox();
			this._fontFamilyCombo = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _fontDialog
			//
			this._fontDialog.ShowEffects = false;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 59);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Text Sample Area:";
			//
			// _sampleTextBox
			//
			this._sampleTextBox.AcceptsReturn = true;
			this._sampleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._sampleTextBox.BackColor = System.Drawing.Color.White;
			this._sampleTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._sampleTextBox.Location = new System.Drawing.Point(13, 75);
			this._sampleTextBox.Multiline = true;
			this._sampleTextBox.Name = "_sampleTextBox";
			this._sampleTextBox.Size = new System.Drawing.Size(242, 20);
			this._sampleTextBox.TabIndex = 6;
			//
			// _fontFamilyCombo
			//
			this._fontFamilyCombo.FormattingEnabled = true;
			this._fontFamilyCombo.Location = new System.Drawing.Point(13, 25);
			this._fontFamilyCombo.Name = "_fontFamilyCombo";
			this._fontFamilyCombo.Size = new System.Drawing.Size(242, 21);
			this._fontFamilyCombo.TabIndex = 7;
			this._fontFamilyCombo.SelectedIndexChanged += new System.EventHandler(this._fontFamilyCombo_SelectedIndexChanged);
			this._fontFamilyCombo.TextChanged += new System.EventHandler(this._fontFamilyCombo_TextChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Font Family:";
			//
			// FontControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._fontFamilyCombo);
			this.Controls.Add(this._sampleTextBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FontControl";
			this.Size = new System.Drawing.Size(258, 150);
			this.Load += new System.EventHandler(this.FontControl_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FontDialog _fontDialog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _sampleTextBox;
		private System.Windows.Forms.ComboBox _fontFamilyCombo;
		private System.Windows.Forms.Label label2;
	}
}