using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.ClearShare;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class ImageMetadataControl : UserControl
	{
		private PalasoImage _image;

		public ImageMetadataControl()
		{
			InitializeComponent();
			UpdateDisplay();
		}

		public void SetImage(PalasoImage image)
		{
			SaveChanges();
			_image = image;
			UpdateDisplay();
		}

		private void SaveChanges()
		{
			if(_image!=null && _image.MetaData.HasChanges)
			{
				_image.MetaData.Write();
			}
		}


		public override bool ValidateChildren()
		{
			SaveChanges();
			return base.ValidateChildren();
		}
		private void UpdateDisplay()
		{
			if(_image == null)
			{
				_illustrator.ReadOnly = _copyright.ReadOnly == false;
				return;
			}

			this.Visible = _image.Image != null;

			//_lockedCheckbox.Checked = _image.MetaDataLocked;
			_illustrator.Text = _image.MetaData.Creator;
			_copyright.Text = _image.MetaData.CopyrightNotice;
			_illustrator.ReadOnly = _copyright.ReadOnly = !_image.MetaData.AllowEditingMetadata;
			_illustrator.BorderStyle = _copyright.BorderStyle = _image.MetaData.AllowEditingMetadata ? BorderStyle.FixedSingle: BorderStyle.None;

			if (_image.MetaData.AllowEditingMetadata)
			{
			}
			//only handle the first one, for now
			if (FirstLicense ==null)
			{
				_licenseImage.Image = null;
				//_licenseDescription.Visible = false;
			}
			else
			{
//                _licenseDescription.ReadOnly = FirstLicense.EditingAllowed;
//                _licenseDescription.Visible = true;
//                _licenseDescription.Text = FirstLicense.GetDescription("en");
				_licenseImage.Image = FirstLicense.GetImage();
			}
		}
		private LicenseInfo FirstLicense
		{
			get
			{
				return _image.MetaData.License;
			}
		}

		private void _illustrator_TextChanged(object sender, EventArgs e)
		{
			_image.MetaData.Creator = _illustrator.Text;
		}

		private void _copyright_TextChanged(object sender, EventArgs e)
		{
			_image.MetaData.CopyrightNotice = _copyright.Text;
		}

		private void _lockedCheckbox_CheckedChanged(object sender, EventArgs e)
		{
		   // _image.MetaDataLocked = _lockedCheckbox.Checked;
			UpdateDisplay();
		}

		private void _licenseDescription_TextChanged(object sender, EventArgs e)
		{
		   // FirstLicense.SetDescription("en",_licenseDescription.Text);
		}

		private void ImageMetadataControl_Validating(object sender, CancelEventArgs e)
		{
			SaveChanges();
		}

		private void ImageMetadataControl_Load(object sender, EventArgs e)
		{
			ParentForm.FormClosing +=new FormClosingEventHandler((s,o)=>SaveChanges());
		}

		//TODO: need a simple chooser (combo box?) for Creative Commons or custom, if custom, show editable description.
	}
}
