// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class wraps the numberingSystems.xml file from CLDR
	/// To update replace the file in the CLDRNumberingResources.resx file with the latest data.
	/// </summary>
	public static class CLDRNumberingSystems
	{
		private static readonly Dictionary<string, string> _mapDigitsToId = new Dictionary<string, string>();

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
				_mapDigitsToId[numberingSystem.Attribute("digits").Value] = numberingSystem.Attribute("id").Value;
			}
		}

		public static string FindNumberingSystemID(string digits)
		{
			_mapDigitsToId.TryGetValue(digits, out var id);
			return id;
		}

		public static string GetDigitsForID(string id)
		{
			foreach (var keyValuePair in _mapDigitsToId)
				if (keyValuePair.Value.Equals(id))
					return keyValuePair.Key;

			return null;
		}

		/// <summary/>
		/// <returns>The list of numbering system digit strings found in the CLDR supplemental data</returns>
		public static List<string> StandardNumberingSystems => _mapDigitsToId.Keys.ToList();
	}
}