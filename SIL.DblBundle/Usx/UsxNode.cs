using System.Xml;

namespace SIL.DblBundle.Usx
{
	/// <summary>
	/// An XML node in a USX file (Scripture file in XML format)
	/// </summary>
	public class UsxNode
	{
		private readonly XmlNode m_node;

		/// <summary>
		/// Constructs a USX XML node
		/// </summary>
		public UsxNode(XmlNode node)
		{
			m_node = node;
		}

		/// <summary>
		/// This XML node
		/// </summary>
		protected XmlNode Node
		{
			get { return m_node; }
		}

		/// <summary>
		/// Gets style applied to current node
		/// </summary>
		public string StyleTag
		{
			get { return m_node.Attributes.GetNamedItem("style").Value; }
		}

		/// <summary>
		/// List of nodes that are children of this node.
		/// </summary>
		public XmlNodeList ChildNodes
		{
			get { return m_node.ChildNodes; }
		}
	}

	/// <summary>
	/// A USX node that specifies the chapter
	/// </summary>
	public class UsxChapter : UsxNode
	{
		/// <summary>
		/// Creates a chapter XML node 
		/// </summary>
		public UsxChapter(XmlNode paraNode)
			: base(paraNode)
		{
		}

		/// <summary>
		/// Gets the chapter number
		/// </summary>
		public string ChapterNumber
		{
			get { return Node.Attributes.GetNamedItem("number").Value; }
		}

	}
}
