using System;
using System.Windows.Forms;
using SIL.Core.ClearShare;
using SIL.Reporting;
using SIL.Windows.Forms.ClearShare;

namespace SIL.Windows.Forms.ImageToolbox
{
	public partial class ImageToolboxDialog : Form
	{
		/// <param name="imageInfo">optional (can be null)</param>
		/// <param name="initialSearchString">optional</param>
		public ImageToolboxDialog(PalasoImage imageInfo, string initialSearchString) : this(imageInfo, initialSearchString, null) { }

		/// <param name="imageInfo">optional (can be null)</param>
		/// <param name="initialSearchString">optional</param>
		/// <param name="editMetadataActionOverride">If non-null, this action will be used
		/// instead of the default (launching <see cref="ClearShare.WinFormsUI.MetadataEditorDialog"/>).
		/// For example, the client may want to use a different UI to edit the `Metadata`.
		/// The `Action<Metadata>` callback saves the modified `Metadata` to the image.
		/// <see cref="ImageToolboxControl.SetNewImageMetadata(Metadata)"/></param>
		public ImageToolboxDialog(PalasoImage imageInfo, string initialSearchString, Action<Metadata, Action<Metadata>> editMetadataActionOverride)
		{
			InitializeComponent();
			_imageToolboxControl.ImageInfo = imageInfo;
			_imageToolboxControl.InitialSearchString = initialSearchString;
			_imageToolboxControl.EditMetadataActionOverride = editMetadataActionOverride;
			SearchLanguage = "en";	// unless the caller specifies otherwise explicitly
		}

		public PalasoImage ImageInfo { get { return _imageToolboxControl.ImageInfo; } }
		/// <summary>
		/// Used to report problems loading images. See more detail on AcquireImageControl
		/// </summary>
		public Action<string, Exception, string> ImageLoadingExceptionReporter
		{
			get { return _imageToolboxControl.ImageLoadingExceptionReporter; }
			set
			{
				_imageToolboxControl.ImageLoadingExceptionReporter = value;
			}
		}

		/// <summary>
		/// Sets the language used in searching for an image by words.
		/// </summary>
		public string SearchLanguage
		{
			get { return _imageToolboxControl.SearchLanguage; }
			set { _imageToolboxControl.SearchLanguage = value; }
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			//enhance: doesn't tell us all that much.
			UsageReporter.SendNavigationNotice("ImageToolboxDialog/Ok");
			DialogResult = (ImageInfo==null || ImageInfo.Image==null)? DialogResult.Cancel : DialogResult.OK;
			_imageToolboxControl.Closing();
			Close();
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			_imageToolboxControl.Closing();
			Close();
		}

		private void ImageToolboxDialog_Load(object sender, EventArgs e)
		{
			UsageReporter.SendNavigationNotice("ImageToolbox");
		}
	}
}
