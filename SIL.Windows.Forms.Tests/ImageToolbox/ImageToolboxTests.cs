using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using L10NSharp;
using NUnit.Framework;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ImageToolbox;
using SIL.Windows.Forms.ImageToolbox.ImageGallery;

namespace SIL.Windows.Forms.Tests.ImageToolbox
{
	[Apartment(ApartmentState.STA)]
	public class ImageToolboxTests
	{
		private bool previousInitializationMode;
		[OneTimeSetUp]
		public void Setup()
		{
			previousInitializationMode = LocalizationManager.StrictInitializationMode;
			LocalizationManager.StrictInitializationMode = false;
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			LocalizationManager.StrictInitializationMode = previousInitializationMode;
		}

		[Test]
		[Explicit("By hand only")]
		public void ShowToolbox()
		{
			Application.EnableVisualStyles();
			using (var dlg = new ImageToolboxDialog(new PalasoImage(),null))// "arrow"))
			{
				if (DialogResult.OK == dlg.ShowDialog())
				{
					// File name ending in .tmp will confuse TagLib#...doesn't know what kind of metadata to write.
					var ext = ".png";
					if (Path.GetExtension(dlg.ImageInfo.OriginalFilePath) == ".jpg")
						ext = ".jpg";
					string path  = Path.ChangeExtension(Path.GetTempFileName(), ext);
					dlg.ImageInfo.Save(path);
					Process.Start("explorer.exe", "/select, \"" + path + "\"");
				}
			}
		}

		[Test]
		[Explicit("By hand only")]
		public void ShowGeckoToolbox()
		{
			Application.EnableVisualStyles();
			ThumbnailViewer.UseWebViewer = true;
			using (var dlg = new ImageToolboxDialog(new PalasoImage(), null))// "arrow"))
			{
				if (DialogResult.OK == dlg.ShowDialog())
				{
					// File name ending in .tmp will confuse TagLib#...doesn't know what kind of metadata to write.
					string path = Path.ChangeExtension(Path.GetTempFileName(), ".png");
					dlg.ImageInfo.Save(path);
					Process.Start("explorer.exe", "/select, \"" + path + "\"");
				}

			}
		}

		[Test]
		[Explicit("By hand only")]
		public void ShowToolboxWith_PreExisting_Image_WithMetadata()
		{
			Application.EnableVisualStyles();
			PalasoImage i = PalasoImage.FromImage(LicenseLogos.by_nd);
			i.Metadata.License = new CreativeCommonsLicenseWithImage(true,true, CreativeCommonsLicenseWithImage.DerivativeRules.DerivativesWithShareAndShareAlike);
			i.Metadata.CopyrightNotice = "Copyright 1992 Papua New Guinea Department of Education and Other Good Things";
			i.Metadata.CollectionName = "International Illustrations: The Art Of Reading";
			i.Metadata.Creator = "Various Talented Illustrators";
			//using (var f = TempFile.WithExtension(".png"))
			{
				//i.Save(f.Path);
				using (var dlg = new ImageToolboxDialog(i, "arrow"))
				{
					dlg.ShowDialog();
				}
			}
		}

		[Test]
		[Explicit("By hand only")]
		public void ShowToolboxWith_PreExisting_EnsureRawFormatUnchanged()
		{
			Application.EnableVisualStyles();
			PalasoImage i = PalasoImage.FromImage(TestImages.logo);

			using (var dlg = new ImageToolboxDialog(i, ""))
				{
					dlg.ShowDialog();
					Assert.AreEqual(ImageFormat.Jpeg.Guid, dlg.ImageInfo.Image.RawFormat.Guid);
				}
		}

		[Test]
		public void DoubleCheckFileFilterWorks()
		{
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(null, "foo.txt"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(null, "foo.doc"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(null, "foo"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(null, "foo."));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(null, "foo.bar"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(null, "foo.bar.baz"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter("", "foo.txt"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter("", "foo.doc"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter("", "foo"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter("", "foo."));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter("", "foo.bar"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter("", "foo.bar.baz"));

			var filterAll = "All files|*.*";
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterAll, "foo.txt"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterAll, "foo.doc"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterAll, "foo"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterAll, "foo."));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterAll, "foo.bar"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterAll, "foo.bar.baz"));

			var filterTxtOnly = "Text files|*.txt";
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtOnly, "foo.txt"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtOnly, "foo.doc"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtOnly, "foo"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtOnly, "foo."));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtOnly, "foo.bar"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtOnly, "foo.txt.baz"));

			var filterTxtAndDoc = "Text files|*.txt|Word files|*.doc";
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDoc, "foo.txt"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDoc, "foo.doc"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDoc, "foo"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDoc, "foo."));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDoc, "foo.bar"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDoc, "foo.txt.baz"));

			var filterTxtAndDocWithAll = "Text files|*.txt|Word files|*.doc|All files|*.*";
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDocWithAll, "foo.txt"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDocWithAll, "foo.doc"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDocWithAll, "foo"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDocWithAll, "foo."));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDocWithAll, "foo.bar"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterTxtAndDocWithAll, "foo.txt.baz"));

			var filterCodeSourceFiles = "Image files|*.png;*.jpg;*.jpeg;*.tiff";
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCodeSourceFiles, "foo.png"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCodeSourceFiles, "foo.jpg"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCodeSourceFiles, "foo.jpeg"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCodeSourceFiles, "foo.tiff"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCodeSourceFiles, "foo"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCodeSourceFiles, "foo."));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCodeSourceFiles, "foo.bar"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCodeSourceFiles, "foo.png.baz"));

			var filterCsCppSourceFiles = "Bitmap files|*.bmp;*.png;*.tiff|JPEG files|*.jpg;*.jpeg";
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.bmp"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.png"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.tiff"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.jpg"));
			Assert.IsTrue(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.jpeg"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.h"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.cs"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo."));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.bar"));
			Assert.IsFalse(AcquireImageControl.DoubleCheckFileFilter(filterCsCppSourceFiles, "foo.png.baz"));
		}
	}
}
