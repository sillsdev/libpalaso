using Palaso.UI.WindowsForms.Properties;

namespace Palaso.UI.WindowsForms.FileSystem
{
	partial class ConfirmRecycleDialog
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfirmRecycleDialog));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.localizationHelper1 = new Palaso.UI.WindowsForms.i18n.LocalizationHelper(this.components);
			this.deleteBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this._messageLabel = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.localizationHelper1)).BeginInit();
			this.SuspendLayout();
			//
			// pictureBox1
			//
			this.pictureBox1.Image = Resources.RecycleBin;
			this.pictureBox1.Location = new System.Drawing.Point(16, 24);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(102, 102);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			//
			// localizationHelper1
			//
			this.localizationHelper1.Parent = this;
			//
			// deleteBtn
			//
			this.deleteBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.deleteBtn.Image = Resources.DeleteMessageBoxButtonImage;
			this.deleteBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.deleteBtn.Location = new System.Drawing.Point(134, 132);
			this.deleteBtn.Name = "deleteBtn";
			this.deleteBtn.Size = new System.Drawing.Size(121, 33);
			this.deleteBtn.TabIndex = 1;
			this.deleteBtn.Text = "&Delete";
			this.deleteBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.deleteBtn.UseVisualStyleBackColor = true;
			this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
			//
			// cancelBtn
			//
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.cancelBtn.Location = new System.Drawing.Point(271, 132);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(108, 33);
			this.cancelBtn.TabIndex = 0;
			this.cancelBtn.Text = "&Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			//
			// _messageLabel
			//
			this._messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._messageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._messageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._messageLabel.Location = new System.Drawing.Point(134, 24);
			this._messageLabel.Multiline = true;
			this._messageLabel.Name = "_messageLabel";
			this._messageLabel.ReadOnly = true;
			this._messageLabel.Size = new System.Drawing.Size(245, 87);
			this._messageLabel.TabIndex = 4;
			this._messageLabel.TabStop = false;
			this._messageLabel.Text = "{0} will be moved to the Recycle Bin.";
			//
			// ConfirmDelete
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(399, 177);
			this.ControlBox = false;
			this.Controls.Add(this._messageLabel);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.deleteBtn);
			this.Controls.Add(this.pictureBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ConfirmRecycleDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Confirm Delete";
			this.Load += new System.EventHandler(this.ConfirmDelete_Load);
			this.BackColorChanged += new System.EventHandler(this.ConfirmDelete_BackColorChanged);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.localizationHelper1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private Palaso.UI.WindowsForms.i18n.LocalizationHelper localizationHelper1;
		private System.Windows.Forms.Button deleteBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.TextBox _messageLabel;
	}
}