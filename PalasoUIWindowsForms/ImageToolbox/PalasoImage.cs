using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Palaso.Code;
using Palaso.IO;
using Palaso.UI.WindowsForms.ClearShare;
using System.Linq;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public class PalasoImage : IDisposable

	{
		private Metadata _metadata;

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
		/// Generally, when we load an image, we can happily forget where it came from, because
		/// the nature of the palaso image system is to deliver images, not file paths, to documents
		/// (we don't believe in "linking" to files somewhere on the disk which is just asking for problems
		/// as the document is shared).
		/// But in one circumumstance, we do care: when the user chooses from disk (as opposed to from camera or scanner)
		/// and enters metadata, we want to store that metadata in the original.  
		/// However, there is one circumstance (currently) in which this is not the original path:
		/// If we attempt to save metadata and can't (e.g. file is readonly), we create a temp file and 
		/// store the metadata there, then serve the temp file to the requestor.  That's why we store this path.
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
				if(_image!=null)
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
		/// Save an image to the given path, with the metadata embeded
		/// </summary>
		/// <param name="path"></param>
		public void Save(string path)
		{
			ThrowIfDisposedOfAlready();
			SaveImageSafely(path);
			Metadata.Write(path);
		}

		private void SaveImageSafely(string path)
		{
			using (var image = new Bitmap(Image))
			//nb: there are cases (notibly http://jira.palaso.org/issues/browse/WS-34711, after cropping a jpeg) where we get out of memory if we are not operating on a copy
			{
				if (File.Exists(path))
				{
					try
					{
						File.Delete(path);
					}
					catch (System.IO.IOException error)
					{
						throw new ApplicationException("The program could not replace the image " + path +
													   ", perhaps because this program or another locked it. Quit and try again. Then restart your computer and try again."+System.Environment.NewLine+error.Message);
					}
				}
				image.Save(path, ShouldSaveAsJpeg ? ImageFormat.Jpeg : ImageFormat.Png);
			}
		}

		private bool ShouldSaveAsJpeg
		{
			get
			{
				/*
			 * Note, each guid is VERY SIMILAR. The difference is only in the last 2 digits of the 1st group.
			   Undefined  B96B3CA9
				MemoryBMP  B96B3CAA
				BMP    B96B3CAB
				EMF    B96B3CAC
				WMF    B96B3CAD
				JPEG    B96B3CAE
				PNG    B96B3CAF
				GIF    B96B3CB0
				TIFF    B96B3CB1
				EXIF    B96B3CB2
				Icon    B96B3CB5
			 */
				if (ImageFormat.Jpeg.Guid == Image.RawFormat.Guid)
					return true;

				if (ImageFormat.Jpeg.Equals(Image.PixelFormat)) //review
					return true;

				if (string.IsNullOrEmpty(FileName))
					return false;

				return new[] {"jpg", "jpeg"}.Contains(Path.GetExtension(FileName).ToLower());
			}
		}

		/// <summary>
		/// If you edit the metadata, call this. If it happens to have an actual file associated, it will save it.
		/// If not (e.g. the image came from a scanner), it won't do anything.
		/// </summary>
		public void SaveUpdatedMetadataIfItMakesSense()
		{
			try
			{
				ThrowIfDisposedOfAlready();
				if (Metadata != null && Metadata.HasChanges && !string.IsNullOrEmpty(_pathForSavingMetadataChanges) && File.Exists(_pathForSavingMetadataChanges))
					SaveUpdatedMetadata();
			}
			catch (SystemException ex)
			{
				if (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
				{
					//maybe we just can't write to the original file
					//so try making a copy and writing to that

					//note: this means the original file will not have metadata saved to it meaning that
					//if we insert the same file again, the rights will not be the same (or have to be re-modified)
					//enhance: we could, theoretically, maintain some sort persistent map with the source file and metadata
					string origFilePath = PathForSavingMetadataChanges;
					if (!string.IsNullOrEmpty(origFilePath) && File.Exists(origFilePath))
					{
						string tempPath = TempFile.WithExtension(Path.GetExtension(origFilePath)).Path;
						Save(tempPath);
						PathForSavingMetadataChanges = tempPath;
						FileName = Path.GetFileName(tempPath);
						SaveUpdatedMetadata();
					}
					else
						throw;
				}
				else
					throw;
			}
		}
		private void SaveUpdatedMetadata()
		{
			Metadata.Write(_pathForSavingMetadataChanges);
			Metadata.HasChanges = false;
		}

		private static Image LoadImageWithoutLocking(string path)
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

			//3) Just leak a temp file.

			//if(Path.GetExtension(path)==".jpg")
			{
				var leakMe = TempFile.WithExtension(Path.GetExtension(path));
				File.Delete(leakMe.Path);
				File.Copy(path, leakMe.Path);
				return Image.FromFile(leakMe.Path);
			}
		}


		public static PalasoImage FromFile(string path)
		{
			var i = new PalasoImage
			{
				Image = LoadImageWithoutLocking(path),
				FileName = Path.GetFileName(path),
				_originalFilePath = path,
				_pathForSavingMetadataChanges = path,
				Metadata = Metadata.FromFile(path)
			};
			return i;
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
			// Therefore, you should call GC.SupressFinalize to
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
				Debug.WriteLine("Disposing PalasoImage "+imageLabel);
				if (Image != null)
				{
					Image.Dispose();
					Image = null;
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
				Debug.Assert(Disposed, "PalasoImage wasn't disposed of properly: " + imageLabel);
			}
		}
	}


}