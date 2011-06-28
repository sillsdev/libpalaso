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
			this.components = new System.ComponentModel.Container();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this._okButton = new System.Windows.Forms.Button();
			this._aboutLink = new System.Windows.Forms.LinkLabel();
			this._aboutLink639_1 = new System.Windows.Forms.LinkLabel();
			this._searchText = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this._searchTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			//
			// listView1
			//
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader1,
			this.columnHeader2});
			this.listView1.FullRowSelect = true;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(12, 46);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(280, 208);
			this.listView1.TabIndex = 1;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
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
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._okButton.Location = new System.Drawing.Point(217, 288);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _aboutLink
			//
			this._aboutLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._aboutLink.Location = new System.Drawing.Point(12, 301);
			this._aboutLink.Name = "_aboutLink";
			this._aboutLink.Size = new System.Drawing.Size(196, 14);
			this._aboutLink.TabIndex = 2;
			this._aboutLink.TabStop = true;
			this._aboutLink.Text = "About Language 639-3 Codes";
			this._aboutLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._aboutLink_LinkClicked);
			//
			// _aboutLink639_1
			//
			this._aboutLink639_1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._aboutLink639_1.Location = new System.Drawing.Point(12, 273);
			this._aboutLink639_1.Name = "_aboutLink639_1";
			this._aboutLink639_1.Size = new System.Drawing.Size(196, 14);
			this._aboutLink639_1.TabIndex = 3;
			this._aboutLink639_1.TabStop = true;
			this._aboutLink639_1.Text = "About Language 639-1 Codes";
			this._aboutLink639_1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._aboutLink639_1_LinkClicked);
			//
			// _searchText
			//
			this._searchText.Location = new System.Drawing.Point(83, 12);
			this._searchText.Name = "_searchText";
			this._searchText.Size = new System.Drawing.Size(209, 20);
			this._searchText.TabIndex = 0;
			//
			// label1
			//
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 19);
			this.label1.TabIndex = 5;
			this.label1.Text = "Search";
			//
			// _searchTimer
			//
			this._searchTimer.Tick += new System.EventHandler(this._searchTimer_Tick);
			//
			// LookupISOCodeDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(309, 323);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._searchText);
			this.Controls.Add(this._aboutLink639_1);
			this.Controls.Add(this._aboutLink);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.listView1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LookupISOCodeDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Lookup Language Code...";
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
		private System.Windows.Forms.LinkLabel _aboutLink639_1;
		private System.Windows.Forms.TextBox _searchText;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Timer _searchTimer;
	}
}