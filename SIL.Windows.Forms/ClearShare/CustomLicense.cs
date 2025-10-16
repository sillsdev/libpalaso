using System.Drawing;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare
{
	public class CustomLicense : CustomLicenseBase, ILicenseWithImage
	{
		public Image GetImage()
		{
			return null;
		}
	}
}