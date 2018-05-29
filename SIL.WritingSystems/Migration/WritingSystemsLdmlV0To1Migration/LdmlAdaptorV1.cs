using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SIL.Xml;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	internal class LdmlAdaptorV1
	{
		private readonly XmlNamespaceManager _nameSpaceManager;
		private bool _wsIsFlexPrivateUse;

		public LdmlAdaptorV1()
		{
			_nameSpaceManager = MakeNameSpaceManager();
		}

		public void Read(string filePath, WritingSystemDefinitionV1 ws)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			var settings = new XmlReaderSettings();
			settings.NameTable = _nameSpaceManager.NameTable;
			settings.ValidationType = ValidationType.None;
			settings.XmlResolver = null;
			settings.DtdProcessing = DtdProcessing.Parse;
			using (XmlReader reader = XmlReader.Create(filePath, settings))
			{
				ReadLdml(reader, ws);
			}
		}

		public void Read(XmlReader xmlReader, WritingSystemDefinitionV1 ws)
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
			settings.DtdProcessing = DtdProcessing.Parse;
			using (XmlReader reader = XmlReader.Create(xmlReader, settings))
			{
				ReadLdml(reader, ws);
			}
		}

		private static bool FindElement(XmlReader reader, string name)
		{
			return XmlHelpers.FindNextElementInSequence(reader, name, LdmlNodeComparer.CompareElementNames);
		}

		public static void WriteLdmlText(XmlWriter writer, string text)
		{
			// Not all Unicode characters are valid in an XML document, so we need to create
			// the <cp hex="X"> elements to replace the invalid characters.
			// Note: While 0xD (carriage return) is a valid XML character, it is automatically
			// either dropped or coverted to 0xA by any conforming XML parser, so we also make a <cp>
			// element for that one.
			StringBuilder sb = new StringBuilder(text.Length);
			for (int i = 0; i < text.Length; i++)
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

		private void ReadLdml(XmlReader reader, WritingSystemDefinitionV1 ws)
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
			while (FindElement(reader, "special"))
			{
				ReadTopLevelSpecialElement(reader, ws);
			}
			ws.StoreID = "";
		}

		protected virtual void ReadTopLevelSpecialElement(XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			if (reader.GetAttribute("xmlns:palaso") != null)
			{
				reader.ReadStartElement("special");
				ws.Abbreviation = GetSpecialValue(reader, "palaso", "abbreviation");
				ws.DefaultFontName = GetSpecialValue(reader, "palaso", "defaultFontFamily");
				float fontSize;
				if (float.TryParse(GetSpecialValue(reader, "palaso", "defaultFontSize"), out fontSize))
				{
					ws.DefaultFontSize = fontSize;
				}
				ws.Keyboard = GetSpecialValue(reader, "palaso", "defaultKeyboard");
				string isLegacyEncoded = GetSpecialValue(reader, "palaso", "isLegacyEncoded");
				if (!String.IsNullOrEmpty(isLegacyEncoded))
				{
					ws.IsUnicodeEncoded = !Convert.ToBoolean(isLegacyEncoded);
				}
				ws.LanguageName = GetSpecialValue(reader, "palaso", "languageName");
				ws.SpellCheckingId = GetSpecialValue(reader, "palaso", "spellCheckingId");
				if (!_wsIsFlexPrivateUse)
				{
					string version = GetSpecialValue(reader, "palaso", "version");
					version = string.IsNullOrEmpty(version) ? "0" : version;
					const string expectedVersion = "2";
					if (version != expectedVersion)
					{
						throw new ApplicationException(String.Format(
														   "The LDML tag '{0}' is version {1}.  Version {2} was expected.",
														   ws.Bcp47Tag,
														   version,
														   expectedVersion
														   ));
					}
				}
				ReadSpecialEndElement(reader);
			}
			else if (reader.GetAttribute("xmlns:palaso2") != null)
			{
				reader.ReadStartElement("special");
				GetKnownKeyboards(reader, ws);
				ReadSpecialEndElement(reader);
			}
			else if (reader.GetAttribute("xmlns:fw") != null)
			{
				ws.WindowsLcid = GetLcid(reader);
				ReadSpecialEndElement(reader);
			}
			else
			{
				reader.Skip();
			}
		}

		/// <summary>
		/// Finds and reads the EndElement "&lt;/special&gt;"
		/// </summary>
		/// <param name="reader"></param>
		protected static void ReadSpecialEndElement(XmlReader reader)
		{
			XmlHelpers.ReadEndElement(reader, "special");
		}

		private string GetLcid(XmlReader reader)
		{
			reader.MoveToContent();
			Debug.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == "special");
			using (var lcidReader = reader.ReadSubtree())
			{
				lcidReader.ReadStartElement("special");
				lcidReader.MoveToContent();
				return GetSpecialValue(lcidReader, "fw", "windowsLCID", "urn://fieldworks.sil.org/ldmlExtensions/v1");
			}
		}

		private void GetKnownKeyboards(XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			reader.MoveToContent();
			Debug.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == Palaso2NamespaceName+ ":" + KnownKeyboardsElementName);
			using (var knownKeyboardsReader = reader.ReadSubtree())
			{
				knownKeyboardsReader.MoveToContent();
				knownKeyboardsReader.ReadStartElement(Palaso2NamespaceName + ":" + KnownKeyboardsElementName);
				knownKeyboardsReader.MoveToContent();
				while (knownKeyboardsReader.NodeType == XmlNodeType.Element && knownKeyboardsReader.Name == Palaso2NamespaceName + ":" + KeyboardElementName)
				{
					var keyboard = new KeyboardDefinitionV1(knownKeyboardsReader.GetAttribute(LayoutAttrName),
						knownKeyboardsReader.GetAttribute(LocaleAttrName));
					// Review EberhardB (JohnT): do we actually want to store OS in the LDML, or at all? If not, get rid of this.
					// If so, we need to make sure it can be loaded into the objects made by the real KeyboardController.
					PlatformID id;
					if (Enum.TryParse(knownKeyboardsReader.GetAttribute(OSAttrName), out id))
						keyboard.OperatingSystem = id;
					knownKeyboardsReader.Read();
					FindElement(knownKeyboardsReader, KeyboardElementName);
					ws.AddKnownKeyboard(keyboard);
				}
			}
		}

		private void ReadIdentityElement(XmlReader reader, WritingSystemDefinitionV1 ws)
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

				string language = GetSubNodeAttributeValue(identityReader, "language", "type");
				string script = GetSubNodeAttributeValue(identityReader, "script", "type");
				string region = GetSubNodeAttributeValue(identityReader, "territory", "type");
				string variant = GetSubNodeAttributeValue(identityReader, "variant", "type");

				if ((language.StartsWith("x-", StringComparison.OrdinalIgnoreCase) || language.Equals("x", StringComparison.OrdinalIgnoreCase)))
				{
					var flexRfcTagInterpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
					flexRfcTagInterpreter.ConvertToPalasoConformPrivateUseRfc5646Tag(language, script, region, variant);
					ws.SetAllComponents(flexRfcTagInterpreter.Language, flexRfcTagInterpreter.Script, flexRfcTagInterpreter.Region, flexRfcTagInterpreter.Variant);

					_wsIsFlexPrivateUse = true;
				}
				else
				{
					ws.SetAllComponents(language, script, region, variant);

					_wsIsFlexPrivateUse = false;
				}
				//Set the id simply as the concatenation of whatever was in the ldml file.
				ws.Id = String.Join("-", new[] { language, script, region, variant }.Where(subtag => !String.IsNullOrEmpty(subtag)).ToArray());
				// move to end of identity node
				while (identityReader.Read()) ;
			}
			if (!reader.IsEmptyElement)
			{
				reader.ReadEndElement();
			}
		}

		private void ReadLayoutElement(XmlReader reader, WritingSystemDefinitionV1 ws)
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
				while (layoutReader.Read()) ;
			}
			if (!reader.IsEmptyElement)
			{
				reader.ReadEndElement();
			}
		}

		private void ReadCollationsElement(XmlReader reader, WritingSystemDefinitionV1 ws)
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
				while (collationsReader.Read()) ;
			}
			if (!reader.IsEmptyElement)
			{
				reader.ReadEndElement();
			}
		}

		private void ReadCollationElement(string collationXml, WritingSystemDefinitionV1 ws)
		{
			Debug.Assert(collationXml != null);
			Debug.Assert(ws != null);

			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.CloseInput = true;
			readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			using (XmlReader collationReader = XmlReader.Create(new StringReader(collationXml), readerSettings))
			{
				if (FindElement(collationReader, "special"))
				{
					collationReader.Read();
					string rulesTypeAsString = GetSpecialValue(collationReader, "palaso", "sortRulesType");
					if (!String.IsNullOrEmpty(rulesTypeAsString))
					{
						ws.SortUsing = (WritingSystemDefinitionV1.SortRulesType) Enum.Parse(typeof(WritingSystemDefinitionV1.SortRulesType), rulesTypeAsString);
					}
				}
			}
			switch (ws.SortUsing)
			{
				case WritingSystemDefinitionV1.SortRulesType.OtherLanguage:
					ReadCollationRulesForOtherLanguage(collationXml, ws);
					break;
				case WritingSystemDefinitionV1.SortRulesType.CustomSimple:
					ReadCollationRulesForCustomSimple(collationXml, ws);
					break;
				case WritingSystemDefinitionV1.SortRulesType.CustomICU:
					ReadCollationRulesForCustomICU(collationXml, ws);
					break;
				case WritingSystemDefinitionV1.SortRulesType.DefaultOrdering:
					break;
				default:
					string message = string.Format("Unhandled SortRulesType '{0}' while writing LDML definition file.", ws.SortUsing);
					throw new ApplicationException(message);
			}
		}

		private void ReadCollationRulesForOtherLanguage(string collationXml, WritingSystemDefinitionV1 ws)
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
					ws.SortUsing = WritingSystemDefinitionV1.SortRulesType.CustomICU;
					ReadCollationRulesForCustomICU(collationXml, ws);
				}
			}
		}

		private void ReadCollationRulesForCustomICU(string collationXml, WritingSystemDefinitionV1 ws)
		{
			ws.SortRules = LdmlCollationParserV1.GetIcuRulesFromCollationNode(collationXml);
		}

		private void ReadCollationRulesForCustomSimple(string collationXml, WritingSystemDefinitionV1 ws)
		{
			string rules;
			if (LdmlCollationParserV1.TryGetSimpleRulesFromCollationNode(collationXml, out rules))
			{
				ws.SortRules = rules;
				return;
			}
			// fall back to ICU rules if Simple rules don't work
			ws.SortUsing = WritingSystemDefinitionV1.SortRulesType.CustomICU;
			ReadCollationRulesForCustomICU(collationXml, ws);
		}

		/// <summary>
		/// The "oldFile" parameter allows the LdmldataMapper to allow data that it doesn't understand to be roundtripped.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="ws"></param>
		/// <param name="oldFile"></param>
		public void Write(string filePath, WritingSystemDefinitionV1 ws, Stream oldFile)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			XmlReader reader = null;
			try
			{
				if (oldFile != null)
				{
					var readerSettings = new XmlReaderSettings();
					readerSettings.NameTable = _nameSpaceManager.NameTable;
					readerSettings.IgnoreWhitespace = true;
					readerSettings.ConformanceLevel = ConformanceLevel.Auto;
					readerSettings.ValidationType = ValidationType.None;
					readerSettings.XmlResolver = null;
					readerSettings.DtdProcessing = DtdProcessing.Parse;
					reader = XmlReader.Create(oldFile, readerSettings);
				}
				
				string backupFilePath = null;
				if (File.Exists(filePath))
				{
					try
					{
						backupFilePath = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(filePath));
						File.Copy(filePath, backupFilePath);
					}
					catch (Exception)
					{
						backupFilePath = null;
					}
				}
				try
				{
					using (var writer = XmlWriter.Create(filePath, CanonicalXmlSettings.CreateXmlWriterSettings()))
					{
						writer.WriteStartDocument();
						WriteLdml(writer, reader, ws);
					}
				}
				catch (Exception)
				{
					if (backupFilePath != null)
						File.Copy(backupFilePath, filePath, true);
					throw;
				}

				if (backupFilePath != null)
					File.Delete(backupFilePath);
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}
		}

		/// <summary>
		/// The "oldFileReader" parameter allows the LdmldataMapper to allow data that it doesn't understand to be roundtripped.
		/// </summary>
		public void Write(XmlWriter xmlWriter, WritingSystemDefinitionV1 ws, XmlReader oldFileReader)
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
				if (oldFileReader != null)
				{
					XmlReaderSettings settings = new XmlReaderSettings();
					settings.NameTable = _nameSpaceManager.NameTable;
					settings.IgnoreWhitespace = true;
					settings.ConformanceLevel = ConformanceLevel.Auto;
					settings.ValidationType = ValidationType.None;
					settings.XmlResolver = null;
					settings.DtdProcessing = DtdProcessing.Parse;
					reader = XmlReader.Create(oldFileReader, settings);
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

		private void WriteLdml(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			_wsIsFlexPrivateUse = false;
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
			WriteTopLevelSpecialElements(writer, reader, ws);
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
			while(!reader.EOF && reader.NodeType != XmlNodeType.EndElement
					 && (reader.NodeType != XmlNodeType.Element || reader.Name == "special"))
			{
				if(reader.NodeType == XmlNodeType.Element)
				{
					bool knownNS = IsKnownSpecialElement(reader);
					reader.MoveToElement();
					if(knownNS)
					{
						reader.Skip();
						continue;
					}
				}
				writer.WriteNode(reader, false);
			}
		}

		private bool IsKnownSpecialElement(XmlReader reader)
		{
			while (reader.MoveToNextAttribute())
			{
				if (reader.Name.StartsWith("xmlns:") && _nameSpaceManager.HasNamespace(reader.Name.Substring(6, reader.Name.Length - 6)))
					return true;
			}
			return false;
		}

		private string GetSubNodeAttributeValue(XmlReader reader, string elementName, string attributeName)
		{
			return FindElement(reader, elementName) ? (reader.GetAttribute(attributeName) ?? string.Empty) : string.Empty;
		}

		protected string GetSpecialValue(XmlReader reader, string ns, string field)
		{
			return GetSpecialValue(reader, ns, field, _nameSpaceManager.LookupNamespace(ns));
		}

		protected string GetSpecialValue(XmlReader reader, string ns, string field, string nameSpaceUri)
		{
			if(!XmlHelpers.FindNextElementInSequence(reader, ns + ":" + field, nameSpaceUri, string.Compare))
			{
				return string.Empty;
			}
			return reader.GetAttribute("value") ?? string.Empty;
		}

		private XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNamespaceManager m = new XmlNamespaceManager(new NameTable());
			AddNamespaces(m);
			return m;
		}

		protected virtual void AddNamespaces(XmlNamespaceManager m)
		{
			m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			m.AddNamespace("palaso2", "urn://palaso.org/ldmlExtensions/v2");
		}

		private void WriteElementWithAttribute(XmlWriter writer, string elementName, string attributeName, string value)
		{
			writer.WriteStartElement(elementName);
			writer.WriteAttributeString(attributeName, value);
			writer.WriteEndElement();
		}

		protected void WriteSpecialValue(XmlWriter writer, string ns, string field, string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return;
			}
			writer.WriteStartElement(field, _nameSpaceManager.LookupNamespace(ns));
			writer.WriteAttributeString("value", value);
			writer.WriteEndElement();
		}

		private void WriteIdentityElement(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			writer.WriteStartElement("identity");
			writer.WriteStartElement("version");
			writer.WriteAttributeString("number", ws.VersionNumber);
			writer.WriteString(ws.VersionDescription);
			writer.WriteEndElement();
			WriteElementWithAttribute(writer, "generation", "date", String.Format("{0:s}", ws.DateModified));

			bool copyFlexFormat = false;
			string language = String.Empty;
			string script = String.Empty;
			string territory = String.Empty;
			string variant = String.Empty;
			bool readerIsOnIdentityElement = IsReaderOnElementNodeNamed(reader, "identity");
			if(readerIsOnIdentityElement && !reader.IsEmptyElement)
			{
				reader.ReadToDescendant("language");
				while(!IsReaderOnElementNodeNamed(reader, "special") && !IsReaderOnEndElementNodeNamed(reader, "identity"))
				{
					switch(reader.Name)
					{
						case "language":
							language = reader.GetAttribute("type");
							break;
						case "script":
							script = reader.GetAttribute("type");
							break;
						case "territory":
							territory = reader.GetAttribute("type");
							break;
						case "variant":
							variant = reader.GetAttribute("type");
							break;
					}
					reader.Read();
				}
			}
			if (copyFlexFormat)
			{
				WriteRFC5646TagElements(writer, language, script, territory, variant);
			}
			else
			{
				WriteRFC5646TagElements(writer, ws.Language, ws.Script, ws.Region, ws.Variant);
			}
			if (IsReaderOnElementNodeNamed(reader, "identity"))
			{
				if (reader.IsEmptyElement)
				{
					reader.Skip();
				}
			}
			if (IsReaderOnElementNodeNamed(reader, "special"))
			{
				CopyToEndElement(writer, reader);
			}
			if(IsReaderOnEndElementNodeNamed(reader, "identity"))
			{
				reader.Read();
			}
			writer.WriteEndElement();
		}

		private void WriteRFC5646TagElements(XmlWriter writer, string language, string script, string region, string variant)
		{
			WriteElementWithAttribute(writer, "language", "type", language);
			if (!String.IsNullOrEmpty(script))
			{
				WriteElementWithAttribute(writer, "script", "type", script);
			}
			if (!String.IsNullOrEmpty(region))
			{
				WriteElementWithAttribute(writer, "territory", "type", region);
			}
			if (!String.IsNullOrEmpty(variant))
			{
				WriteElementWithAttribute(writer, "variant", "type", variant);
			}
		}

		private bool IsReaderOnElementNodeNamed(XmlReader reader, string name)
		{
			return reader != null && reader.NodeType == XmlNodeType.Element && reader.Name == name;
		}

		private bool IsReaderOnEndElementNodeNamed(XmlReader reader, string name)
		{
			return reader != null && reader.NodeType == XmlNodeType.EndElement && reader.Name == name;
		}

		private void WriteLayoutElement(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			bool needToCopy = reader != null && reader.NodeType == XmlNodeType.Element && reader.Name == "layout";
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

		protected void WriteBeginSpecialElement(XmlWriter writer, string ns)
		{
			writer.WriteStartElement("special");
			writer.WriteAttributeString("xmlns", ns, null, _nameSpaceManager.LookupNamespace(ns));
		}

		private const string KnownKeyboardsElementName = "knownKeyboards";
		private const string Palaso2NamespaceName = "palaso2";
		private const string KeyboardElementName = "keyboard";
		private const string LayoutAttrName = "layout";
		private const string LocaleAttrName = "locale";
		private const string OSAttrName = "os";
		protected virtual void WriteTopLevelSpecialElements(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			// Note. As per appendix L2 'Canonical Form' of the LDML specification elements are ordered alphabetically.
			WriteBeginSpecialElement(writer, "palaso");
			WriteFlexOrPalasoConformElement(writer, reader, "palaso", "abbreviation", ws.Abbreviation);
			WriteSpecialValue(writer, "palaso", "defaultFontFamily", ws.DefaultFontName);
			if (ws.DefaultFontSize != 0)
			{
				WriteSpecialValue(writer, "palaso", "defaultFontSize", ws.DefaultFontSize.ToString());
			}
			WriteSpecialValue(writer, "palaso", "defaultKeyboard", ws.Keyboard);
			if (!ws.IsUnicodeEncoded)
			{
				WriteSpecialValue(writer, "palaso", "isLegacyEncoded", (!ws.IsUnicodeEncoded).ToString());
			}
			WriteFlexOrPalasoConformElement(writer, reader, "palaso", "languageName", ws.LanguageName);
			if (!String.IsNullOrEmpty(ws.SpellCheckingId))
			{
				WriteSpecialValue(writer, "palaso", "spellCheckingId", ws.SpellCheckingId);
			}
			// V1 only migrates to version 2
			WriteFlexOrPalasoConformElement(writer, reader, "palaso", "version", "2");
			writer.WriteEndElement();

			if (ws.KnownKeyboards.Any())
			{
				var p2Namespace = _nameSpaceManager.LookupNamespace(Palaso2NamespaceName);
				WriteBeginSpecialElement(writer, Palaso2NamespaceName);
				writer.WriteStartElement(KnownKeyboardsElementName, p2Namespace);
				foreach (var keyboard in ws.KnownKeyboards)
				{
					writer.WriteStartElement(KeyboardElementName, p2Namespace);
					writer.WriteAttributeString(LayoutAttrName, keyboard.Layout);
					writer.WriteAttributeString(LocaleAttrName, keyboard.Locale);
					writer.WriteAttributeString(OSAttrName, keyboard.OperatingSystem.ToString());
					writer.WriteEndElement(); // Keyboard
				}
				writer.WriteEndElement(); // KnownKeyboards
				WriteFlexOrPalasoConformElement(writer, reader, Palaso2NamespaceName, "version",
					LdmlDataMapper.CurrentLdmlLibraryVersion.ToString());
				writer.WriteEndElement(); // Special
			}
		}

		private void WriteFlexOrPalasoConformElement(XmlWriter writer, XmlReader reader, string nameSpaceName, string nodeName, string value)
		{
			if(_wsIsFlexPrivateUse)
			{
				CopyOldFlexNode(reader, writer, nameSpaceName, nodeName);
			}
			else
			{
				WriteSpecialValue(writer, nameSpaceName, nodeName, value);
			}
		}

		private void CopyOldFlexNode(XmlReader reader, XmlWriter writer, string nameSpaceName, string nodeName)
		{
			if(reader != null && reader.ReadToDescendant(nodeName, _nameSpaceManager.LookupNamespace(nameSpaceName)))
			{
				writer.WriteNode(reader, true);
			}
		}

		private void WriteCollationsElement(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
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

		private void WriteCollationElement(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
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

			if (ws.SortUsing == WritingSystemDefinitionV1.SortRulesType.DefaultOrdering && !needToCopy)
				return;

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

			if (ws.SortUsing != WritingSystemDefinitionV1.SortRulesType.DefaultOrdering)
			{
				writer.WriteStartElement("collation");
				switch (ws.SortUsing)
				{
					case WritingSystemDefinitionV1.SortRulesType.OtherLanguage:
						WriteCollationRulesFromOtherLanguage(writer, reader, ws);
						break;
					case WritingSystemDefinitionV1.SortRulesType.CustomSimple:
						WriteCollationRulesFromCustomSimple(writer, reader, ws);
						break;
					case WritingSystemDefinitionV1.SortRulesType.CustomICU:
						WriteCollationRulesFromCustomICU(writer, reader, ws);
						break;
					default:
						string message = string.Format("Unhandled SortRulesType '{0}' while writing LDML definition file.", ws.SortUsing);
						throw new ApplicationException(message);
				}
				WriteBeginSpecialElement(writer, "palaso");
				WriteSpecialValue(writer, "palaso", "sortRulesType", ws.SortUsing.ToString());
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
			else if (needToCopy)
			{
				bool startElementWritten = false;
				if (FindElement(reader, "special"))
				{
					// write out any other special elements
					while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement
						&& (reader.NodeType != XmlNodeType.Element || reader.Name == "special"))
					{
						if (reader.NodeType == XmlNodeType.Element)
						{
							bool knownNS = IsKnownSpecialElement(reader);
							reader.MoveToElement();
							if (knownNS)
							{
								reader.Skip();
								continue;
							}
						}
						if (!startElementWritten)
						{
							writer.WriteStartElement("collation");
							startElementWritten = true;
						}
						writer.WriteNode(reader, false);
					}
				}

				if (!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
				{
					// copy any other elements
					if (!startElementWritten)
					{
						writer.WriteStartElement("collation");
						startElementWritten = true;
					}
					CopyToEndElement(writer, reader);
				}
				if (startElementWritten)
					writer.WriteEndElement();
			}
		}

		private void WriteCollationRulesFromOtherLanguage(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			Debug.Assert(ws.SortUsing == WritingSystemDefinitionV1.SortRulesType.OtherLanguage);

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

		private void WriteCollationRulesFromCustomSimple(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			Debug.Assert(ws.SortUsing == WritingSystemDefinitionV1.SortRulesType.CustomSimple);

			var parser = new SimpleRulesParser();
			string message;
			// avoid throwing exception, just don't save invalid data
			if (!parser.ValidateSimpleRules(ws.SortRules ?? string.Empty, out message))
			{
				return;
			}
			string icu = parser.ConvertToIcuRules(ws.SortRules ?? string.Empty);
			WriteCollationRulesFromICUString(writer, reader, icu);
		}

		private void WriteCollationRulesFromCustomICU(XmlWriter writer, XmlReader reader, WritingSystemDefinitionV1 ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			Debug.Assert(ws.SortUsing == WritingSystemDefinitionV1.SortRulesType.CustomICU);
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
