using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using SIL.Code;
using SIL.Core.ClearShare;
using SIL.IO;
using SIL.Windows.Forms.ClearShare;

namespace SIL.Windows.Forms.ImageToolbox
{
	public class PalasoImage : IDisposable
	{
		private Metadata _metadata;

		private string _tempFilePath;

		public Metadata Metadata
		{
			get { return _metadata; }
			set
			{
				_metadata = value;
				_metadata.HasChanges = true; //i.e., we *do* want to save this to disk when we get a chance
			}
		}

		private string _originalFilePath;


		/// <summary>
		/// If false, you get a debug.Fail(). If true, you get a throw.
		/// </summary>
		public static bool ThrowOnFailureToDisposeAnyPalasoImage = false;

		/// <summary>
		/// If the object isn't disposed, the resulting message will give this label.
		/// This can help trace where it was created.
		/// </summary>
		public string LabelForDebugging = "unlabeled";

		/// <summary>
		/// Generally, when we load an image, we can happily forget where it came from, because
		/// the nature of the palaso image system is to deliver images, not file paths, to documents
		/// (we don't believe in "linking" to files somewhere on the disk which is just asking for problems
		/// as the document is shared).
		/// But in one circumstance, we do care: when the user chooses from disk (as opposed to from camera or scanner)
		/// and enters metadata, we want to store that metadata in the original.
		/// However, there is one circumstance (currently) in which this is not the original path:
		/// If we attempt to save metadata and can't (e.g. file is readonly), we create a temp file and
		/// store the metadata there, then serve the temp file to the requester. That's why we store this path.
		/// </summary>
		private string _pathForSavingMetadataChanges;

		public bool Disposed;

		public PalasoImage()
		{
			Metadata = new Metadata();
		}

		public PalasoImage(Image image)
		{
			Image = image;
			FileName = null;
			Metadata = new Metadata();
		}

		public static PalasoImage FromImage(Image image)
		{
			Guard.AgainstNull(image, "image");
			return new PalasoImage()
			{
				Image = image
			};
		}

		private Image _image;

		public Image Image
		{
			get
			{
				ThrowIfDisposedOfAlready();
				return _image;
			}
			set
			{
				// Note: If being re-assigned to same value as before, you don't want to dispose it, otherwise {value} will basically be gone
				if(_image!=null && _image!=value)
				{
					_image.Tag = "**** Disposed by palasoImage";
					_image.Dispose();
				}
				_image = value;
			}
		}

		private void ThrowIfDisposedOfAlready()
		{
			Guard.Against(Disposed, "This PalasoImage was used after being disposed of.");
		}

		/// <summary>
		/// Really, just the name, not the path.  Use if you want to save the image somewhere.
		/// Will be null if the PalasoImage was created via an Image instead of a file.
		/// </summary>
		public string FileName { get; internal set; }


		/// <summary>
		/// Should the user be able to make changes to Metadata?
		/// </summary>
		public bool MetadataLocked
		{
			get; set;
		}

		/// <summary>
		/// Save an image to the given path, with the metadata embedded.
		/// Use the file extension of path to determine to format to save.
		/// </summary>
		/// <param name="path"></param>
		public void Save(string path)
		{
			var format = GetImageFormatForExtension(Path.GetExtension(path));
			SaveInFormat(path, format);
		}

		/// <summary>
		/// Return the preferred file extension to be used for web format.
		/// Preserve jpeg or convert to png format.
		/// </summary>
		/// <param name="fileExtension"></param>
		public static string FileExtForWebFormat(string fileExtension)
		{
			var format = GetImageFormatForExtension(fileExtension);
			return format == ImageFormat.Jpeg ? ".jpg" : ".png";
		}

		private void SaveInFormat(string path, ImageFormat format)
		{
			ThrowIfDisposedOfAlready();
			SaveImageSafely(path, format);
			Metadata.Write(path);
		}

