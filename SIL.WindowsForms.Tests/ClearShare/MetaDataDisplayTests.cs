using System.Windows.Forms;
using NUnit.Framework;
using SIL.WindowsForms.ClearShare;
using SIL.WindowsForms.ClearShare.WinFormsUI;

namespace SIL.WindowsForms.Tests.ClearShare
{
	[TestFixture]
	public class MetadataDisplayTests
	{
		[Test, Ignore("By Hand")]
		public void ShowControl()
		{
			var m = new Metadata();
			m.CopyrightNotice = "copyright me";
			m.Creator = "you";
			m.AttributionUrl = "http://google.com";
			m.License = new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			var c = new MetadataDisplayControl();
			c.SetMetadata(m);
			var dlg = new Form();
			c.Dock = DockStyle.Fill;
			dlg.Controls.Add(c);
			dlg.ShowDialog();
		}
	}
}
