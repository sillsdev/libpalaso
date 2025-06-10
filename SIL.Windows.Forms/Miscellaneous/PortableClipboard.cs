using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reporting;
using SIL.Windows.Forms.Clipboarding;
using SIL.Windows.Forms.ImageToolbox;

namespace SIL.Windows.Forms.Miscellaneous
{
	/// <summary>Uses the GTK classes when accessing the clipboard on Linux</summary>
	public static class PortableClipboard
	{
		private static readonly IClipboard _Clipboard =
			Platform.IsWindows ? new WindowsClipboard() : (IClipboard)new LinuxClipboard();

		public static bool ContainsText() => _Clipboard.ContainsText();
		public static string GetText() => _Clipboard.GetText();
		public static string GetText(TextDataFormat format) => _Clipboard.GetText(format);
		public static void SetText(string text) => _Clipboard.SetText(text);
		public static void SetText(string text, TextDataFormat format) => _Clipboard.SetText(text, format);
		public static bool ContainsImage() => _Clipboard.ContainsImage();

		public static bool CanGetImage() => _Clipboard.CanGetImage();
		public static Image GetImage() => _Clipboard.GetImage();
		public static void CopyImageToClipboard(PalasoImage image) => _Clipboard.CopyImageToClipboard(image);

		// Get an image from the clipboard, ignoring any errors that occur while we attempt to
		// convert whatever is on the clipboard into an image.
		public static PalasoImage GetImageFromClipboard()
		{
			try
			{
				return _Clipboard.GetImageFromClipboard();
			}
			catch (Exception)
			{}

			return null;
		}

		// Try to get an image from the clipboard. If there simply isn't anything on the clipboard that
		// can reasonably be interpreted as an image, return null. If there is something that makes sense
		// as an image, but trying to load it causes an exception, let the exception propagate.
		public static PalasoImage GetImageFromClipboardWithExceptions() => _Clipboard.GetImageFromClipboard();

		// -------------------------------------------------------------------------------------------------------------
		// Clipboard operations (copy/cut/paste) in text boxes do not work properly on Linux in some situations,
		// freezing or crashing the program. See https://issues.bloomlibrary.org/youtrack/issue/BL-5681 for one example.
		// The following methods were added to allow dialogs to use the PortableClipboard in TextBox and RichTextBox
		// controls.  It's possible that these methods might only be used on Linux, but they compile (and would work)
		// fine for Windows.
		// These methods are used by FormUsingPortableClipboard.

		/// <summary>
		/// Recursively remove all TextBox menus found owned by the control.
		/// </summary>
		/// <remarks>>
		/// It might be better to hook up the copy/cut/paste commands to use the PortableClipboard instead, but that
		/// would be a lot trickier to pull off reliably.
		/// </remarks>
		public static void RemoveTextboxMenus(Control control)
		{
			if (control is TextBoxBase textBoxBase)
			{
				textBoxBase.ContextMenuStrip = null;
				return;
			}

			if (control?.Controls == null)
				return;

			foreach (var ctl in control.Controls)
				RemoveTextboxMenus(ctl as Control);
		}

		/// <summary>
		/// Process the clipboard cmd keys for a dialog.  This is called from a ProcessCmdKeys override method.
		/// </summary>
		/// <returns><c>true</c>, if a clipboard command key for this dialog was processed, <c>false</c> otherwise.</returns>
		public static bool ProcessClipboardCmdKeysForDialog(Form form, Message msg, Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Control|Keys.V:
				return PortablePasteIntoTextBox(form, msg.HWnd);
			case Keys.Control|Keys.C:
				return PortableCopyOrCutFromTextBox(form, msg.HWnd, false);
			case Keys.Control|Keys.X:
				return PortableCopyOrCutFromTextBox(form, msg.HWnd, true);
			}
			return false;
		}

		/// <summary>
		/// Recursively search for a TextBox or RichTextBox control with the given handle.
		/// </summary>
		/// <returns>the matching TextBoxBase control if found, null otherwise.</returns>
		private static TextBoxBase GetTextBoxFromHWnd(Control control, IntPtr hwnd)
		{
			if (control is TextBoxBase textBoxBase && textBoxBase.Handle == hwnd)
				return textBoxBase;
			if (control?.Controls == null)
				return null;

			foreach (var ctl in control.Controls)
			{
				var box = GetTextBoxFromHWnd(ctl as Control, hwnd);
				if (box != null)
					return box;
			}
			return null;
		}

		/// <summary>
		/// Paste from the PortableClipboard into a textbox in the given form that matches the given hwnd.
		/// </summary>
		/// <returns><c>true</c>, if pasting into a textbox was successful, <c>false</c> otherwise.</returns>
		private static bool PortablePasteIntoTextBox(Form form, IntPtr hwnd)
		{
			var box = GetTextBoxFromHWnd(form, hwnd);
			if (box == null)
				return false;
			if (!ContainsText())
				return true;

			var start = box.SelectionStart;
			var length = box.SelectionLength;
			var text = box.Text;
			if (length > 0)
			{
				if (start + length > text.Length) // shouldn't happen, but sometimes paranoia pays
					text = text.Remove(start);
				else
					text = text.Remove(start, length);
			}
			var clipText = GetText();
			box.Text = text.Insert(start, clipText);
			box.SelectionStart = start + clipText.Length;
			return true;
		}

		/// <summary>
		/// Copy (or cut) into the PortableClipboard from a textbox in the given form that matches the given hwnd.
		/// </summary>
		/// <returns><c>true</c>, if copying or cutting from a textbox was successful, <c>false</c> otherwise.</returns>
		private static bool PortableCopyOrCutFromTextBox(Form form, IntPtr hwnd, bool cut)
		{
			var box = GetTextBoxFromHWnd(form, hwnd);
			if (box == null)
				return false;
			var length = box.SelectionLength;
			if (length <= 0)
				return true;

			var start = box.SelectionStart;
			if (start + length > box.Text.Length)
				length = box.Text.Length - start;	// shouldn't happen, but paranoia sometimes pays.
			if (length <= 0)
				return true;
			var text = box.Text.Substring(start, length);
			SetText(text);
			if (!cut)
				return true;

			box.Text = box.Text.Remove(start, length);
			box.SelectionStart = start;
			return true;
		}
	}
}
