using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.ImageGallery;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class ArtOfReadingChooser : UserControl, IImageToolboxControl
	{
				private readonly IImageCollection _images;
		private PalasoImage _previousImage;

		public ArtOfReadingChooser(string searchWords)
		{
			_images =  ArtOfReadingImageCollection.FromStandardLocations();
			InitializeComponent();
			_searchTermsBox.Text = searchWords;
		}

		private void _searchButton_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			_searchButton.Enabled = false;
			try
			{
				_thumbnailViewer.Clear();
				if (!string.IsNullOrEmpty(_searchTermsBox.Text))
				{
					IEnumerable<object> results = _images.GetMatchingPictures(_searchTermsBox.Text);
					if (results.Count() == 0)
					{
						_notFoundLabel.Visible = true;
					}
					else
					{
						_notFoundLabel.Visible = false;
						_thumbnailViewer.LoadItems(_images.GetPathsFromResults(results, true));
					}
				}

			}
			catch (Exception error)
			{

			}
			_searchButton.Enabled = true;
			//_okButton.Enabled = false;
			Cursor.Current = Cursors.Default;
		}

		private void PictureChooser_Load(object sender, EventArgs e)
		{
			_thumbnailViewer.CaptionMethod = _images.CaptionMethod;

			if (_searchTermsBox.Text.Length > 0)
				_searchButton_Click(this, null);
		}
		public string ChosenPath { get { return _thumbnailViewer.SelectedPath; } }


		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			_thumbnailViewer.Closing();
		}

		public void SetImage(PalasoImage image)
		{
			_previousImage = image;
		}

		public PalasoImage GetImage()
		{
			if(ChosenPath!=null &&  File.Exists(ChosenPath))
			{
				try
				{
					var pi = new PalasoImage();
					pi.Image = Image.FromFile(ChosenPath);
					return pi;
				}
				catch (Exception error)
				{
					Palaso.Reporting.ErrorReport.ReportNonFatalExceptionWithMessage(error, "There was a problem choosing that image.");
					return _previousImage;
				}
			}
			return _previousImage;
		}
	}
}
