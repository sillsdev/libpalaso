using L10NSharp.UI;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class LookupISOControl
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
			this.components = new System.ComponentModel.Container();
			this._listView = new System.Windows.Forms.ListView();
			this.PrimaryNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.codeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.countryHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.AlternateNamesHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._searchText = new System.Windows.Forms.TextBox();
			this._searchTimer = new System.Windows.Forms.Timer(this.components);
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._cannotFindLanguageLink = new System.Windows.Forms.LinkLabel();
			this._desiredLanguageLabel = new System.Windows.Forms.Label();
			this._desiredLanguageDisplayName = new System.Windows.Forms.TextBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			//
			// _listView
			//
			this._listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.PrimaryNameHeader,
			this.codeHeader,
			this.countryHeader,
			this.AlternateNamesHeader});
			this._listView.FullRowSelect = true;
			this._listView.HideSelection = false;
			this._listView.Location = new System.Drawing.Point(0, 29);
			this._listView.Name = "_listView";
			this._listView.Size = new System.Drawing.Size(722, 237);
			this._listView.TabIndex = 1;
			this._listView.UseCompatibleStateImageBehavior = false;
			this._listView.View = System.Windows.Forms.View.Details;
			this._listView.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
			this._listView.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
			//
			// PrimaryNameHeader
			//
			this._L10NSharpExtender.SetLocalizableToolTip(this.PrimaryNameHeader, null);
			this._L10NSharpExtender.SetLocalizationComment(this.PrimaryNameHeader, null);
			this._L10NSharpExtender.SetLocalizingId(this.PrimaryNameHeader, "LanguageLookup.PrimaryNameHeader");
			this.PrimaryNameHeader.Text = "Name";
			this.PrimaryNameHeader.Width = 104;
			//
			// codeHeader
			//
			this._L10NSharpExtender.SetLocalizableToolTip(this.codeHeader, null);
			this._L10NSharpExtender.SetLocalizationComment(this.codeHeader, null);
			this._L10NSharpExtender.SetLocalizingId(this.codeHeader, "LanguageLookup.CodeHeader");
			this.codeHeader.Text = "Code";
			this.codeHeader.Width = 57;
			//
			// countryHeader
			//
			this._L10NSharpExtender.SetLocalizableToolTip(this.countryHeader, null);
			this._L10NSharpExtender.SetLocalizationComment(this.countryHeader, null);
			this._L10NSharpExtender.SetLocalizingId(this.countryHeader, "LanguageLookup.CountryHeader");
			this.countryHeader.Text = "Country";
			this.countryHeader.Width = 116;
			//
			// AlternateNamesHeader
			//
			this._L10NSharpExtender.SetLocalizableToolTip(this.AlternateNamesHeader, null);
			this._L10NSharpExtender.SetLocalizationComment(this.AlternateNamesHeader, null);
			this._L10NSharpExtender.SetLocalizingId(this.AlternateNamesHeader, "LanguageLookup.AlternateNamesHeader");
			this.AlternateNamesHeader.Text = "Other Names";
			this.AlternateNamesHeader.Width = 437;
			//
			// _searchText
			//
			this._L10NSharpExtender.SetLocalizableToolTip(this._searchText, null);
			this._L10NSharpExtender.SetLocalizationComment(this._searchText, null);
			this._L10NSharpExtender.SetLocalizingId(this._searchText, "LanguageLookup.LookupISOControl._searchText");
			this._searchText.Location = new System.Drawing.Point(1, 3);
			this._searchText.Name = "_searchText";
			this._searchText.Size = new System.Drawing.Size(228, 20);
			this._searchText.TabIndex = 0;
			//
			// _searchTimer
			//
			this._searchTimer.Tick += new System.EventHandler(this._searchTimer_Tick);
			//
			// pictureBox1
			//
			this.pictureBox1.BackColor = System.Drawing.Color.White;
			this.pictureBox1.Image = global::Palaso.UI.WindowsForms.Properties.Resources.search18x18;
			this._L10NSharpExtender.SetLocalizableToolTip(this.pictureBox1, null);
			this._L10NSharpExtender.SetLocalizationComment(this.pictureBox1, null);
			this._L10NSharpExtender.SetLocalizingId(this.pictureBox1, "LanguageLookup.LookupISOControl.pictureBox1");
			this.pictureBox1.Location = new System.Drawing.Point(210, 5);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(15, 15);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 11;
			this.pictureBox1.TabStop = false;
			//
			// _cannotFindLanguageLink
			//
			this._cannotFindLanguageLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._cannotFindLanguageLink.AutoSize = true;
			this._L10NSharpExtender.SetLocalizableToolTip(this._cannotFindLanguageLink, null);
			this._L10NSharpExtender.SetLocalizationComment(this._cannotFindLanguageLink, null);
			this._L10NSharpExtender.SetLocalizingId(this._cannotFindLanguageLink, "LanguageLookup._cannotFindLanguageLink");
			this._cannotFindLanguageLink.Location = new System.Drawing.Point(592, 6);
			this._cannotFindLanguageLink.Name = "_cannotFindLanguageLink";
			this._cannotFindLanguageLink.Size = new System.Drawing.Size(127, 13);
			this._cannotFindLanguageLink.TabIndex = 12;
			this._cannotFindLanguageLink.TabStop = true;
			this._cannotFindLanguageLink.Text = "Can\'t find your language?";
			this._cannotFindLanguageLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._cannotFindLanguageLink_LinkClicked);
			//
			// _desiredLanguageLabel
			//
			this._desiredLanguageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._desiredLanguageLabel.AutoSize = true;
			this._L10NSharpExtender.SetLocalizableToolTip(this._desiredLanguageLabel, null);
			this._L10NSharpExtender.SetLocalizationComment(this._desiredLanguageLabel, null);
			this._L10NSharpExtender.SetLocalizingId(this._desiredLanguageLabel, "LanguageLookup.DesiredLanguageDisplayNameLabel");
			this._desiredLanguageLabel.Location = new System.Drawing.Point(141, 0);
			this._desiredLanguageLabel.Name = "_desiredLanguageLabel";
			this._desiredLanguageLabel.Size = new System.Drawing.Size(309, 13);
			this._desiredLanguageLabel.TabIndex = 14;
			this._desiredLanguageLabel.Text = "You can change how the language name will be displayed here:";
			//
			// _desiredLanguageDisplayName
			//
			this._desiredLanguageDisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._desiredLanguageDisplayName.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._desiredLanguageDisplayName, null);
			this._L10NSharpExtender.SetLocalizationComment(this._desiredLanguageDisplayName, null);
			this._L10NSharpExtender.SetLocalizingId(this._desiredLanguageDisplayName, "LanguageLookup.LookupISOControl._desiredLanguageDisplayName");
			this._desiredLanguageDisplayName.Location = new System.Drawing.Point(479, 282);
			this._desiredLanguageDisplayName.Name = "_desiredLanguageDisplayName";
			this._desiredLanguageDisplayName.Size = new System.Drawing.Size(243, 33);
			this._desiredLanguageDisplayName.TabIndex = 13;
			this._desiredLanguageDisplayName.TextChanged += new System.EventHandler(this._desiredLanguageDisplayName_TextChanged);
			//
			// flowLayoutPanel1
			//
			this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel1.Controls.Add(this._desiredLanguageLabel);
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(20, 283);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(453, 37);
			this.flowLayoutPanel1.TabIndex = 15;
			//
			// _L10NSharpExtender
			//
			this._L10NSharpExtender.LocalizationManagerId = null;
			this._L10NSharpExtender.PrefixForNewItems = null;
			//
			// LookupISOControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this._desiredLanguageDisplayName);
			this.Controls.Add(this._cannotFindLanguageLink);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this._listView);
			this.Controls.Add(this._searchText);
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizingId(this, "LanguageLookup.LookupISOControl.LookupISOControl");
			this.Name = "LookupISOControl";
			this.Size = new System.Drawing.Size(722, 323);
			this.Load += new System.EventHandler(this.OnLoad);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView _listView;
		private System.Windows.Forms.ColumnHeader PrimaryNameHeader;
		private System.Windows.Forms.ColumnHeader codeHeader;
		private System.Windows.Forms.TextBox _searchText;
		private System.Windows.Forms.Timer _searchTimer;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.LinkLabel _cannotFindLanguageLink;
		private System.Windows.Forms.ColumnHeader countryHeader;
		private System.Windows.Forms.ColumnHeader AlternateNamesHeader;
		private System.Windows.Forms.Label _desiredLanguageLabel;
		private System.Windows.Forms.TextBox _desiredLanguageDisplayName;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
	}
}
