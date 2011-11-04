using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
{
	public partial class MetdataEditorControl : UserControl
	{
		private Metadata _metadata;

		public MetdataEditorControl()
		{
			InitializeComponent();
		}

		public Metadata Metadata
		{
			get { return _metadata; }
			set
			{
				_metadata = value;

				if (_metadata == null)
				{
					this.Visible = false;
					return;
				}
				this.Visible = true;
				_illustrator.Text = _metadata.Creator;
				_copyright.Text = _metadata.CopyrightNotice;
				_licenseImage.Image = _metadata.License.GetImage();
				if (_metadata.License is CreativeCommonsLicense)
				{
					var cc = (CreativeCommonsLicense) _metadata.License;
					_creativeCommons.Checked = true;
					_noDerivates.Checked = cc.DerivativeRule == CreativeCommonsLicense.DerivativeRules.NoDerivatives;
					_shareAlike.Checked = cc.DerivativeRule == CreativeCommonsLicense.DerivativeRules.Derivatives;
					_derivatives.Checked = cc.DerivativeRule == CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike;
					_commercial.Checked = cc.CommercialUseAllowed;
					_nonCommercial.Checked = !cc.CommercialUseAllowed;
				}
				else
				{
					_noLicense.Checked = true;
				}

			}
		}

		private void OnLicenseComponentChanged(object sender, System.EventArgs e)
		{
			if(_metadata.License ==null || !(_metadata.License is CreativeCommonsLicense))//todo: that's kinda heavy-handed
				_metadata.License = new CreativeCommonsLicense(true,true,CreativeCommonsLicense.DerivativeRules.Derivatives);

			panel1.Enabled = panel2.Enabled = _creativeCommons.Checked;
			_licenseImage.Visible = _creativeCommons.Checked;

			if (_creativeCommons.Checked)
			{
				var cc = (CreativeCommonsLicense) _metadata.License;
				cc.AttributionRequired = true; // for now, we don't have a way to turn that off
				cc.CommercialUseAllowed = _commercial.Checked;
				if (_derivatives.Checked)
					cc.DerivativeRule = CreativeCommonsLicense.DerivativeRules.Derivatives;
				else if (_shareAlike.Checked)
					cc.DerivativeRule = CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike;
				else
					cc.DerivativeRule = CreativeCommonsLicense.DerivativeRules.NoDerivatives;
				_licenseImage.Image = cc.GetImage();
			}
		}

		/*       private PalasoImage _image;

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
			if (_image != null && _image.Metadata.HasChanges)
			{
				_image.Metadata.Write();
			}
		}


		public override bool ValidateChildren()
		{
			SaveChanges();
			return base.ValidateChildren();
		}
		private void UpdateDisplay()
		{
			if (_image == null)
			{
				_illustrator.ReadOnly = _copyright.ReadOnly == false;
				return;
			}

			this.Visible = _image.Image != null;

			//_lockedCheckbox.Checked = _image.MetadataLocked;
			_illustrator.Text = _image.Metadata.Creator;
			_copyright.Text = _image.Metadata.CopyrightNotice;
			_illustrator.ReadOnly = _copyright.ReadOnly = !_image.Metadata.AllowEditingMetadata;
			_illustrator.BorderStyle = _copyright.BorderStyle = _image.Metadata.AllowEditingMetadata ? BorderStyle.FixedSingle : BorderStyle.None;

			if (_image.Metadata.AllowEditingMetadata)
			{
			}
			//only handle the first one, for now
			if (FirstLicense == null)
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
				return _image.Metadata.License;
			}
		}

		private void _illustrator_TextChanged(object sender, EventArgs e)
		{
			_image.Metadata.Creator = _illustrator.Text;
		}

		private void _copyright_TextChanged(object sender, EventArgs e)
		{
			_image.Metadata.CopyrightNotice = _copyright.Text;
		}

		private void _lockedCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			// _image.MetadataLocked = _lockedCheckbox.Checked;
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
			ParentForm.FormClosing += new FormClosingEventHandler((s, o) => SaveChanges());
		}

		//TODO: need a simple chooser (combo box?) for Creative Commons or custom, if custom, show editable description.
  */

	}
}
