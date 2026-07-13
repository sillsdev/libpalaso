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
		public void GetCroppedImage_JpegImage_ReturnsUsableBitmap()
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

					using (var result = cropper.GetCroppedImage())
					{
						Assert.IsNotNull(result);
						// The crop is a stand-alone in-memory bitmap, not backed by a file or stream, so
						// it reports MemoryBmp format even for a JPEG source; the caller chooses the actual
						// save format via the file extension.
						Assert.AreEqual(ImageFormat.MemoryBmp.Guid, result.RawFormat.Guid);
						// Re-encode to force GDI+ to re-read the pixel data from the backing store.
						using (var ms = new MemoryStream())
							Assert.DoesNotThrow(() => result.Save(ms, ImageFormat.Png));
					}
				}
			}
		}

		[Test]
		public void GetCroppedImage_JpegImage_SetViaPropertyDirectly_ReturnsUsableBitmap()
		{
			// Setting Image directly (not via SetImage) must still yield a usable crop.
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
						Assert.Greater(result.Width, 0);
						using (var ms = new MemoryStream())
							Assert.DoesNotThrow(() => result.Save(ms, ImageFormat.Png));
					}
				}
			}
		}

		[Test]
		public void SetImage_Reassign_DoesNotThrow()
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
						Assert.Greater(result.Width, 0);
					}
				}
			}
		}

		[Test]
		public void SetImage_ReCropPreviouslyCroppedJpeg_DoesNotThrow()
		{
			// Regression test for issue #1275: cropping a JPEG, then feeding the result back into a
			// new cropper (which re-saves it in the Image setter via value.Image.Save) crashed when
			// the cropped bitmap was backed by a disposed stream. The crop is now a stand-alone
			// bitmap, so the round-trip must not throw.
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
