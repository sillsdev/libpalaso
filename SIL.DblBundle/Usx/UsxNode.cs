using System.Xml;

namespace SIL.DblBundle.Usx
{
	public class UsxNode
	{
		private readonly XmlNode m_node;

		public UsxNode(XmlNode node)
		{
			m_node = node;
		}

		protected XmlNode Node
		{
			get { return m_node; }
		}

		public string StyleTag
		{
			get { return m_node.Attributes.GetNamedItem("style").Value; }
		}

		public XmlNodeList ChildNodes
		{
			get { return m_node.ChildNodes; }
		}
	}

	public class UsxChapter : UsxNode
	{
		public UsxChapter(XmlNode paraNode)
			: base(paraNode)
		{
		}

		public string ChapterNumber
		{
			get { return Node.Attributes.GetNamedItem("number").Value; }
		}

	}
}
