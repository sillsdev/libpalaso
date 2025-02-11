namespace SIL.Windows.Forms.Reporting
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
			this._message = new System.Windows.Forms.TextBox();
			this._reoccurrenceMessage = new System.Windows.Forms.Label();
			this.tableLayoutOuter = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this._icon)).BeginInit();
			this.tableLayoutOuter.SuspendLayout();
			this.SuspendLayout();
			//
			// _icon
			//
			this._icon.Location = new System.Drawing.Point(3, 0);
			this._icon.Margin = new System.Windows.Forms.Padding(3, 0, 12, 0);
			this._icon.Name = "_icon";
			this._icon.Size = new System.Drawing.Size(45, 36);
			this._icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this._icon.TabIndex = 1;
			this._icon.TabStop = false;
			//
			// _acceptButton
			//
			this._acceptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._acceptButton.AutoSize = true;
			this._acceptButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._acceptButton.Location = new System.Drawing.Point(370, 153);
			this._acceptButton.Name = "_acceptButton";
			this._acceptButton.Size = new System.Drawing.Size(83, 23);
			this._acceptButton.TabIndex = 1;
			this._acceptButton.Text = "&OK";
			this._acceptButton.UseVisualStyleBackColor = true;
			this._acceptButton.Click += new System.EventHandler(this._acceptButton_Click);
			//
			// _alternateButton1
			//
			this._alternateButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._alternateButton1.AutoSize = true;
			this._alternateButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._alternateButton1.Location = new System.Drawing.Point(281, 153);
			this._alternateButton1.Name = "_alternateButton1";
			this._alternateButton1.Size = new System.Drawing.Size(83, 23);
			this._alternateButton1.TabIndex = 0;
			this._alternateButton1.Text = "&Caller Defined";
			this._alternateButton1.UseVisualStyleBackColor = true;
			this._alternateButton1.Visible = false;
			this._alternateButton1.Click += new System.EventHandler(this.OnAlternateButton1_Click);
			//
			// _message
			//
			this._message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._message.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayoutOuter.SetColumnSpan(this._message, 3);
			this._message.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._message.Location = new System.Drawing.Point(63, 3);
			this._message.Multiline = true;
			this._message.Name = "_message";
			this._message.ReadOnly = true;
			this._message.Size = new System.Drawing.Size(390, 144);
			this._message.TabIndex = 2;
			this._message.Text = "Blah blah";
			this._message.TextChanged += new System.EventHandler(this.HandleMessageTextChanged);
			//
			// _reoccurrenceMessage
			//
			this._reoccurrenceMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._reoccurrenceMessage.AutoSize = true;
			this.tableLayoutOuter.SetColumnSpan(this._reoccurrenceMessage, 2);
			this._reoccurrenceMessage.ForeColor = System.Drawing.Color.Gray;
			this._reoccurrenceMessage.Location = new System.Drawing.Point(3, 158);
			this._reoccurrenceMessage.Name = "_reoccurrenceMessage";
			this._reoccurrenceMessage.Size = new System.Drawing.Size(272, 13);
			this._reoccurrenceMessage.TabIndex = 3;
			this._reoccurrenceMessage.Text = "Reoccurrence message";
			//
			// tableLayoutOuter
			//
			this.tableLayoutOuter.ColumnCount = 4;
			this.tableLayoutOuter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutOuter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutOuter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutOuter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutOuter.Controls.Add(this._message, 1, 0);
			this.tableLayoutOuter.Controls.Add(this._icon, 0, 0);
			this.tableLayoutOuter.Controls.Add(this._acceptButton, 3, 1);
			this.tableLayoutOuter.Controls.Add(this._alternateButton1, 2, 1);
			this.tableLayoutOuter.Controls.Add(this._reoccurrenceMessage, 0, 1);
			this.tableLayoutOuter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutOuter.Location = new System.Drawing.Point(5, 5);
			this.tableLayoutOuter.Name = "tableLayoutOuter";
			this.tableLayoutOuter.RowCount = 2;
			this.tableLayoutOuter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutOuter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutOuter.Size = new System.Drawing.Size(456, 179);
			this.tableLayoutOuter.TabIndex = 0;
			//
			// ProblemNotificationDialog
			//
			this.AcceptButton = this._acceptButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this._acceptButton;
			this.ClientSize = new System.Drawing.Size(466, 189);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutOuter);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(450, 38);
			this.Name = "ProblemNotificationDialog";
			this.Padding = new System.Windows.Forms.Padding(5);
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Problem Notification";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.ProblemNotificationDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this._icon)).EndInit();
			this.tableLayoutOuter.ResumeLayout(false);
			this.tableLayoutOuter.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox _icon;
		internal System.Windows.Forms.Button _acceptButton;
		internal System.Windows.Forms.Button _alternateButton1;
		internal System.Windows.Forms.Label _reoccurrenceMessage;
		private System.Windows.Forms.TextBox _message;
		private System.Windows.Forms.TableLayoutPanel tableLayoutOuter;
	}
}
