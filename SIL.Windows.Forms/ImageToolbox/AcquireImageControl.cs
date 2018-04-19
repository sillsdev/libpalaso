using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using SIL.PlatformUtilities;
using SIL.Reporting;
#if !MONO
using System.Drawing.Imaging;
using SIL.IO;
using WIA;
#endif

namespace SIL.Windows.Forms.ImageToolbox
{
	/// <summary>
	/// Provides 4 ways to get an image: Gallery (art of reading), scanner, camera, or file system.
	/// </summary>
	public partial class AcquireImageControl : UserControl, IImageToolboxControl
	{
		private PalasoImage _previousImage;
		private PalasoImage _currentImage;

		public event EventHandler ImageChanged;

		public AcquireImageControl()
		{
			InitializeComponent();

			if (!Platform.IsWindows)
			{
				_scannerButton.Enabled = _cameraButton.Enabled = false;
				// Mono layout doesn't always handle Anchor point properly.  Fix it.
				FixMyLayoutForMono();
			}

			_galleryControl.ImageChanged += _galleryControl_ImageChanged;
		}

		void _galleryControl_ImageChanged(object sender, EventArgs e)
		{
			//propogate that event to the outer toolbox
			if (ImageChanged != null)
				ImageChanged(sender, e);
		}

		private bool _actModal;

		private void OnGetFromFileSystemClick(object sender, EventArgs e)
		{
			if (_actModal)
				return;		// The GTK file chooser on Linux isn't acting modal, so we simulate it.
			_actModal = true;
			// Something in the Mono runtime state machine keeps the GTK filechooser from getting the
			// focus immediately when we invoke OpenFileDialogWithViews.ShowDialog() directly at this
			// point.  Waiting for the next idle gets it into a state where the filechooser does receive
			// the focus as desired.  See https://silbloom.myjetbrains.com/youtrack/issue/BL-5809.
			if (Platform.IsMono)
				Application.Idle += DelayGetImageFileFromSystem;
			else
				GetImageFileFromSystem();
		}

		private void DelayGetImageFileFromSystem(object sender, EventArgs e)
		{
			Application.Idle -= DelayGetImageFileFromSystem;
			GetImageFileFromSystem();
		}

		private void GetImageFileFromSystem()
		{
			try
			{
				SetMode(Modes.SingleImage);
				// The primary thing that OpenFileDialogWithViews buys us is the ability to default to large icons.
				// OpenFileDialogWithViews still doesn't let us read (and thus remember) the selected view.
				using (var dlg = new OpenFileDialogWithViews(OpenFileDialogWithViews.DialogViewTypes.Large_Icons))
				{
					if (string.IsNullOrEmpty(ImageToolboxSettings.Default.LastImageFolder))
					{
						dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
					}
					else
					{
						dlg.InitialDirectory = ImageToolboxSettings.Default.LastImageFolder; ;
					}

					dlg.Title = "Choose Picture File".Localize("ImageToolbox.FileChooserTitle", "Title shown for a file chooser dialog brought up by the ImageToolbox");

					//NB: dissallowed gif because of a .net crash:  http://jira.palaso.org/issues/browse/BL-85
					dlg.Filter = "picture files".Localize("ImageToolbox.PictureFiles", "Shown in the file-picking dialog to describe what kind of files the dialog is filtering for") + "(*.png;*.tif;*.tiff;*.jpg;*.jpeg;*.bmp)|*.png;*.tif;*.tiff;*.jpg;*.jpeg;*.bmp;";

					if (DialogResult.OK == dlg.ShowDialog(this.ParentForm))
					{
						ImageToolboxSettings.Default.LastImageFolder = Path.GetDirectoryName(dlg.FileName);
						ImageToolboxSettings.Default.Save();

						try
						{
							_currentImage = PalasoImage.FromFile(dlg.FileName);
						}
						catch (Exception err) //for example, http://jira.palaso.org/issues/browse/BL-199
						{
							ErrorReport.NotifyUserOfProblem(err, "Sorry, there was a problem loading that image.".Localize("ImageToolbox.ProblemLoadingImage"));
							return;
						}
						_pictureBox.Image = _currentImage.Image;
						if (ImageChanged != null)
							ImageChanged.Invoke(this, null);
					}
				}
			}
			finally
			{
				_actModal = false;
			}
		}

