namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
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
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this._abbreviation = new System.Windows.Forms.TextBox();
			this._detailPanel = new System.Windows.Forms.Panel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.betterLabel5 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel4 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.SuspendLayout();
			//
			// comboBox1
			//
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(102, 56);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(263, 21);
			this.comboBox1.TabIndex = 8;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
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
			this._detailPanel.Size = new System.Drawing.Size(264, 100);
			this._detailPanel.TabIndex = 13;
			//
			// betterLabel5
			//
			this.betterLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel5.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel5.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel5.Location = new System.Drawing.Point(22, 59);
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
			this.betterLabel4.Location = new System.Drawing.Point(22, 22);
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
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.betterLabel5);
			this.Controls.Add(this.betterLabel4);
			this.Name = "WSIdentifierView";
			this.Size = new System.Drawing.Size(381, 229);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel4;
		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel5;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.TextBox _abbreviation;
		private System.Windows.Forms.Panel _detailPanel;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}
