using System;
using System.Drawing;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.ImageGallery;
using Palaso.UI.WindowsForms.ImageToolbox.Cropping;
using Palaso.UI.WindowsForms.ImageToolbox.Scanner;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class ImageToolboxControl : UserControl
	{
		private readonly ImageList _toolImages;
		private Control _currentControl;
		private PalasoImage _imageInfo;

		public ImageToolboxControl()
		{
			InitializeComponent();
			_panelForControls.Visible = false;

			ImageInfo = new PalasoImage();
			_toolListView.Groups.Clear();
			_toolListView.Items.Clear();

			var getImageGroup = AddGroup("Get Image");
			var editImageGroup = AddGroup("Edit Image");
			var otherGroup = AddGroup("Other");

			//doing our own image list because VS2010 croaks their resx if have an imagelist while set to .net 3.5 with x86 on a 64bit os (something like that). This is a known bug MS doesn't plan to fix.
			_toolImages = new ImageList();
			_toolListView.LargeImageList = _toolImages;
			_toolImages.ColorDepth = ColorDepth.Depth24Bit;
			_toolImages.ImageSize = new Size(32,32);


			AddControl("From File", ImageToolboxButtons.browse, "browse", getImageGroup, (x) => new GetImageFromFileSystemControl());
			AddControl("From Scan", ImageToolboxButtons.scanner, "scanner", getImageGroup, (x) => new DeviceAcquire(ImageAcquisitionService.DeviceKind.Scanner));
			AddControl("From Camera", ImageToolboxButtons.scanner, "camera", getImageGroup, (x) => new DeviceAcquire(ImageAcquisitionService.DeviceKind.Camera));
			AddControl("From Gallery", ImageToolboxButtons.searchFolder, "gallery", getImageGroup, (x) => new ArtOfReadingChooser(string.Empty));
			AddControl("Crop",  ImageToolboxButtons.crop, "crop", editImageGroup, (x) => new ImageCropper());
			AddControl("Credits", ImageToolboxButtons.credits, "credits", otherGroup, (x) => new ImageCreditsControl());
			AddControl("License", ImageToolboxButtons.Licenses, "license", otherGroup, (x) => new ImageLicenseControl());
			_toolListView.Refresh();
		}

		/// <summary>
		/// This is the main input/output of this dialog
		/// </summary>
		public PalasoImage ImageInfo
		{
			get{ return _imageInfo;}
			set
			{

				if (value == null || value.Image == null)
				{
					_currentImageBox.Image = null;
				}
				else
				{
					_currentImageBox.Image = value.Image;
				}
				_imageInfo = value;
			}
		}

		private ListViewGroup AddGroup(string header)
		{
			var g= new ListViewGroup(header,HorizontalAlignment.Left);
			_toolListView.Groups.Add(g);
			return g;
		}

		private void AddControl(string label, Bitmap bitmap, string imageKey, ListViewGroup group, System.Func<PalasoImage, Control> makeControl)
		{
			_toolImages.Images.Add(bitmap);
			_toolImages.Images.SetKeyName(_toolImages.Images.Count - 1, imageKey);

			var item= new ListViewItem(label, group);
			item.ImageKey = imageKey;
			item.Tag = makeControl;
			this._toolListView.Items.Add(item);
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_toolListView.SelectedItems.Count == 0)
				return;
			if(_currentControl !=null)
			{
				GetImageFromCurrentControl();

				Controls.Remove(_currentControl);
				((IImageToolboxControl)_currentControl).ImageChanged -= new EventHandler(imageToolboxControl_ImageChanged);
				_currentControl.Dispose();
			}
			System.Func<PalasoImage, Control> fun =
				(System.Func<PalasoImage, Control>) _toolListView.SelectedItems[0].Tag;
			_currentControl = fun(ImageInfo);
			_currentControl.Bounds = _panelForControls.Bounds;
			_currentControl.Anchor = _panelForControls.Anchor;

			Controls.Add(_currentControl);
			IImageToolboxControl imageToolboxControl = ((IImageToolboxControl)_currentControl);
			imageToolboxControl.SetImage(ImageInfo);
			imageToolboxControl.ImageChanged += new EventHandler(imageToolboxControl_ImageChanged);
			Refresh();
		}

		private void GetImageFromCurrentControl()
		{
			ImageInfo = ((IImageToolboxControl) _currentControl).GetImage();
			_currentImageBox.Image = ImageInfo.Image;
		}

		void imageToolboxControl_ImageChanged(object sender, EventArgs e)
		{
			GetImageFromCurrentControl();
		}

		public void Closing()
		{
			ImageInfo = ((IImageToolboxControl)_currentControl).GetImage();
			Controls.Remove(_currentControl);
			_currentControl.Dispose();
		}
	}

	public interface IImageToolboxControl
	{
		void SetImage(PalasoImage image);
		PalasoImage GetImage();
		event EventHandler ImageChanged;
	}
}
