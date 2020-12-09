namespace SIL.Windows.Forms.FileSystem
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
			this.pictureRecycleBin = new System.Windows.Forms.PictureBox();
			this.deleteBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this._messageLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureRecycleBin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureRecycleBin
			// 
			this.pictureRecycleBin.Image = global::SIL.Windows.Forms.Properties.Resources.RecycleBin;
			this._L10NSharpExtender.SetLocalizableToolTip(this.pictureRecycleBin, null);
			this._L10NSharpExtender.SetLocalizationComment(this.pictureRecycleBin, null);
			this._L10NSharpExtender.SetLocalizingId(this.pictureRecycleBin, "ConfirmRecycleDialog.pictureRecycleBin");
			this.pictureRecycleBin.Location = new System.Drawing.Point(20, 20);
			this.pictureRecycleBin.Margin = new System.Windows.Forms.Padding(0, 0, 20, 10);
			this.pictureRecycleBin.Name = "pictureRecycleBin";
			this.pictureRecycleBin.Size = new System.Drawing.Size(55, 64);
			this.pictureRecycleBin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureRecycleBin.TabIndex = 1;
			this.pictureRecycleBin.TabStop = false;
			// 
			// deleteBtn
			// 
			this.deleteBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.deleteBtn.AutoSize = true;
			this.deleteBtn.Image = global::SIL.Windows.Forms.Properties.Resources.DeleteMessageBoxButtonImage;
			this._L10NSharpExtender.SetLocalizableToolTip(this.deleteBtn, null);
			this._L10NSharpExtender.SetLocalizationComment(this.deleteBtn, null);
			this._L10NSharpExtender.SetLocalizingId(this.deleteBtn, "DialogBoxes.ConfirmRecycleDialog.deleteBtn");
			this.deleteBtn.Location = new System.Drawing.Point(284, 0);
			this.deleteBtn.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.deleteBtn.Name = "deleteBtn";
			this.deleteBtn.Size = new System.Drawing.Size(71, 26);
			this.deleteBtn.TabIndex = 0;
			this.deleteBtn.Text = "&Delete";
			this.deleteBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.deleteBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.deleteBtn.UseVisualStyleBackColor = true;
			this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.AutoSize = true;
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._L10NSharpExtender.SetLocalizableToolTip(this.cancelBtn, null);
			this._L10NSharpExtender.SetLocalizationComment(this.cancelBtn, null);
			this._L10NSharpExtender.SetLocalizingId(this.cancelBtn, "DialogBoxes.ConfirmRecycleDialog.cancelBtn");
			this.cancelBtn.Location = new System.Drawing.Point(363, 0);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(71, 26);
			this.cancelBtn.TabIndex = 1;
			this.cancelBtn.Text = "&Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// _L10NSharpExtender
			// 
			this._L10NSharpExtender.LocalizationManagerId = "SIL.Windows.Forms.FileSystem";
			this._L10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel1.Controls.Add(this.cancelBtn);
			this.flowLayoutPanel1.Controls.Add(this.deleteBtn);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(20, 148);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(434, 26);
			this.flowLayoutPanel1.TabIndex = 3;
			// 
			// _messageLabel
			// 
			this._L10NSharpExtender.SetLocalizableToolTip(this._messageLabel, null);
			this._L10NSharpExtender.SetLocalizationComment(this._messageLabel, "Param {0} is a file name");
			this._L10NSharpExtender.SetLocalizingId(this._messageLabel, "DialogBoxes.ConfirmRecycleDialog.MessageForSingleItem");
			this._messageLabel.Location = new System.Drawing.Point(95, 20);
			this._messageLabel.Margin = new System.Windows.Forms.Padding(0, 4, 0, 10);
			this._messageLabel.Name = "_messageLabel";
			this._messageLabel.Size = new System.Drawing.Size(359, 113);
			this._messageLabel.TabIndex = 2;
			this._messageLabel.Text = "{0} will be moved to the Recycle Bin.";
			// 
			// ConfirmRecycleDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(474, 194);
			this.ControlBox = false;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.pictureRecycleBin);
			this.Controls.Add(this._messageLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ConfirmRecycleDialog.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(480, 200);
			this.Name = "ConfirmRecycleDialog";
			this.Padding = new System.Windows.Forms.Padding(20);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Confirm Delete";
			((System.ComponentModel.ISupportInitialize)(this.pictureRecycleBin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureRecycleBin;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Button deleteBtn;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label _messageLabel;
	}
}