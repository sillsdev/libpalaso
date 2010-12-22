using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class ImageToolboxDialog : Form
	{
		public ImageToolboxDialog(PalasoImage imageInfo)
		{
			InitializeComponent();
			imageToolboxControl1.ImageInfo = imageInfo;
		}
		public PalasoImage ImageInfo { get { return imageToolboxControl1.ImageInfo; } }
	}
}
