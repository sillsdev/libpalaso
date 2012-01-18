namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
{
	partial class MetdataEditorControl
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
			this.label3 = new System.Windows.Forms.Label();
			this._copyrightBy = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this._illustrator = new System.Windows.Forms.TextBox();
			this._illustratorLabel = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this._shareAlike = new System.Windows.Forms.RadioButton();
			this._noDerivates = new System.Windows.Forms.RadioButton();
			this._derivatives = new System.Windows.Forms.RadioButton();
			this.panel2 = new System.Windows.Forms.Panel();
			this._nonCommercial = new System.Windows.Forms.RadioButton();
			this._commercial = new System.Windows.Forms.RadioButton();
			this._licenseImage = new System.Windows.Forms.PictureBox();
			this._creativeCommons = new System.Windows.Forms.RadioButton();
			this._unknownLicense = new System.Windows.Forms.RadioButton();
			this._copyrightYear = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this._customLicense = new System.Windows.Forms.RadioButton();
			this._customLicenseDescription = new System.Windows.Forms.TextBox();
			this.betterLabel1 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel2 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._licenseImage)).BeginInit();
			this.SuspendLayout();
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(13, 75);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(46, 15);
			this.label3.TabIndex = 9;
			this.label3.Text = "License";
			//
			// _copyrightBy
			//
			this._copyrightBy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._copyrightBy.Location = new System.Drawing.Point(173, 47);
			this._copyrightBy.Name = "_copyrightBy";
			this._copyrightBy.Size = new System.Drawing.Size(128, 23);
			this._copyrightBy.TabIndex = 1;
			this._copyrightBy.TextChanged += new System.EventHandler(this._copyrightYear_TextChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(12, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(86, 15);
			this.label2.TabIndex = 7;
			this.label2.Text = "Copyright Year";
			//
			// _illustrator
			//
			this._illustrator.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._illustrator.Location = new System.Drawing.Point(104, 20);
			this._illustrator.Name = "_illustrator";
			this._illustrator.Size = new System.Drawing.Size(197, 23);
			this._illustrator.TabIndex = 0;
			this._illustrator.TextChanged += new System.EventHandler(this._illustrator_TextChanged);
			//
			// _illustratorLabel
			//
			this._illustratorLabel.AutoSize = true;
			this._illustratorLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._illustratorLabel.Location = new System.Drawing.Point(12, 21);
			this._illustratorLabel.Name = "_illustratorLabel";
			this._illustratorLabel.Size = new System.Drawing.Size(57, 15);
			this._illustratorLabel.TabIndex = 5;
			this._illustratorLabel.Text = "Illustrator";
			//
			// panel1
			//
			this.panel1.Controls.Add(this._shareAlike);
			this.panel1.Controls.Add(this._noDerivates);
			this.panel1.Controls.Add(this._derivatives);
			this.panel1.Location = new System.Drawing.Point(60, 212);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(222, 67);
			this.panel1.TabIndex = 1;
			//
			// _shareAlike
			//
			this._shareAlike.AutoSize = true;
			this._shareAlike.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._shareAlike.Location = new System.Drawing.Point(3, 25);
			this._shareAlike.Name = "_shareAlike";
			this._shareAlike.Size = new System.Drawing.Size(195, 19);
			this._shareAlike.TabIndex = 19;
			this._shareAlike.TabStop = true;
			this._shareAlike.Text = "Yes, as long as others share alike";
			this._shareAlike.UseVisualStyleBackColor = true;
			this._shareAlike.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			//
			// _noDerivates
			//
			this._noDerivates.AutoSize = true;
			this._noDerivates.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._noDerivates.Location = new System.Drawing.Point(3, 47);
			this._noDerivates.Name = "_noDerivates";
			this._noDerivates.Size = new System.Drawing.Size(41, 19);
			this._noDerivates.TabIndex = 18;
			this._noDerivates.TabStop = true;
			this._noDerivates.Text = "No";
			this._noDerivates.UseVisualStyleBackColor = true;
			this._noDerivates.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			//
			// _derivatives
			//
			this._derivatives.AutoSize = true;
			this._derivatives.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._derivatives.Location = new System.Drawing.Point(3, 3);
			this._derivatives.Name = "_derivatives";
			this._derivatives.Size = new System.Drawing.Size(43, 19);
			this._derivatives.TabIndex = 17;
			this._derivatives.TabStop = true;
			this._derivatives.Text = "Yes";
			this._derivatives.UseVisualStyleBackColor = true;
			this._derivatives.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			//
			// panel2
			//
			this.panel2.Controls.Add(this.betterLabel1);
			this.panel2.Controls.Add(this._nonCommercial);
			this.panel2.Controls.Add(this._commercial);
			this.panel2.Location = new System.Drawing.Point(60, 115);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(229, 65);
			this.panel2.TabIndex = 20;
			//
			// _nonCommercial
			//
			this._nonCommercial.AutoSize = true;
			this._nonCommercial.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._nonCommercial.Location = new System.Drawing.Point(0, 42);
			this._nonCommercial.Name = "_nonCommercial";
			this._nonCommercial.Size = new System.Drawing.Size(41, 19);
			this._nonCommercial.TabIndex = 15;
			this._nonCommercial.TabStop = true;
			this._nonCommercial.Text = "No";
			this._nonCommercial.UseVisualStyleBackColor = true;
			this._nonCommercial.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			//
			// _commercial
			//
			this._commercial.AutoSize = true;
			this._commercial.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._commercial.Location = new System.Drawing.Point(0, 17);
			this._commercial.Name = "_commercial";
			this._commercial.Size = new System.Drawing.Size(43, 19);
			this._commercial.TabIndex = 14;
			this._commercial.TabStop = true;
			this._commercial.Text = "Yes";
			this._commercial.UseVisualStyleBackColor = true;
			this._commercial.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			//
			// _licenseImage
			//
			this._licenseImage.Location = new System.Drawing.Point(57, 285);
			this._licenseImage.Name = "_licenseImage";
			this._licenseImage.Size = new System.Drawing.Size(156, 40);
			this._licenseImage.TabIndex = 21;
			this._licenseImage.TabStop = false;
			//
			// _creativeCommons
			//
			this._creativeCommons.AutoSize = true;
			this._creativeCommons.Location = new System.Drawing.Point(43, 93);
			this._creativeCommons.Name = "_creativeCommons";
			this._creativeCommons.Size = new System.Drawing.Size(113, 17);
			this._creativeCommons.TabIndex = 22;
			this._creativeCommons.TabStop = true;
			this._creativeCommons.Text = "Creative Commons";
			this._creativeCommons.UseVisualStyleBackColor = true;
			this._creativeCommons.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			//
			// _unknownLicense
			//
			this._unknownLicense.AutoSize = true;
			this._unknownLicense.Location = new System.Drawing.Point(43, 331);
			this._unknownLicense.Name = "_unknownLicense";
			this._unknownLicense.Size = new System.Drawing.Size(250, 17);
			this._unknownLicense.TabIndex = 2;
			this._unknownLicense.TabStop = true;
			this._unknownLicense.Text = "Contact the copyright holder for any permissions";
			this._unknownLicense.UseVisualStyleBackColor = true;
			this._unknownLicense.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			//
			// _copyrightYear
			//
			this._copyrightYear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._copyrightYear.Location = new System.Drawing.Point(104, 47);
			this._copyrightYear.Name = "_copyrightYear";
			this._copyrightYear.Size = new System.Drawing.Size(38, 23);
			this._copyrightYear.TabIndex = 2;
			this._copyrightYear.TextChanged += new System.EventHandler(this._copyrightYear_TextChanged);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(149, 50);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(20, 15);
			this.label1.TabIndex = 25;
			this.label1.Text = "By";
			//
			// _customLicense
			//
			this._customLicense.AutoSize = true;
			this._customLicense.Location = new System.Drawing.Point(43, 354);
			this._customLicense.Name = "_customLicense";
			this._customLicense.Size = new System.Drawing.Size(60, 17);
			this._customLicense.TabIndex = 26;
			this._customLicense.TabStop = true;
			this._customLicense.Text = "Custom";
			this._customLicense.UseVisualStyleBackColor = true;
			this._customLicense.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			//
			// _customLicenseDescription
			//
			this._customLicenseDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._customLicenseDescription.Location = new System.Drawing.Point(63, 377);
			this._customLicenseDescription.Multiline = true;
			this._customLicenseDescription.Name = "_customLicenseDescription";
			this._customLicenseDescription.Size = new System.Drawing.Size(219, 83);
			this._customLicenseDescription.TabIndex = 27;
			this._customLicenseDescription.TextChanged += new System.EventHandler(this._customLicenseDescription_TextChanged);
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.Location = new System.Drawing.Point(0, 0);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(392, 20);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Allow commercial uses of your work?";
			//
			// betterLabel2
			//
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel2.Location = new System.Drawing.Point(59, 192);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(277, 20);
			this.betterLabel2.TabIndex = 12;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "Allow modifications of your work?";
			//
			// MetdataEditorControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._customLicenseDescription);
			this.Controls.Add(this._customLicense);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._copyrightYear);
			this.Controls.Add(this._unknownLicense);
			this.Controls.Add(this._creativeCommons);
			this.Controls.Add(this._licenseImage);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.betterLabel2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._copyrightBy);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._illustrator);
			this.Controls.Add(this._illustratorLabel);
			this.Name = "MetdataEditorControl";
			this.Size = new System.Drawing.Size(338, 481);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._licenseImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _copyrightBy;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox _illustrator;
		private System.Windows.Forms.Label _illustratorLabel;
		private Widgets.BetterLabel betterLabel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton _shareAlike;
		private System.Windows.Forms.RadioButton _noDerivates;
		private System.Windows.Forms.RadioButton _derivatives;
		private System.Windows.Forms.Panel panel2;
		private Widgets.BetterLabel betterLabel1;
		private System.Windows.Forms.RadioButton _nonCommercial;
		private System.Windows.Forms.RadioButton _commercial;
		private System.Windows.Forms.PictureBox _licenseImage;
		private System.Windows.Forms.RadioButton _creativeCommons;
		private System.Windows.Forms.RadioButton _unknownLicense;
		private System.Windows.Forms.TextBox _copyrightYear;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton _customLicense;
		private System.Windows.Forms.TextBox _customLicenseDescription;
	}
}
