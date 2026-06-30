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
		public void Dispose_DoesNotThrow()
		{
			var cropper = new ImageCropper();
			Assert.DoesNotThrow(() => cropper.Dispose());
		}

		[Test]
		public void Dispose_CalledTwice_DoesNotThrow()
		{
			var cropper = new ImageCropper();
			cropper.Dispose();
			Assert.DoesNotThrow(() => cropper.Dispose());
		}

		[Test]
		public void GetCroppedImage_PngImage_ReturnsUsableBitmap()
		{
			using (var tempFile = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Png);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper())
				{
					cropper.Size = new Size(400, 300);
					cropper.SetImage(palasoImage);

					using (var result = cropper.GetCroppedImage())
					{
						Assert.IsNotNull(result);
						// Accessing Width forces GDI+ to decode the image data; this would throw
						// if the backing stream had been prematurely disposed.
						Assert.Greater(result.Width, 0);
						Assert.Greater(result.Height, 0);
					}
				}
			}
		}

		[Test]
		public void GetCroppedImage_JpegImage_ReturnsJpegBitmapUsableAfterReturn()
		{
			using (var tempFile = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Jpeg);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper())
				{
					cropper.Size = new Size(400, 300);
					cropper.SetImage(palasoImage);

					Image result;
					// GetCroppedImage returns a Bitmap backed by a MemoryStream for JPEG.
					// The stream must still be alive after the method returns.
					result = cropper.GetCroppedImage();
					try
					{
						Assert.IsNotNull(result);
						Assert.AreEqual(ImageFormat.Jpeg.Guid, result.RawFormat.Guid);
						// Force pixel data access to confirm the stream is still alive.
						Assert.Greater(result.Width, 0);
						Assert.Greater(result.Height, 0);
					}
					finally
					{
						result?.Dispose();
					}
				}
			}
		}
	}
}
