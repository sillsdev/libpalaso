using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.ImageGallery;

namespace WeSay.LexicalTools.AddPictures
{
	public partial class PictureChooser : Form
	{
		private readonly IImageCollection _images;

		public PictureChooser(IImageCollection images, string searchWords)
		{
			_images = images;
			InitializeComponent();
			_searchTermsBox.Text = searchWords;
		}

		private void _searchButton_Click(object sender, EventArgs e)
		{

			if (!string.IsNullOrEmpty(_searchTermsBox.Text))
			{
				IList<object> results = _images.GetMatchingPictures(_searchTermsBox.Text);

				_thumbnailViewer.LoadItems(_images.GetPathsFromResults(results, true));
			}
		}

		private void PictureChooser_Load(object sender, EventArgs e)
		{
			_thumbnailViewer.CaptionMethod = _images.CaptionMethod;

			if (_searchTermsBox.Text.Length > 0)
				_searchButton_Click(this, null);
		}
		public string ChosenPath { get; set; }
		private void _thumbnailViewer_DoubleClick(object sender, EventArgs e)
		{
			ChosenPath = _thumbnailViewer.SelectedPath;
			if (string.IsNullOrEmpty(ChosenPath))
				return;
			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
		   ChosenPath = _thumbnailViewer.SelectedPath;
			if (string.IsNullOrEmpty(ChosenPath))
			{
				this.DialogResult = DialogResult.Cancel;
			}
			else
			{
				this.DialogResult = DialogResult.OK;
			}
			Close();
		}

	}

}
