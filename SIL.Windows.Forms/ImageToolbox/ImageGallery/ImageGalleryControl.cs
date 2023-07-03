using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using SIL.Reporting;
using SIL.Windows.Forms.Extensions;

namespace SIL.Windows.Forms.ImageToolbox.ImageGallery
{
	public partial class ImageGalleryControl : UserControl, IImageToolboxControl
	{
		private ImageCollectionManager _imageCollectionManager;
		private PalasoImage _previousImage;

		public ImageGalleryControl()
		{
			InitializeComponent();
			_thumbnailViewer.CaptionMethod = ((s) => string.Empty);//don't show a caption
			_thumbnailViewer.LoadComplete += ThumbnailViewerOnLoadComplete;
			_searchResultStats.Text = "";
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				// For Linux, we can install the package if requested.
				_downloadInstallerLink.Text = "Install the Art Of Reading package (this may be very slow)".Localize("ImageToolbox.InstallArtOfReading");
				_downloadInstallerLink.URL = null;
				_downloadInstallerLink.LinkClicked += InstallLinkClicked;
			}
			else
			{
				// Ensure that we can get localized text here.
				_downloadInstallerLink.Text = "Download Art Of Reading Installer".Localize("ImageToolbox.DownloadArtOfReading");
			}
			_labelSearch.Text = "Image Galleries".Localize("ImageToolbox.ImageGalleries");
			SearchLanguage = "en";	// until/unless the owner specifies otherwise explicitly
			// Get rid of any trace of a border on the toolstrip.
			toolStrip1.Renderer = new NoBorderToolStripRenderer();

			// For some reason, setting these BackColor values in InitializeComponent() doesn't work.
			// The BackColor gets set back to the standard control background color somewhere...
			_downloadInstallerLink.BackColor = Color.White;
			_messageLabel.BackColor = Color.White;
			_messageLabel.SizeChanged += MessageLabelSizeChanged;
		}

		/// <summary>
		/// use if the calling app already has some notion of what the user might be looking for (e.g. the definition in a dictionary program)
		/// </summary>
		/// <param name="searchTerm"></param>
		public void SetInitialSearchTerm(string searchTerm)
		{
			_searchTermsBox.Text = searchTerm;
		}

		/// <summary>
		/// use if the calling app already has some notion of what the user might be looking for (e.g. the definition in a dictionary program)
		/// </summary>
		/// <param name="searchTerm"></param>
		[Obsolete("Use SetInitialSearchTerm (spelling corrected)")]
		public void SetIntialSearchTerm(string searchTerm)
		{
			SetInitialSearchTerm(searchTerm);
		}

