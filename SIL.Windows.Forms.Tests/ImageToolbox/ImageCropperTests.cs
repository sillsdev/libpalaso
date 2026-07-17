using System;
using System.Drawing;
using System.Drawing.Imaging;
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

		private static Image GetCroppingImage(ImageCropper cropper)
		{
			return (Image)typeof(ImageCropper)
				.GetField("_croppingImage", BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(cropper);
		}
	}
}
