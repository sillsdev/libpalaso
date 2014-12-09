namespace SIL.WritingSystems.WindowsForms.WSTree
{
	partial class GetDialectNameDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._dialectName = new System.Windows.Forms.TextBox();
			this._okButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _dialectName
			//
			this._dialectName.Location = new System.Drawing.Point(116, 43);
			this._dialectName.Name = "_dialectName";
			this._dialectName.Size = new System.Drawing.Size(100, 20);
			this._dialectName.TabIndex = 0;
			//
			// _okButton
			//
			this._okButton.Location = new System.Drawing.Point(141, 85);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 46);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(71, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Dialect Name";
			//
			// GetDialectNameDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(245, 131);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._dialectName);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GetDialectNameDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Get Dialect Name";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _dialectName;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Label label1;
	}
}