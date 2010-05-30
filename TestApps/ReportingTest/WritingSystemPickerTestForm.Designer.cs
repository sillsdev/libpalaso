namespace TestApp
{
	partial class WritingSystemPickerTestForm
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
			this.wsPickerUsingListView1 = new Palaso.UI.WindowsForms.WritingSystems.WSPickerUsingListView();
			this._currentWsLabel = new System.Windows.Forms.Label();
			this._editWsLink = new System.Windows.Forms.LinkLabel();
			this.pickerUsingComboBox1 = new Palaso.UI.WindowsForms.WritingSystems.WSPickerUsingComboBox(TODO);
			this.SuspendLayout();
			//
			// wsPickerUsingListView1
			//
			this.wsPickerUsingListView1.Location = new System.Drawing.Point(0, 12);
			this.wsPickerUsingListView1.Name = "wsPickerUsingListView1";
			this.wsPickerUsingListView1.SelectedIndex = -1;
			this.wsPickerUsingListView1.Size = new System.Drawing.Size(103, 216);
			this.wsPickerUsingListView1.TabIndex = 0;
			//
			// _currentWsLabel
			//
			this._currentWsLabel.AutoSize = true;
			this._currentWsLabel.Location = new System.Drawing.Point(21, 242);
			this._currentWsLabel.Name = "_currentWsLabel";
			this._currentWsLabel.Size = new System.Drawing.Size(58, 13);
			this._currentWsLabel.TabIndex = 1;
			this._currentWsLabel.Text = "description";
			//
			// _editWsLink
			//
			this._editWsLink.AutoSize = true;
			this._editWsLink.Location = new System.Drawing.Point(148, 235);
			this._editWsLink.Name = "_editWsLink";
			this._editWsLink.Size = new System.Drawing.Size(112, 13);
			this._editWsLink.TabIndex = 2;
			this._editWsLink.TabStop = true;
			this._editWsLink.Text = "Edit Writing Systems...";
			this._editWsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnEditWsLink_LinkClicked);
			//
			// pickerUsingComboBox1
			//
			this.pickerUsingComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.pickerUsingComboBox1.FormattingEnabled = true;
			this.pickerUsingComboBox1.Items.AddRange(new object[] {
			"---",
			"More..."});
			this.pickerUsingComboBox1.Location = new System.Drawing.Point(165, 23);
			this.pickerUsingComboBox1.Name = "pickerUsingComboBox1";
			this.pickerUsingComboBox1.Size = new System.Drawing.Size(121, 21);
			this.pickerUsingComboBox1.TabIndex = 3;
			//
			// WritingSystemPickerTestForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(491, 303);
			this.Controls.Add(this.pickerUsingComboBox1);
			this.Controls.Add(this._editWsLink);
			this.Controls.Add(this._currentWsLabel);
			this.Controls.Add(this.wsPickerUsingListView1);
			this.Name = "WritingSystemPickerTestForm";
			this.Text = "WritingSystemPickerForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Palaso.UI.WindowsForms.WritingSystems.WSPickerUsingListView wsPickerUsingListView1;
		private System.Windows.Forms.Label _currentWsLabel;
		private System.Windows.Forms.LinkLabel _editWsLink;
		private Palaso.UI.WindowsForms.WritingSystems.WSPickerUsingComboBox pickerUsingComboBox1;
	}
}