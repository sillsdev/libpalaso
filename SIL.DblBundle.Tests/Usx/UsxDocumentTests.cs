using System.Xml;
using NUnit.Framework;
using SIL.DblBundle.Usx;

namespace SIL.DblBundle.Tests.Usx
{
	/// <summary>
	/// Tests the UsxDocument class.
	///
	/// Note: This class also provides constants and helper methods which are used by Glyssen. They must remain public.
	/// </summary>
	[TestFixture]
	public class UsxDocumentTests
	{
		public const string kUsxFrameStart = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<usx version=\"2.0\">" +
			"<book code=\"MRK\" style=\"id\">Acholi Bible 1985 Digitised by Africa Typesetting Network for DBL April 2013</book>";
		public const string kUsxChapter1AndContentPlaceholder = "<chapter number=\"1\" style=\"c\" />{0}";
		public const string kUsxFrameEnd = "</usx>";
		public const string kUsxFrame =
			kUsxFrameStart +
			kUsxChapter1AndContentPlaceholder +
			kUsxFrameEnd;
		private const string kParaNodeText = "<para style=\"p\"><verse number=\"1\" style=\"v\" />Verse One Text. <verse number=\"2\" style=\"v\" />Verse Two Text.</para>";

		public static XmlDocument CreateMarkOneDoc(string paraXmlNodes, string usxFrame = kUsxFrame)
		{
			return CreateDocFromString(string.Format(usxFrame, paraXmlNodes));
		}

		public static XmlDocument CreateDocFromString(string xml)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml(xml);
			return xmlDoc;
		}

		public static XmlNodeList GetChaptersAndParasForMarkOneContaining2Verses(bool includeChapterEndNode = false)
		{
			var usxDocument = new UsxDocument(CreateMarkOneDoc(kParaNodeText +
				(includeChapterEndNode ? "<chapter eid=\"MRK 1\" />" : "")));
			return usxDocument.GetChaptersAndParas();
		}

		/// <summary>
		/// Tests getting a book id from a UsxDocument.
		/// </summary>
		[Test]
		public void BookId_GetsBookIdFromBookNode()
		{
			var usxDocument = new UsxDocument(CreateMarkOneDoc(string.Empty));
			Assert.AreEqual("MRK", usxDocument.BookId);
		}

		/// <summary>
		/// Tests getting chapters and paragraphs from a UsxDocument.
		/// </summary>
		[Test]
		public void GetChaptersAndParas_ReturnsOnlyChaptersAndParas()
		{
			XmlNodeList nodes = GetChaptersAndParasForMarkOneContaining2Verses();
			Assert.AreEqual(2, nodes.Count);

			var chapterNode = nodes[0];
			Assert.AreEqual("chapter", chapterNode.Name);
			Assert.AreEqual("1", chapterNode.Attributes["number"].Value);
			Assert.AreEqual("c", chapterNode.Attributes["style"].Value);

			var paraNode = nodes[1];
			Assert.AreEqual(4, paraNode.ChildNodes.Count);
			var verse2Node = paraNode.ChildNodes[2];
			var verse2TextNode = paraNode.ChildNodes[3];
			Assert.AreEqual("verse", verse2Node.Name);
			Assert.AreEqual("2", verse2Node.Attributes["number"].Value);
			Assert.AreEqual("#text", verse2TextNode.Name);
			Assert.AreEqual("Verse Two Text.", verse2TextNode.Value);
		}
	}
}
