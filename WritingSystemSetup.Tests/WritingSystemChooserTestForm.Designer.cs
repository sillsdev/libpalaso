namespace WritingSystemSetup.Tests
{
	partial class WritingSystemChooserTestForm
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
			this.pickerUsingListView1 = new Palaso.UI.WindowsForms.WritingSystems.PickerUsingListView();
			this.pickerUsingComboBox1 = new Palaso.UI.WindowsForms.WritingSystems.PickerUsingComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// pickerUsingListView1
			//
			this.pickerUsingListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.pickerUsingListView1.IdentifierOfSelectedWritingSystem = "fbr-Thai";
			this.pickerUsingListView1.Location = new System.Drawing.Point(3, 3);
			this.pickerUsingListView1.Name = "pickerUsingListView1";
			this.pickerUsingListView1.Size = new System.Drawing.Size(277, 202);
			this.pickerUsingListView1.TabIndex = 0;
			this.pickerUsingListView1.DoubleClicked += new System.EventHandler(this.pickerUsingListView1_DoubleClicked);
			//
			// pickerUsingComboBox1
			//
			this.pickerUsingComboBox1.FormattingEnabled = true;
			this.pickerUsingComboBox1.IdentifierOfSelectedWritingSystem = null;
			this.pickerUsingComboBox1.Location = new System.Drawing.Point(89, 233);
			this.pickerUsingComboBox1.Name = "pickerUsingComboBox1";
			this.pickerUsingComboBox1.Size = new System.Drawing.Size(121, 21);
			this.pickerUsingComboBox1.TabIndex = 1;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 236);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Writing System";
			//
			// WritingSystemChooserTestForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 298);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pickerUsingComboBox1);
			this.Controls.Add(this.pickerUsingListView1);
			this.Name = "WritingSystemChooserTestForm";
			this.Text = "WritingSystemChooserTestForm";
			this.Load += new System.EventHandler(this.WritingSystemChooserTestForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Palaso.UI.WindowsForms.WritingSystems.PickerUsingListView pickerUsingListView1;
		private Palaso.UI.WindowsForms.WritingSystems.PickerUsingComboBox pickerUsingComboBox1;
		private System.Windows.Forms.Label label1;

	}
}