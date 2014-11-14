using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.ClearShare;
using Palaso.UI.WindowsForms.ImageToolbox;

namespace PalasoUIWindowsForms.Tests.ImageToolbox
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
				dlg.ShowDialog();
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
