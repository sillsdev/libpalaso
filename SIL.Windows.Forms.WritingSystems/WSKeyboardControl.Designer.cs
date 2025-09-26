using System.Windows.Forms;
using SIL.Windows.Forms.Keyboarding;

namespace SIL.Windows.Forms.WritingSystems
{
	partial class WSKeyboardControl
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
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				if (!DesignMode)
				{
					KeyboardController.UnregisterControl(_testArea);
				}

				if (_hookedToForm)
				{
					_hookedToForm = false;
					var form = TopLevelControl as Form;
					if (form != null)
						form.Activated -= WSKeyboardControl_Activated;
				}
			}
			components = null;
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this._keymanConfigurationLink = new System.Windows.Forms.LinkLabel();
			this._keyboardSettingsLink = new System.Windows.Forms.LinkLabel();
			this.label3 = new System.Windows.Forms.Label();
			this._possibleKeyboardsList = new System.Windows.Forms.ListView();
			this._keyboards = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._selectKeyboardLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this._testArea = new System.Windows.Forms.TextBox();
			this.l10NSharpExtender1 = new L10NSharp.Windows.Forms.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// splitContainer1
			//
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// splitContainer1.Panel1
			//
			this.splitContainer1.Panel1.Controls.Add(this._keymanConfigurationLink);
			this.splitContainer1.Panel1.Controls.Add(this._keyboardSettingsLink);
			this.splitContainer1.Panel1.Controls.Add(this.label3);
			this.splitContainer1.Panel1.Controls.Add(this._possibleKeyboardsList);
			this.splitContainer1.Panel1.Controls.Add(this._selectKeyboardLabel);
			//
			// splitContainer1.Panel2
			//
			this.splitContainer1.Panel2.Controls.Add(this.label1);
			this.splitContainer1.Panel2.Controls.Add(this._testArea);
			this.splitContainer1.Size = new System.Drawing.Size(460, 297);
			this.splitContainer1.SplitterDistance = 181;
			this.splitContainer1.TabIndex = 0;
			this.splitContainer1.TabStop = false;
			//
			// _keymanConfigurationLink
			//
			this._keymanConfigurationLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._keymanConfigurationLink.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._keymanConfigurationLink, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._keymanConfigurationLink, null);
			this.l10NSharpExtender1.SetLocalizingId(this._keymanConfigurationLink, "WSKeyboardControl.KeymanConfigurationLink");
			this._keymanConfigurationLink.Location = new System.Drawing.Point(303, 119);
			this._keymanConfigurationLink.Name = "_keymanConfigurationLink";
			this._keymanConfigurationLink.Size = new System.Drawing.Size(110, 13);
			this._keymanConfigurationLink.TabIndex = 5;
			this._keymanConfigurationLink.TabStop = true;
			this._keymanConfigurationLink.Text = "Keyman Configuration";
			this._keymanConfigurationLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._keymanConfigurationLink_LinkClicked);
			//
			// _keyboardSettingsLink
			//
			this._keyboardSettingsLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._keyboardSettingsLink.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._keyboardSettingsLink, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._keyboardSettingsLink, null);
			this.l10NSharpExtender1.SetLocalizingId(this._keyboardSettingsLink, "WSKeyboardControl.KeyboardSettingsLink");
			this._keyboardSettingsLink.Location = new System.Drawing.Point(303, 84);
			this._keyboardSettingsLink.Name = "_keyboardSettingsLink";
			this._keyboardSettingsLink.Size = new System.Drawing.Size(137, 13);
			this._keyboardSettingsLink.TabIndex = 4;
			this._keyboardSettingsLink.TabStop = true;
			this._keyboardSettingsLink.Text = "Windows keyboard settings";
			this._keyboardSettingsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._windowsKeyboardSettingsLink_LinkClicked);
			//
			// label3
			//
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label3, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label3, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label3, "WSKeyboardControl.KeyboardNotListed");
			this.label3.Location = new System.Drawing.Point(303, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(154, 60);
			this.label3.TabIndex = 3;
			this.label3.Text = "If the keyboard you need is not listed, click the appropriate link below to set it up";
			//
			// _possibleKeyboardsList
			//
			this._possibleKeyboardsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._possibleKeyboardsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this._keyboards});
			this._possibleKeyboardsList.FullRowSelect = true;
			this._possibleKeyboardsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this._possibleKeyboardsList.HideSelection = false;
			this._possibleKeyboardsList.LabelWrap = false;
			this._possibleKeyboardsList.Location = new System.Drawing.Point(0, 24);
			this._possibleKeyboardsList.MultiSelect = false;
			this._possibleKeyboardsList.Name = "_possibleKeyboardsList";
			this._possibleKeyboardsList.Size = new System.Drawing.Size(297, 146);
			this._possibleKeyboardsList.TabIndex = 2;
			this._possibleKeyboardsList.UseCompatibleStateImageBehavior = false;
			this._possibleKeyboardsList.View = System.Windows.Forms.View.Details;
			this._possibleKeyboardsList.SelectedIndexChanged += new System.EventHandler(this._possibleKeyboardsList_SelectedIndexChanged);
			//
			// _keyboards
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this._keyboards, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._keyboards, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._keyboards, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this._keyboards, "_keyboards");
			this._keyboards.Text = "Keyboards";
			//
			// _selectKeyboardLabel
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this._selectKeyboardLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._selectKeyboardLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._selectKeyboardLabel, "WSKeyboardControl.SelectKeyboardLabel");
			this._selectKeyboardLabel.Location = new System.Drawing.Point(3, 0);
			this._selectKeyboardLabel.Name = "_selectKeyboardLabel";
			this._selectKeyboardLabel.Size = new System.Drawing.Size(454, 21);
			this._selectKeyboardLabel.TabIndex = 0;
			this._selectKeyboardLabel.Text = "Select the &keyboard with which to type {0} text";
			//
			// label1
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label1, "WSKeyboardControl.TestAreaCaption");
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(454, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Test Area (Use this area to type something to test out your keyboard.)";
			//
			// _testArea
			//
			this._testArea.AcceptsReturn = true;
			this._testArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._testArea.Location = new System.Drawing.Point(0, 16);
			this._testArea.Multiline = true;
			this._testArea.Name = "_testArea";
			this._testArea.Size = new System.Drawing.Size(460, 96);
			this._testArea.TabIndex = 1;
			this._testArea.Enter += new System.EventHandler(this._testArea_Enter);
			this._testArea.Leave += new System.EventHandler(this._testArea_Leave);
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Palaso";
			this.l10NSharpExtender1.PrefixForNewItems = "WSKeyboardControl";
			//
			// WSKeyboardControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "WSKeyboardControl.WSKeyboardControl");
			this.Name = "WSKeyboardControl";
			this.Size = new System.Drawing.Size(460, 297);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Label _selectKeyboardLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _testArea;
		private System.Windows.Forms.ListView _possibleKeyboardsList;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.LinkLabel _keymanConfigurationLink;
		private System.Windows.Forms.LinkLabel _keyboardSettingsLink;
		private System.Windows.Forms.ColumnHeader _keyboards;
		private L10NSharp.Windows.Forms.L10NSharpExtender l10NSharpExtender1;
	}
}
