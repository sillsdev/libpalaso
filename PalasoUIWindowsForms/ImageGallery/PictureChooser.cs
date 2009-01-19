using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.ImageGallery;

namespace WeSay.LexicalTools.AddPictures
{
	public partial class PictureChooser : Form
	{
		private readonly IImageCollection _images;

		public PictureChooser(IImageCollection images)
		{
			_images = images;
			InitializeComponent();
		}

		private void _searchButton_Click(object sender, EventArgs e)
		{

			if (!string.IsNullOrEmpty(_searchTermsBox.Text))
			{
				IList<object> results = _images.GetMatchingPictures(_searchTermsBox.Text);

				_thumbnailViewer.LoadItems(_images.GetPathsFromResults(results, true));
			}
		}

	}

}
