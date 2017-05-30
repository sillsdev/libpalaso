using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using MarkdownDeep;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks
{
	/// <summary>
	/// Given a markdown file, this class will generate the documentation artifacts required to make a cross platform release.
	/// </summary>
	/* Example uses:
	 * <GenerateReleaseArtifacts MarkdownFile="$(RootDir)\src\Installer\ReleaseNotes.md" StampMarkdown="false"
	 *   VersionNumber="4.1" ProductName="bloom" Stability="stable" Urgency="medium"
	 *   DebianChangeLog="$(RootDir)\debian\changelog" ChangeLogAuthorInfo="Stephen McConnel &lt;stephen_mcconnel@sil.org&gt;" />
	 *
	 * This uses the MarkdownFile and VersionNumber to generate a changelog entry in the DebianChangeLog file giving the author credit to
	 * Steve.
	 *
	 * <GenerateReleaseArtifacts MarkdownFile="$(RootDir)\src\Installer\ReleaseNotes.md" StampMarkdown="true"
	 *   HtmlFile="$(RootDir)\src\Installer\$(UploadFolder).htm" VersionNumber="$(Version)" ProductName="flexbridge"
	 *   DebianChangeLog="$(RootDir)\debian\changelog" ChangeLogAuthorInfo="Jason Naylor &lt;jason_naylor@sil.org&gt;" />
	 *
	 * This stamps the MarkDownFile with the version numbers (replacing the first line with '## VERSION_NUMBER DATE') and Generates a .htm file
	 * by creating a new file or by replacing the <div class='releasenotes'> in an existing .htm with a generated one, and updates the changelog
	 * with the new entry
	 */
	public class GenerateReleaseArtifacts : Task
	{
		[Required]
		public string MarkdownFile { get; set; }

		[Required]
		public bool StampMarkdownFile { get; set; }

		[Required]
		public string VersionNumber { get; set; }

		[Required]
		public string ProductName { get; set; }

		public string HtmlFile { get; set; }

		public string Stability { get; set; }

		public string Urgency { get; set; }

		/// <summary>
		/// Name and e-mail string
		/// </summary>
		public string ChangelogAuthorInfo { get; set; }

		[Required]
		public string DebianChangelog { get; set; }

		public override bool Execute()
		{
			return StampMarkdownFileWithVersion() && CreateHtmFromMarkdownFile() && UpdateDebianChangelog();
		}

		internal bool UpdateDebianChangelog()
		{
			if(string.IsNullOrEmpty(Stability))
				Stability = "unstable";
			if(string.IsNullOrEmpty(Urgency))
				Urgency = "low";
			var oldChangeLog = Path.ChangeExtension(DebianChangelog, ".old");
			File.Delete(oldChangeLog);
			File.Move(DebianChangelog, oldChangeLog);
			WriteMostRecentMarkdownEntryToChangelog();
			File.AppendAllLines(DebianChangelog, File.ReadAllLines(oldChangeLog));
			return true;
		}

		private void WriteMostRecentMarkdownEntryToChangelog()
		{
			if(string.IsNullOrEmpty(ChangelogAuthorInfo))
			{
				ChangelogAuthorInfo = "Annonymous <annonymous@sil.org>";
			}
			var markdownLines = File.ReadAllLines(MarkdownFile);
			var newEntryLines = new List<string>();
			// Write out the first markdown line as the changelog version line
			newEntryLines.Add(string.Format("{0} ({1}) {2}; urgency={3}", ProductName, VersionNumber, Stability, Urgency));
			newEntryLines.Add(string.Empty);
			for(var i = 1; i < markdownLines.Length; ++i)
			{
				if(markdownLines[i].StartsWith("##"))
					break;
				ConvertMarkdownLineToChangelogLine(markdownLines[i], newEntryLines);
			}
			newEntryLines.Add(string.Empty);
			// The debian changelog needs RFC 2822 format (Thu, 15 Oct 2015 08:25:16 -0500), which is not quite what .NET can provide
			var debianDate = string.Format("{0:ddd, dd MMM yyyy HH'|'mm'|'ss zzz}", DateTime.Now).Replace(":", "").Replace('|', ':');
			newEntryLines.Add(string.Format(" -- {0}  {1}", ChangelogAuthorInfo, debianDate));
			newEntryLines.Add(string.Empty);
			File.AppendAllLines(DebianChangelog, newEntryLines);
		}

		private static void ConvertMarkdownLineToChangelogLine(string markdownLine, List<string> newEntryLines)
		{
			switch(markdownLine[0])
			{
				case '*':
				case '-':
				case '+':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '0': // treat all unordered and ordered list items the same in the changelog
					newEntryLines.Add(string.Format("  *{0}", markdownLine.Substring(1)));
					break;
				case ' ': // Handle lists within lists, only second level items are handled, any further indentation is currently ignored
					newEntryLines.Add(string.Format("    *{0}", markdownLine.Trim().Substring(1).Trim('.')));
				break;
			}
		}

		/// <summary>
		/// Replaces the first line in a Release.md with the version and date
		/// (Assumes that a temporary line is currently at the top: e.g. ## DEV_VERSION_NUMBER: DEV_RELEASE_DATE
		/// </summary>
		/// <returns></returns>
		internal bool StampMarkdownFileWithVersion()
		{
			if(StampMarkdownFile)
			{
				var markdownLines = File.ReadAllLines(MarkdownFile);
				markdownLines[0] = string.Format("## {0} {1:dd/MMM/yyyy}", VersionNumber, DateTime.Today);
				File.WriteAllLines(MarkdownFile, markdownLines);
			}
			return true;
		}

		/// <summary>
		/// Creates an htm file given a markdown file.
		/// </summary>
		/// <remarks>internal for testing</remarks>
		internal bool CreateHtmFromMarkdownFile()
		{
			if(!File.Exists(MarkdownFile))
			{
				Log.LogError(string.Format("The given markdown file ({0}) does not exist.", MarkdownFile));
				return false;
			}
			var markDownTransformer = new Markdown();
			try
			{
				var markdownHtml = markDownTransformer.Transform(File.ReadAllText(MarkdownFile));
				if(File.Exists(HtmlFile))
				{
					var htmlDoc = XDocument.Load(HtmlFile);
					var releaseNotesElement = htmlDoc.XPathSelectElement("//*[@class='releasenotes']");
					if(releaseNotesElement != null)
					{
						releaseNotesElement.RemoveNodes();
						var mdDocument = XDocument.Parse(string.Format("<div>{0}</div>", markdownHtml));
						// ReSharper disable once PossibleNullReferenceException - Will either throw or work
						releaseNotesElement.Add(mdDocument.Root.Elements());
						htmlDoc.Save(HtmlFile);
					}
				}
				else
				{
					WriteBasicHtmlFromMarkdown(markdownHtml);
				}
				return true;
			}
			catch(Exception e)
			{
				Log.LogErrorFromException(e, true);
				return false;
			}
		}

		private void WriteBasicHtmlFromMarkdown(string markdownHtml)
		{
			File.WriteAllText(HtmlFile, string.Format("<html><div class='releasenotes'>{0}</div></html>",
				markdownHtml));
		}
	}
}
