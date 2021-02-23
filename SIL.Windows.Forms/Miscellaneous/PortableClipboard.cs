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
using SIL.Windows.Forms.ImageToolbox;

namespace SIL.Windows.Forms.Miscellaneous
{
	/// <summary>Uses the GTK classes when accessing the clipboard on Linux</summary>
	public static class PortableClipboard
	{
		public static bool ContainsText()
		{
			return Platform.IsWindows ? Clipboard.ContainsText() : GtkContainsText();
		}

		public static string GetText()
		{
			return Platform.IsWindows ? Clipboard.GetText() : GtkGetText();
		}

		public static string GetText(TextDataFormat format)
		{
			return Platform.IsWindows ? Clipboard.GetText(format) : GtkGetText();
		}

		public static void SetText(string text)
		{
			if (Platform.IsWindows)
				Clipboard.SetText(text);
			else
			GtkSetText(text);
		}

		public static void SetText(string text, TextDataFormat format)
		{
			if (Platform.IsWindows)
				Clipboard.SetText(text, format);
			else
			GtkSetText(text);
		}

		public static bool ContainsImage()
		{
			return Platform.IsWindows ? Clipboard.ContainsImage() : GtkContainsImage();
		}

		public static Image GetImage()
		{
			return Platform.IsWindows ? Clipboard.GetImage() : GtkGetImage();
		}

		public static void CopyImageToClipboard(PalasoImage image)
		{
			// N.B.: PalasoImage does not handle .svg files
			if(image == null)
				return;

			if (!Platform.IsWindows)
			{
			// Review: Someone who knows how needs to implement this!
				throw new NotImplementedException(
					"SIL.Windows.Forms.Miscellaneous.PortableClipboard.CopyImageToClipboard() is not yet implemented for Linux");
			}

			if (image.Image == null)
			{
				if (string.IsNullOrEmpty(image.OriginalFilePath))
					return;
				// no image, but a path
				Clipboard.SetFileDropList(new StringCollection() {image.OriginalFilePath});
			}
			else
			{
				if (string.IsNullOrEmpty(image.OriginalFilePath))
					Clipboard.SetImage(image.Image);
				else
				{
					IDataObject clips = new DataObject();
					clips.SetData(DataFormats.UnicodeText, image.OriginalFilePath);
					clips.SetData(DataFormats.Bitmap, image.Image);
					// true here means that the image should remain on the clipboard if the application exits
					Clipboard.SetDataObject(clips,true);
				}
			}
		}

		// Get an image from the clipboard, ignoring any errors that occur while we attempt to
		// convert whatever is on the clipboard into an image.
		public static PalasoImage GetImageFromClipboard()
		{
			try
			{
				return GetImageFromClipboardWithExceptions();
			}
			catch (Exception)
			{}

			return null;
		}

		// Try to get an image from the clipboard. If there simply isn't anything on the clipboard that
		// can reasonably be interpreted as an image, return null. If there is something that makes sense
		// as an image, but trying to load it causes an exception, let the exception propagate.
		public static PalasoImage GetImageFromClipboardWithExceptions() {
			// N.B.: PalasoImage does not handle .svg files
			if (!Platform.IsWindows)
			{
			if (GtkContainsImage())
				return PalasoImage.FromImage(GtkGetImage());

			if (GtkContainsText())
			{
				//REVIEW: I can find no documentation on GtkClipboard. If ContainsText means we have a file
				//	path, then it would be better to do PalasoImage.FromFileRobustly(); on the file path
				return PalasoImage.FromImage(GtkGetImageFromText());
			}

			return null;
			}

			var dataObject = Clipboard.GetDataObject();
			if (dataObject == null)
				return null;
			Exception ex = null;
			var textData = String.Empty;
			if (dataObject.GetDataPresent(DataFormats.UnicodeText))
				textData = dataObject.GetData(DataFormats.UnicodeText) as String;
			if (Clipboard.ContainsImage())
			{
				PalasoImage plainImage = null;
				try
				{
					plainImage = PalasoImage.FromImage(Clipboard.GetImage()); // this method won't copy any metadata
					var haveFileUrl = !String.IsNullOrEmpty(textData) && RobustFile.Exists(textData);

					// If we have an image on the clipboard, and we also have text that is a valid url to an image file,
					// use the url to create a PalasoImage (which will pull in any metadata associated with the image too)
					if (haveFileUrl)
					{
						var imageWithPathAndMaybeMetadata = PalasoImage.FromFileRobustly(textData);
						plainImage.Dispose();//important: don't do this until we've successfully created the imageWithPathAndMaybeMetadata
						return imageWithPathAndMaybeMetadata;
					}

						return plainImage;
					}
				catch (Exception e)
				{
					Logger.WriteEvent("PortableClipboard.GetImageFromClipboard() failed with message " + e.Message);
					if (plainImage != null)
					return plainImage; // at worst, we should return null; if FromFile() failed, we return an image
					throw;
				}
			}
			// the ContainsImage() returns false when copying an PNG from MS Word
			// so here we explicitly ask for a PNG and see if we can convert it.
			if (dataObject.GetDataPresent("PNG"))
			{
				var o = dataObject.GetData("PNG") as Stream;
				try
				{
					return PalasoImage.FromImage(Image.FromStream(o));
				}
				catch (Exception e)
				{
					// I'm not sure why, but previous versions of this code would continue trying other
					// options at this point.
					ex = e;
				}
			}

			//People can do a "copy" from the WIndows Photo Viewer but what it puts on the clipboard is a path, not an image
			if (dataObject.GetDataPresent(DataFormats.FileDrop))
			{
				//This line gets all the file paths that were selected in explorer
				string[] files = dataObject.GetData(DataFormats.FileDrop) as string[];
				if (files == null) return null;

				foreach (var file in files.Where(f => RobustFile.Exists(f)))
				{
						return PalasoImage.FromFileRobustly(file);
					}

				return null; //not an image
			}

			if (Clipboard.ContainsText() && RobustFile.Exists(Clipboard.GetText()))
				return PalasoImage.FromImage( Image.FromStream(new FileStream(Clipboard.GetText(), FileMode.Open)));

			if (ex != null)
			{
				throw ex;
			}

			return null;
		}

