using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Palaso.BuildTasks.MakePot;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace Palaso.BuildTask.Tests
{
	[TestFixture]
	public class MakePotTests
	{


		private class EnvironmentForTest : TemporaryFolder
		{
			public EnvironmentForTest() :
				base("Palaso.BuildTaskTests.MakePotTests")
			{
			}

			public static ITaskItem[] CreateTaskItemsForFilePath(string filePath)
			{
				var items = new ITaskItem[1];
				items[0] = new MockTaskItem(filePath);
				return items;
			}

			public string MakePotFile(string input)
			{
				string csharpFilePath = System.IO.Path.Combine(Path, "csharp.cs");
				File.WriteAllText(csharpFilePath, input);

				var pot = new MakePot();
				pot.OutputFile = System.IO.Path.Combine(Path, "output.pot");
				pot.CSharpFiles = CreateTaskItemsForFilePath(csharpFilePath);
				pot.Execute();

				return File.ReadAllText(pot.OutputFile);
			}
		}

		[Test]
		public void MatchesInCSharpString_StringWithTilde_HasMatch()
		{
			string contents = @"
somevar.MyLocalizableFunction('~MyLocalizableString');
".Replace("'", "\"");

			var pot = new MakePot();
			MatchCollection matches = pot.MatchesInCSharpString(contents);
			Assert.AreEqual(1, matches.Count);
			foreach (Match match in matches)
			{
				Assert.AreEqual(3, match.Groups.Count);
				Assert.AreEqual("MyLocalizableString", match.Groups["key"].Value);
			}
		}

		[Test]
		public void MatchesInCSharpString_StringWithTildeAndNotes_HasMatchAndNotes()
		{
			string contents = @"
somevar.MyLocalizableFunction('~MyLocalizableString', 'MyTranslationNotes');
".Replace("'", "\"");

			var pot = new MakePot();
			MatchCollection matches = pot.MatchesInCSharpString(contents);
			Assert.AreEqual(1, matches.Count);
			foreach (Match match in matches)
			{
				Assert.AreEqual(3, match.Groups.Count);
				Assert.AreEqual("MyLocalizableString", match.Groups["key"].Value);
				Assert.AreEqual("MyTranslationNotes", match.Groups["note"].Value);

			}
		}

		[Test]
		public void MatchesInCSharpString_StringWithTwoMatches_DoesntContainTildeInResult()
		{
			string contents = @"
somevar.MyLocalizableFunction(StringCatalog.Get('~MyLocalizableString', 'MyTranslationNotes'));
".Replace("'", "\"");

			var pot = new MakePot();
			MatchCollection matches = pot.MatchesInCSharpString(contents);
			Assert.AreEqual(1, matches.Count);
			foreach (Match match in matches)
			{
				Assert.AreEqual(3, match.Groups.Count);
				Assert.AreEqual("MyLocalizableString", match.Groups["key"].Value);
				Assert.AreEqual("MyTranslationNotes", match.Groups["note"].Value);

			}
		}

		[Test]
		public void MatchesInCSharpString_UsingStringCatalogNoTilde_HasMatchAndNotes()
		{
			string contents = @"
somevar.MyLocalizableFunction(StringCatalog.Get('MyLocalizableString', 'MyTranslationNotes'));
".Replace("'", "\"");

			var pot = new MakePot();
			MatchCollection matches = pot.MatchesInCSharpString(contents);
			Assert.AreEqual(1, matches.Count);
			foreach (Match match in matches)
			{
				Assert.AreEqual(3, match.Groups.Count);
				Assert.AreEqual("MyLocalizableString", match.Groups["key"].Value);
				Assert.AreEqual("MyTranslationNotes", match.Groups["note"].Value);

			}
		}

		[Test]
		public void MatchesInCSharpString_UsingStringCatalogGetFormattedNoTilde_HasMatchAndNotes()
		{
			string contents = @"
somevar.MyLocalizableFunction(StringCatalog.GetFormatted('MyLocalizableString {0}', 'MyTranslationNotes', someArg));
".Replace("'", "\"");

			var pot = new MakePot();
			MatchCollection matches = pot.MatchesInCSharpString(contents);
			Assert.AreEqual(1, matches.Count);
			foreach (Match match in matches)
			{
				Assert.AreEqual(3, match.Groups.Count);
				Assert.AreEqual("MyLocalizableString {0}", match.Groups["key"].Value);
				Assert.AreEqual("MyTranslationNotes", match.Groups["note"].Value);

			}
		}

		[Test]
		public void MatchesInCSharpString_UsingTextEqual_HasMatchAndNotes()
		{
			string contents = @"
somevar.Text = 'MyLocalizableString';
".Replace("'", "\"");

			var pot = new MakePot();
			MatchCollection matches = pot.MatchesInCSharpString(contents);
			Assert.AreEqual(1, matches.Count);
			foreach (Match match in matches)
			{
				Assert.AreEqual(3, match.Groups.Count);
				Assert.AreEqual("MyLocalizableString", match.Groups["key"].Value);

			}
		}

		[Test]
		public void MatchesInCSharpString_StringWithBackslashQuote_MatchesToEndOfString()
		{
			string contents = @"
somevar.Text = 'MyLocalizableString \'InQuote\' end';
".Replace("'", "\"");

			string expected = "MyLocalizableString \\\"InQuote\\\" end";

			var pot = new MakePot();
			MatchCollection matches = pot.MatchesInCSharpString(contents);
			Assert.AreEqual(1, matches.Count);
			foreach (Match match in matches)
			{
				Assert.AreEqual(3, match.Groups.Count);
				Assert.AreEqual(expected, match.Groups["key"].Value);

			}
		}

		[Test]
		public void UnescapeString_WithBackSlash_HasNoBackslash()
		{
			const string contents = @"don\'t want backslash";
			const string expected = @"don't want backslash";

			string actual = MakePot.UnescapeString(contents);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ProcessSrcFile_AllMatches_OutputsGoodPo()
		{
			string contents = @"
somevar.Text = 'FirstLocalizableString';

somevar.MyLocalizableFunction(StringCatalog.Get('SecondLocalizableString', 'SecondNotes'));

somevar.MyLocalizableFunction('~ThirdLocalizableString', 'ThirdNotes');

".Replace("'", "\"");

			var expectedSb = new StringBuilder();
			expectedSb.AppendLine("msgid ''");
			expectedSb.AppendLine("msgstr ''");
			expectedSb.AppendLine("'Project-Id-Version: \\n'");
			expectedSb.AppendLine("'POT-Creation-Date: .*");
			expectedSb.AppendLine("'PO-Revision-Date: \\n'");
			expectedSb.AppendLine("'Last-Translator: \\n'");
			expectedSb.AppendLine("'Language-Team: \\n'");
			expectedSb.AppendLine("'Plural-Forms: \\n'");
			expectedSb.AppendLine("'MIME-Version: 1.0\\n'");
			expectedSb.AppendLine("'Content-Type: text/plain; charset=UTF-8\\n'");
			expectedSb.AppendLine("'Content-Transfer-Encoding: 8bit\\n'");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("# Project-Id-Version: ");
			expectedSb.AppendLine("# Report-Msgid-Bugs-To: ");
			expectedSb.AppendLine("# POT-Creation-Date: .*");
			expectedSb.AppendLine("# Content-Type: text/plain; charset=UTF-8");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("#: .*");
			expectedSb.AppendLine("msgid 'FirstLocalizableString'");
			expectedSb.AppendLine("msgstr ''");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("#: .*");
			expectedSb.AppendLine("#. SecondNotes");
			expectedSb.AppendLine("msgid 'SecondLocalizableString'");
			expectedSb.AppendLine("msgstr ''");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("#: .*");
			expectedSb.AppendLine("#. ThirdNotes");
			expectedSb.AppendLine("msgid 'ThirdLocalizableString'");
			expectedSb.AppendLine("msgstr ''");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("");

			string expected = expectedSb.ToString().Replace("'", "\"");
			using (var e = new EnvironmentForTest())
			{
				Console.Out.WriteLine(expected);
				Assert.That(e.MakePotFile(contents), ConstrainStringByLine.Matches(expected));
			}


		}

		[Test]
		public void ProcessSrcFile_BackupStringWithDots_DoesNotHaveDuplicates()
		{
			string contents = @"
somevar.Text = 'Backing Up...';
".Replace("'", "\"");

			var expectedSb = new StringBuilder();
			expectedSb.AppendLine("msgid ''");
			expectedSb.AppendLine("msgstr ''");
			expectedSb.AppendLine("'Project-Id-Version: \\n'");
			expectedSb.AppendLine("'POT-Creation-Date: .*");
			expectedSb.AppendLine("'PO-Revision-Date: \\n'");
			expectedSb.AppendLine("'Last-Translator: \\n'");
			expectedSb.AppendLine("'Language-Team: \\n'");
			expectedSb.AppendLine("'Plural-Forms: \\n'");
			expectedSb.AppendLine("'MIME-Version: 1.0\\n'");
			expectedSb.AppendLine("'Content-Type: text/plain; charset=UTF-8\\n'");
			expectedSb.AppendLine("'Content-Transfer-Encoding: 8bit\\n'");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("# Project-Id-Version: ");
			expectedSb.AppendLine("# Report-Msgid-Bugs-To: ");
			expectedSb.AppendLine("# POT-Creation-Date: .*");
			expectedSb.AppendLine("# Content-Type: text/plain; charset=UTF-8");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("#: .*csharp.cs");
			expectedSb.AppendLine("msgid 'Backing Up...'");
			expectedSb.AppendLine("msgstr ''");

			string expected = expectedSb.ToString().Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), ConstrainStringByLine.Matches(expected));
			}
		}

		[Test]
		public void ProcessSrcFile_BackupStringWithDuplicates_HasOnlyOneInOutput()
		{
			string contents = @"
somevar.Text = 'Backing Up...';

somevar.Text = 'Backing Up...';
".Replace("'", "\"");

			var expectedSb = new StringBuilder();
			expectedSb.AppendLine("msgid ''");
			expectedSb.AppendLine("msgstr ''");
			expectedSb.AppendLine("'Project-Id-Version: \\n'");
			expectedSb.AppendLine("'POT-Creation-Date: .*");
			expectedSb.AppendLine("'PO-Revision-Date: \\n'");
			expectedSb.AppendLine("'Last-Translator: \\n'");
			expectedSb.AppendLine("'Language-Team: \\n'");
			expectedSb.AppendLine("'Plural-Forms: \\n'");
			expectedSb.AppendLine("'MIME-Version: 1.0\\n'");
			expectedSb.AppendLine("'Content-Type: text/plain; charset=UTF-8\\n'");
			expectedSb.AppendLine("'Content-Transfer-Encoding: 8bit\\n'");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("# Project-Id-Version: ");
			expectedSb.AppendLine("# Report-Msgid-Bugs-To: ");
			expectedSb.AppendLine("# POT-Creation-Date: .*");
			expectedSb.AppendLine("# Content-Type: text/plain; charset=UTF-8");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("#: .*csharp.cs");
			expectedSb.AppendLine("#: .*csharp.cs");
			expectedSb.AppendLine("msgid 'Backing Up...'");
			expectedSb.AppendLine("msgstr ''");

			string expected = expectedSb.ToString().Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), ConstrainStringByLine.Matches(expected));
			}
		}

		[Test]
		public void ProcessSrcFile_EmptyString_NotPresentInOutput()
		{
			string contents = @"
somevar.Text = '';
".Replace("'", "\"");

			var expectedSb = new StringBuilder();
			expectedSb.AppendLine("msgid ''");
			expectedSb.AppendLine("msgstr ''");
			expectedSb.AppendLine("'Project-Id-Version: \\n'");
			expectedSb.AppendLine("'POT-Creation-Date: .*");
			expectedSb.AppendLine("'PO-Revision-Date: \\n'");
			expectedSb.AppendLine("'Last-Translator: \\n'");
			expectedSb.AppendLine("'Language-Team: \\n'");
			expectedSb.AppendLine("'Plural-Forms: \\n'");
			expectedSb.AppendLine("'MIME-Version: 1.0\\n'");
			expectedSb.AppendLine("'Content-Type: text/plain; charset=UTF-8\\n'");
			expectedSb.AppendLine("'Content-Transfer-Encoding: 8bit\\n'");
			expectedSb.AppendLine("");
			expectedSb.AppendLine("# Project-Id-Version: ");
			expectedSb.AppendLine("# Report-Msgid-Bugs-To: ");
			expectedSb.AppendLine("# POT-Creation-Date: .*");
			expectedSb.AppendLine("# Content-Type: text/plain; charset=UTF-8");
			expectedSb.AppendLine("");

			string expected = expectedSb.ToString().Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), ConstrainStringByLine.Matches(expected));
			}
		}

	}
}
