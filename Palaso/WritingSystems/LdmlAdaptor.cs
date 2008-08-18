using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Collation;

namespace Palaso.WritingSystems
{
	public class LdmlAdaptor
	{
		private XmlNamespaceManager _nameSpaceManager;

		public LdmlAdaptor()
		{
			_nameSpaceManager = MakeNameSpaceManager();
		}

		public void Read(string filePath, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.Load(filePath);
			Read(doc, ws);
		}

		public void Read(XmlReader xmlReader, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.Load(xmlReader);
			Read(doc, ws);
		}

		public void Read(XmlNode parentOfLdmlNode, WritingSystemDefinition ws)
		{
			if (parentOfLdmlNode == null)
			{
				throw new ArgumentNullException("parentOfLdmlNode");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlNode ldmlNode = parentOfLdmlNode.SelectSingleNode("ldml", _nameSpaceManager);
			if (ldmlNode == null)
			{
				return;
			}
			ReadIdentityElement(ldmlNode, ws);
			ReadLayoutElement(ldmlNode, ws);
			ReadCollationElement(ldmlNode, ws);

			ws.Abbreviation = GetSpecialValue(ldmlNode, "abbreviation");
			ws.LanguageName = GetSpecialValue(ldmlNode, "languageName");
			ws.DefaultFontName = GetSpecialValue(ldmlNode, "defaultFontFamily");
			float fontSize;
			if (float.TryParse(GetSpecialValue(ldmlNode, "defaultFontSize"), out fontSize))
			{
				ws.DefaultFontSize = fontSize;
			}
			ws.Keyboard = GetSpecialValue(ldmlNode, "defaultKeyboard");
			ws.SpellCheckingId = GetSpecialValue(ldmlNode, "spellCheckingId");
			ws.StoreID = "";
			ws.Modified = false;
		}

		private void ReadIdentityElement(XmlNode ldmlNode, WritingSystemDefinition ws)
		{
			XmlNode identityNode = ldmlNode.SelectSingleNode("identity", _nameSpaceManager);
			if (identityNode == null)
			{
				return;
			}
			ws.ISO = GetSubNodeAttributeValue(identityNode, "language", "type");
			ws.Variant = GetSubNodeAttributeValue(identityNode, "variant", "type");
			ws.Region = GetSubNodeAttributeValue(identityNode, "territory", "type");
			ws.Script = GetSubNodeAttributeValue(identityNode, "script", "type");
			string dateTime = GetSubNodeAttributeValue(identityNode, "generation", "date");
			ws.DateModified = DateTime.Parse(dateTime);
			XmlNode versionNode = identityNode.SelectSingleNode("version");
			ws.VersionNumber = XmlHelpers.GetOptionalAttributeValue(versionNode, "number");
			ws.VersionDescription = versionNode == null ? string.Empty : versionNode.InnerText;
		}

		private void ReadLayoutElement(XmlNode ldmlNode, WritingSystemDefinition ws)
		{
			// The orientation node has two attributes, "lines" and "characters" which define direction of writing.
			// The valid values are: "top-to-bottom", "bottom-to-top", "left-to-right", and "right-to-left"
			// Currently we only handle horizontal character orders with top-to-bottom line order, so
			// any value other than characters right-to-left, we treat as our default left-to-right order.
			// This probably works for many scripts such as various East Asian scripts which traditionally
			// are top-to-bottom characters and right-to-left lines, but can also be written with
			// left-to-right characters and top-to-bottom lines.
			ws.RightToLeftScript = GetSubNodeAttributeValue(ldmlNode, "layout/orientation", "characters") ==
								   "right-to-left";
		}

		private void ReadCollationElement(XmlNode ldmlNode, WritingSystemDefinition ws)
		{
			// no type is the same as type=standard, and is the only one we're interested in
			XmlNode node = ldmlNode.SelectSingleNode("collations/collation[@type='']") ??
						   ldmlNode.SelectSingleNode("collations/collation[@type='standard']");
			if (node == null)
			{
				return;
			}
			string rulesType = GetSpecialValue(node, "sortRulesType");
			if (!Enum.IsDefined(typeof (WritingSystemDefinition.SortRulesType), rulesType))
			{
				rulesType = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			}
			ws.SortUsing = rulesType;
			switch ((WritingSystemDefinition.SortRulesType)Enum.Parse(typeof(WritingSystemDefinition.SortRulesType), rulesType))
			{
				case WritingSystemDefinition.SortRulesType.OtherLanguage:
					ReadCollationRulesForOtherLanguage(node, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomSimple:
					ReadCollationRulesForCustomSimple(node, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomICU:
					ReadCollationRulesForCustomICU(node, ws);
					break;
				default:
					string message = string.Format("Unhandled SortRulesType '{0}' while writing LDML definition file.", ws.SortUsing);
					throw new ApplicationException(message);
			}
		}

		private void ReadCollationRulesForOtherLanguage(XmlNode node, WritingSystemDefinition ws)
		{
			XmlNode alias = node.SelectSingleNode("alias", _nameSpaceManager);
			if (alias != null)
			{
				ws.SortRules = XmlHelpers.GetOptionalAttributeValue(alias, "source", string.Empty);
			}
			else
			{
				// missing alias element, fall back to ICU rules
				ws.SortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
				ReadCollationRulesForCustomICU(node, ws);
			}
		}

		private void ReadCollationRulesForCustomICU(XmlNode node, WritingSystemDefinition ws)
		{
			ws.SortRules = LdmlCollationParser.GetIcuRulesFromCollationNode(node, _nameSpaceManager);
		}

		private void ReadCollationRulesForCustomSimple(XmlNode node, WritingSystemDefinition ws)
		{
			string rules;
			if (LdmlCollationParser.TryGetSimpleRulesFromCollationNode(node, _nameSpaceManager, out rules))
			{
				ws.SortRules = rules;
				return;
			}
			// fall back to ICU rules if Simple rules don't work
			ws.SortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			ReadCollationRulesForCustomICU(node, ws);
		}

		public void Write(string filePath, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.CreateXmlDeclaration("1.0", "", "no");
			WriteToDom(doc, ws);
			doc.Save(filePath);
		}

		public void Write(XmlWriter xmlWriter, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			WriteToDom(doc, ws);
			doc.Save(xmlWriter); //??? Not sure about this, does this need to be Write(To) or similar?
		}

		public void FillWithDefaults(string rfc4646, WritingSystemDefinition ws)
		{
			string id = rfc4646.ToLower();
			switch (id)
			{
				case "en-latn":
					ws.ISO = "en";
					ws.LanguageName = "English";
					ws.Abbreviation = "eng";
					ws.Script = "Latn";
					break;
				 default:
					ws.Script = "Latn";
					break;
			}
		}

		private string GetSpecialValue(XmlNode parent, string field)
		{
			XmlNode node = parent.SelectSingleNode("special/palaso:"+field, _nameSpaceManager);
			return XmlHelpers.GetOptionalAttributeValue(node, "value", string.Empty);
		}

		private string GetSubNodeAttributeValue(XmlNode parent, string path, string attributeName)
		{
			XmlNode node = parent.SelectSingleNode(path);
			return XmlHelpers.GetOptionalAttributeValue(node, attributeName, string.Empty);
		}

		public static XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNamespaceManager m = new XmlNamespaceManager(new NameTable());
			m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			return m;
		}

		public void WriteToDom(XmlNode parentOfLdmlNode, WritingSystemDefinition ws)
		{
			if (parentOfLdmlNode == null)
			{
				throw new ArgumentNullException("parentOfLdmlNode");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlNode ldmlNode = XmlHelpers.GetOrCreateElement(parentOfLdmlNode, ".", "ldml", null, _nameSpaceManager);

			UpdateIdentityElement(ldmlNode, ws);
			UpdateLayoutElement(ldmlNode, ws);
			UpdateCollationElement(ldmlNode, ws);

			SetSpecialNode(ldmlNode, "languageName", ws.LanguageName);
			SetSpecialNode(ldmlNode, "abbreviation", ws.Abbreviation);
			SetSpecialNode(ldmlNode, "defaultFontFamily", ws.DefaultFontName);
			SetSpecialNode(ldmlNode, "defaultFontSize", ws.DefaultFontSize.ToString());
			SetSpecialNode(ldmlNode, "defaultKeyboard", ws.Keyboard);
			SetSpecialNode(ldmlNode, "spellCheckingId", ws.SpellCheckingId);
		}

		private void SetSubNodeWithAttribute(XmlNode parent, string field, string attributeName, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				XmlNode node = XmlHelpers.GetOrCreateElement(parent, ".", field, null, _nameSpaceManager, LdmlNodeComparer.Singleton);
				XmlHelpers.AddOrUpdateAttribute(node, attributeName, value, LdmlNodeComparer.Singleton);
			}
			else
			{
				XmlHelpers.RemoveElement(parent, field, _nameSpaceManager);
			}
		}

		private void SetSpecialNode(XmlNode parent, string field, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				XmlHelpers.GetOrCreateElement(parent, ".", "special", null, _nameSpaceManager, LdmlNodeComparer.Singleton);
				XmlNode node = XmlHelpers.GetOrCreateElement(parent, "special", field, "palaso", _nameSpaceManager,
					LdmlNodeComparer.Singleton);
				XmlHelpers.AddOrUpdateAttribute(node, "value", value, LdmlNodeComparer.Singleton);
			}
			else
			{
				XmlHelpers.RemoveElement(parent, "special/palaso:" + field, _nameSpaceManager);
			}
		}

		private void UpdateIdentityElement(XmlNode ldmlNode, WritingSystemDefinition ws)
		{
			XmlNode identityNode = XmlHelpers.GetOrCreateElement(ldmlNode, ".", "identity", null,
				_nameSpaceManager, LdmlNodeComparer.Singleton);
			SetSubNodeWithAttribute(identityNode, "language", "type", ws.ISO);
			SetSubNodeWithAttribute(identityNode, "script", "type", ws.Script);
			SetSubNodeWithAttribute(identityNode, "territory", "type", ws.Region);
			SetSubNodeWithAttribute(identityNode, "variant", "type", ws.Variant);
			SetSubNodeWithAttribute(identityNode, "generation", "date", String.Format("{0:s}", ws.DateModified));
			XmlNode node = XmlHelpers.GetOrCreateElement(identityNode, ".", "version", null, _nameSpaceManager,
				LdmlNodeComparer.Singleton);
			XmlHelpers.AddOrUpdateAttribute(node, "number", ws.VersionNumber, LdmlNodeComparer.Singleton);
			node.InnerText = ws.VersionDescription;
		}

		private void UpdateLayoutElement(XmlNode ldmlNode, WritingSystemDefinition ws)
		{
			XmlNode layoutNode = XmlHelpers.GetOrCreateElement(ldmlNode, ".", "layout", null, _nameSpaceManager,
				LdmlNodeComparer.Singleton);
			XmlNode orientationNode = XmlHelpers.GetOrCreateElement(layoutNode, ".", "orientation", null,
				_nameSpaceManager, LdmlNodeComparer.Singleton);
			// Currently we don't support line orientations other than top-to-bottom.
			// We also don't support vertical character orientations, although both of these are allowed in LDML
			XmlHelpers.AddOrUpdateAttribute(orientationNode, "lines", "top-to-bottom", LdmlNodeComparer.Singleton);
			XmlHelpers.AddOrUpdateAttribute(orientationNode, "characters", ws.RightToLeftScript ? "right-to-left" : "left-to-right",
				LdmlNodeComparer.Singleton);
		}

		private void UpdateCollationElement(XmlNode ldmlNode, WritingSystemDefinition ws)
		{
			Debug.Assert(ldmlNode != null);
			Debug.Assert(ws != null);
			if (string.IsNullOrEmpty(ws.SortUsing) || !Enum.IsDefined(typeof (WritingSystemDefinition.SortRulesType), ws.SortUsing))
			{
				return;
			}
			XmlNode parentNode = XmlHelpers.GetOrCreateElement(ldmlNode, ".", "collations", null,
				_nameSpaceManager, LdmlNodeComparer.Singleton);
			// Because we have to check the type attribute, we can't just use the XmlHelpers.GetOrCreateElement
			XmlNode node = parentNode.SelectSingleNode("collation[@type='']", _nameSpaceManager) ??
				parentNode.SelectSingleNode("collation[@type='standard']", _nameSpaceManager);
			if (node == null)
			{
				node = parentNode.OwnerDocument.CreateElement("collation");
				XmlHelpers.InsertNodeUsingDefinedOrder(parentNode, node, LdmlNodeComparer.Singleton);
			}
			switch ((WritingSystemDefinition.SortRulesType)Enum.Parse(typeof(WritingSystemDefinition.SortRulesType), ws.SortUsing))
			{
				case WritingSystemDefinition.SortRulesType.OtherLanguage:
					UpdateCollationRulesFromOtherLanguage(node, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomSimple:
					UpdateCollationRulesFromCustomSimple(node, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomICU:
					UpdateCollationRulesFromCustomICU(node, ws);
					break;
				default:
					string message = string.Format("Unhandled SortRulesType '{0}' while writing LDML definition file.", ws.SortUsing);
					throw new ApplicationException(message);
			}
			SetSpecialNode(node, "sortRulesType", ws.SortUsing);
		}

		private void UpdateCollationRulesFromOtherLanguage(XmlNode parentNode, WritingSystemDefinition ws)
		{
			Debug.Assert(parentNode != null);
			Debug.Assert(ws.SortUsing == "OtherLanguage");
			// Since the alias element gets all information from another source,
			// we should remove all other elements in this collation element.  We
			// leave "special" elements as they are custom data from some other app.
			List<XmlNode> nodesToRemove = new List<XmlNode>();
			foreach (XmlNode child in parentNode.ChildNodes)
			{
				if (child.NodeType != XmlNodeType.Element || child.Name == "special")
				{
					continue;
				}
				nodesToRemove.Add(child);
			}
			foreach (XmlNode node in nodesToRemove)
			{
				parentNode.RemoveChild(node);
			}

			XmlNode alias = XmlHelpers.GetOrCreateElement(parentNode, ".", "alias", string.Empty,
				_nameSpaceManager, LdmlNodeComparer.Singleton);
			XmlHelpers.AddOrUpdateAttribute(alias, "source", ws.SortRules, LdmlNodeComparer.Singleton);
		}

		private void UpdateCollationRulesFromCustomSimple(XmlNode parentNode, WritingSystemDefinition ws)
		{
			Debug.Assert(parentNode != null);
			Debug.Assert(ws.SortUsing == "CustomSimple");
			string icu = SimpleRulesCollator.ConvertToIcuRules(ws.SortRules ?? string.Empty);
			UpdateCollationRulesFromICUString(parentNode, icu);
		}

		private void UpdateCollationRulesFromCustomICU(XmlNode parentNode, WritingSystemDefinition ws)
		{
			Debug.Assert(parentNode != null);
			Debug.Assert(ws.SortUsing == "CustomICU");
			UpdateCollationRulesFromICUString(parentNode, ws.SortRules);
		}

		private void UpdateCollationRulesFromICUString(XmlNode parentNode, string icu)
		{
			Debug.Assert(parentNode != null);
			icu = icu ?? string.Empty;
			// remove any alias that would override our rules
			XmlHelpers.RemoveElement(parentNode, "alias", _nameSpaceManager);
			IcuRulesParser parser = new IcuRulesParser(true);
			parser.AddIcuRulesToNode(parentNode, icu, _nameSpaceManager);
		}
	}

	/// <summary>
	/// Class for comparison of order of nodes in an LDML document.
	/// Based on http://www.unicode.org/cldr/data/docs/design/ldml_canonical_form.html
	/// </summary>
	class LdmlNodeComparer: IComparer<XmlNode>
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
		private LdmlNodeComparer() {}

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

		static readonly string[] _valueOrderedList = new string[] {
			"sun", "mon", "tue", "wed", "thu", "fri", "sat", "full", "long", "medium", "short", "wide", "abbreviated",
			"narrow", "era", "year", "month", "week", "day", "weekday", "dayperiod", "hour", "minute", "second", "zone"};

		private static Dictionary<string, int> _elementNameValues;
		private static Dictionary<string, int> _attributeNameValues;
		private static Dictionary<string, int> _valueValues;

		private static int CompareElementNames(XmlNode x, XmlNode y)
		{
			lock (_key)
			{
				if (_elementNameValues == null)
				{
					_elementNameValues = new Dictionary<string, int>(_elementOrderedNameList.Length);
					for (int i = 0; i < _elementOrderedNameList.Length; i++)
					{
						_elementNameValues.Add(_elementOrderedNameList[i], i);
					}
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
					_attributeNameValues = new Dictionary<string, int>(_attributeOrderedNameList.Length);
					for (int i = 0; i < _attributeOrderedNameList.Length; i++)
					{
						_attributeNameValues.Add(_attributeOrderedNameList[i], i);
					}
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
					_valueValues = new Dictionary<string, int>(_valueOrderedList.Length);
					for (int i = 0; i < _valueOrderedList.Length; i++)
					{
						_valueValues.Add(_valueOrderedList[i], i);
					}
				}
			}
			int result = 0;
			if (x.Name == "type" || x.Name == "day")
			{
				result = GetItemStrength(x.Value, _valueValues).CompareTo(GetItemStrength(y.Value, _valueValues));
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
	}
}