using System;
using System.Collections.Generic;
using System.Linq;
using SIL.WritingSystems;

namespace LanguageData
{
    class Program
    {
 

        static void Usage()
        {
            //LanguageData will process Ethnologue, IANA subtag and ISO693-3 data to a single language index file
            // Tasks/Parameters
            // -g Get fresh source files
            // -c Check if source files have changed (implies -g) and return
            // input directory for where source files will be and fresh ones should go (default LanguageData/Resources)
                // IANA subtags and 2to3lettercodes are used in SIL.WritingSystems by other parts,
                // but Ethnologue LanguageIndex is only used in LanguageLookup
            // output directory for index file (default SIL.WritingSystems/Reources)
            // output filename for index file
            Console.Error.WriteLine(
                    "Usage: LanguageData");
            //Console.Error.WriteLine("source: an XPath which selects the nodes that will be used to generate sort keys");
            //Console.Error.WriteLine("sort: the sort identifier (en-US) or sort rules");
            //Console.Error.WriteLine("  sort rules should be prefixed by their type (icu: or simple:)");
            //Console.Error.WriteLine("location: a relative xpath (from the source) to the element the sortkey attribute will be put in");
            //Console.Error.WriteLine("attribute: the name of the attribute to put the generated sort key in");
        }

        static void Main(string[] args)
        {
            Sldr.Initialize();
			GetAndCheckSources getcheck = new GetAndCheckSources ();
			getcheck.GetOldSources ();
			getcheck.GetNewSources();
			getcheck.CheckSourcesAreDifferent ();
			getcheck.WriteNewFiles (".");
			LanguageIndex langIndex = new LanguageIndex(getcheck.GetFileStrings(false));
            langIndex.WriteIndex();
        }
    }
}
