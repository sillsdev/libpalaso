using SIL.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.String;
using static System.Text.RegularExpressions.RegexOptions;

namespace SIL.Core
{
	public static class HtmlUtils
	{
		public static readonly Regex RegexTarget =
			new Regex(@"target\s*=\s*(['""])(?<target>.*?)\1", IgnoreCase | Compiled);

		public static readonly Regex RegexHasBaseTarget =
			new Regex(@"<base\s+[^>]*\b" + RegexTarget, IgnoreCase | Compiled);

		public static readonly Regex RegexLinkTags =
			new Regex(@"<a\s+(?<attrs>[^>]+)>", IgnoreCase | Compiled);

		public static readonly Regex RegexHref =
			new Regex(@"href\s*=\s*(['""])(?<href>.+?)\1", IgnoreCase | Compiled);

		public static readonly Regex RegexHeadTag =
			new Regex(@"<head\b[^>]*>", IgnoreCase | Compiled);

		public static readonly Regex RegexLocalAssetReferences =
			new Regex(@"(?:src|href)\s*=\s*(['""])\s*(?:./)?(?<filename>[^/>\s\\]+?)\1",
			IgnoreCase | Compiled);

		public static readonly Regex RegexUriSchemeLike = new Regex(@"^[a-zA-Z][a-zA-Z0-9+\-.]*:",
			IgnoreCase | Compiled);

		/// <summary>
		/// Returns whether the given HTML has a head element that specifies a base target.
		/// </summary>
		public static bool HasBaseTarget(string html)
		{
			return RegexHasBaseTarget.IsMatch(html);
		}

