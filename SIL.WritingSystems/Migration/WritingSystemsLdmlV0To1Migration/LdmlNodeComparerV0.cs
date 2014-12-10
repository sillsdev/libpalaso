using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	/// <summary>
	/// Class for comparison of order of nodes in an LDML document.
	/// Based on http://www.unicode.org/cldr/data/docs/design/ldml_canonical_form.html
	/// The supplemental metadata shipped with the current CLDR release will have more complete
	/// element and attribute order data.
	/// </summary>
	class LdmlNodeComparerV0 : IComparer<XmlNode>
	{
		public static LdmlNodeComparerV0 Singleton
		{
			get
			{
				if (_singleton == null)
				{
					_singleton = new LdmlNodeComparerV0();
				}
				return _singleton;
			}
		}

		private static LdmlNodeComparerV0 _singleton;
		private LdmlNodeComparerV0() { }

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

		public static int CompareElementNames(string x, string y)
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
			if (x == "special" && y != "special")
			{
				return 1;
			}
			if (y == "special" && x != "special")
			{
				return -1;
			}
			return GetItemStrength(x, _elementNameValues).CompareTo(GetItemStrength(y, _elementNameValues));
		}

		public static int CompareAttributeNames(string x, string y)
		{
			lock (_key)
			{
				if (_attributeNameValues == null)
				{
					_attributeNameValues = BuildOrderDictionary(_attributeOrderedNameList);
				}
			}
			return GetItemStrength(x, _attributeNameValues).CompareTo(GetItemStrength(y, _attributeNameValues));
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
			"ldml", "alternate", "attributeOrder", "attributes", "blockingItems", "calendarSystem", "character",
			"character-fallback", "codePattern", "codesByTerritory", "comment", "context", "cp", "deprecatedItems",
			"distinguishingItems", "elementOrder", "first_variable", "fractions", "identity", "info", "languageAlias",
			"languageCodes", "languageCoverage", "languagePopulation", "last_variable", "first_tertiary_ignorable",
			"last_tertiary_ignorable", "first_secondary_ignorable", "last_secondary_ignorable",
			"first_primary_ignorable", "last_primary_ignorable", "first_non_ignorable", "last_non_ignorable",
			"first_trailing", "last_trailing", "likelySubtag", "mapTimezones", "mapZone", "pluralRule", "pluralRules",
			"reference", "region", "scriptAlias", "scriptCoverage", "serialElements", "substitute", "suppress",
			"tRule", "telephoneCountryCode", "territoryAlias", "territoryCodes", "territoryCoverage",
			"currencyCoverage", "timezone", "timezoneCoverage", "transform", "usesMetazone", "validity", "alias",
			"appendItem", "base", "beforeCurrency", "afterCurrency", "currencyMatch", "dateFormatItem", "day",
			"deprecated", "distinguishing", "blocking", "coverageAdditions", "era", "eraNames", "eraAbbr",
			"eraNarrow", "exemplarCharacters", "fallback", "field", "generic", "greatestDifference", "height",
			"hourFormat", "hoursFormat", "gmtFormat", "intervalFormatFallback", "intervalFormatItem", "key",
			"localeDisplayNames", "layout", "localeDisplayPattern", "languages", "localePattern", "localeSeparator",
			"localizedPatternChars", "dateRangePattern", "calendars", "long", "mapping", "measurementSystem",
			"measurementSystemName", "messages", "minDays", "firstDay", "month", "months", "monthNames", "monthAbbr",
			"days", "dayNames", "dayAbbr", "orientation", "inList", "inText", "paperSize", "pattern", "displayName",
			"quarter", "quarters", "quotationStart", "quotationEnd", "alternateQuotationStart",
			"alternateQuotationEnd", "regionFormat", "fallbackFormat", "abbreviationFallback", "preferenceOrdering",
			"relative", "reset", "p", "pc", "rule", "s", "sc", "scripts", "segmentation", "settings", "short",
			"commonlyUsed", "exemplarCity", "singleCountries", "default", "calendar", "collation", "currency",
			"currencyFormat", "currencySpacing", "currencyFormatLength", "dateFormat", "dateFormatLength",
			"dateTimeFormat", "dateTimeFormatLength", "availableFormats", "appendItems", "dayContext", "dayWidth",
			"decimalFormat", "decimalFormatLength", "intervalFormats", "monthContext", "monthWidth", "percentFormat",
			"percentFormatLength", "quarterContext", "quarterWidth", "scientificFormat", "scientificFormatLength",
			"skipDefaultLocale", "defaultContent", "standard", "daylight", "suppress_contractions", "optimize",
			"rules", "surroundingMatch", "insertBetween", "symbol", "decimal", "group", "list", "percentSign",
			"nativeZeroDigit", "patternDigit", "plusSign", "minusSign", "exponential", "perMille", "infinity", "nan",
			"currencyDecimal", "currencyGroup", "symbols", "decimalFormats", "scientificFormats", "percentFormats",
			"currencyFormats", "currencies", "t", "tc", "q", "qc", "i", "ic", "extend", "territories", "timeFormat",
			"timeFormatLength", "timeZoneNames", "type", "unit", "unitPattern", "unitName", "variable",
			"attributeValues", "variables", "segmentRules", "variantAlias", "variants", "keys", "types",
			"measurementSystemNames", "codePatterns", "version", "generation", "currencyData", "language", "script",
			"territory", "territoryContainment", "languageData", "territoryInfo", "calendarData", "variant", "week",
			"am", "pm", "eras", "dateFormats", "timeFormats", "dateTimeFormats", "fields", "weekData",
			"measurementData", "timezoneData", "characters", "delimiters", "measurement", "dates", "numbers",
			"transforms", "metadata", "codeMappings", "likelySubtags", "metazoneInfo", "plurals", "telephoneCodeData",
			"units", "collations", "posix", "segmentations", "references", "weekendStart", "weekendEnd", "width", "x",
			"yesstr", "nostr", "yesexpr", "noexpr", "zone", "metazone", "special", "zoneAlias", "zoneFormatting",
			"zoneItem", "supplementalData"};

		static readonly string[] _attributeOrderedNameList = new string[] {
			"_q", "type", "id", "choice", "key", "registry", "source", "target", "path", "day", "date", "version",
			"count", "lines", "characters", "iso4217", "before", "from", "to", "mzone", "number", "time", "casing",
			"list", "uri", "digits", "rounding", "iso3166", "hex", "request", "direction", "alternate", "backwards",
			"caseFirst", "caseLevel", "hiraganaQuarternary", "hiraganaQuaternary", "variableTop", "normalization",
			"numeric", "strength", "elements", "element", "attributes", "attribute", "aliases", "attributeValue",
			"contains", "multizone", "order", "other", "replacement", "scripts", "services", "territories",
			"territory", "tzidVersion", "value", "values", "variant", "variants", "visibility", "alpha3", "code",
			"end", "exclude", "fips10", "gdp", "internet", "literacyPercent", "locales", "officialStatus",
			"population", "populationPercent", "start", "used", "writingPercent", "validSubLocales", "standard",
			"references", "alt", "draft"};

		private static Dictionary<string, int> _elementNameValues;
		private static Dictionary<string, int> _attributeNameValues;
		// this is a map of of "element/attribute" string to a map of values to weights
		private static Dictionary<string, Dictionary<string, int>> _valueValues;

		private static int CompareElementNames(XmlNode x, XmlNode y)
		{
			return CompareElementNames(x.Name, y.Name);
		}

		private static int CompareAttributeNames(XmlNode x, XmlNode y)
		{
			return CompareAttributeNames(x.Name, y.Name);
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
			XmlNode ownerElement = ((XmlAttribute)x).OwnerElement ?? ((XmlAttribute)y).OwnerElement;
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
				new string[] { "sun", "mon", "tue", "wed", "thu", "fri", "sat" });
			_valueValues["weekendStart/day"] = workingList;
			_valueValues["weekendEnd/day"] = workingList;
			_valueValues["day/type"] = workingList;
			workingList = BuildOrderDictionary(new string[] { "full", "long", "medium", "short" });
			_valueValues["dateFormatLength/type"] = workingList;
			_valueValues["timeFormatLength/type"] = workingList;
			_valueValues["dateTimeFormatLength/type"] = workingList;
			_valueValues["decimalFormatLength/type"] = workingList;
			_valueValues["scientificFormatLength/type"] = workingList;
			_valueValues["percentFormatLength/type"] = workingList;
			_valueValues["currencyFormatLength/type"] = workingList;
			workingList = BuildOrderDictionary(new string[] { "wide", "abbreviated", "narrow" });
			_valueValues["monthWidth/type"] = workingList;
			_valueValues["dayWidth/type"] = workingList;
			workingList = BuildOrderDictionary(
				new string[] { "era", "year", "month", "week", "day", "weekday", "dayperiod", "hour", "minute", "second", "zone" });
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
