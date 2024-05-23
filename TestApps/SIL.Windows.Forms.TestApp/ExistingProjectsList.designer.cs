namespace SIL.Windows.Forms.TestApp
{
	partial class ExistingProjectsList
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
				components?.Dispose();
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
			this.colFullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.SuspendLayout();
			this.m_list.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colFullName,
			this.colType});
			//
			// colFullName
			//
			this.colFullName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.colFullName.HeaderText = "Full Name";
			this.colFullName.MinimumWidth = 50;
			this.colFullName.Name = "colFullName";
			this.colFullName.ReadOnly = true;
			this.colFullName.Width = 200;
			//
			// colType
			//
			this.colType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.colType.HeaderText = "Type";
			this.colType.MinimumWidth = 50;
			this.colType.Name = "colType";
			this.colType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.colType.Width = 100;
			//
			// ExistingProjectsList
			//
			this.Name = "ExistingProjectsList";
			this.Size = new System.Drawing.Size(368, 147);
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.DataGridViewTextBoxColumn colFullName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colType;
	}
}