		/// <summary>
		/// Gets or sets the language used in searching for an image by words.
		/// </summary>
		public string SearchLanguage { get; set; }

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
				if (!string.IsNullOrWhiteSpace(_searchTermsBox.Text))
				{
					bool foundExactMatches;
					// (avoid enumerating the returned IEnumerable<object> more than once by copying to a List.)
					var results = _imageCollectionManager.GetMatchingImages(_searchTermsBox.Text, true, out foundExactMatches).ToList();
					if (results.Any())
					{
						_messageLabel.Visible = false;
						_downloadInstallerLink.Visible = false;
						_thumbnailViewer.LoadItems(results);
						var fmt = results.Count == 1 ?
							"Found 1 image".Localize("ImageToolbox.MatchingImageSingle") :
							"Found {0} images".Localize("ImageToolbox.MatchingImages", "The {0} will be replaced by the number of matching images");
						if (!foundExactMatches)
						{
							fmt = results.Count == 1 ?
								"Found 1 image with a name close to \u201C{1}\u201D".Localize("ImageToolbox.AlmostMatchingImageSingle",
									"The {1} will be replaced with the search string.") :
								"Found {0} images with names close to \u201C{1}\u201D".Localize(
								"ImageToolbox.AlmostMatchingImages",
								"The {0} will be replaced by the number of images found. The {1} will be replaced with the search string.");
						}

						_searchResultStats.Text = string.Format(fmt, results.Count, _searchTermsBox.Text);
						_searchResultStats.ForeColor = foundExactMatches ? Color.Black : Color.FromArgb(0x34, 0x65, 0xA4); //#3465A4
						_searchResultStats.Font = new Font("Segoe UI", 9F, foundExactMatches ? FontStyle.Regular : FontStyle.Bold);
					}
					else
					{
						_messageLabel.Visible = true;
						if (!_searchLanguageMenu.Visible)
							_downloadInstallerLink.Visible = true;
						_searchResultStats.Text = "Found no matching images".Localize("ImageToolbox.NoMatchingImages");
						_searchResultStats.ForeColor = Color.Black;
						_searchResultStats.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
					}
				}
			}
			catch (Exception)
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
			get { return _imageCollectionManager != null; }
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
					ErrorReport.ReportNonFatalExceptionWithMessage(error, "There was a problem choosing that image.");
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

			_imageCollectionManager = ImageCollectionManager.FromStandardLocations(SearchLanguage);
			_collectionToolStrip.Visible = false;
			if (_imageCollectionManager == null)
			{
				_messageLabel.Visible = true;
				_messageLabel.Text = "This computer doesn't appear to have any galleries installed yet.".Localize("ImageToolbox.NoGalleries");
				_downloadInstallerLink.Visible = true;
				_searchTermsBox.Enabled = false;
				_searchButton.Enabled = false;
			}
			else
			{
#if DEBUG
				//  _searchTermsBox.Text = @"flower";
#endif
				SetupSearchLanguageChoice();
				_messageLabel.Visible = string.IsNullOrEmpty(_searchTermsBox.Text);
				// Adjust size to avoid text truncation
				_messageLabel.Height = 200;
				SetMessageLabelText();
				_thumbnailViewer.SelectedIndexChanged += new EventHandler(_thumbnailViewer_SelectedIndexChanged);
				if (_imageCollectionManager.Collections.Count() > 1)
				{
					_collectionToolStrip.Visible = true;
					_collectionDropDown.Visible = true;
					_collectionDropDown.Text =
						"Galleries".Localize("ImageToolbox.Galleries");
					if (ImageToolboxSettings.Default.DisabledImageCollections == null)
					{
						ImageToolboxSettings.Default.DisabledImageCollections = new StringCollection();
					}

					foreach (var collection in _imageCollectionManager.Collections)
					{
					    if(ImageToolboxSettings.Default.DisabledImageCollections.Contains(collection.FolderPath))
					    {
					        collection.Enabled = false;
					    }
						var text = Path.GetFileNameWithoutExtension(collection.Name);
						var item = new ToolStripMenuItem(text);
						_collectionDropDown.DropDownItems.Add(item);
						item.CheckOnClick = true;
						item.CheckState = collection.Enabled ? CheckState.Checked : CheckState.Unchecked;
						item.CheckedChanged += (o, args) =>
						{
						    if(_collectionDropDown.DropDownItems.Cast<ToolStripMenuItem>().Count(x => x.Checked) == 0)
						        item.Checked = true; // tried to uncheck the last one, don't allow it.
						    else
						    {
						        collection.Enabled = item.Checked;
								var disabledSettings = ImageToolboxSettings.Default.DisabledImageCollections;
								if (disabledSettings == null)
									ImageToolboxSettings.Default.DisabledImageCollections = disabledSettings = new StringCollection();
								if (item.Checked && disabledSettings.Contains(collection.FolderPath))
									disabledSettings.Remove(collection.FolderPath);
								if (!item.Checked && !disabledSettings.Contains(collection.FolderPath))
									disabledSettings.Add(collection.FolderPath);
								ImageToolboxSettings.Default.Save();
							}
						};
					}
				}
				else
				{
					// otherwise, just leave them all enabled
				}
			}
			_messageLabel.Font = new Font(SystemFonts.DialogFont.FontFamily, 10);

#if DEBUG
			//if (!HaveImageCollectionOnThisComputer)
			//	return;
			//when just testing, I just want to see some choices.
			// _searchTermsBox.Text = @"flower";
			//_searchButton_Click(this,null);
