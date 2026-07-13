using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
		public void Dispose_CalledTwiceAfterSettingImage_DoesNotThrow()
		{
			// Exercises double-dispose after the cropper actually holds state to clean up
			// (saved-original temp file, cropping image, Application.Idle subscription),
			// not just on a freshly-constructed, empty instance.
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
		public void SetImage_Reassign_DisposesPreviousSavedOriginalAndCroppingImage()
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

					var firstSavedOriginal = (TempFile)GetPrivateField(cropper, "_savedOriginalImage");
					var firstCroppingImage = (Image)GetPrivateField(cropper, "_croppingImage");
					var firstSavedOriginalPath = firstSavedOriginal.Path;
					Assert.That(File.Exists(firstSavedOriginalPath), "Sanity check: first saved-original temp file should exist before reassignment");

					cropper.SetImage(img2);

					// Regression test: the Image setter used to overwrite _savedOriginalImage/_croppingImage
					// without disposing the previous instances, leaking the temp file and the cropping bitmap.
					Assert.That(File.Exists(firstSavedOriginalPath), Is.False,
						"Previous saved-original temp file should have been disposed (and deleted) on reassignment");
					Assert.Throws<ArgumentException>(() => { var _ = firstCroppingImage.Width; },
						"Previous cropping image should have been disposed on reassignment");

					using (var result = cropper.GetCroppedImage())
					{
						Assert.IsNotNull(result);
						Assert.Greater(result.Width, 0);
					}
				}
			}
		}

		[Test]
		public void SetImage_TallImage_DownscalesCroppingImage()
		{
			// Regression test for the height/width typo (BL-1275): the downscale-threshold check
			// tested Width > 1000 twice instead of Width > 1000 and Height > 1000, so a tall image
			// (height > 1000, width <= 1000) skipped downscaling entirely.
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
					var croppingImage = (Image)GetPrivateField(cropper, "_croppingImage");
					Assert.Less(croppingImage.Height, 1200,
						"Tall image should have been downscaled before cropping");
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

		private static object GetPrivateField(ImageCropper cropper, string fieldName)
		{
			return typeof(ImageCropper)
				.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(cropper);
		}
	}
}
