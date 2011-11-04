using System.Drawing;
using System.IO;
using Palaso.Code;
using Palaso.UI.WindowsForms.ClearShare;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public class PalasoImage
	{
		public MetaData MetaData;

		public PalasoImage()
		{
			MetaData = new MetaData();
		}

		public PalasoImage(Image image)
		{
			Image = image;
			FileName = null;
			MetaData = new MetaData();
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
		/// Should the user be able to make changes to MetaData?
		/// </summary>
		public bool MetaDataLocked
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
			MetaData.Write();
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
			var i = new PalasoImage()
					   {
						   Image = LoadImageWithoutLocking(path),
						   FileName = Path.GetFileName(path)
			};
			i.MetaData = MetaData.FromFile(path);
			return i;
		}

		/*
		 *
		[Test]
		public void MetaDataLocked_LoadedWithNonEmptyAttributionName_True()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			pi.AttributionName = "Joe Shmo";
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.IsTrue(incoming.MetaDataLocked);
			}
		}

		[Test]
		public void MetaDataLocked_LoadedWithNoMetaData_False()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.IsFalse(incoming.MetaDataLocked);
			}
		}*/
	}


}