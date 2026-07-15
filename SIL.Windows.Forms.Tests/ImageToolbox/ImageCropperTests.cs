using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
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
		public void SetImage_TallImage_DownscalesCroppingImage()
		{
			using (var tempFile = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(100, 1200))
					bmp.Save(tempFile.Path, ImageFormat.Png);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper())
				{
					cropper.Size = new Size(400, 300);
					cropper.SetImage(palasoImage);

					// Not owned by this test -- the cropper still holds and will dispose this itself.
					var croppingImage = GetCroppingImage(cropper);

					Assert.Less(croppingImage.Height, 1200,
						"Tall image should have been downscaled before cropping");
				}
			}
		}

		[Test]
		public void SetImage_WideImage_DownscalesCroppingImage()
		{
			using (var tempFile = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(1200, 100))
					bmp.Save(tempFile.Path, ImageFormat.Png);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper())
				{
					cropper.Size = new Size(400, 300);
					cropper.SetImage(palasoImage);

					// Not owned by this test -- the cropper still holds and will dispose this itself.
					var croppingImage = GetCroppingImage(cropper);

					Assert.Less(croppingImage.Width, 1200,
						"Wide image should have been downscaled before cropping");
				}
			}
		}

		private static Image GetCroppingImage(ImageCropper cropper)
		{
			return (Image)typeof(ImageCropper)
				.GetField("_croppingImage", BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(cropper);
		}
	}
}
