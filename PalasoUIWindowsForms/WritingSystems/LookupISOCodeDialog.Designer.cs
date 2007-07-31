namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class LookupISOCodeDialog
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
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this._okButton = new System.Windows.Forms.Button();
			this._aboutLink = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			//
			// listView1
			//
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader1,
			this.columnHeader2});
			this.listView1.FullRowSelect = true;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(0, -1);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(292, 226);
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.VirtualMode = true;
			this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			this.listView1.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listView1_RetrieveVirtualItem);
			//
			// columnHeader1
			//
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 163;
			//
			// columnHeader2
			//
			this.columnHeader2.Text = "Code";
			//
			// _okButton
			//
			this._okButton.Location = new System.Drawing.Point(205, 231);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _aboutLink
			//
			this._aboutLink.AutoSize = true;
			this._aboutLink.Location = new System.Drawing.Point(12, 236);
			this._aboutLink.Name = "_aboutLink";
			this._aboutLink.Size = new System.Drawing.Size(119, 13);
			this._aboutLink.TabIndex = 2;
			this._aboutLink.TabStop = true;
			this._aboutLink.Text = "About ISO 639-3 Codes";
			this._aboutLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._aboutLink_LinkClicked);
			//
			// LookupISOCodeDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this._aboutLink);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.listView1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LookupISOCodeDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Lookup ISO Code...";
			this.Load += new System.EventHandler(this.LookupISOCodeDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.LinkLabel _aboutLink;
	}
}