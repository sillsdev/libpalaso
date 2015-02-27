using System;

namespace SIL.Windows.Forms.WritingSystems
{
	partial class WritingSystemSetupDialog
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
				if (DisposeRepository)
				{
					var diposable = _model.WritingSystems as IDisposable;
					if (diposable != null)
						diposable.Dispose();
				}
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
			this._writingSystemSetupView = new WritingSystemSetupView();
			this._closeButton = new System.Windows.Forms.Button();
			this._openGlobal = new System.Windows.Forms.Button();
			this._openDirectory = new System.Windows.Forms.Button();
			this._openLabel = new System.Windows.Forms.Label();
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
			// _openGlobal
			//
			this._openGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._openGlobal.Location = new System.Drawing.Point(521, 444);
			this._openGlobal.Name = "_openGlobal";
			this._openGlobal.Size = new System.Drawing.Size(75, 23);
			this._openGlobal.TabIndex = 2;
			this._openGlobal.Text = "Computer";
			this._openGlobal.UseVisualStyleBackColor = true;
			this._openGlobal.Click += new System.EventHandler(this._openGlobal_Click);
			//
			// _openDirectory
			//
			this._openDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._openDirectory.Location = new System.Drawing.Point(602, 444);
			this._openDirectory.Name = "_openDirectory";
			this._openDirectory.Size = new System.Drawing.Size(75, 23);
			this._openDirectory.TabIndex = 4;
			this._openDirectory.Text = "Directory";
			this._openDirectory.UseVisualStyleBackColor = true;
			this._openDirectory.Click += new System.EventHandler(this._openDirectory_Click);
			//
			// _openLabel
			//
			this._openLabel.AutoSize = true;
			this._openLabel.Location = new System.Drawing.Point(380, 448);
			this._openLabel.Name = "_openLabel";
			this._openLabel.Size = new System.Drawing.Size(36, 13);
			this._openLabel.TabIndex = 10;
			this._openLabel.Text = "Open Writing Systems:";
			//
			// WritingSystemSetupDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(855, 479);
			this.Controls.Add(this._openLabel);
			this.Controls.Add(this._openDirectory);
			this.Controls.Add(this._openGlobal);
			this.Controls.Add(this._closeButton);
			this.Controls.Add(this._writingSystemSetupView);
			this.Name = "WritingSystemSetupDialog";
			this.Text = "Writing Systems";
			this.ResumeLayout(false);

		}

		#endregion

		private WritingSystemSetupView _writingSystemSetupView;
		private System.Windows.Forms.Button _closeButton;
		private System.Windows.Forms.Button _openGlobal;
		private System.Windows.Forms.Button _openDirectory;
		private System.Windows.Forms.Label _openLabel;
	}
}