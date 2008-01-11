namespace Palaso.Services.Dictionary.SampleClient
{
	partial class AddEntry
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this._word = new System.Windows.Forms.TextBox();
			this._definition = new System.Windows.Forms.TextBox();
			this._example = new System.Windows.Forms.TextBox();
			this._addButton = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																				  | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 106F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._word, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this._definition, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this._example, 1, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 20);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(373, 103);
			this.tableLayoutPanel1.TabIndex = 0;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(33, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Word";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 31);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(51, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Definition";
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 62);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(96, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Example Sentence";
			//
			// _word
			//
			this._word.Dock = System.Windows.Forms.DockStyle.Fill;
			this._word.Location = new System.Drawing.Point(109, 3);
			this._word.Name = "_word";
			this._word.Size = new System.Drawing.Size(261, 20);
			this._word.TabIndex = 0;
			//
			// _definition
			//
			this._definition.Dock = System.Windows.Forms.DockStyle.Fill;
			this._definition.Location = new System.Drawing.Point(109, 34);
			this._definition.Name = "_definition";
			this._definition.Size = new System.Drawing.Size(261, 20);
			this._definition.TabIndex = 1;
			//
			// _example
			//
			this._example.Dock = System.Windows.Forms.DockStyle.Fill;
			this._example.Location = new System.Drawing.Point(109, 65);
			this._example.Name = "_example";
			this._example.Size = new System.Drawing.Size(261, 20);
			this._example.TabIndex = 2;
			//
			// _addButton
			//
			this._addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._addButton.Location = new System.Drawing.Point(301, 129);
			this._addButton.Name = "_addButton";
			this._addButton.Size = new System.Drawing.Size(75, 23);
			this._addButton.TabIndex = 0;
			this._addButton.Text = "Add";
			this._addButton.UseVisualStyleBackColor = true;
			this._addButton.Click += new System.EventHandler(this.OnAddButton_Click);
			//
			// AddEntry
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._addButton);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "AddEntry";
			this.Size = new System.Drawing.Size(403, 157);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _word;
		private System.Windows.Forms.TextBox _definition;
		private System.Windows.Forms.TextBox _example;
		private System.Windows.Forms.Button _addButton;
	}
}