		private void OpenFileFromDrag(string path)
		{
			SetMode(Modes.SingleImage);
			_currentImage = PalasoImage.FromFile(path);
			_pictureBox.Image = _currentImage.Image;
			ImageToolboxSettings.Default.LastImageFolder = Path.GetDirectoryName(path);
			ImageToolboxSettings.Default.Save();
			if (ImageChanged != null)
				ImageChanged.Invoke(this, null);
		}

		private void OnDragDrop(object sender, DragEventArgs e)
		{
			try
			{
				var a = (Array)e.Data.GetData(DataFormats.FileDrop);

				if (a != null)
				{
					// Extract string from first array element
					// (ignore all files except first if number of files are dropped).
					string s = a.GetValue(0).ToString();

					// Call OpenFile asynchronously.
					// Explorer instance from which file is dropped is not responding
					// all the time when DragDrop handler is active, so we need to return
					// immidiately (especially if OpenFile shows MessageBox).

					BeginInvoke(new Action<string>(OpenFileFromDrag), s);

					ParentForm.Activate();        // in the case Explorer overlaps this form
				}
			}
			catch (Exception)
			{
#if DEBUG
				throw;
#endif
			}
		}

		public void SetImage(PalasoImage image)
		{
			_previousImage = image;
			_scannerButton.Checked = _cameraButton.Checked = false;
			_currentImage = image;
			SetMode(Modes.SingleImage);
			if (image == null)
			{
				_pictureBox.Image = null;
			}
			else
			{
				_pictureBox.Image = image.Image;
			}
		}

		public PalasoImage GetImage()
		{
			if (_galleryControl.Visible)
			{
				return _galleryControl.GetImage();
			}
			if (_currentImage != null)
			//            if (_pictureBox.Image != null)
			{
				//return new PalasoImage() { Image = _pictureBox.Image, FileName = _fileName };
				return _currentImage;
			}
			return _previousImage;
		}


		private void OnScannerClick(object sender, EventArgs e)
		{
#if !MONO
			_scannerButton.Checked = true;
			SetImage(null);
			UsageReporter.SendNavigationNotice("ImageToolbox:GetFromScanner");
			GetFromDevice(ImageAcquisitionService.DeviceKind.Scanner);
#endif
		}

		private void OnCameraClick(object sender, EventArgs e)
		{
#if !MONO
			SetImage(null);
			_cameraButton.Checked = true;
			UsageReporter.SendNavigationNotice("ImageToolbox:GetFromCamera");
			GetFromDevice(ImageAcquisitionService.DeviceKind.Camera);
#endif
		}

#if !MONO
		private void GetFromDevice(ImageAcquisitionService.DeviceKind deviceKind)
		{
			//_pictureBox.Image = SampleImages.sampleScan;
			try
			{
				var acquisitionService = new ImageAcquisitionService(deviceKind);

				var wiaImageFile = acquisitionService.Acquire();
				if (wiaImageFile == null)
					return;

				var imageFile = ConvertToPngOrJpegIfNotAlready(wiaImageFile);
				_currentImage = PalasoImage.FromFile(imageFile);
				_pictureBox.Image = _currentImage.Image;

				if (ImageChanged != null)
					ImageChanged.Invoke(this, null);
			}
			catch (ImageDeviceNotFoundException error)
			{
				_messageLabel.Text = error.Message + Environment.NewLine + Environment.NewLine +
									 "Note: this program works with devices that have a 'WIA' driver, not the old-style 'TWAIN' driver";
				_messageLabel.Visible = true;
			}
			catch (WIA_Version2_MissingException)
			{
				_messageLabel.Text = "Windows XP does not come with a crucial DLL that lets you use a WIA scanner with this program. Get a technical person to downloand and follow the directions at http://vbnet.mvps.org/files/updates/wiaautsdk.zip";
				_messageLabel.Visible = true;
			}
			catch (Exception error)
			{
				ErrorReport.NotifyUserOfProblem(error, "Problem Getting Image".Localize("ImageToolbox.ProblemGettingImageFromDevice"));
			}
		}
#endif

