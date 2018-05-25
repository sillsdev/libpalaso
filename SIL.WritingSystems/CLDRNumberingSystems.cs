// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class wraps the numberingSystems.xml file from CLDR - To update replace the file in the CLDRNumberingResources.resx file with
	/// the latest data.
	/// </summary>
    public static class CLDRNumberingSystems
    {
	    private static Dictionary<string, string> _numberingSystemsDictionary = new Dictionary<string, string>();
	    static CLDRNumberingSystems()
	    {
		    LoadNumberingSystems();
	    }

	    private static void LoadNumberingSystems()
	    {
			var cldrNumSysDoc = XDocument.Parse(CLDRResources.numberingSystems);
			var numericNumberingSystems = cldrNumSysDoc.XPathSelectElements("/supplementalData/numberingSystems/numberingSystem[@type='numeric']");
		    foreach (var numberingSystem in numericNumberingSystems)
		    {
			    _numberingSystemsDictionary[numberingSystem.Attribute("digits").Value] = numberingSystem.Attribute("id").Value;
		    }
	    }

	    public static string FindNumberingSystemID(string digits)
	    {
		    _numberingSystemsDictionary.TryGetValue(digits, out var id);
		    return id;
	    }

	    public static string GetDigitsForID(string id)
	    {
		    foreach (var keyValuePair in _numberingSystemsDictionary)
		    {
			    if (keyValuePair.Value.Equals(id))
				    return keyValuePair.Key;
		    }

		    return null;
	    }
    }
}
