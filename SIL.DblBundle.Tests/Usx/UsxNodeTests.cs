using System.Xml;
using NUnit.Framework;
using SIL.DblBundle.Usx;

namespace SIL.DblBundle.Tests.Usx
{
	[TestFixture]
	public class UsxNodeTests
	{
		private XmlNodeList m_nodes;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			m_nodes = UsxDocumentTests.GetChaptersAndParasForMarkOneContaining2Verses();
		}

		[Test]
		public void StyleTag_GetsCorrectStyleTagFromUsxNode()
		{
			var usxNode = new UsxNode(m_nodes[0]);
			Assert.AreEqual("c", usxNode.StyleTag);
		}

		[Test]
		public void ChapterNumber_GetsCorrectStyleTagFromUsxChapter()
		{
			var usxChapter = new UsxChapter(m_nodes[0]);
			Assert.AreEqual("1", usxChapter.ChapterNumber);
		}
	}
}
