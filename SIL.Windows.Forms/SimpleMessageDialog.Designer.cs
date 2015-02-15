namespace SIL.Windows.Forms
{
	partial class SimpleMessageDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._dialogMessage = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _dialogMessage
			// 
			this._dialogMessage.AutoSize = true;
			this._dialogMessage.Location = new System.Drawing.Point(22, 22);
			this._dialogMessage.MaximumSize = new System.Drawing.Size(240, 0);
			this._dialogMessage.Name = "_dialogMessage";
			this._dialogMessage.Size = new System.Drawing.Size(49, 13);
			this._dialogMessage.TabIndex = 0;
			this._dialogMessage.Text = "message";
			// 
			// SimpleMessageDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(292, 85);
			this.ControlBox = false;
			this.Controls.Add(this._dialogMessage);
			this.Name = "SimpleMessageDialog";
			this.Text = "caption";
			this.Activated += new System.EventHandler(this.SimpleMessageDialog_Activated);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label _dialogMessage;
	}
}