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
		private XmlNodeList m_nodes;

		/// <summary>
		/// Test fixture setup
		/// </summary>
		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			m_nodes = UsxDocumentTests.GetChaptersAndParasForMarkOneContaining2Verses();
		}

		/// <summary>
		/// Tests that the <see cref="UsxPara"/> constructor throws an <see>ArgumentException</see>
		/// if the underlying USX node is not a para element.
		/// </summary>
		[Test]
		public void Constructor_UnderlyingNodeIsNotAParaElement_ThrowsArgumentException()
		{
			Assert.That(() => new UsxPara(m_nodes[0]),
				Throws.ArgumentException.With.Message.StartsWith("Not a valid para node."));
		}

		/// <summary>
		/// Tests that correct StyleTag is obtained from the UsxPara.
		/// </summary>
		[Test]
		public void StyleTag_ChapterStart_GetsCorrectStyleTagFrom()
		{
			Assert.AreEqual("p", new UsxPara(m_nodes[1]).StyleTag);
		}
	}

	/// <summary>
	/// Tests the UsxChar class
	/// </summary>
	[TestFixture]
	public class UsxCharTests
	{
		/// <summary>
		/// Tests that the <see cref="UsxPara"/> constructor throws an <see>ArgumentException</see>
		/// if the underlying USX node is not a para element.
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
		/// Tests that correct StyleTag is obtained from the UsxPara.
		/// </summary>
		[Test]
		public void StyleTag_ChapterStart_GetsCorrectStyleTagFrom()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<para style=\"p\">This is some text that should <char style=\"add\">be</char> good.</para>");
			var usxDoc = new UsxDocument(xmlDoc);
			var para = new UsxPara(usxDoc.GetChaptersAndParas()[0]);
			Assert.AreEqual("add", new UsxChar(para.ChildNodes[1]).StyleTag);
		}
	}

	/// <summary>
	/// Tests the UsxChapter class
	/// </summary>
	[TestFixture]
	public class UsxChapterTests
	{
		private XmlNodeList m_nodes;

		/// <summary>
		/// Test fixture setup
		/// </summary>
		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			m_nodes = UsxDocumentTests.GetChaptersAndParasForMarkOneContaining2Verses(true);
		}

		/// <summary>
		/// Tests that Constructor throws an <see>ArgumentException</see> if
		/// the underlying USX node is not a chapter node.
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
		/// Tests that correct StyleTag is obtained from the UsxChapter.
		/// </summary>
		[Test]
		public void StyleTag_ChapterStart_GetsCorrectStyleTag()
		{
			Assert.AreEqual("c", new UsxChapter(m_nodes[0]).StyleTag);
		}

		/// <summary>
		/// Tests that StyleTag returns null if the UsxNode is a chapter end.
		/// </summary>
		[Test]
		public void StyleTag_ChapterEnd_ReturnsNull()
		{
			var usxNode = new UsxChapter(m_nodes[m_nodes.Count - 1]);
			Assert.IsNull(usxNode.StyleTag);
		}

		/// <summary>
		/// Tests that IsChapterStart returns <c>true</c> when the object is based on a node that
		/// does not have an sid or eid attribute.
		/// </summary>
		[Test]
		public void IsChapterStart_StartNoSid_ReturnsTrue()
		{
			var usxChapterNode = new UsxChapter(m_nodes[0]);
			Assert.IsTrue(usxChapterNode.IsChapterStart);
		}

		/// <summary>
		/// Tests that IsChapterStart returns <c>true</c> when the object is based on a node that
		/// has an sid attribute.
		/// </summary>
		[Test]
		public void IsChapterStart_StartWithSid_ReturnsTrue()
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml("<chapter number=\"2\" style=\"c\" sid=\"MRK 2\"/>");
			var usxDoc = new UsxDocument(xmlDoc);
			var usxChapterNode = new UsxChapter(usxDoc.GetChaptersAndParas()[0]);
			Assert.IsTrue(usxChapterNode.IsChapterStart);
		}

		/// <summary>
		/// Tests that IsChapterStart returns <c>true</c> when the object is based on a node that
		/// has an sid attribute.
		/// </summary>
		[Test]
		public void IsChapterStart_End_ReturnsFalse()
		{
			var usxChapter = new UsxChapter(m_nodes[m_nodes.Count - 1]);
			Assert.IsFalse(usxChapter.IsChapterStart);
		}

		/// <summary>
		/// Texts that the correct chapter number is obtained from the UsxChapter.
		/// </summary>
		[Test]
		public void ChapterNumber_StartNoSid_GetsCorrectChapterNumberFromNumberAttribute()
		{
			var usxChapter = new UsxChapter(m_nodes[0]);
			Assert.AreEqual("1", usxChapter.ChapterNumber);
		}

		/// <summary>
		/// Tests that StyleTag returns null if the UsxNode is a chapter end.
		/// </summary>
		[Test]
		public void ChapterNumber_ChapterEnd_GetsCorrectChapterNumberFromEidAttribute()
		{
			var usxChapter = new UsxChapter(m_nodes[m_nodes.Count - 1]);
			Assert.AreEqual("1", usxChapter.ChapterNumber);
		}

		/// <summary>
		/// Tests that IsChapterStart throws a <see>NullReferenceException</see> if
		/// the underlying USX node is a chapter with neither a "number" attribute
		/// nor an "eid" attribute.
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
