namespace Palaso.UI.WindowsForms.WritingSystems
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this._testArea = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this._keyboardComboBox = new System.Windows.Forms.ComboBox();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
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
			this.splitContainer1.Panel1.Controls.Add(this._keyboardComboBox);
			this.splitContainer1.Panel1.Controls.Add(this.label2);
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
			this._testArea.Text = "Use this area to type something to test out your keyboard.";
			this._testArea.Leave += new System.EventHandler(this._testArea_Leave);
			this._testArea.Enter += new System.EventHandler(this._testArea_Enter);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Test Area:";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "&Keyboards:";
			//
			// _keyboardComboBox
			//
			this._keyboardComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._keyboardComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
			this._keyboardComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this._keyboardComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
			this._keyboardComboBox.IntegralHeight = false;
			this._keyboardComboBox.Location = new System.Drawing.Point(0, 16);
			this._keyboardComboBox.Name = "_keyboardComboBox";
			this._keyboardComboBox.Size = new System.Drawing.Size(460, 166);
			this._keyboardComboBox.TabIndex = 1;
			this._keyboardComboBox.TextChanged += new System.EventHandler(this._keyboardComboBox_TextChanged);
			//
			// WSKeyboardControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "WSKeyboardControl";
			this.Size = new System.Drawing.Size(460, 297);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ComboBox _keyboardComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _testArea;
	}
}
