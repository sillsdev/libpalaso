using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
		public void Dispose_CalledTwice_DoesNotThrow()
		{
			var cropper = new ImageCropper();
			Assert.DoesNotThrow(() => cropper.Dispose());
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
						Assert.Greater(result.Width, 0);
						// Re-encode to force GDI+ to re-read the pixel data from the backing store.
						using (var ms = new MemoryStream())
							Assert.DoesNotThrow(() => result.Save(ms, ImageFormat.Png));
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
						// Re-encode to force GDI+ to re-read the pixel data from the backing stream.
						using (var ms = new MemoryStream())
							Assert.DoesNotThrow(() => result.Save(ms, ImageFormat.Png));
					}
					finally
					{
						result?.Dispose();
					}
				}
			}
		}
		[Test]
		public void GetCroppedImage_JpegImage_SetViaPropertyDirectly_ReturnsJpegBitmap()
		{
			// Setting Image directly (not via SetImage) must still initialize _originalFormat so
			// GetCroppedImage does not throw NullReferenceException on JPEG re-encoding.
			using (var tempFile = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Jpeg);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper())
				{
					cropper.Size = new Size(400, 300);
					cropper.Image = palasoImage; // bypass SetImage intentionally

					using (var result = cropper.GetCroppedImage())
					{
						Assert.IsNotNull(result);
						Assert.AreEqual(ImageFormat.Jpeg.Guid, result.RawFormat.Guid);
						Assert.Greater(result.Width, 0);
					}
				}
			}
		}

		[Test]
		public void SetImage_Reassign_UpdatesFormatAndDoesNotThrow()
		{
			using (var tempFile1 = TempFile.WithExtension(".png"))
			using (var tempFile2 = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(100, 80))
				{
					bmp.Save(tempFile1.Path, ImageFormat.Png);
					bmp.Save(tempFile2.Path, ImageFormat.Jpeg);
				}

				using (var img1 = PalasoImage.FromFile(tempFile1.Path))
				using (var img2 = PalasoImage.FromFile(tempFile2.Path))
				using (var cropper = new ImageCropper())
				{
					cropper.Size = new Size(400, 300);
					cropper.SetImage(img1);
					cropper.SetImage(img2);

					using (var result = cropper.GetCroppedImage())
					{
						Assert.IsNotNull(result);
						Assert.AreEqual(ImageFormat.Jpeg.Guid, result.RawFormat.Guid);
					}
				}
			}
		}

		[Test]
		public void SetImage_ReCropPreviouslyCroppedJpeg_DoesNotThrow()
		{
			// GetImage() returns a JPEG Bitmap backed by a MemoryStream; feeding that result
			// back into a new cropper re-saves it in the Image setter (value.Image.Save), which
			// crashed once the backing stream had been disposed.
			using (var tempFile = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(1200, 900))
					bmp.Save(tempFile.Path, ImageFormat.Jpeg);

				// GetImage() returns the same PalasoImage instance, now holding the cropped JPEG,
				// so the outer using disposes it exactly once.
				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				{
					PalasoImage cropped;
					using (var cropper1 = new ImageCropper())
					{
						cropper1.Size = new Size(400, 300);
						cropper1.SetImage(palasoImage);
						cropped = cropper1.GetImage();
					}
					Assert.That(cropped, Is.Not.Null);

					using (var cropper2 = new ImageCropper())
					{
						cropper2.Size = new Size(400, 300);
						Assert.DoesNotThrow(() => cropper2.SetImage(cropped));
					}
				}
			}
		}
	}
}
