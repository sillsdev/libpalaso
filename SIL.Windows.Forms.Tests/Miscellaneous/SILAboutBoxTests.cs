using System;
using NUnit.Framework;
using SIL.Windows.Forms.Miscellaneous;

namespace SIL.Windows.Forms.Tests.Miscellaneous
{
	/// <summary>
	/// The SILAboutBox class has different behavior depending on whether it is run
	/// in Debug or Release mode, therefore different tests depending on which version is running.
	/// </summary>
	[TestFixture]
	public class SILAboutBoxTests
	{

#if DEBUG
		[Test]
		[ExpectedException(ExpectedException = typeof(ApplicationException),
			ExpectedMessage = "Html has no head or body and needs a charset meta tag.")]
		public void ValidateHtml_Debug_MissingBodyTag_ThrowsException()
		{
			const string htmlInput =
				@"<html>
					<meta charset='UTF-16' />
					<some>stuff</some>
				</html>";
			var result = SILAboutBox.ValidateHtml(htmlInput);
			Assert.That(result, Is.EqualTo(htmlInput));
		}

		[Test]
		[ExpectedException(ExpectedException = typeof(ApplicationException),
			ExpectedMessage = "Start tag <head> was not found at line 3")]
		public void ValidateHtml_Debug_InvalidHtml_ThrowsException()
		{
			const string htmlInput =
				@"<html>
					ad>
					</head>
					<body>
						<some>stuff</some>
					</body>
				</html>";
			var result = SILAboutBox.ValidateHtml(htmlInput);
			Assert.That(result, Is.EqualTo(htmlInput));
		}
#else
		[Test]
		public void ValidateHtml_Release_MissingBodyTagReturnsOriginalHtml()
		{
			const string htmlInput =
				@"<html>
					<meta charset='UTF-16' />
					<some>stuff</some>
				</html>";
			var result = SILAboutBox.ValidateHtml(htmlInput);
			Assert.That(result, Is.EqualTo(htmlInput));
		}

		[Test]
		public void ValidateHtml_Release_InvalidHtml_ReturnsOriginalHtml()
		{
			const string htmlInput =
				@"<html>
					ad>
					</head>
					<body>
						<some>stuff</some>
					</body>
				</html>";
			var result = SILAboutBox.ValidateHtml(htmlInput);
			Assert.That(result, Is.EqualTo(htmlInput));
		}
#endif

		// These tests should behave the same whether Debug or Release

		[Test]
		public void ValidateHtml_ValidHtml_CharsetNotOverriden()
		{
			const string htmlInput =
				@"<html>
					<head>
						<meta charset='UTF-16' />
					</head>
					<body>
						<some>stuff</some>
					</body>
				</html>";
			var result = SILAboutBox.ValidateHtml(htmlInput);
			Assert.That(result, Is.StringContaining("charset='UTF-16'"));
		}

		[Test]
		public void ValidateHtml_ValidHtmlNoHead_GetsHeadAndMeta()
		{
			const string htmlInput =
				@"<html>
					<body>
						<some>stuff</some>
					</body>
				</html>";
			var result = SILAboutBox.ValidateHtml(htmlInput);
			Assert.That(result, Is.StringContaining("<head><meta charset=\"UTF-8\"></head>"));
		}

		[Test]
		public void ValidateHtml_ValidHtmlMetaTagHasNoCharsetAttribute_GetsOne()
		{
			const string htmlInput =
				@"<html>
					<head>
						<meta content='text/html' />
					</head>
					<body>
						<some>stuff</some>
					</body>
				</html>";
			var result = SILAboutBox.ValidateHtml(htmlInput);
			var trimmedResult = result.Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty);
			Assert.That(trimmedResult, Is.StringContaining("<head><meta content='text/html' charset=\"UTF-8\"></head>"));
		}
	}
}
