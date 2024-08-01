namespace SIL.Windows.Forms.Media
{
	partial class SoundFieldControl
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
			this._stopButton = new System.Windows.Forms.Button();
			this._playButton = new System.Windows.Forms.Button();
			this._recordButton = new System.Windows.Forms.Button();
			this._timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			//
			// _stopButton
			//
			this._stopButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._stopButton.Image = global::SIL.Windows.Forms.Media.Properties.Resources.stop15x16;
			this._stopButton.Location = new System.Drawing.Point(82, 0);
			this._stopButton.Name = "_stopButton";
			this._stopButton.Size = new System.Drawing.Size(38, 31);
			this._stopButton.TabIndex = 3;
			this._stopButton.TabStop = false;
			this._stopButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this._stopButton.UseVisualStyleBackColor = true;
			this._stopButton.Click += new System.EventHandler(this._stopButton_Click);
			//
			// _playButton
			//
			this._playButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._playButton.Image = global::SIL.Windows.Forms.Media.Properties.Resources.play14x16;
			this._playButton.Location = new System.Drawing.Point(0, 0);
			this._playButton.Name = "_playButton";
			this._playButton.Size = new System.Drawing.Size(38, 31);
			this._playButton.TabIndex = 2;
			this._playButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this._playButton.UseVisualStyleBackColor = true;
			this._playButton.Click += new System.EventHandler(this._playButton_Click);
			//
			// _recordButton
			//
			this._recordButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._recordButton.Image = global::SIL.Windows.Forms.Media.Properties.Resources.record16x16;
			this._recordButton.Location = new System.Drawing.Point(41, 0);
			this._recordButton.Name = "_recordButton";
			this._recordButton.Size = new System.Drawing.Size(38, 31);
			this._recordButton.TabIndex = 1;
			this._recordButton.TabStop = false;
			this._recordButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this._recordButton.UseVisualStyleBackColor = true;
			this._recordButton.Click += new System.EventHandler(this._recordButton_Click);
			//
			// _timer
			//
			this._timer.Tick += new System.EventHandler(this.timer1_Tick);
			//
			// SoundFieldControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._stopButton);
			this.Controls.Add(this._playButton);
			this.Controls.Add(this._recordButton);
			this.Name = "SoundFieldControl";
			this.Size = new System.Drawing.Size(150, 34);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _stopButton;
		private System.Windows.Forms.Button _playButton;
		private System.Windows.Forms.Button _recordButton;
		private System.Windows.Forms.Timer _timer;
	}
}
