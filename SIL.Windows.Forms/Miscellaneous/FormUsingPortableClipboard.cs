// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Windows.Forms;

namespace SIL.Windows.Forms.Miscellaneous
{
	/// <summary>
	/// This class helps dialogs (or program main windows) with textboxes to use the PortableClipboard
	/// instead of the regular clipboard if needed.  Standard clipboard operations (copy/cut/paste) in
	/// text boxes may not work properly on Linux in some situations, freezing or crashing the program.
	/// </summary>
	/// <remarks>
	/// See https://issues.bloomlibrary.org/youtrack/issue/BL-5681 for one example of where this is needed.
	/// </remarks>
	public class FormUsingPortableClipboard : Form
	{
		private bool _usePortableClipboard;
		public bool UsePortableClipboard
		{
			get { return _usePortableClipboard; }
			set
			{
				_usePortableClipboard = value;
				// Note that TextBox.ShortcutsEnabled does nothing in the Mono runtime.  Needing to
				// remove (or fix) the context menu means that this must be done after the dialog
				// has been initialized.
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
