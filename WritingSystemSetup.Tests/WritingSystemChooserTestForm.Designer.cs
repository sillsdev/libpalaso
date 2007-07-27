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
			this.pickerUsingListView1.Size = new System.Drawing.Size(277, 194);
			this.pickerUsingListView1.TabIndex = 0;
			//
			// WritingSystemChooserTestForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 209);
			this.Controls.Add(this.pickerUsingListView1);
			this.Name = "WritingSystemChooserTestForm";
			this.Text = "WritingSystemChooserTestForm";
			this.Load += new System.EventHandler(this.WritingSystemChooserTestForm_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private Palaso.UI.WindowsForms.WritingSystems.PickerUsingListView pickerUsingListView1;

	}
}