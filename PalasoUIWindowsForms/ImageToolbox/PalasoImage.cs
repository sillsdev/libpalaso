using System.Drawing;
using System.IO;
using Palaso.Code;
using Palaso.UI.WindowsForms.ClearShare;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public class PalasoImage
	{
		public Metadata Metadata;

		/// <summary>
		/// generally, when we load an image, we can happily forget where it came from, becuase
		/// the nature of the palso image system is to deliver images, not file paths, to documents
		/// (we don't believe in "linking" to files somewhere on the disk which is just asking for problems
		/// as the document is shared.
		/// But in on circumumstance, we do care: when the user chooses a from disk (as opposed to from camera or scanner)
		/// and enters metadata, we want to store that metadata in the original.  That's the only reason we store this path.
		/// </summary>
		private static string _pathForSavingMetadataChanges;

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


		public Image Image { get; private set; }

		/// <summary>
		/// Really, just the name, not the path.  Use if you want to save the image somewhere.
		/// Will be null if the PalasoImage was created via an Image instead of a file.
		/// </summary>
		public string FileName { get; private set; }


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
		   Image.Save(path);
			Metadata.Write();
		}

		/// <summary>
		/// If you edit the metadata, call this. If it happens to have an actual file associated, it will save it.
		/// If not (e.g. the image came from a scanner), it won't do anything.
		/// </summary>
		public void SaveUpdatedMetadataIfItMakesSense()
		{
			if(Metadata!=null && Metadata.HasChanges && !string.IsNullOrEmpty(_pathForSavingMetadataChanges) && File.Exists(_pathForSavingMetadataChanges))
			{
				Metadata.Write(_pathForSavingMetadataChanges);
				Metadata.HasChanges = false;
			}
		}

		private static Image LoadImageWithoutLocking(string path)
		{
			//locks until the image is dispose of some day, which is counter-intuitive to me
			//  return Image.FromFile(path);

			//following work-around from http://support.microsoft.com/kb/309482
			using (var fs = new System.IO.FileStream(path, FileMode.Open, FileAccess.Read))
			{
				return Image.FromStream(fs);
			}
		}


		public static PalasoImage FromFile(string path)
		{
			_pathForSavingMetadataChanges = path;
			var i = new PalasoImage()
					   {
						   Image = LoadImageWithoutLocking(path),
						   FileName = Path.GetFileName(path)
			};
			i.Metadata = Metadata.FromFile(path);
			return i;
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
	}


}