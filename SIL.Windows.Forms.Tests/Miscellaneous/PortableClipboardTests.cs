using SIL.IO;
using SIL.Windows.Forms.ImageToolbox;
using SIL.Windows.Forms.Miscellaneous;
using NUnit.Framework;

namespace SIL.Windows.Forms.Tests.Miscellaneous
{
	[TestFixture]
	[RequiresSTA] // or you get a ThreadStateException
	class PortableClipboardTests
	{
		private const string TestImageDir = "SIL.Windows.Forms.Tests/Miscellaneous/PortableClipboardTestImages";

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

		[Test]
		[Platform(Exclude = "Linux", Reason = "Linux code to copy image to clipboard not yet implemented.")]
		public void ClipboardRoundTripWorks_Bmp()
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

		[Test]
		[Platform(Exclude = "Linux", Reason = "Linux code to copy image to clipboard not yet implemented.")]
		public void ClipboardRoundTripWorks_GetsExistingMetadata()
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

		[Test]
		[Platform(Exclude = "Linux", Reason = "This requires a GTK message loop to run or something like this")]
		public void ClipboardRoundTripWorks_Text()
		{
			PortableClipboard.SetText("Hello world");
			Assert.That(PortableClipboard.ContainsText(), Is.True);
			Assert.That(PortableClipboard.GetText(), Is.EqualTo("Hello world"));
		}
	}
}
