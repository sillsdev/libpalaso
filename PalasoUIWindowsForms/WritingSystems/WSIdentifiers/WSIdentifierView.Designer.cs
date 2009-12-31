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
			this._code = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this._name = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel5 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel4 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel3 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel2 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.SuspendLayout();
			//
			// comboBox1
			//
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(102, 107);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(263, 21);
			this.comboBox1.TabIndex = 8;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			//
			// _abbreviation
			//
			this._abbreviation.Location = new System.Drawing.Point(102, 73);
			this._abbreviation.Name = "_abbreviation";
			this._abbreviation.Size = new System.Drawing.Size(46, 20);
			this._abbreviation.TabIndex = 12;
			this.toolTip1.SetToolTip(this._abbreviation, "Abbreviation to use in field labels");
			//
			// _detailPanel
			//
			this._detailPanel.Location = new System.Drawing.Point(102, 131);
			this._detailPanel.Name = "_detailPanel";
			this._detailPanel.Size = new System.Drawing.Size(264, 100);
			this._detailPanel.TabIndex = 13;
			//
			// _code
			//
			this._code.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._code.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._code.Location = new System.Drawing.Point(102, 45);
			this._code.Multiline = true;
			this._code.Name = "_code";
			this._code.ReadOnly = true;
			this._code.Size = new System.Drawing.Size(67, 20);
			this._code.TabIndex = 10;
			this._code.TabStop = false;
			this._code.Text = "xyz";
			//
			// _name
			//
			this._name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._name.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._name.Location = new System.Drawing.Point(102, 19);
			this._name.Multiline = true;
			this._name.Name = "_name";
			this._name.ReadOnly = true;
			this._name.Size = new System.Drawing.Size(67, 20);
			this._name.TabIndex = 9;
			this._name.TabStop = false;
			this._name.Text = "blahblah";
			//
			// betterLabel5
			//
			this.betterLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel5.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.betterLabel5.Location = new System.Drawing.Point(22, 110);
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
			this.betterLabel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.betterLabel4.Location = new System.Drawing.Point(22, 73);
			this.betterLabel4.Multiline = true;
			this.betterLabel4.Name = "betterLabel4";
			this.betterLabel4.ReadOnly = true;
			this.betterLabel4.Size = new System.Drawing.Size(75, 20);
			this.betterLabel4.TabIndex = 6;
			this.betterLabel4.TabStop = false;
			this.betterLabel4.Text = "Abbreviation";
			//
			// betterLabel3
			//
			this.betterLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel3.Location = new System.Drawing.Point(22, 45);
			this.betterLabel3.Multiline = true;
			this.betterLabel3.Name = "betterLabel3";
			this.betterLabel3.ReadOnly = true;
			this.betterLabel3.Size = new System.Drawing.Size(61, 20);
			this.betterLabel3.TabIndex = 5;
			this.betterLabel3.TabStop = false;
			this.betterLabel3.Text = "Code:";
			//
			// betterLabel2
			//
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Location = new System.Drawing.Point(22, 19);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(50, 20);
			this.betterLabel2.TabIndex = 4;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "Name:";
			//
			// WSIdentifierView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._detailPanel);
			this.Controls.Add(this._abbreviation);
			this.Controls.Add(this._code);
			this.Controls.Add(this._name);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.betterLabel5);
			this.Controls.Add(this.betterLabel4);
			this.Controls.Add(this.betterLabel3);
			this.Controls.Add(this.betterLabel2);
			this.Name = "WSIdentifierView";
			this.Size = new System.Drawing.Size(381, 261);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel3;
		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel2;
		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel4;
		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel5;
		private System.Windows.Forms.ComboBox comboBox1;
		private Palaso.UI.WindowsForms.Widgets.BetterLabel _name;
		private Palaso.UI.WindowsForms.Widgets.BetterLabel _code;
		private System.Windows.Forms.TextBox _abbreviation;
		private System.Windows.Forms.Panel _detailPanel;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}
