using NUnit.Framework;
using System.IO;
using SIL.Core;
using SIL.IO;
using static System.IO.Path;
using static System.StringComparison;

namespace SIL.Tests.Html
{
	[TestFixture]
	public class HtmlUtilsTests
	{
		private const string htmlWithNoHead = @"<html>
<body>
	<h3>Stuff</h3>
	<p>Here is something you need to know.</p>
</body>
</html>";

		private const string htmlWithSelfClosingEmptyHead = @"<html>
<head/>
<body>
	<h3>Stuff</h3>
	<p>Here is something you need to know.</p>
</body>
</html>";

		private const string htmlWithNonEmptyHeadButNoTarget = @"<html>
<head><meta charset='UTF-8' /></head>
<body>
	<h3>Stuff</h3>
	<p>Here is something you need to know.</p>
</body>
</html>";

		[TestCase(@"<html>
<head><base target=""_blank"" rel=""noopener noreferrer""></head>
<body>
	<p>Good</p>
</body>
</html>")]
		[TestCase(@"<html>
<head><base target='_blank'></head>
<body>
	<p>Stuff.</p>
</body>
</html>")]
		[TestCase(@"<html>
<head><base target=""_self""></head>
<body>
	<p>Stuff.</p>
</body>
</html>")]
		public void HasBaseTarget_Yes_ReturnsTrue(string html)
		{
			Assert.That(HtmlUtils.HasBaseTarget(html), Is.True);
		}

		[TestCase(htmlWithNoHead)]
		[TestCase(htmlWithSelfClosingEmptyHead)]
		[TestCase(htmlWithNonEmptyHeadButNoTarget)]
		public void HasBaseTarget_No_ReturnsFalse(string html)
		{
			Assert.That(HtmlUtils.HasBaseTarget(html), Is.False);
		}

		[TestCase("")]
		[TestCase(null)]
		public void HandleMissingLinkTargets_EmptyHtml_ReturnsNull(string html)
		{
			var result = HtmlUtils.HandleMissingLinkTargets(html);
			Assert.IsNull(result);
		}

		[TestCase(htmlWithNoHead)]
		[TestCase(htmlWithSelfClosingEmptyHead)]
		[TestCase(htmlWithNonEmptyHeadButNoTarget)]
		public void HandleMissingLinkTargets_HtmlWithNoLinks_ReturnsNull(string html)
		{
			var result = HtmlUtils.HandleMissingLinkTargets(html);
			Assert.IsNull(result);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void HandleMissingLinkTargets_ExternalLinkWithoutTarget_AddsBaseTarget(
			bool useDoubleQuotes)
		{
			var html = @"<html>
<head></head>
<body>
	<a href='http://example.com'>External</a>
</body>
</html>";
			if (useDoubleQuotes)
				html = html.Replace("'", "\"");
			var origBody = html.Substring(html.IndexOf("<body>", Ordinal));
			var result = HtmlUtils.HandleMissingLinkTargets(html);
			Assert.That(HtmlUtils.HasBaseTarget(result), Is.True);
			Assert.That(result.Substring(result.IndexOf("<body>", Ordinal)),
				Is.EqualTo(origBody));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void HandleMissingLinkTargets_ExternalLinkWithTarget_ReturnsNull(
			bool useDoubleQuotes)
		{
			var html = @"<html>
<head></head>
<body>
	<a href='https://example.com' target='_blank'>External</a>
</body>
</html>";
			if (useDoubleQuotes)
				html = html.Replace("'", "\"");
			var result = HtmlUtils.HandleMissingLinkTargets(html);
			Assert.That(result, Is.Null, "No need to modify; explicit target already present");
		}

		[TestCase(true)]
		[TestCase(false)]
		public void HandleMissingLinkTargets_OnlyInternalLinks_ReturnsNull(
			bool useDoubleQuotes)
		{
			var html = @"<html>
<head></head>
<body>
	<a href='#section1'>Internal</a>
	<a href='mailto:someone@example.com'>Email</a>
	<h4 id='section1'>This is the internal section</h4>
	<p>You jumped here using an internal anchor link.</p>
</body>
</html>";
			if (useDoubleQuotes)
				html = html.Replace("'", "\"");
			var result = HtmlUtils.HandleMissingLinkTargets(html);
			Assert.That(result, Is.Null, "No need to modify; all links are internal/special");
		}

		[Test]
		public void HandleMissingLinkTargets_InternalAnchor_GetsTargetSelf()
		{
			var html = @"<html><head></head><body><a href=""#section'1'"">Jump</a> and 
				<a href='http://example.com'>Go</a>!</body></html>";

			var origTail = html.Substring(html.IndexOf("Jump</a> and", Ordinal));

			var result = HtmlUtils.HandleMissingLinkTargets(html);

			Assert.That(HtmlUtils.HasBaseTarget(result), Is.True);
			Assert.That(result, Does.Contain(@"<a href=""#section'1'"" target=""_self"">Jump</a>"));
			Assert.That(result, Does.EndWith(origTail));
		}

		[Test]
		public void HandleMissingLinkTargets_HrefContainsExternalLinkWithInternalQuote_OnlyBaseTargetAdded()
		{
			var html = @"<html><body>
<a href=""https://example.com/page?title=John'sBook"">Link</a>
</body></html>";

			var origBody = html.Substring(html.IndexOf("<body>", Ordinal));
			var result = HtmlUtils.HandleMissingLinkTargets(html);
			Assert.That(HtmlUtils.HasBaseTarget(result), Is.True);
			Assert.That(result.Substring(result.IndexOf("<body>", Ordinal)),
				Is.EqualTo(origBody));
		}

		/// <summary>
		/// Not really sure how much we care what this method does with malformed HTML.
		/// </summary>
		[Test]
		public void HandleMissingLinkTargets_MalformedHref_ReturnsNull()
		{
			var html = @"<html><body>
<a href=""https://example.com'>Link</a>
</body></html>";

			Assert.That(HtmlUtils.HandleMissingLinkTargets(html), Is.Null);
		}

		[Test]
		public void HandleMissingLinkTargets_Mailto_GetsTargetSelf()
		{
			var html = @"<html><head></head><body><a href='http://example.com'>Go</a> here to 
				<a href='mailto:someone@example.com'>Email</a> someone.</body></html>";

			int bodyStart = html.IndexOf("<body>", Ordinal);
			var origBodyStart = html.Substring(bodyStart,
				html.IndexOf("<a href='mailto", Ordinal) - bodyStart);

			var result = HtmlUtils.HandleMissingLinkTargets(html);

			Assert.That(HtmlUtils.HasBaseTarget(result), Is.True);
			Assert.That(result, Does.Contain(origBodyStart));
			Assert.That(result, Does.EndWith("<a href='mailto:someone@example.com' " +
			                                 @"target=""_self"">Email</a> someone.</body></html>"));
		}

		[TestCase(@"href=""#internal"" target=""_self""")]
		[TestCase(@"target=""_self"" href=""#internal""")]
		[TestCase(@"target='_self' href=""#internal""")]
		[TestCase(@"href='#internal' target=""_self""")]
		public void HandleMissingLinkTargets_InternalLink_WithTargetAlready_OnlyBaseTargetAdded(
			string internalLinkAttributes)
		{
			var html = $"<html><head></head><body><a {internalLinkAttributes}>Stay</a>" +
			           @"alert as you <a href=""www.example.com"">walk</a>.</body></html>";

			var origBody = html.Substring(html.IndexOf("<body>", Ordinal));
			var result = HtmlUtils.HandleMissingLinkTargets(html);
			Assert.That(HtmlUtils.HasBaseTarget(result), Is.True);
			Assert.That(result.Substring(result.IndexOf("<body>", Ordinal)),
				Is.EqualTo(origBody));
		}

		[TestCase("www.example.com")]
		[TestCase("http://www.example.com")]
		[TestCase("https://www.example.com")]
		public void IsExternalHref_IsExternal_ReturnsTrue(string href)
		{
			Assert.That(HtmlUtils.IsExternalHref(href), Is.True);
		}

		[TestCase("#internal")]
		[TestCase("mailto:someone@example.com")]
		[TestCase("tel:8008008000")]
		[TestCase("")]
		[TestCase(null)]
		public void IsExternalHref_IsNotExternal_ReturnsFalse(string href)
		{
			Assert.That(HtmlUtils.IsExternalHref(href), Is.False);
		}

		[TestCase(htmlWithNoHead)]
		[TestCase(htmlWithSelfClosingEmptyHead)]
		[TestCase(htmlWithNonEmptyHeadButNoTarget)]
		public void InjectBaseTarget_Missing_AddsHeadElementWithBaseTarget(string html)
		{
			var result = HtmlUtils.InjectBaseTarget(html);
			Assert.That(HtmlUtils.HasBaseTarget(result), Is.True);
		}

		[TestCase("")]
		[TestCase(@"<p><a name='gumby'/></p>")]
		[TestCase(@"<p><a href='https://www.example.com'/></p>")]
		public void InjectBaseTarget_AlreadyHasBaseTarget_ReturnsOriginalHtml(string body)
		{
			var html = $"<html><head><base target='_blank'></head><body>{body}</body></html>";
			var result = HtmlUtils.InjectBaseTarget(html);
			Assert.That(result, Is.EqualTo(html));
		}
	}

	[TestFixture]
	public class HtmlUtilsCreatePatchedTempHtmlFileTests
	{
		private string _testDir;
		private TempFile _modifiedHtml;
		private string _htmlPath;

		[SetUp]
		public void Setup()
		{
			_modifiedHtml = TempFile.WithFilenameInTempFolder("about.html");
			_testDir = GetDirectoryName(_modifiedHtml.Path);
			_htmlPath = _modifiedHtml.Path;
		}

		[TearDown]
		public void Teardown()
		{
			_modifiedHtml.Dispose();
		}

		[TestCase("")]
		[TestCase("./")]
		[TestCase(" ")]
		public void SimpleCssLink_AssetCopied(string prefix)
		{
			const string cssName = "style.css";
			var cssPath = Combine(_testDir, cssName);
			File.WriteAllText(cssPath, "body { background: black; }");

			var html = $@"<html><head>
<link rel=""stylesheet"" href=""{prefix}{cssName}""></head><body>hello</body></html>";
			File.WriteAllText(_htmlPath, html);

			using var tempFile = HtmlUtils.CreatePatchedTempHtmlFile(html, _htmlPath);

			var tempDir = GetDirectoryName(tempFile.Path);
			Assert.That(tempFile.Path, Does.Exist);
			Assert.That(Combine(tempDir, cssName), Does.Exist);
		}

		[TestCase("")]
		[TestCase("./")]
		[TestCase(" ")]
		[TestCase(" ./")]
		public void MultipleSimpleAssets_AllCopied(string prefix)
		{
			const string cssName = "style.css";
			const string jsName = "script.js";
			const string logoName = "hawai'i.png";
			File.WriteAllText(Combine(_testDir, cssName), "css");
			File.WriteAllText(Combine(_testDir, jsName), "js");
			File.WriteAllText(Combine(_testDir, logoName), "png");

			var html = $@"<html><head>
<link rel=""stylesheet"" href = ""{prefix}{cssName}"">
<script src=""{prefix}{jsName}""></script>
</head><body><img src=""{prefix}{logoName}""></body></html>";
			File.WriteAllText(_htmlPath, html);

			using var tempFile = HtmlUtils.CreatePatchedTempHtmlFile(html, _htmlPath);
			var tempDir = GetDirectoryName(tempFile.Path);

			Assert.That(File.Exists(Combine(tempDir, cssName)), Is.True);
			Assert.That(File.Exists(Combine(tempDir, jsName)), Is.True);
			Assert.That(File.Exists(Combine(tempDir, logoName)), Is.True);
		}

		[Test]
		public void ExternalLinks_Ignored()
		{
			const string html = @"<html><head>
<link rel='stylesheet' href=""https://example.com/style.css"">
</head><body>hello</body></html>";
			File.WriteAllText(_htmlPath, html);

			using var tempFile = HtmlUtils.CreatePatchedTempHtmlFile(html, _htmlPath);

			var tempDir = GetDirectoryName(tempFile.Path);
			Assert.That(File.Exists(tempFile.Path), Is.True);
			Assert.That(Directory.GetFiles(tempDir).Length, Is.EqualTo(1),
				"Should not attempt to copy external resources.");
		}

		/// <summary>
		/// Since we're purposefully trying to keep things simple by ignoring relative/
		/// subdirectory assets, this test ensures that we don't attempt to copy them.
		/// </summary>
		[TestCase(@"\")]
		[TestCase("/")]
		public void SubdirectoryAsset_NotCopied(string slash)
		{
			var assetsDir = Combine(_testDir, "assets");
			Directory.CreateDirectory(assetsDir);
			File.WriteAllText(Combine(assetsDir, "style.css"), "should not copy");

			var html = $@"<html><head>
<link rel=""stylesheet"" href=""assets{slash}style.css"">
</head><body>hello</body></html>";
			File.WriteAllText(_htmlPath, html);

			using var tempFile = HtmlUtils.CreatePatchedTempHtmlFile(html, _htmlPath);

			var tempDir = GetDirectoryName(tempFile.Path);
			Assert.That(Directory.GetFiles(tempDir).Length, Is.EqualTo(1),
				"Subdirectory assets should not be copied.");
		}
	}
}
