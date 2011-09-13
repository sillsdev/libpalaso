namespace Palaso.Media.Naudio.UI
{
	partial class RecordingDeviceButton
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
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._recordingDeviceButton = new BitmapButton();
			this.SuspendLayout();
			//
			// _recordingDeviceButton
			//
			this._recordingDeviceButton.BorderColor = System.Drawing.Color.Transparent;
			this._recordingDeviceButton.FlatAppearance.BorderSize = 0;
			this._recordingDeviceButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._recordingDeviceButton.FocusRectangleEnabled = true;
			this._recordingDeviceButton.Image = null;
			this._recordingDeviceButton.ImageBorderColor = System.Drawing.Color.Transparent;
			this._recordingDeviceButton.ImageBorderEnabled = false;
			this._recordingDeviceButton.ImageDropShadow = false;
			this._recordingDeviceButton.ImageFocused = null;
			this._recordingDeviceButton.ImageInactive = null;
			this._recordingDeviceButton.ImageMouseOver = null;
			this._recordingDeviceButton.ImageNormal = global::Palaso.Media.Naudio.UI.AudioDeviceIcons.Microphone;
			this._recordingDeviceButton.ImagePressed = null;
			this._recordingDeviceButton.InnerBorderColor = System.Drawing.Color.LightGray;
			this._recordingDeviceButton.InnerBorderColor_Focus = System.Drawing.Color.LightBlue;
			this._recordingDeviceButton.InnerBorderColor_MouseOver = System.Drawing.Color.Gold;
			this._recordingDeviceButton.Location = new System.Drawing.Point(0, 0);
			this._recordingDeviceButton.Name = "_recordingDeviceButton";
			this._recordingDeviceButton.OffsetPressedContent = true;
			this._recordingDeviceButton.Size = new System.Drawing.Size(30, 36);
			this._recordingDeviceButton.StretchImage = false;
			this._recordingDeviceButton.TabIndex = 24;
			this._recordingDeviceButton.TextDropShadow = false;
			this._recordingDeviceButton.UseVisualStyleBackColor = true;
			//
			// RecordingDeviceButton
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._recordingDeviceButton);
			this.Name = "RecordingDeviceButton";
			this.Size = new System.Drawing.Size(163, 39);
			this.ResumeLayout(false);

		}

		#endregion

		private BitmapButton _recordingDeviceButton;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}
