using Markdig;
using NUnit.Framework;
using SIL.Windows.Forms.ReleaseNotes;

namespace SIL.Windows.Forms.Tests.ReleaseNotes
{
	/// <summary>
	/// Pins down Markdig's rendering behavior for the exact pipeline ShowReleaseNotesDialog uses,
	/// so a Markdig version bump that changes output is caught here instead of silently shipping.
	/// </summary>
	[TestFixture]
	public class ShowReleaseNotesDialogMarkdownTests
	{
		private static string ToHtml(string markdown) => Markdown.ToHtml(markdown, ShowReleaseNotesDialog.Pipeline);

		[Test]
		public void ToHtml_Heading_RendersHeadingTag()
		{
			// UseAdvancedExtensions() includes auto-identifiers, hence the id attribute.
			Assert.That(ToHtml("# Release 5.0"), Is.EqualTo("<h1 id=\"release-5.0\">Release 5.0</h1>\n"));
		}

		[Test]
		public void ToHtml_BoldAndItalic_RendersStrongAndEmTags()
		{
			var html = ToHtml("**bold** and *italic*");

			Assert.That(html, Does.Contain("<strong>bold</strong>"));
			Assert.That(html, Does.Contain("<em>italic</em>"));
		}

		[Test]
		public void ToHtml_BulletList_RendersUlAndLiTags()
		{
			var html = ToHtml("- first\n- second");

			Assert.That(html, Does.Contain("<ul>"));
			Assert.That(html, Does.Contain("<li>first</li>"));
			Assert.That(html, Does.Contain("<li>second</li>"));
		}

		[Test]
		public void ToHtml_Link_RendersAnchorTag()
		{
			var html = ToHtml("[SIL](https://sil.org)");

			Assert.That(html, Does.Contain("<a href=\"https://sil.org\">SIL</a>"));
		}

		[Test]
		public void ToHtml_PipeTable_RendersTableTags()
		{
			// Pipe tables are only enabled via UseAdvancedExtensions(), so this specifically
			// exercises that part of the pipeline, not just base CommonMark support.
			var html = ToHtml("| A | B |\n|---|---|\n| 1 | 2 |");

			Assert.That(html, Does.Contain("<table>"));
			Assert.That(html, Does.Contain("<td>1</td>"));
			Assert.That(html, Does.Contain("<td>2</td>"));
		}

		[Test]
		public void ToHtml_Autolink_RendersAnchorTag()
		{
			// Bare-URL autolinking is also an advanced-extension behavior, not base CommonMark.
			var html = ToHtml("See https://sil.org for details.");

			Assert.That(html, Does.Contain("<a href=\"https://sil.org\">https://sil.org</a>"));
		}
	}
}
