using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Collation;

namespace Palaso.WritingSystems
{
	public class LdmlAdaptor
	{
		private XmlNamespaceManager _nameSpaceManager;
		private string _abbreviationKey;
		private string _languageNameKey;
		private string _defaultFontFamilyKey;
		private string _defaultFontSizeKey;
		private string _spellCheckingIdKey;
		private string _identityKey;
		private string _languageKey;
		private string _variantKey;
		private string _territoryKey;
		private string _scriptKey;
		private string _generationKey;
		private string _versionKey;
		private string _numberKey;
		private string _layoutKey;
		private string _charactersKey;
		private string _rightToLeftKey;
		private string _collationsKey;
		private string _collationKey;
		private string _typeKey;
		private string _sortRulesTypeKey;

		public LdmlAdaptor()
		{
			_nameSpaceManager = MakeNameSpaceManager();
		}

		public void Read(string filePath, WritingSystemDefinition ws)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.NameTable = _nameSpaceManager.NameTable;
			settings.ValidationType = ValidationType.None;
			settings.XmlResolver = null;
			settings.ProhibitDtd = false;
			using (XmlReader reader = XmlReader.Create(filePath, settings))
			{
				ReadLdml(reader, ws);
			}
		}

		public void Read(XmlReader xmlReader, WritingSystemDefinition ws)
		{
			if (xmlReader == null)
			{
				throw new ArgumentNullException("xmlReader");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.NameTable = _nameSpaceManager.NameTable;
			settings.ConformanceLevel = ConformanceLevel.Auto;
			settings.ValidationType = ValidationType.None;
			settings.XmlResolver = null;
			settings.ProhibitDtd = false;
			using (XmlReader reader = XmlReader.Create(xmlReader, settings))
			{
				ReadLdml(reader, ws);
			}
		}

		private static bool FindElement(XmlReader reader, string name)
		{
			return XmlHelpers.FindElement(reader, name, LdmlNodeComparer.CompareElementNames);
		}

		private static bool FindElement(XmlReader reader, string name, string nameSpace)
		{
			return XmlHelpers.FindElement(reader, name, nameSpace, LdmlNodeComparer.CompareElementNames);
		}

		private void ReadLdml(XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(reader != null);
			Debug.Assert(ws != null);
			if (reader.MoveToContent() != XmlNodeType.Element || reader.Name != "ldml")
			{
				throw new ApplicationException("Unable to load writing system definition: Missing <ldml> tag.");
			}
			reader.Read();
			if (FindElement(reader, "identity"))
			{
				ReadIdentityElement(reader, ws);
			}
			if (FindElement(reader, "layout"))
			{
				ReadLayoutElement(reader, ws);
			}
			if (FindElement(reader, "collations"))
			{
				ReadCollationsElement(reader, ws);
			}
			if (FindElement(reader, "special"))
			{
				reader.ReadStartElement("special");
				ws.Abbreviation = GetSpecialValue(reader, "abbreviation");
				ws.DefaultFontName = GetSpecialValue(reader, "defaultFontFamily");
				float fontSize;
				if (float.TryParse(GetSpecialValue(reader, "defaultFontSize"), out fontSize))
				{
					ws.DefaultFontSize = fontSize;
				}
				ws.Keyboard = GetSpecialValue(reader, "defaultKeyboard");
				ws.LanguageName = GetSpecialValue(reader, "languageName");
				ws.SpellCheckingId = GetSpecialValue(reader, "spellCheckingId");
			}
			ws.StoreID = "";
			ws.Modified = false;
		}

		private void ReadIdentityElement(XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == "identity");
			using (XmlReader identityReader = reader.ReadSubtree())
			{
				identityReader.MoveToContent();
				identityReader.ReadStartElement("identity");
				if (FindElement(identityReader, "version"))
				{
					ws.VersionNumber = identityReader.GetAttribute("number") ?? string.Empty;
					if (!identityReader.IsEmptyElement)
					{
						ws.VersionDescription = identityReader.ReadString();
						identityReader.ReadEndElement();
					}
				}
				string dateTime = GetSubNodeAttributeValue(identityReader, "generation", "date");
				DateTime modified = DateTime.UtcNow;
				if (!string.IsNullOrEmpty(dateTime.Trim()) && !DateTime.TryParse(dateTime, out modified))
				{
					//CVS format:    "$Date: 2008/06/18 22:52:35 $"
					modified = DateTime.ParseExact(dateTime, "'$Date: 'yyyy/MM/dd HH:mm:ss $", null,
												   DateTimeStyles.AssumeUniversal);
				}
				ws.DateModified = modified;
				ws.ISO = GetSubNodeAttributeValue(identityReader, "language", "type");
				ws.Script = GetSubNodeAttributeValue(identityReader, "script", "type");
				ws.Region = GetSubNodeAttributeValue(identityReader, "territory", "type");
				ws.Variant = GetSubNodeAttributeValue(identityReader, "variant", "type");
				// move to end of identity node
				while (identityReader.Read()) ;
			}
			if (!reader.IsEmptyElement)
			{
				reader.ReadEndElement();
			}
		}

