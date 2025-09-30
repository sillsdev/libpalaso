using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
{
	partial class VoiceIdentifierView
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
			this.components = new System.ComponentModel.Container();
			this.betterLabel1 = new SIL.Windows.Forms.Widgets.BetterLabel();
			this._l10nsharpExtender = new L10NSharp.Windows.Forms.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this._l10nsharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// betterLabel1
			// 
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.betterLabel1.Enabled = false;
			this.betterLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.betterLabel1.IsTextSelectable = false;
			this._l10nsharpExtender.SetLocalizableToolTip(this.betterLabel1, null);
			this._l10nsharpExtender.SetLocalizationComment(this.betterLabel1, "Displayed if a user selects the voice special option in writing system setup");
			this._l10nsharpExtender.SetLocalizingId(this.betterLabel1, "VoiceIdentifierView.betterLabel1");
			this.betterLabel1.Location = new System.Drawing.Point(0, 0);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(221, 39);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "In applications which support this option, fields with this input system will be " +
    "able to play and record voice.";
			// 
			// _l10nsharpExtender
			// 
			this._l10nsharpExtender.LocalizationManagerId = null;
			this._l10nsharpExtender.PrefixForNewItems = null;
			// 
			// VoiceIdentifierView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.betterLabel1);
			this._l10nsharpExtender.SetLocalizableToolTip(this, null);
			this._l10nsharpExtender.SetLocalizationComment(this, null);
			this._l10nsharpExtender.SetLocalizingId(this, "VoiceIdentifierView.VoiceIdentifierView");
			this.Name = "VoiceIdentifierView";
			this.Size = new System.Drawing.Size(221, 76);
			((System.ComponentModel.ISupportInitialize)(this._l10nsharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BetterLabel betterLabel1;
		private L10NSharp.Windows.Forms.L10NSharpExtender _l10nsharpExtender;
	}
}
