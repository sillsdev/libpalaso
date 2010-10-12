using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Palaso.Keyboarding;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Collation;
using Palaso.Xml;

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

		public static void WriteLdmlText(XmlWriter writer, string text)
		{
			// Not all Unicode characters are valid in an XML document, so we need to create
			// the <cp hex="X"> elements to replace the invalid characters.
			// Note: While 0xD (carriage return) is a valid XML character, it is automatically
			// either dropped or coverted to 0xA by any conforming XML parser, so we also make a <cp>
			// element for that one.
			StringBuilder sb = new StringBuilder(text.Length);
			for (int i=0; i < text.Length; i++)
			{
				int code = Char.ConvertToUtf32(text, i);
				if ((code == 0x9) ||
					(code == 0xA) ||
					(code >= 0x20 && code <= 0xD7FF) ||
					(code >= 0xE000 && code <= 0xFFFD) ||
					(code >= 0x10000 && code <= 0x10FFFF))
				{
					sb.Append(Char.ConvertFromUtf32(code));
				}
				else
				{
					writer.WriteString(sb.ToString());
					writer.WriteStartElement("cp");
					writer.WriteAttributeString("hex", String.Format("{0:X}", code));
					writer.WriteEndElement();
					sb = new StringBuilder(text.Length - i);
				}

				if (Char.IsSurrogatePair(text, i))
				{
					i++;
				}
			}
			writer.WriteString(sb.ToString());
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
				ws.Keyboard = GetKeyboardDescriptor(reader);
				string isLegacyEncoded = GetSpecialValue(reader, "isLegacyEncoded");
				if (!String.IsNullOrEmpty(isLegacyEncoded))
				{
					ws.IsLegacyEncoded = Convert.ToBoolean(isLegacyEncoded);
				}
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
			string rulesTypeAsString = string.Empty;
			WritingSystemDefinition.SortRulesType rulesType = WritingSystemDefinition.SortRulesType.OtherLanguage;
			using (XmlReader collationReader = XmlReader.Create(new StringReader(collationXml), readerSettings))
			{
				if (FindElement(collationReader, "special"))
				{
					collationReader.Read();
					rulesTypeAsString = GetSpecialValue(collationReader, "sortRulesType");
				}
				ws.SortUsing = (WritingSystemDefinition.SortRulesType)Enum.Parse(typeof(WritingSystemDefinition.SortRulesType), rulesTypeAsString);
			}
			switch (ws.SortUsing)
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
				bool foundValue = false;
				if (FindElement(collationReader, "base"))
				{
					if (!collationReader.IsEmptyElement && collationReader.ReadToDescendant("alias"))
					{
						string sortRules = collationReader.GetAttribute("source");
						if (sortRules != null)
						{
							ws.SortRules = sortRules;
							foundValue = true;
						}
					}
				}
				if (!foundValue)
				{
					// missing base alias element, fall back to ICU rules
					ws.SortUsing = WritingSystemDefinition.SortRulesType.CustomICU;
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
			ws.SortUsing = WritingSystemDefinition.SortRulesType.CustomICU;
			ReadCollationRulesForCustomICU(collationXml, ws);
		}

		public void Write(string filePath, WritingSystemDefinition ws, Stream oldFile)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Indent = true;
			writerSettings.IndentChars = "\t";
			writerSettings.NewLineHandling = NewLineHandling.None;
			XmlReader reader = null;
			try
			{
				if (oldFile != null)
				{
					XmlReaderSettings readerSettings = new XmlReaderSettings();
					readerSettings.NameTable = _nameSpaceManager.NameTable;
					readerSettings.ConformanceLevel = ConformanceLevel.Auto;
					readerSettings.ValidationType = ValidationType.None;
					readerSettings.XmlResolver = null;
					readerSettings.ProhibitDtd = false;
					reader = XmlReader.Create(oldFile, readerSettings);
				}
				using (XmlWriter writer = XmlWriter.Create(filePath, writerSettings))
				{
					writer.WriteStartDocument();
					WriteLdml(writer, reader, ws);
					writer.Close();
				}
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}
		}

		public void Write(XmlWriter xmlWriter, WritingSystemDefinition ws, XmlReader xmlReader)
		{
			if (xmlWriter == null)
			{
				throw new ArgumentNullException("xmlWriter");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlReader reader = null;
			try
			{
				if (xmlReader != null)
				{
					XmlReaderSettings settings = new XmlReaderSettings();
					settings.NameTable = _nameSpaceManager.NameTable;
					settings.ConformanceLevel = ConformanceLevel.Auto;
					settings.ValidationType = ValidationType.None;
					settings.XmlResolver = null;
					settings.ProhibitDtd = false;
					reader = XmlReader.Create(xmlReader, settings);
				}
				WriteLdml(xmlWriter, reader, ws);
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}
		}

		private void WriteLdml(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			writer.WriteStartElement("ldml");
			if (reader != null)
			{
				reader.MoveToContent();
				reader.ReadStartElement("ldml");
				CopyUntilElement(writer, reader, "identity");
			}
			WriteIdentityElement(writer, reader, ws);
			if (reader != null)
			{
				CopyUntilElement(writer, reader, "layout");
			}
			WriteLayoutElement(writer, reader, ws);
			if (reader != null)
			{
				CopyUntilElement(writer, reader, "collations");
			}
			WriteCollationsElement(writer, reader, ws);
			if (reader != null)
			{
				CopyUntilElement(writer, reader, "special");
			}
			WriteTopLevelSpecialElement(writer, ws);
			if (reader != null)
			{
				CopyOtherSpecialElements(writer, reader);
				CopyToEndElement(writer, reader);
			}
			writer.WriteEndElement();
		}

		private void CopyUntilElement(XmlWriter writer, XmlReader reader, string elementName)
		{
			Debug.Assert(writer != null);
			Debug.Assert(reader != null);
			Debug.Assert(!string.IsNullOrEmpty(elementName));
			if (reader.NodeType == XmlNodeType.None)
			{
				reader.Read();
			}
			while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement
				&& (reader.NodeType != XmlNodeType.Element || LdmlNodeComparer.CompareElementNames(reader.Name, elementName) < 0))
			{
				// XmlWriter.WriteNode doesn't do anything if the node type is Attribute
				if (reader.NodeType == XmlNodeType.Attribute)
				{
					writer.WriteAttributes(reader, false);
				}
				else
				{
					writer.WriteNode(reader, false);
				}
			}
		}

		private void CopyToEndElement(XmlWriter writer, XmlReader reader)
		{
			Debug.Assert(writer != null);
			Debug.Assert(reader != null);
			while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
			{
				// XmlWriter.WriteNode doesn't do anything if the node type is Attribute
				if (reader.NodeType == XmlNodeType.Attribute)
				{
					writer.WriteAttributes(reader, false);
				}
				else
				{
					writer.WriteNode(reader, false);
				}
			}
			// either read the end element or no-op if EOF
			reader.Read();
		}

		private void CopyOtherSpecialElements(XmlWriter writer, XmlReader reader)
		{
			Debug.Assert(writer != null);
			Debug.Assert(reader != null);
			while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement
				&& (reader.NodeType != XmlNodeType.Element || reader.Name == "special"))
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					bool palasoNS = false;
					while (reader.MoveToNextAttribute())
					{
						if (reader.Name.StartsWith("xmlns:") && reader.Value == _nameSpaceManager.LookupNamespace("palaso"))
						{
							palasoNS = true;
							break;
						}
					}
					reader.MoveToElement();
					if (palasoNS)
					{
						reader.Skip();
						continue;
					}
				}
				writer.WriteNode(reader, false);
			}
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

		private KeyboardDescriptor GetKeyboardDescriptor(XmlReader reader)
		{
			KeyboardDescriptor keyboard;
			if (!XmlHelpers.FindElement(reader, "palaso:defaultKeyboard", _nameSpaceManager.LookupNamespace("palaso"), string.Compare))
			{
				return KeyboardDescriptor.DefaultKeyboard;
			}
			string keyboardName = reader.GetAttribute("name") ?? string.Empty;
			string keyboardingEngineAsString = reader.GetAttribute("provider") ?? string.Empty;
			Engines keyboardingEngine = (Engines) Enum.Parse(typeof (Engines), keyboardingEngineAsString);
			string id = reader.GetAttribute("id") ?? string.Empty;
			if(String.IsNullOrEmpty(keyboardName))
			{
				keyboard = KeyboardDescriptor.DefaultKeyboard;
			}
			else
			{
				keyboard = new KeyboardDescriptor(keyboardName, keyboardingEngine, id);
			}
			return keyboard;
		}

		private string GetSubNodeAttributeValue(XmlReader reader, string elementName, string attributeName)
		{
			return FindElement(reader, elementName) ? (reader.GetAttribute(attributeName) ?? string.Empty) : string.Empty;
		}

		private XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNamespaceManager m = new XmlNamespaceManager(new NameTable());
			m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			return m;
		}

		private void WriteElementWithAttribute(XmlWriter writer, string elementName, string attributeName, string value)
		{
			writer.WriteStartElement(elementName);
			writer.WriteAttributeString(attributeName, value);
			writer.WriteEndElement();
		}

		private void WriteSpecialValue(XmlWriter writer, string field, string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return;
			}
			writer.WriteStartElement(field, _nameSpaceManager.LookupNamespace("palaso"));
			writer.WriteAttributeString("value", value);
			writer.WriteEndElement();
		}

		private void WriteKeyboard(XmlWriter writer, KeyboardDescriptor keyboard)
		{
			if (keyboard == KeyboardDescriptor.DefaultKeyboard)
			{
				return;
			}
			writer.WriteStartElement("defaultKeyboard", _nameSpaceManager.LookupNamespace("palaso"));
			writer.WriteAttributeString("name", keyboard.KeyboardName);
			writer.WriteAttributeString("provider", keyboard.KeyboardingEngine.ToString());
			writer.WriteAttributeString("id", keyboard.Id);
			writer.WriteEndElement();
		}

		private void WriteIdentityElement(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			bool needToCopy = reader != null && reader.NodeType == XmlNodeType.Element && reader.Name == "identity";

			writer.WriteStartElement("identity");
			writer.WriteStartElement("version");
			writer.WriteAttributeString("number", ws.VersionNumber);
			writer.WriteString(ws.VersionDescription);
			writer.WriteEndElement();
			WriteElementWithAttribute(writer, "generation", "date", String.Format("{0:s}", ws.DateModified));
			WriteElementWithAttribute(writer, "language", "type", ws.ISO);
			if (!String.IsNullOrEmpty(ws.Script))
			{
				WriteElementWithAttribute(writer, "script", "type", ws.Script);
			}
			if (!String.IsNullOrEmpty(ws.Region))
			{
				WriteElementWithAttribute(writer, "territory", "type", ws.Region);
			}
			if (!String.IsNullOrEmpty(ws.Variant))
			{
				WriteElementWithAttribute(writer, "variant", "type", ws.Variant);
			}
			if (needToCopy)
			{
				if (reader.IsEmptyElement)
				{
					reader.Skip();
				}
				else
				{
					reader.Read(); // move past "identity" element}

					// <special> is the only node we possibly left out and need to copy
					FindElement(reader, "special");
					CopyToEndElement(writer, reader);
				}
			}
			writer.WriteEndElement();
		}

		private void WriteLayoutElement(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			bool needToCopy = reader != null && reader.NodeType == XmlNodeType.Element && reader.Name == "identity";
			// if we're left-to-right, we don't need to write out default values
			bool needLayoutElement = ws.RightToLeftScript;

			if (needLayoutElement)
			{
				writer.WriteStartElement("layout");
				writer.WriteStartElement("orientation");
				// omit default value for "lines" attribute
				writer.WriteAttributeString("characters", "right-to-left");
				writer.WriteEndElement();
			}
			if (needToCopy)
			{
				if (reader.IsEmptyElement)
				{
					reader.Skip();
				}
				else
				{
					reader.Read();
					// skip any existing orientation and alias element, and copy the rest
					if (FindElement(reader, "orientation"))
					{
						reader.Skip();
					}
					if (reader.NodeType != XmlNodeType.EndElement && !needLayoutElement)
					{
						needLayoutElement = true;
						writer.WriteStartElement("layout");
					}
					CopyToEndElement(writer, reader);
				}
			}
			if (needLayoutElement)
			{
				writer.WriteEndElement();
			}
		}

		private void WriteBeginSpecialElement(XmlWriter writer)
		{
			writer.WriteStartElement("special");
			writer.WriteAttributeString("xmlns", "palaso", null, _nameSpaceManager.LookupNamespace("palaso"));
		}

		private void WriteTopLevelSpecialElement(XmlWriter writer, WritingSystemDefinition ws)
		{
			WriteBeginSpecialElement(writer);
			WriteSpecialValue(writer, "abbreviation", ws.Abbreviation);
			WriteSpecialValue(writer, "defaultFontFamily", ws.DefaultFontName);
			if (ws.DefaultFontSize != 0)
			{
				WriteSpecialValue(writer, "defaultFontSize", ws.DefaultFontSize.ToString());
			}
			WriteKeyboard(writer, ws.Keyboard);
			if (ws.IsLegacyEncoded)
			{
				WriteSpecialValue(writer, "isLegacyEncoded", ws.IsLegacyEncoded.ToString());
			}
			WriteSpecialValue(writer, "languageName", ws.LanguageName);
			if (ws.SpellCheckingId != ws.ISO)
			{
				WriteSpecialValue(writer, "spellCheckingId", ws.SpellCheckingId);
			}
			writer.WriteEndElement();
		}

		private void WriteCollationsElement(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			bool needToCopy = reader != null && reader.NodeType == XmlNodeType.Element && reader.Name == "collations";

			writer.WriteStartElement("collations");
			if (needToCopy)
			{
				if (reader.IsEmptyElement)
				{
					reader.Skip();
					needToCopy = false;
				}
				else
				{
					reader.ReadStartElement("collations");
					if (FindElement(reader, "alias"))
					{
						reader.Skip();
					}
					CopyUntilElement(writer, reader, "collation");
				}
			}
			WriteCollationElement(writer, reader, ws);
			if (needToCopy)
			{
				CopyToEndElement(writer, reader);
			}
			writer.WriteEndElement();
		}

		private void WriteCollationElement(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			bool needToCopy = reader != null && reader.NodeType == XmlNodeType.Element && reader.Name == "collation";
			if (needToCopy)
			{
				string collationType = reader.GetAttribute("type");
				needToCopy = String.IsNullOrEmpty(collationType) || collationType == "standard";
			}
			if (needToCopy && reader.IsEmptyElement)
			{
				reader.Skip();
				needToCopy = false;
			}

			if (ws.SortUsing == WritingSystemDefinition.SortRulesType.DefaultOrdering)
			{
				if (needToCopy)
				{
					// copy whole existing node - invalid/undefined collation rules in our definition
					writer.WriteNode(reader, false);
				}
				return;
			}

			if (needToCopy && reader.IsEmptyElement)
			{
				reader.Skip();
				needToCopy = false;
			}
			if (!needToCopy)
			{
				// set to null if we don't need to copy to make it easier to tell in the methods we call
				reader = null;
			}
			else
			{
				reader.ReadStartElement("collation");
				while (reader.NodeType == XmlNodeType.Attribute)
				{
					reader.Read();
				}
			}

			writer.WriteStartElement("collation");
			switch (ws.SortUsing)
			{
				case WritingSystemDefinition.SortRulesType.OtherLanguage:
					WriteCollationRulesFromOtherLanguage(writer, reader, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomSimple:
					WriteCollationRulesFromCustomSimple(writer, reader, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomICU:
					WriteCollationRulesFromCustomICU(writer, reader, ws);
					break;
				default:
					string message = string.Format("Unhandled SortRulesType '{0}' while writing LDML definition file.", ws.SortUsing);
					throw new ApplicationException(message);
			}
			WriteBeginSpecialElement(writer);
			WriteSpecialValue(writer, "sortRulesType", ws.SortUsing.ToString());
			writer.WriteEndElement();
			if (needToCopy)
			{
				if (FindElement(reader, "special"))
				{
					CopyOtherSpecialElements(writer, reader);
				}
				CopyToEndElement(writer, reader);
			}
			writer.WriteEndElement();
		}

		private void WriteCollationRulesFromOtherLanguage(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			Debug.Assert(ws.SortUsing == WritingSystemDefinition.SortRulesType.OtherLanguage);

			// Since the alias element gets all information from another source,
			// we should remove all other elements in this collation element.  We
			// leave "special" elements as they are custom data from some other app.
			writer.WriteStartElement("base");
			WriteElementWithAttribute(writer, "alias", "source", ws.SortRules);
			writer.WriteEndElement();
			if (reader != null)
			{
				// don't copy anything, but skip to the 1st special node
				FindElement(reader, "special");
			}
		}

		private void WriteCollationRulesFromCustomSimple(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			Debug.Assert(ws.SortUsing == WritingSystemDefinition.SortRulesType.CustomSimple);

			string message;
			// avoid throwing exception, just don't save invalid data
			if (!SimpleRulesCollator.ValidateSimpleRules(ws.SortRules ?? string.Empty, out message))
			{
				return;
			}
			string icu = SimpleRulesCollator.ConvertToIcuRules(ws.SortRules ?? string.Empty);
			WriteCollationRulesFromICUString(writer, reader, icu);
		}

		private void WriteCollationRulesFromCustomICU(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			Debug.Assert(ws.SortUsing == WritingSystemDefinition.SortRulesType.CustomICU);
			WriteCollationRulesFromICUString(writer, reader, ws.SortRules);
		}

		private void WriteCollationRulesFromICUString(XmlWriter writer, XmlReader reader, string icu)
		{
			Debug.Assert(writer != null);
			icu = icu ?? string.Empty;
			if (reader != null)
			{
				// don't copy any alias that would override our rules
				if (FindElement(reader, "alias"))
				{
					reader.Skip();
				}
				CopyUntilElement(writer, reader, "settings");
				// for now we'll omit anything in the suppress_contractions and optimize nodes
				FindElement(reader, "special");
			}
			IcuRulesParser parser = new IcuRulesParser(false);
			string message;
			// avoid throwing exception, just don't save invalid data
			if (!parser.ValidateIcuRules(icu, out message))
			{
				return;
			}
			parser.WriteIcuRules(writer, icu);
		}
	}
}