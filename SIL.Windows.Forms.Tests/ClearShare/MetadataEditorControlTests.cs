﻿using System.Windows.Forms;
using NUnit.Framework;
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
			var m = new Metadata();
			using (var dlg = new MetadataEditorDialog(m))
			{
				dlg.ShowDialog();
			}
		}

		[Test, Ignore("By Hand")]
		public void ShowFullDialogTwiceToCheckRoundTripping()
		{
			var m = new Metadata();
			m.License = CreativeCommonsLicense.FromToken("by");
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
			var m = new Metadata();
			m.CopyrightNotice = "copyright me";
			m.Creator = "you";
			m.AttributionUrl = "http://google.com";
			m.License = new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
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
			var m = new Metadata();
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
			var m = new Metadata();
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
	}
}
