using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// Class for comparison of order of nodes in an LDML document.
	/// Based on http://www.unicode.org/cldr/data/docs/design/ldml_canonical_form.html
	/// </summary>
	class LdmlNodeComparer : IComparer<XmlNode>
	{
		public static LdmlNodeComparer Singleton
		{
			get
			{
				if (_singleton == null)
				{
					_singleton = new LdmlNodeComparer();
				}
				return _singleton;
			}
		}

		private static LdmlNodeComparer _singleton;
		private LdmlNodeComparer() { }

		public int Compare(XmlNode x, XmlNode y)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}
			if (y == null)
			{
				return 1;
			}
			int result = CompareNodeTypes(x, y);
			if (result != 0)
			{
				return result;
			}
			switch (x.NodeType)
			{
				case XmlNodeType.Element:
					result = CompareElementNames(x, y);
					if (result != 0)
					{
						return result;
					}
					result = CompareElementAttributes(x, y);
					break;
				case XmlNodeType.Attribute:
					result = CompareAttributeNames(x, y);
					if (result != 0)
					{
						return result;
					}
					result = CompareAttributeValues(x, y);
					break;
			}
			return result;
		}

		private static readonly object _key = new object();
		private static Dictionary<XmlNodeType, int> _nodeTypeStrengths;

		private static int GetItemStrength<T>(T item, IDictionary<T, int> strengthMap)
		{
			Debug.Assert(strengthMap != null);
			return strengthMap.ContainsKey(item) ? strengthMap[item] : int.MaxValue;
		}

		private static int CompareNodeTypes(XmlNode x, XmlNode y)
		{
			lock (_key)
			{
				if (_nodeTypeStrengths == null)
				{
					_nodeTypeStrengths = new Dictionary<XmlNodeType, int>();
					_nodeTypeStrengths[XmlNodeType.Attribute] = 1;
					_nodeTypeStrengths[XmlNodeType.Element] = 2;
				}
			}
			return GetItemStrength(x.NodeType, _nodeTypeStrengths).CompareTo(GetItemStrength(y.NodeType, _nodeTypeStrengths));
		}

		static readonly string[] _elementOrderedNameList = new string[] {
			"ldml", "identity", "alias", "localeDisplayNames", "layout", "characters", "delimiters", "measurement",
			"dates", "numbers", "collations", "posix", "version", "generation", "language", "script", "territory",
			"variant", "languages", "scripts", "territories", "variants", "keys", "types", "key", "type", "orientation",
			"exemplarCharacters", "mapping", "cp", "quotationStart", "quotationEnd", "alternateQuotationStart",
			"alternateQuotationEnd", "measurementSystem", "paperSize", "height", "width", "localizedPatternChars",
			"calendars", "timeZoneNames", "months", "monthNames", "monthAbbr", "days", "dayNames", "dayAbbr", "week",
			"am", "pm", "eras", "dateFormats", "timeFormats", "dateTimeFormats", "fields", "month", "day", "minDays",
			"firstDay", "weekendStart", "weekendEnd", "eraNames", "eraAbbr", "era", "pattern", "displayName",
			"hourFormat", "hoursFormat", "gmtFormat", "regionFormat", "fallbackFormat", "abbreviationFallback",
			"preferenceOrdering", "default", "calendar", "monthContext", "monthWidth", "dayContext", "dayWidth",
			"dateFormatLength", "dateFormat", "timeFormatLength", "timeFormat", "dateTimeFormatLength", "dateTimeFormat",
			"zone", "long", "short", "exemplarCity", "generic", "standard", "daylight", "field", "relative", "symbols",
			"decimalFormats", "scientificFormats", "percentFormats", "currencyFormats", "currencies",
			"decimalFormatLength", "decimalFormat", "scientificFormatLength", "scientificFormat", "percentFormatLength",
			"percentFormat", "currencyFormatLength", "currencyFormat", "currency", "symbol", "decimal", "group", "list",
			"percentSign", "nativeZeroDigit", "patternDigit", "plusSign", "minusSign", "exponential", "perMille",
			"infinity", "nan", "collation", "messages", "yesstr", "nostr", "yesexpr", "noexpr", "special"};

		static readonly string[] _attributeOrderedNameList = new string[] {
			"type", "key", "registry", "alt", "source", "path", "day", "date", "version", "count", "lines",
			"characters", "before", "number", "time", "validSubLocales", "standard", "references", "draft"};

		private static Dictionary<string, int> _elementNameValues;
		private static Dictionary<string, int> _attributeNameValues;
		// this is a map of of "element/attribute" string to a map of values to weights
		private static Dictionary<string, Dictionary<string, int>> _valueValues;

		private static int CompareElementNames(XmlNode x, XmlNode y)
		{
			lock (_key)
			{
				if (_elementNameValues == null)
				{
					_elementNameValues = BuildOrderDictionary(_elementOrderedNameList);
				}
			}
			// Any element named "special" always comes last, even after other new elements that may have been
			// added to the standard after this code was written
			if (x.Name == "special" && y.Name != "special")
			{
				return 1;
			}
			if (y.Name == "special" && x.Name != "special")
			{
				return -1;
			}
			return GetItemStrength(x.Name, _elementNameValues).CompareTo(GetItemStrength(y.Name, _elementNameValues));
		}

		private static int CompareAttributeNames(XmlNode x, XmlNode y)
		{
			lock (_key)
			{
				if (_attributeNameValues == null)
				{
					_attributeNameValues = BuildOrderDictionary(_attributeOrderedNameList);
				}
			}
			return GetItemStrength(x.Name, _attributeNameValues).CompareTo(GetItemStrength(y.Name, _attributeNameValues));
		}

		private int CompareElementAttributes(XmlNode x, XmlNode y)
		{
			for (int i = 0; i < x.Attributes.Count && i < y.Attributes.Count; i++)
			{
				int result = Compare(x.Attributes[i], y.Attributes[i]);
				if (result != 0)
				{
					return result;
				}
			}
			return x.Attributes.Count.CompareTo(y.Attributes.Count);
		}

		/// <summary>
		/// Compares two attribute values.  There is a value order table that defines the order of values
		/// for some attributes of some elements.  After that, sort in numeric order, and then in alphabetic order.
		/// There is a more complicated rule for the "type" attribute of the "zone" element, but since we're
		/// not using that element at this time, I'm not going to try to write code for it.
		/// </summary>
		private static int CompareAttributeValues(XmlNode x, XmlNode y)
		{
			lock (_key)
			{
				if (_valueValues == null)
				{
					BuildValueOrderLists();
				}
			}
			int result = 0;
			XmlNode ownerElement = ((XmlAttribute) x).OwnerElement ?? ((XmlAttribute) y).OwnerElement;
			string elementName = ownerElement == null ? string.Empty : ownerElement.Name;
			string valueListKey = elementName + "/" + x.Name;
			if (_valueValues.ContainsKey(valueListKey))
			{
				result = GetItemStrength(x.Value, _valueValues[valueListKey]).CompareTo(
					GetItemStrength(y.Value, _valueValues[valueListKey]));
			}
			if (result != 0)
			{
				return result;
			}
			double xNumericValue, yNumericValue;
			bool xIsNumeric = double.TryParse(x.Value, out xNumericValue);
			bool yIsNumeric = double.TryParse(y.Value, out yNumericValue);
			if (xIsNumeric && !yIsNumeric)
			{
				result = -1;
			}
			else if (!xIsNumeric && yIsNumeric)
			{
				result = 1;
			}
			else if (xIsNumeric && yIsNumeric)
			{
				result = xNumericValue.CompareTo(yNumericValue);
			}
			if (result == 0)
			{
				result = x.Value.CompareTo(y.Value);
			}
			return result;
		}

		private static void BuildValueOrderLists()
		{
			_valueValues = new Dictionary<string, Dictionary<string, int>>(14);
			Dictionary<string, int> workingList = BuildOrderDictionary(
				new string[] {"sun", "mon", "tue", "wed", "thu", "fri", "sat"});
			_valueValues["weekendStart/day"] = workingList;
			_valueValues["weekendEnd/day"] = workingList;
			_valueValues["day/type"] = workingList;
			workingList = BuildOrderDictionary(new string[] {"full", "long", "medium", "short"});
			_valueValues["dateFormatLength/type"] = workingList;
			_valueValues["timeFormatLength/type"] = workingList;
			_valueValues["dateTimeFormatLength/type"] = workingList;
			_valueValues["decimalFormatLength/type"] = workingList;
			_valueValues["scientificFormatLength/type"] = workingList;
			_valueValues["percentFormatLength/type"] = workingList;
			_valueValues["currencyFormatLength/type"] = workingList;
			workingList = BuildOrderDictionary(new string[] {"wide", "abbreviated", "narrow"});
			_valueValues["monthWidth/type"] = workingList;
			_valueValues["dayWidth/type"] = workingList;
			workingList = BuildOrderDictionary(
				new string[] {"era", "year", "month", "week", "day", "weekday", "dayperiod", "hour", "minute", "second", "zone"});
			_valueValues["field/type"] = workingList;
		}

		private static Dictionary<string, int> BuildOrderDictionary(string[] valuesInOrder)
		{
			Debug.Assert(valuesInOrder != null);
			Dictionary<string, int> result = new Dictionary<string, int>(valuesInOrder.Length);
			for (int i = 0; i < valuesInOrder.Length; i++)
			{
				result.Add(valuesInOrder[i], i);
			}
			return result;
		}
	}
}
