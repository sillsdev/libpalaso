using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
					var cropper = new ImageCropper { Size = new Size(400, 300) };
					cropper.SetImage(palasoImage);

					Assert.DoesNotThrow(() => cropper.Dispose());
					Assert.DoesNotThrow(() => cropper.Dispose());
				}
			}
		}

		// Garbage collection is non-deterministic, so this test may be flaky.
		// If it turns out to be a problem, drop this test and its supporting method.
		[Test]

		public void Dispose_AllowsGarbageCollection()
		{
			// The ImageCropper subscribes to the static Application.Idle event in its
			// constructor, so it needs to unsubscribe on Dispose.
			var reference = CreateAndDisposeImageCropper();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.That(reference.IsAlive, Is.False,
				"ImageCropper was not garbage collected after disposal. " +
				"It may be subscribed to a static event.");
		}

		// Kept in a separate, non-inlined method so the local ImageCropper reference is
		// guaranteed out of scope before the caller forces garbage collection.
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference CreateAndDisposeImageCropper()
		{
			using (var tempFile = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Png);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				{
					var cropper = new ImageCropper { Size = new Size(400, 300) };
					cropper.SetImage(palasoImage);

					var reference = new WeakReference(cropper);
					cropper.Dispose();
					return reference;
				}
			}
		}

		[Test]
		public void SetImage_TallImage_DownscalesCroppingImage()
		{
			using (var tempFile = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(100, 1200))
					bmp.Save(tempFile.Path, ImageFormat.Png);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(palasoImage);

					// Don't dispose this: the cropper owns _croppingImage and disposes it itself.
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
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(palasoImage);

					// Don't dispose this: the cropper owns _croppingImage and disposes it itself.
					var croppingImage = GetCroppingImage(cropper);

					Assert.Less(croppingImage.Width, 1200,
						"Wide image should have been downscaled before cropping");
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
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(palasoImage);

					using (var result = cropper.GetCroppedImage())
					{
						Assert.That(result, Is.Not.Null);
						// Re-encode to force GDI+ to read the pixel data back from its backing store.
						using (var stream = new MemoryStream())
							Assert.That(() => result.Save(stream, ImageFormat.Png), Throws.Nothing);
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
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(palasoImage);

					using (var result = cropper.GetCroppedImage())
					{
						Assert.That(result, Is.Not.Null);
						// The crop is a stand-alone in-memory bitmap rather than one backed by a file or
						// stream, so it reports MemoryBmp even for a JPEG source. Callers pick the save
						// format from the file extension.
						Assert.That(result.RawFormat.Guid, Is.EqualTo(ImageFormat.MemoryBmp.Guid));
						using (var stream = new MemoryStream())
							Assert.That(() => result.Save(stream, ImageFormat.Png), Throws.Nothing);
					}
				}
			}
		}

		[Test]
		public void GetCroppedImage_ImageSetViaPropertyDirectly_ReturnsUsableBitmap()
		{
			// Setting Image directly rather than through SetImage used to leave _originalFormat null,
			// so GetCroppedImage threw a NullReferenceException reading it.
			using (var tempFile = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Jpeg);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.Image = palasoImage;

					using (var result = cropper.GetCroppedImage())
					{
						Assert.That(result, Is.Not.Null);
						using (var stream = new MemoryStream())
							Assert.That(() => result.Save(stream, ImageFormat.Png), Throws.Nothing);
					}
				}
			}
		}

		[Test]
		public void GetImage_ReCropPreviouslyCroppedJpeg_DoesNotThrow()
		{
			// Issue #1275: cropping a JPEG and feeding the result back into a new cropper, which
			// re-saves it in the Image setter, failed once the crop was backed by a disposed stream.
			using (var tempFile = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(1200, 900))
					bmp.Save(tempFile.Path, ImageFormat.Jpeg);

				// GetImage returns the same PalasoImage, now holding the crop, so the outer using
				// disposes it exactly once.
				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				{
					PalasoImage cropped;
					using (var firstCropper = new ImageCropper { Size = new Size(400, 300) })
					{
						firstCropper.SetImage(palasoImage);
						cropped = firstCropper.GetImage();
					}
					Assert.That(cropped, Is.Not.Null);

					using (var secondCropper = new ImageCropper { Size = new Size(400, 300) })
						Assert.That(() => secondCropper.SetImage(cropped), Throws.Nothing);
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
