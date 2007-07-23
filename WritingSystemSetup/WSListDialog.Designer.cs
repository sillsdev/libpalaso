namespace Palaso
{
	partial class WSListDialog
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
			this._closeButton = new System.Windows.Forms.Button();
			this._writingSystemList = new WeSay.UI.ControlListBox();
			this.SuspendLayout();
			//
			// _closeButton
			//
			this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._closeButton.Location = new System.Drawing.Point(500, 290);
			this._closeButton.Name = "_closeButton";
			this._closeButton.Size = new System.Drawing.Size(75, 23);
			this._closeButton.TabIndex = 1;
			this._closeButton.Text = "&Close";
			this._closeButton.UseVisualStyleBackColor = true;
			this._closeButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _writingSystemList
			//
			this._writingSystemList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._writingSystemList.Location = new System.Drawing.Point(12, 22);
			this._writingSystemList.Name = "_writingSystemList";
			this._writingSystemList.Size = new System.Drawing.Size(563, 262);
			this._writingSystemList.TabIndex = 0;
			this._writingSystemList.Load += new System.EventHandler(this.controlListBox1_Load);
			//
			// WSListDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ClientSize = new System.Drawing.Size(587, 325);
			this.ControlBox = false;
			this.Controls.Add(this._closeButton);
			this.Controls.Add(this._writingSystemList);
			this.Name = "WSListDialog";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Setup Writing Systems...";
			this.ResizeBegin += new System.EventHandler(this.WSListDialog_ResizeBegin);
			this.Resize += new System.EventHandler(this.WSListDialog_Resize);
			this.ResumeLayout(false);

		}

		#endregion

		private WeSay.UI.ControlListBox _writingSystemList;
		private System.Windows.Forms.Button _closeButton;
	}
}