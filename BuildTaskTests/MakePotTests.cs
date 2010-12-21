using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Palaso.BuildTasks.MakePot;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace BuildTaskTests
{
	[TestFixture]
	public class MakePotTests
	{

		private class MockTaskItem : ITaskItem
		{

			public MockTaskItem(string filePath)
			{
				ItemSpec = filePath;
			}

			public string GetMetadata(string metadataName)
			{
				return "";
			}

			public void SetMetadata(string metadataName, string metadataValue)
			{
			}

			public void RemoveMetadata(string metadataName)
			{
			}

			public void CopyMetadataTo(ITaskItem destinationItem)
			{
			}

			public IDictionary CloneCustomMetadata()
			{
				return null;
			}

			public string ItemSpec { get; set; }

			public ICollection MetadataNames
			{
				get { return null; }
			}

			public int MetadataCount
			{
				get { return 0; }
			}
		}

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
				pot.CSharpFiles = EnvironmentForTest.CreateTaskItemsForFilePath(csharpFilePath);
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

			string expected =
@"msgid ''
msgstr ''
'Project-Id-Version: \n'
'POT-Creation-Date: .*
'PO-Revision-Date: \n'
'Last-Translator: \n'
'Language-Team: \n'
'Plural-Forms: \n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=UTF-8\n'
'Content-Transfer-Encoding: 8bit\n'

# Project-Id-Version:
# Report-Msgid-Bugs-To:
# POT-Creation-Date: .*
# Content-Type: text/plain; charset=UTF-8


#: .*
msgid 'FirstLocalizableString'
msgstr ''

#: .*
#. SecondNotes
msgid 'SecondLocalizableString'
msgstr ''

#: .*
#. ThirdNotes
msgid 'ThirdLocalizableString'
msgstr ''
".Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), ConstrainStringByLine.Matches(expected));
			}


		}

		[Test]
		public void ProcessSrcFile_BackupStringWithDots_DoesNotHaveDuplicates()
		{
			string contents = @"
somevar.Text = 'Backing Up...';
".Replace("'", "\"");

			string expected =
@"msgid ''
msgstr ''
'Project-Id-Version: \n'
'POT-Creation-Date: .*
'PO-Revision-Date: \n'
'Last-Translator: \n'
'Language-Team: \n'
'Plural-Forms: \n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=UTF-8\n'
'Content-Transfer-Encoding: 8bit\n'

# Project-Id-Version:
# Report-Msgid-Bugs-To:
# POT-Creation-Date: .*
# Content-Type: text/plain; charset=UTF-8


#: .*csharp.cs
msgid 'Backing Up...'
msgstr ''
".Replace("'", "\"");

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

			string expected =
@"msgid ''
msgstr ''
'Project-Id-Version: \n'
'POT-Creation-Date: .*
'PO-Revision-Date: \n'
'Last-Translator: \n'
'Language-Team: \n'
'Plural-Forms: \n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=UTF-8\n'
'Content-Transfer-Encoding: 8bit\n'

# Project-Id-Version:
# Report-Msgid-Bugs-To:
# POT-Creation-Date: .*
# Content-Type: text/plain; charset=UTF-8


#: .*csharp.cs
#: .*csharp.cs
msgid 'Backing Up...'
msgstr ''
".Replace("'", "\"");

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

			string expected =
@"msgid ''
msgstr ''
'Project-Id-Version: \n'
'POT-Creation-Date: .*
'PO-Revision-Date: \n'
'Last-Translator: \n'
'Language-Team: \n'
'Plural-Forms: \n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=UTF-8\n'
'Content-Transfer-Encoding: 8bit\n'

# Project-Id-Version:
# Report-Msgid-Bugs-To:
# POT-Creation-Date: .*
# Content-Type: text/plain; charset=UTF-8

".Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), ConstrainStringByLine.Matches(expected));
			}
		}

	}
}
