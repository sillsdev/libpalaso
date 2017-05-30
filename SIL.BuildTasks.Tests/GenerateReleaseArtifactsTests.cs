using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using NUnit.Framework;
using SIL.TestUtilities;

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	internal class GenerateReleaseArtifactsTests
	{
		#region TestHelper classes

		private sealed class MockEngine : IBuildEngine
		{
			public List<string> LoggedMessages = new List<string>();

			public void LogErrorEvent(BuildErrorEventArgs e)
			{
				LoggedMessages.Add(e.Message);
			}

			public void LogWarningEvent(BuildWarningEventArgs e)
			{
				LoggedMessages.Add(e.Message);
			}

			public void LogMessageEvent(BuildMessageEventArgs e)
			{
				LoggedMessages.Add(e.Message);
			}

			public void LogCustomEvent(CustomBuildEventArgs e)
			{
				LoggedMessages.Add(e.Message);
			}

			public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
				IDictionary targetOutputs)
			{
				throw new NotImplementedException();
			}

			public bool ContinueOnError { get; private set; }
			public int LineNumberOfTaskNode { get; private set; }
			public int ColumnNumberOfTaskNode { get; private set; }
			public string ProjectFileOfTaskNode { get; private set; }
		}

		/// <summary>
		/// This class is implemented to avoid a dependency on Palaso (which isn't strictly circular, but sure feels like it)
		/// The TempFile class that lives in SIL.IO is a more robust and generally preferred implementation.
		/// </summary>
		private sealed class TwoTempFilesForTest : IDisposable
		{
			public string FirstFile { get; set; }
			public string SecondFile;

			public TwoTempFilesForTest(string firstFile, string secondFile)
			{
				FirstFile = firstFile;
				SecondFile = secondFile;
			}
			public void Dispose()
			{
				try
				{
					if(File.Exists(FirstFile))
					{
						File.Delete(FirstFile);
					}
					if(File.Exists(SecondFile))
					{
						File.Delete(SecondFile);
					}
				}
				catch(Exception)
				{
					// We try to clean up after ourselves, but we aren't going to fail tests if we couldn't
				}
			}
		}