		/// <summary>
		/// use if the calling app already has some notion of what the user might be looking for (e.g. the definition in a dictionary program)
		/// </summary>
		/// <param name="searchTerm"></param>
		public void SetIntialSearchString(string searchTerm)
		{
			_galleryControl.SetIntialSearchTerm(searchTerm);
		}

		/// <summary>
		/// Gets or sets the language used in searching for an image by words.
		/// </summary>
		public string SearchLanguage
		{
			get { return _galleryControl.SearchLanguage; }
			set { _galleryControl.SearchLanguage = value; }
		}

		/*
		/// <summary>
		/// Bitmaps --> PNG, JPEGs stay as jpegs.
		/// Will delete the incoming file if it needs to do a conversion.
		/// </summary>
		private string ConvertToPngOrJpegIfNotAlready(string incoming)
		{
			string outgoing = incoming;
			//important to dispose of these things, they lock down the file.
			using (var image = Image.FromFile(incoming))
			{
				if (!ImageFormat.Png.Equals(image.PixelFormat) && !ImageFormat.Jpeg.Equals(image.PixelFormat))
				{
					using (var stream = new MemoryStream())
					{
						incoming.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
						var oldCropped = cropped;
						cropped = System.Drawing.Image.FromStream(stream) as Bitmap;
						oldCropped.Dispose();
						Require.That(ImageFormat.Jpeg.Guid == cropped.RawFormat.Guid, "lost jpeg formatting");
					}

					outgoing = Path.GetTempFileName();
					image.Save(outgoing, ImageFormat.Png);
				}
			}
			if (outgoing != incoming)
			{
				try
				{
					File.Delete(incoming);
				}
				catch (Exception e)
				{
					Debug.Fail(e.Message);
					//in release, just keep going
				}
			}
			return outgoing;
		}
		*/

#if !MONO
		private string ConvertToPngOrJpegIfNotAlready(ImageFile wiaImageFile)
		{
			Image acquiredImage;//with my scanner, always a .bmp

			var imageBytes = (byte[])wiaImageFile.FileData.get_BinaryData();
			if (wiaImageFile.FileExtension == ".jpg" || wiaImageFile.FileExtension == ".png")
			{
				var temp = TempFile.WithExtension(wiaImageFile.FileExtension);
				wiaImageFile.SaveFile(temp.Path);
				return temp.Path;
			}

			using (var stream = new MemoryStream(imageBytes))
			{
				//Ok, so we want to know if we should save a jpeg (photo) or a png (line art).
				//Here, we just chose whichever makes for a the smaller file.
				//Test results
				//                  PNG             JPG
				//B&W Drawing       110k            813k
				//Newspaper photo   1.3M            97k
				//Little doodle     1.7k            11k

				//NB: we do not want to dispose of these, because we will be passing back the path of one
				//(and will manually delete the other)
				TempFile jpeg = TempFile.WithExtension(".jpg");
				TempFile png = TempFile.WithExtension(".png");

				//DOCS: "You must keep the stream open for the lifetime of the Image."(maybe just true for jpegs)
				using (acquiredImage = Image.FromStream(stream))
				{
					acquiredImage.Save(jpeg.Path, ImageFormat.Jpeg);
					acquiredImage.Save(png.Path, ImageFormat.Png);
				}

				//now return the smaller of the two;
				if (new FileInfo(jpeg.Path).Length > new FileInfo(png.Path).Length)
				{
					File.Delete(jpeg.Path);
					return png.Path;
				}
				else
				{
					File.Delete(png.Path);
					return jpeg.Path;
				}
			}
		}
#endif

