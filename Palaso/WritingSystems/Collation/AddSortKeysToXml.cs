using System;
using System.Globalization;
using System.Xml.XPath;
using Palaso.Base32;

namespace Palaso.WritingSystems.Collation
{
	public class AddSortKeysToXml
	{
		public delegate SortKey SortKeyGenerator(string s);

		/// <summary>
		/// Annotate an xml document with a sort key suitable for xslt version 1 sorting algorithms (use lang='en')
		/// </summary>
		/// <param name="document">input to add sort keys to</param>
		/// <param name="xpathSortKeySource">an xpath that selects the source to use to create a sort key</param>
		/// <param name="sortKeyGenerator">delegate that returns a SortKey given a string</param>
		/// <param name="xpathElementToPutSortKeyAttributeIn">a relative xpath (from the source) that selects the element to put the sortkey attribute in</param>
		/// <param name="attribute">The sort key attribute</param>
		public static void AddSortKeys(XPathNavigator document, string xpathSortKeySource, SortKeyGenerator sortKeyGenerator, string xpathElementToPutSortKeyAttributeIn, string attribute)
		{
			AddSortKeys(document,
						xpathSortKeySource,
						sortKeyGenerator,
						xpathElementToPutSortKeyAttributeIn,
						null,
						attribute,
						null);
		}

		/// <summary>
		/// Annotate an xml document with a sort key suitable for xslt version 1 sorting algorithms (use lang='en')
		/// </summary>
		/// <param name="document">input to add sort keys to</param>
		/// <param name="xpathSortKeySource">an xpath that selects the source to use to create a sort key</param>
		/// <param name="sortKeyGenerator">delegate that returns a SortKey given a string</param>
		/// <param name="xpathElementToPutSortKeyAttributeIn">a relative xpath (from the source) that selects the element to put the sortkey attribute in</param>
		/// <param name="prefix">The prefix of the sort-key attribute</param>
		/// <param name="attribute">The sort key attribute</param>
		/// <param name="namespaceUri">The namespace of the sortkey attribute</param>
		public static void AddSortKeys(XPathNavigator document, string xpathSortKeySource, SortKeyGenerator sortKeyGenerator, string xpathElementToPutSortKeyAttributeIn, string prefix, string attribute, string namespaceUri)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}
			if (xpathSortKeySource == null)
			{
				throw new ArgumentNullException("xpathSortKeySource");
			}
			if (sortKeyGenerator == null)
			{
				throw new ArgumentNullException("sortKeyGenerator");
			}
			if (xpathElementToPutSortKeyAttributeIn == null)
			{
				throw new ArgumentNullException("xpathElementToPutSortKeyAttributeIn");
			}
			if (attribute == null)
			{
				throw new ArgumentNullException("attribute");
			}
			if (prefix != null | namespaceUri != null) // if both are null it's okay but if only one it's an error
			{
				if (prefix == null)
				{
					throw new ArgumentNullException("prefix");
				}
				if (prefix.Length == 0)
				{
					throw new ArgumentException("Invalid prefix. Prefix cannot be empty.");
				}
				if (namespaceUri == null)
				{
					throw new ArgumentNullException("prefix");
				}
				if (namespaceUri.Length == 0)
				{
					throw new ArgumentException("Invalid namespace URI. Cannot be empty.");
				}
			}
			XPathExpression compiledXpathElementToPutSortKeyAttributeIn = XPathExpression.Compile(xpathElementToPutSortKeyAttributeIn);
			foreach (XPathNavigator sortKeySource in document.Select(xpathSortKeySource))
			{
				byte[] sortKeyData = sortKeyGenerator(sortKeySource.Value).KeyData;
				string sortKeyBase32 = Base32Convert.ToBase32HexString(sortKeyData, Base32FormattingOptions.None);
				XPathNavigator elementToPutSortKeyAttributeIn = sortKeySource.SelectSingleNode(compiledXpathElementToPutSortKeyAttributeIn);
				if (elementToPutSortKeyAttributeIn.MoveToAttribute(attribute, namespaceUri??string.Empty))
				{
				   elementToPutSortKeyAttributeIn.DeleteSelf();
				}
				elementToPutSortKeyAttributeIn.CreateAttribute(prefix,
															   attribute,
															   namespaceUri,
															   sortKeyBase32);
			}
		}

	}
}
