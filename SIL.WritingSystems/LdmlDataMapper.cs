using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Icu;
using Palaso.Extensions;
using Palaso.Xml;

namespace SIL.WritingSystems
{
	/// <summary>
	/// The LdmlDatamapper Reads and Writes WritingSystemDefinitions to LDML files. A typical consuming application should not
	/// need to use the LdmlDataMapper directly but should rather use an IWritingSystemRepository (such as the
	/// LdmlInfolderWritingSystemRepository) to manage it's writing systems.
	/// The LdmlDatamapper is tightly tied to a particular (palaso) version of LDML. If the LdmlDatamapper refuses to Read a
	/// particular Ldml file it may need to be migrated to the latest version. Please use the
	/// LdmlInFolderWritingSystemRepository class for this purpose.
	/// Please note that the LdmlDataMapper.Write method can round trip data that it does not understand if passed an
	/// appropriate stream or xmlreader produced from the old file.
	/// Be aware that as of Jul-5-2011 an exception was made for certain well defined Fieldworks LDML files whose contained
	/// Rfc5646 tag begin with "x-". These will load correctly, albeit in a transformed state, in spite of being "Version 0".
	/// Furthermore writing systems containing RfcTags beginning with "x-" and that have a matching Fieldworks conform LDML file
	/// in the repository will not be changed including no marking with "version 1".
	/// </summary>
	public class LdmlDataMapper
	{
		private readonly XmlNamespaceManager _nameSpaceManager;
		private bool _wsIsFlexPrivateUse;
#if WS_FIX
		private WritingSystemCompatibility _compatibilityMode;
#endif
		private static XNamespace FW = "urn://fieldworks.sil.org/ldmlExtensions/v1";
		private static XNamespace Palaso = "urn://palaso.org/ldmlExtensions/v1";
		private static XNamespace Palaso2 = "urn://palaso.org/ldmlExtensions/v2";
		private static XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		public LdmlDataMapper()
		{
			_nameSpaceManager = MakeNameSpaceManager();
		}

		/// <summary>
		/// Mapping of font engine attribute to FontEngines enumeration.
		/// If this attribute is missing, the engines are assumed to be "gr ot"
		/// </summary>
		private static readonly Dictionary<string, FontEngines> EngineToFontEngines = new Dictionary<string, FontEngines>
		{
			{string.Empty, FontEngines.OpenType | FontEngines.Graphite },
			{"ot", FontEngines.OpenType},
			{"gr", FontEngines.Graphite}
		};

		/// <summary>
		/// Mapping of font role/type attribute to FontRoles enumeration
		/// If this attribute is missing, the font role default is used
		/// </summary>
		private static readonly Dictionary<string, FontRoles> RoleToFontRoles = new Dictionary<string, FontRoles>
		{
			{string.Empty, FontRoles.Default},
			{"default", FontRoles.Default},
			{"heading", FontRoles.Heading},
			{"emphasis", FontRoles.Emphasis}
		};

		/// <summary>
		/// Mapping of spell checking type attribute to SpellCheckDictionaryFormat
		/// </summary>
		private static readonly Dictionary<string, SpellCheckDictionaryFormat> SpellCheckToSpecllCheckDictionaryFormats = new Dictionary
			<string, SpellCheckDictionaryFormat>
		{
			{string.Empty, SpellCheckDictionaryFormat.Unknown},
			{"hunspell", SpellCheckDictionaryFormat.Hunspell},
			{"wordlist", SpellCheckDictionaryFormat.Wordlist},
			{"lift", SpellCheckDictionaryFormat.Lift}
		};

		/// <summary>
		/// Mapping of keyboard type attribute to KeyboardFormat
		/// </summary>
		private static readonly Dictionary<string, KeyboardFormat> KeyboardToKeyboardFormat = new Dictionary<string, KeyboardFormat>
		{
			{string.Empty, KeyboardFormat.Unknown},
			{"kmn", KeyboardFormat.Keyman },
			{"kmx", KeyboardFormat.CompiledKeyman },
			{"msklc", KeyboardFormat.Msklc},
			{"ldml", KeyboardFormat.Ldml},
			{"keylayout", KeyboardFormat.Keylayout}
		}; 