		private enum Modes { Gallery, SingleImage }
		private void SetMode(Modes mode)
		{
			_messageLabel.Visible = false;
			switch (mode)
			{
				case Modes.Gallery:
					_pictureBox.Visible = false;
					_pictureBox.Image = null; //prevent error upon resetting visibility to true
					_galleryControl.Visible = true;
					//_galleryButton.Select();
					_galleryButton.Checked = true;
					_galleryControl.Focus();
					break;
				case Modes.SingleImage:
					_galleryButton.Checked = false;
					_pictureBox.Visible = true;
					_galleryControl.Visible = false;
					break;
				default:
					throw new ArgumentOutOfRangeException("mode");
			}
		}

		private void OnGalleryClick(object sender, EventArgs e)
		{
			SetMode(Modes.Gallery);
			_galleryControl.Focus();
		}

		private void AcquireImageControl_Load(object sender, EventArgs e)
		{
			if (_galleryControl.HaveImageCollectionOnThisComputer)
			{
				SetMode(Modes.Gallery);
				_focusTimer.Interval = 100;
				_focusTimer.Enabled = true;
			}
			else
			{
				SetMode(Modes.SingleImage);
			}

		}

		/// <summary>
		/// Set the focus on the search box in the ArtOfReadingChooser after a delay
		/// to let things settle down.  It's unfortunate, but the Mono runtime
		/// library appears to need this.  (See https://jira.sil.org/browse/BL-964.)
		/// </summary>
		private void _focusTimer_Tick(object sender, EventArgs e)
		{
			_focusTimer.Enabled = false;
			_focusTimer.Dispose();
			_focusTimer = null;
			_galleryControl.FocusSearchBox();
		}

		private void AcquireImageControl_DragEnter(object sender, DragEventArgs e)
		{
			try
			{

				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					var a = (Array)e.Data.GetData(DataFormats.FileDrop);

					if (a != null)
					{
						var path = a.GetValue(0).ToString();
						if ((new List<string>(new[] { ".tif", ".png", ".bmp", ".jpg", ".jpeg" })).Contains(Path.GetExtension(path).ToLower()))
						{
							e.Effect = DragDropEffects.Copy;
							return;
						}
					}

				}
			}
			catch (Exception)
			{
			}
			e.Effect = DragDropEffects.None;
		}

		/// <summary>
		/// _galleryControl is supposed to fill us with just some minor margins to the left and right.
		/// Mono changes our size before it calculates the margin to maintain with those anchors being
		/// set left and right.  Being a timing/sequencing issue, this is hard to fix in Mono, so for
		/// now we'll patch it with a workaround here.
		/// </summary>
		/// <remarks>
		/// I know this is a hack, but my first attempt to patch Mono had no effect, and I'm not sure
		/// this bug is worth spending much more time on.  (https://jira.sil.org/browse/BL-778 "[Linux]
		/// Image Toolbox only displays 2 columns of AoR images (Windows does 3)")
		/// </remarks>
		protected void FixMyLayoutForMono()
		{
			// The numbers used here come from comparing the Size assignments of the various
			// controls in the .Designer.cs file, taking into account also the Location
			// assignments of the child controls.  These exact values are gone by the time
			// we get here, so the differences (the margins for anchor right and bottom) are
			// also gone.  This is exactly the bug in the Mono code:  the anchor margins are
			// calculated at the wrong point when sizes have already changed.  (Note that the
			// Location gives the margins for anchor left and top.)
			if (_galleryControl != null && Contains(_galleryControl) &&
				(Width - _galleryControl.Width > 6 || Height - _galleryControl.Height > 60))
			{
				var oldSize = _galleryControl.Size;
				_galleryControl.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				_galleryControl.Size = new Size(Width - 6, Height - 60);
				_galleryControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
										 AnchorStyles.Left | AnchorStyles.Right;
			}
			if (_messageLabel != null && Contains(_messageLabel) && Width - _messageLabel.Width != 223)
			{
				_messageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				_messageLabel.Size = new Size(Width - 223, _messageLabel.Height);
				_messageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left |
									   AnchorStyles.Right;
			}
			if (_pictureBox != null && Contains(_pictureBox) &&
				(Width != _pictureBox.Width || Height - _pictureBox.Height != 60))
			{
				_pictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				_pictureBox.Size = new Size(Width, Height - 60);
				_pictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
									 AnchorStyles.Left | AnchorStyles.Right;
			}
		}
	}
}
