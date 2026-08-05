using System;

namespace SIL.Windows.Forms.SettingProtection
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
			System.Diagnostics.Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			if (_isDisposed)
				return;
			if (disposing)
			{
				if (components != null)
					components.Dispose();
				_componentsUnderSettingsProtection.Clear();
				_alwaysHiddenComponents.Clear();
			}
			// Must run before _isDisposed is set: the base implementation removes this component
			// from its container, which reads the Site property, and our override of Site throws
			// once disposed.
			base.Dispose(disposing);
			_isDisposed = true;
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
