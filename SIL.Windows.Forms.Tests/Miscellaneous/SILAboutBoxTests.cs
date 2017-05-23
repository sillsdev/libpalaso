using NUnit.Framework;
using SIL.Windows.Forms.Miscellaneous;

namespace SIL.Windows.Forms.Tests.Miscellaneous
{
	[TestFixture]
	public class SILAboutBoxTests
	{
		[Test]
		public void ValidateHtml_MissingBodyTagReturnsOriginalHtml()
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
		public void ValidateHtml_ValidHtmlNotOverriden()
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
		public void ValidateHtml_InvalidHtml_ReturnsOriginalHtml()
		{
			// returning the original input string even though ValidateHtml fails
			// maintains backwards compatibility, but a Debug.Fail makes sure
			// that programmers become aware of the problem.
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

		[Test]
		public void ValidateHtml_ValidHtmlNoMetaTag_GetsOne()
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
	}
}
