using System.IO;
using System.Threading;
using SIL.IO;
using SIL.Windows.Forms.ImageToolbox;
using SIL.Windows.Forms.Miscellaneous;
using NUnit.Framework;
using System;

namespace SIL.Windows.Forms.Tests.Miscellaneous
{
	[TestFixture]
	[Apartment(ApartmentState.STA)] // or you get a ThreadStateException
	class PortableClipboardTests
	{
		private const string TestImageDir = "SIL.Windows.Forms.Tests/Miscellaneous/PortableClipboardTestImages";

		// This prevents flakiness (ExternalException) with tests running in parallel with utilizing the clipboard
		private static readonly object _lock = new object();

		[SetUp]
		public void Setup()
		{
		}

		private static string GetPathToImage(string requestedImage)
		{
			return FileLocationUtilities.GetFileDistributedWithApplication(TestImageDir, requestedImage);
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "Linux code to copy image to clipboard not yet implemented.")]
		public void ClipboardRoundTripWorks_Png()
		{
			lock (_lock)
			{
				var imagePath = GetPathToImage("LineSpacing.png");
				using (var image = PalasoImage.FromFileRobustly(imagePath))
				{
					PortableClipboard.CopyImageToClipboard(image);
					using (var resultingImage = PortableClipboard.GetImageFromClipboard())
					{
						// There is no working PalasoImage.Equals(), so just try a few properties
						Assert.AreEqual(image.FileName, resultingImage.FileName);
						Assert.AreEqual(image.Image.Size, resultingImage.Image.Size);
						Assert.AreEqual(image.Image.Flags, resultingImage.Image.Flags);
					}
				}
			}
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "Linux code to copy image to clipboard not yet implemented.")]
		public void ClipboardRoundTripWorks_Bmp()
		{
			lock (_lock)
			{
				var imagePath = GetPathToImage("PasteHS.bmp");
				using (var image = PalasoImage.FromFileRobustly(imagePath))
				{
					PortableClipboard.CopyImageToClipboard(image);
					using (var resultingImage = PortableClipboard.GetImageFromClipboard())
					{
						// There is no working PalasoImage.Equals(), so just try a few properties
						Assert.AreEqual(image.FileName, resultingImage.FileName);
						Assert.AreEqual(image.Image.Size, resultingImage.Image.Size);
						Assert.AreEqual(image.Image.Flags, resultingImage.Image.Flags);
					}
				}
			}
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "Linux code to copy image to clipboard not yet implemented.")]
		public void ClipboardRoundTripWorks_GetsExistingMetadata()
		{
			lock (_lock)
			{
				var imagePath = GetPathToImage("AOR_EAG00864.png");
				using (var image = PalasoImage.FromFileRobustly(imagePath))
				{
					var preCopyLicense = image.Metadata.License.Token;
					var preCopyCollectionUri = image.Metadata.CollectionUri;
					PortableClipboard.CopyImageToClipboard(image);
					using (var resultingImage = PortableClipboard.GetImageFromClipboard())
					{
						// Test that the same metadata came through
						Assert.IsTrue(resultingImage.Metadata.IsMinimallyComplete);
						Assert.AreEqual(preCopyLicense, resultingImage.Metadata.License.Token);
						Assert.AreEqual(preCopyCollectionUri, resultingImage.Metadata.CollectionUri);
						Assert.AreEqual(image.Image.Flags, resultingImage.Image.Flags);
					}
				}
			}
		}

		/// <summary>
		/// Pasting from the text of a copied path to an image file gets the image, and does not
		/// leave the file locked: the user may well want to move or delete it afterwards.
		/// </summary>
		[Test]
		[Platform(Exclude = "Linux", Reason = "This requires a GTK message loop to run or something like this")]
		public void GetImageFromClipboard_TextPathToImage_GetsImageAndDoesNotLockFile()
		{
			lock (_lock)
			{
				var tempPath = Path.Combine(Path.GetTempPath(), "PortableClipboardTests_" + Guid.NewGuid() + ".png");
				RobustFile.Copy(GetPathToImage("LineSpacing.png"), tempPath);
				try
				{
					PortableClipboard.SetText(tempPath);
					Assert.That(PortableClipboard.GetText(), Is.EqualTo(tempPath), "setup: the path should be on the clipboard");
					using (var resultingImage = PortableClipboard.GetImageFromClipboard())
					{
						Assert.That(resultingImage, Is.Not.Null);
						Assert.That(resultingImage.Image.Width, Is.GreaterThan(0));
					}

					Assert.DoesNotThrow(() => File.Delete(tempPath), "the pasted file should not still be locked");
					Assert.That(File.Exists(tempPath), Is.False);
				}
				finally
				{
					try
					{
						if (File.Exists(tempPath))
							File.Delete(tempPath);
					}
					catch (IOException)
					{
						// The test has already failed because the file is still locked; leave the
						// stray temp file rather than hide that failure behind a cleanup error.
					}
				}
			}
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "This requires a GTK message loop to run or something like this")]
		public void ClipboardRoundTripWorks_Text()
		{
			lock (_lock)
			{
				PortableClipboard.SetText("Hello world");
				Assert.That(PortableClipboard.ContainsText(), Is.True);
				Assert.That(PortableClipboard.GetText(), Is.EqualTo("Hello world"));
			}
		}
	}
}
