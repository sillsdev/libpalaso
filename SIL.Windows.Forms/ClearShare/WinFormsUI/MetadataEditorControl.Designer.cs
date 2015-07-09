﻿using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.ClearShare.WinFormsUI
{
	partial class MetadataEditorControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MetadataEditorControl));
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this._shareAlike = new System.Windows.Forms.RadioButton();
			this._noDerivates = new System.Windows.Forms.RadioButton();
			this._derivatives = new System.Windows.Forms.RadioButton();
			this.panel2 = new System.Windows.Forms.Panel();
			this.betterLabel1 = new BetterLabel();
			this._nonCommercial = new System.Windows.Forms.RadioButton();
			this._commercial = new System.Windows.Forms.RadioButton();
			this._licenseImage = new System.Windows.Forms.PictureBox();
			this._creativeCommons = new System.Windows.Forms.RadioButton();
			this._unknownLicense = new System.Windows.Forms.RadioButton();
			this._customLicense = new System.Windows.Forms.RadioButton();
			this._customRightsStatement = new System.Windows.Forms.TextBox();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.betterLabel2 = new BetterLabel();
			this._illustratorLabel = new System.Windows.Forms.Label();
			this._illustrator = new System.Windows.Forms.TextBox();
			this._copyrightYear = new System.Windows.Forms.TextBox();
			this._copyrightBy = new System.Windows.Forms.TextBox();
			this._copyrightHolderLabel = new System.Windows.Forms.Label();
			this._linkToDefinitionOfNonCommercial = new BetterLinkLabel();
			this._linkToRefinedCreativeCommonsWarning = new BetterLinkLabel();
			this._additionalRequestsLabel = new BetterLabel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._licenseImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this.label2, null);
			this._L10NSharpExtender.SetLocalizationComment(this.label2, null);
			this._L10NSharpExtender.SetLocalizingId(this.label2, "MetadataEditor.CopyrightYear");
			this.label2.Location = new System.Drawing.Point(3, 29);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(99, 29);
			this.label2.TabIndex = 8;
			this.label2.Text = "Copyright Year";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this.label3, null);
			this._L10NSharpExtender.SetLocalizationComment(this.label3, null);
			this._L10NSharpExtender.SetLocalizingId(this.label3, "MetadataEditor.License");
			this.label3.Location = new System.Drawing.Point(12, 132);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(46, 15);
			this.label3.TabIndex = 9;
			this.label3.Text = "License";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this._shareAlike);
			this.panel1.Controls.Add(this._noDerivates);
			this.panel1.Controls.Add(this._derivatives);
			this.panel1.Location = new System.Drawing.Point(59, 269);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(222, 67);
			this.panel1.TabIndex = 3;
			// 
			// _shareAlike
			// 
			this._shareAlike.AutoSize = true;
			this._shareAlike.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._shareAlike, null);
			this._L10NSharpExtender.SetLocalizationComment(this._shareAlike, null);
			this._L10NSharpExtender.SetLocalizingId(this._shareAlike, "MetadataEditor.YesShareAlike");
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
			this._L10NSharpExtender.SetLocalizableToolTip(this._noDerivates, null);
			this._L10NSharpExtender.SetLocalizationComment(this._noDerivates, null);
			this._L10NSharpExtender.SetLocalizingId(this._noDerivates, "Common.No");
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
			this._L10NSharpExtender.SetLocalizableToolTip(this._derivatives, null);
			this._L10NSharpExtender.SetLocalizationComment(this._derivatives, null);
			this._L10NSharpExtender.SetLocalizingId(this._derivatives, "Common.Yes");
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
			this.panel2.Location = new System.Drawing.Point(59, 172);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(229, 65);
			this.panel2.TabIndex = 20;
			// 
			// betterLabel1
			// 
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Enabled = false;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
			this._L10NSharpExtender.SetLocalizableToolTip(this.betterLabel1, null);
			this._L10NSharpExtender.SetLocalizationComment(this.betterLabel1, null);
			this._L10NSharpExtender.SetLocalizingId(this.betterLabel1, "MetadataEditor.AllowCommercialUse");
			this.betterLabel1.Location = new System.Drawing.Point(0, 0);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(287, 17);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Allow commercial uses of your work?";
			// 
			// _nonCommercial
			// 
			this._nonCommercial.AutoSize = true;
			this._nonCommercial.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._nonCommercial, null);
			this._L10NSharpExtender.SetLocalizationComment(this._nonCommercial, null);
			this._L10NSharpExtender.SetLocalizingId(this._nonCommercial, "Common.No");
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
			this._L10NSharpExtender.SetLocalizableToolTip(this._commercial, null);
			this._L10NSharpExtender.SetLocalizationComment(this._commercial, null);
			this._L10NSharpExtender.SetLocalizingId(this._commercial, "Common.Yes");
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
			this._L10NSharpExtender.SetLocalizableToolTip(this._licenseImage, null);
			this._L10NSharpExtender.SetLocalizationComment(this._licenseImage, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._licenseImage, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._licenseImage, "MetadataEditor.MetadataEditorControl._licenseImage");
			this._licenseImage.Location = new System.Drawing.Point(56, 342);
			this._licenseImage.Name = "_licenseImage";
			this._licenseImage.Size = new System.Drawing.Size(124, 40);
			this._licenseImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._licenseImage.TabIndex = 21;
			this._licenseImage.TabStop = false;
			// 
			// _creativeCommons
			// 
			this._creativeCommons.AutoSize = true;
			this._creativeCommons.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._L10NSharpExtender.SetLocalizableToolTip(this._creativeCommons, null);
			this._L10NSharpExtender.SetLocalizationComment(this._creativeCommons, null);
			this._L10NSharpExtender.SetLocalizingId(this._creativeCommons, "MetadataEditor.CreativeCommons");
			this._creativeCommons.Location = new System.Drawing.Point(42, 150);
			this._creativeCommons.Name = "_creativeCommons";
			this._creativeCommons.Size = new System.Drawing.Size(127, 19);
			this._creativeCommons.TabIndex = 22;
			this._creativeCommons.TabStop = true;
			this._creativeCommons.Text = "Creative Commons";
			this._creativeCommons.UseVisualStyleBackColor = true;
			this._creativeCommons.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			// 
			// _unknownLicense
			// 
			this._unknownLicense.AutoSize = true;
			this._unknownLicense.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._L10NSharpExtender.SetLocalizableToolTip(this._unknownLicense, null);
			this._L10NSharpExtender.SetLocalizationComment(this._unknownLicense, null);
			this._L10NSharpExtender.SetLocalizingId(this._unknownLicense, "MetadataEditor.UnknownLicense");
			this._unknownLicense.Location = new System.Drawing.Point(42, 388);
			this._unknownLicense.Name = "_unknownLicense";
			this._unknownLicense.Size = new System.Drawing.Size(284, 19);
			this._unknownLicense.TabIndex = 4;
			this._unknownLicense.TabStop = true;
			this._unknownLicense.Text = "Contact the copyright holder for any permissions";
			this._unknownLicense.UseVisualStyleBackColor = true;
			this._unknownLicense.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			// 
			// _customLicense
			// 
			this._customLicense.AutoSize = true;
			this._customLicense.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._L10NSharpExtender.SetLocalizableToolTip(this._customLicense, null);
			this._L10NSharpExtender.SetLocalizationComment(this._customLicense, null);
			this._L10NSharpExtender.SetLocalizingId(this._customLicense, "MetadataEditor.CustomLicense");
			this._customLicense.Location = new System.Drawing.Point(42, 411);
			this._customLicense.Name = "_customLicense";
			this._customLicense.Size = new System.Drawing.Size(67, 19);
			this._customLicense.TabIndex = 26;
			this._customLicense.TabStop = true;
			this._customLicense.Text = "Custom";
			this._customLicense.UseVisualStyleBackColor = true;
			this._customLicense.CheckedChanged += new System.EventHandler(this.OnLicenseComponentChanged);
			// 
			// _customRightsStatement
			// 
			this._customRightsStatement.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._customRightsStatement, null);
			this._L10NSharpExtender.SetLocalizationComment(this._customRightsStatement, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._customRightsStatement, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._customRightsStatement, "MetadataEditor.MetadataEditorControl._customRightsStatement");
			this._customRightsStatement.Location = new System.Drawing.Point(62, 450);
			this._customRightsStatement.Multiline = true;
			this._customRightsStatement.Name = "_customRightsStatement";
			this._customRightsStatement.Size = new System.Drawing.Size(268, 67);
			this._customRightsStatement.TabIndex = 3;
			this._customRightsStatement.TextChanged += new System.EventHandler(this._customLicenseDescription_TextChanged);
			// 
			// _L10NSharpExtender
			// 
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "MetadataEditor";
			// 
			// betterLabel2
			// 
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Enabled = false;
			this.betterLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel2.ForeColor = System.Drawing.SystemColors.ControlText;
			this._L10NSharpExtender.SetLocalizableToolTip(this.betterLabel2, null);
			this._L10NSharpExtender.SetLocalizationComment(this.betterLabel2, null);
			this._L10NSharpExtender.SetLocalizingId(this.betterLabel2, "MetadataEditor.AllowDerivatives");
			this.betterLabel2.Location = new System.Drawing.Point(58, 249);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(288, 17);
			this.betterLabel2.TabIndex = 12;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "Allow modifications of your work?";
			// 
			// _illustratorLabel
			// 
			this._illustratorLabel.AutoSize = true;
			this._illustratorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._illustratorLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._illustratorLabel, null);
			this._L10NSharpExtender.SetLocalizationComment(this._illustratorLabel, null);
			this._L10NSharpExtender.SetLocalizingId(this._illustratorLabel, "MetadataEditor.CreatorLabel");
			this._illustratorLabel.Location = new System.Drawing.Point(3, 0);
			this._illustratorLabel.Name = "_illustratorLabel";
			this._illustratorLabel.Size = new System.Drawing.Size(99, 29);
			this._illustratorLabel.TabIndex = 6;
			this._illustratorLabel.Text = "Creator";
			// 
			// _illustrator
			// 
			this._illustrator.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._illustrator, null);
			this._L10NSharpExtender.SetLocalizationComment(this._illustrator, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._illustrator, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._illustrator, "MetadataEditor.MetadataEditorControl._illustrator");
			this._illustrator.Location = new System.Drawing.Point(108, 3);
			this._illustrator.Name = "_illustrator";
			this._illustrator.Size = new System.Drawing.Size(197, 23);
			this._illustrator.TabIndex = 9;
			this._illustrator.TextChanged += new System.EventHandler(this._illustrator_TextChanged);
			// 
			// _copyrightYear
			// 
			this._copyrightYear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._copyrightYear, null);
			this._L10NSharpExtender.SetLocalizationComment(this._copyrightYear, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._copyrightYear, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._copyrightYear, "MetadataEditor.MetadataEditorControl._copyrightYear");
			this._copyrightYear.Location = new System.Drawing.Point(108, 32);
			this._copyrightYear.Name = "_copyrightYear";
			this._copyrightYear.Size = new System.Drawing.Size(38, 23);
			this._copyrightYear.TabIndex = 10;
			this._copyrightYear.TextChanged += new System.EventHandler(this._copyrightYear_TextChanged);
			// 
			// _copyrightBy
			// 
			this._copyrightBy.Dock = System.Windows.Forms.DockStyle.Fill;
			this._copyrightBy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._copyrightBy, null);
			this._L10NSharpExtender.SetLocalizationComment(this._copyrightBy, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._copyrightBy, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._copyrightBy, "MetadataEditor.MetadataEditorControl._copyrightBy");
			this._copyrightBy.Location = new System.Drawing.Point(108, 61);
			this._copyrightBy.Multiline = true;
			this._copyrightBy.Name = "_copyrightBy";
			this._copyrightBy.Size = new System.Drawing.Size(220, 45);
			this._copyrightBy.TabIndex = 27;
			this._copyrightBy.TextChanged += new System.EventHandler(this._copyrightBy_TextChanged);
			// 
			// _copyrightHolderLabel
			// 
			this._copyrightHolderLabel.AutoSize = true;
			this._copyrightHolderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._copyrightHolderLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._L10NSharpExtender.SetLocalizableToolTip(this._copyrightHolderLabel, null);
			this._L10NSharpExtender.SetLocalizationComment(this._copyrightHolderLabel, null);
			this._L10NSharpExtender.SetLocalizingId(this._copyrightHolderLabel, "MetadataEditor.CopyrightHolder");
			this._copyrightHolderLabel.Location = new System.Drawing.Point(3, 58);
			this._copyrightHolderLabel.Name = "_copyrightHolderLabel";
			this._copyrightHolderLabel.Size = new System.Drawing.Size(99, 51);
			this._copyrightHolderLabel.TabIndex = 28;
			this._copyrightHolderLabel.Text = "Copyright Holder";
			// 
			// _linkToDefinitionOfNonCommercial
			// 
			this._linkToDefinitionOfNonCommercial.BackColor = System.Drawing.SystemColors.Control;
			this._linkToDefinitionOfNonCommercial.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._linkToDefinitionOfNonCommercial.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Underline);
			this._linkToDefinitionOfNonCommercial.ForeColor = System.Drawing.Color.Blue;
			this._L10NSharpExtender.SetLocalizableToolTip(this._linkToDefinitionOfNonCommercial, null);
			this._L10NSharpExtender.SetLocalizationComment(this._linkToDefinitionOfNonCommercial, "The meaning of  \"non-commercial\" is vague but important. This hyperlink takes you" +
        " somewhere that defines it.");
			this._L10NSharpExtender.SetLocalizingId(this._linkToDefinitionOfNonCommercial, "MetadataEditor.betterLinkLabel1");
			this._linkToDefinitionOfNonCommercial.Location = new System.Drawing.Point(272, 172);
			this._linkToDefinitionOfNonCommercial.Multiline = true;
			this._linkToDefinitionOfNonCommercial.Name = "_linkToDefinitionOfNonCommercial";
			this._linkToDefinitionOfNonCommercial.Size = new System.Drawing.Size(61, 16);
			this._linkToDefinitionOfNonCommercial.TabIndex = 28;
			this._linkToDefinitionOfNonCommercial.TabStop = false;
			this._linkToDefinitionOfNonCommercial.Text = "more info";
			this._linkToDefinitionOfNonCommercial.URL = "http://creativecommons.org/licenses/by-nc/4.0/legalcode#s1i";
			// 
			// _linkToRefinedCreativeCommonsWarning
			// 
			this._linkToRefinedCreativeCommonsWarning.BackColor = System.Drawing.SystemColors.Control;
			this._linkToRefinedCreativeCommonsWarning.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._linkToRefinedCreativeCommonsWarning.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Underline);
			this._linkToRefinedCreativeCommonsWarning.ForeColor = System.Drawing.Color.Blue;
			this._L10NSharpExtender.SetLocalizableToolTip(this._linkToRefinedCreativeCommonsWarning, null);
			this._L10NSharpExtender.SetLocalizationComment(this._linkToRefinedCreativeCommonsWarning, resources.GetString("_linkToRefinedCreativeCommonsWarning.LocalizationComment"));
			this._L10NSharpExtender.SetLocalizingId(this._linkToRefinedCreativeCommonsWarning, "MetadataEditor.linkToWarningAboutRefiningCreativeCommons");
			this._linkToRefinedCreativeCommonsWarning.Location = new System.Drawing.Point(163, 432);
			this._linkToRefinedCreativeCommonsWarning.Multiline = true;
			this._linkToRefinedCreativeCommonsWarning.Name = "_linkToRefinedCreativeCommonsWarning";
			this._linkToRefinedCreativeCommonsWarning.Size = new System.Drawing.Size(170, 16);
			this._linkToRefinedCreativeCommonsWarning.TabIndex = 29;
			this._linkToRefinedCreativeCommonsWarning.TabStop = false;
			this._linkToRefinedCreativeCommonsWarning.Text = "Not Enforceable";
			this._linkToRefinedCreativeCommonsWarning.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this._linkToRefinedCreativeCommonsWarning.URL = "http://creativecommons.org/licenses/by-nc/4.0/legalcode#s7a";
			// 
			// _additionalRequestsLabel
			// 
			this._additionalRequestsLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._additionalRequestsLabel.Enabled = false;
			this._additionalRequestsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this._L10NSharpExtender.SetLocalizableToolTip(this._additionalRequestsLabel, null);
			this._L10NSharpExtender.SetLocalizationComment(this._additionalRequestsLabel, "When you choose a Creative Commons License, this label shows over the text box at" +
        " the bottom.");
			this._L10NSharpExtender.SetLocalizingId(this._additionalRequestsLabel, "MetadataEditor.additionalRequestsLabel");
			this._additionalRequestsLabel.Location = new System.Drawing.Point(42, 432);
			this._additionalRequestsLabel.Multiline = true;
			this._additionalRequestsLabel.Name = "_additionalRequestsLabel";
			this._additionalRequestsLabel.ReadOnly = true;
			this._additionalRequestsLabel.Size = new System.Drawing.Size(151, 13);
			this._additionalRequestsLabel.TabIndex = 30;
			this._additionalRequestsLabel.TabStop = false;
			this._additionalRequestsLabel.Text = "Additional Requests";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._illustratorLabel, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._illustrator, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this._copyrightYear, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this._copyrightBy, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this._copyrightHolderLabel, 0, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 20);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(318, 109);
			this.tableLayoutPanel1.TabIndex = 27;
			// 
			// MetadataEditorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._additionalRequestsLabel);
			this.Controls.Add(this._linkToRefinedCreativeCommonsWarning);
			this.Controls.Add(this._linkToDefinitionOfNonCommercial);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this._customRightsStatement);
			this.Controls.Add(this._customLicense);
			this.Controls.Add(this._unknownLicense);
			this.Controls.Add(this._creativeCommons);
			this.Controls.Add(this._licenseImage);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.betterLabel2);
			this.Controls.Add(this.label3);
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizingId(this, "MetadataEditor.MetadataEditorControl.MetadataEditorControl");
			this.Name = "MetadataEditorControl";
			this.Size = new System.Drawing.Size(349, 529);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._licenseImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

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
		private System.Windows.Forms.RadioButton _customLicense;
		private System.Windows.Forms.TextBox _customRightsStatement;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label _illustratorLabel;
		private System.Windows.Forms.TextBox _illustrator;
		private System.Windows.Forms.TextBox _copyrightYear;
		private System.Windows.Forms.TextBox _copyrightBy;
		private System.Windows.Forms.Label _copyrightHolderLabel;
		private Widgets.BetterLinkLabel _linkToDefinitionOfNonCommercial;
		private Widgets.BetterLinkLabel _linkToRefinedCreativeCommonsWarning;
		private Widgets.BetterLabel _additionalRequestsLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
	}
}
