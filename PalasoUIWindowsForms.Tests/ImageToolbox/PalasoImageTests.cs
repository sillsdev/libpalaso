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
	[TestFixture]
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

		[Test, Ignore("not yet")]
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

		[Test, Ignore("not yet")]
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
				Assert.IsFalse(pi.Locked);
			}
		}
		[Test]
		public void Locked_NewOne_False()
		{
			var pi = new PalasoImage();
			Assert.IsFalse(pi.Locked);
		}

		[Test, Ignore("not yet")]
		public void Locked_LoadedWithIllustrator_True()
		{
			var png = new Bitmap(10, 10);
			using (var temp = new TempFile(false))
			{
				var pi = PalasoImage.FromImage(png);
				pi.IllustratorPhotographer = "me";
				pi.Save(temp.Path);
				var incoming = PalasoImage.FromFile(temp.Path);
				Assert.IsTrue(incoming.Locked);
			}
		}


		[Test, Ignore("not yet")]
		public void Illustrator_TryToChangeWhileLocked_Throws()
		{
			var pi = new PalasoImage();
			pi.Locked = true;
			Assert.Throws<ApplicationException>(() => pi.IllustratorPhotographer = "me");
		}
	}
}
