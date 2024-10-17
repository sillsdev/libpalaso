// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Windows.Forms;

namespace SIL.Windows.Forms.Miscellaneous
{
	/// <summary>
	/// This class helps dialogs (or program main windows) with textboxes to use the PortableClipboard
	/// instead of the regular clipboard if needed.  Standard clipboard operations (copy/cut/paste) in
	/// text boxes may not work properly on Linux in some situations, freezing or crashing the program.
	/// The PortableClipboard is always used on Linux with this subclass.  It may optionally be used
	/// on Windows as well if the program design requires identical behavior (no context menu) on both
	/// platforms.
	/// </summary>
	/// <remarks>
	/// See https://issues.bloomlibrary.org/youtrack/issue/BL-5681 for one example of where this is needed.
	/// </remarks>
	public class FormForUsingPortableClipboard : Form
	{
		private bool _usePortableClipboard;

		public FormForUsingPortableClipboard()
		{
			// The TextBox objects aren't yet created, so we have to delay removing/fixing
			// their context menus.  OnLoad seems like a reasonable time for that.
			_usePortableClipboard = PlatformUtilities.Platform.IsLinux;
		}

		protected override void OnLoad(System.EventArgs e)
		{
			// Note that TextBox.ShortcutsEnabled does nothing in the Mono runtime.  Needing to
			// remove (or fix) the context menu means that this must be done after the dialog
			// has been initialized.
			if (_usePortableClipboard)
				PortableClipboard.RemoveTextboxMenus(this);
			base.OnLoad (e);
		}

		/// <summary>
		/// Get or set a value indicating whether this
		/// <see cref="Windows.Forms.Miscellaneous.FormForUsingPortableClipboard"/> use PortableClipboard.
		/// This allows Windows builds to join in the fun if desired.
		/// </summary>
		/// <value>
		/// <c>true</c> if we want to use PortableClipboard in all TextBox and RichTextBox controls; otherwise, <c>false</c>.
		/// </value>
		public bool UsePortableClipboard
		{
			get => _usePortableClipboard;
			set
			{
				_usePortableClipboard = value;
				if (value)
					PortableClipboard.RemoveTextboxMenus(this);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (UsePortableClipboard &&
				PortableClipboard.ProcessClipboardCmdKeysForDialog(this, msg, keyData))
			{
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
