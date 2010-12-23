using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class ImageCreditsControl : UserControl, IImageToolboxControl
	{
		private PalasoImage _previousImage;

		public ImageCreditsControl()
		{
			InitializeComponent();
		}
		public void SetImage(PalasoImage image)
		{
			_previousImage = image;
		}

		public PalasoImage GetImage()
		{
			return _previousImage;
		}
	}
}
