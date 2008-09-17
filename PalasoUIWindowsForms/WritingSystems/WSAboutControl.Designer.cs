namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSAboutControl
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
			this._pgAbout = new System.Windows.Forms.PropertyGrid();
			this.SuspendLayout();
			//
			// _pgAbout
			//
			this._pgAbout.Dock = System.Windows.Forms.DockStyle.Fill;
			this._pgAbout.Location = new System.Drawing.Point(0, 0);
			this._pgAbout.Name = "_pgAbout";
			this._pgAbout.PropertySort = System.Windows.Forms.PropertySort.NoSort;
			this._pgAbout.Size = new System.Drawing.Size(430, 227);
			this._pgAbout.TabIndex = 0;
			this._pgAbout.ToolbarVisible = false;
			this._pgAbout.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this._pgAbout_PropertyValueChanged);
			//
			// WSAboutControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._pgAbout);
			this.Name = "WSAboutControl";
			this.Size = new System.Drawing.Size(430, 227);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PropertyGrid _pgAbout;
	}
}
