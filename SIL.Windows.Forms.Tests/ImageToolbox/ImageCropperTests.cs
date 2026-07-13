using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using NUnit.Framework;
using SIL.IO;
using SIL.Windows.Forms.ImageToolbox;
using SIL.Windows.Forms.ImageToolbox.Cropping;

namespace SIL.Windows.Forms.Tests.ImageToolbox
{
	[Apartment(ApartmentState.STA)]
	[TestFixture]
	public class ImageCropperTests
	{
		[Test]
		public void Dispose_CalledTwiceAfterSettingImage_DoesNotThrow()
		{
			using (var tempFile = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Png);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				{
					var cropper = new ImageCropper();
					cropper.Size = new Size(400, 300);
					cropper.SetImage(palasoImage);

					Assert.DoesNotThrow(() => cropper.Dispose());
					Assert.DoesNotThrow(() => cropper.Dispose());
				}
			}
		}
	}
}
