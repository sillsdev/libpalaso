using System;
using System.Xml;
using NUnit.Framework;
using SIL.DblBundle.Usx;

namespace SIL.DblBundle.Tests.Usx
{
	/// <summary>
	/// Tests the (abstract) UsxNode class
	/// </summary>
	[TestFixture]
	public class UsxNodeTests
	{
		/// <summary>
		/// Tests that the <see cref="UsxNode"/> constructor throws an <see>ArgumentException</see>
		/// if the underlying USX node has no attributes.
		/// </summary>
		[Test]
		public void Constructor_UnderlyingNodeIsNotAnElement_ThrowsArgumentException()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<para style=\"p\">This is some text</para>");
			var usxDoc = new UsxDocument(xmlDoc);
			Assert.That(() => new UsxPara(usxDoc.GetChaptersAndParas()[0].Attributes[0]),
				Throws.ArgumentException.With.Message.StartsWith(
					"Given node is not a valid USX element."));
		}

		/// <summary>
		/// Tests that the <see cref="UsxNode"/> constructor throws an <see>ArgumentException</see>
		/// if the underlying USX node has no attributes.
		/// </summary>
		[Test]
		public void Constructor_UnderlyingNodeHasNoAttributes_ThrowsArgumentException()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<para>This is some text</para>");
			var usxDoc = new UsxDocument(xmlDoc);
			Assert.That(() => new UsxPara(usxDoc.GetChaptersAndParas()[0]),
				Throws.ArgumentException.With.Message.StartsWith(
					"Invalid USX node. Must have at least one attribute."));
		}
	}

	/// <summary>
	/// Tests the UsxPara class
	/// </summary>
	[TestFixture]
	public class UsxParaTests
	{
		/// <summary>
		/// Tests that the <see cref="UsxPara"/> constructor throws an <see>ArgumentException</see>
		/// if the underlying USX node is not a para element.
		/// </summary>
		[Test]
		public void Constructor_UnderlyingNodeIsNotAParaElement_ThrowsArgumentException()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<char style=\"wj\">This is some text</char>");
			var nodes = xmlDoc.SelectNodes("//char");
			Assert.That(() => new UsxPara(nodes[0]),
				Throws.ArgumentException.With.Message.StartsWith("Not a valid para node."));
		}

		/// <summary>
		/// Tests that the <see cref="UsxPara"/> constructor throws an
		/// <see>ArgumentNullException</see> if the underlying node is null.
		/// </summary>
		[Test]
		public void Constructor_NullNode_ThrowsArgumentNullException()
		{
			Assert.That(() => new UsxPara(null),
				Throws.ArgumentNullException);
		}

		/// <summary>
		/// Tests that correct StyleTag is obtained from the UsxPara.
		/// </summary>
		[TestCase("p")]
		[TestCase("q1")]
		public void StyleTag_NormalPara_GetsCorrectStyleTagFrom(string style)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml($"<para style=\"{style}\">This is some text</para>");
			var usxDoc = new UsxDocument(xmlDoc);
			Assert.AreEqual(style, new UsxPara(usxDoc.GetChaptersAndParas()[0]).StyleTag);
		}
	}

	/// <summary>
	/// Tests the UsxChar class
	/// </summary>
	[TestFixture]
	public class UsxCharTests
	{
		/// <summary>
		/// Tests that the <see cref="UsxChar"/> constructor throws an <see>ArgumentException</see>
		/// if the underlying USX node is not a "char" element.
		/// </summary>
		[Test]
		public void Constructor_UnderlyingNodeIsNotACharElement_ThrowsArgumentException()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<para style=\"p\">This is some text</para>");
			var usxDoc = new UsxDocument(xmlDoc);
			Assert.That(() => new UsxChar(usxDoc.GetChaptersAndParas()[0]),
				Throws.ArgumentException.With.Message.StartsWith("Not a valid char node."));
		}

		/// <summary>
		/// Tests that the <see cref="UsxChar"/> constructor throws an
		/// <see>ArgumentNullException</see> if the underlying node is null.
		/// </summary>
		[Test]
		public void Constructor_NullNode_ThrowsArgumentNullException()
		{
			Assert.That(() => new UsxChar(null),
				Throws.ArgumentNullException);
		}

		/// <summary>
		/// Tests that correct StyleTag is obtained from the UsxChar.
		/// </summary>
		[TestCase("add")]
		[TestCase("wj")]
		public void StyleTag_ChapterStart_GetsCorrectStyleTagFrom(string charStyle)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<para style=\"p\">This is some text that should " +
				$"<char style=\"{charStyle}\">be</char> good.</para>");
			var usxDoc = new UsxDocument(xmlDoc);
			var para = new UsxPara(usxDoc.GetChaptersAndParas()[0]);
			Assert.AreEqual(charStyle, new UsxChar(para.ChildNodes[1]).StyleTag);
		}
	}

	/// <summary>
	/// Tests the UsxChapter class
	/// </summary>
	[TestFixture]
	public class UsxChapterTests
	{
		/// <summary>
		/// Tests that <see cref="UsxChapter"/> constructor throws an <see>ArgumentException</see>
		/// if the underlying USX node is not a chapter node.
		/// </summary>
		[Test]
		public void Constructor_NotAChapterNode_ThrowsArgumentException()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<para chapter=\"2\" style=\"c\"/>");
			var usxDoc = new UsxDocument(xmlDoc);
			Assert.That(() => { new UsxChapter(usxDoc.GetChaptersAndParas()[0]); },
				Throws.ArgumentException);
		}

		/// <summary>
		/// Tests that the <see cref="UsxChapter"/> constructor throws an
		/// <see>ArgumentNullException</see> if the underlying node is null.
		/// </summary>
		[Test]
		public void Constructor_NullNode_ThrowsArgumentNullException()
		{
			Assert.That(() => new UsxChapter(null),
				Throws.ArgumentNullException);
		}
		
		/// <summary>
		/// Tests that <see cref="UsxChapter.StyleTag"/> returns "c" when the object is based on a
		/// chapter start node.
		/// </summary>
		[TestCase()]
		[TestCase(" sid=\"LUK 2\"")]
		public void StyleTag_ChapterStart_GetsCorrectStyleTag(string sidAttr = null)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml($"<chapter number=\"2\" style=\"c\"{sidAttr}/>");
			var nodes = xmlDoc.SelectNodes("//chapter");
			Assert.AreEqual("c", new UsxChapter(nodes[0]).StyleTag);
		}

		/// <summary>
		/// Tests that <see cref="UsxChapter.StyleTag"/> returns null when the object is based on a
		/// chapter end node.
		/// </summary>
		[Test]
		public void StyleTag_ChapterEnd_ReturnsNull()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<chapter eid=\"REV 6\"/>");
			var nodes = xmlDoc.SelectNodes("//chapter");
			Assert.IsNull(new UsxChapter(nodes[0]).StyleTag);
		}

		/// <summary>
		/// Tests that <see cref="UsxChapter.IsChapterStart"/> returns <c>true</c> when the object
		/// is based on a chapter start node.
		/// </summary>
		[TestCase()]
		[TestCase(" sid=\"LUK 2\"")]
		public void IsChapterStart_ChapterStart_ReturnsTrue(string sidAttr = null)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml($"<chapter number=\"2\" style=\"c\"{sidAttr}/>");
			var nodes = xmlDoc.SelectNodes("//chapter");
			var usxChapterNode = new UsxChapter(nodes[0]);
			Assert.IsTrue(usxChapterNode.IsChapterStart);
		}

		/// <summary>
		/// Tests that <see cref="UsxChapter.IsChapterStart"/> returns <c>false</c> when the object
		/// is based on a chapter end node.
		/// </summary>
		[Test]
		public void IsChapterStart_End_ReturnsFalse()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<chapter eid=\"ACT 27\"/>");
			var nodes = xmlDoc.SelectNodes("//chapter");
			var usxChapter = new UsxChapter(nodes[0]);
			Assert.IsFalse(usxChapter.IsChapterStart);
		}

		/// <summary>
		/// Tests that <see cref="UsxChapter.ChapterNumber"/> gets the chapter number from the
		/// number attribute if the node is a chapter start.
		/// </summary>
		[TestCase()]
		[TestCase(" sid=\"LUK 2\"")] // Note: This is bad data
		[TestCase(" sid=\"LUK 3\"")]
		public void ChapterNumber_ChapterStart_GetsCorrectChapterNumberFromNumberAttribute(
			string sidAttr = null)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml($"<chapter number=\"3\" style=\"c\"{sidAttr}/>");
			var nodes = xmlDoc.SelectNodes("//chapter");
			var usxChapter = new UsxChapter(nodes[0]);
			Assert.AreEqual("3", usxChapter.ChapterNumber);
		}

		/// <summary>
		/// Tests that <see cref="UsxChapter.ChapterNumber"/> gets the chapter number from the eid
		/// attribute if the node is a chapter end.
		/// </summary>
		[TestCase("1")]
		[TestCase("13")]
		public void ChapterNumber_ChapterEnd_GetsCorrectChapterNumberFromEidAttribute(string chapter)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml($"<chapter eid=\"ACT {chapter}\"/>");
			var nodes = xmlDoc.SelectNodes("//chapter");
			var usxChapter = new UsxChapter(nodes[0]);
			Assert.AreEqual(chapter, usxChapter.ChapterNumber);
		}

		/// <summary>
		/// Tests that <see cref="UsxChapter.ChapterNumber"/> throws a
		/// <see>NullReferenceException</see> if the underlying USX node is a chapter with neither
		/// a "number" attribute nor an "eid" attribute.
		/// </summary>
		[Test]
		public void ChapterNumber_InvalidUnderlyingNode_ThrowsNullReferenceException()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<chapter sid=\"JHN 5\"/>");
			var usxDoc = new UsxDocument(xmlDoc);
			var usxChapterNode = new UsxChapter(usxDoc.GetChaptersAndParas()[0]);
			Assert.That(() => usxChapterNode.ChapterNumber,
				Throws.Exception.TypeOf<NullReferenceException>());
		}
	}
}
