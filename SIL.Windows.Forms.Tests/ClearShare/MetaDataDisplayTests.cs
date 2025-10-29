using System.Windows.Forms;
using NUnit.Framework;
using SIL.Core.ClearShare;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;

namespace SIL.Windows.Forms.Tests.ClearShare
{
	[TestFixture]
	public class MetadataDisplayTests
	{
		[Test, Ignore("By Hand")]
		public void ShowControl()
		{
			var m = new MetadataForLicenseWithImage();
			m.CopyrightNotice = "copyright me";
			m.Creator = "you";
			m.AttributionUrl = "http://google.com";
			m.License = new CreativeCommonsLicenseWithImage(true, false, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			var c = new MetadataDisplayControl();
			c.SetMetadata(m);
			var dlg = new Form();
			c.Dock = DockStyle.Fill;
			dlg.Controls.Add(c);
			dlg.ShowDialog();
		}
	}
}
