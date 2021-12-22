using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using SIL.Code;
using SIL.PlatformUtilities;
using SIL.Reporting;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;
using SIL.Windows.Forms.ImageToolbox.Cropping;
using SIL.Windows.Forms.Miscellaneous;

namespace SIL.Windows.Forms.ImageToolbox
{
	/// <summary>
	/// The ImageToolbox lets you acquire images from camera, scanner, image collection, or the file system.
	/// It has a cropping control.
	/// It can read metadata, and allow you to enter metadata for an image.
	/// </summary>
	public partial class ImageToolboxControl : UserControl
	{
		private readonly ImageList _toolImages;
		private Control _currentControl;
		private PalasoImage _imageInfo;
		private ListViewItem _cropToolListItem;
		private string _incomingSearchLanguage;
		private AcquireImageControl _acquireImageControl;

		public ImageToolboxControl()
		{
			InitializeComponent();
			_toolImages = new ImageList();
			ImageInfo = new PalasoImage();
			_copyExemplarMetadata.Font = _editMetadataLink.Font;
			SearchLanguage = "en";	// unless/until the owner specifies otherwise explicitly
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_toolImages.Dispose();
				if (_imageInfo!=null)
				{
					_imageInfo.Dispose();
					_imageInfo = null;
				}
				if (components != null)
				{
					components.Dispose();
					components = null;
				}
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// This is the main input/output of this dialog
		/// </summary>
		public PalasoImage ImageInfo
		{
			get { return _imageInfo; }
			set
			{
				try
				{
					var oldValue = _imageInfo;
					// We've been having trouble with an obscure situation where one of the edit metadata links is
					// clicked while _imageInfo is null. Wondering whether an exception might somehow be caught
					// in such a way that SetupMetaDataControls is called here based on value being non-null,
					// but _imageInfo never actually gets set to the new value. Seems a setter should always
					// set the backing variable to the new value, so we now do this first.
					_imageInfo = value;

					if (value == null || value.Image == null)
					{
						_currentImageBox.Image = null;
						_metadataDisplayControl.Visible = false;
						// These two controls when clicked run code that requires _imageInfo to be non-null.
						// Make sure they are hidden if it is set to null.
						_invitationToMetadataPanel.Visible = false;
						_editLink.Visible = false;
					}
					else
					{
						if (value.Image == _currentImageBox.Image)
						{
							return;
						}
						/* this seemed like a good idea, but it lead to "parameter errors" later in the image
						 * try
												{
													if (_currentImageBox.Image != null)
													{
														  _currentImageBox.Image.Dispose();
													}
												}
												catch (Exception)
												{
													//ah well. I haven't got a way to know when it's disposable and when it isn't
													throw;
												}
						  */
						_currentImageBox.Image = value.Image;

						SetCurrentImageToolTip(value);
						SetupMetaDataControls(value.Metadata);
					}
					if (oldValue != null && oldValue != value)
					{
						oldValue.Dispose();
					}

					GC.Collect(); //having trouble reliably tracking down a PalasoImage which is not being disposed of.
				}
				catch (Exception e)
				{
					ErrorReport.NotifyUserOfProblem(e, "Sorry, something went wrong while getting the image.".Localize("ImageToolbox.GenericGettingImageProblem"));
				}
			}
		}

		/// <summary>
		/// Gets or sets the language used in searching for an image by words.
		/// </summary>
		public string SearchLanguage
		{
			//the acquireImageControl is added at some point during use; if we have it, we want to tell
			//the client what search language the user has actually chosen. But if we don't have that
			//control yet, just return whatever value they set us to.
			get { return _acquireImageControl != null ? _acquireImageControl.SearchLanguage : _incomingSearchLanguage; }

			// We store this until we get an acquireImageControl, then pass it along to it when it is created.
			set { _incomingSearchLanguage = value; }
		}

		private void SetCurrentImageToolTip(PalasoImage image)
		{
			_toolTip.SetToolTip(_currentImageBox, "");

			//enchance: this only uses the "originalpath" version, which may be a lot larger than what we
			//currently have, if we cropped, for example. But I'm loath to save it to disk just to get an accurate size.
			if (image!=null && !string.IsNullOrEmpty(image.OriginalFilePath) && File.Exists(image.OriginalFilePath))
			{
				try
				{
					float size = new System.IO.FileInfo(image.OriginalFilePath).Length;
					if (size > 1000*1024)
						_toolTip.SetToolTip(_currentImageBox,
											string.Format("{0} {1:N2}M", image.OriginalFilePath, size/(1024f*1000f)));
					else
					{
						_toolTip.SetToolTip(_currentImageBox, string.Format("{0} {1:N2}K", image.OriginalFilePath, size/1024f));
					}
				}
				catch (Exception error)
				{
					_toolTip.SetToolTip(_currentImageBox, error.Message);
				}
			}
		}

		/// <summary>
		/// used by galleries (e.g. art of reading)
		/// </summary>
		public string InitialSearchString { get; set; }

		/// <summary>
		/// Used to report problems loading images. See more detail on AcquireImageControl
		/// </summary>
		public Action<string, Exception, string> ImageLoadingExceptionReporter
		{
			get { return _imageLoadingExceptionReporter; }
			set
			{
				_imageLoadingExceptionReporter = value;
				if (_acquireImageControl != null)
					_acquireImageControl.ImageLoadingExceptionReporter = value;
			}
		}

		private void SetupMetaDataControls(Metadata metaData)
		{
			Guard.AgainstNull(_imageInfo, "_imageInfo");
			if (_currentImageBox.Image == null)
			{
				// Otherwise, the metadata controls are visible upon first load (with no image).
				// Clicking them causes crashes.
				_invitationToMetadataPanel.Visible = false;
				_metadataDisplayControl.Visible = false;
				return;
			}

			//NB: there was a bug here where the display control refused to go to visible, if this was called before loading. Weird.  So now, we have an OnLoad() to call it again.
			_invitationToMetadataPanel.Visible = (metaData == null || metaData.IsEmpty);
			_metadataDisplayControl.Visible = !_invitationToMetadataPanel.Visible;
			bool looksOfficial = !string.IsNullOrEmpty(metaData.CollectionUri);
			_editLink.Visible = metaData!=null && _metadataDisplayControl.Visible && !looksOfficial;
			if (_metadataDisplayControl.Visible)
				_metadataDisplayControl.SetMetadata(metaData);

			_copyExemplarMetadata.Visible = Metadata.HaveStoredExemplar(Metadata.FileCategory.Image);
			if (_invitationToMetadataPanel.Visible && _copyExemplarMetadata.Visible)
			{
				var s = LocalizationManager.GetString("ImageToolbox.CopyExemplarMetadata", "Use {0}", "Used to copy a previous metadata set to the current image. The  {0} will be replaced with the name of the exemplar image.");
				_copyExemplarMetadata.Text = string.Format(s, Metadata.GetStoredExemplarSummaryString(Metadata.FileCategory.Image));
			}

			if (Platform.IsWindows)
				return;

			// Ensure that the metadata gets fully displayed if at all possible.
			// See https://silbloom.myjetbrains.com/youtrack/issue/BL-2354 for what can happen otherwise.
			// The need for this appears to be a bug in the Mono library that allows _metadataDisplayControl
			// to grow taller as its internal content exceeds its initial external size.  (Any difference
			// from Windows/.Net behavior is by definition a bug.)
			if (_metadataDisplayControl.Visible)
			{
				var heightClipped = (_metadataDisplayControl.Location.Y + _metadataDisplayControl.Height) - panel1.Height;
				if (_editLink.Visible)
					heightClipped = (_metadataDisplayControl.Location.Y + _metadataDisplayControl.Height) - _editLink.Location.Y;
				if (heightClipped != 0)
				{
					var yNew = _metadataDisplayControl.Location.Y - heightClipped;
					var loc = new Point(_metadataDisplayControl.Location.X, yNew < 0 ? 0 : yNew);
					_metadataDisplayControl.Location = loc;
				}
			}
		}

		private ListViewItem AddControl(string label, Bitmap bitmap, string imageKey, System.Func<PalasoImage, Control> makeControl)
		{
			_toolImages.Images.Add(bitmap);
			_toolImages.Images.SetKeyName(_toolImages.Images.Count - 1, imageKey);

			var item= new ListViewItem(label);
			item.ImageKey = imageKey;

			item.Tag = makeControl;
			this._toolListView.Items.Add(item);
			return item;
		}

		// Changed this to remember the selected index because something is causing the SelectedIndexChanged
		// event to fire twice each time an icon is clicked.
		int _previousSelectedIndex = -1;
		private Action<string, Exception, string> _imageLoadingExceptionReporter;

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (_toolListView.SelectedItems.Count == 0)
				{
					_previousSelectedIndex = -1;
					return;
				}

				if (_previousSelectedIndex == _toolListView.SelectedIndices[0])
					return;

				_previousSelectedIndex = _toolListView.SelectedIndices[0];

				ListViewItem selectedItem = _toolListView.SelectedItems[0];

				if (selectedItem.Tag == _currentControl)
					return;

				bool haveImage = !(ImageInfo == null || ImageInfo.Image == null);

				//don't let them select crop (don't have a cheap way of 'disabling' a list item, sigh)

				if (!haveImage && selectedItem == _cropToolListItem)
				{
					_cropToolListItem.Selected = false;
					_toolListView.Items[0].Selected = true;
					return;
				}

				if (_currentControl != null)
				{
					GetImageFromCurrentControl();

					_panelForControls.Controls.Remove(_currentControl);
					((IImageToolboxControl) _currentControl).ImageChanged -= new EventHandler(imageToolboxControl_ImageChanged);
					_currentControl.Dispose();
				}
				System.Func<PalasoImage, Control> fun =
					(System.Func<PalasoImage, Control>) selectedItem.Tag;
				_currentControl = fun(ImageInfo);

				_currentControl.Dock = DockStyle.Fill;
				_panelForControls.Controls.Add(_currentControl);

				IImageToolboxControl imageToolboxControl = ((IImageToolboxControl) _currentControl);
				if (ImageInfo!=null && ImageInfo.Image != null)
				{
					imageToolboxControl.SetImage(ImageInfo);
				}
				imageToolboxControl.ImageChanged += new EventHandler(imageToolboxControl_ImageChanged);
				Refresh();
			}
			catch (Exception error)
			{
				ErrorReport.NotifyUserOfProblem(error, "Sorry, something went wrong with the ImageToolbox".Localize("ImageToolbox.GenericProblem"));
			}
		}