		// The following methods are derived from GtkClipboard.cs from https://github.com/phillip-hopper/GtkUtils.

		/// <summary>Set the clipboard text</summary>
		private static void GtkSetText(string text)
		{
			using (var cb = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false)))
			{
				cb.Text = text;
			}
		}

		/// <summary>Is there text on the clipboard?</summary>
		private static bool GtkContainsText()
		{
			using (var cb = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false)))
			{
				return cb.WaitIsTextAvailable();
			}
		}

		/// <summary>Get the clipboard text</summary>
		private static string GtkGetText()
		{
			using (var cb = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false)))
			{
				return cb.WaitForText();
			}
		}

		/// <summary>Is there an image on the clipboard?</summary>
		private static bool GtkContainsImage()
		{
			using (var cb = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false)))
			{
				return cb.WaitIsImageAvailable();
			}
		}

		/// <summary>Get the image from clipboard</summary>
		private static Image GtkGetImage()
		{
			using (var cb = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false)))
			{
				Gdk.Pixbuf buff = cb.WaitForImage();
				TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
				return (Image)tc.ConvertFrom(buff.SaveToBuffer("png"));
			}
		}

		/// <summary>Get the image from a file name on the clipboard</summary>
		private static Image GtkGetImageFromText()
		{
			var stringSeparators = new string[] { Environment.NewLine };
			var paths = GetText().Split(stringSeparators, StringSplitOptions.None);

			foreach (var path in paths)
			{
				if (File.Exists(path))
				{
					try
					{
						var bytes = File.ReadAllBytes(path);
						var buff = new Gdk.Pixbuf(bytes);
						TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
						return (Image)tc.ConvertFrom(buff.SaveToBuffer("png"));
					}
					catch (Exception e)
					{
						Console.Out.WriteLine("{0} is not an image file ({1}).", path, e.Message);
					}
				}
			}
			return null;
		}

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
			if (control is TextBoxBase)
			{
				(control as TextBoxBase).ContextMenu = null;
				return;
			}
			else if (control == null || control.Controls == null)
			{
				return;
			}
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
			if (control is TextBoxBase && (control as TextBoxBase).Handle == hwnd)
				return (control as TextBoxBase);
			else if (control == null || control.Controls == null)
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
			if (ContainsText())
			{
				var start = box.SelectionStart;
				var length = box.SelectionLength;
				var text = box.Text;
				if (length > 0)
				{
					if (start + length > text.Length)	// shouldn't happen, but sometimes paranoia pays
						text = text.Remove(start);
					else
						text = text.Remove(start, length);
				}
				var clipText = GetText();
				box.Text = text.Insert(start, clipText);
				box.SelectionStart = start + clipText.Length;
			}
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
			if (length > 0)
			{
				var start = box.SelectionStart;
				if (start + length > box.Text.Length)
					length = box.Text.Length - start;	// shouldn't happen, but paranoia sometimes pays.
				if (length <= 0)
					return true;
				var text = box.Text.Substring(start, length);
				SetText(text);
				if (cut)
				{
					box.Text = box.Text.Remove(start, length);
					box.SelectionStart = start;
				}
			}
			return true;
		}
	}
}
