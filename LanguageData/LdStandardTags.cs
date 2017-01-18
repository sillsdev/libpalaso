// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using SIL.WritingSystems;
/*
namespace LanguageData
{
	/// <summary>
	/// This class parses the IANA subtag registry in order to provide a list of valid language, script, region and variant subtags.
	/// This is an extenstion of StandardSubtags to process a different file to allow testing of the latest registry
	/// </summary>
	public class LdStandardTags : StandardSubtags
	{

		public LdStandardTags(IDictionary<string,string> sourcefiles)
		{
			// need to load these in from files not resources so that we can use new source files that have been downloaded
			string twotothreecodes = sourcefiles["TwoToThreeCodes.txt"];
			string[] encodingPairs = twotothreecodes.Replace("\r\n", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			string subtagregistry = sourcefiles["ianaSubtagRegistry.txt"];
			string[] ianaSubtagsAsStrings = subtagregistry.Split(new[] { "%%" }, StringSplitOptions.None);
			InitialiseIanaSubtags(encodingPairs, ianaSubtagsAsStrings);
			// Iso3Languages not needed by this so no need to populate it
		}
	}
}
*/