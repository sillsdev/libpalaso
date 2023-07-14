using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SIL.Windows.Forms.ClearShare.WinFormsUI
{
	public partial class MetadataEditorControl : UserControl
	{
		private Metadata _metadata;
		private bool _settingUp;

		public MetadataEditorControl()
		{
			InitializeComponent();

			_settingUp = true;
			//set some defaults in case they turn on CC
			_shareAlike.Checked = true;
			_nonCommercial.Checked = true;

			//the system PictureBox makes the CC licenses look awful, so we are using one with a custom OnPaint()
			var betterPictureBox = new BetterPictureBox()
			{
				SizeMode = _licenseImage.SizeMode,
				Bounds = _licenseImage.Bounds,
				TabStop = false
			};
			Controls.Add(betterPictureBox);
			Controls.Remove(_licenseImage);
			_licenseImage.Dispose();
			_licenseImage = betterPictureBox;

			_linkToPublicDomainCC0.Text = string.Format(_linkToPublicDomainCC0.Text, "CC0");
		}

		protected override void OnLoad(EventArgs e)
		{
			ActiveControl = _copyrightBy;
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			// ReSharper disable once PossibleNullReferenceException
			ParentForm.Shown += (sender, ee) => UpdateDisplay();
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
				_settingUp = true;
				this.Visible = true;
				_illustrator.Text = _metadata.Creator;
				_copyrightYear.Text = _metadata.GetCopyrightYear();
				if(_copyrightYear.Text =="")
					_copyrightYear.Text = DateTime.Now.Year.ToString();

				_copyrightBy.Text = _metadata.GetCopyrightBy();
				if(_metadata.License!=null)
					_licenseImage.Image = _metadata.License.GetImage();


				if (_metadata.License is CreativeCommonsLicense)
				{
					var cc = (CreativeCommonsLicense) _metadata.License;
					if (cc.AttributionRequired)
					{
						_creativeCommons.Checked = true;
						_noDerivates.Checked = cc.DerivativeRule == CreativeCommonsLicense.DerivativeRules.NoDerivatives;
						_shareAlike.Checked = cc.DerivativeRule == CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike;
						_derivatives.Checked = cc.DerivativeRule == CreativeCommonsLicense.DerivativeRules.Derivatives;
						_commercial.Checked = cc.CommercialUseAllowed;
						_nonCommercial.Checked = !cc.CommercialUseAllowed;
						_customRightsStatement.Text = _metadata.License.RightsStatement;
					}
					else
					{
						_publicDomainCC0.Checked = true;
						_customRightsStatement.Text = _metadata.License.RightsStatement;
						_commercial.Checked = true;		// emphasize freedom, set up for transition to normal cc license
						_derivatives.Checked = true;
					}
				}
				else if(_metadata.License is CustomLicense)
				{
					_customLicense.Checked = true;
					_customRightsStatement.Text = _metadata.License.RightsStatement;
				}
				else
				{
					_unknownLicense.Checked = true;
				}
				_settingUp = false;

				if (IsHandleCreated)
					UpdateDisplay();
			}
		}

		/// <summary>
		/// Set this to false if you don't want to collect info on who created it (e.g. you're just getting copyright/license)
		/// </summary>
		public bool ShowCreator
		{
			get { return _illustrator.Visible; }
			set { _illustrator.Visible = _illustratorLabel.Visible = value; }
		}

		private void OnLicenseComponentChanged(object sender, System.EventArgs e)
		{
			if(_settingUp)
				return;

			var previousLicense = _metadata.License;
			var previousWasCC = previousLicense != null && previousLicense is CreativeCommonsLicense;
			var previousWasCC0 = previousWasCC && !((CreativeCommonsLicense)previousLicense).AttributionRequired;
			var currentIsCC0 = false;

			if (_metadata.License == null || !(_metadata.License is CreativeCommonsLicense))//todo: that's kinda heavy-handed
				_metadata.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives);

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

				// If we're going from custom to CC, we could as easily just copy the statement into CC license.
				// Maybe they want that. If they don't, maybe they'll fail to notice that we turned their
				// custom license into a CC restriction, or be confused by why we did that.
				// In addition, custom restrictions are so... undesirable / unenforceable. So we have to guess,
				// and we're going to guess on the side of getting rid of it.
				if (!previousWasCC)
			    {
			        _customRightsStatement.Text = "";
			    }
				// We need to ensure that the default CC version is referenced if changing from another
				// license.  CC0 installs Version 1.0, which is wrong for all other CC licenses.
				// (See https://issues.bloomlibrary.org/youtrack/issue/BL-11032.)
				if (previousWasCC0 || !previousWasCC)
				{
					// reset license to our desired defaults
					cc.Version = CreativeCommonsLicense.kDefaultVersion;
					cc.AttributionRequired = true;
					cc.CommercialUseAllowed = true;
					cc.DerivativeRule = CreativeCommonsLicense.DerivativeRules.Derivatives;
					cc.IntergovernmentalOrganizationQualifier = false;
					_commercial.Checked = true;
					_derivatives.Checked = true;
				}
			}
			else if (_publicDomainCC0.Checked)
			{
				// Public Domain CC0 = totally open CC license
				var cc0 = (CreativeCommonsLicense)_metadata.License;
				cc0.AttributionRequired = false;
				cc0.CommercialUseAllowed = true;
				cc0.DerivativeRule = CreativeCommonsLicense.DerivativeRules.Derivatives;
				cc0.IntergovernmentalOrganizationQualifier = false;
				cc0.Version = "";
				_licenseImage.Image = cc0.GetImage();
				// Keep custom statement only if previous license was CC0.
				if (!previousWasCC0)
				{
					_customRightsStatement.Text = "";
				}
				currentIsCC0 = true;
			}
			else if(_unknownLicense.Checked)
			{
				_metadata.License = new NullLicense();
			}
			else if (_customLicense.Checked)
			{
				_metadata.License = new CustomLicense() {RightsStatement = _customRightsStatement.Text};
			}
			// Wording of copyright notice changes for Public Domain compared to other licenses.
			// (Public Domain doesn't claim copyright, but still attributes creator initially.)
			if (previousWasCC0 != currentIsCC0)
				_metadata.SetCopyrightNotice(_copyrightYear.Text, _copyrightBy.Text);

			UpdateDisplay();
		}

		private bool CreativeCommonsStyleLicenseIsChecked()
		{
			return _creativeCommons.Checked || _publicDomainCC0.Checked;
		}

		private void UpdateDisplay()
		{
			panel1.Enabled = panel2.Enabled = _creativeCommons.Checked;
			_licenseImage.Visible = CreativeCommonsStyleLicenseIsChecked();
			_customRightsStatement.Enabled = _customLicense.Checked || CreativeCommonsStyleLicenseIsChecked();
			_linkToRefinedCreativeCommonsWarning.Visible = CreativeCommonsStyleLicenseIsChecked() && !string.IsNullOrWhiteSpace(_customRightsStatement.Text);
			_additionalRequestsLabel.Visible = CreativeCommonsStyleLicenseIsChecked();

			if (CreativeCommonsStyleLicenseIsChecked())
			{
				_customRightsStatement.Top = _additionalRequestsLabel.Bottom+10;
				_customRightsStatement.Left = _creativeCommons.Left;
				_customRightsStatement.Width = tableLayoutPanel1.Right - _customRightsStatement.Left;
			}
			else
			{
				_customRightsStatement.Top = _additionalRequestsLabel.Top;
				_customRightsStatement.Left = _licenseImage.Left;
				_customRightsStatement.Width = tableLayoutPanel1.Right - _customRightsStatement.Left;
			}
		}

		private void _illustrator_TextChanged(object sender, EventArgs e)
		{
			_metadata.Creator = _illustrator.Text;
		}


		private void _copyrightYear_TextChanged(object sender, EventArgs e)
		{
			if (_settingUp)
				return;
			_metadata.SetCopyrightNotice(_copyrightYear.Text, _copyrightBy.Text);
		}

		private void _customLicenseDescription_TextChanged(object sender, EventArgs e)
		{
			if (_customLicense.Checked)
			{
				var customLicense = _metadata.License as CustomLicense;

				if (customLicense != null)
					customLicense.RightsStatement = _customRightsStatement.Text;
			}
			if (CreativeCommonsStyleLicenseIsChecked())
			{
				var l = _metadata.License as CreativeCommonsLicense;

				l.RightsStatement = _customRightsStatement.Text;
			}
			UpdateDisplay();
		}

		private void _copyrightBy_TextChanged(object sender, EventArgs e)
		{
			if (_settingUp)
				return;
			_metadata.SetCopyrightNotice(_copyrightYear.Text, _copyrightBy.Text);
		}
	}
	public class BetterPictureBox : PictureBox
	{
		protected override void OnPaint(PaintEventArgs paintEventArgs)
		{
			paintEventArgs.Graphics.InterpolationMode = InterpolationMode.High;
			base.OnPaint(paintEventArgs);
		}
	}
}
