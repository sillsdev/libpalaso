// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2022, SIL International.
// <copyright from='2011' to='2022' company='SIL International'>
//		Copyright (c) 2022, SIL International.
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright>
#endregion
//
// File: ScrReferenceFilterDlg.Designer.cs
// ---------------------------------------------------------------------------------------------
namespace SIL.Windows.Forms.TestApp
{
	partial class ScrReferenceFilterDlg
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
			System.Diagnostics.Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType() + ". ****** ");
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
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.toolStripFrom = new System.Windows.Forms.ToolStrip();
			this.toolStripLabelFrom = new System.Windows.Forms.ToolStripLabel();
			this.scrPsgFrom = new SIL.Windows.Forms.Scripture.ToolStripVerseControl();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabelTo = new System.Windows.Forms.ToolStripLabel();
			this.scrPsgTo = new SIL.Windows.Forms.Scripture.ToolStripVerseControl();
			this.m_lblInvalidReference = new System.Windows.Forms.Label();
			this.m_timerWarning = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.toolStripFrom.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.btnOk, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.btnOk, null);
			this.l10NSharpExtender1.SetLocalizingId(this.btnOk, "Common.OK");
			this.btnOk.Location = new System.Drawing.Point(96, 91);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 5;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.btnCancel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.btnCancel, null);
			this.l10NSharpExtender1.SetLocalizingId(this.btnCancel, "Common.Cancel");
			this.btnCancel.Location = new System.Drawing.Point(177, 91);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 6;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Transcelerator";
			this.l10NSharpExtender1.PrefixForNewItems = null;
			// 
			// toolStripFrom
			// 
			this.toolStripFrom.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripFrom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelFrom,
            this.scrPsgFrom});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.toolStripFrom, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.toolStripFrom, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.toolStripFrom, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.toolStripFrom, "ScrReferenceFilterDlg.toolStripFrom");
			this.toolStripFrom.Location = new System.Drawing.Point(8, 8);
			this.toolStripFrom.Name = "toolStripFrom";
			this.toolStripFrom.Size = new System.Drawing.Size(248, 27);
			this.toolStripFrom.TabIndex = 7;
			// 
			// toolStripLabelFrom
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.toolStripLabelFrom, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.toolStripLabelFrom, null);
			this.l10NSharpExtender1.SetLocalizingId(this.toolStripLabelFrom, "ScrReferenceFilterDlg.m_lblFrom");
			this.toolStripLabelFrom.Name = "toolStripLabelFrom";
			this.toolStripLabelFrom.Size = new System.Drawing.Size(38, 24);
			this.toolStripLabelFrom.Text = "&From:";
			// 
			// scrPsgFrom
			// 
			this.scrPsgFrom.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.scrPsgFrom.BackColor = System.Drawing.SystemColors.Control;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.scrPsgFrom, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.scrPsgFrom, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.scrPsgFrom, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.scrPsgFrom, "ScrReferenceFilterDlg.scrPsgFrom");
			this.scrPsgFrom.Name = "scrPsgFrom";
			this.scrPsgFrom.Size = new System.Drawing.Size(191, 24);
			this.scrPsgFrom.Leave += new System.EventHandler(this.OnScrPassageLeave);
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelTo,
            this.scrPsgTo});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.toolStrip1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.toolStrip1, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.toolStrip1, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.toolStrip1, "ScrReferenceFilterDlg.toolStrip1");
			this.toolStrip1.Location = new System.Drawing.Point(8, 35);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(248, 27);
			this.toolStrip1.TabIndex = 8;
			// 
			// toolStripLabelTo
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.toolStripLabelTo, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.toolStripLabelTo, null);
			this.l10NSharpExtender1.SetLocalizingId(this.toolStripLabelTo, "ScrReferenceFilterDlg.m_lblTo");
			this.toolStripLabelTo.Name = "toolStripLabelTo";
			this.toolStripLabelTo.Size = new System.Drawing.Size(23, 24);
			this.toolStripLabelTo.Text = "&To:";
			// 
			// scrPsgTo
			// 
			this.scrPsgTo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.scrPsgTo.BackColor = System.Drawing.SystemColors.Control;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.scrPsgTo, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.scrPsgTo, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.scrPsgTo, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.scrPsgTo, "ScrReferenceFilterDlg.scrPsgTo");
			this.scrPsgTo.Name = "scrPsgTo";
			this.scrPsgTo.Size = new System.Drawing.Size(191, 24);
			this.scrPsgTo.Leave += new System.EventHandler(this.OnScrPassageLeave);
			// 
			// m_lblInvalidReference
			// 
			this.m_lblInvalidReference.AutoSize = true;
			this.m_lblInvalidReference.ForeColor = System.Drawing.Color.Red;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblInvalidReference, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblInvalidReference, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblInvalidReference, "ScrReferenceFilterDlg.m_lblInvalidReference");
			this.m_lblInvalidReference.Location = new System.Drawing.Point(67, 68);
			this.m_lblInvalidReference.Name = "m_lblInvalidReference";
			this.m_lblInvalidReference.Size = new System.Drawing.Size(131, 13);
			this.m_lblInvalidReference.TabIndex = 9;
			this.m_lblInvalidReference.Text = "Invalid Scripture reference";
			this.m_lblInvalidReference.Visible = false;
			// 
			// m_timerWarning
			// 
			this.m_timerWarning.Tick += new System.EventHandler(this.ResetOkButtonAndStartToFadeWarning);
			// 
			// ScrReferenceFilterDlg
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(264, 126);
			this.Controls.Add(this.m_lblInvalidReference);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.toolStripFrom);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "ScrReferenceFilterDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(320, 183);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(280, 153);
			this.Name = "ScrReferenceFilterDlg";
			this.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Filter by Reference";
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.toolStripFrom.ResumeLayout(false);
			this.toolStripFrom.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.ToolStrip toolStripFrom;
		private System.Windows.Forms.ToolStripLabel toolStripLabelFrom;
		private Windows.Forms.Scripture.ToolStripVerseControl scrPsgFrom;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel toolStripLabelTo;
		private Windows.Forms.Scripture.ToolStripVerseControl scrPsgTo;
		private System.Windows.Forms.Label m_lblInvalidReference;
		private System.Windows.Forms.Timer m_timerWarning;
	}
}