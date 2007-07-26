namespace Elsehemy
{
	partial class SuperToolTipWindowData
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
			this.picBody = new System.Windows.Forms.PictureBox();
			this.lblFooter = new System.Windows.Forms.Label();
			this.flwPnlBody = new System.Windows.Forms.FlowLayoutPanel();
			this.lblBody = new System.Windows.Forms.Label();
			this.flwPnlFooter = new System.Windows.Forms.FlowLayoutPanel();
			this.picFooter = new System.Windows.Forms.PictureBox();
			this.lblHeader = new System.Windows.Forms.Label();
			this.tblPnl = new System.Windows.Forms.TableLayoutPanel();
			this.pnlHeaderSeparator = new System.Windows.Forms.Panel();
			this.pnlFooterSeparator = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.picBody)).BeginInit();
			this.flwPnlBody.SuspendLayout();
			this.flwPnlFooter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picFooter)).BeginInit();
			this.tblPnl.SuspendLayout();
			this.SuspendLayout();
			//
			// picBody
			//
			this.picBody.Location = new System.Drawing.Point(3, 3);
			this.picBody.Name = "picBody";
			this.picBody.Size = new System.Drawing.Size(100, 27);
			this.picBody.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.picBody.TabIndex = 0;
			this.picBody.TabStop = false;
			this.picBody.Visible = false;
			//
			// lblFooter
			//
			this.lblFooter.AutoSize = true;
			this.lblFooter.BackColor = System.Drawing.Color.Transparent;
			this.lblFooter.Location = new System.Drawing.Point(41, 0);
			this.lblFooter.Name = "lblFooter";
			this.lblFooter.Size = new System.Drawing.Size(0, 13);
			this.lblFooter.TabIndex = 2;
			//
			// flwPnlBody
			//
			this.flwPnlBody.AutoSize = true;
			this.flwPnlBody.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flwPnlBody.BackColor = System.Drawing.Color.Transparent;
			this.flwPnlBody.Controls.Add(this.picBody);
			this.flwPnlBody.Controls.Add(this.lblBody);
			this.flwPnlBody.Location = new System.Drawing.Point(3, 44);
			this.flwPnlBody.Name = "flwPnlBody";
			this.flwPnlBody.Size = new System.Drawing.Size(121, 33);
			this.flwPnlBody.TabIndex = 9;
			//
			// lblBody
			//
			this.lblBody.AutoSize = true;
			this.lblBody.BackColor = System.Drawing.Color.Transparent;
			this.lblBody.Location = new System.Drawing.Point(118, 0);
			this.lblBody.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
			this.lblBody.Name = "lblBody";
			this.lblBody.Size = new System.Drawing.Size(0, 13);
			this.lblBody.TabIndex = 3;
			//
			// flwPnlFooter
			//
			this.flwPnlFooter.AutoSize = true;
			this.flwPnlFooter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flwPnlFooter.BackColor = System.Drawing.Color.Transparent;
			this.flwPnlFooter.Controls.Add(this.picFooter);
			this.flwPnlFooter.Controls.Add(this.lblFooter);
			this.flwPnlFooter.Location = new System.Drawing.Point(3, 99);
			this.flwPnlFooter.Name = "flwPnlFooter";
			this.flwPnlFooter.Size = new System.Drawing.Size(44, 38);
			this.flwPnlFooter.TabIndex = 8;
			//
			// picFooter
			//
			this.picFooter.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.picFooter.Location = new System.Drawing.Point(3, 3);
			this.picFooter.Name = "picFooter";
			this.picFooter.Size = new System.Drawing.Size(32, 32);
			this.picFooter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.picFooter.TabIndex = 1;
			this.picFooter.TabStop = false;
			this.picFooter.Visible = false;
			//
			// lblHeader
			//
			this.lblHeader.AutoSize = true;
			this.lblHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblHeader.Location = new System.Drawing.Point(3, 12);
			this.lblHeader.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
			this.lblHeader.Name = "lblHeader";
			this.lblHeader.Size = new System.Drawing.Size(0, 13);
			this.lblHeader.TabIndex = 7;
			//
			// tblPnl
			//
			this.tblPnl.AutoSize = true;
			this.tblPnl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tblPnl.BackColor = System.Drawing.Color.Transparent;
			this.tblPnl.ColumnCount = 1;
			this.tblPnl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tblPnl.Controls.Add(this.lblHeader, 0, 0);
			this.tblPnl.Controls.Add(this.flwPnlFooter, 0, 4);
			this.tblPnl.Controls.Add(this.flwPnlBody, 0, 2);
			this.tblPnl.Controls.Add(this.pnlHeaderSeparator, 0, 1);
			this.tblPnl.Controls.Add(this.pnlFooterSeparator, 0, 3);
			this.tblPnl.Location = new System.Drawing.Point(0, 0);
			this.tblPnl.Margin = new System.Windows.Forms.Padding(3, 23, 3, 3);
			this.tblPnl.Name = "tblPnl";
			this.tblPnl.RowCount = 5;
			this.tblPnl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblPnl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblPnl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblPnl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblPnl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblPnl.Size = new System.Drawing.Size(127, 140);
			this.tblPnl.TabIndex = 10;
			//
			// pnlHeaderSeparator
			//
			this.pnlHeaderSeparator.Location = new System.Drawing.Point(3, 28);
			this.pnlHeaderSeparator.Name = "pnlHeaderSeparator";
			this.pnlHeaderSeparator.Size = new System.Drawing.Size(120, 10);
			this.pnlHeaderSeparator.TabIndex = 10;
			this.pnlHeaderSeparator.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlHeaderSeparator_Paint);
			//
			// pnlFooterSeparator
			//
			this.pnlFooterSeparator.Location = new System.Drawing.Point(3, 83);
			this.pnlFooterSeparator.Name = "pnlFooterSeparator";
			this.pnlFooterSeparator.Size = new System.Drawing.Size(120, 10);
			this.pnlFooterSeparator.TabIndex = 11;
			this.pnlFooterSeparator.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlFooterSeparator_Paint);
			//
			// SuperToolTipWindowData
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tblPnl);
			this.Name = "SuperToolTipWindowData";
			this.Size = new System.Drawing.Size(130, 143);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SuperToolTipWindowData_MouseMove);
			this.SizeChanged += new System.EventHandler(this.SuperToolTipWindowData_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.picBody)).EndInit();
			this.flwPnlBody.ResumeLayout(false);
			this.flwPnlBody.PerformLayout();
			this.flwPnlFooter.ResumeLayout(false);
			this.flwPnlFooter.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.picFooter)).EndInit();
			this.tblPnl.ResumeLayout(false);
			this.tblPnl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox picBody;
		private System.Windows.Forms.Label lblFooter;
		private System.Windows.Forms.FlowLayoutPanel flwPnlBody;
		private System.Windows.Forms.Label lblBody;
		private System.Windows.Forms.FlowLayoutPanel flwPnlFooter;
		private System.Windows.Forms.PictureBox picFooter;
		private System.Windows.Forms.Label lblHeader;
		private System.Windows.Forms.TableLayoutPanel tblPnl;
		private System.Windows.Forms.Panel pnlHeaderSeparator;
		private System.Windows.Forms.Panel pnlFooterSeparator;

	}
}