#endregion

		[Test]
		public void MissingMarkdownReturnsFalse()
		{
			var mockEngine = new MockEngine();
			var testMarkdown = new GenerateReleaseArtifacts();
			testMarkdown.MarkdownFile = Path.GetRandomFileName();
			testMarkdown.BuildEngine = mockEngine;
			Assert.That(testMarkdown.CreateHtmFromMarkdownFile(), Is.False);
			Assert.That(mockEngine.LoggedMessages[0], Is.StringMatching("The given markdown file .* does not exist\\."));
		}

		[Test]
		public void SimpleMdResultsInSimpleHtml()
		{
			var testMarkdown = new GenerateReleaseArtifacts();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
				File.WriteAllLines(filesForTest.FirstFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				testMarkdown.MarkdownFile = filesForTest.FirstFile;
				testMarkdown.HtmlFile = filesForTest.SecondFile;
				Assert.That(testMarkdown.CreateHtmFromMarkdownFile(), Is.True);
			}
		}

		[Test]
		public void HtmlWithNoReleaseNotesElementIsCompletelyReplaced()
		{
			var testMarkdown = new GenerateReleaseArtifacts();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
				var markdownFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(markdownFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				File.WriteAllLines(htmlFile,
					new[] {"<html>", "<body>", "<div class='notmarkdown'/>", "</body>", "</html>"});
				testMarkdown.MarkdownFile = markdownFile;
				testMarkdown.HtmlFile = htmlFile;
				Assert.That(testMarkdown.CreateHtmFromMarkdownFile(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasNoMatchForXpath("//div[@notmarkdown]");
			}
		}

		[Test]
		public void HtmlWithReleaseNotesElementHasOnlyReleaseNoteElementChanged()
		{
			var testMarkdown = new GenerateReleaseArtifacts();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
				var markdownFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(markdownFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				File.WriteAllLines(htmlFile,
					new[] {"<html>", "<body>", "<div class='notmarkdown'/>", "<div class='releasenotes'/>", "</body>", "</html>"});
				testMarkdown.MarkdownFile = markdownFile;
				testMarkdown.HtmlFile = htmlFile;
				Assert.That(testMarkdown.CreateHtmFromMarkdownFile(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath("//*[@class='notmarkdown']", 1);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath("//*[@class='releasenotes']", 1);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath("//*[@class='releasenotes']//*[text()[contains(., 'does some things')]]", 1);
			}
		}

		[Test]
		public void HtmlWithReleaseNotesElementWithContentsIsChanged()
		{
			var testMarkdown = new GenerateReleaseArtifacts();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
				var markdownFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(markdownFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				File.WriteAllLines(htmlFile,
					new[]
					{
						"<html>", "<body>", "<div class='releasenotes'>", "<span class='note'/>", "</div>",
						"</body>", "</html>"
					});
				testMarkdown.MarkdownFile = markdownFile;
				testMarkdown.HtmlFile = htmlFile;
				Assert.That(testMarkdown.CreateHtmFromMarkdownFile(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasNoMatchForXpath("//span[@class='note']");
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath("//*[@class='releasenotes']", 1);
			}
		}

		[Test]
		public void StampMarkdownWorks()
		{
			var testMarkdown = new GenerateReleaseArtifacts();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"), null))
			{
				File.WriteAllLines(tempFiles.FirstFile,
					new[] {"## DEV_VERSION_NUMBER: DEV_RELEASE_DATE", "*with some random content", "*does some things"});
				testMarkdown.MarkdownFile = tempFiles.FirstFile;
				testMarkdown.VersionNumber = "2.3.10";
				testMarkdown.StampMarkdownFile = true;
				var day = string.Format("{0:dd/MMM/yyyy}", DateTime.Now);
				Assert.That(testMarkdown.StampMarkdownFileWithVersion(), Is.True);
				var newContents = File.ReadAllLines(tempFiles.FirstFile);
				Assert.That(newContents.Length == 3);
				Assert.That(newContents[0], Is.StringMatching("## 2.3.10 " + day));
			}
		}

		[Test]
		public void StampMarkdownDoesNothingWhenTold()
		{
			var testMarkdown = new GenerateReleaseArtifacts();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"), null))
			{
				var devVersionLine = "## DEV_VERSION_NUMBER: DEV_RELEASE_DATE";
				File.WriteAllLines(tempFiles.FirstFile,
					new[] {devVersionLine, "*with some random content", "*does some things"});
				testMarkdown.MarkdownFile = tempFiles.FirstFile;
				testMarkdown.VersionNumber = "2.3.10";
				testMarkdown.StampMarkdownFile = false;
				Assert.That(testMarkdown.StampMarkdownFileWithVersion(), Is.True);
				var newContents = File.ReadAllLines(tempFiles.FirstFile);
				Assert.That(newContents.Length == 3);
				Assert.That(newContents[0], Is.StringMatching(devVersionLine));
			}
		}

		[Test]
		public void UpdateDebianChangelogWorks()
		{
			var testMarkdown = new GenerateReleaseArtifacts();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"),
				Path.Combine(Path.GetTempPath(), "changelog")))
			{
				var mdFile = tempFiles.FirstFile;
				var changeLogFile = tempFiles.SecondFile;
				File.WriteAllLines(mdFile, new[] {"## 2.3.10: 4/Sep/2014", "* with some random content", "* does some things"});
				File.WriteAllLines(changeLogFile, new[]
				{
					"myfavoriteapp (2.1.0~alpha1) unstable; urgency=low", "", "  * Initial Release for Linux.", "",
					" -- Stephen McConnel <stephen_mcconnel@sil.org>  Fri, 12 Jul 2013 14:57:59 -0500", ""
				});
				testMarkdown.MarkdownFile = mdFile;
				testMarkdown.VersionNumber = "2.3.11";
				testMarkdown.ProductName = "myfavoriteapp";
				testMarkdown.ChangelogAuthorInfo = "Steve McConnel <stephen_mcconnel@sil.org>";
				testMarkdown.DebianChangelog = changeLogFile;
				Assert.That(testMarkdown.UpdateDebianChangelog(), Is.True);
				var newContents = File.ReadAllLines(changeLogFile);
				Assert.AreEqual(newContents.Length, 13, "New changelog entry was not the expected length");
				Assert.That(newContents[0], Is.StringStarting("myfavoriteapp (2.3.11) unstable; urgency=low"));
				//Make sure that the author line matches debian standards for time offset and spacing around author name
				Assert.That(newContents[5], Is.StringMatching(" -- " + testMarkdown.ChangelogAuthorInfo + "  .*[+-]\\d\\d\\d\\d"));
			}
		}

		[Test]
		public void UpdateDebianChangelogAllMdListItemsWork()
		{
			var testingTask = new GenerateReleaseArtifacts();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"),
				Path.Combine(Path.GetTempPath(), "changelog")))
			{
				var markdownFile = tempFiles.FirstFile;
				File.WriteAllLines(markdownFile, new[]
				{
					"## 3.0.97 Beta",
					"- Update French UI Translation",
					"+ When importing, Bloom no longer",
					"  1. makes images transparent when importing.",
					"  4. compresses images transparent when importing.",
					"  9. saves copyright/license back to the original files",
					"    * extra indented list",
					"* Fix insertion of unwanted space before bolded, underlined, and italicized portions of words",
				});
				var debianChangelog = tempFiles.SecondFile;
				File.WriteAllLines(debianChangelog, new[]
				{
					"Bloom (3.0.82 Beta) unstable; urgency=low", "", "  * Older release", "",
					" -- Stephen McConnel <stephen_mcconnel@sil.org>  Fri, 12 Jul 2014 14:57:59 -0500", ""
				});
				testingTask.MarkdownFile = markdownFile;
				testingTask.VersionNumber = "3.0.97 Beta";
				testingTask.ProductName = "myfavoriteapp";
				testingTask.ChangelogAuthorInfo = "John Hatton <john_hatton@sil.org>";
				testingTask.DebianChangelog = debianChangelog;
				Assert.That(testingTask.UpdateDebianChangelog(), Is.True);
				var newContents = File.ReadAllLines(debianChangelog);
				Assert.That(newContents[0], Is.StringContaining("3.0.97 Beta"));
				Assert.That(newContents[2], Is.StringStarting("  *"));
				Assert.That(newContents[3], Is.StringStarting("  *"));
				Assert.That(newContents[4], Is.StringStarting("    *"));
				Assert.That(newContents[5], Is.StringStarting("    *"));
				Assert.That(newContents[6], Is.StringStarting("    *"));
				Assert.That(newContents[7], Is.StringStarting("    *")); // The 3rd (and further) level indentation isn't currently supported
				Assert.That(newContents[8], Is.StringStarting("  *"));
			}
		}
	}
}