		private void SaveImageSafely(string path, ImageFormat format)
		{
			//related to preserving pixelformat (bit depth):
			//	http://stackoverflow.com/questions/7276212/reading-preserving-a-pixelformat-format48bpprgb-png-bitmap-in-net
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
				}
				catch (System.IO.IOException error)
				{
					throw new ApplicationException("The program could not replace the image " + path +
												   ", perhaps because this program or another locked it. Quit and try again. Then restart your computer and try again." + System.Environment.NewLine + error.Message);
				}
			}
			if (format.Equals(ImageFormat.Png) || format.Equals(ImageFormat.Bmp))
			{
				//The JPEG indirect saving below isn't needed for pngs and bmps, and
				// keeping it simple here prevents us from losing the bit depth of the original
				//reported in https://silbloom.myjetbrains.com/youtrack/issue/BL-2841
				Image.Save(path, format);
			}
			else
			{
				//nb: there are cases (notably http://jira.palaso.org/issues/browse/WS-34711, after cropping a jpeg) where we get out of memory if we are not operating on a copy
				using (var image = new Bitmap(Image))
				{
					image.Save(path, format);
				}
			}
		}

		private static ImageFormat GetImageFormatForExtension(string fileExtension)
		{
			if (string.IsNullOrEmpty(fileExtension))
				throw new ArgumentException(
					string.Format("Bad extension: {0}", fileExtension));

			switch (fileExtension.ToLower())
			{
			case @".bmp":
				return ImageFormat.Bmp;

			case @".gif":
				return ImageFormat.Gif;

			case @".jpg":
			case @".jpeg":
				return ImageFormat.Jpeg;

			case @".png":
				return ImageFormat.Png;

			case @".tif":
			case @".tiff":
				return ImageFormat.Tiff;

			default:
				throw new NotImplementedException();
			}
		}


		/// <summary>
		/// If you edit the metadata, call this. If it happens to have an actual file associated, it will save it.
		/// If not (e.g. the image came from a scanner), it won't do anything.
		///
		/// Warning. Don't use this on original images. See https://jira.sil.org/browse/BL-1001.
		/// </summary>
		public void SaveUpdatedMetadataIfItMakesSense()
		{
			ThrowIfDisposedOfAlready();
			Guard.AssertThat(FileFormatSupportsMetadata, "We can't put metadata into images of this format.");

			if (Metadata != null && Metadata.HasChanges && !string.IsNullOrEmpty(_pathForSavingMetadataChanges) && File.Exists(_pathForSavingMetadataChanges))
				SaveUpdatedMetadata();
		}

		/// <summary>Returns if the format of the image file supports metadata</summary>
		public bool FileFormatSupportsMetadata
		{
			get { return Metadata.FileFormatSupportsMetadata(_pathForSavingMetadataChanges); }
		}

		private void SaveUpdatedMetadata()
		{
			Metadata.Write(_pathForSavingMetadataChanges);
		}

		// Figure what extension an image file SHOULD have, based on its contents.
		// Adapted from code at https://stackoverflow.com/questions/210650/validate-image-from-file-in-c-sharp
		// by https://stackoverflow.com/users/499558/alex
		private static string GetCorrectImageExtension(string path)
		{
			byte[] bytes = new byte[10];
			RetryUtility.Retry(() => {
				using (var file = File.OpenRead(path))
				{
					file.Read(bytes, 0, 10);
				}
			}, memo:$"PalasoImage.GetCorrectImageExtension({path})");
			// see http://www.mikekunz.com/image_file_header.html
				var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
				var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
				var png = new byte[] { 137, 80, 78, 71 };    // PNG
				var tiff = new byte[] { 73, 73, 42 };         // TIFF
				var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
				var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
				var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon

				if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
					return "bmp";

				if (gif.SequenceEqual(bytes.Take(gif.Length)))
					return "gif";

				if (png.SequenceEqual(bytes.Take(png.Length)))
					return "png";

				if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
					return "tif";

				if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
					return "tif";

				if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
					return "jpg";

				if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
					return "jpg";

				// We can't guess, keep what it came with.
				return Path.GetExtension(path);
		}

		private static Image LoadImageWithoutLocking(string path, out string tempPath)
		{
			/*          1) Naïve approach:  locks until the image is dispose of some day, which is counter-intuitive to me
							  return Image.FromFile(path);

						2) Contrary to the docs on Image.FromStream ("You must keep the stream open for the lifetime of the Image."),
							MSDN http://support.microsoft.com/kb/309482 suggests the following work-around
							using (var fs = new System.IO.FileStream(path, FileMode.Open, FileAccess.Read))
							{
								return Image.FromStream(fs);
							}
			*/

			//But note, it's not clear if (2) will very occasionally die with "out of memory": http://jira.palaso.org/issues/browse/BL-199
			// Note: if the FileStream is closed, attempts to modify the image result in an "out of memory" error.

			//3) Use a temp file, which remains locked, but no one notices because the original file is unlocked.

			//if(Path.GetExtension(path)==".jpg")
			{
				var leakMe = TempFile.WithExtension(GetCorrectImageExtension(path));
				RobustFile.Copy(path, leakMe.Path, true);
				//we output the tempPath so that the caller can clean it up later
				tempPath = leakMe.Path;

				//Note, Image.FromFile(some 8 bit or 48 bit png) will always give you a 32 bit image.
				//See http://stackoverflow.com/questions/7276212/reading-preserving-a-pixelformat-format48bpprgb-png-bitmap-in-net
				//There is a second argument here, useEmbeddedColorManagement, that is said to preserve it if set to true, but it doesn't work.

				try
				{
					return Image.FromFile(leakMe.Path, true);
				}
				catch (OutOfMemoryException e)
				{
					// very often means really that the image file is corrupt.
					// We'll try to diagnose that by attempting to get metadata. If that throws (probably TagLib.CorruptFileException),
					// assume it's a better indication of the problem.
					var metadata = Metadata.FromFile(path);
					if (metadata.IsOutOfMemoryPlausible(e))
						#pragma warning disable CA2200
						// ReSharper disable once PossibleIntendedRethrow
						throw e; // Deliberately NOT just "throw", that loses the extra information IsOutOfMemoryPlausible added to the exception.
						#pragma warning restore CA2200
					throw new TagLib.CorruptFileException("File could not be read and is possible corrupted", e);
				}
			}
		}


		public static PalasoImage FromFile(string path)
		{
			string tempPath;
			var i = new PalasoImage
			{
				Image = LoadImageWithoutLocking(path, out tempPath),
				FileName = Path.GetFileName(path),
				_originalFilePath = path,
				_pathForSavingMetadataChanges = path,
				Metadata = Metadata.FromFile(tempPath),
				_tempFilePath = tempPath
			};
			NormalizeImageOrientation(i);
			return i;
		}

		/// <summary>
		/// Load a PalasoImage from a file, trying several times if needed.
		/// </summary>
		/// <remarks>
		/// This would logically belong in SIL.Core.IO.RobustIO except that PalasoImage is in SIL.Windows.Forms.
		/// </remarks>
		public static PalasoImage FromFileRobustly(string path)
		{
			try
			{
				return RetryUtility.Retry(() => PalasoImage.FromFile(path),
					RetryUtility.kDefaultMaxRetryAttempts,
					RetryUtility.kDefaultRetryDelay,
					new HashSet<Type>
					{
						typeof(System.IO.IOException),
						// Odd type to catch... but it seems that Image.FromFile (which is called in the bowels of PalasoImage.FromFile)
						// throws OutOfMemoryException when the file is inaccessible.
						// See http://stackoverflow.com/questions/2610416/is-there-a-reason-image-fromfile-throws-an-outofmemoryexception-for-an-invalid-i
						typeof(System.OutOfMemoryException),
						// Again you'd expect that if it's corrupt, it would stay that way, but
						// experimentally, it seems we can get this if the file can't be read because it is (temporarily?) locked.
						// (The text of the message reads, "File could not be read and is possible corrupted", which
						// suggests they are using this to cover any case of not being able to read the file."
						typeof(TagLib.CorruptFileException)
					});
			}
			catch (Exception e)
			{
				// In case something else goes wrong, at least some errors we've seen from here
				// (including TagLib.CorruptFileException) don't tell us WHICH FILE has the
				// problem, so wrap in another layer that does.
				throw new ApplicationException(
					"Could not make PalasoImage from " + path + " because " + e.Message, e);
			}
		}

		/// <summary>
		/// Save a PalasoImage to a file, trying several times if needed.
		/// </summary>
		/// <remarks>
		/// This would logically belong in SIL.Core.IO.RobustIO except that PalasoImage is in SIL.Windows.Forms.
		/// </remarks>
		public static void SaveImageRobustly(PalasoImage image, string fileName)
		{
			RetryUtility.Retry(() => image.Save(fileName),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				});
		}

		/// <summary>
		/// If the image contains metadata indicating that it is mirrored or rotated,
		/// convert it to normal orientation (and remove the metadata).
		/// </summary>
		/// <param name="i"></param>
		private static void NormalizeImageOrientation(PalasoImage i)
		{
			var img = i.Image;
			if (Array.IndexOf(img.PropertyIdList, 274) > -1)
			{
				var orientation = (int)img.GetPropertyItem(274).Value[0];
				switch (orientation)
				{
					case 1:
						// No rotation required.
						break;
					case 2:
						img.RotateFlip(RotateFlipType.RotateNoneFlipX);
						break;
					case 3:
						img.RotateFlip(RotateFlipType.Rotate180FlipNone);
						break;
					case 4:
						img.RotateFlip(RotateFlipType.Rotate180FlipX);
						break;
					case 5:
						img.RotateFlip(RotateFlipType.Rotate90FlipX);
						break;
					case 6:
						img.RotateFlip(RotateFlipType.Rotate90FlipNone);
						break;
					case 7:
						img.RotateFlip(RotateFlipType.Rotate270FlipX);
						break;
					case 8:
						img.RotateFlip(RotateFlipType.Rotate270FlipNone);
						break;
				}
				// This EXIF data is now invalid and should be removed.
				img.RemovePropertyItem(274);
			}
			i.Metadata.NormalizeOrientation(); // remove it from metadata too.
		}

		/// <summary>
		/// will be set if this was created using FromFile
		/// </summary>
		public string OriginalFilePath
		{
			get { return _originalFilePath; }
		}

		/// <summary>
		/// will be set if this was created using FromFile
		/// </summary>
		public string PathForSavingMetadataChanges
		{
			get { return _pathForSavingMetadataChanges; }
			internal set { _pathForSavingMetadataChanges = value; }
		}

		/*
		 *
		[Test]
		public void MetadataLocked_LoadedWithNonEmptyAttributionName_True()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			pi.AttributionName = "Joe Shmo";
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.IsTrue(incoming.MetadataLocked);
			}
		}

		[Test]
		public void MetadataLocked_LoadedWithNoMetadata_False()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.IsFalse(incoming.MetadataLocked);
			}
		}*/

		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SuppressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			if (Disposed)
				return;

			if (disposing)
			{
				string imageLabel = _image==null? "no-image":"with-image";
				if (Image != null)
				{
					Image.Dispose();
					Image = null;
				}

				if (!string.IsNullOrEmpty(_tempFilePath))
				{
					try
					{
						File.Delete(_tempFilePath);
					}
					catch (Exception e)
					{
						Debug.Fail("Not able to delete image temp file.", e.ToString());
					}
				}
				Disposed = true;
			}
		}

		~PalasoImage()
		{
			if (_image!=null && //Todo: it would be nice if we were water tight, but at the moment, if there was not image, we don't really care
				!Disposed && LicenseManager.UsageMode != LicenseUsageMode.Designtime)//don't know if this will work here
			{
				string imageLabel = _image == null ? "no-image" : "with-image";

				if (!Disposed)
				{
					var message = "PalasoImage wasn't disposed of properly: " + imageLabel + ". LabelForDebugging=" + LabelForDebugging;
					if (ThrowOnFailureToDisposeAnyPalasoImage)
					{
						throw new PalasoImageNotDisposed(message);
					}
					else
					{
						Debug.Fail(message);
					}
				}
			}
		}
	}
	public class PalasoImageNotDisposed : ApplicationException
	{
		public PalasoImageNotDisposed(string message) : base(message)
		{
		}
	}
}
