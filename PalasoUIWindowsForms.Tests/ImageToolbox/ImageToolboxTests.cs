using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
		[STAThread]
		public void ShowToolbox()
		{
			Application.EnableVisualStyles();
			using (var dlg = new ImageToolboxDialog(new PalasoImage() { Image = null}))
			{
				dlg.ShowDialog();
			}
		}

		[Test]
		public void MethodBeingTested_Situation_Result()
		{
			Bitmap i = new Bitmap(64, 64);
			i.MakeTransparent(Color.White);
		}

	}
}
