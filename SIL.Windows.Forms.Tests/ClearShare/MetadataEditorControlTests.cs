using System.Windows.Forms;
using NUnit.Framework;
using SIL.Core.ClearShare;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;

namespace SIL.Windows.Forms.Tests.ClearShare
{
	[TestFixture]
	public class MetadataEditorControlTests
	{

		[Test, Ignore("By Hand")]
		public void ShowFullDialog()
		{
			var m = new MetadataForLicenseWithImage();
			using (var dlg = new MetadataEditorDialog(m))
			{
				dlg.ShowDialog();
			}
		}

		[Test, Ignore("By Hand")]
		public void ShowFullDialogTwiceToCheckRoundTripping()
		{
			var m = new MetadataForLicenseWithImage();
			m.License = CreativeCommonsLicenseWithImage.FromToken("by");
			m.License.RightsStatement = "some restrictions";

			using (var dlg = new MetadataEditorDialog(m))
			{
				dlg.ShowDialog();
				m = dlg.Metadata;
			}

			using (var dlg = new MetadataEditorDialog(m))
			{
				dlg.ShowDialog();
			}
		}

		[Test, Ignore("By Hand")]
		public void ShowControl()
		{
			var m = new MetadataForLicenseWithImage();
			m.CopyrightNotice = "copyright me";
			m.Creator = "you";
			m.AttributionUrl = "http://google.com";
			m.License = new CreativeCommonsLicenseWithImage(true, false, CreativeCommonsLicenseWithImage.DerivativeRules.DerivativesWithShareAndShareAlike);
			var c = new MetadataEditorControl();
			c.Metadata = m;
			var dlg = new Form();
			dlg.Height = c.Height;
			dlg.Width = c.Width + 20;
			c.Dock = DockStyle.Fill;
			dlg.Controls.Add(c);
			dlg.ShowDialog();
		}
		[Test, Ignore("By Hand")]
		public void ShowControlDefault()
		{
			var m = new MetadataForLicenseWithImage();
			m.CopyrightNotice = "copyright me";
			m.Creator = "you";
			m.AttributionUrl = "http://google.com";
			m.License = new NullLicense();
			var c = new MetadataEditorControl();
			c.Metadata = m;
			var dlg = new Form();
			dlg.Height = c.Height;
			dlg.Width = c.Width + 20;
			c.Dock = DockStyle.Fill;
			dlg.Controls.Add(c);
			dlg.ShowDialog();
		}

		[Test, Ignore("By Hand")]
		public void ShowControl_NoLicense()
		{
			var m = new MetadataForLicenseWithImage();
			m.CopyrightNotice = "copyright me";
			m.Creator = "you";
			m.AttributionUrl = "http://google.com";
			m.License = new NullLicense();
			var c = new MetadataEditorControl();
			c.Metadata = m;
			var dlg = new Form();
			dlg.Height = c.Height;
			dlg.Width = c.Width + 20;
			c.Dock = DockStyle.Fill;
			dlg.Controls.Add(c);
			dlg.ShowDialog();
		}

		[Test]
		public void MetadataSetter_WasIGO_UncheckingProducesCurrentDefaultLicense()
		{
			var m = new Metadata();
			m.CopyrightNotice = "test1";
			m.Creator = "test2";
			var ccLicense = new CreativeCommonsLicenseWithImage(true, false, CreativeCommonsLicenseWithImage.DerivativeRules.DerivativesWithShareAndShareAlike);
			ccLicense.IntergovernmentalOrganizationQualifier = true;
			m.License = ccLicense;
			Assert.That(m.License.Url, Does.EndWith("3.0/igo/"));
			// SUT
			ccLicense.IntergovernmentalOrganizationQualifier = false;
			m.License = ccLicense;
			Assert.That(m.License.Url, Does.EndWith(CreativeCommonsLicenseWithImage.kDefaultVersion+"/"));
		}

		[Test]
		public void MetadataSetter_WasCC3_thenIGO_UncheckingStillProducesCurrentDefaultLicense()
		{
			var m = new Metadata();
			m.CopyrightNotice = "test1";
			m.Creator = "test2";
			var ccLicense = new CreativeCommonsLicenseWithImage(true, false, CreativeCommonsLicenseWithImage.DerivativeRules.DerivativesWithShareAndShareAlike);
			ccLicense.Version = "3.0"; // set old version (but non-IGO)
			m.License = ccLicense;
			Assert.That(m.License.Url, Does.EndWith("3.0/"));
			ccLicense.IntergovernmentalOrganizationQualifier = true;
			m.License = ccLicense;
			Assert.That(m.License.Url, Does.EndWith("3.0/igo/"));
			// SUT
			ccLicense.IntergovernmentalOrganizationQualifier = false;
			m.License = ccLicense;
			// Considered an acceptable loss of information, since the user was messing with the IGO setting.
			Assert.That(m.License.Url, Does.EndWith(CreativeCommonsLicenseWithImage.kDefaultVersion + "/"));
		}
	}
}
