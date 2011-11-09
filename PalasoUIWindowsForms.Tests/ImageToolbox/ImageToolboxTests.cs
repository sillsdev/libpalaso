using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.IO;
using Palaso.UI.WindowsForms.ClearShare;
using Palaso.UI.WindowsForms.ClearShare.WinFormsUI;
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
			using (var dlg = new ImageToolboxDialog(new PalasoImage()))
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
			//using (var f = TempFile.WithExtension(".png"))
			{
				//i.Save(f.Path);
				using (var dlg = new ImageToolboxDialog(i))
				{
					dlg.ShowDialog();
				}
			}
		}
	}
}
