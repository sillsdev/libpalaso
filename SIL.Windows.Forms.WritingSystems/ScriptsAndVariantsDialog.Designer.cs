namespace SIL.Windows.Forms.WritingSystems
{
	partial class ScriptsAndVariantsDialog
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
			this.components = new System.ComponentModel.Container();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._wsIdentifierView = new SIL.Windows.Forms.WritingSystems.WSIdentifiers.WSIdentifierView();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._L10NSharpExtender.SetLocalizableToolTip(this._okButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._okButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._okButton, "ScriptsAndVariants.ScriptsAndVariantsDialog._okButton");
			this._okButton.Location = new System.Drawing.Point(205, 280);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._L10NSharpExtender.SetLocalizableToolTip(this._cancelButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._cancelButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._cancelButton, "ScriptsAndVariants.ScriptsAndVariantsDialog._cancelButton");
			this._cancelButton.Location = new System.Drawing.Point(286, 280);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 2;
			this._cancelButton.Text = "Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			// 
			// _L10NSharpExtender
			// 
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "ScriptsAndVariantsDialog";
			// 
			// _wsIdentifierView
			// 
			this._L10NSharpExtender.SetLocalizableToolTip(this._wsIdentifierView, null);
			this._L10NSharpExtender.SetLocalizationComment(this._wsIdentifierView, null);
			this._L10NSharpExtender.SetLocalizingId(this._wsIdentifierView, "ScriptsAndVariants.ScriptsAndVariantsDialog.WSIdentifierView");
			this._wsIdentifierView.Location = new System.Drawing.Point(-2, 12);
			this._wsIdentifierView.Name = "_wsIdentifierView";
			this._wsIdentifierView.Size = new System.Drawing.Size(381, 270);
			this._wsIdentifierView.TabIndex = 0;
			// 
			// ScriptsAndVariantsDialog
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(376, 317);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._wsIdentifierView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizingId(this, "ScriptsAndVariantsDialog.ScriptsAndVariants");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ScriptsAndVariantsDialog";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Script and Variant";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		private WSIdentifiers.WSIdentifierView _wsIdentifierView;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
	}
}