namespace Palaso.Reporting
{
	partial class ProblemNotificationDialog
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
			this._icon = new System.Windows.Forms.PictureBox();
			this._acceptButton = new System.Windows.Forms.Button();
			this._alternateButton1 = new System.Windows.Forms.Button();
			this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this._reoccurenceMessage = new System.Windows.Forms.Label();
			this._message = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this._icon)).BeginInit();
			this.tableLayout.SuspendLayout();
			this.SuspendLayout();
			//
			// _icon
			//
			this._icon.Location = new System.Drawing.Point(0, 0);
			this._icon.Margin = new System.Windows.Forms.Padding(0, 0, 15, 23);
			this._icon.Name = "_icon";
			this._icon.Size = new System.Drawing.Size(45, 36);
			this._icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this._icon.TabIndex = 1;
			this._icon.TabStop = false;
			//
			// _acceptButton
			//
			this._acceptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._acceptButton.AutoSize = true;
			this._acceptButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._acceptButton.Location = new System.Drawing.Point(328, 59);
			this._acceptButton.Margin = new System.Windows.Forms.Padding(4, 0, 0, 5);
			this._acceptButton.MinimumSize = new System.Drawing.Size(75, 26);
			this._acceptButton.Name = "_acceptButton";
			this._acceptButton.Size = new System.Drawing.Size(75, 26);
			this._acceptButton.TabIndex = 0;
			this._acceptButton.Text = "&OK";
			this._acceptButton.UseVisualStyleBackColor = true;
			this._acceptButton.Click += new System.EventHandler(this._acceptButton_Click);
			//
			// _alternateButton1
			//
			this._alternateButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._alternateButton1.AutoSize = true;
			this._alternateButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._alternateButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._alternateButton1.Location = new System.Drawing.Point(237, 59);
			this._alternateButton1.Margin = new System.Windows.Forms.Padding(6, 0, 4, 5);
			this._alternateButton1.MinimumSize = new System.Drawing.Size(75, 26);
			this._alternateButton1.Name = "_alternateButton1";
			this._alternateButton1.Size = new System.Drawing.Size(75, 26);
			this._alternateButton1.TabIndex = 3;
			this._alternateButton1.Text = "&Caller Defined";
			this._alternateButton1.UseVisualStyleBackColor = true;
			this._alternateButton1.Visible = false;
			this._alternateButton1.Click += new System.EventHandler(this.OnAlternateButton1_Click);
			//
			// tableLayout
			//
			this.tableLayout.AutoSize = true;
			this.tableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayout.BackColor = System.Drawing.Color.Transparent;
			//this.tableLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.tableLayout.ColumnCount = 4;
			this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayout.Controls.Add(this._reoccurenceMessage, 0, 1);
			this.tableLayout.Controls.Add(this._icon, 0, 0);
			this.tableLayout.Controls.Add(this._acceptButton, 3, 1);
			this.tableLayout.Controls.Add(this._alternateButton1, 2, 1);
			this.tableLayout.Controls.Add(this._message, 1, 0);
			this.tableLayout.Location = new System.Drawing.Point(20, 22);
			this.tableLayout.Name = "tableLayout";
			this.tableLayout.RowCount = 2;
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayout.Size = new System.Drawing.Size(403, 100);
			this.tableLayout.TabIndex = 5;
			//
			// _reoccurenceMessage
			//
			this._reoccurenceMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._reoccurenceMessage.AutoSize = true;
			this.tableLayout.SetColumnSpan(this._reoccurenceMessage, 2);
			this._reoccurenceMessage.ForeColor = System.Drawing.Color.Gray;
			this._reoccurenceMessage.Location = new System.Drawing.Point(0, 71);
			this._reoccurenceMessage.Margin = new System.Windows.Forms.Padding(10, 0, 0, 3);
			this._reoccurenceMessage.Name = "_reoccurenceMessage";
			this._reoccurenceMessage.Size = new System.Drawing.Size(50, 13);
			this._reoccurenceMessage.TabIndex = 6;
			this._reoccurenceMessage.Text = "Re-occurence message";
			//
			// _message
			//
			this._message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._message.AutoSize = false;
			this._message.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayout.SetColumnSpan(this._message, 3);
			this._message.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._message.Location = new System.Drawing.Point(60, 0);
			this._message.Margin = new System.Windows.Forms.Padding(0, 0, 0, 23);
			this._message.Multiline = true;
			this._message.Name = "_message";
			this._message.ReadOnly = true;
			this._message.Size = new System.Drawing.Size(60, 36);
			this._message.TabIndex = 0;
			this._message.Text = "Blah blah";
			this._message.TextChanged += new System.EventHandler(this.HandleMessageTextChanged);
			//
			// ProblemNotificationDialog
			//
			this.AcceptButton = this._acceptButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this._acceptButton;
			this.ClientSize = new System.Drawing.Size(444, 189);
			this.ControlBox = true;
			this.Controls.Add(this.tableLayout);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(250, 80);
			this.MaximumSize = new System.Drawing.Size(600, 450);
			this.Name = "ProblemNotificationDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 22, 15, 0);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Problem Notification";
			((System.ComponentModel.ISupportInitialize)(this._icon)).EndInit();
			this.tableLayout.ResumeLayout(false);
			this.tableLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox _icon;
		internal System.Windows.Forms.Button _acceptButton;
		internal System.Windows.Forms.Button _alternateButton1;
		private System.Windows.Forms.TableLayoutPanel tableLayout;
		internal System.Windows.Forms.Label _reoccurenceMessage;
		private System.Windows.Forms.TextBox _message;
	}
}