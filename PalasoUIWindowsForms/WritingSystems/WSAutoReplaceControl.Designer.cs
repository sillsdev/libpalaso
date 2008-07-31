namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSAutoReplaceControl
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
			this.label1 = new System.Windows.Forms.Label();
			this._autoReplaceRules = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(105, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Auto Replace Rules:";
			//
			// _autoReplaceRules
			//
			this._autoReplaceRules.AcceptsReturn = true;
			this._autoReplaceRules.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._autoReplaceRules.Location = new System.Drawing.Point(3, 16);
			this._autoReplaceRules.Multiline = true;
			this._autoReplaceRules.Name = "_autoReplaceRules";
			this._autoReplaceRules.Size = new System.Drawing.Size(475, 283);
			this._autoReplaceRules.TabIndex = 1;
			this._autoReplaceRules.TextChanged += new System.EventHandler(this._autoReplaceRules_TextChanged);
			//
			// WSAutoReplaceControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._autoReplaceRules);
			this.Controls.Add(this.label1);
			this.Name = "WSAutoReplaceControl";
			this.Size = new System.Drawing.Size(478, 299);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _autoReplaceRules;
	}
}
