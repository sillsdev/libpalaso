namespace WeSay.UI
{
	partial class ControlListBox
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
			this._panel = new System.Windows.Forms.Panel();
			this._table = new System.Windows.Forms.TableLayoutPanel();
			this._panel.SuspendLayout();
			this.SuspendLayout();
			//
			// _panel
			//
			this._panel.BackColor = System.Drawing.SystemColors.Window;
			this._panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._panel.Controls.Add(this._table);
			this._panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._panel.Location = new System.Drawing.Point(0, 0);
			this._panel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this._panel.Name = "_panel";
			this._panel.Size = new System.Drawing.Size(547, 258);
			this._panel.TabIndex = 1;
			//
			// _table
			//
			this._table.AutoScroll = true;
			this._table.ColumnCount = 1;
			this._table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._table.Dock = System.Windows.Forms.DockStyle.Fill;
			this._table.Location = new System.Drawing.Point(0, 0);
			this._table.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this._table.Name = "_table";
			this._table.RowCount = 1;
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 257F));
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 257F));
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 257F));
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 257F));
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 257F));
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 257F));
			this._table.Size = new System.Drawing.Size(545, 256);
			this._table.TabIndex = 1;
			//
			// ControlListBox
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._panel);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "ControlListBox";
			this.Size = new System.Drawing.Size(547, 258);
			this._panel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel _panel;

		public void Clear()
		{
			_table.Controls.Clear();
			_table.RowStyles.Clear();
			_firstOne = true;
		}

		private System.Windows.Forms.TableLayoutPanel _table;
	}
}