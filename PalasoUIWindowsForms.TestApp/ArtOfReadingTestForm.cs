using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Palaso.UI.WindowsForms.ImageGallery;

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
			images.LoadIndex(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ImageGallery/artofreadingindexv3_en.txt"));
			images.RootImagePath = RootImagePath.Text;
			var form = new PictureChooser(images, "duck");
			form.ShowDialog();
			Result.Text = "Result: " + form.ChosenPath;
		}

		private void OnLoad(object sender, EventArgs e)
		{
			RootImagePath.Text = @"C:\ProgramData\SIL\Art Of Reading\images";
		}
	}
}
