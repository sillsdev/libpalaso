using System;
using System.IO;
using SIL.WritingSystems;


namespace LanguageData
{
    /// <summary>
    /// This class parses the IANA subtag registry in order to provide a list of valid language, script, region and variant subtags.
    /// This is an extenstion of StandardSubtags to process a different file to allow testing of the latest registry
    /// </summary>
    public class LdStandardTags : StandardSubtags
    {
        static LdStandardTags()
        {
            // need to load these in from files not resources so that can use new source files that have been downloaded
            string twotothreecodes = File.ReadAllText(@"TwoToThreeCodes.txt");
            string[] encodingPairs = twotothreecodes.Replace("\r\n", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string subtagregistry = File.ReadAllText(@"ianaSubtagRegistry.txt");
            string[] ianaSubtagsAsStrings = subtagregistry.Split(new[] { "%%" }, StringSplitOptions.None);
            InitialiseIanaSubtags(encodingPairs, ianaSubtagsAsStrings);
            // Iso3Languages not needed by this so no need to populate it
        }
    }
}