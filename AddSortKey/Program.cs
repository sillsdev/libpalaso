using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using SIL.WritingSystems;

namespace AddSortKey
{
	class Program
	{

		static void Usage()
		{
			//AddSortKey will create (overwriting if necessary) a sortKey from the content that matches
			// a given xpath statement.
			Console.Error.WriteLine(
					"Usage: AddSortKey input_file output_file source sort location attribute");
			Console.Error.WriteLine("source: an XPath which selects the nodes that will be used to generate sort keys");
			Console.Error.WriteLine("sort: the sort identifier (en-US) or sort rules");
			Console.Error.WriteLine("  sort rules should be prefixed by their type (icu: or simple:)");
			Console.Error.WriteLine("location: a relative xpath (from the source) to the element the sortKey attribute will be put in");
			Console.Error.WriteLine("attribute: the name of the attribute to put the generated sort key in");
		}

		static int Main(string[] args)
		{
			if(args.Length != 6)
			{
				Usage();
				return (1);
			}

			var document = new XmlDocument();
			document.PreserveWhitespace = true;
			document.Load(args[0]);
			XPathNavigator navigator = document.CreateNavigator();


			AddSortKeysToXml.SortKeyGenerator sortKeyGenerator = GetSortKeyGeneratorFromArgument(args[3]);
			string xpathSortKeySource = args[2];
			string xpathPlaceToPutSortKey = args[4];
			string attribute = args[5];
			AddSortKeysToXml.AddSortKeys(navigator, xpathSortKeySource, sortKeyGenerator, xpathPlaceToPutSortKey, attribute);

			document.Save(args[1]);

			return 0;
		}



		private static AddSortKeysToXml.SortKeyGenerator GetSortKeyGeneratorFromArgument(string s)
		{
			string icuRules = null;
			if (s.StartsWith("icu:", StringComparison.CurrentCultureIgnoreCase))
			{
				icuRules = s.Substring(4);
			}
			else if (s.StartsWith("simple:", StringComparison.CurrentCultureIgnoreCase))
			{
				var parser = new SimpleRulesParser();
				icuRules = parser.ConvertToIcuRules(s.Substring(7));
			}

			if (icuRules != null)
			{
				var collator = new IcuRulesCollator(icuRules);
				return collator.GetSortKey;
			}
			return CultureInfo.GetCultureInfo(s).CompareInfo.GetSortKey;
		}

	}
}
