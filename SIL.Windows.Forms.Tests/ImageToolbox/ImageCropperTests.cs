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
		public void SetImage_Reassigned_DisposesStateBuiltForPreviousImage()
		{
			using (var tempFile1 = TempFile.WithExtension(".png"))
			using (var tempFile2 = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(100, 80))
				{
					bmp.Save(tempFile1.Path, ImageFormat.Png);
					bmp.Save(tempFile2.Path, ImageFormat.Png);
				}

				using (var firstImage = PalasoImage.FromFile(tempFile1.Path))
				using (var secondImage = PalasoImage.FromFile(tempFile2.Path))
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(firstImage);

					var firstSavedOriginalPath = GetSavedOriginalImage(cropper).Path;
					var firstCroppingImage = GetCroppingImage(cropper);
					Assert.That(File.Exists(firstSavedOriginalPath), Is.True,
						"Sanity check: the first saved-original temp file should exist before reassignment");

					cropper.SetImage(secondImage);

					Assert.That(File.Exists(firstSavedOriginalPath), Is.False,
						"Temp file holding the first original should have been deleted on reassignment");
					Assert.That(() => firstCroppingImage.Width, Throws.TypeOf<ArgumentException>(),
						"Cropping image for the first original should have been disposed on reassignment");
				}
			}
		}

		[Test]
		public void Image_NewImageFailsToLoad_LeavesPreviousImageStateIntact()
		{
			using (var tempFile = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Png);

				using (var goodImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(goodImage);

					var savedOriginalImage = GetSavedOriginalImage(cropper);
					var croppingImage = GetCroppingImage(cropper);

					var unusableBitmap = new Bitmap(100, 80);
					unusableBitmap.Dispose();
					var unusableImage = PalasoImage.FromImage(unusableBitmap);

					// Assign the property rather than calling SetImage: SetImage reads
					// image.Image.RawFormat first, which would throw before the setter is entered.
					Assert.Throws<ArgumentException>(() => cropper.Image = unusableImage);

					Assert.That(GetSavedOriginalImage(cropper), Is.SameAs(savedOriginalImage),
						"A failed assignment must leave us cropping from the original we already saved");
					Assert.That(File.Exists(savedOriginalImage.Path), Is.True,
						"A failed assignment must not delete the temp file we are still cropping from");
					Assert.That(GetCroppingImage(cropper), Is.SameAs(croppingImage),
						"A failed assignment must leave the cropper on the image it was already showing");
					Assert.That(() => croppingImage.Width, Throws.Nothing,
						"A failed assignment must not dispose the cropping image still in use");
				}
			}
		}

		private static TempFile GetSavedOriginalImage(ImageCropper cropper)
		{
			return (TempFile)GetPrivateField(cropper, "_savedOriginalImage");
		}

		private static Image GetCroppingImage(ImageCropper cropper)
		{
			return (Image)GetPrivateField(cropper, "_croppingImage");
		}

		private static object GetPrivateField(ImageCropper cropper, string fieldName)
		{
			return typeof(ImageCropper)
				.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(cropper);
		}
	}
}
