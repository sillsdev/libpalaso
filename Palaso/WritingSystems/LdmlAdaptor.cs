using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.Load(filePath);
			Read(doc, ws);
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
			DateTime modified;
			if (!DateTime.TryParse(dateTime, out modified))
			{
				//CVS format:    "$Date: 2008/06/18 22:52:35 $"
				modified = DateTime.ParseExact(dateTime, "'$Date: 'yyyy/MM/dd HH:mm:ss $", null, DateTimeStyles.AssumeUniversal);
			}
			ws.DateModified = modified;
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
			XmlNode node = ldmlNode.SelectSingleNode("collations/collation[not(@type) or @type='standard']");
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

			SetSpecialNode(ldmlNode, "languageName", ws.LanguageName);
			SetSpecialNode(ldmlNode, "abbreviation", ws.Abbreviation);
			SetSpecialNode(ldmlNode, "defaultFontFamily", ws.DefaultFontName);
			SetSpecialNode(ldmlNode, "defaultFontSize", ws.DefaultFontSize.ToString());
			SetSpecialNode(ldmlNode, "defaultKeyboard", ws.Keyboard);
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

		private string GetSpecialValue(XmlNode parent, string field)
		{
			XmlNode node = parent.SelectSingleNode("palaso:special/palaso:"+field, _nameSpaceManager);
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
				XmlHelpers.GetOrCreateElement(parent, ".", "special", "palaso", _nameSpaceManager, LdmlNodeComparer.Singleton);
				XmlNode node = XmlHelpers.GetOrCreateElement(parent, "palaso:special", field, "palaso", _nameSpaceManager,
					LdmlNodeComparer.Singleton);
				XmlHelpers.AddOrUpdateAttribute(node, "value", value, LdmlNodeComparer.Singleton);
			}
			else
			{
				XmlHelpers.RemoveElement(parent, "palaso:special/palaso:" + field, _nameSpaceManager);
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