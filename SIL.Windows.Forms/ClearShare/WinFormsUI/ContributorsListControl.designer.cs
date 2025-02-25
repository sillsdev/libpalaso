using System.Windows.Forms;
using SIL.Windows.Forms.Widgets.BetterGrid;

namespace SIL.Windows.Forms.ClearShare.WinFormsUI
{
	partial class ContributorsListControl
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
			if (disposing)
			{
				if (_model != null)
				{
					_model.NewContributionListAvailable -= HandleNewContributionListAvailable;
					_model = null;
				}

				if (components != null)
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this._grid = new BetterGrid();
			this._panelGrid = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
			this._panelGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// _grid
			//
			this._grid.AllowUserToAddRows = false;
			this._grid.AllowUserToDeleteRows = false;
			this._grid.AllowUserToOrderColumns = true;
			this._grid.AllowUserToResizeRows = false;
			this._grid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			this._grid.BackgroundColor = System.Drawing.SystemColors.Window;
			this._grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this._grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this._grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this._grid.DrawTextBoxEditControlBorder = false;
			this._grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this._grid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._grid.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this._grid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this._grid.Location = new System.Drawing.Point(1, 1);
			this._grid.MultiSelect = false;
			this._grid.Name = "_grid";
			this._grid.PaintHeaderAcrossFullGridWidth = true;
			this._grid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this._grid.RowHeadersWidth = 24;
			this._grid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this._grid.SelectedCellBackColor = System.Drawing.Color.Empty;
			this._grid.SelectedCellForeColor = System.Drawing.Color.Empty;
			this._grid.SelectedRowBackColor = System.Drawing.Color.Empty;
			this._grid.SelectedRowForeColor = System.Drawing.Color.Empty;
			this._grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this._grid.Size = new System.Drawing.Size(451, 254);
			this._grid.TabIndex = 0;
			this._grid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			//
			// _panelGrid
			//
			this._panelGrid.BackColor = System.Drawing.SystemColors.ControlDark;
			this._panelGrid.Controls.Add(this._grid);
			this._panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this._panelGrid.Location = new System.Drawing.Point(0, 0);
			this._panelGrid.Margin = new System.Windows.Forms.Padding(0);
			this._panelGrid.Name = "_panelGrid";
			this._panelGrid.Padding = new System.Windows.Forms.Padding(1);
			this._panelGrid.Size = new System.Drawing.Size(453, 256);
			this._panelGrid.TabIndex = 0;
			//
			// ContributorsListControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this._panelGrid);
			this.DoubleBuffered = true;
			this.Name = "ContributorsListControl";
			this.Size = new System.Drawing.Size(453, 256);
			((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
			this._panelGrid.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private BetterGrid _grid;
		private Panel _panelGrid;
	}
}
