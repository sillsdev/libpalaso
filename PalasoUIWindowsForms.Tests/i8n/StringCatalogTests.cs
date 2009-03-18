using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using Palaso.UI.WindowsForms.i8n;


namespace PalasoUIWindowsForms.Tests.i8n
{
	[TestFixture]
	public class StringCatalogTests
	{
		private string _poFile = Path.GetTempFileName();

		[SetUp]
		public void Setup()
		{
			//BasilProject.InitializeForTests();
			string contents =
				@"# SOME DESCRIPTIVE TITLE.
# Copyright (C) YEAR THE PACKAGE'S COPYRIGHT HOLDER
# This file is distributed under the same license as the PACKAGE package.
# FIRST AUTHOR <EMAIL@ADDRESS>, YEAR.
#
#, fuzzy
msgid ''
msgstr ''
'Project-Id-Version: PACKAGE VERSION\n'
'Report-Msgid-Bugs-To: blah balh\n'
'POT-Creation-Date: 2005-09-20 20:52+0200\n'
'PO-Revision-Date: YEAR-MO-DA HO:MI+ZONE\n'
'Last-Translator: FULL NAME <EMAIL@ADDRESS>\n'
'Language-Team: LANGUAGE <LL@li.org>\n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=CHARSET\n'
'Content-Transfer-Encoding: 8bit\n'

#: string is empty
msgid 'justid'
msgstr ''

#: normal
msgid 'red'
msgstr 'deng'

msgid 'this & that'
msgstr 'this or that'

#: long strings
msgid 'multi1'
msgstr ''
'This is a long '
'sentence.'

#: long strings
msgid 'multi2'
msgstr 'one'
'two'
'three'

#: formatted correctly
msgid 'oneParam'
msgstr 'first={0}'


#: missing param
msgid 'noParams'
msgstr 'first'

";

			contents = contents.Replace('\'', '"');
			File.WriteAllText(_poFile, contents);
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_poFile);
		}

		[Test]
		public void GetFormatted_FoundCorrectString_Formats()
		{
			StringCatalog catalog = new StringCatalog(_poFile, null, 9);
			Assert.AreEqual("first=one", StringCatalog.GetFormatted("oneParam", "blah blah", "one"));
		}

/* TODO we haven't implemented this detection yet
	[Test]
		public void GetFormatted_TooFewParamsInCatalog_ShowsMessageAndReturnRightMessage()
		{
			StringCatalog catalog = new StringCatalog(_poFile, null, 9);
			//todo: when palaso is upgraded to the other branch, we can add something like this
			//using(new Palaso.Reporting.NonFatalErrorDialog())
			var answer = StringCatalog.GetFormatted("noParams", "blah blah","one");
			Assert.AreEqual("!!first", answer);
		}
  */

		[Test]
		public void GetFormatted_TooManyParamsInCatalog_ShowsMessageAndReturnRightMessage()
		{
			StringCatalog catalog = new StringCatalog(_poFile, null, 9);
			using(new Palaso.Reporting.ErrorReport.NonFatalErrorReportExpected())
			{
				var answer = StringCatalog.GetFormatted("oneParam", "blah blah");
				Assert.AreEqual("!!first={0}", answer);
			}
		}

		[Test]
		public void MultiLines_EmtpyMsgStr_Concatenated()
		{
			StringCatalog catalog = new StringCatalog(_poFile, null, 9);
			Assert.AreEqual("This is a long sentence.", catalog["multi1"]);
		}

		[Test]
		public void LongLines_NonEmtpyMsgStr_Concatenated()
		{
			StringCatalog catalog = new StringCatalog(_poFile, null, 9);
			Assert.AreEqual("onetwothree", catalog["multi2"]);
		}


		[Test]
		public void RequestedHasDoubleAmpersands_MatchesSingle()
		{
			StringCatalog catalog = new StringCatalog(_poFile, null, 9);
			Assert.AreEqual("this or that", catalog["this && that"]);
		}

		[Test]
		public void NotTranslated()
		{
			StringCatalog catalog = new StringCatalog(_poFile, null, 9);
			Assert.AreEqual("justid", catalog["justid"]);
		}

		[Test]
		public void NotListedAtAll()
		{
			StringCatalog catalog = new StringCatalog(_poFile, null, 9);
			Assert.AreEqual("notinthere", catalog["notinthere"]);
		}

		[Test]
		public void Normal()
		{
			StringCatalog catalog = new StringCatalog(_poFile,null, 9);
			Assert.AreEqual("deng", catalog["red"]);
		}

		[Test]
		public void FontsScaleUp()
		{
			StringCatalog catalog = new StringCatalog(_poFile, "Onyx", 30);
			Font normal = new Font(System.Drawing.FontFamily.GenericSerif, 20);
			Font localized = StringCatalog.ModifyFontForLocalization(normal);
			Assert.AreEqual(41,Math.Floor(localized.SizeInPoints));
		}
		[Test]
		public void FontsChanged()
		{
			StringCatalog catalog = new StringCatalog(_poFile, FontFamily.GenericSansSerif.Name, 30);
			Font normal = new Font(FontFamily.GenericSerif, 20);
			Font localized = StringCatalog.ModifyFontForLocalization(normal);
			Assert.AreEqual(FontFamily.GenericSansSerif.Name, localized.FontFamily.Name);
		}
		[Test]
		public void ModifyFontForLocation_SmallFont_ValidFont()
		{
			StringCatalog catalog = new StringCatalog(_poFile, FontFamily.GenericSerif.Name, Single.Epsilon);
			Font normal = new Font(FontFamily.GenericSerif, Single.Epsilon);
			Font localized = StringCatalog.ModifyFontForLocalization(normal);
			Assert.IsNotNull(localized);
		}
		[Test]
		public void ModifyFontForLocation_HugeFont_ValidFont()
		{
			StringCatalog catalog = new StringCatalog(_poFile, FontFamily.GenericSerif.Name, Single.MaxValue);
			Font normal = new Font(FontFamily.GenericSerif, Single.MaxValue);
			Font localized = StringCatalog.ModifyFontForLocalization(normal);
			Assert.IsNotNull(localized);
		}
	}
}