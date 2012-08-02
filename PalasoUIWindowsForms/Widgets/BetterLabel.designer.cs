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
				if (_textBrush != null)
				{
					_textBrush.Dispose();
					_textBrush = null;
				}
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
			this.BackColorChanged += new System.EventHandler(this.BetterLabel_BackColorChanged);
			this.ForeColorChanged += new System.EventHandler(this.BetterLabel_ForeColorChanged);
			this.SizeChanged += new System.EventHandler(this.BetterLabel_SizeChanged);
			this.TextChanged += new System.EventHandler(this.BetterLabel_TextChanged);
			this.ParentChanged += new System.EventHandler(this.BetterLabel_ParentChanged);
			this.ResumeLayout(false);

		}

		#endregion
	}
}