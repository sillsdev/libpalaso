using System;
using System.Linq;
using System.Xml;

namespace SIL.DblBundle.Usx
{
	/// <summary>
	/// An XML node in a USX file (XML representation of USFM Scripture)
	/// </summary>
	public abstract class UsxNode
	{
		public const string kCharNodeName = "char";
		public const string kChapterNodeName = "chapter";
		public const string kParaNodeName = "para";
		public const string kVerseNodeName = "verse";
		public const string kMilestoneNodeName = "ms";

		private readonly XmlNode m_node;

		protected XmlNode GetAttribute(string attributeName) =>
			m_node.Attributes.GetNamedItem(attributeName);

		/// <summary>
		/// Constructs a USX XML node
		/// </summary>
		/// <exception cref="ArgumentException">Given underlying node is not an XML element or
		/// has no attributes. A valid USX element will typically have a style attribute, but
		/// an end milestone will just have an eid attribute.</exception>
		public UsxNode(XmlNode node)
		{
			m_node = node ?? throw new ArgumentNullException(nameof(node));
			if (m_node.NodeType != XmlNodeType.Element)
			{
				throw new ArgumentException("Given node is not a valid USX element.",
					nameof(node));
			}
			if (m_node.Attributes == null || m_node.Attributes.Count == 0)
			{
				throw new ArgumentException("Invalid USX node. Must have at least one attribute.",
					nameof(node));
			}
		}

		/// <summary>
		/// This XML node
		/// </summary>
		protected XmlNode Node => m_node;

		/// <summary>
		/// Gets the style tag (SFM without the leading backslash) of this node.
		/// </summary>
		public virtual string StyleTag => GetAttribute("style").Value;

		/// <summary>
		/// List of nodes that are children of this node.
		/// </summary>
		public XmlNodeList ChildNodes => m_node.ChildNodes;
	}

	/// <summary>
	/// A USX node that represents a paragraph (including poetry lines, etc.)
	/// </summary>
	public sealed class UsxPara : UsxNode
	{
		/// <summary>
		/// Creates an object representing a USX paragraph based on a USX node.
		/// </summary>
		public UsxPara(XmlNode node) : base(node)
		{
			if (node.Name != kParaNodeName)
				throw new ArgumentException($"Not a valid {kParaNodeName} node.",
					nameof(node));
		}
	}

	/// <summary>
	/// A USX node that specifies the start or end of a run of text marked with a character style
	/// </summary>
	public sealed class UsxChar : UsxNode
	{
		/// <summary>
		/// Creates an object representing a USX paragraph based on a USX node.
		/// </summary>
		public UsxChar(XmlNode node) : base(node)
		{
			if (node.Name != kCharNodeName)
				throw new ArgumentException($"Not a valid {kCharNodeName} node.",
					nameof(node));
		}
	}

	/// <summary>
	/// A USX node that specifies a chapter start or end
	/// </summary>
	public sealed class UsxChapter : UsxNode
	{
		/// <summary>
		/// Creates an object representing the start or end of a chapter based
		/// on a USX node.
		/// </summary>
		public UsxChapter(XmlNode node) : base(node)
		{
			if (node.Name != kChapterNodeName)
				throw new ArgumentException($"Not a valid {kChapterNodeName} node.",
					nameof(node));
		}

		/// <summary>
		/// Gets the style tag (SFM without the leading backslash) of this node.
		/// </summary>
		/// <remarks>chapter end nodes do not correspond to any specific marker in USFM and
		/// therefore do not have a StyleTag defined. Rather than returning <c>null</c>, we could
		/// return "c" or a fictitious "c*" or even throw an exception (as we used to do), but
		/// <c>null</c> seems to best represent the situation.</remarks>
		public override string StyleTag => IsChapterStart ? base.StyleTag : null;

		/// <summary>
		/// Gets whether this node represents the start (as opposed to the end) of a chapter.
		/// </summary>
		/// <remarks>Starting with USX v. 3, chapter nodes can have an "eid" attribute (and no
		/// "number" attribute) to indicate the end of a chapter. A chapter node should never have
		/// both an "sid" and an "eid" attribute, but to accommodate older versions of USX, we
		/// can't just check for the presence of an "sid" attribute. We probably could just check
		/// for the presence of a "number" attribute or the absence of an "eid" attribute, but just
		/// in case some future version of USX allows the number to be specified for an end node or
		/// the possibility of both an "sid" and an "eid" in the same node (neither of which seems
		/// likely, since this would definitely break backwards compatibility), we'll take this
		/// safer approach of explicitly looking for an "sid" or the absence of an "eid".</remarks>
		public bool IsChapterStart => GetAttribute("sid") != null || GetAttribute("eid") == null;

		/// <summary>
		/// Gets the chapter number.
		/// </summary>
		/// <exception cref="NullReferenceException">This object was constructed from an invalid
		/// USX chapter node (having neither a "number" attribute nor an "eid" attribute).
		/// </exception>
		/// <remarks>Previously this would throw an exception if accessed for a chapter node
		/// that is not a chapter start, but now it correctly retrieves the chapter number
		/// from the "eid" attribute if it is formatted correctly. This is (subtly) a breaking
		/// change since this used to throw an (unadvertised) exception in this case. A caller
		/// must now explicitly check the <see cref="IsChapterStart"/> property to know how to
		/// interpret this property.</remarks>
		public string ChapterNumber =>
			GetAttribute("number")?.Value ?? GetAttribute("eid").Value.Split(' ').Last();
	}
}
