using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
{
	partial class WSIdentifierView
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
			this._specialTypeComboBox = new System.Windows.Forms.ComboBox();
			this._abbreviation = new System.Windows.Forms.TextBox();
			this._detailPanel = new System.Windows.Forms.Panel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.betterLabel5 = new BetterLabel();
			this.betterLabel4 = new BetterLabel();
			this.SuspendLayout();
			//
			// _specialTypeComboBox
			//
			this._specialTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._specialTypeComboBox.FormattingEnabled = true;
			this._specialTypeComboBox.Location = new System.Drawing.Point(102, 56);
			this._specialTypeComboBox.Name = "_specialTypeComboBox";
			this._specialTypeComboBox.Size = new System.Drawing.Size(263, 21);
			this._specialTypeComboBox.TabIndex = 8;
			this._specialTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.specialTypeComboBox_SelectedIndexChanged);
			//
			// _abbreviation
			//
			this._abbreviation.Location = new System.Drawing.Point(102, 22);
			this._abbreviation.Name = "_abbreviation";
			this._abbreviation.Size = new System.Drawing.Size(46, 20);
			this._abbreviation.TabIndex = 12;
			this.toolTip1.SetToolTip(this._abbreviation, "Abbreviation to use in field labels");
			this._abbreviation.TextChanged += new System.EventHandler(this._abbreviation_TextChanged);
			//
			// _detailPanel
			//
			this._detailPanel.Location = new System.Drawing.Point(102, 80);
			this._detailPanel.Name = "_detailPanel";
			this._detailPanel.Size = new System.Drawing.Size(264, 140);
			this._detailPanel.TabIndex = 13;
			//
			// betterLabel5
			//
			this.betterLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel5.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel5.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel5.Location = new System.Drawing.Point(13, 59);
			this.betterLabel5.Multiline = true;
			this.betterLabel5.Name = "betterLabel5";
			this.betterLabel5.ReadOnly = true;
			this.betterLabel5.Size = new System.Drawing.Size(75, 20);
			this.betterLabel5.TabIndex = 7;
			this.betterLabel5.TabStop = false;
			this.betterLabel5.Text = "Special:";
			//
			// betterLabel4
			//
			this.betterLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel4.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel4.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel4.Location = new System.Drawing.Point(13, 22);
			this.betterLabel4.Multiline = true;
			this.betterLabel4.Name = "betterLabel4";
			this.betterLabel4.ReadOnly = true;
			this.betterLabel4.Size = new System.Drawing.Size(75, 20);
			this.betterLabel4.TabIndex = 6;
			this.betterLabel4.TabStop = false;
			this.betterLabel4.Text = "Abbreviation";
			//
			// WSIdentifierView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._detailPanel);
			this.Controls.Add(this._abbreviation);
			this.Controls.Add(this._specialTypeComboBox);
			this.Controls.Add(this.betterLabel5);
			this.Controls.Add(this.betterLabel4);
			this.Name = "WSIdentifierView";
			this.Size = new System.Drawing.Size(381, 270);
			this.VisibleChanged += new System.EventHandler(this.OnVisibleChanged);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BetterLabel betterLabel4;
		private BetterLabel betterLabel5;
		private System.Windows.Forms.ComboBox _specialTypeComboBox;
		private System.Windows.Forms.TextBox _abbreviation;
		private System.Windows.Forms.Panel _detailPanel;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}
