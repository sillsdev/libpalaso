namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
{
	partial class MetadataEditorDialog
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
			this.metdataEditorControl1 = new Palaso.UI.WindowsForms.ClearShare.WinFormsUI.MetdataEditorControl();
			this._okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// metdataEditorControl1
			//
			this.metdataEditorControl1.Location = new System.Drawing.Point(-1, 0);
			this.metdataEditorControl1.Metadata = null;
			this.metdataEditorControl1.Name = "metdataEditorControl1";
			this.metdataEditorControl1.Size = new System.Drawing.Size(338, 392);
			this.metdataEditorControl1.TabIndex = 0;
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(242, 394);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// MetadataEditorDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(338, 429);
			this.ControlBox = false;
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.metdataEditorControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "MetadataEditorDialog";
			this.Text = "Meta";
			this.ResumeLayout(false);

		}

		#endregion

		private MetdataEditorControl metdataEditorControl1;
		private System.Windows.Forms.Button _okButton;
	}
}