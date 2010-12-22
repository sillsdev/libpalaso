using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.ImageToolbox;

namespace PalasoUIWindowsForms.Tests.ImageToolbox
{
	class ImageToolboxTests
	{
		[Test, Ignore("by hand only")]
		public void ShowToolbox()
		{
			Application.EnableVisualStyles();
			using(var dlg = new ImageToolboxDialog())
			{
				dlg.ShowDialog();
			}
		}

	}
}