		/// <summary>
		/// Mapping of context attribute to PunctuationPatternContext
		/// </summary>
		private static readonly Dictionary<string, PunctuationPatternContext> ContextToPunctuationPatternContext = new Dictionary<string, PunctuationPatternContext>
		{
			{"init", PunctuationPatternContext.Initial},
			{"medial", PunctuationPatternContext.Medial},
			{"final", PunctuationPatternContext.Final},
			{"break", PunctuationPatternContext.Break},
			{"isolate", PunctuationPatternContext.Isolate}
		};

		/// <summary>
		/// Mapping of paraContinueType attribute to QuotationParagraphContinueType
		/// </summary>
		private static readonly Dictionary<string, QuotationParagraphContinueType> QuotationToQuotationParagraphContinueTypes = new Dictionary<string, QuotationParagraphContinueType>
		{
			{string.Empty, QuotationParagraphContinueType.None},
			{"all", QuotationParagraphContinueType.All},
			{"outer", QuotationParagraphContinueType.Outermost},
			{"innter", QuotationParagraphContinueType.Innermost}
		};

		/// <summary>
		/// Mapping of quotation marking system attribute to QuotationMarkingSystemType
		/// </summary>
		private static readonly Dictionary<string, QuotationMarkingSystemType>QuotationToQuotationMarkingSystemTypes = new Dictionary<string, QuotationMarkingSystemType>
		{
			{string.Empty, QuotationMarkingSystemType.Normal},
			{"narrative", QuotationMarkingSystemType.Narrative}
		};

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
			XElement element = XElement.Load(filePath);
			ReadLdml(element, ws);
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
			var settings = new XmlReaderSettings
			{
				NameTable = _nameSpaceManager.NameTable,
				ConformanceLevel = ConformanceLevel.Auto,
				ValidationType = ValidationType.None,
				XmlResolver = null,
				DtdProcessing = DtdProcessing.Parse
			};
			using (XmlReader reader = XmlReader.Create(xmlReader, settings))
			{
				XElement element = XElement.Load(reader);
				ReadLdml(element, ws);
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

		private void ReadLdml(XElement element, WritingSystemDefinition ws)
		{
			Debug.Assert(element != null);
			Debug.Assert(ws != null);
			if (element.Name != "ldml")
			{
				throw new ApplicationException("Unable to load writing system definition: Missing <ldml> tag.");
			}

			XElement identityElem = element.Element("identity");
			if (identityElem != null)
				ReadIdentityElement(identityElem, ws);

			XElement charactersElem = element.Element("characters");
			if (charactersElem != null)
				ReadCharacterElem(charactersElem, ws);

			XElement delimitersElem = element.Element("delimiters");
			if (delimitersElem != null)
				ReadDelimitersElem(delimitersElem, ws);

			XElement layoutElem = element.Element("layout");
			if (layoutElem != null)
				ReadLayoutElement(layoutElem, ws);

			XElement collationsElem = element.Element("collations");
			if (collationsElem != null)
				ReadCollationsElement(collationsElem, ws);

			foreach (XElement specialElem in element.Elements("special"))
			{
				ReadTopLevelSpecialElement(specialElem, ws);
			}
			ws.StoreID = "";
			ws.AcceptChanges();
		}

		protected virtual void ReadTopLevelSpecialElement(XElement specialElem, WritingSystemDefinition ws)
		{
			if (specialElem.Attribute(XNamespace.Xmlns + "palaso") != null)
			{
				ws.Abbreviation = GetSpecialValue(specialElem, Palaso, "abbreviation");
#if WS_FIX
				ws.DefaultFontName = GetAttributeString(specialElem, "palaso", "defaultFontFamily");
				ws.DefaultFontSize = GetAttributeFloat(specialElem, "palaso", "defaultFontSize");
#endif
				ws.Keyboard = GetSpecialValue(specialElem, Palaso, "defaultKeyboard");
				string isLegacyEncoded = GetSpecialValue(specialElem, Palaso, "isLegacyEncoded");
				if (!String.IsNullOrEmpty(isLegacyEncoded))
				{
					ws.IsUnicodeEncoded = !Convert.ToBoolean(isLegacyEncoded);
				}
				ws.LanguageName = GetSpecialValue(specialElem, Palaso, "languageName");
#if WS_FIX
				ws.SpellCheckingId = GetSpecialValue(specialElem, Palaso, "spellCheckingId");
#endif
				if (!_wsIsFlexPrivateUse)
				{
					string version = GetSpecialValue(specialElem, Palaso, "version");
					version = string.IsNullOrEmpty(version) ? "0" : version;
					if (version != WritingSystemDefinition.LatestWritingSystemDefinitionVersion.ToString())
					{
						throw new ApplicationException(String.Format(
							"The LDML tag '{0}' is version {1}.  Version {2} was expected.",
							ws.Bcp47Tag,
							version,
							WritingSystemDefinition.LatestWritingSystemDefinitionVersion
							));
					}
				}
			}
			else if (specialElem.Attribute(XNamespace.Xmlns + "palaso2") != null)
			{
				XElement keyboardElem = specialElem.Element("special");
				GetKnownKeyboards(keyboardElem, ws);
			}
			else if (specialElem.Attribute(XNamespace.Xmlns + "fw") != null)
			{
				ws.WindowsLcid = GetLcid(specialElem);
			}
			else
			{
				XElement externalResourcesElem = specialElem.Element(Sil + "external-resources");
				if (externalResourcesElem != null)
				{
					ReadFontElement(externalResourcesElem, ws);
					ReadSpellcheckElement(externalResourcesElem, ws);
					ReadKeyboardElement(externalResourcesElem, ws);
				}
			}
		}

		private string GetLcid(XElement element)
		{
			Debug.Assert(element.NodeType == XmlNodeType.Element && element.Name == "special");
			XElement windowsLcidElem = element.Element(FW + "windowsLCID");
			return windowsLcidElem == null ? string.Empty : (string) windowsLcidElem.Attribute("value");
		}

		private void GetKnownKeyboards(XElement knownKeyboards, WritingSystemDefinition ws)
		{
			Debug.Assert(knownKeyboards.Name == KnownKeyboardsElementName);
			IEnumerable<XElement> keyboardList = knownKeyboards.Elements(Palaso2 + KeyboardElementName);
			foreach(XElement keyboardElem in keyboardList)
			{
#if WS_FIX
				IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboardDefinition((string)keyboardElem.Attribute(LayoutAttrName),
					(string)keyboardElem.Attribute(LocaleAttrName));
				ws.KnownKeyboards.Add(keyboard);
#endif
			}
		}

		private void ReadFontElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			foreach (XElement fontElem in externalResourcesElem.Elements(Sil + "font"))
			{
				string fontName = fontElem.GetAttributeValue("name");
				if (!fontName.Equals(string.Empty))
				{
					FontDefinition fd = new FontDefinition(fontName);

					// Types (space separate list)
					string roles = fontElem.GetAttributeValue("types");
					if (!String.IsNullOrEmpty(roles))
					{
						IEnumerable<string> roleList = roles.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
						foreach (string roleEntry in roleList)
						{
							fd.Roles |= RoleToFontRoles[roleEntry];
						}
					}
					else
					{
						fd.Roles = FontRoles.Default;
					}
							
					// Size
					fd.DefaultSize = (float?) fontElem.Attribute("size") ?? 1.0f;

					// Minversion
					fd.MinVersion = fontElem.GetAttributeValue("minversion");

					// Features (space separated list of key=value pairs)
					fd.Features = fontElem.GetAttributeValue("features");

					// Language
					fd.Language = fontElem.GetAttributeValue("lang");

					// OpenType language
					fd.OpenTypeLanguage = fontElem.GetAttributeValue("otlang");

					// Font Engine (space separated list) supercedes legacy isGraphite flag
					string engines = fontElem.GetAttributeValue("engines");
					if (!String.IsNullOrEmpty(engines))
					{
						IEnumerable<string> engineList = engines.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
						foreach (string engineEntry in engineList)
						{
							fd.Engines |= (EngineToFontEngines[engineEntry]);
						}
					}

					// Subset
					fd.Subset = fontElem.GetAttributeValue("subset").ToLower();

					// URL elements
					foreach (XElement urlElem in fontElem.Elements(Sil + "url"))
					{
						fd.Urls.Add(urlElem.Value);
					}
					ws.Fonts.Add(fd);
				}
			}
		}