		private void GetImageFromCurrentControl()
		{
			ImageInfo = ((IImageToolboxControl) _currentControl).GetImage();
			if (ImageInfo == null)
				_currentImageBox.Image = null;
			else
				_currentImageBox.Image = ImageInfo.Image;
		}

		void imageToolboxControl_ImageChanged(object sender, EventArgs e)
		{
			GetImageFromCurrentControl();
		}

		public void Closing()
		{
			if (_currentControl == null)
			{

			}
			else
			{
				ImageInfo = ((IImageToolboxControl)_currentControl).GetImage();
				Controls.Remove(_currentControl);
				_currentControl.Dispose();
			}

		}

		/// <summary>
		/// If set, this action will be used instead of the default (launching <see cref="MetadataEditorDialog"/>).
		/// For example, the client may want to use a different UI to edit the `Metadata`.
		/// The `Action<Metadata>` callback saves the modified `Metadata` to the image.
		/// <see cref="SetNewImageMetadata(Metadata)"/>
		/// </summary>
		public Action<Metadata, Action<Metadata>> EditMetadataActionOverride { get; set; }

		private void OnEditMetadataLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			//http://jira.palaso.org/issues/browse/BL-282 hada null in here somewhere
			Guard.AgainstNull(_imageInfo, "_imageInfo");
			Guard.AgainstNull(_imageInfo.Metadata, "_imageInfo.Metadata");

