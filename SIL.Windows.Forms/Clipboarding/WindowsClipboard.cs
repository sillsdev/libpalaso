// Copyright (c) 2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.ImageToolbox;

namespace SIL.Windows.Forms.Clipboarding
{
	internal class WindowsClipboard: IClipboard
	{
		public bool ContainsText() => Clipboard.ContainsText();
		public string GetText() => Clipboard.GetText();
		public string GetText(TextDataFormat format) => Clipboard.GetText(format);
		public void SetText(string text) => Clipboard.SetText(text);
		public void SetText(string text, TextDataFormat format) => Clipboard.SetText(text, format);
		public bool ContainsImage() => Clipboard.ContainsImage();
		public Image GetImage() => Clipboard.GetImage();

		public void CopyImageToClipboard(PalasoImage image)
		{
			// N.B.: PalasoImage does not handle .svg files
			if(image == null)
				return;

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
					// true here means that the image should remain on the System.Windows.Forms.Clipboard if the application exits
					Clipboard.SetDataObject(clips,true);
				}
			}
		}

		// Try to get an image from the System.Windows.Forms.Clipboard. If there simply isn't anything on the System.Windows.Forms.Clipboard that
		// can reasonably be interpreted as an image, return null. If there is something that makes sense
		// as an image, but trying to load it causes an exception, let the exception propagate.
		public PalasoImage GetImageFromClipboard()
		{
			// N.B.: PalasoImage does not handle .svg files
			var dataObject = Clipboard.GetDataObject();
			if (dataObject == null)
				return null;
			Exception ex = null;
			var textData = string.Empty;
			if (dataObject.GetDataPresent(DataFormats.UnicodeText))
				textData = dataObject.GetData(DataFormats.UnicodeText) as string;

			// The order of operations here is a bit tricky.
			// We want to maintain transparency in the original image, so we need to use the PNG format.
			// Clipboard.ContainsImage() and Clipboard.GetImage() use the plain bitmap format which loses
			// transparency.  So here we explicitly ask for a PNG, and see if we can get it.
			if (dataObject.GetDataPresent("PNG"))
			{
				var o = dataObject.GetData("PNG") as Stream;
				try
				{
					return PalasoImage.FromImage(Image.FromStream(o));
				}
				catch (Exception e)
				{
					ex = e;
				}
			}
			if (Clipboard.ContainsImage())
			{
				PalasoImage plainImage = null;
				try
				{
					plainImage = PalasoImage.FromImage(Clipboard.GetImage()); // this method won't copy any metadata
					var haveFileUrl = !string.IsNullOrEmpty(textData) && RobustFile.Exists(textData);

					// If we have an image on the System.Windows.Forms.Clipboard, and we also have text that is a valid url to an image file,
					// use the url to create a PalasoImage (which will pull in any metadata associated with the image too)
					if (haveFileUrl)
					{
						var imageWithPathAndMaybeMetadata = PalasoImage.FromFileRobustly(textData);
						plainImage.Dispose(); // important: don't do this until we've successfully created the imageWithPathAndMaybeMetadata
						return imageWithPathAndMaybeMetadata;
					}

					return plainImage;
				}
				catch (Exception e)
				{
					Logger.WriteEvent("PortableSystem.Windows.Forms.Clipboard.GetImageFromSystem.Windows.Forms.Clipboard() failed with message " + e.Message);
					if (plainImage != null)
						return plainImage; // at worst, we should return null; if FromFile() failed, we return an image
					throw;
				}
			}

			// People can do a "copy" from the Windows Photo Viewer but what it puts on the System.Windows.Forms.Clipboard is a path, not an image
			if (dataObject.GetDataPresent(DataFormats.FileDrop))
			{
				// This line gets all the file paths that were selected in explorer
				string[] files = dataObject.GetData(DataFormats.FileDrop) as string[];
				if (files == null)
					return null;

				foreach (var file in files.Where(f => RobustFile.Exists(f)))
				{
					return PalasoImage.FromFileRobustly(file);
				}

				return null; // not an image
			}

			if (Clipboard.ContainsText() && RobustFile.Exists(Clipboard.GetText()))
				return PalasoImage.FromImage(Image.FromStream(new FileStream(Clipboard.GetText(), FileMode.Open)));

			if (ex != null)
			{
				throw ex;
			}

			return null;
		}
	}
}