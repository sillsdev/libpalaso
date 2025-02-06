// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SIL.Windows.Forms.ImageToolbox;

namespace SIL.Windows.Forms.Clipboarding
{
	internal partial class LinuxClipboard: IClipboard
	{
		private static IntPtr Clipboard => gtk_clipboard_get(gdk_atom_intern("CLIPBOARD", false));

		public bool ContainsText() => gtk_clipboard_wait_is_text_available(Clipboard);

		public string GetText() => gtk_clipboard_wait_for_text(Clipboard);

		public string GetText(TextDataFormat format) => GetText();

		public void SetText(string text)
		{
			gtk_clipboard_set_text(Clipboard, text, text.Length);
		}

		public void SetText(string text, TextDataFormat format) => SetText(text);

		public bool ContainsImage()
		{
			return gtk_clipboard_wait_is_image_available(Clipboard);
		}

		public Image GetImage()
		{
			var pixBuf = gtk_clipboard_wait_for_image(Clipboard);
			if (pixBuf == IntPtr.Zero)
				return null;

			var typeConverter = TypeDescriptor.GetConverter(typeof(Bitmap));
			return (Image)typeConverter.ConvertFrom(SaveToBuffer(pixBuf, "png"));
		}

		public void CopyImageToClipboard(PalasoImage image)
		{
			// N.B.: PalasoImage does not handle .svg files
			if(image == null)
				return;

			// Review: Someone who knows how needs to implement this!  #1105
			throw new NotImplementedException(
				"SIL.Windows.Forms.Miscellaneous.PortableClipboard.CopyImageToClipboard() is not yet implemented for Linux");
		}

		public PalasoImage GetImageFromClipboard()
		{
			// N.B.: PalasoImage does not handle .svg files
			if (ContainsImage())
				return PalasoImage.FromImage(GetImage());

			if (!ContainsText())
				return null;

			var stringSeparators = new[] { Environment.NewLine };
			var paths = GetText().Split(stringSeparators, StringSplitOptions.None);

			foreach (var path in paths)
			{
				if (!File.Exists(path))
					continue;

				try
				{
					return PalasoImage.FromFile(path);
				}
				catch (Exception e)
				{
					Console.Out.WriteLine("{0} is not an image file ({1}).", path, e.Message);
				}
			}
			return null;
		}
	}
}