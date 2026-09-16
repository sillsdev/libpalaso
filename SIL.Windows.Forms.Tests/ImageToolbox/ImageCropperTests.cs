using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;
using SIL.IO;
using SIL.Reflection;
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
						// The crop is read back from the PNG temp file, so it is never in the
						// source's format. With the grips unmoved it is the whole of that file and
						// reports Png; a real crop reports MemoryBmp.
						Assert.That(result.RawFormat.Guid, Is.EqualTo(ImageFormat.Png.Guid));
						using (var stream = new MemoryStream())
							Assert.That(() => result.Save(stream, ImageFormat.Png), Throws.Nothing);
					}
				}
			}
		}

		[Test]
		public void GetCroppedImage_GripMoved_ReturnsDetachedBitmap()
		{
			// The whole-image case (unmoved grips) and the partial case take different paths
			// through GDI+, so cover both.
			using (var tempFile = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Jpeg);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(palasoImage);
					MoveRightGripIn(cropper);

					using (var result = cropper.GetCroppedImage())
					{
						Assert.That(result, Is.Not.Null);
						Assert.That(result.RawFormat.Guid, Is.EqualTo(ImageFormat.MemoryBmp.Guid));
						using (var stream = new MemoryStream())
							Assert.That(() => result.Save(stream, ImageFormat.Png), Throws.Nothing);
					}
				}
			}
		}

		[Test]
		public void GetImage_NothingCropped_ReturnsImageUntouched()
		{
			// Going in and back out of the Crop tab without cropping used to replace the image with
			// a copy of itself round-tripped through the PNG temp file, losing the original format.
			using (var tempFile = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Jpeg);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(palasoImage);
					var imageBefore = palasoImage.Image;
					var rawFormatBefore = imageBefore.RawFormat.Guid;

					var result = cropper.GetImage();

					Assert.That(result, Is.SameAs(palasoImage));
					Assert.That(result.Image, Is.SameAs(imageBefore), "The image itself should not have been replaced");
					Assert.That(result.Image.RawFormat.Guid, Is.EqualTo(rawFormatBefore));
				}
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetImage_ResultOutlivesCropper_SavedOriginalTempFileIsDeleted(bool moveGrip)
		{
			// A result that still shares the saved-original's file-backed data keeps that temp file
			// locked, so the delete in Dispose fails and the file is leaked.
			using (var tempFile = TempFile.WithExtension(".jpg"))
			{
				using (var bmp = new Bitmap(100, 80))
					bmp.Save(tempFile.Path, ImageFormat.Jpeg);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				{
					string savedOriginalPath;
					using (var cropper = new ImageCropper { Size = new Size(400, 300) })
					{
						cropper.SetImage(palasoImage);
						savedOriginalPath = GetSavedOriginalImage(cropper).Path;
						if (moveGrip)
							MoveRightGripIn(cropper);
						Assert.That(cropper.GetImage(), Is.Not.Null);
					}

					Assert.That(File.Exists(savedOriginalPath), Is.False,
						"Saved-original temp file should have been deleted even though the result is still alive");
				}
			}
		}

		[Test]
		public void GetCroppedImage_OneBitPng_PreservesPixelFormat()
		{
			// Copying the crop into a new Bitmap to detach it would widen this to 32bpp, undoing the
			// bit-depth preservation PalasoImage.SaveImageSafely goes out of its way to get (BL-2841).
			using (var tempFile = TempFile.WithExtension(".png"))
			{
				using (var bmp = new Bitmap(100, 80, PixelFormat.Format1bppIndexed))
					bmp.Save(tempFile.Path, ImageFormat.Png);

				using (var palasoImage = PalasoImage.FromFile(tempFile.Path))
				using (var cropper = new ImageCropper { Size = new Size(400, 300) })
				{
					cropper.SetImage(palasoImage);

					using (var result = cropper.GetCroppedImage())
						Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format1bppIndexed));
				}
			}
		}

		[Test]
		public void GetCroppedImage_ImageSetViaPropertyDirectly_ReturnsUsableBitmap()
		{
			// The Image setter has to leave the cropper fully usable on its own. SetImage once did
			// extra setup that GetCroppedImage depended on, so assigning the property directly
			// produced a crop that threw.
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
						MoveRightGripIn(firstCropper); // otherwise GetImage has nothing to do
						cropped = firstCropper.GetImage();
					}
					Assert.That(cropped, Is.Not.Null);

					using (var secondCropper = new ImageCropper { Size = new Size(400, 300) })
						Assert.That(() => secondCropper.SetImage(cropped), Throws.Nothing);
				}
			}
		}

		/// <summary>
		/// Drags the right grip inward, so the selection is no longer the whole image.
		/// </summary>
		private static void MoveRightGripIn(ImageCropper cropper)
		{
			var rightGrip = (Grip)ReflectionHelper.GetField(cropper, "_rightGrip");
			rightGrip.Value -= 20;
		}

		private static TempFile GetSavedOriginalImage(ImageCropper cropper)
		{
			return (TempFile)ReflectionHelper.GetField(cropper, "_savedOriginalImage");
		}

		private static Image GetCroppingImage(ImageCropper cropper)
		{
			return (Image)ReflectionHelper.GetField(cropper, "_croppingImage");
		}
	}
}
