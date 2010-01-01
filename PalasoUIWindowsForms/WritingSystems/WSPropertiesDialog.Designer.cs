namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSPropertiesDialog
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
			this._writingSystemSetupView = new Palaso.UI.WindowsForms.WritingSystems.WritingSystemSetupView();
			this._closeButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _writingSystemSetupView
			//
			this._writingSystemSetupView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._writingSystemSetupView.Location = new System.Drawing.Point(1, 12);
			this._writingSystemSetupView.Name = "_writingSystemSetupView";
			this._writingSystemSetupView.Size = new System.Drawing.Size(841, 461);
			this._writingSystemSetupView.TabIndex = 0;
			//
			// _closeButton
			//
			this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._closeButton.Location = new System.Drawing.Point(767, 444);
			this._closeButton.Name = "_closeButton";
			this._closeButton.Size = new System.Drawing.Size(75, 23);
			this._closeButton.TabIndex = 1;
			this._closeButton.Text = "Close";
			this._closeButton.UseVisualStyleBackColor = true;
			this._closeButton.Click += new System.EventHandler(this._closeButton_Click);
			//
			// WSPropertiesDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(855, 479);
			this.Controls.Add(this._closeButton);
			this.Controls.Add(this._writingSystemSetupView);
			this.Name = "WSPropertiesDialog";
			this.Text = "Writing Systems";
			this.ResumeLayout(false);

		}

		#endregion

		private WritingSystemSetupView _writingSystemSetupView;
		private System.Windows.Forms.Button _closeButton;
	}
}