using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ImageToolbox;
using SIL.Windows.Forms.ImageToolbox.ImageGallery;

namespace SIL.Windows.Forms.Tests.ImageToolbox
{
	class ImageToolboxTests
	{
		[Test]
		[Explicit("By hand only")]
		[STAThread]
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

		[Test]
		[Explicit("By hand only")]
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

		[Test]
		[Explicit("By hand only")]
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

		[Test]
		[Platform(Exclude="Win", Reason="applies only to Mono")]
		public void CheckMonoForSelectLargeIconView()
		{
			using (var dlg = new OpenFileDialog ())
			{
				Assert.IsTrue(dlg is FileDialog, "OpenFileDialog no longer inherits from FileDialog");
				Type dlgType = typeof(FileDialog);
				var dlgViewField = dlgType.GetField("mwfFileView", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				Assert.IsNotNull(dlgViewField, "FileDialog no longer has an mwfFileView field");
				var fileView = dlgViewField.GetValue(dlg);
				Assert.IsNotNull(fileView, "FileDialog has not yet set the mwfFileView value");
				var viewType = fileView.GetType();
				var viewItemField = viewType.GetField("largeIconMenutItem", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				Assert.IsNotNull(viewItemField, "MWFFileView no longer has a largeIconMenutItem field");
				var largeIcon = viewItemField.GetValue(fileView) as MenuItem;
				Assert.IsNotNull(largeIcon, "MWFFileView has not yet set the largeIconMenutItem value");
				var viewOnClickMethod = viewType.GetMethod("OnClickViewMenuSubItem", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
					null, new Type[] { typeof(Object), typeof(EventArgs) }, null);
				Assert.IsNotNull(viewOnClickMethod, "MWFFileView no longer has an OnClickViewMenuSubItem(object, EventArgs) method");
			}
		}
	}
}
