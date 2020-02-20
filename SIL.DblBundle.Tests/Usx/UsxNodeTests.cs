using System.Xml;
using NUnit.Framework;
using SIL.DblBundle.Usx;

namespace SIL.DblBundle.Tests.Usx
{
	/// <summary>
	/// Tests the UsxNode class
	/// </summary>
	[TestFixture]
	public class UsxNodeTests
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
		/// Tests that correct StyleTag is obtained from the UsxNode.
		/// </summary>
		[Test]
		public void StyleTag_GetsCorrectStyleTagFromUsxNode()
		{
			var usxNode = new UsxNode(m_nodes[0]);
			Assert.AreEqual("c", usxNode.StyleTag);
		}

		/// <summary>
		/// Texts that the correct chapter number is obtained from the UsxChapter.
		/// </summary>
		[Test]
		public void ChapterNumber_GetsCorrectStyleTagFromUsxChapter()
		{
			var usxChapter = new UsxChapter(m_nodes[0]);
			Assert.AreEqual("1", usxChapter.ChapterNumber);
		}
	}
}