		private void ReadLayoutElement(XmlReader reader, WritingSystemDefinition ws)
		{
			// The orientation node has two attributes, "lines" and "characters" which define direction of writing.
			// The valid values are: "top-to-bottom", "bottom-to-top", "left-to-right", and "right-to-left"
			// Currently we only handle horizontal character orders with top-to-bottom line order, so
			// any value other than characters right-to-left, we treat as our default left-to-right order.
			// This probably works for many scripts such as various East Asian scripts which traditionally
			// are top-to-bottom characters and right-to-left lines, but can also be written with
			// left-to-right characters and top-to-bottom lines.
			Debug.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == "layout");
			using (XmlReader layoutReader = reader.ReadSubtree())
			{
				layoutReader.MoveToContent();
				layoutReader.ReadStartElement("layout");
				ws.RightToLeftScript = GetSubNodeAttributeValue(layoutReader, "orientation", "characters") ==
									   "right-to-left";
				while (layoutReader.Read());
			}
			if (!reader.IsEmptyElement)
			{
				reader.ReadEndElement();
			}
		}

		private void ReadCollationsElement(XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == "collations");
			using (XmlReader collationsReader = reader.ReadSubtree())
			{
				collationsReader.MoveToContent();
				collationsReader.ReadStartElement("collations");
				bool found = false;
				while (FindElement(collationsReader, "collation"))
				{
					// having no type is the same as type=standard, and is the only one we're interested in
					string typeValue = collationsReader.GetAttribute("type");
					if (string.IsNullOrEmpty(typeValue) || typeValue == "standard")
					{
						found = true;
						break;
					}
					reader.Skip();
				}
				if (found)
				{
					reader.MoveToElement();
					string collationXml = reader.ReadInnerXml();
					ReadCollationElement(collationXml, ws);
				}
				while (collationsReader.Read());
			}
			if (!reader.IsEmptyElement)
			{
				reader.ReadEndElement();
			}
		}

		private void ReadCollationElement(string collationXml, WritingSystemDefinition ws)
		{
			Debug.Assert(collationXml != null);
			Debug.Assert(ws != null);

			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.CloseInput = true;
			readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			string rulesType = string.Empty;
			using (XmlReader collationReader = XmlReader.Create(new StringReader(collationXml), readerSettings))
			{
				if (FindElement(collationReader, "special"))
				{
					collationReader.Read();
					rulesType = GetSpecialValue(collationReader, "sortRulesType");
				}
			}
			if (!Enum.IsDefined(typeof (WritingSystemDefinition.SortRulesType), rulesType))
			{
				rulesType = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			}
			ws.SortUsing = rulesType;
			switch ((WritingSystemDefinition.SortRulesType)Enum.Parse(typeof(WritingSystemDefinition.SortRulesType), rulesType))
			{
				case WritingSystemDefinition.SortRulesType.OtherLanguage:
					ReadCollationRulesForOtherLanguage(collationXml, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomSimple:
					ReadCollationRulesForCustomSimple(collationXml, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomICU:
					ReadCollationRulesForCustomICU(collationXml, ws);
					break;
				default:
					string message = string.Format("Unhandled SortRulesType '{0}' while writing LDML definition file.", ws.SortUsing);
					throw new ApplicationException(message);
			}
		}

		private void ReadCollationRulesForOtherLanguage(string collationXml, WritingSystemDefinition ws)
		{
			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.CloseInput = true;
			readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			using (XmlReader collationReader = XmlReader.Create(new StringReader(collationXml), readerSettings))
			{
				if (FindElement(collationReader, "alias"))
				{
					ws.SortRules = collationReader.GetAttribute("source") ?? string.Empty;
				}
				else
				{
					// missing alias element, fall back to ICU rules
					ws.SortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
					ReadCollationRulesForCustomICU(collationXml, ws);
				}
			}
		}

		private void ReadCollationRulesForCustomICU(string collationXml, WritingSystemDefinition ws)
		{
			ws.SortRules = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
		}

		private void ReadCollationRulesForCustomSimple(string collationXml, WritingSystemDefinition ws)
		{
			string rules;
			if (LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out rules))
			{
				ws.SortRules = rules;
				return;
			}
			// fall back to ICU rules if Simple rules don't work
			ws.SortUsing = WritingSystemDefinition.SortRulesType.CustomICU.ToString();
			ReadCollationRulesForCustomICU(collationXml, ws);
		}

		public void Write(string filePath, WritingSystemDefinition ws)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.CreateXmlDeclaration("1.0", "", "no");
			Write(doc, ws);
			doc.Save(filePath);
		}

		public void Write(XmlWriter xmlWriter, WritingSystemDefinition ws)
		{
			if (xmlWriter == null)
			{
				throw new ArgumentNullException("xmlWriter");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			Write(doc, ws);
			doc.Save(xmlWriter); //??? Not sure about this, does this need to be Write(To) or similar?
		}

		public void Write(XmlNode parentOfLdmlNode, WritingSystemDefinition ws)
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

			SetSpecialNode(ldmlNode, "abbreviation", ws.Abbreviation);
			SetSpecialNode(ldmlNode, "defaultFontFamily", ws.DefaultFontName);
			SetSpecialNode(ldmlNode, "defaultFontSize", ws.DefaultFontSize.ToString());
			SetSpecialNode(ldmlNode, "defaultKeyboard", ws.Keyboard);
			SetSpecialNode(ldmlNode, "languageName", ws.LanguageName);
			SetSpecialNode(ldmlNode, "spellCheckingId", ws.SpellCheckingId);
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

		private string GetSpecialValue(XmlReader reader, string field)
		{
			if (!XmlHelpers.FindElement(reader, "palaso:" + field, _nameSpaceManager.LookupNamespace("palaso"), string.Compare))
			{
				return string.Empty;
			}
			return reader.GetAttribute("value") ?? string.Empty;
		}

		private string GetSubNodeAttributeValue(XmlReader reader, string elementName, string attributeName)
		{
			return FindElement(reader, elementName) ? (reader.GetAttribute(attributeName) ?? string.Empty) : string.Empty;
		}

		private XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNameTable nameTable = new NameTable();
			_abbreviationKey = nameTable.Add("abbreviation");
			_languageNameKey = nameTable.Add("languageName");
			_defaultFontFamilyKey = nameTable.Add("defaultFontFamily");
			_defaultFontSizeKey = nameTable.Add("defaultFontSize");
			_spellCheckingIdKey = nameTable.Add("spellCheckingId");
			_identityKey = nameTable.Add("identity");
			_languageKey = nameTable.Add("language");
			_variantKey = nameTable.Add("variant");
			_territoryKey = nameTable.Add("territory");
			_scriptKey = nameTable.Add("script");
			_generationKey = nameTable.Add("generation");
			_versionKey = nameTable.Add("version");
			_numberKey = nameTable.Add("number");
			_layoutKey = nameTable.Add("layout");
			_charactersKey = nameTable.Add("characters");
			_rightToLeftKey = nameTable.Add("right-to-left");
			_collationsKey = nameTable.Add("collations");
			_collationKey = nameTable.Add("collation");
			_typeKey = nameTable.Add("type");
			_sortRulesTypeKey = nameTable.Add("sortRulesType");
			XmlNamespaceManager m = new XmlNamespaceManager(nameTable);
			m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			return m;
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
				XmlHelpers.GetOrCreateElement(parent, ".", "special", string.Empty, _nameSpaceManager, LdmlNodeComparer.Singleton);
				XmlNode node = XmlHelpers.GetOrCreateElement(parent, "special", field, "palaso", _nameSpaceManager);
				XmlHelpers.AddOrUpdateAttribute(node, "value", value);
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
}