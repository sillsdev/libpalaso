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

		public void Read(XmlDocument doc, WritingSystemDefinition ws)
		{
			ws.ISO = GetIdentityValue(doc, "language", "type");
			ws.Variant = GetIdentityValue(doc, "variant", "type");
			ws.Region = GetIdentityValue(doc, "territory", "type");
			ws.Script = GetIdentityValue(doc, "script", "type");
			string dateTime = GetIdentityValue(doc, "generation", "date");
			ws.DateModified = DateTime.Parse(dateTime);
			XmlNode node = doc.SelectSingleNode("ldml/identity/version");
			ws.VersionNumber = XmlHelpers.GetOptionalAttributeValue(node, "number");
			ws.VersionDescription = node.InnerText;

			ws.Abbreviation = GetSpecialValue(doc, "abbreviation");
			ws.LanguageName = GetSpecialValue(doc, "languageName");
			ws.DefaultFontName = GetSpecialValue(doc, "defaultFontFamily");
			ws.Keyboard = GetSpecialValue(doc, "keyboard");
			string rtl = GetSpecialValue(doc, "rightToLeft");
			ws.RightToLeftScript = rtl == "true";
			ws.StoreID = "";
			ws.Modified = false;
		}

		public void Write(string filePath, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.CreateXmlDeclaration("1.0", "", "no");
			XmlHelpers.GetOrCreateElement(doc, ".", "ldml", null, _nameSpaceManager);
			XmlHelpers.GetOrCreateElement(doc, "ldml", "identity", null, _nameSpaceManager);
			UpdateDOM(doc, ws);
			doc.Save(filePath);
		}

		public void Write(XmlWriter xmlWriter, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			XmlHelpers.GetOrCreateElement(doc, ".", "ldml", null, _nameSpaceManager);
			XmlHelpers.GetOrCreateElement(doc, "ldml", "identity", null, _nameSpaceManager);
			UpdateDOM(doc, ws);
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

		private string GetSpecialValue(XmlDocument doc, string field)
		{
			XmlNode node = doc.SelectSingleNode("ldml/special/palaso:"+field, _nameSpaceManager);
			return XmlHelpers.GetOptionalAttributeValue(node, "value", string.Empty);
		}

		private string GetIdentityValue(XmlDocument doc, string field, string attributeName)
		{
			XmlNode node = doc.SelectSingleNode("ldml/identity/" + field);
			return XmlHelpers.GetOptionalAttributeValue(node, attributeName, string.Empty);
		}

		public static XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNamespaceManager m = new XmlNamespaceManager(new NameTable());
			m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			return m;
		}

		private void UpdateDOM(XmlDocument dom, WritingSystemDefinition ws)
		{
			SetSubIdentityNode(dom, "language", "type", ws.ISO);
			SetSubIdentityNode(dom, "script", "type", ws.Script);
			SetSubIdentityNode(dom, "territory", "type", ws.Region);
			SetSubIdentityNode(dom, "variant", "type", ws.Variant);
			SetSubIdentityNode(dom, "generation", "date", String.Format("{0:s}", ws.DateModified));
			XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml/identity", "version", null, _nameSpaceManager);
			XmlHelpers.AddOrUpdateAttribute(node, "number", ws.VersionNumber);
			node.InnerText = ws.VersionDescription;
			UpdateCollationElement(dom, ws);

			SetTopLevelSpecialNode(dom, "languageName", ws.LanguageName);
			SetTopLevelSpecialNode(dom, "abbreviation", ws.Abbreviation);
			SetTopLevelSpecialNode(dom, "defaultFontFamily", ws.DefaultFontName);
			SetTopLevelSpecialNode(dom, "keyboard", ws.Keyboard);
			SetTopLevelSpecialNode(dom, "rightToLeft", ws.RightToLeftScript ? "true": "false");
		}

		private void SetSubIdentityNode(XmlDocument dom, string field, string attributeName, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml/identity", field, null, _nameSpaceManager);
				XmlHelpers.AddOrUpdateAttribute(node, attributeName, value);
			}
			else
			{
				XmlHelpers.RemoveElement(dom, "ldml/identity/" + field, _nameSpaceManager);
			}
		}

		private void SetTopLevelSpecialNode(XmlDocument dom, string field, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				XmlHelpers.GetOrCreateElement(dom, "ldml", "special", null, _nameSpaceManager);
				XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml/special", field, "palaso", _nameSpaceManager);
				Palaso.XmlHelpers.AddOrUpdateAttribute(node, "value", value);
			}
			else
			{
				XmlHelpers.RemoveElement(dom, "ldml/special/palaso:" + field, _nameSpaceManager);
			}
		}

		private void UpdateCollationElement(XmlDocument dom, WritingSystemDefinition ws)
		{
			Debug.Assert(dom != null);
			Debug.Assert(ws != null);
			if (string.IsNullOrEmpty(ws.SortUsing) || !Enum.IsDefined(typeof (WritingSystemDefinition.SortRulesType), ws.SortUsing))
			{
				return;
			}
			XmlNode parentNode = XmlHelpers.GetOrCreateElement(dom, "ldml", "collations", null, _nameSpaceManager);
			XmlNode node = parentNode.SelectSingleNode("collation[@type='']", _nameSpaceManager);
			if (node == null)
			{
				node = parentNode.OwnerDocument.CreateElement("collation");
				parentNode.AppendChild(node);
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
					break;
			}
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

			XmlNode alias = XmlHelpers.GetOrCreateElement(parentNode, ".", "alias", string.Empty, _nameSpaceManager);
			XmlHelpers.AddOrUpdateAttribute(alias, "source", ws.SortRules);
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