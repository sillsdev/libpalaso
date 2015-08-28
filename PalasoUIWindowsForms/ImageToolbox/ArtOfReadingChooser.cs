using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
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
			_thumbnailViewer.LoadComplete += ThumbnailViewerOnLoadComplete;
			_searchResultStats.Text = "";
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				// For Linux, we can install the package if requested.
				this.betterLinkLabel1.Text = "Install the Art Of Reading package (this may be very slow)";
				this.betterLinkLabel1.URL = null;
				this.betterLinkLabel1.LinkClicked += InstallLinkClicked;
			}
			SearchLanguage = "en";	// until/unless the owner specifies otherwise explicitly
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

		/// <summary>
		/// Gets or sets the language used in searching for an image by words.
		/// </summary>
		public string SearchLanguage { internal get; set; }

		void _thumbnailViewer_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(ImageChanged!=null && _thumbnailViewer.HasSelection)
			{
				ImageChanged.Invoke(this, null);
			}
		}

		private void _thumbnailViewer_DoubleClick(object sender, EventArgs e)
		{
			if (ImageChangedAndAccepted != null && _thumbnailViewer.HasSelection)
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
					bool foundExactMatches;
					IEnumerable<object> results = _imageCollection.GetMatchingPictures(_searchTermsBox.Text, out foundExactMatches);
					if (results.Count() == 0)
					{
						_messageLabel.Visible = true;
						_searchResultStats.Text = "Found no matching images".Localize("ImageToolbox.NoMatchingImages");
					}
					else
					{
						_messageLabel.Visible = false;
						_thumbnailViewer.LoadItems(_imageCollection.GetPathsFromResults(results, true));
						_searchResultStats.Text = string.Format("Found {0} images", results.Count());
						if (!foundExactMatches)
							_searchResultStats.Text += string.Format(" with names close to {0}.", _searchTermsBox.Text);
					}
				}
				else
				{

				}

			}
			catch (Exception error)
			{

			}
			_searchButton.Enabled = true;
			//_okButton.Enabled = false;
		}

		private void ThumbnailViewerOnLoadComplete(object sender, EventArgs eventArgs)
		{
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
			else
			{
				_searchResultStats.Text = "";
			}
		}

		private new bool DesignMode
		{
			get
			{
				return (base.DesignMode || GetService(typeof(IDesignerHost)) != null) ||
					(LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}
		}

		private void ArtOfReadingChooser_Load(object sender, EventArgs e)
		{
			if (DesignMode)
				return;

			_imageCollection = ArtOfReadingImageCollection.FromStandardLocations(SearchLanguage);
			if (_imageCollection == null)
			{
				//label1.Visible = _searchTermsBox.Visible = _searchButton.Visible = _thumbnailViewer.Visible = false;
				_messageLabel.Visible = true;
				_messageLabel.Font = new Font(SystemFonts.DialogFont.FontFamily, 10);
				_messageLabel.Text = @"This computer doesn't appear to have the 'International Illustrations: the Art Of Reading' gallery installed yet.";
				// Adjust size to avoid text truncation
				_messageLabel.Size = new Size(400,100);
				betterLinkLabel1.Visible = true;
			}
			else
			{
#if DEBUG
				//  _searchTermsBox.Text = @"flower";
#endif
				if (string.IsNullOrEmpty(_searchTermsBox.Text))
				{
					_messageLabel.Visible = true;
					_messageLabel.Font = new Font(SystemFonts.DialogFont.FontFamily, 10);
					_messageLabel.Text = "This is the 'Art Of Reading' gallery. In the box above, type what you are searching for, then press ENTER. You can type words in English and Indonesian.".Localize("ImageToolbox.EnterSearchTerms");
					// Adjust size to avoid text truncation
					_messageLabel.Height = 100;
				}
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
		}

		/// <summary>
		/// To actually focus on the search box, the Mono runtime library appears to
		/// first need us to focus the search button, wait a bit, and then focus the
		/// search box.  Bizarre, unfortunate, but true.  (One of those bugs that we
		/// couldn't write code to do if we tried!)
		/// See https://jira.sil.org/browse/BL-964.
		/// </summary>
		internal void FocusSearchBox()
		{
			_searchButton.GotFocus += _searchButtonGotSetupFocus;
			_searchButton.Select();
		}

		private System.Windows.Forms.Timer _focusTimer1;

		private void _searchButtonGotSetupFocus(object sender, EventArgs e)
		{
			_searchButton.GotFocus -= _searchButtonGotSetupFocus;
			_focusTimer1 = new System.Windows.Forms.Timer(this.components);
			_focusTimer1.Tick += new System.EventHandler(this._focusTimer1_Tick);
			_focusTimer1.Interval = 100;
			_focusTimer1.Enabled = true;
		}

		private void _focusTimer1_Tick(object sender, EventArgs e)
		{
			_focusTimer1.Enabled = false;
			_focusTimer1.Dispose();
			_focusTimer1 = null;
			_searchTermsBox.TextBox.Focus();
		}

		/// <summary>
		/// Try to install the artofreading package if possible.  Use a GUI program if
		/// possible, but if not, try the command-line program with a GUI password
		/// dialog.
		/// </summary>
		/// <remarks>
		/// On Windows, the link label opens a web page to let the user download the
		/// installer.  This is the analogous behavior for Linux, but is potentially
		/// so slow (300MB download) that we fire off the program without waiting for
		/// it to finish.
		/// </remarks>
		private void InstallLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix)
				return;
			// Install the artofreading package if at all possible.
			if (File.Exists("/usr/bin/software-center"))
			{
				using (var process = new Process())
				{
					process.StartInfo = new ProcessStartInfo {
						FileName = "/usr/bin/python",
						Arguments = "/usr/bin/software-center art-of-reading",
						UseShellExecute = false,
						RedirectStandardOutput = false,
						CreateNoWindow = false
					};
					process.Start();
				}
			}
			else if (File.Exists("/usr/bin/ssh-askpass"))
			{
				using (var process = new Process())
				{
					process.StartInfo = new ProcessStartInfo {
						FileName = "/usr/bin/sudo",
						Arguments = "-A /usr/bin/apt-get -y install art-of-reading",
						UseShellExecute = false,
						RedirectStandardOutput = false,
						CreateNoWindow = false
					};
					process.StartInfo.EnvironmentVariables.Add("SUDO_ASKPASS", "/usr/bin/ssh-askpass");
					process.Start();
				}
			}
		}
	}
}
