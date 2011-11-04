using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.ClearShare;
using Palaso.UI.WindowsForms.ImageToolbox;

namespace PalasoUIWindowsForms.Tests.ImageToolbox
{
	[TestFixture]
	public class MetaDataDisplayTests
	{
		[Test, Ignore("By Hand")]
		public void ShowControl()
		{
			var m = new MetaData();
			m.CopyrightNotice = "copyright me";
			m.Creator = "you";
			m.AttributionUrl = "http://google.com";
			m.License = new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			var c = new MetadataDisplayControl();
			c.SetLicense(m);
			var dlg = new Form();
			c.Dock = DockStyle.Fill;
			dlg.Controls.Add(c);
			dlg.ShowDialog();
		}
	}
}
