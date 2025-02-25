using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.ClearShare.WinFormsUI;

namespace SIL.Windows.Forms.TestApp
{
	partial class ContributorsForm
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this._tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this._contributorsControl = new SIL.Windows.Forms.ClearShare.WinFormsUI.ContributorsListControl();
			this._contributorNames = new SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.btnUpdateContributorNames = new System.Windows.Forms.Label();
			this.timerToTestNonUiThreadAccess = new System.Windows.Forms.Timer(this.components);
			this._tableLayout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._contributorNames)).BeginInit();
			this.SuspendLayout();
			// 
			// _tableLayout
			// 
			this._tableLayout.AutoSize = true;
			this._tableLayout.ColumnCount = 2;
			this._tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this._tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._tableLayout.Controls.Add(this._contributorsControl, 0, 0);
			this._tableLayout.Controls.Add(this._contributorNames, 0, 1);
			this._tableLayout.Controls.Add(this.btnUpdateContributorNames, 1, 1);
			this._tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tableLayout.Location = new System.Drawing.Point(0, 0);
			this._tableLayout.Name = "_tableLayout";
			this._tableLayout.RowCount = 2;
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this._tableLayout.Size = new System.Drawing.Size(700, 350);
			this._tableLayout.TabIndex = 0;
			// 
			// _contributorsControl
			// 
			this._contributorsControl.BackColor = System.Drawing.Color.Transparent;
			this._tableLayout.SetColumnSpan(this._contributorsControl, 2);
			this._contributorsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._contributorsControl.Location = new System.Drawing.Point(3, 3);
			this._contributorsControl.Name = "_contributorsControl";
			this._contributorsControl.Size = new System.Drawing.Size(694, 169);
			this._contributorsControl.TabIndex = 0;
			this._contributorsControl.ValidatingContributor += new SIL.Windows.Forms.ClearShare.WinFormsUI.ContributorsListControl.ValidatingContributorHandler(this.HandleValidatingContributor);
			// 
			// _contributorNames
			// 
			this._contributorNames.AllowUserToAddRows = false;
			this._contributorNames.AllowUserToDeleteRows = false;
			this._contributorNames.AllowUserToOrderColumns = true;
			this._contributorNames.AllowUserToResizeRows = false;
			this._contributorNames.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this._contributorNames.BackgroundColor = System.Drawing.SystemColors.Window;
			this._contributorNames.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this._contributorNames.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this._contributorNames.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this._contributorNames.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this._contributorNames.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName});
			this._contributorNames.Dock = System.Windows.Forms.DockStyle.Fill;
			this._contributorNames.DrawTextBoxEditControlBorder = false;
			this._contributorNames.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._contributorNames.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this._contributorNames.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this._contributorNames.Location = new System.Drawing.Point(3, 178);
			this._contributorNames.MultiSelect = false;
			this._contributorNames.Name = "_contributorNames";
			this._contributorNames.PaintHeaderAcrossFullGridWidth = true;
			this._contributorNames.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this._contributorNames.RowHeadersWidth = 22;
			this._contributorNames.SelectedCellBackColor = System.Drawing.Color.Empty;
			this._contributorNames.SelectedCellForeColor = System.Drawing.Color.Empty;
			this._contributorNames.SelectedRowBackColor = System.Drawing.Color.Empty;
			this._contributorNames.SelectedRowForeColor = System.Drawing.Color.Empty;
			this._contributorNames.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this._contributorNames.ShowWaterMarkWhenDirty = false;
			this._contributorNames.Size = new System.Drawing.Size(480, 169);
			this._contributorNames.TabIndex = 1;
			this._contributorNames.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this._contributorNames.WaterMark = "!";
			// 
			// colName
			// 
			this.colName.HeaderText = "Name";
			this.colName.Name = "colName";
			// 
			// btnUpdateContributorNames
			// 
			this.btnUpdateContributorNames.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnUpdateContributorNames.AutoSize = true;
			this.btnUpdateContributorNames.Location = new System.Drawing.Point(489, 256);
			this.btnUpdateContributorNames.Name = "btnUpdateContributorNames";
			this.btnUpdateContributorNames.Size = new System.Drawing.Size(208, 13);
			this.btnUpdateContributorNames.TabIndex = 2;
			this.btnUpdateContributorNames.Text = "Hover over this text to Update Contributors";
			this.btnUpdateContributorNames.MouseHover += new System.EventHandler(this.UpdateNames);
			// 
			// timerToTestNonUiThreadAccess
			// 
			this.timerToTestNonUiThreadAccess.Tick += new System.EventHandler(this.timerToTestNonUiThreadAccess_Tick);
			// 
			// ContributorsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(700, 350);
			this.Controls.Add(this._tableLayout);
			this.Name = "ContributorsForm";
			this.ShowIcon = false;
			this.Text = "Contributors";
			this._tableLayout.ResumeLayout(false);
			this._tableLayout.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._contributorNames)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.TableLayoutPanel _tableLayout;
		private SIL.Windows.Forms.ClearShare.WinFormsUI.ContributorsListControl _contributorsControl;
		private SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid _contributorNames;
		private System.Windows.Forms.Label btnUpdateContributorNames;
		private System.Windows.Forms.DataGridViewTextBoxColumn colName;
		private Timer timerToTestNonUiThreadAccess;
	}
}
