﻿using SIL.WindowsForms.Widgets;

namespace SIL.WritingSystems.WindowsForms.WSIdentifiers
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
			this.betterLabel1 = new BetterLabel();
			this.SuspendLayout();
			//
			// betterLabel1
			//
			this.betterLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Location = new System.Drawing.Point(0, 11);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(221, 65);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "In applications which support this option, fields with this input system will b" +
				"e able to play and record voice.";
			//
			// VoiceIdentifierView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.betterLabel1);
			this.Name = "VoiceIdentifierView";
			this.Size = new System.Drawing.Size(221, 76);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BetterLabel betterLabel1;
	}
}
