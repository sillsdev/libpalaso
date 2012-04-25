using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CustomControls;
using Palaso.UI.WindowsForms.FileDialogExtender;

namespace Palaso.UI.WindowsForms.ImageToolbox
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

			#if MONO
			_scannerButton.Enabled =  _cameraButton.Enabled = false;
			#endif

			_galleryControl.ImageChanged += new EventHandler(_galleryControl_ImageChanged);
		}

		void _galleryControl_ImageChanged(object sender, EventArgs e)
		{
			//propogate that event to the outer toolbox
			if (ImageChanged != null)
				ImageChanged(sender, e);
		}

		private void OnGetFromFileSystemClick(object sender, EventArgs e)
		{
			SetMode(Modes.SingleImage);
#if MONO
						using (var dlg = new OpenFileDialog())
			{
				if (string.IsNullOrEmpty(ImageToolboxSettings.Default.LastImageFolder))
				{
					dlg.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				}
				else
				{
					dlg.InitialDirectory = ImageToolboxSettings.Default.LastImageFolder;
				}

				dlg.Filter = "picture files|*.png;*.tif;*.tiff;*.jpg;*.jpeg;*.bmp;";
				dlg.Multiselect = false;
				dlg.AutoUpgradeEnabled = true;
				if (DialogResult.OK == dlg.ShowDialog())
				{
					_currentImage = PalasoImage.FromFile(dlg.FileName);
					_pictureBox.Image = _currentImage.Image;
					ImageToolboxSettings.Default.LastImageFolder = Path.GetDirectoryName(dlg.FileName);
					ImageToolboxSettings.Default.Save();
					if (ImageChanged != null)
						ImageChanged.Invoke(this, null);
				}
			}
#else
			//The primary thing this ExtendedFileDialog buys us is that with the standard one, there's
			//no way to preserve, let alone pre-set, what "view" the user gets. With the standard dialog,
			//We had complaints that a user had to change the view to show icons *each time* they used this.
			using (var dlg = new ExtendedFileDialog())
			{
				dlg.FileDlgDefaultViewMode = FolderViewMode.Thumbnails;
				if (string.IsNullOrEmpty(ImageToolboxSettings.Default.LastImageFolder))
				{
					dlg.FileDlgInitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				}
				else
				{
					dlg.FileDlgInitialDirectory = ImageToolboxSettings.Default.LastImageFolder;
				}

				//NB: dissallowed because of a .net crash:  http://jira.palaso.org/issues/browse/BL-85
				dlg.FileDlgFilter = "picture files(*.png;*.tif;*.tiff;*.jpg;*.jpeg;*.bmp)|*.png;*.tif;*.tiff;*.jpg;*.jpeg;*.bmp;";

				if (DialogResult.OK == dlg.ShowDialog())
				{
					_currentImage = PalasoImage.FromFile(dlg.FileDlgFileName);
					_pictureBox.Image = _currentImage.Image;
					ImageToolboxSettings.Default.LastImageFolder = Path.GetDirectoryName(dlg.FileDlgFileName);
					ImageToolboxSettings.Default.Save();
					if (ImageChanged != null)
						ImageChanged.Invoke(this, null);
				}
			}
#endif
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

					this.BeginInvoke(new Action<string>(OpenFileFromDrag), s);

					this.ParentForm.Activate();        // in the case Explorer overlaps this form
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
			if (image == null)
				_pictureBox.Image = null;
			else
				_pictureBox.Image = image.Image;

			SetMode(Modes.SingleImage);
		}

		public PalasoImage GetImage()
		{
			if(_galleryControl.Visible)
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
			SetMode(Modes.SingleImage);
			GetFromDevice(ImageAcquisitionService.DeviceKind.Scanner);
			#endif
		}
		private void OnCameraClick(object sender, EventArgs e)
		{
			#if !MONO
			SetMode(Modes.SingleImage);
			SetImage(null);
			_cameraButton.Checked = true;
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

				var file = acquisitionService.Acquire();
				if (file == null)
					return;
				var temp = Path.GetTempFileName();
				File.Delete(temp);
				file.SaveFile(temp);
				temp = ConvertToPngOrJpegIfNotAlready(temp);
				_pictureBox.Load(temp);
				File.Delete(temp);
				if (ImageChanged != null)
					ImageChanged.Invoke(this, null);
			}
			catch (ImageDeviceNotFoundException error)
			{
				_messageLabel.Text = error.Message + Environment.NewLine +Environment.NewLine+
									 "Note: this program works with devices that have a 'WIA' driver, not the old-style 'TWAIN' driver";
				_messageLabel.Visible = true;
			}
			catch (Exception error)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(error, "Problem Getting Image");
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

		private enum Modes {Gallery, SingleImage}
		private void SetMode(Modes mode)
		{
			_messageLabel.Visible = false;
			switch (mode)
			{
				case Modes.Gallery:
					_pictureBox.Visible = false;
					_galleryControl.Visible= true;
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
			if(_galleryControl.HaveImageCollectionOnThisComputer)
			{
				SetMode(Modes.Gallery);
				_focusTimer.Enabled = true;
			}
			else
			{
				SetMode(Modes.SingleImage);
			}

		}

		private void _focusTimer_Tick(object sender, EventArgs e)
		{
			_focusTimer.Enabled = false;
			_galleryControl.Focus();
		}

		private void AcquireImageControl_DragEnter(object sender, DragEventArgs e)
		{
			try
			{

				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					var a = (Array) e.Data.GetData(DataFormats.FileDrop);

					if (a != null)
					{
						var path = a.GetValue(0).ToString();
						if ((new List<string>(new[] {".tif", ".png", ".bmp", ".jpg", ".jpeg"})).Contains(Path.GetExtension(path).ToLower()))
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
	}
}
