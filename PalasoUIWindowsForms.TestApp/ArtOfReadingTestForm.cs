using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Palaso.UI.WindowsForms.ImageGallery;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using PalasoUIWindowsForms.TestApp.Properties;
using Palaso.PlatformUtilities;

namespace PalasoUIWindowsForms.TestApp
{
	public partial class ArtOfReadingTestForm : Form
	{
		public ArtOfReadingTestForm()
		{
			InitializeComponent();
		}

		private void OnPictureChooserClicked(object sender, EventArgs e)
		{
			ThumbnailViewer.UseWebViewer = _useGeckoVersion.Checked;
			var images = new ArtOfReadingImageCollection();
			images.LoadIndex(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ImageGallery/ArtOfReadingIndexV3_en.txt"));
			images.RootImagePath = RootImagePath.Text;
			using (var form = new PictureChooser(images, "duck"))
			{
				form.ShowDialog();
				Result.Text = "Result: " + form.ChosenPath;
			}
		}

		private void OnLoad(object sender, EventArgs e)
		{
			if (Platform.IsWindows)
				RootImagePath.Text = @"C:\ProgramData\SIL\Art Of Reading\images";
			else
				RootImagePath.Text = "/usr/share/ArtOfReading/images";
		}
	}
}
