namespace SIL.Windows.Forms.ClearShare.WinFormsUI
{
	partial class MetadataDisplayControl
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
			this._table = new System.Windows.Forms.TableLayoutPanel();
			this.SuspendLayout();
			//
			// _table
			//
			this._table.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._table.BackColor = System.Drawing.SystemColors.Control;
			this._table.ColumnCount = 2;
			this._table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._table.Location = new System.Drawing.Point(0, 23);
			this._table.Name = "_table";
			this._table.RowCount = 1;
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._table.Size = new System.Drawing.Size(269, 205);
			this._table.TabIndex = 6;
			//
			// MetadataDisplayControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._table);
			this.Name = "MetadataDisplayControl";
			this.Size = new System.Drawing.Size(269, 228);
			this.Load += new System.EventHandler(this.MetadataDisplayControl_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel _table;


	}
}
