namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSListItem
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WSListItem));
			Elsehemy.SuperToolTipInfoWrapper superToolTipInfoWrapper1 = new Elsehemy.SuperToolTipInfoWrapper();
			Elsehemy.SuperToolTipInfo superToolTipInfo1 = new Elsehemy.SuperToolTipInfo();
			Elsehemy.SuperToolTipInfoWrapper superToolTipInfoWrapper2 = new Elsehemy.SuperToolTipInfoWrapper();
			Elsehemy.SuperToolTipInfo superToolTipInfo2 = new Elsehemy.SuperToolTipInfo();
			Elsehemy.SuperToolTipInfoWrapper superToolTipInfoWrapper3 = new Elsehemy.SuperToolTipInfoWrapper();
			Elsehemy.SuperToolTipInfo superToolTipInfo3 = new Elsehemy.SuperToolTipInfo();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._writingSystemLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this._language = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this._iso = new System.Windows.Forms.TextBox();
			this._fontAndKeboardLink = new System.Windows.Forms.LinkLabel();
			this.linkLabel3 = new System.Windows.Forms.LinkLabel();
			this.linkLabel4 = new System.Windows.Forms.LinkLabel();
			this.linkLabel5 = new System.Windows.Forms.LinkLabel();
			this._labelSummary = new System.Windows.Forms.Label();
			this._deleteButton = new System.Windows.Forms.Button();
			this._duplicateButton = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label4 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this._scriptBox = new System.Windows.Forms.ComboBox();
			this._variant = new System.Windows.Forms.TextBox();
			this._abbreviation = new System.Windows.Forms.TextBox();
			this.superToolTip1 = new Elsehemy.SuperToolTip(this.components);
			this._regionBox = new System.Windows.Forms.TextBox();
			this._deletionIndicator = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			//
			// pictureBox1
			//
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(13, 10);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(19, 19);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.WSListItem_Click);
			//
			// _writingSystemLabel
			//
			this._writingSystemLabel.AutoSize = true;
			this._writingSystemLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._writingSystemLabel.Location = new System.Drawing.Point(49, 10);
			this._writingSystemLabel.Name = "_writingSystemLabel";
			this._writingSystemLabel.Size = new System.Drawing.Size(31, 17);
			this._writingSystemLabel.TabIndex = 1;
			this._writingSystemLabel.Text = "foo";
			this._writingSystemLabel.Click += new System.EventHandler(this.WSListItem_Click);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(10, 41);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 15);
			this.label1.TabIndex = 1;
			this.label1.Text = "Language Name";
			//
			// _language
			//
			this._language.Location = new System.Drawing.Point(114, 41);
			this._language.Name = "_language";
			this._language.Size = new System.Drawing.Size(83, 20);
			this._language.TabIndex = 0;
			this._language.TextChanged += new System.EventHandler(this.OnSomethingChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(10, 66);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(59, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "ISO Code";
			//
			// _iso
			//
			this._iso.Location = new System.Drawing.Point(114, 66);
			this._iso.MaxLength = 3;
			this._iso.Name = "_iso";
			this._iso.Size = new System.Drawing.Size(35, 20);
			superToolTipInfo1.BackgroundGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			superToolTipInfo1.BackgroundGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(218)))), ((int)(((byte)(239)))));
			superToolTipInfo1.BackgroundGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(251)))));
			superToolTipInfo1.BodyFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			superToolTipInfo1.BodyText = null;
			superToolTipInfo1.HeaderText = "ISO Code";
			superToolTipInfo1.OffsetForWhereToDisplay = new System.Drawing.Point(0, 0);
			superToolTipInfoWrapper1.SuperToolTipInfo = superToolTipInfo1;
			superToolTipInfoWrapper1.UseSuperToolTip = true;
			this.superToolTip1.SetSuperStuff(this._iso, superToolTipInfoWrapper1);
			this._iso.TabIndex = 1;
			//
			// _fontAndKeboardLink
			//
			this._fontAndKeboardLink.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this._fontAndKeboardLink.AutoSize = true;
			this._fontAndKeboardLink.LinkColor = System.Drawing.Color.RoyalBlue;
			this._fontAndKeboardLink.Location = new System.Drawing.Point(10, 99);
			this._fontAndKeboardLink.Name = "_fontAndKeboardLink";
			this._fontAndKeboardLink.Size = new System.Drawing.Size(85, 13);
			this._fontAndKeboardLink.TabIndex = 7;
			this._fontAndKeboardLink.TabStop = true;
			this._fontAndKeboardLink.Text = "Font && Keyboard";
			this._fontAndKeboardLink.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this._fontAndKeboardLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnFontAndKeyboardLinkClicked);
			//
			// linkLabel3
			//
			this.linkLabel3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.linkLabel3.AutoSize = true;
			this.linkLabel3.Enabled = false;
			this.linkLabel3.LinkColor = System.Drawing.Color.RoyalBlue;
			this.linkLabel3.Location = new System.Drawing.Point(112, 99);
			this.linkLabel3.Name = "linkLabel3";
			this.linkLabel3.Size = new System.Drawing.Size(115, 13);
			this.linkLabel3.TabIndex = 8;
			this.linkLabel3.TabStop = true;
			this.linkLabel3.Text = "Characters and Sorting";
			this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
			//
			// linkLabel4
			//
			this.linkLabel4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.linkLabel4.AutoSize = true;
			this.linkLabel4.Enabled = false;
			this.linkLabel4.LinkColor = System.Drawing.Color.RoyalBlue;
			this.linkLabel4.Location = new System.Drawing.Point(238, 99);
			this.linkLabel4.Name = "linkLabel4";
			this.linkLabel4.Size = new System.Drawing.Size(78, 13);
			this.linkLabel4.TabIndex = 9;
			this.linkLabel4.TabStop = true;
			this.linkLabel4.Text = "Spell Checking";
			//
			// linkLabel5
			//
			this.linkLabel5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.linkLabel5.AutoSize = true;
			this.linkLabel5.Enabled = false;
			this.linkLabel5.LinkColor = System.Drawing.Color.RoyalBlue;
			this.linkLabel5.Location = new System.Drawing.Point(324, 99);
			this.linkLabel5.Name = "linkLabel5";
			this.linkLabel5.Size = new System.Drawing.Size(66, 13);
			this.linkLabel5.TabIndex = 10;
			this.linkLabel5.TabStop = true;
			this.linkLabel5.Text = "Transducers";
			//
			// _labelSummary
			//
			this._labelSummary.AutoSize = true;
			this._labelSummary.ForeColor = System.Drawing.SystemColors.ControlDark;
			this._labelSummary.Location = new System.Drawing.Point(116, 10);
			this._labelSummary.Name = "_labelSummary";
			this._labelSummary.Size = new System.Drawing.Size(378, 13);
			this._labelSummary.TabIndex = 4;
			this._labelSummary.Text = "The western variant of the Foo language written in Latin script. (bin-ltn-western" +
				")";
			this._labelSummary.Click += new System.EventHandler(this.WSListItem_Click);
			//
			// _deleteButton
			//
			this._deleteButton.FlatAppearance.BorderSize = 0;
			this._deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._deleteButton.Image = ((System.Drawing.Image)(resources.GetObject("_deleteButton.Image")));
			this._deleteButton.Location = new System.Drawing.Point(447, 89);
			this._deleteButton.Name = "_deleteButton";
			this._deleteButton.Size = new System.Drawing.Size(27, 23);
			this._deleteButton.TabIndex = 11;
			this.toolTip1.SetToolTip(this._deleteButton, "Delete Writing System");
			this._deleteButton.UseVisualStyleBackColor = true;
			this._deleteButton.Click += new System.EventHandler(this._deleteButton_Click);
			//
			// _duplicateButton
			//
			this._duplicateButton.FlatAppearance.BorderSize = 0;
			this._duplicateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._duplicateButton.Image = ((System.Drawing.Image)(resources.GetObject("_duplicateButton.Image")));
			this._duplicateButton.Location = new System.Drawing.Point(492, 89);
			this._duplicateButton.Name = "_duplicateButton";
			this._duplicateButton.Size = new System.Drawing.Size(27, 23);
			this._duplicateButton.TabIndex = 12;
			this.toolTip1.SetToolTip(this._duplicateButton, "Duplicate Writing System");
			this._duplicateButton.UseVisualStyleBackColor = true;
			this._duplicateButton.Click += new System.EventHandler(this._duplicateButton_Click);
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(208, 41);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(38, 15);
			this.label4.TabIndex = 1;
			this.label4.Text = "Script";
			//
			// label6
			//
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(208, 66);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(47, 15);
			this.label6.TabIndex = 1;
			this.label6.Text = "Region";
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(365, 41);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(45, 15);
			this.label5.TabIndex = 1;
			this.label5.Text = "Variant";
			//
			// label7
			//
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(365, 66);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(74, 15);
			this.label7.TabIndex = 1;
			this.label7.Text = "Abbreviation";
			//
			// _scriptBox
			//
			this._scriptBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
			this._scriptBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this._scriptBox.FormattingEnabled = true;
			this._scriptBox.Location = new System.Drawing.Point(262, 41);
			this._scriptBox.Name = "_scriptBox";
			this._scriptBox.Size = new System.Drawing.Size(88, 21);
			this._scriptBox.TabIndex = 2;
			this._scriptBox.SelectedIndexChanged += new System.EventHandler(this.OnSomethingChanged);
			this._scriptBox.TextChanged += new System.EventHandler(this.OnSomethingChanged);
			//
			// _variant
			//
			this._variant.Location = new System.Drawing.Point(446, 40);
			this._variant.Name = "_variant";
			this._variant.Size = new System.Drawing.Size(73, 20);
			superToolTipInfo2.BackgroundGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			superToolTipInfo2.BackgroundGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(218)))), ((int)(((byte)(239)))));
			superToolTipInfo2.BackgroundGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(251)))));
			superToolTipInfo2.BodyText = "Variant subtags are values used to indicate dialects or script variations not alr" +
				"eady covered by combinations of language, script and region subtag.";
			superToolTipInfo2.HeaderText = "Variant";
			superToolTipInfo2.OffsetForWhereToDisplay = new System.Drawing.Point(0, 0);
			superToolTipInfoWrapper2.SuperToolTipInfo = superToolTipInfo2;
			superToolTipInfoWrapper2.UseSuperToolTip = true;
			this.superToolTip1.SetSuperStuff(this._variant, superToolTipInfoWrapper2);
			this._variant.TabIndex = 4;
			this._variant.TextChanged += new System.EventHandler(this.OnSomethingChanged);
			//
			// _abbreviation
			//
			this._abbreviation.Location = new System.Drawing.Point(447, 65);
			this._abbreviation.Name = "_abbreviation";
			this._abbreviation.Size = new System.Drawing.Size(42, 20);
			superToolTipInfo3.BackgroundGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			superToolTipInfo3.BackgroundGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(218)))), ((int)(((byte)(239)))));
			superToolTipInfo3.BackgroundGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(251)))));
			superToolTipInfo3.BodyText = "Programs will use this short label on-screen.";
			superToolTipInfo3.HeaderText = "Abbreviation";
			superToolTipInfo3.OffsetForWhereToDisplay = new System.Drawing.Point(0, 0);
			superToolTipInfoWrapper3.SuperToolTipInfo = superToolTipInfo3;
			superToolTipInfoWrapper3.UseSuperToolTip = true;
			this.superToolTip1.SetSuperStuff(this._abbreviation, superToolTipInfoWrapper3);
			this._abbreviation.TabIndex = 5;
			this._abbreviation.TextChanged += new System.EventHandler(this.OnSomethingChanged);
			//
			// superToolTip1
			//
			this.superToolTip1.FadingInterval = 10;
			//
			// _regionBox
			//
			this._regionBox.Location = new System.Drawing.Point(262, 68);
			this._regionBox.Name = "_regionBox";
			this._regionBox.Size = new System.Drawing.Size(83, 20);
			this._regionBox.TabIndex = 3;
			this._regionBox.TextChanged += new System.EventHandler(this.OnSomethingChanged);
			//
			// _deletionIndicator
			//
			this._deletionIndicator.BackColor = System.Drawing.Color.Red;
			this._deletionIndicator.ForeColor = System.Drawing.Color.Red;
			this._deletionIndicator.Location = new System.Drawing.Point(17, 17);
			this._deletionIndicator.Name = "_deletionIndicator";
			this._deletionIndicator.Size = new System.Drawing.Size(470, 2);
			this._deletionIndicator.TabIndex = 13;
			this._deletionIndicator.Visible = false;
			//
			// WSListItem
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this._deletionIndicator);
			this.Controls.Add(this._scriptBox);
			this.Controls.Add(this._duplicateButton);
			this.Controls.Add(this._deleteButton);
			this.Controls.Add(this._labelSummary);
			this.Controls.Add(this.linkLabel5);
			this.Controls.Add(this.linkLabel4);
			this.Controls.Add(this.linkLabel3);
			this.Controls.Add(this._fontAndKeboardLink);
			this.Controls.Add(this._regionBox);
			this.Controls.Add(this._iso);
			this.Controls.Add(this._abbreviation);
			this.Controls.Add(this._variant);
			this.Controls.Add(this._language);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._writingSystemLabel);
			this.Controls.Add(this.pictureBox1);
			this.Name = "WSListItem";
			this.Size = new System.Drawing.Size(529, 116);
			this.Load += new System.EventHandler(this.OnLoad);
			this.Click += new System.EventHandler(this.WSListItem_Click);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label _writingSystemLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _language;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox _iso;
		private System.Windows.Forms.LinkLabel _fontAndKeboardLink;
		private System.Windows.Forms.LinkLabel linkLabel3;
		private System.Windows.Forms.LinkLabel linkLabel4;
		private System.Windows.Forms.LinkLabel linkLabel5;
		private System.Windows.Forms.Label _labelSummary;
		private System.Windows.Forms.Button _deleteButton;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Button _duplicateButton;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox _scriptBox;
		private System.Windows.Forms.TextBox _variant;
		private System.Windows.Forms.TextBox _abbreviation;
		private Elsehemy.SuperToolTip superToolTip1;
		private System.Windows.Forms.TextBox _regionBox;
		private System.Windows.Forms.Panel _deletionIndicator;
	}
}