		/// <summary>
		/// Evaluates the given HTML to determine if it is likely to have links that should
		/// probably open in the default browser but would instead open in the hosted browser
		/// control. If so, the HTML is modified to try to fix the problem. If running in Debug
		/// mode and a <paramref name="contextDescription"/> has been provided, an
		/// assertion is raised to alert the developer to this potential problem.
		/// </summary>
		/// <param name="html">The HTML to evaluate</param>
		/// <param name="contextDescription">A short bit of text that describes the context (e.g.,
		/// name of a dialog box) in which this is being used. This is used only in debug mode and
		/// is strictly for the benefit of developers. If omitted, no debug message will be
		/// displayed.</param>
		/// <param name="additionalDebugInfo">Any additional information to be appended to the
		/// debug message for the developer.</param>
		/// <returns>
		/// Updated HTML with proposed fixes, or <c>null></c> if no changes were made.
		/// </returns>
		public static string HandleMissingLinkTargets(string html, string contextDescription = null,
			string additionalDebugInfo = null)
		{
			if (IsNullOrEmpty(html) || HasBaseTarget(html))
				return null;

			bool hasExternalLink = false;

			StringBuilder sb = null;
			int lastIndex = 0;

			foreach (Match match in RegexLinkTags.Matches(html))
			{
				var attrGroup = match.Groups["attrs"];
				
				var hrefMatch = RegexHref.Match(attrGroup.Value);
				if (!hrefMatch.Success)
					continue; // Not really a link. Irrelevant.
				string href = hrefMatch.Groups["href"].Value;
				var hasTarget = RegexTarget.IsMatch(attrGroup.Value);

				if (IsExternalHref(href))
				{
					if (hasTarget)
					{
						// if even one external link has target, bail early
						return null;
					}
					hasExternalLink = true;
				}
				else if (!hasTarget)
				{
					// Most likely (unless we later find an external ref with an explicit target),
					// we're going to need to tweak the HTML to override targets for internal
					// links, so let's go ahead and start doing the replacements now.
					var insertIndex = attrGroup.Index + attrGroup.Length;
					if (sb == null)
						sb = new StringBuilder(html.Substring(0, insertIndex));
					else
						sb.Append(html, lastIndex, insertIndex - lastIndex);

					// Insert target="_self" right after the end of the attributes
					sb.Append(" target=\"_self\"");

					// The rest of the tag and anything following will be appended later.
					lastIndex = insertIndex;
				}
			}

			if (!hasExternalLink)
				return null;

			if (contextDescription != null)
			{
				Debug.Fail(Format(
					@"{0} HTML has links but has neither a base target in the head element nor explicit target attributes for any of the links.
Unless you override the default Navigating behavior, links may open directly in the {0} browser window and will probably not behave as expected.
Easy fix: consider adding `<base target=""_blank"" rel=""noopener noreferrer"">` inside your HTML head if your HTML does not use internal or mailto links.
If you do nothing, then a reasonable effort will be made to tweak the HTML to force external links to target the default browser.", contextDescription) +
					additionalDebugInfo);
			}

			if (sb != null)
			{
				if (lastIndex < html.Length)
					sb.Append(html, lastIndex, html.Length - lastIndex);
				html = sb.ToString();
			}

			var rewrittenHtml = InjectBaseTarget(html);

			return rewrittenHtml;
		}

		internal static bool IsExternalHref(string href)
		{
			if (href == null)
				return false;

			href = href.TrimStart();

			if (href.StartsWith("mailto:") ||
			    href.StartsWith("tel:"))
				return false;

			// An empty string gets turned into a link to the current base folder. The browser
			// control will happily open that in the current window, displaying a
			// Windows-Explorer-like view, but then the user has no way to get back, so that's
			// probably not what we want.
			return href.Length == 0 || 
					RegexUriSchemeLike.IsMatch(href) ||
			        href.StartsWith("www.") ||
					// Obviously, not an exhaustive list, but these are the most common and are
					// probably never going to be used as file extensions that would be likely to
					// be used in an HTML link.
					href.EndsWith(".com") ||
			        href.EndsWith(".org") ||
			        href.EndsWith(".net");
		}

		internal static string InjectBaseTarget(string html)
		{
			// In production use, we've already verified that it doesn't but for the sake of
			// unit tests, we check again. It's just too weird to have it inject another.
			if (RegexHasBaseTarget.IsMatch(html))
				return html;

			var match = RegexHeadTag.Match(html);
			if (match.Success)
			{
				return html.Insert(match.Index + match.Length,
					"<base target=\"_blank\" rel=\"noopener noreferrer\">");
			}

			// Fallback: inject entire head element before <body>
			var iInsertionPoint = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
			if (iInsertionPoint >= 0)
			{
				return html.Insert(iInsertionPoint,
					"<head><base target=\"_blank\" rel=\"noopener noreferrer\"></head>");
			}

			// No <head> or <body> found — we probably shouldn't be modifying this
			Debug.Fail("HTML has no <head> or <body> — skipping base target injection");
			return html;
		}


		/// <summary>
		/// Creates a temporary HTML file with the specified (modified) content and ensures that
		/// simple local dependencies (e.g., CSS, JavaScript, or image files) referenced in the
		/// HTML are copied to the  same temporary directory.
		/// </summary>
		/// <param name="modifiedHtml">The HTML content (presumably a modified version of the
		/// original) to be written to the temporary file.</param>
		/// <param name="origHtmlFilePath">The file path of the original HTML file, used to locate
		/// local dependencies.</param>
		/// <returns>A <see cref="TempFile"/> object representing the temporary HTML file created.
		/// Caller is responsible for disposing this object when done with it. Disposing will also
		/// remove the temporary folder with any copied asset files.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="modifiedHtml"/> or <paramref name="origHtmlFilePath"/> is null, empty, 
		/// or consists only of whitespace.
		/// </exception>
		public static TempFile CreatePatchedTempHtmlFile(string modifiedHtml,
			string origHtmlFilePath)
		{
			if (IsNullOrWhiteSpace(origHtmlFilePath))
				throw new ArgumentException("Must supply original HTML path", nameof(origHtmlFilePath));

			if (IsNullOrWhiteSpace(modifiedHtml))
				throw new ArgumentException("Must supply HTML contents", nameof(modifiedHtml));

			var originalFolder = Path.GetDirectoryName(origHtmlFilePath);
			var tempFile = TempFile.WithFilenameInTempFolder(Path.GetFileName(origHtmlFilePath));
			File.WriteAllText(tempFile.Path, modifiedHtml);
			
			var tempFolder = Path.GetDirectoryName(tempFile.Path);
			Debug.Assert(tempFolder != null, $"{nameof(TempFile.WithFilenameInTempFolder)} " +
				"should have created the temp file in a temp directory.");


			foreach (Match match in RegexLocalAssetReferences.Matches(modifiedHtml))
			{
				var fileName = match.Groups["filename"].Value;

				if (IsNullOrEmpty(fileName))
					continue;

				var sourceFile = originalFolder != null ? Path.Combine(originalFolder, fileName) : fileName;

				if (File.Exists(sourceFile))
				{
					var destFile = Path.Combine(tempFolder, fileName);
					File.Copy(sourceFile, destFile, overwrite: true);
				}
			}

			return tempFile;
		}
	}
}
