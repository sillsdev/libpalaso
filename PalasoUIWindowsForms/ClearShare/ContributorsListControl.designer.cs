using System.Windows.Forms;
using Palaso.UI.WindowsForms.Widgets.Grid;

namespace Palaso.UI.WindowsForms.ClearShare
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
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this._tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this._panelGrid = new System.Windows.Forms.Panel();
			this._grid = new Palaso.UI.WindowsForms.Widgets.Grid.BetterGrid();
			this._buttonDelete = new System.Windows.Forms.Button();
			this._toolTip = new System.Windows.Forms.ToolTip(this.components);
			this._tableLayout.SuspendLayout();
			this._panelGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
			this.SuspendLayout();
			//
			// _tableLayout
			//
			this._tableLayout.BackColor = System.Drawing.Color.Transparent;
			this._tableLayout.ColumnCount = 1;
			this._tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayout.Controls.Add(this._panelGrid, 0, 0);
			this._tableLayout.Controls.Add(this._buttonDelete, 0, 1);
			this._tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tableLayout.Location = new System.Drawing.Point(0, 0);
			this._tableLayout.Margin = new System.Windows.Forms.Padding(0);
			this._tableLayout.Name = "_tableLayout";
			this._tableLayout.RowCount = 2;
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayout.Size = new System.Drawing.Size(453, 256);
			this._tableLayout.TabIndex = 0;
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
			this._panelGrid.Size = new System.Drawing.Size(453, 225);
			this._panelGrid.TabIndex = 0;
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
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this._grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
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
			this._grid.Size = new System.Drawing.Size(451, 223);
			this._grid.TabIndex = 0;
			this._grid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this._grid.VirtualMode = true;
			//
			// _buttonDelete
			//
			this._buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._buttonDelete.AutoSize = true;
			this._buttonDelete.Location = new System.Drawing.Point(378, 230);
			this._buttonDelete.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this._buttonDelete.Name = "_buttonDelete";
			this._buttonDelete.Size = new System.Drawing.Size(75, 26);
			this._buttonDelete.TabIndex = 1;
			this._buttonDelete.Text = "Delete";
			this._buttonDelete.UseVisualStyleBackColor = true;
			this._buttonDelete.Click += new System.EventHandler(this.HandleDeleteButtonClicked);
			//
			// ContributorsListControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this._tableLayout);
			this.DoubleBuffered = true;
			this.Name = "ContributorsListControl";
			this.Size = new System.Drawing.Size(453, 256);
			this._tableLayout.ResumeLayout(false);
			this._tableLayout.PerformLayout();
			this._panelGrid.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Panel _panelGrid;
		private System.Windows.Forms.TableLayoutPanel _tableLayout;
		private BetterGrid _grid;
		private System.Windows.Forms.Button _buttonDelete;
		private System.Windows.Forms.ToolTip _toolTip;
	}
}
