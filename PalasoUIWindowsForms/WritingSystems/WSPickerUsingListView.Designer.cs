namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSPickerUsingListView
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
			this._listView = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			//
			// _listView
			//
			this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._listView.FullRowSelect = true;
			this._listView.HideSelection = false;
			this._listView.Location = new System.Drawing.Point(0, 0);
			this._listView.MultiSelect = false;
			this._listView.Name = "_listView";
			this._listView.Size = new System.Drawing.Size(381, 294);
			this._listView.TabIndex = 0;
			this._listView.UseCompatibleStateImageBehavior = false;
			this._listView.View = System.Windows.Forms.View.List;
			//
			// WSPickerUsingListView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._listView);
			this.Name = "WSPickerUsingListView";
			this.Size = new System.Drawing.Size(381, 294);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView _listView;

	}
}
