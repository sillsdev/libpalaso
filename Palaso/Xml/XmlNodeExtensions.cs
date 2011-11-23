using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Palaso.Code;

namespace Palaso.Xml
{
	public static class XmlNodeExtensions
	{

		/// <summary>
		/// this is safe to use with foreach, unlike SelectNodes
		/// </summary>
		public static XmlNodeList SafeSelectNodes(this XmlNode node, string path, XmlNamespaceManager namespaceManager)
		{
			var x = node.SelectNodes(path, namespaceManager);
			if (x == null)
				return new NullXMlNodeList();
			return x;
		}

		/// <summary>
		/// honors default namespace and will return an empty list rather than null
		/// </summary>
		public static XmlNodeList SafeSelectNodes(this XmlNode node, string path)
		{
			Guard.AgainstNull(node, "SafeSelectNodes(node,"+path+"): node was null");
			//REVIEW JH(jh): this will put pfx in front of every element in the path, but in html, that actually makes the queries fail.
			const string prefix = "pfx";
			XmlNamespaceManager nsmgr = GetNsmgr(node, prefix);
			string prefixedPath = GetPrefixedPath(path, prefix);
			var x= node.SelectNodes(prefixedPath, nsmgr);

			if (x == null)
				return new NullXMlNodeList();
			return x;
		}

		public static string SelectTextPortion(this XmlNode node, string path, params object[] args)
		{
			var x = node.SelectNodes(string.Format(path, args));
			if (x == null || x.Count ==0)
				return string.Empty;
			return x[0].InnerText;
		}

		public static string GetStringAttribute(this XmlNode node, string attr)
		{
			try
			{
				return node.Attributes[attr].Value;
			}
			catch (NullReferenceException)
			{
				throw new XmlFormatException(string.Format("Expected a '{0}' attribute on {1}.", attr, node.OuterXml));
			}
		}
		public static string GetOptionalStringAttribute(this XmlNode node, string attributeName, string defaultValue)
		{
			XmlAttribute attr = node.Attributes[attributeName];
			if (attr == null)
				return defaultValue;
			return attr.Value;
		}

		#region HonorDefaultNamespace  // from http://stackoverflow.com/questions/585812/using-xpath-with-default-namespace-in-c/2054877#2054877

		public const string DefaultNamespacePrefix = "pfx";

		/// <summary>
		/// This is for doing selections in xhtml, where there is a default namespace, which makes
		/// normal selects fail.  This tries to set a namespace and inject prefix into the xpath.
		/// </summary>
		public static XmlNode SelectSingleNodeHonoringDefaultNS(this XmlNode node, string path)
		{

			XmlNamespaceManager nsmgr = GetNsmgr(node, DefaultNamespacePrefix);
			string prefixedPath = GetPrefixedPath(path, DefaultNamespacePrefix);
			return node.SelectSingleNode(prefixedPath, nsmgr);
		}


		private static XmlNamespaceManager GetNsmgr(XmlNode node, string prefix)
		{
			Guard.AgainstNull(node, "GetNsmgr(node, prefix): node was null");
			string namespaceUri;
			XmlNameTable nameTable;
			try
			{
				if (node is XmlDocument)
				{
					nameTable = ((XmlDocument) node).NameTable;
					Guard.AgainstNull(((XmlDocument) node).DocumentElement, "((XmlDocument) node).DocumentElement");
					namespaceUri = ((XmlDocument) node).DocumentElement.NamespaceURI;
				}
				else
				{
					Guard.AgainstNull(node.OwnerDocument, "node.OwnerDocument");
					nameTable = node.OwnerDocument.NameTable;
					namespaceUri = node.NamespaceURI;
				}
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(nameTable);
				nsmgr.AddNamespace(prefix, namespaceUri);
				return nsmgr;

			}
			catch (Exception error)
			{
				throw new ApplicationException("Could not create a namespace manager for the following node:\r\n"+node.OuterXml,error);
			}
		}


		// review: I (CP) think that this method changes the syntax of xpath to account for the use of a default namespace
		// such that for example:
		//  xpath = a/b
		//  xml = <a xmlns="MyNameSpace"><b></a>
		// would match when it should not.  The xpath should be:
		//  xpath = MyNameSpace:a/MyNameSpace:b
		// bug: The code below currently doesn't allow for a / in a literal string which should not have pfx: prepended.
		private static string GetPrefixedPath(string xPath, string prefix)
		{
			//the code I purloined from stackoverflow didn't cope with axes and the double colon (ancestor::)
			//Rather than re-write it, I just get the axes out of the way, then put them back after we insert the prefix
			var axes = new List<string>(new[] {"ancestor","ancestor-or-self","attribute","child","descendant","descendant-or-self","following","following-sibling","namespace","parent","preceding","preceding-sibling","self" });
			foreach (var axis in axes)
			{
				xPath = xPath.Replace(axis+"::", "#"+axis);
			}

			char[] validLeadCharacters = "@/".ToCharArray();
			char[] quoteChars = "\'\"".ToCharArray();

			List<string> pathParts = xPath.Split("/".ToCharArray()).ToList();
			string result = string.Join("/",
										pathParts.Select(
											x =>
											(string.IsNullOrEmpty(x) ||
											 x.IndexOfAny(validLeadCharacters) == 0 ||
											 (x.IndexOf(':') > 0 &&
											  (x.IndexOfAny(quoteChars) < 0 || x.IndexOfAny(quoteChars) > x.IndexOf(':'))))
												? x
												: prefix + ":" + x).ToArray());

			foreach (var axis in axes)
			{
				if (result.Contains(axis + "-"))//don't match on, e.g., "following" if what we have is "following-sibling"
					continue;
				result = result.Replace(prefix + ":#"+axis, axis+"::" + prefix + ":");
			}

			result = result.Replace(prefix + ":text()", "text()");//remove the pfx from the text()
			result = result.Replace(prefix + ":node()", "node()");
			return result;
		}

		#endregion
	}


	public class NullXMlNodeList : XmlNodeList
	{
		public override XmlNode Item(int index)
		{
			throw new ArgumentOutOfRangeException();
		}

		public override IEnumerator GetEnumerator()
		{
			yield return null;
		}

		public override int Count
		{
			get { return 0; }
		}
	}
	public class XmlFormatException : ApplicationException
	{
		private string _filePath;
		public XmlFormatException(string message)
			: base(message)
		{
		}

		public string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; }
		}
	}
}