namespace Palaso.UI.WindowsForms.Widgets
{

	partial class StdTextInputBox
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
			// Debug.WriteLine(" TextBox " + Name + "-textBox   Disposing=" + disposing);
			if (disposing && (components != null))
			{
				components.Dispose();
				OnSpellCheckingDisabled();
			}
			_oldFont = null;
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
			// WeSayTextBox
			//
			BackColor = System.Drawing.Color.White;
			//BackColor = System.Drawing.Color.AliceBlue;
			Multiline = true;
			WordWrap = true;
			BorderStyle = System.Windows.Forms.BorderStyle.None;

			this.ResumeLayout(false);

		}

		#endregion
	}
}
