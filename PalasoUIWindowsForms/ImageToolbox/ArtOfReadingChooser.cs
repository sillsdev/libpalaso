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
		private IImageCollection _imageCollection;
		private PalasoImage _previousImage;
		public bool InSomeoneElesesDesignMode;

		public ArtOfReadingChooser()
		{
			InitializeComponent();
			_thumbnailViewer.CaptionMethod = ((s) => string.Empty);//don't show a caption
		}

		public void Dispose()
		{
			_thumbnailViewer.Closing(); //this guy was working away in the background
		}

		/// <summary>
		/// use if the calling app already has some notion of what the user might be looking for (e.g. the definition in a dictionary program)
		/// </summary>
		/// <param name="searchTerm"></param>
		public void SetIntialSearchTerm(string searchTerm)
		{
			_searchTermsBox.Text = searchTerm;
		}

		void _thumbnailViewer_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(ImageChanged!=null && _thumbnailViewer.SelectedItems.Count>0)
			{
				ImageChanged.Invoke(this, null);
			}
		}

		private void _thumbnailViewer_DoubleClick(object sender, EventArgs e)
		{
			if (ImageChangedAndAccepted != null && _thumbnailViewer.SelectedItems.Count > 0)
			{
				ImageChangedAndAccepted.Invoke(this, null);
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
		/// <summary>
		/// happens when you double click an item
		/// </summary>
		public event EventHandler ImageChangedAndAccepted;

		private void _searchTermsBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode  ==Keys.Enter)
			{
				e.SuppressKeyPress = true;
				_searchButton_Click(sender, null);
			}
		}

		private void ArtOfReadingChooser_Load(object sender, EventArgs e)
		{
			if (InSomeoneElesesDesignMode)
				return;

			_imageCollection = ArtOfReadingImageCollection.FromStandardLocations();
			if (_imageCollection == null)
			{
				label1.Visible = _searchTermsBox.Visible = _searchButton.Visible = _thumbnailViewer.Visible = false;
				_messageLabel.Visible = true;
				_messageLabel.Font = new Font(SystemFonts.DialogFont.FontFamily, 10);
				_messageLabel.Text = @"This computer doesn't appear to have the 'International Illustrations: the Art Of Reading' gallery installed yet.";
				betterLinkLabel1.Visible = true;
			}
			else
			{
#if DEBUG
				//  _searchTermsBox.Text = @"flower";
#endif
				_thumbnailViewer.SelectedIndexChanged += new EventHandler(_thumbnailViewer_SelectedIndexChanged);
			}

#if DEBUG
			if (!HaveImageCollectionOnThisComputer)
				return;
			//when just testing, I just want to see some choices.
		   // _searchTermsBox.Text = @"flower";
			_searchButton_Click(this,null);
#endif

			_messageLabel.BackColor = Color.White;
			_searchTermsBox.Focus();
		}


	}
}
