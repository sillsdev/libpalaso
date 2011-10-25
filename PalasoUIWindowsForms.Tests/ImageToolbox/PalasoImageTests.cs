using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.IO;
using Palaso.UI.WindowsForms.ImageToolbox;

namespace PalasoUIWindowsForms.Tests.ImageToolbox
{
	[TestFixture, Ignore("Needs exiftool in the distfiles")]
	public class PalasoImageTests
	{
		[Test]
		public void FileName_CreatedWithImageOnly_Null()
		{
			var pi = PalasoImage.FromImage(new Bitmap(10,10));
			Assert.IsNull(pi.FileName);
		}

		/// <summary>
		/// now, whether it should give the same image or just a clone is left to the future,
		/// this would just document whatever is true
		/// </summary>
		[Test]
		public void Image_CreatedWithImageOnly_GivesSameImage()
		{
			Bitmap bitmap = new Bitmap(10, 10);
			var pi = new PalasoImage(bitmap);
			Assert.AreEqual(bitmap, pi.Image);
		}

		[Test]
		public void Image_FromFile_GivesImage()
		{
			using(Bitmap bitmap = new Bitmap(10, 10))
			using (var temp = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				bitmap.Save(temp.Path);
				var pi = PalasoImage.FromFile(temp.Path);
				Assert.AreEqual(10, pi.Image.Width);
			}
		}

		[Test]
		public void FromFile_DoesNotLockFile()
		{
		   using(Bitmap bitmap = new Bitmap(10, 10))
			using (var temp = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				bitmap.Save(temp.Path);
				PalasoImage.FromFile(temp.Path);
				Assert.DoesNotThrow(() => File.Delete(temp.Path));
			}
		}

		[Test]
		public void Constructor_CreatedWithFileThatDoesNotExist_Throws()
		{
			Assert.Throws<FileNotFoundException>(
				() => PalasoImage.FromFile("not going to find me"));
		}

		[Test]
		public void FromImage_NullImage_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => PalasoImage.FromImage(null));
		}

		[Test]
		public void SaveAndLoad_PNG_PreservesCopyrightNotice()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			pi.CopyrightNotice = "Copyright Test";
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
			   Assert.AreEqual(pi.CopyrightNotice, incoming.CopyrightNotice);
			}
		}

		[Test]
		public void SaveAndLoad_HasCreativeCommonsLicense_ReadsInSameLicense()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			pi.Licenses.Add(new CreativeCommonsLicense());
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.AreEqual(1,incoming.Licenses.Count);
				Assert.IsInstanceOf(typeof (CreativeCommonsLicense), incoming.Licenses[0]);
			}
		}

		[Test]
		public void SaveAndLoad_HasAttributionNameButNoCCLicense_Roundtrips()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			pi.AttributionName = "little < big";
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.AreEqual(pi.AttributionName, incoming.AttributionName);
			}
		}
		[Test]
		public void SaveAndLoad_HasAttributionUrlButNoCCLicense_Roundtrips()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			pi.AttributionUrl = "http://somewhere.com";
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.AreEqual(pi.AttributionUrl, incoming.AttributionUrl);
			}
		}

		[Test]
		public void SaveAndLoad_PNG_PreservesAttributionName()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			pi.AttributionName = "Joe Shmo";
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.AreEqual(pi.AttributionName, incoming.AttributionName);
			}
		}


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
		}

		[Test]
		public void LoadFromFile_CopyrightNotSet_CopyrightGivesNull()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			using (var temp = TempFile.WithExtension("png"))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.IsNull(incoming.CopyrightNotice);
			}
		}

		[Test]
		public void SaveAndLoad_PNGWithDangerousCharacters_PreservesCopyrightNotice()
		{
			var png = new Bitmap(10, 10);
			var pi = new PalasoImage(png);
			pi.CopyrightNotice = "Copyright <! ' <hello>";
			using (var temp = new TempFile(false))
			{
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.AreEqual(pi.CopyrightNotice, incoming.CopyrightNotice);
			}
		}

		[Test]
		public void LoadAndSave_DeleteAfter_NothingLocked()
		{
			var png = new Bitmap(10, 10);
			using (var temp = new TempFile(false))
			{
				png.Save(temp.Path);
				var pi = PalasoImage.FromFile(temp.Path);
				pi.CopyrightNotice = "Copyright 2011 me";
				Assert.DoesNotThrow(() => pi.Save(temp.Path));
				Assert.DoesNotThrow(() => File.Delete(temp.Path));
			}
		}

		[Test]
		public void Locked_LoadedButImageHasNoMetaData_False()
		{
			var png = new Bitmap(10, 10);
			using (var temp = new TempFile(false))
			{
				png.Save(temp.Path);
				var pi = PalasoImage.FromFile(temp.Path);
				Assert.IsFalse(pi.MetaDataLocked);
			}
		}
		[Test]
		public void Locked_NewOne_False()
		{
			var pi = new PalasoImage();
			Assert.IsFalse(pi.MetaDataLocked);
		}

		[Test]
		public void MetaDataLocked_IfLoadedWithIllustrator_True()
		{
			var png = new Bitmap(10, 10);
			using (var temp = new TempFile(false))
			{
				var pi = PalasoImage.FromImage(png);
				pi.AttributionName = "me";
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.IsTrue(incoming.MetaDataLocked);
			}
		}

	}
}
