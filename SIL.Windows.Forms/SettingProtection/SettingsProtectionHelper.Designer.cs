﻿namespace SIL.Windows.Forms.SettingProtection
{
	partial class SettingsProtectionHelper
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
			this._checkForCtrlKeyTimer = new System.Windows.Forms.Timer(this.components);
			//
			// _checkForCtrlKeyTimer
			//
			this._checkForCtrlKeyTimer.Tick += new System.EventHandler(this._checkForCtrlKeyTimer_Tick);

		}

		#endregion

		private System.Windows.Forms.Timer _checkForCtrlKeyTimer;
	}
}