		private void ReadSpellcheckElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			foreach (XElement scElem in externalResourcesElem.Elements(Sil + "spellcheck"))
			{
				string type = scElem.GetAttributeValue("type");
				if (!type.Equals(string.Empty))
				{
					SpellCheckDictionaryDefinition scd =
						new SpellCheckDictionaryDefinition(ws.Bcp47Tag, SpellCheckToSpecllCheckDictionaryFormats[type]);

					// URL elements
					foreach (XElement urlElem in scElem.Elements(Sil + "url"))
					{
						scd.Urls.Add(urlElem.Value);
					}
					ws.SpellCheckDictionaries.Add(scd);
				}
			}
		}

		private void ReadKeyboardElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			foreach (XElement kbdElem in externalResourcesElem.Elements(Sil + "kbd"))
			{
				string id = kbdElem.GetAttributeValue("id");
				if (!string.IsNullOrEmpty(id))
				{
					KeyboardFormat format = KeyboardToKeyboardFormat[kbdElem.GetAttributeValue("type")];
					List<string> urls = new List<string>();
					foreach (XElement urlElem in kbdElem.Elements(Sil + "url"))
					{
						urls.Add(urlElem.Value);
					}
					IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboardDefinition(id, format, urls );
					ws.KnownKeyboards.Add(keyboard);
				}
			}
		}

		private void ReadIdentityElement(XElement identityElem, WritingSystemDefinition ws)
		{
			Debug.Assert(identityElem.Name == "identity");
			XElement versionElem = identityElem.Element("version");
			if (versionElem != null)
			{
				ws.VersionNumber = (string) versionElem.Attribute("number") ?? string.Empty;
				ws.VersionDescription = (string) versionElem;
			}

			XElement generationElem = identityElem.Element("generation");
			if (generationElem != null)
			{
				string dateTime = (string) generationElem.Attribute("date") ?? string.Empty;
				DateTime modified = DateTime.UtcNow;
				const string dateUninitialized = "$Date$";
				if (!string.Equals(dateTime, dateUninitialized) && (!string.IsNullOrEmpty(dateTime.Trim()) && !DateTime.TryParse(dateTime, out modified)))
				{
					//CVS format:    "$Date: 2008/06/18 22:52:35 $"
					modified = DateTime.ParseExact(dateTime, "'$Date: 'yyyy/MM/dd HH:mm:ss $", null,
						DateTimeStyles.AssumeUniversal);
				}

				ws.DateModified = modified;
			}

			string language = identityElem.GetAttributeValue("language", "type");
			string script = identityElem.GetAttributeValue("script", "type");
			string region = identityElem.GetAttributeValue("territory", "type");
			string variant = identityElem.GetAttributeValue("variant", "type");

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
			ws.Id = String.Join("-", new[] {language, script, region, variant}.Where(subtag => !String.IsNullOrEmpty(subtag)).ToArray());

			// TODO: Parse rest of special element.  Currently only handling a subset
			XElement specialElem = identityElem.Element("special");
			if (specialElem != null)
			{
				XElement silIdentityElem = specialElem.Element(Sil + "identity");
				if (silIdentityElem != null)
				{
					ws.WindowsLcid = silIdentityElem.GetAttributeValue("windowsLCID");
					ws.DefaultRegion = silIdentityElem.GetAttributeValue("defaultRegion");
					string variantName = silIdentityElem.GetAttributeValue("variantName");
					if (!string.IsNullOrEmpty(variantName) && ws.Variants.Count > 0)
						ws.Variants[0] = new VariantSubtag(ws.Variants[0], variantName);
				}
			}
		}

		private void ReadCharacterElem(XElement charactersElem, WritingSystemDefinition ws)
		{
			Debug.Assert(charactersElem.Name == "characters");

			foreach (XElement exemplarCharactersElem in charactersElem.Elements("exemplarCharacters"))
			{
				ReadExemplarCharactersElem(exemplarCharactersElem, ws);
			}

			XElement specialElem = charactersElem.Element("special");
			if (specialElem != null)
			{
				foreach (XElement exemplarCharactersElem in specialElem.Elements(Sil + "exemplarCharacters"))
				{
					ReadExemplarCharactersElem(exemplarCharactersElem, ws);
				}
			}
		}

		private void ReadExemplarCharactersElem(XElement exemplarCharactersElem, WritingSystemDefinition ws)
		{
			string type = (string) exemplarCharactersElem.Attribute("type") ?? "main";
			CharacterSetDefinition csd = new CharacterSetDefinition(type);

			var charList = UnicodeSet.ToCharacters((string) exemplarCharactersElem);
			foreach (string charItem in charList)
			{
				csd.Characters.Add(charItem);
			}
			ws.CharacterSets.Add(csd);
		}

		private void ReadDelimitersElem(XElement delimitersElem, WritingSystemDefinition ws)
		{
			Debug.Assert(delimitersElem.Name == "delimiters");

			// Currently we don't use quotationStart, quotationEnd, alternateQuotationStart, alternateQuotationEnd

			XElement specialElem = delimitersElem.Element("special");
			if (specialElem != null)
			{
				XElement matchedPairsElem = specialElem.Element(Sil + "matched-pairs");
				if (matchedPairsElem != null)
				{
					foreach (XElement matchedPairElem in matchedPairsElem.Elements(Sil + "matched-pair"))
					{
						string open = matchedPairElem.GetAttributeValue("open");
						string close = matchedPairElem.GetAttributeValue("close");
						bool paraClose = (bool?) matchedPairElem.Attribute("paraClose") ?? false;
						MatchedPair mp = new MatchedPair(open, close, paraClose);
						ws.MatchedPairs.Add(mp);
					}
				}

				XElement punctuationPatternsElem = specialElem.Element(Sil + "punctuation-patterns");
				if (punctuationPatternsElem != null)
				{
					foreach (XElement punctuationPatternElem in punctuationPatternsElem.Elements(Sil + "punctuation-pattern"))
					{
						string pattern = punctuationPatternElem.GetAttributeValue("pattern");
						PunctuationPatternContext ppc = ContextToPunctuationPatternContext[
							punctuationPatternElem.GetAttributeValue("context")];
						PunctuationPattern pp = new PunctuationPattern(pattern, ppc);
						ws.PunctuationPatterns.Add(pp);
					}
				}

				XElement quotationsElem = specialElem.Element(Sil + "quotation-marks");
				if (quotationsElem != null)
				{
					// Currently we don't use quotationContinue or alternateQuotationContinue

					ws.QuotationParagraphContinueType = QuotationToQuotationParagraphContinueTypes[
						quotationsElem.GetAttributeValue("paraContinueType")];

					foreach (XElement quotationElem in quotationsElem.Elements(Sil + "quotation"))
					{
						string open = quotationElem.GetAttributeValue("open");
						string close = quotationElem.GetAttributeValue("close");
						string cont = quotationElem.GetAttributeValue("continue");
						int level = (int?)quotationElem.Attribute("level") ?? 1;
						QuotationMarkingSystemType type;
						if (QuotationToQuotationMarkingSystemTypes.TryGetValue(quotationElem.GetAttributeValue("type"), out type))
						{
							QuotationMark qm = new QuotationMark(open, close, cont, level, type);
							ws.QuotationMarks.Add(qm);
						}
					}
				}
			}
		}

		private void ReadLayoutElement(XElement layoutElem, WritingSystemDefinition ws)
		{
			// The orientation node has two attributes, "lines" and "characters" which define direction of writing.
			// The valid values are: "top-to-bottom", "bottom-to-top", "left-to-right", and "right-to-left"
			// Currently we only handle horizontal character orders with top-to-bottom line order, so
			// any value other than characters right-to-left, we treat as our default left-to-right order.
			// This probably works for many scripts such as various East Asian scripts which traditionally
			// are top-to-bottom characters and right-to-left lines, but can also be written with
			// left-to-right characters and top-to-bottom lines.
			//Debug.Assert(layoutElem.NodeType == XmlNodeType.Element && layoutElem.Name == "layout");
			string characters = layoutElem.GetAttributeValue("orientation", "characters");
			ws.RightToLeftScript = (characters == "right-to-left");
		}

		private void ReadCollationsElement(XElement collationsElem, WritingSystemDefinition ws)
		{
			Debug.Assert(collationsElem.NodeType == XmlNodeType.Element && collationsElem.Name == "collations");
			XElement defaultCollationElem = collationsElem.Element("defaultCollation");
			string defaultCollation = (string) defaultCollationElem ?? "standard";
			foreach (XElement collationElem in collationsElem.Elements("collation"))
			{
				ReadCollationElement(collationElem, ws, defaultCollation);
			}
		}

		private void ReadCollationElement(XElement collationElem, WritingSystemDefinition ws, string defaultCollation)
		{
			Debug.Assert(collationElem != null);
			Debug.Assert(ws != null);

			string collationType = collationElem.GetAttributeValue("type");
			bool needsCompiling = (bool?) collationElem.Attribute(Sil + "needscompiling") ?? false;

			CollationDefinition cd = null;
			XElement specialElem = collationElem.Element("special");
			if ((specialElem != null) && (specialElem.HasElements))
			{
				string specialType = (specialElem.Elements().First().Name.LocalName);
				switch (specialType)
				{
					case "inherited":
						XElement inheritedElem = specialElem.Element(Sil + "inherited");
						cd = ReadCollationRulesForOtherLanguage(inheritedElem, collationType);
						break;
					case "simple":
						XElement simpleElem = specialElem.Element(Sil + "simple");
						cd = ReadCollationRulesForCustomSimple(simpleElem, collationType);
						break;
					case "reordered":
						// Skip for now
						break;
				}
			}
			else
			{
				cd = new CollationDefinition(collationType);
			}

			// Only add collation definition if it's been set
			if (cd != null)
			{
				// If ICU rules are out of sync, re-compile
				if (needsCompiling)
				{
					string errorMsg;
					cd.Validate(out errorMsg);
					// TODO: Throw exception with ErrorMsg?
				}
				else
					cd.IcuRules = LdmlCollationParser.GetIcuRulesFromCollationNode(collationElem);

				ws.Collations.Add(cd);
				if (collationType == defaultCollation)
					ws.DefaultCollation = cd;
			}
		}

		private CollationDefinition ReadCollationRulesForOtherLanguage(XElement inheritedElem, string collationType)
		{
			Debug.Assert(inheritedElem != null);
			string baseLanguageTag = inheritedElem.GetAttributeValue("base");
			string baseType = inheritedElem.GetAttributeValue("type");

			// TODO: Read referenced LDML and get collation from there
			InheritedCollationDefinition cd = new InheritedCollationDefinition(collationType);
			cd.BaseLanguageTag = baseLanguageTag;
			cd.BaseType = baseType;
			return cd;
		}

		private CollationDefinition ReadCollationRulesForCustomSimple(XElement simpleElem, string collationType)
		{
			Debug.Assert(simpleElem != null);

			SimpleCollationDefinition cd = new SimpleCollationDefinition(collationType);
			cd.SimpleRules = (string)simpleElem;
			return cd;
		}

		/// <summary>
		/// The "oldFile" parameter allows the LdmldataMapper to allow data that it doesn't understand to be roundtripped.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="ws"></param>
		/// <param name="oldFile"></param>
		public void Write(string filePath, WritingSystemDefinition ws, Stream oldFile)
		{
#if WS_FIX
			_compatibilityMode = compatibilityMode;
#endif
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
					var readerSettings = new XmlReaderSettings
					{
						NameTable = _nameSpaceManager.NameTable,
						IgnoreWhitespace = true,
						ConformanceLevel = ConformanceLevel.Auto,
						ValidationType = ValidationType.None,
						XmlResolver = null,
						DtdProcessing = DtdProcessing.Parse
					};
					reader = XmlReader.Create(oldFile, readerSettings);
				}
				using (var writer = XmlWriter.Create(filePath, CanonicalXmlSettings.CreateXmlWriterSettings()))
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

		/// <summary>
		/// The "oldFileReader" parameter allows the LdmldataMapper to allow data that it doesn't understand to be roundtripped.
		/// </summary>
		/// <param name="xmlWriter"></param>
		/// <param name="ws"></param>
		/// <param name="oldFileReader"></param>
		public void Write(XmlWriter xmlWriter, WritingSystemDefinition ws, XmlReader oldFileReader)
		{
#if WS_FIX
			_compatibilityMode = compatibilityMode;
#endif
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
					var settings = new XmlReaderSettings
					{
						NameTable = _nameSpaceManager.NameTable,
						IgnoreWhitespace = true,
						ConformanceLevel = ConformanceLevel.Auto,
						ValidationType = ValidationType.None,
						XmlResolver = null,
						DtdProcessing = DtdProcessing.Parse
					};
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

		private void WriteLdml(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
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
					bool knownNs = IsKnownSpecialElement(reader);
					reader.MoveToElement();
					if(knownNs)
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

		public void FillWithDefaults(string rfc4646, WritingSystemDefinition ws)
		{
			string id = rfc4646.ToLower();
			switch (id)
			{
				case "en-latn":
					ws.Language = "en";
					ws.LanguageName = "English";
					ws.Abbreviation = "eng";
					ws.Script = "Latn";
					break;
				 default:
					ws.Script = "Latn";
					break;
			}
		}


		protected string GetSpecialValue(XElement element, XNamespace ns, string field)
		{
			XElement child = element.Element(ns + field);
			return child == null ? string.Empty : (string) child.Attribute("value");
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

		private void WriteIdentityElement(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			writer.WriteStartElement("identity");
			writer.WriteStartElement("version");
			writer.WriteAttributeString("number", ws.VersionNumber);
			if (!string.IsNullOrEmpty(ws.VersionDescription))
				writer.WriteString(ws.VersionDescription);
			writer.WriteEndElement();
			WriteElementWithAttribute(writer, "generation", "date", String.Format("{0:s}", ws.DateModified));

#if WS_FIX
			bool copyFlexFormat = false;
			string language = String.Empty;
			string script = String.Empty;
			string territory = String.Empty;
			string variant = String.Empty;
			bool readerIsOnIdentityElement = IsReaderOnElementNodeNamed(reader, "identity");
			if (readerIsOnIdentityElement && !reader.IsEmptyElement)
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
				if (_compatibilityMode == WritingSystemCompatibility.Flex7V0Compatible)
				{
					var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
					interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag(language, script, territory, variant);
					if ((language.StartsWith("x-", StringComparison.OrdinalIgnoreCase) ||  language.Equals("x", StringComparison.OrdinalIgnoreCase))&&
						interpreter.Rfc5646Tag == ws.Bcp47Tag)
					{
						copyFlexFormat = true;
						_wsIsFlexPrivateUse = true;
					}
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
#else
			WriteLanguageTagElements(writer, ws.Bcp47Tag);
#endif
			if (IsReaderOnElementNodeNamed(reader, "identity"))
			{
				reader.Skip();
			}
			if (IsReaderOnElementNodeNamed(reader, "special"))
			{
				CopyToEndElement(writer, reader);
			}
			if (IsReaderOnEndElementNodeNamed(reader, "identity"))
			{
				reader.Read();
			}
			writer.WriteEndElement();
		}

		private void WriteLanguageTagElements(XmlWriter writer, string languageTag)
		{
			string language, script, region, variant;
			IetfLanguageTag.GetCodes(languageTag, out language, out script, out region, out variant);

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

		private void WriteLayoutElement(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
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
		protected virtual void WriteTopLevelSpecialElements(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			// Note. As per appendix L2 'Canonical Form' of the LDML specification elements are ordered alphabetically.
			WriteBeginSpecialElement(writer, "palaso");
			WriteFlexOrPalasoConformElement(writer, reader, "palaso", "abbreviation", ws.Abbreviation);
#if WS_FIX
			WriteSpecialValue(writer, "palaso", "defaultFontFamily", ws.DefaultFontName);
			if (ws.DefaultFontSize != 0)
			{
				WriteSpecialValue(writer, "palaso", "defaultFontSize", ws.DefaultFontSize.ToString());
			}
#endif
			WriteSpecialValue(writer, "palaso", "defaultKeyboard", ws.Keyboard);
			if (!ws.IsUnicodeEncoded)
			{
				WriteSpecialValue(writer, "palaso", "isLegacyEncoded", (!ws.IsUnicodeEncoded).ToString());
			}
			WriteFlexOrPalasoConformElement(writer, reader, "palaso", "languageName", ws.LanguageName);
#if WS_FIX
			if (!String.IsNullOrEmpty(ws.SpellCheckingId))
			{
				WriteSpecialValue(writer, "palaso", "spellCheckingId", ws.SpellCheckingId);
			}
#endif
			WriteFlexOrPalasoConformElement(writer, reader, "palaso", "version", WritingSystemDefinition.LatestWritingSystemDefinitionVersion.ToString());
			writer.WriteEndElement();

#if WS_FIX
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
					writer.WriteEndElement(); // Keyboard
				}
				writer.WriteEndElement(); // KnownKeyboards
				WriteFlexOrPalasoConformElement(writer, reader, Palaso2NamespaceName, "version",
					WritingSystemDefinition.LatestWritingSystemDefinitionVersion.ToString());
				writer.WriteEndElement(); // Special
			}
#endif
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
#if WS_FIX
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

			if (ws.CollationRulesType == CollationRulesTypes.DefaultOrdering && !needToCopy)
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

			if (ws.CollationRulesType != CollationRulesTypes.DefaultOrdering)
			{
				writer.WriteStartElement("collation");
				switch (ws.CollationRulesType)
				{
					case CollationRulesTypes.OtherLanguage:
						WriteCollationRulesFromOtherLanguage(writer, reader, ws);
						break;
					case CollationRulesTypes.CustomSimple:
						WriteCollationRulesFromCustomSimple(writer, reader, ws);
						break;
					case CollationRulesTypes.CustomIcu:
						WriteCollationRulesFromCustomICU(writer, reader, ws);
						break;
					default:
						string message = string.Format("Unhandled SortRulesType '{0}' while writing LDML definition file.", ws.CollationRulesType);
						throw new ApplicationException(message);
				}
				WriteBeginSpecialElement(writer, "palaso");
				WriteSpecialValue(writer, "palaso", "sortRulesType", ws.CollationRulesType.ToString());
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
							bool knownNs = IsKnownSpecialElement(reader);
							reader.MoveToElement();
							if (knownNs)
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
#endif
		}

#if WS_FIX
		private void WriteCollationRulesFromOtherLanguage(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			Debug.Assert(ws.CollationRulesType == CollationRulesTypes.OtherLanguage);

			// Since the alias element gets all information from another source,
			// we should remove all other elements in this collation element.  We
			// leave "special" elements as they are custom data from some other app.
			writer.WriteStartElement("base");
			WriteElementWithAttribute(writer, "alias", "source", ws.CollationRules);
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
			Debug.Assert(ws.CollationRulesType == CollationRulesTypes.CustomSimple);

			string message;
			// avoid throwing exception, just don't save invalid data
			if (!SimpleRulesCollator.ValidateSimpleRules(ws.CollationRules ?? string.Empty, out message))
			{
				return;
			}
			string icu = SimpleRulesCollator.ConvertToIcuRules(ws.CollationRules ?? string.Empty);
			WriteCollationRulesFromICUString(writer, reader, icu);
		}

		private void WriteCollationRulesFromCustomICU(XmlWriter writer, XmlReader reader, WritingSystemDefinition ws)
		{
			Debug.Assert(writer != null);
			Debug.Assert(ws != null);
			Debug.Assert(ws.CollationRulesType == CollationRulesTypes.CustomIcu);
			WriteCollationRulesFromICUString(writer, reader, ws.CollationRules);
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
#endif
	}
}