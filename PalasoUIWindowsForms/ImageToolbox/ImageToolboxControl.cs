using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using Palaso.Code;
using Palaso.UI.WindowsForms.ClearShare;
using Palaso.UI.WindowsForms.ClearShare.WinFormsUI;
using Palaso.UI.WindowsForms.ImageToolbox.Cropping;

#if !MONO

#endif

namespace Palaso.UI.WindowsForms.ImageToolbox
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

		public ImageToolboxControl()
		{
			InitializeComponent();
			_toolImages = new ImageList();
			ImageInfo = new PalasoImage();
			_copyExemplarMetadata.Font = _editMetadataLink.Font;
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
					if (value == null || value.Image == null)
					{
						_currentImageBox.Image = null;
						_metadataDisplayControl.Visible = false;
						_invitationToMetadataPanel.Visible = false;
					}
					else
					{
						if(value.Image == _currentImageBox.Image)
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
					if(_imageInfo!=null && _imageInfo!=value)
					{
						_imageInfo.Dispose();
					}

					_imageInfo = value;
					GC.Collect();//having trouble reliably tracking down a PalasoImage which is not being disposed of.

				}
				catch (Exception e)
				{
					Palaso.Reporting.ErrorReport.NotifyUserOfProblem(e, "Sorry, something went wrong while getting the image.".Localize("ImageToolbox.GenericGettingImageProblem"));
				}
			}
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

		private void SetupMetaDataControls(Metadata metaData)
		{
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
				var s = LocalizationManager.GetString("Use {0}", "ImageToolbox.CopyExemplarMetadata", "Used to copy a previous metadata set to the current image. The  {0} will be replaced with the name of the exemplar image.");
				_copyExemplarMetadata.Text = string.Format(s, Metadata.GetStoredExemplarSummaryString(Metadata.FileCategory.Image));
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

		bool _inIndexChanging	;

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{

			if (_inIndexChanging)
				return;

			try
			{
				_inIndexChanging = true;

				if (_toolListView.SelectedItems.Count == 0)
					return;

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
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(error,
																 "Sorry, something went wrong with the ImageToolbox".Localize("ImageToolbox.GenericProblem"));
			}
			finally
			{
				_inIndexChanging = false;
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

		private void OnEditMetadataLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			//http://jira.palaso.org/issues/browse/BL-282 hada null in here somewhere
			Guard.AgainstNull(_imageInfo, "_imageInfo");
			Guard.AgainstNull(_imageInfo.Metadata, "_imageInfo.Metadata");

			//it's not clear at the moment where the following belongs... but we want
			//to encourage Creative Commons Licensing, so if there is no license, we'll start
			//the following dialog out with a reasonable default.
			_imageInfo.Metadata.SetupReasonableLicenseDefaultBeforeEditing();

			using(var dlg = new MetadataEditorDialog(_imageInfo.Metadata))
			{
				if(DialogResult.OK == dlg.ShowDialog())
				{
					Guard.AgainstNull(dlg.Metadata, " dlg.Metadata");
					_imageInfo.Metadata = dlg.Metadata;
					SetupMetaDataControls(_imageInfo.Metadata);
					_imageInfo.SaveUpdatedMetadataIfItMakesSense();
					_imageInfo.Metadata.StoreAsExemplar(Metadata.FileCategory.Image);
				}
			}
		}

		private void OnLoad(object sender, EventArgs e)
		{
			_toolListView.Items.Clear();


			//doing our own image list because VS2010 croaks their resx if have an imagelist while set to .net 3.5 with x86 on a 64bit os (something like that). This is a known bug MS doesn't plan to fix.

			_toolListView.LargeImageList = _toolImages;
			_toolImages.ColorDepth = ColorDepth.Depth24Bit;
			_toolImages.ImageSize = new Size(32, 32);

			_editLink.Visible = false;

			AddControl("Get Picture".Localize("ImageToolbox.GetPicture"), ImageToolboxButtons.browse, "browse", (x) =>
			{
				var c = new AcquireImageControl();
				c.SetIntialSearchString(InitialSearchString);
				return c;
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
			_imageInfo.SaveUpdatedMetadataIfItMakesSense();
		}
	}

	public interface IImageToolboxControl
	{
		void SetImage(PalasoImage image);
		PalasoImage GetImage();
		event EventHandler ImageChanged;
	}
}
