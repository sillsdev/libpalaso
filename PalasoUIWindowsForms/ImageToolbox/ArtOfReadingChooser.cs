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
		private readonly IImageCollection _imageCollection;
		private PalasoImage _previousImage;

		public ArtOfReadingChooser()
		{
			InitializeComponent();
			_imageCollection = ArtOfReadingImageCollection.FromStandardLocations();
			if (_imageCollection == null)
			{
				label1.Visible= _searchTermsBox.Visible = _searchButton.Visible = _thumbnailViewer.Visible = false;
				_messageLabel.Visible = true;
				_messageLabel.Text = "The Art Of Reading collection was not found on this computer.";
				_messageLabel.Text=@"This computer doesn't appear to have the 'International Illustrations: the Art Of Reading' gallery installed yet. If you can find a copy of it, you can freely copy the images to this comptuer, according to its license. But you must only copy the images (which SIL owns), not the software (which is commercial). To do this, follow these steps:
1) Create a folder named 'Art of Reading' at the root of your 'C:\' drive.
2) Copy the 'Images' folder from the Art of Reading DVD into that folder.

If you can't get a copy of Art Of Reading locally, you can purchase the DVD from www.ethnologue.com.";
			}
			else
			{
#if DEBUG
				_searchTermsBox.Text = @"flower";
#endif
				_thumbnailViewer.SelectedIndexChanged += new EventHandler(_thumbnailViewer_SelectedIndexChanged);
			}
		}


		void _thumbnailViewer_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(ImageChanged!=null && _thumbnailViewer.SelectedItems.Count>0)
			{
				ImageChanged.Invoke(this, null);
			}
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
					IEnumerable<object> results = _imageCollection.GetMatchingPictures(_searchTermsBox.Text);
					if (results.Count() == 0)
					{
						_messageLabel.Visible = true;
					}
					else
					{
						_messageLabel.Visible = false;
						_thumbnailViewer.LoadItems(_imageCollection.GetPathsFromResults(results, true));
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
			_thumbnailViewer.CaptionMethod = _imageCollection.CaptionMethod;

			if (_searchTermsBox.Text.Length > 0)
				_searchButton_Click(this, null);
		}
		public string ChosenPath { get { return _thumbnailViewer.SelectedPath; } }

		public bool HaveImageCollectionOnThisComputer
		{
			get { return _imageCollection != null; }
		}


		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			_thumbnailViewer.Closing();
		}

		public void SetImage(PalasoImage image)
		{
			_previousImage = image;
			if(ImageChanged!=null)
				ImageChanged.Invoke(this,null);
		}

		public PalasoImage GetImage()
		{
			if(ChosenPath!=null &&  File.Exists(ChosenPath))
			{
				try
				{
					return PalasoImage.FromFile(ChosenPath);
				}
				catch (Exception error)
				{
					Palaso.Reporting.ErrorReport.ReportNonFatalExceptionWithMessage(error, "There was a problem choosing that image.");
					return _previousImage;
				}
			}
			return _previousImage;
		}

		public event EventHandler ImageChanged;

		private void _searchTermsBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode  ==Keys.Enter)
			{
				_searchButton_Click(sender, null);
			}
		}

		private void ArtOfReadingChooser_Load(object sender, EventArgs e)
		{
#if DEBUG
			if (!HaveImageCollectionOnThisComputer)
				return;
			//when just testing, I just want to see some choices.
			_searchTermsBox.Text = @"flower";
			_searchButton_Click(this,null);
#endif

		}
	}
}
