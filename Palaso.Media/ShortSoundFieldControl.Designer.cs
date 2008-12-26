namespace Palaso.Media
{
	partial class ShortSoundFieldControl
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
			this._timer = new System.Windows.Forms.Timer(this.components);
			this._playButton = new System.Windows.Forms.Button();
			this._deleteButton = new System.Windows.Forms.Button();
			this._recordButton = new System.Windows.Forms.Button();
			this._hint = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._poorMansWaveform = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _timer
			//
			this._timer.Interval = 200;
			this._timer.Tick += new System.EventHandler(this.timer1_Tick);
			//
			// _playButton
			//
			this._playButton.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
			this._playButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._playButton.Image = global::Palaso.Media.Properties.Resources.play14x16;
			this._playButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._playButton.Location = new System.Drawing.Point(0, 0);
			this._playButton.Name = "_playButton";
			this._playButton.Size = new System.Drawing.Size(38, 19);
			this._playButton.TabIndex = 2;
			this._playButton.TabStop = false;
			this._playButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.toolTip1.SetToolTip(this._playButton, "Click to play the recording.");
			this._playButton.UseVisualStyleBackColor = false;
			this._playButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnClickPlay);
			//
			// _deleteButton
			//
			this._deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._deleteButton.Enabled = false;
			this._deleteButton.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
			this._deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._deleteButton.Image = global::Palaso.Media.Properties.Resources.Delete;
			this._deleteButton.Location = new System.Drawing.Point(287, 0);
			this._deleteButton.Name = "_deleteButton";
			this._deleteButton.Size = new System.Drawing.Size(38, 19);
			this._deleteButton.TabIndex = 1;
			this._deleteButton.TabStop = false;
			this._deleteButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.toolTip1.SetToolTip(this._deleteButton, "set at runtime");
			this._deleteButton.UseVisualStyleBackColor = false;
			this._deleteButton.Click += new System.EventHandler(this.OnDeleteClick);
			//
			// _recordButton
			//
			this._recordButton.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
			this._recordButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._recordButton.Image = global::Palaso.Media.Properties.Resources.record16x16;
			this._recordButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._recordButton.Location = new System.Drawing.Point(41, 0);
			this._recordButton.Name = "_recordButton";
			this._recordButton.Size = new System.Drawing.Size(38, 19);
			this._recordButton.TabIndex = 1;
			this._recordButton.TabStop = false;
			this._recordButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.toolTip1.SetToolTip(this._recordButton, "Hold down the mouse button while talking, then release.  Like a walkie-talkie.");
			this._recordButton.UseVisualStyleBackColor = false;
			this._recordButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnRecordDown);
			this._recordButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnRecordUp);
			//
			// _hint
			//
			this._hint.AutoEllipsis = true;
			this._hint.AutoSize = true;
			this._hint.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._hint.Location = new System.Drawing.Point(82, 0);
			this._hint.Name = "_hint";
			this._hint.Size = new System.Drawing.Size(28, 14);
			this._hint.TabIndex = 3;
			this._hint.Text = "hint";
			//
			// toolTip1
			//
			this.toolTip1.AutomaticDelay = 300;
			this.toolTip1.AutoPopDelay = 15000;
			this.toolTip1.InitialDelay = 300;
			this.toolTip1.ReshowDelay = 60;
			//
			// _poorMansWaveform
			//
			this._poorMansWaveform.AutoSize = true;
			this._poorMansWaveform.ForeColor = System.Drawing.SystemColors.ControlDark;
			this._poorMansWaveform.Location = new System.Drawing.Point(122, 2);
			this._poorMansWaveform.Name = "_poorMansWaveform";
			this._poorMansWaveform.Size = new System.Drawing.Size(76, 13);
			this._poorMansWaveform.TabIndex = 4;
			this._poorMansWaveform.Text = "• • • • • • • •";
			//
			// ShortSoundFieldControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._poorMansWaveform);
			this.Controls.Add(this._hint);
			this.Controls.Add(this._playButton);
			this.Controls.Add(this._deleteButton);
			this.Controls.Add(this._recordButton);
			this.Name = "ShortSoundFieldControl";
			this.Size = new System.Drawing.Size(328, 22);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _playButton;
		private System.Windows.Forms.Button _recordButton;
		private System.Windows.Forms.Timer _timer;
		private System.Windows.Forms.Button _deleteButton;
		private System.Windows.Forms.Label _hint;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label _poorMansWaveform;
	}
}
