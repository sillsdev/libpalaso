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
	public partial class ImageLicenseControl : UserControl, IImageToolboxControl
	{
		private PalasoImage _previousImage;

		public ImageLicenseControl()
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

		public event EventHandler ImageChanged;
	}
}
