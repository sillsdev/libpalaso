using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.WindowsForms.ClearShare;
using SIL.WindowsForms.ImageGallery;
using SIL.WindowsForms.ImageToolbox;

namespace SIL.WindowsForms.Tests.ImageToolbox
{
	class ImageToolboxTests
	{
		[Test, Ignore("by hand only")]
		[STAThread]
		public void ShowToolbox()
		{
			Application.EnableVisualStyles();
			using (var dlg = new ImageToolboxDialog(new PalasoImage(),null))// "arrow"))
			{
				if (DialogResult.OK == dlg.ShowDialog())
				{
					// File name ending in .tmp will confuse TagLib#...doesn't know what kind of metadata to write.
					string path  = Path.ChangeExtension(Path.GetTempFileName(), ".png");
					dlg.ImageInfo.Save(path);
					Process.Start("explorer.exe", "/select, \"" + path + "\"");
				}
			}
		}

		[Test, Ignore("by hand only")]
		[STAThread]
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

		[Test, Ignore("by hand only")]
		[STAThread]
		public void ShowToolboxWith_PreExisting_Image_WithMetadata()
		{
			Application.EnableVisualStyles();
			PalasoImage i = PalasoImage.FromImage(LicenseLogos.by_nd);
			i.Metadata.License = new CreativeCommonsLicense(true,true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
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

		[Test, Ignore("by hand only")]
		[STAThread]
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
	}
}
