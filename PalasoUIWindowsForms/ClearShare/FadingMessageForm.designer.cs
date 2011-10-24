namespace Palaso.UI.WindowsForms.ClearShare
{
	partial class FadingMessageForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._labelMessage = new System.Windows.Forms.Label();
			this._tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this._pictureInfo = new System.Windows.Forms.PictureBox();
			this._tableLayout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._pictureInfo)).BeginInit();
			this.SuspendLayout();
			//
			// _labelMessage
			//
			this._labelMessage.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this._labelMessage.AutoSize = true;
			this._labelMessage.BackColor = System.Drawing.Color.Transparent;
			this._labelMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._labelMessage.Location = new System.Drawing.Point(19, 0);
			this._labelMessage.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this._labelMessage.Name = "_labelMessage";
			this._labelMessage.Size = new System.Drawing.Size(65, 15);
			this._labelMessage.TabIndex = 0;
			this._labelMessage.Text = "Enter Role";
			//
			// _tableLayout
			//
			this._tableLayout.AutoSize = true;
			this._tableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._tableLayout.BackColor = System.Drawing.Color.Transparent;
			this._tableLayout.ColumnCount = 2;
			this._tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._tableLayout.Controls.Add(this._labelMessage, 1, 0);
			this._tableLayout.Controls.Add(this._pictureInfo, 0, 0);
			this._tableLayout.Location = new System.Drawing.Point(12, 12);
			this._tableLayout.Name = "_tableLayout";
			this._tableLayout.RowCount = 1;
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayout.Size = new System.Drawing.Size(84, 16);
			this._tableLayout.TabIndex = 1;
			//
			// _pictureInfo
			//
			this._pictureInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this._pictureInfo.BackColor = System.Drawing.Color.Transparent;
			this._pictureInfo.Image =  ClearShareResources.Information;
			this._pictureInfo.Location = new System.Drawing.Point(0, 0);
			this._pictureInfo.Margin = new System.Windows.Forms.Padding(0);
			this._pictureInfo.Name = "_pictureInfo";
			this._pictureInfo.Size = new System.Drawing.Size(16, 16);
			this._pictureInfo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this._pictureInfo.TabIndex = 2;
			this._pictureInfo.TabStop = false;
			//
			// MissingDataMessageForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Magenta;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.ClientSize = new System.Drawing.Size(139, 46);
			this.ControlBox = false;
			this.Controls.Add(this._tableLayout);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MissingDataMessageForm";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TransparencyKey = System.Drawing.Color.Magenta;
			this._tableLayout.ResumeLayout(false);
			this._tableLayout.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._pictureInfo)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label _labelMessage;
		private System.Windows.Forms.TableLayoutPanel _tableLayout;
		private System.Windows.Forms.PictureBox _pictureInfo;
	}
}