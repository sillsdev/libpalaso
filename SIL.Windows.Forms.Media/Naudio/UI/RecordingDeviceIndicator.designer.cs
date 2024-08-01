namespace SIL.Windows.Forms.Media.Naudio.UI
{
	partial class RecordingDeviceIndicator
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecordingDeviceIndicator));
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._recordingDeviceImage = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this._recordingDeviceImage)).BeginInit();
			this.SuspendLayout();
			//
			// _recordingDeviceImage
			//
			this._recordingDeviceImage.Image = ((System.Drawing.Image)(resources.GetObject("_recordingDeviceImage.Image")));
			this._recordingDeviceImage.Location = new System.Drawing.Point(0, 0);
			this._recordingDeviceImage.Margin = new System.Windows.Forms.Padding(0);
			this._recordingDeviceImage.Name = "_recordingDeviceImage";
			this._recordingDeviceImage.Size = new System.Drawing.Size(16, 16);
			this._recordingDeviceImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this._recordingDeviceImage.TabIndex = 0;
			this._recordingDeviceImage.TabStop = false;
			this._recordingDeviceImage.Click += new System.EventHandler(this.RecordingDeviceIndicator_Click);
			//
			// RecordingDeviceIndicator
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this._recordingDeviceImage);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "RecordingDeviceIndicator";
			this.Size = new System.Drawing.Size(20, 26);
			this.Click += new System.EventHandler(this.RecordingDeviceIndicator_Click);
			((System.ComponentModel.ISupportInitialize)(this._recordingDeviceImage)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.PictureBox _recordingDeviceImage;
	}
}
