using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Palaso.Extensions;

namespace Palaso.Tests.Extensions
{
	[TestFixture]
	public class StringExtensions
	{
		[Test]
		public void SplitTrimmed_StringHasSpacesOnly_GivesEmptyList()
		{
			Assert.AreEqual(0, "  ".SplitTrimmed(',').Count);
		}

		[Test]
		public void SplitTrimmed_StringIsEmpty_GivesEmptyList()
		{
			Assert.AreEqual(0, string.Empty.SplitTrimmed(',').Count);
		}

		[Test]
		public void SplitTrimmed_SingleItem_GivesSingleTrimmedItem()
		{
			var list = " hello ".SplitTrimmed(',');
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("hello", list[0]);
		}

		[Test]
		public void SplitTrimmed_StartsWithSeparator_JustSkipsFirstItem()
		{
			var list = " , first".SplitTrimmed(',');
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("first", list[0]);
		}

		[Test]
		public void SplitTrimmed_HasEmptyItems_JustSkipsThem()
		{
			var list = "one,, ,,two".SplitTrimmed(',');
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("one", list[0]);
			Assert.AreEqual("two", list[1]);
		}

		[Test]
		public void SplitTrimmed_Various_GivesEmptyList()
		{
			var list = " hello , bye ".SplitTrimmed(',');
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("hello", list[0]);
			Assert.AreEqual("bye", list[1]);
		}

		[Test]
		public void EscapeAnyUnicodeCharactersIllegalInXml_HasMixOfXmlAndIllegalChars_GivesXmlWithEscapedChars()
		{
			var s  ="This <span href=\"reference\">is well \u001F formed</span> XML!";
			Assert.AreEqual("This <span href=\"reference\">is well &#x1F; formed</span> XML!",
				s.EscapeAnyUnicodeCharactersIllegalInXml());
		}

		[Test]
		public void EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml_HasMixOfXmlAndIllegalChars_GivesPureTextEscapedChars()
		{
			var s = "This <span href=\"reference\">is not really \u001F xml.";
			Assert.AreEqual("This &lt;span href=\"reference\"&gt;is not really &#x1F; xml.",
				s.EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml());
//
//            XmlDocument x = new XmlDocument();
//            x.LoadXml("<hello>&#x1f;</hello>");
//            var writer = XmlWriter.Create(Path.GetTempFileName());
//            writer.WriteElementString("x", x.InnerText);
//
//            writer.WriteNode(z);
		 }

		[Test]
		public void Format_NormalSafeString_GivesSameAsStringFormat()
		{
			Assert.That("1={0}".FormatWithErrorStringInsteadOfException("1").Equals("1=1"));
		}


		[Test]
		public void Format_UnSafeString_GivesErrorString()
		{
			Assert.That("{".FormatWithErrorStringInsteadOfException().Contains("Error"));
		}
	}
}