#endif
		}

		private void SetMessageLabelText()
		{
			var msg = "In the box above, type what you are searching for, then press ENTER.".Localize("ImageToolbox.EnterSearchTerms");
			// Allow for the old index that contained English and Indonesian together.
			var searchLang = "English + Indonesian";
			// If we have the new multilingual index, _searchLanguageMenu will be visible.  Its tooltip
			// contains both the native name of the current search language + its English name in
			// parentheses if its in a nonRoman script or otherwise thought to be unguessable by a
			// literate speaker of an European language.  (The menu displays only the native name, and
			// SearchLanguage stores only the ISO code.)
			if (_searchLanguageMenu.Visible)
				searchLang = _searchLanguageMenu.ToolTipText;
			msg += Environment.NewLine + Environment.NewLine +
					String.Format("The search box is currently set to {0}".Localize("ImageToolbox.SearchLanguage"), searchLang);
			if (PlatformUtilities.Platform.IsWindows && !_searchLanguageMenu.Visible)
			{
				msg += Environment.NewLine + Environment.NewLine +
						"Did you know that there is a new version of this collection which lets you search in Arabic, Bengali, Chinese, English, French, Indonesian, Hindi, Portuguese, Spanish, Thai, or Swahili?  It is free and available for downloading."
							.Localize("ImageToolbox.NewMultilingual");
				_downloadInstallerLink.Visible = true;
				_downloadInstallerLink.BackColor = Color.White;
			}
			// Restore alignment (from center) for messages.  (See https://silbloom.myjetbrains.com/youtrack/issue/BL-2753.)
			_messageLabel.TextAlign = _messageLabel.RightToLeft==RightToLeft.Yes ? HorizontalAlignment.Right : HorizontalAlignment.Left;
			_messageLabel.Text = msg;
		}

		/// <summary>
		/// Position the download link label properly whenever the size of the main message label changes,
		/// whether due to changing its text or changing the overall dialog box size.  (BL-2853)
		/// </summary>
		private void MessageLabelSizeChanged(object sender, EventArgs eventArgs)
		{
			if (_searchLanguageMenu.Visible || !PlatformUtilities.Platform.IsWindows || !_downloadInstallerLink.Visible)
				return;
			_downloadInstallerLink.Width = _messageLabel.Width;		// not sure why this isn't automatic
			if (_downloadInstallerLink.Location.Y != _messageLabel.Bottom + 5)
				_downloadInstallerLink.Location = new Point(_downloadInstallerLink.Left, _messageLabel.Bottom + 5);
		}

		protected class LanguageChoice
		{
			static readonly List<string> idsOfRecognizableLanguages = new List<string> { "en", "fr", "es", "it", "tpi", "pt", "id" };
			private readonly CultureInfo _info;

			public LanguageChoice(CultureInfo ci)
			{
				_info = ci;
			}

			public string Id { get { return _info.Name == "zh-Hans" ? "zh" : _info.Name; } }

			public string NativeName
			{
				get
				{
					if (_info.Name == "id" && _info.NativeName == "Indonesia")
						return "Bahasa Indonesia";	// This is a known problem in Windows/.Net.
					return _info.NativeName;
				}
			}

			public override string ToString()
			{
				if (_info.NativeName == _info.EnglishName)
					return NativeName;	// English (English) looks rather silly...
				if (idsOfRecognizableLanguages.Contains(Id))
					return NativeName;
				return String.Format("{0} ({1})", _info.NativeName, _info.EnglishName);
			}
		}

		private void SetupSearchLanguageChoice()
		{
			var indexLangs = _imageCollectionManager.IndexLanguageIds;
			if (indexLangs == null)
			{
				_searchLanguageMenu.Visible = false;
			}
			else
			{
				_searchLanguageMenu.Visible = true;
				foreach (var id in indexLangs)
				{
					var ci = id == "zh" ? new CultureInfo("zh-Hans") : new CultureInfo(id);
					var choice = new LanguageChoice(ci);
					var item = _searchLanguageMenu.DropDownItems.Add(choice.ToString());
					item.Tag = choice;
					item.Click += SearchLanguageClick;
					if (id == SearchLanguage)
					{
						_searchLanguageMenu.Text = choice.NativeName;
						_searchLanguageMenu.ToolTipText = choice.ToString();
					}
				}
			}
			// The Mono renderer makes the toolstrip stick out.  (This is a Mono bug that
			// may not be worth spending time on.)  Let's not poke the user in the eye
			// with an empty toolstrip.
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				toolStrip1.Visible = _searchLanguageMenu.Visible;
		}

		void SearchLanguageClick(object sender, EventArgs e)
		{
			var item = sender as ToolStripItem;
			if (item != null)
			{
				var lang = item.Tag as LanguageChoice;
				if (lang != null && SearchLanguage != lang.Id)
				{
					_searchLanguageMenu.Text = lang.NativeName;
					_searchLanguageMenu.ToolTipText = lang.ToString();
					SearchLanguage = lang.Id;
					_imageCollectionManager.ChangeSearchLanguageAndReloadIndex(lang.Id);
					SetMessageLabelText();		// Update with new language name.
				}
			}
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
