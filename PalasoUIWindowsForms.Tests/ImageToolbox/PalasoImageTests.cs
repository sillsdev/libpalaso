﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.IO;
using Palaso.UI.WindowsForms.ClearShare;
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

		[Test, Ignore("by hand only")]
		public void FromFile_HugeJPEG_DoesNotCrash()
		{
			//nb: trying to reproduce a problem that came up in bloom with this very image, but
			//i never did get this to crash here
				PalasoImage.FromFile(@"C:\Users\John\Desktop\hugetestimage.jpg");
				PalasoImage.FromFile(@"C:\Users\John\Desktop\hugetestimage.jpg");
				PalasoImage.FromFile(@"C:\Users\John\Desktop\hugetestimage.jpg");
				PalasoImage.FromFile(@"C:\Users\John\Desktop\hugetestimage.jpg");
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
		public void LoadAndSave_DeleteAfter_NothingLocked()
		{
			var png = new Bitmap(10, 10);
			using (var temp = new TempFile(false))
			{
				png.Save(temp.Path);
				var pi = PalasoImage.FromFile(temp.Path);
				pi.Metadata.CopyrightNotice = "Copyright 2011 me";
				Assert.DoesNotThrow(() => pi.Save(temp.Path));
				Assert.DoesNotThrow(() => File.Delete(temp.Path));
			}
		}

		[Test]
		public void Locked_LoadedButImageHasNoMetadata_False()
		{
			var png = new Bitmap(10, 10);
			using (var temp = new TempFile(false))
			{
				png.Save(temp.Path);
				var pi = PalasoImage.FromFile(temp.Path);
				Assert.IsFalse(pi.MetadataLocked);
			}
		}
		[Test]
		public void Locked_NewOne_False()
		{
			var pi = new PalasoImage();
			Assert.IsFalse(pi.MetadataLocked);
		}

//        [Test]
//        public void MetadataLocked_IfLoadedWithIllustrator_True()
//        {
//            var png = new Bitmap(10, 10);
//            using (var temp = new TempFile(false))
//            {
//                var pi = PalasoImage.FromImage(png);
//                pi.Metadata.AttributionName = "me";
//                pi.Save(temp.Path);
//                var incoming = PalasoImage.FromFile(temp.Path);
//                Assert.IsTrue(incoming.MetadataLocked);
//            }
//        }

	}
}
