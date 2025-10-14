using System.Drawing;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare
{
	public class CustomLicenseWithImage : CustomLicense, ILicenseWithImage
	{
		public Image GetImage()
		{
			return null;
		}
	}
}