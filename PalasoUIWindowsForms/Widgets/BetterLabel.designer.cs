namespace Palaso.UI.WindowsForms.Widgets
{
	partial class BetterLabel
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
			this.SuspendLayout();
			//
			// BetterLabel
			//
			this.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																| System.Windows.Forms.AnchorStyles.Right)));
			this.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.Multiline = true;
			this.ReadOnly = true;
			this.Size = new System.Drawing.Size(100, 20);
			this.TabStop = false;
			this.ParentChanged += new System.EventHandler(this.BetterLabel_ParentChanged);
			this.ResumeLayout(false);

		}

		#endregion
	}
}