			//it's not clear at the moment where the following belongs... but we want
			//to encourage Creative Commons Licensing, so if there is no license, we'll start
			//the following dialog out with a reasonable default.
			_imageInfo.Metadata.SetupReasonableLicenseDefaultBeforeEditing();

			if (EditMetadataActionOverride != null)
			{
				EditMetadataActionOverride(_imageInfo.Metadata, SetNewImageMetadata);
				return;
			}

			using(var dlg = new MetadataEditorDialog(_imageInfo.Metadata))
			{
				if(DialogResult.OK == dlg.ShowDialog())
				{
					Guard.AgainstNull(dlg.Metadata, " dlg.Metadata");
					SetNewImageMetadata(dlg.Metadata);
				}
			}
		}

		private void SetNewImageMetadata(Metadata newMetadata)
		{
			_imageInfo.Metadata = newMetadata;
			SetupMetaDataControls(_imageInfo.Metadata);
			//Not doing this anymore, too risky. See https://jira.sil.org/browse/BL-1001 _imageInfo.SaveUpdatedMetadataIfItMakesSense();
			_imageInfo.Metadata.StoreAsExemplar(Metadata.FileCategory.Image);
		}

		private void OnLoad(object sender, EventArgs e)
		{
			_toolListView.Items.Clear();


			//doing our own image list because VS2010 croaks their resx if have an imagelist while set to .net 3.5 with x86 on a 64bit os (something like that). This is a known bug MS doesn't plan to fix.

			_toolListView.LargeImageList = _toolImages;
			_toolImages.ColorDepth = ColorDepth.Depth24Bit;
			_toolImages.ImageSize = new Size(32, 32);

			// These two controls should never be visible except when made so by SetupMetaDataControls when ImageInfo is not null.
			// To help ensure this we make both not visible right at the start.
			_editLink.Visible = false;
			_invitationToMetadataPanel.Visible = false;

			AddControl("Get Picture".Localize("ImageToolbox.GetPicture"), ImageToolboxButtons.browse, "browse", (x) =>
			{
				_acquireImageControl = new AcquireImageControl();
				_acquireImageControl.ImageLoadingExceptionReporter =
					ImageLoadingExceptionReporter;
				_acquireImageControl.SetIntialSearchString(InitialSearchString);
				_acquireImageControl.SearchLanguage = _incomingSearchLanguage;
				return _acquireImageControl;
			});
			_cropToolListItem = AddControl("Crop".Localize("ImageToolbox.Crop"), ImageToolboxButtons.crop, "crop", (x) => new ImageCropper());

			_toolListView.Items[0].Selected = true;
			_toolListView.Refresh();

			if (ImageInfo == null)
				return;

			SetupMetaDataControls(ImageInfo.Metadata);
			this._toolListView.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);

		}


		private void OnCopyExemplar_MouseClick(object sender, MouseEventArgs e)
		{
			_imageInfo.Metadata.LoadFromStoredExemplar(Metadata.FileCategory.Image);
			SetupMetaDataControls(ImageInfo.Metadata);
			//Not doing this anymore, too risky. See https://jira.sil.org/browse/BL-1001  _imageInfo.SaveUpdatedMetadataIfItMakesSense();
		}
	}

	public interface IImageToolboxControl
	{
		void SetImage(PalasoImage image);
		PalasoImage GetImage();
		event EventHandler ImageChanged;
	}
}
