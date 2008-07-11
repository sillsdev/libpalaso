namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class FontAndKeyboardControl
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
			this._keyboardCombo = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this._rightToLeftBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			//
			// _fontDialog
			//
			this._fontDialog.ShowEffects = false;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 122);
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
			this._sampleTextBox.Location = new System.Drawing.Point(13, 146);
			this._sampleTextBox.Multiline = true;
			this._sampleTextBox.Name = "_sampleTextBox";
			this._sampleTextBox.Size = new System.Drawing.Size(310, 54);
			this._sampleTextBox.TabIndex = 2;
			this._sampleTextBox.Enter += new System.EventHandler(this._sampleTextBox_Enter);
			this._sampleTextBox.Leave += new System.EventHandler(this._sampleTextBox_Leave);
			//
			// _fontFamilyCombo
			//
			this._fontFamilyCombo.FormattingEnabled = true;
			this._fontFamilyCombo.Location = new System.Drawing.Point(89, 25);
			this._fontFamilyCombo.Name = "_fontFamilyCombo";
			this._fontFamilyCombo.Size = new System.Drawing.Size(234, 21);
			this._fontFamilyCombo.TabIndex = 0;
			this._fontFamilyCombo.SelectedIndexChanged += new System.EventHandler(this._fontFamilyCombo_SelectedIndexChanged);
			this._fontFamilyCombo.TextChanged += new System.EventHandler(this._fontFamilyCombo_TextChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 28);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Font Family:";
			//
			// _keyboardCombo
			//
			this._keyboardCombo.FormattingEnabled = true;
			this._keyboardCombo.Location = new System.Drawing.Point(89, 52);
			this._keyboardCombo.Name = "_keyboardCombo";
			this._keyboardCombo.Size = new System.Drawing.Size(234, 21);
			this._keyboardCombo.TabIndex = 1;
			this._keyboardCombo.SelectedIndexChanged += new System.EventHandler(this._keyboardCombo_SelectedIndexChanged);
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(55, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Keyboard:";
			//
			// label4
			//
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(164, 122);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(159, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "You can test the keyboard here.";
			//
			// _rightToLeftBox
			//
			this._rightToLeftBox.AutoSize = true;
			this._rightToLeftBox.Location = new System.Drawing.Point(89, 79);
			this._rightToLeftBox.Name = "_rightToLeftBox";
			this._rightToLeftBox.Size = new System.Drawing.Size(115, 17);
			this._rightToLeftBox.TabIndex = 6;
			this._rightToLeftBox.Text = "Script is right to left";
			this._rightToLeftBox.UseVisualStyleBackColor = true;
			this._rightToLeftBox.CheckedChanged += new System.EventHandler(this._rightToLeftBox_CheckedChanged);
			//
			// FontAndKeyboardControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._rightToLeftBox);
			this.Controls.Add(this._keyboardCombo);
			this.Controls.Add(this._fontFamilyCombo);
			this.Controls.Add(this._sampleTextBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Name = "FontAndKeyboardControl";
			this.Size = new System.Drawing.Size(326, 214);
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FontDialog _fontDialog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _sampleTextBox;
		private System.Windows.Forms.ComboBox _fontFamilyCombo;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox _keyboardCombo;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox _rightToLeftBox;
	}
}