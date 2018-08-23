namespace SIL.Windows.Forms.Widgets
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
				if(_backgroundBrush!=null)
				{
					_backgroundBrush.Dispose();
					_backgroundBrush = null;
				}
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
			this.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.Multiline = true;
			this.ReadOnly = true;
			this.Size = new System.Drawing.Size(100, 20);
			this.TabStop = false;
			this.ResumeLayout(false);
		}

		#endregion
	}
}