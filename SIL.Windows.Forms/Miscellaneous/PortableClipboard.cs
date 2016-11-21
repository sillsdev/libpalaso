using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.ImageToolbox;

namespace SIL.Windows.Forms.Miscellaneous
{
	/// <summary>Uses the GTK classes when accessing the clipboard on Linux</summary>
	public static class PortableClipboard
	{
		public static bool ContainsText()
		{
#if __MonoCS__
			return GtkContainsText();
#else
			return Clipboard.ContainsText();
#endif
		}

		public static string GetText()
		{
#if __MonoCS__
			return GtkGetText();
#else
			return Clipboard.GetText();
#endif
		}

		public static string GetText(TextDataFormat format)
		{
#if __MonoCS__
			return GtkGetText();
#else
			return Clipboard.GetText(format);
#endif
		}

		public static void SetText(string text)
		{
#if __MonoCS__
			GtkSetText(text);
#else
			Clipboard.SetText(text);
#endif
		}

		public static void SetText(string text, TextDataFormat format)
		{
#if __MonoCS__
			GtkSetText(text);
#else
			Clipboard.SetText(text, format);
#endif
		}

		public static bool ContainsImage()
		{
#if __MonoCS__
			return GtkContainsImage();
#else
			return Clipboard.ContainsImage();
#endif
		}

		public static System.Drawing.Image GetImage()
		{
#if __MonoCS__
			return GtkGetImage();
#else
			return Clipboard.GetImage();
#endif
		}

		public static void CopyImageToClipboard(PalasoImage image)
		{
			// N.B.: PalasoImage does not handle .svg files
			if(image == null)
				return;
#if __MonoCS__
			// Review: Someone who knows how needs to implement this!
			throw new NotImplementedException("SIL.Windows.Forms.Miscellaneous.PortableClipboard.CopyImageToClipboard() is not yet implemented for Linux");
#else
			if (image.Image == null)
			{
				if (String.IsNullOrEmpty(image.OriginalFilePath))
					return;
				// no image, but a path
				Clipboard.SetFileDropList(new StringCollection() {image.OriginalFilePath});
			}
			else
			{
				if (String.IsNullOrEmpty(image.OriginalFilePath))
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
#endif
		}

		public static PalasoImage GetImageFromClipboard()
		{
			// N.B.: PalasoImage does not handle .svg files
#if __MonoCS__
			if (GtkContainsImage())
				return PalasoImage.FromImage(GtkGetImage());

			if (GtkContainsText())
			{
				//REVIEW: I can find no documentation on GtkClipboard. If ContainsText means we have a file
				//	path, then it would be better to do PalasoImage.FromFileRobustly(); on the file path
				return PalasoImage.FromImage(GtkGetImageFromText());
			}

			return null;
#else
			var dataObject = Clipboard.GetDataObject();
			if (dataObject == null)
				return null;

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
					else
					{
						return plainImage;
					}
				}
				catch (Exception e)
				{
					Logger.WriteEvent("PortableClipboard.GetImageFromClipboard() failed with message " + e.Message);
					return plainImage; // at worst, we should return null; if FromFile() failed, we return an image
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
				catch (Exception)
				{}
			}

			//People can do a "copy" from the WIndows Photo Viewer but what it puts on the clipboard is a path, not an image
			if (dataObject.GetDataPresent(DataFormats.FileDrop))
			{
				//This line gets all the file paths that were selected in explorer
				string[] files = dataObject.GetData(DataFormats.FileDrop) as string[];
				if (files == null) return null;

				foreach (var file in files.Where(f => RobustFile.Exists(f)))
				{
					try
					{
						return PalasoImage.FromFileRobustly(file);
					}
					catch (Exception)
					{}
				}

				return null; //not an image
			}

			if (!Clipboard.ContainsText() || !RobustFile.Exists(Clipboard.GetText())) return null;

			try
			{
				return PalasoImage.FromImage( Image.FromStream(new FileStream(Clipboard.GetText(), FileMode.Open)));
			}
			catch (Exception)
			{}

			return null;
#endif
		}

#if __MonoCS__
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
		private static System.Drawing.Image GtkGetImage()
		{
			using (var cb = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false)))
			{
				Gdk.Pixbuf buff = cb.WaitForImage();
				TypeConverter tc = TypeDescriptor.GetConverter(typeof(System.Drawing.Bitmap));
				return (System.Drawing.Image)tc.ConvertFrom(buff.SaveToBuffer("png"));
			}
		}

		/// <summary>Get the image from a file name on the clipboard</summary>
		private static System.Drawing.Image GtkGetImageFromText()
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
						TypeConverter tc = TypeDescriptor.GetConverter(typeof(System.Drawing.Bitmap));
						return (System.Drawing.Image)tc.ConvertFrom(buff.SaveToBuffer("png"));
					}
					catch (Exception e)
					{
						Console.Out.WriteLine("{0} is not an image file ({1}).", path, e.Message);
					}
				}
			}
			return null;
		}
#endif
	}

}
