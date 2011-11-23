using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.ClearShare;
using Palaso.UI.WindowsForms.ClearShare.WinFormsUI;
using Palaso.UI.WindowsForms.ImageToolbox;

namespace PalasoUIWindowsForms.Tests.ImageToolbox
{
	[TestFixture]
	public class MetadataEditorControlTests
	{
		[Test, Ignore("By Hand")]
		public void ShowControl()
		{
			var m = new Metadata();
			m.CopyrightNotice = "copyright me";
			m.Creator = "you";
			m.AttributionUrl = "http://google.com";
			m.License = new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			var c = new MetdataEditorControl();
			c.Metadata = m;
			var dlg = new Form();
			dlg.Height = c.Height;
			dlg.Width = c.Width + 20;
			c.Dock = DockStyle.Fill;
			dlg.Controls.Add(c);
			dlg.ShowDialog();
		}
	}
}
