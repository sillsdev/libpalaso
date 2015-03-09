using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Icu;
using SIL.Extensions;
using SIL.Keyboarding;
using SIL.Xml;

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
		private static readonly XNamespace Palaso = "urn://palaso.org/ldmlExtensions/v1";
		private static readonly XNamespace Sil = "urn://www.sil.org/ldml/0.1";
		private const string IdentifierDelimiter = "\ufdd0";

		/// <summary>
		/// Mapping of font engine attribute to FontEngines enumeration.
		/// </summary>
		private static readonly Dictionary<string, FontEngines> EngineToFontEngines = new Dictionary<string, FontEngines>
		{
			{"ot", FontEngines.OpenType},
			{"gr", FontEngines.Graphite}
		};

		/// <summary>
		/// Mapping of FontEngines enumeration to font engine attribute.
		/// If the engine is none, leave an empty string
		/// </summary>
		private static readonly Dictionary<FontEngines, string> FontEnginesToEngine = new Dictionary<FontEngines, string>
		{
			{FontEngines.None, string.Empty},
			{FontEngines.OpenType, "ot"},
			{FontEngines.Graphite, "gr"}
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
		/// Mapping of FontRoles enumeration to font role/type attribute
		/// </summary>
		private static readonly Dictionary<FontRoles, string> FontRolesToRole = new Dictionary<FontRoles, string> 
		{
			{FontRoles.Default, "default"},
			{FontRoles.Heading, "heading"},
			{FontRoles.Emphasis, "emphasis"}
		};

		/// <summary>
		/// Mapping of spell checking type attribute to SpellCheckDictionaryFormat enumeration
		/// </summary>
		private static readonly Dictionary<string, SpellCheckDictionaryFormat> SpellCheckToSpecllCheckDictionaryFormats = new Dictionary
			<string, SpellCheckDictionaryFormat>
		{
			{"hunspell", SpellCheckDictionaryFormat.Hunspell},
			{"wordlist", SpellCheckDictionaryFormat.Wordlist},
			{"lift", SpellCheckDictionaryFormat.Lift}
		};

		/// <summary>
		/// Mapping of SpellCheckDictionaryFormat enumeration to spell checking type attribute
		/// </summary>
		private static readonly Dictionary<SpellCheckDictionaryFormat, string> SpellCheckDictionaryFormatsToSpellCheck = new Dictionary
			<SpellCheckDictionaryFormat, string>
		{
			{SpellCheckDictionaryFormat.Hunspell, "hunspell"},
			{SpellCheckDictionaryFormat.Wordlist, "wordlist"},
			{SpellCheckDictionaryFormat.Lift, "lift"}
		};

		/// <summary>
		/// Mapping of keyboard type attribute to KeyboardFormat enumeration
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
		/// Mapping of KeyboardFormat enumeration to keyboard type attribute
		/// </summary>
		private static readonly Dictionary<KeyboardFormat, string> KeyboardFormatToKeyboard = new Dictionary<KeyboardFormat, string>
		{
			{KeyboardFormat.Unknown, string.Empty},
			{KeyboardFormat.Keyman, "kmn"},
			{KeyboardFormat.CompiledKeyman, "kmx"},
			{KeyboardFormat.Msklc, "msklc"},
			{KeyboardFormat.Ldml, "ldml"},
			{KeyboardFormat.Keylayout, "keylayout"}
		}; 

		/// <summary>
		/// Mapping of context attribute to PunctuationPatternContext enumeration
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
		/// Mapping of PunctuationPatternContext enumeration to context attribute
		/// </summary>
		private static readonly Dictionary<PunctuationPatternContext, string> PunctuationPatternContextToContext = new Dictionary<PunctuationPatternContext, string>
		{
			{PunctuationPatternContext.Initial, "init"},
			{PunctuationPatternContext.Medial, "medial"},
			{PunctuationPatternContext.Final, "final"},
			{PunctuationPatternContext.Break, "break"},
			{PunctuationPatternContext.Isolate, "isolate"}
		};

		/// <summary>
		/// Mapping of paraContinueType attribute to QuotationParagraphContinueType enumeration
		/// </summary>
		private static readonly Dictionary<string, QuotationParagraphContinueType> QuotationToQuotationParagraphContinueTypes = new Dictionary<string, QuotationParagraphContinueType>
		{
			{string.Empty, QuotationParagraphContinueType.None},
			{"all", QuotationParagraphContinueType.All},
			{"outer", QuotationParagraphContinueType.Outermost},
			{"inner", QuotationParagraphContinueType.Innermost}
		};

		/// <summary>
		/// Mapping of QuotationParagraphContinueType enumeration to paraContinueType attribute
		/// </summary>
		private static readonly Dictionary<QuotationParagraphContinueType, string> QuotationParagraphContinueTypesToQuotation = new Dictionary<QuotationParagraphContinueType, string>
		{
			{QuotationParagraphContinueType.None, string.Empty},
			{QuotationParagraphContinueType.All, "all"},
			{QuotationParagraphContinueType.Outermost, "outer"},
			{QuotationParagraphContinueType.Innermost, "inner"}
		};

		/// <summary>
		/// Mapping of quotation marking system attribute to QuotationMarkingSystemType enumeration
		/// </summary>
		private static readonly Dictionary<string, QuotationMarkingSystemType>QuotationToQuotationMarkingSystemTypes = new Dictionary<string, QuotationMarkingSystemType>
		{
			{string.Empty, QuotationMarkingSystemType.Normal},
			{"narrative", QuotationMarkingSystemType.Narrative}
		};

		/// <summary>
		/// Mapping of QuotationMarkingSystemType enumeration to quotation marking system attribute
		/// </summary>
		private static readonly Dictionary<QuotationMarkingSystemType, string> QuotationMarkingSystemTypesToQuotation = new Dictionary<QuotationMarkingSystemType, string>
		{
			{QuotationMarkingSystemType.Normal, string.Empty},
			{QuotationMarkingSystemType.Narrative, "narrative"}
		};

		private readonly IWritingSystemFactory _writingSystemFactory;

		public LdmlDataMapper(IWritingSystemFactory writingSystemFactory)
		{
			_writingSystemFactory = writingSystemFactory;
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

		public static void WriteLdmlText(XmlWriter writer, string text)
		{
			// Not all Unicode characters are valid in an XML document, so we need to create
			// the <cp hex="X"> elements to replace the invalid characters.
			// Note: While 0xD (carriage return) is a valid XML character, it is automatically
			// either dropped or converted to 0xA by any conforming XML parser, so we also make a <cp>
			// element for that one.
			var sb = new StringBuilder(text.Length);
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
				throw new ApplicationException("Unable to load writing system definition: Missing <ldml> tag.");

			XElement identityElem = element.Element("identity");
			if (identityElem != null)
				ReadIdentityElement(identityElem, ws);

			// Check for the proper LDML version after reading identity element so that we have the proper language tag if an error occurs
			foreach (XElement specialElem in element.Elements("special"))
				CheckVersion(specialElem, ws);

			XElement charactersElem = element.Element("characters");
			if (charactersElem != null)
				ReadCharactersElement(charactersElem, ws);

			XElement delimitersElem = element.Element("delimiters");
			if (delimitersElem != null)
				ReadDelimitersElement(delimitersElem, ws);

			XElement layoutElem = element.Element("layout");
			if (layoutElem != null)
				ReadLayoutElement(layoutElem, ws);

			XElement numbersElem = element.Element("numbers");
			if (numbersElem != null)
				ReadNumbersElement(numbersElem, ws);

			XElement collationsElem = element.Element("collations");
			if (collationsElem != null)
				ReadCollationsElement(collationsElem, ws);

			foreach (XElement specialElem in element.Elements("special"))
				ReadTopLevelSpecialElement(specialElem, ws);

			ws.Id = string.Empty;
			ws.AcceptChanges();
		}

		private void CheckVersion(XElement specialElem, WritingSystemDefinition ws)
		{
			// Flag invalid versions (0-2 inclusive) from reading legacy LDML files
			// We're intentionally not using WritingSystemLDmlVersionGetter and the
			// cheeck for Flex7V0Compatible because the migrator will have handled that.
			if (!string.IsNullOrEmpty((string)specialElem.Attribute(XNamespace.Xmlns + "fw")) ||
				!string.IsNullOrEmpty((string)specialElem.Attribute(XNamespace.Xmlns + "palaso")))
			{
				string version = "0";
				// Palaso namespace
				XElement versionElem = specialElem.Element(Palaso + "version");
				if (versionElem != null)
				{
					version = (string)versionElem.Attribute("value");
					version = string.IsNullOrEmpty(version) ? "0" : version;
				}
				throw new ApplicationException(String.Format(
					"The LDML tag '{0}' is version {1}.  Version {2} was expected.",
					ws.IetfLanguageTag,
					version,
					WritingSystemDefinition.LatestWritingSystemDefinitionVersion
					));
			}
		}

		private void ReadTopLevelSpecialElement(XElement specialElem, WritingSystemDefinition ws)
		{
			XElement externalResourcesElem = specialElem.Element(Sil + "external-resources");
			if (externalResourcesElem != null)
			{
				ReadFontElement(externalResourcesElem, ws);
				ReadSpellcheckElement(externalResourcesElem, ws);
				ReadKeyboardElement(externalResourcesElem, ws);
			}
		}

		private void ReadFontElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			foreach (XElement fontElem in externalResourcesElem.Elements(Sil + "font"))
			{
				var fontName = (string) fontElem.Attribute("name");
				if (!string.IsNullOrEmpty(fontName))
				{
					var fd = new FontDefinition(fontName);

					// Types (space separate list)
					var roles = (string) fontElem.Attribute("types");
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

					// Relative Size
					fd.RelativeSize = (float?) fontElem.Attribute("size") ?? 1.0f;

					// Minversion
					fd.MinVersion = (string) fontElem.Attribute("minversion");

					// Features (space separated list of key=value pairs)
					fd.Features = (string) fontElem.Attribute("features");

					// Language
					fd.Language = (string) fontElem.Attribute("lang");

					// OpenType language
					fd.OpenTypeLanguage = (string) fontElem.Attribute("otlang");

					// Font Engine (space separated list) supercedes legacy isGraphite flag
					// If attribute is missing it is assumed to be "gr ot"
					var engines = (string) fontElem.Attribute("engines") ?? "gr ot";
					IEnumerable<string> engineList = engines.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
					foreach (string engineEntry in engineList)
						fd.Engines |= (EngineToFontEngines[engineEntry]);

					// Subset
					fd.Subset = (string) fontElem.Attribute("subset");

					// URL elements
					foreach (XElement urlElem in fontElem.Elements(Sil + "url"))
						fd.Urls.Add(urlElem.Value);

					ws.Fonts.Add(fd);
				}
			}
		}

		private void ReadSpellcheckElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			foreach (XElement scElem in externalResourcesElem.Elements(Sil + "spellcheck"))
			{
				var type = (string) scElem.Attribute("type");
				if (!string.IsNullOrEmpty(type))
				{
					var scd = new SpellCheckDictionaryDefinition(SpellCheckToSpecllCheckDictionaryFormats[type]);

					// URL elements
					foreach (XElement urlElem in scElem.Elements(Sil + "url"))
						scd.Urls.Add(urlElem.Value);
					ws.SpellCheckDictionaries.Add(scd);
				}
			}
		}

		private void ReadKeyboardElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			foreach (XElement kbdElem in externalResourcesElem.Elements(Sil + "kbd"))
			{
				var id = (string) kbdElem.Attribute("id");
				if (!string.IsNullOrEmpty(id))
				{
					KeyboardFormat format = KeyboardToKeyboardFormat[(string) kbdElem.Attribute("type")];
					IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboard(id, format, kbdElem.Elements(Sil + "url").Select(u => (string) u));
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
				// Previous versions of LDML Data Mapper allowed generation date to be in CVS format
				// This is deprecated so we only handle ISO 8601 format
				if (DateTimeExtensions.IsISO8601DateTime(dateTime))
					modified = DateTimeExtensions.ParseISO8601DateTime(dateTime);

				ws.DateModified = modified;
			}

			string language = identityElem.GetAttributeValue("language", "type");
			string script = identityElem.GetAttributeValue("script", "type");
			string region = identityElem.GetAttributeValue("territory", "type");
			string variant = identityElem.GetAttributeValue("variant", "type");

			if (!string.IsNullOrEmpty(language) && (language.StartsWith("x-", StringComparison.OrdinalIgnoreCase) || language.Equals("x", StringComparison.OrdinalIgnoreCase)))
			{
				var flexRfcTagInterpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
				flexRfcTagInterpreter.ConvertToPalasoConformPrivateUseRfc5646Tag(language, script, region, variant);
				ws.IetfLanguageTag = IetfLanguageTagHelper.ToIetfLanguageTag(flexRfcTagInterpreter.Language, flexRfcTagInterpreter.Script, flexRfcTagInterpreter.Region, flexRfcTagInterpreter.Variant);
			}
			else
			{
				ws.IetfLanguageTag = IetfLanguageTagHelper.ToIetfLanguageTag(language, script, region, variant);
			}

			// TODO: Parse rest of special element.  Currently only handling a subset
			XElement specialElem = identityElem.Element("special");
			if (specialElem != null)
			{
				XElement silIdentityElem = specialElem.Element(Sil + "identity");
				if (silIdentityElem != null)
				{
					ws.WindowsLcid = (string) silIdentityElem.Attribute("windowsLCID");
					ws.DefaultRegion = (string) silIdentityElem.Attribute("defaultRegion");
				}
			}
		}

		private void ReadCharactersElement(XElement charactersElem, WritingSystemDefinition ws)
		{
			foreach (XElement exemplarCharactersElem in charactersElem.Elements("exemplarCharacters"))
				ReadExemplarCharactersElem(exemplarCharactersElem, ws);

			XElement specialElem = charactersElem.Element("special");
			if (specialElem != null)
			{
				foreach (XElement exemplarCharactersElem in specialElem.Elements(Sil + "exemplarCharacters"))
				{
					// Sil:exemplarCharacters are required to have a type
					if (!string.IsNullOrEmpty((string) exemplarCharactersElem.Attribute("type")))
						ReadExemplarCharactersElem(exemplarCharactersElem, ws);
				}
			}
		}

		private void ReadExemplarCharactersElem(XElement exemplarCharactersElem, WritingSystemDefinition ws)
		{
			string type = ReadIdentifierAttribute(exemplarCharactersElem, "type", "main");
			var csd = new CharacterSetDefinition(type);
			csd.Characters.UnionWith(UnicodeSet.ToCharacters((string) exemplarCharactersElem));
			ws.CharacterSets.Add(csd);
		}

		private void ReadDelimitersElement(XElement delimitersElem, WritingSystemDefinition ws)
		{
			string open, close;
			string level1Continue = null;
			string level2Continue = null;

			// A bit strange, but we need to read the special element first to get everything we need to write 
			// level 1 and 2. So we just store everything but 1 and 2 in a list and add them after we add 1 and 2.
			var specialQuotationMarks = new List<QuotationMark>();

			XElement specialElem = delimitersElem.Element("special");
			if (specialElem != null)
			{
				XElement matchedPairsElem = specialElem.Element(Sil + "matched-pairs");
				if (matchedPairsElem != null)
				{
					foreach (XElement matchedPairElem in matchedPairsElem.Elements(Sil + "matched-pair"))
					{
						open = (string) matchedPairElem.Attribute("open");
						close = (string) matchedPairElem.Attribute("close");
						bool paraClose = (bool?) matchedPairElem.Attribute("paraClose") ?? false;
						var mp = new MatchedPair(open, close, paraClose);
						ws.MatchedPairs.Add(mp);
					}
				}

				XElement punctuationPatternsElem = specialElem.Element(Sil + "punctuation-patterns");
				if (punctuationPatternsElem != null)
				{
					foreach (XElement punctuationPatternElem in punctuationPatternsElem.Elements(Sil + "punctuation-pattern"))
					{
						var pattern = (string) punctuationPatternElem.Attribute("pattern");
						PunctuationPatternContext ppc = ContextToPunctuationPatternContext[(string) punctuationPatternElem.Attribute("context")];
						var pp = new PunctuationPattern(pattern, ppc);
						ws.PunctuationPatterns.Add(pp);
					}
				}

				XElement quotationsElem = specialElem.Element(Sil + "quotation-marks");
				if (quotationsElem != null)
				{
					string paraContinueType = (string)quotationsElem.Attribute("paraContinueType") ?? string.Empty;
					ws.QuotationParagraphContinueType = QuotationToQuotationParagraphContinueTypes[paraContinueType];

					level1Continue = (string)quotationsElem.Element(Sil + "quotationContinue");
					level2Continue = (string)quotationsElem.Element(Sil + "alternateQuotationContinue");

					foreach (XElement quotationElem in quotationsElem.Elements(Sil + "quotation"))
					{
						open = (string) quotationElem.Attribute("open");
						close = (string) quotationElem.Attribute("close");
						var cont = (string) quotationElem.Attribute("continue");
						int level = (int?) quotationElem.Attribute("level") ?? 1;
						var type = (string) quotationElem.Attribute("type");
						QuotationMarkingSystemType qmType = !string.IsNullOrEmpty(type) ? QuotationToQuotationMarkingSystemTypes[type] : QuotationMarkingSystemType.Normal;
						
						var qm = new QuotationMark(open, close, cont, level, qmType);
						specialQuotationMarks.Add(qm);
					}
				}
			}

			// level 1: quotationStart, quotationEnd
			open = (string)delimitersElem.Element("quotationStart");
			close = (string)delimitersElem.Element("quotationEnd");
			if (!string.IsNullOrEmpty(open) || !string.IsNullOrEmpty(close) || !string.IsNullOrEmpty(level1Continue))
			{
				var qm = new QuotationMark(open, close, level1Continue, 1, QuotationMarkingSystemType.Normal);
				ws.QuotationMarks.Add(qm);
			}

			// level 2: alternateQuotationStart, alternateQuotationEnd
			open = (string)delimitersElem.Element("alternateQuotationStart");
			close = (string)delimitersElem.Element("alternateQuotationEnd");
			if (!string.IsNullOrEmpty(open) || !string.IsNullOrEmpty(close) || !string.IsNullOrEmpty(level2Continue))
			{
				var qm = new QuotationMark(open, close, level2Continue, 2, QuotationMarkingSystemType.Normal);
				ws.QuotationMarks.Add(qm);
			}

			ws.QuotationMarks.AddRange(specialQuotationMarks);
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
			XElement orientationElem = layoutElem.Element("orientation");
			if (orientationElem != null)
			{
				XElement characterOrderElem = orientationElem.Element("characterOrder");
				if (characterOrderElem != null)
					ws.RightToLeftScript = ((string) characterOrderElem == "right-to-left");
			}
		}

		// Numbering system gets added to the character set definition
		private void ReadNumbersElement(XElement numbersElem, WritingSystemDefinition ws)
		{
			XElement defaultNumberingSystemElem = numbersElem.Element("defaultNumberingSystem");
			if (defaultNumberingSystemElem != null)
			{
				var id = (string) defaultNumberingSystemElem;
				XElement numberingSystemsElem = numbersElem.Elements("numberingSystem")
					.FirstOrDefault(e => id == (string) e.Attribute("id") && (string) e.Attribute("type") == "numeric" && e.Attribute("alt") == null);
				if (numberingSystemsElem != null)
				{
					var csd = new CharacterSetDefinition("numeric");
					// Only handle numeric types
					var digits = (string) numberingSystemsElem.Attribute("digits");
					foreach (char charItem in digits)
						csd.Characters.Add(charItem.ToString(CultureInfo.InvariantCulture));
					ws.CharacterSets.Add(csd);
				}
			}
		}

		private void ReadCollationsElement(XElement collationsElem, WritingSystemDefinition ws)
		{
			ws.Collations.Clear();
			XElement defaultCollationElem = collationsElem.Element("defaultCollation");
			string defaultCollation = (string) defaultCollationElem ?? "standard";
			foreach (XElement collationElem in collationsElem.Elements("collation"))
				ReadCollationElement(collationElem, ws, defaultCollation);
		}

		private void ReadCollationElement(XElement collationElem, WritingSystemDefinition ws, string defaultCollation)
		{
			string collationType = ReadIdentifierAttribute(collationElem, "type");
			if (!string.IsNullOrEmpty(collationType))
			{
				CollationDefinition cd = null;
				XElement specialElem = collationElem.Element("special");
				if ((specialElem != null) && (specialElem.HasElements))
				{
					string specialType = (specialElem.Elements().First().Name.LocalName);
					switch (specialType)
					{
						case "simple":
							cd = ReadCollationRulesForCustomSimple(collationElem, specialElem, collationType);
							break;
						case "reordered":
							// Skip for now
							break;
					}
				}
				else
				{
					cd = ReadCollationRulesForCustomIcu(collationElem, collationType);
				}

				// Only add collation definition if it's been set
				if (cd != null)
				{
					ws.Collations.Add(cd);
					if (collationType == defaultCollation)
						ws.DefaultCollation = cd;
				}
			}
		}

		private string ReadIdentifierAttribute(XElement elem, string attributeName, string defaultId = null)
		{
			var identifier = (string) elem.Attribute(attributeName) ?? defaultId;
			var alt = (string) elem.Attribute("alt");
			if (!string.IsNullOrEmpty(alt))
				return identifier + IdentifierDelimiter + alt;
			return identifier;
		}

		private SimpleCollationDefinition ReadCollationRulesForCustomSimple(XElement collationElem, XElement specialElem, string collationType)
		{
			XElement simpleElem = specialElem.Element(Sil + "simple");
			bool needsCompiling = (bool?) specialElem.Attribute(Sil + "needscompiling") ?? false;
			var scd = new SimpleCollationDefinition(collationType) {SimpleRules = ((string) simpleElem).Replace("\n", "\r\n")};
			if (needsCompiling)
			{
				string errorMsg;
				scd.Validate(out errorMsg);
			}
			else
			{
				scd.CollationRules = LdmlCollationParser.GetIcuRulesFromCollationNode(collationElem);
				scd.IsValid = true;
			}
			return scd;
		}

		private IcuCollationDefinition ReadCollationRulesForCustomIcu(XElement collationElem, string collationType)
		{
			var icd = new IcuCollationDefinition(collationType) {WritingSystemFactory = _writingSystemFactory};
			icd.Imports.AddRange(collationElem.Elements("import").Select(ie => new IcuCollationImport((string) ie.Attribute("source"), (string) ie.Attribute("type"))));
			icd.IcuRules = LdmlCollationParser.GetIcuRulesFromCollationNode(collationElem);
			string errorMsg;
			icd.Validate(out errorMsg);
			return icd;
		}

		/// <summary>
		/// Utility to get or create the special element with SIL namespace
		/// </summary>
		/// <param name="element">parent element of the special element</param>
		/// <returns></returns>
		private XElement GetOrCreateSpecialElement(XElement element)
		{
			// Create element
			XElement specialElem = element.GetOrCreateElement("special");
			specialElem.SetAttributeValue(XNamespace.Xmlns + "sil", Sil);
			return specialElem;
		}

		/// <summary>
		/// Utility to remove empty elements. Since isEmpty is true for <element /> 
		/// but false for <element></element>, we have to check both cases
		/// </summary>
		/// <param name="element">XElement to remove if it's empty or has 0 contents/attributes/elements</param>
		private void RemoveIfEmpty(XElement element)
		{
			if (element != null)
			{
				if (element.IsEmpty || (string.IsNullOrEmpty((string)element) && !element.HasElements && !element.HasAttributes))
					element.Remove();
			}
		}
		
		/// <summary>
		/// The "oldFile" parameter allows the LdmldataMapper to allow data that it doesn't understand to be roundtripped.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="ws"></param>
		/// <param name="oldFile"></param>
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
			// We don't want to run any risk of persisting an invalid writing system in an LDML.
			ws.RequiresValidLanguageTag = true;
			XmlReader reader = null;
			try
			{
				XElement element;
				if (oldFile != null)
				{
					var readerSettings = new XmlReaderSettings
					{
						IgnoreWhitespace = true,
						ConformanceLevel = ConformanceLevel.Auto,
						ValidationType = ValidationType.None,
						XmlResolver = null,
						DtdProcessing = DtdProcessing.Parse
					};
					reader = XmlReader.Create(oldFile, readerSettings);
					element = XElement.Load(reader);
				}
				else
				{
					element = new XElement("ldml");
				}
				// Use Canonical xml settings suitable for use in Chorus applications
				// except NewLineOnAttributes to conform to SLDR files
				var writerSettings = CanonicalXmlSettings.CreateXmlWriterSettings();
				writerSettings.NewLineOnAttributes = false;
				using (var writer = XmlWriter.Create(filePath, writerSettings))
				{
					WriteLdml(writer, element, ws);
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
			if (xmlWriter == null)
			{
				throw new ArgumentNullException("xmlWriter");
			}
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			// We don't want to run any risk of persisting an invalid writing system in an LDML.
			ws.RequiresValidLanguageTag = true;
			XElement element = oldFileReader != null ? XElement.Load(oldFileReader) : new XElement("ldml");
			WriteLdml(xmlWriter, element, ws);
		}

		/// <summary>
		/// Update element based on the writing system model.  At the end, write the contents to LDML
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		/// <param name="ws"></param>
		private void WriteLdml(XmlWriter writer, XElement element, WritingSystemDefinition ws)
		{
			Debug.Assert(element != null);
			Debug.Assert(ws != null);

			XElement identityElem = element.GetOrCreateElement("identity");
			WriteIdentityElement(identityElem, ws);
			RemoveIfEmpty(identityElem);

			XElement charactersElem = element.GetOrCreateElement("characters");
			WriteCharactersElement(charactersElem, ws);
			RemoveIfEmpty(charactersElem);

			XElement delimitersElem = element.GetOrCreateElement("delimiters");
			WriteDelimitersElement(delimitersElem, ws);
			RemoveIfEmpty(delimitersElem);

			XElement layoutElem = element.GetOrCreateElement("layout");
			WriteLayoutElement(layoutElem, ws);
			RemoveIfEmpty(layoutElem);

			XElement numbersElem = element.GetOrCreateElement("numbers");
			WriteNumbersElement(numbersElem, ws);
			RemoveIfEmpty(numbersElem);

			XElement collationsElem = element.GetOrCreateElement("collations");
			WriteCollationsElement(collationsElem, ws);
			RemoveIfEmpty(collationsElem);

			// Can have multiple specials.  Find the one with SIL namespace and external-resources.
			// Also handle case where we create special because writingsystem has entries to write
			XElement specialElem = element.Elements("special").FirstOrDefault(
				e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns+"sil")) && e.Element(Sil + "external-resources") != null);
			if (specialElem == null && (ws.Fonts.Count > 0 || ws.KnownKeyboards.Count > 0 || ws.SpellCheckDictionaries.Count > 0))
			{
				// Create special element
				specialElem = GetOrCreateSpecialElement(element);
			}
			if (specialElem != null)
			{
				WriteTopLevelSpecialElements(specialElem, ws);
				RemoveIfEmpty(specialElem);
			}

			element.WriteTo(writer);
		}

		private void WriteIdentityElement(XElement identityElem, WritingSystemDefinition ws)
		{
			Debug.Assert(identityElem != null);
			Debug.Assert(ws != null);

			// Remove non-special elements to repopulate later
			// Preserve special because we don't recreate all its contents
			identityElem.Elements().Where(e => e.Name != "special").Remove();

			// Version is required.  If VersionNumber is blank, the empty attribute is still written
			XElement versionElem = identityElem.GetOrCreateElement("version");
			versionElem.SetAttributeValue("number", ws.VersionNumber);
			if (!string.IsNullOrEmpty(ws.VersionDescription))
				versionElem.SetValue(ws.VersionDescription);

			// Write generation date with UTC so no more ambiguity on timezone
			identityElem.SetAttributeValue("generation", "date", ws.DateModified.ToISO8601TimeFormatWithUTCString());
			WriteLanguageTagElements(identityElem, ws.IetfLanguageTag);

			// Create special element if data needs to be written
			if (!string.IsNullOrEmpty(ws.WindowsLcid) || !string.IsNullOrEmpty(ws.DefaultRegion) || (ws.Variants.Count > 0))
			{
				XElement specialElem = GetOrCreateSpecialElement(identityElem);
				XElement silIdentityElem = specialElem.GetOrCreateElement(Sil + "identity");

				// TODO: how do we recover uid attribute?

				silIdentityElem.SetOptionalAttributeValue("windowsLCID", ws.WindowsLcid);
				silIdentityElem.SetOptionalAttributeValue("defaultRegion", ws.DefaultRegion);
				// TODO: For now, use the first variant as the variantName
				if (ws.Variants.Count > 0)
					silIdentityElem.SetOptionalAttributeValue("variantName", ws.Variants.First().Name);
					
				// Move special to the end of the identity block (preserving order)
				specialElem.Remove();
				identityElem.Add(specialElem);
			}
		}

		private void WriteLanguageTagElements(XElement identityElem, string languageTag) 
		{
			string language, script, region, variant;
			IetfLanguageTagHelper.GetParts(languageTag, out language, out script, out region, out variant);
			
			// language element is required
			identityElem.SetAttributeValue("language", "type", language);
			// write the rest if they have contents
			if (!string.IsNullOrEmpty(script))
				identityElem.SetAttributeValue("script", "type", script);
			if (!string.IsNullOrEmpty(region))
				identityElem.SetAttributeValue("territory", "type", region);
			if (!string.IsNullOrEmpty(variant))
				identityElem.SetAttributeValue("variant", "type", variant);
		}
				
		private void WriteCharactersElement(XElement charactersElem, WritingSystemDefinition ws)
		{
			Debug.Assert(charactersElem != null);
			Debug.Assert(ws != null);

			// Remove exemplarCharacters and special sil:exemplarCharacters elements to repopulate later
			charactersElem.Elements("exemplarCharacters").Remove();
			XElement specialElem = charactersElem.Element("special");
			if (specialElem != null)
			{
				specialElem.Elements(Sil + "exemplarCharacters").Remove();
				RemoveIfEmpty(specialElem);
			}

			foreach (CharacterSetDefinition csd in ws.CharacterSets)
			{
				string type, alt;
				ParseIdentifier(csd.Type, out type, out alt);
				XElement exemplarCharactersElem;
				switch (type)
				{
					// These character sets go to the normal LDML exemplarCharacters space
					// http://unicode.org/reports/tr35/tr35-general.html#Exemplars
					case "main":
					case "auxiliary":
					case "index":
					case "punctuation":
						exemplarCharactersElem = new XElement("exemplarCharacters", UnicodeSet.ToPattern(csd.Characters));
						// Assume main set doesn't have an attribute type
						if (type != "main")
							exemplarCharactersElem.SetAttributeValue("type", type);
						exemplarCharactersElem.SetAttributeValue("alt", alt);
						charactersElem.Add(exemplarCharactersElem);
						break;
					// Numeric characters will be written in the numbers element
					case "numeric":
						break;
					// All others go to special Sil:exemplarCharacters
					default :
						exemplarCharactersElem = new XElement(Sil + "exemplarCharacters", UnicodeSet.ToPattern(csd.Characters));
						exemplarCharactersElem.SetAttributeValue("type", type);
						exemplarCharactersElem.SetAttributeValue("alt", alt);
						specialElem = GetOrCreateSpecialElement(charactersElem);
						specialElem.Add(exemplarCharactersElem);
						break;
				}
			}
		}

		private void WriteDelimitersElement(XElement delimitersElem, WritingSystemDefinition ws)
		{
			Debug.Assert(delimitersElem != null);
			Debug.Assert(ws != null);

			// Remove non-special elements to repopulate later
			delimitersElem.Elements().Where(e => e.Name != "special").Remove();

			// Level 1 normal => quotationStart and quotationEnd
			QuotationMark qm1 = ws.QuotationMarks.FirstOrDefault(q => q.Level == 1 && q.Type == QuotationMarkingSystemType.Normal);
			if (qm1 != null)
			{
				var quotationStartElem = new XElement("quotationStart", qm1.Open);
				var quotationEndElem = new XElement("quotationEnd", qm1.Close);
				delimitersElem.Add(quotationStartElem);
				delimitersElem.Add(quotationEndElem);
			}
			// Level 2 normal => alternateQuotationStart and alternateQuotationEnd
			QuotationMark qm2 = ws.QuotationMarks.FirstOrDefault(q => q.Level == 2 && q.Type == QuotationMarkingSystemType.Normal);
			if (qm2 != null)
			{
				var alternateQuotationStartElem = new XElement("alternateQuotationStart", qm2.Open);
				var alternateQuotationEndElem = new XElement("alternateQuotationEnd", qm2.Close);
				delimitersElem.Add(alternateQuotationStartElem);
				delimitersElem.Add(alternateQuotationEndElem);
			}

			// Remove special Sil:matched-pairs elements to repopulate later
			XElement specialElem = delimitersElem.Element("special");
			XElement matchedPairsElem;
			if (specialElem != null)
			{
				matchedPairsElem = specialElem.Element(Sil + "matched-pairs");
				if (matchedPairsElem != null)
				{
					matchedPairsElem.Elements(Sil + "matched-pair").Remove();
					RemoveIfEmpty(matchedPairsElem);
				}
				RemoveIfEmpty(specialElem);
			}
			foreach (var mp in ws.MatchedPairs)
			{
				var matchedPairElem = new XElement(Sil + "matched-pair");
				// open and close are required
				matchedPairElem.SetAttributeValue("open", mp.Open);
				matchedPairElem.SetAttributeValue("close", mp.Close);
				matchedPairElem.SetAttributeValue("paraClose", mp.ParagraphClose); // optional, default to false?
				specialElem = GetOrCreateSpecialElement(delimitersElem);
				matchedPairsElem = specialElem.GetOrCreateElement(Sil + "matched-pairs");
				matchedPairsElem.Add(matchedPairElem);
			}

			// Remove special Sil:punctuation-patterns elements to repopulate later
			XElement punctuationPatternsElem;
			if (specialElem != null)
			{
				punctuationPatternsElem = specialElem.Element(Sil + "punctuation-patterns");
				if (punctuationPatternsElem != null)
				{
					punctuationPatternsElem.Elements(Sil + "punctuation-patterns").Remove();
					RemoveIfEmpty(punctuationPatternsElem);
				}
				RemoveIfEmpty(specialElem);
			}
			foreach (var pp in ws.PunctuationPatterns)
			{
				var punctuationPatternElem = new XElement(Sil + "punctuation-pattern");
				// text is required
				punctuationPatternElem.SetAttributeValue("pattern", pp.Pattern);
				punctuationPatternElem.SetAttributeValue("context", PunctuationPatternContextToContext[pp.Context]);
				specialElem = GetOrCreateSpecialElement(delimitersElem);
				punctuationPatternsElem = specialElem.GetOrCreateElement(Sil + "punctuation-patterns");
				punctuationPatternsElem.Add(punctuationPatternElem);
			}

			// Preserve existing special sil:quotation-marks that aren't narrative or blank.
			// Remove the rest to repopulate later
			XElement quotationmarksElem = null;
			if (specialElem != null)
			{
				quotationmarksElem = specialElem.Element(Sil + "quotation-marks");
				if (quotationmarksElem != null)
				{
					quotationmarksElem.Elements(Sil + "quotation").Where(e => string.IsNullOrEmpty((string) e.Attribute("type"))).Remove();
					quotationmarksElem.Elements(Sil + "quotation").Where(e => (string) e.Attribute("type") == "narrative").Remove();
					RemoveIfEmpty(quotationmarksElem);
				}
				RemoveIfEmpty(specialElem);
			}

			if (qm1 != null)
			{
				var level1ContinuerElem = new XElement(Sil + "quotationContinue", qm1.Continue);
				specialElem = GetOrCreateSpecialElement(delimitersElem);
				quotationmarksElem = specialElem.GetOrCreateElement(Sil + "quotation-marks");
				quotationmarksElem.Add(level1ContinuerElem);
			}
			if (qm2 != null && !string.IsNullOrEmpty(qm2.Continue))
			{
				var level2ContinuerElem = new XElement(Sil + "alternateQuotationContinue", qm2.Continue);
				specialElem = GetOrCreateSpecialElement(delimitersElem);
				quotationmarksElem = specialElem.GetOrCreateElement(Sil + "quotation-marks");
				quotationmarksElem.Add(level2ContinuerElem);
			}

			foreach (QuotationMark qm in ws.QuotationMarks)
			{
				// Level 1 and 2 normal have already been written
				if (!((qm.Level == 1 || qm.Level == 2) && qm.Type == QuotationMarkingSystemType.Normal))
				{
					var quotationElem = new XElement(Sil + "quotation");
					// open and level required
					quotationElem.SetAttributeValue("open", qm.Open);
					quotationElem.SetOptionalAttributeValue("close", qm.Close);
					quotationElem.SetOptionalAttributeValue("continue", qm.Continue);
					quotationElem.SetAttributeValue("level", qm.Level);
					// normal quotation mark can have no attribute defined.  Narrative --> "narrative"
					quotationElem.SetOptionalAttributeValue("type", QuotationMarkingSystemTypesToQuotation[qm.Type]);

					specialElem = GetOrCreateSpecialElement(delimitersElem);
					quotationmarksElem = specialElem.GetOrCreateElement(Sil + "quotation-marks");
					quotationmarksElem.Add(quotationElem);
				}
			}
			if ((ws.QuotationParagraphContinueType != QuotationParagraphContinueType.None) && (quotationmarksElem != null))
			{
				quotationmarksElem.SetAttributeValue("paraContinueType",
					QuotationParagraphContinueTypesToQuotation[ws.QuotationParagraphContinueType]);
			}
		}

		private void WriteLayoutElement(XElement layoutElem, WritingSystemDefinition ws)
		{
			Debug.Assert(layoutElem != null);
			Debug.Assert(ws != null);

			// Remove characterOrder element to repopulate later
			XElement orientationElem = layoutElem.Element("orientation");
			if (orientationElem != null)
			{
				orientationElem.Elements().Where(e => e.Name == "characterOrder").Remove();
				RemoveIfEmpty(orientationElem);
			}

			// we generally don't need to write out default values, but SLDR seems to always write characterOrder
			orientationElem = layoutElem.GetOrCreateElement("orientation");
			XElement characterOrderElem = orientationElem.GetOrCreateElement("characterOrder");
			characterOrderElem.SetValue(ws.RightToLeftScript ? "right-to-left" : "left-to-right");
			// Ignore lineOrder
		}

		private void WriteNumbersElement(XElement numbersElem, WritingSystemDefinition ws)
		{
			Debug.Assert(numbersElem != null);
			Debug.Assert(ws != null);

			// Save off defaultNumberingSystem if it exists.
			string defaultNumberingSystem = "standard";
			XElement defaultNumberingSystemElem = numbersElem.Element("defaultNumberingSystem");
			if (defaultNumberingSystemElem != null && !string.IsNullOrEmpty((string) defaultNumberingSystemElem))
				defaultNumberingSystem = (string) defaultNumberingSystemElem;
			// Remove defaultNumberingSystem and numberingSystems elements of type "numeric" to repopulate later
			numbersElem.Elements("defaultNumberingSystem").Remove();
			numbersElem.Elements("numberingSystem").Where(e => (string) e.Attribute("id") == defaultNumberingSystem && (string) e.Attribute("type") == "numeric" && e.Attribute("alt") == null).Remove();

			CharacterSetDefinition csd;
			if (ws.CharacterSets.TryGet("numeric", out csd))
			{
				// Create defaultNumberingSystem element and add as the first child
				if (defaultNumberingSystemElem == null)
				{
					defaultNumberingSystemElem = new XElement("defaultNumberingSystem", defaultNumberingSystem);
					numbersElem.AddFirst(defaultNumberingSystemElem);
				}

				// Populate numbering system element
				var numberingSystemsElem = new XElement("numberingSystem");
				numberingSystemsElem.SetAttributeValue("id", defaultNumberingSystem);
				numberingSystemsElem.SetAttributeValue("type", csd.Type);
				string digits = string.Join("", csd.Characters);
				numberingSystemsElem.SetAttributeValue("digits", digits);
				numbersElem.Add(numberingSystemsElem);
			}
		}

		private void WriteCollationsElement(XElement collationsElem, WritingSystemDefinition ws)
		{
			// Preserve exisiting collations since we don't process them all
			// Remove only the collations we can repopulate from the writing system
			collationsElem.Elements("collation").Where(ce => ce.Elements("special").Elements().All(se => se.Name != (Sil + "reordered"))).Remove();

			if (ws.DefaultCollation != null)
			{
				XElement defaultCollationElem = collationsElem.GetOrCreateElement("defaultCollation");
				defaultCollationElem.SetValue(ws.DefaultCollation.Type);
			}
			
			foreach (CollationDefinition collation in ws.Collations)
				WriteCollationElement(collationsElem, collation);
		}

		private void WriteCollationElement(XElement collationsElem, CollationDefinition collation)
		{
			Debug.Assert(collationsElem != null);
			Debug.Assert(collation != null);

			string type, alt;
			ParseIdentifier(collation.Type, out type, out alt);

			var collationElem = new XElement("collation", new XAttribute("type", type));
			collationElem.SetAttributeValue("alt", alt);
			collationsElem.Add(collationElem);

			var icuCollation = collation as IcuCollationDefinition;
			if (icuCollation != null)
				WriteCollationRulesFromCustomIcu(collationElem, icuCollation);

			var simpleCollation = collation as SimpleCollationDefinition;
			if (simpleCollation != null)
				WriteCollationRulesFromCustomSimple(collationElem, simpleCollation);
		}

		private void WriteCollationRulesFromCustomIcu(XElement collationElem, IcuCollationDefinition icd)
		{
			foreach (IcuCollationImport import in icd.Imports)
			{
				var importElem = new XElement("import", new XAttribute("source", import.IetfLanguageTag));
				if (!string.IsNullOrEmpty(import.Type))
					importElem.Add(new XAttribute("type", import.Type));
				collationElem.Add(importElem);
			}

			// If collation valid and icu rules exist, populate icu rules
			if (!string.IsNullOrEmpty(icd.IcuRules))
				collationElem.Add(new XElement("cr", new XCData(icd.IcuRules)));
		}

		private void WriteCollationRulesFromCustomSimple(XElement collationElem, SimpleCollationDefinition scd)
		{
			// If collation valid and icu rules exist, populate icu rules
			if (!string.IsNullOrEmpty(scd.CollationRules))
				collationElem.Add(new XElement("cr", new XCData(scd.CollationRules)));

			XElement specialElem = GetOrCreateSpecialElement(collationElem);
			// SLDR generally doesn't include needsCompiling if false
			specialElem.SetAttributeValue(Sil + "needsCompiling", scd.IsValid ? null : "true");
			specialElem.Add(new XElement(Sil + "simple", new XCData(scd.SimpleRules)));
		}
		
		private void WriteTopLevelSpecialElements(XElement specialElem, WritingSystemDefinition ws)
		{
			XElement externalResourcesElem = specialElem.GetOrCreateElement(Sil + "external-resources");
			WriteFontElement(externalResourcesElem, ws);
			WriteSpellcheckElement(externalResourcesElem, ws);
			WriteKeyboardElement(externalResourcesElem, ws);
		}

		private void WriteFontElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			Debug.Assert(externalResourcesElem != null);
			Debug.Assert(ws != null);

			// Remove sil:font elements to repopulate later
			externalResourcesElem.Elements(Sil + "font").Remove();
			foreach (FontDefinition font in ws.Fonts)
			{
				var fontElem = new XElement(Sil + "font");
				fontElem.SetAttributeValue("name", font.Name);

				// Generate space-separated list of font roles
				if (font.Roles != FontRoles.Default)
				{
					var fontRoleList = new List<string>();
					foreach (FontRoles fontRole in Enum.GetValues(typeof(FontRoles)))
					{
						if ((font.Roles & fontRole) != 0)
							fontRoleList.Add(FontRolesToRole[fontRole]);
					}
					fontElem.SetAttributeValue("types", string.Join(" ", fontRoleList));
				}

				if (font.RelativeSize != 1.0f)
					fontElem.SetAttributeValue("size", font.RelativeSize);

				fontElem.SetOptionalAttributeValue("minversion", font.MinVersion);
				fontElem.SetOptionalAttributeValue("features", font.Features);
				fontElem.SetOptionalAttributeValue("lang", font.Language);
				fontElem.SetOptionalAttributeValue("otlang", font.OpenTypeLanguage);
				fontElem.SetOptionalAttributeValue("subset", font.Subset);

				// Generate space-separated list of font engines
				if (font.Engines != (FontEngines.Graphite | FontEngines.OpenType))
				{
					var fontEngineList = new List<string>();
					foreach (FontEngines fontEngine in Enum.GetValues(typeof (FontEngines)))
					{
						if ((font.Engines & fontEngine) != 0)
							fontEngineList.Add(FontEnginesToEngine[fontEngine]);
					}
					fontElem.SetAttributeValue("engines", string.Join(" ", fontEngineList));
				}
				foreach (var url in font.Urls)
					fontElem.Add(new XElement(Sil + "url", url));

				externalResourcesElem.Add(fontElem);
			}
		}

		private void WriteSpellcheckElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			Debug.Assert(externalResourcesElem != null);
			Debug.Assert(ws != null);

			// Remove sil:spellcheck elements to repopulate later
			externalResourcesElem.Elements(Sil + "spellcheck").Remove();
			foreach (SpellCheckDictionaryDefinition scd in ws.SpellCheckDictionaries)
			{
				var scElem = new XElement(Sil + "spellcheck");
				scElem.SetAttributeValue("type", SpellCheckDictionaryFormatsToSpellCheck[scd.Format]);

				// URL elements
				foreach (var url in scd.Urls)
				{
					var urlElem  = new XElement(Sil + "url", url);
					scElem.Add(urlElem);
				}
				externalResourcesElem.Add(scElem);
			}
		}

		private void WriteKeyboardElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			Debug.Assert(externalResourcesElem != null);
			Debug.Assert(ws != null);
			
			// Remove sil:keyboard elements to repopulate later
			externalResourcesElem.Elements(Sil + "keyboard").Remove();

			foreach (IKeyboardDefinition keyboard in ws.KnownKeyboards)
			{
				var kbdElem = new XElement(Sil + "kbd");
				// id required
				kbdElem.SetAttributeValue("id", keyboard.Id);
				if (!string.IsNullOrEmpty(keyboard.Id))
				{
					kbdElem.SetAttributeValue("type", KeyboardFormatToKeyboard[keyboard.Format]);
					foreach (var url in keyboard.Urls)
					{
						var urlElem = new XElement(Sil + "url", url);
						kbdElem.Add(urlElem);
					}
				}
				externalResourcesElem.Add(kbdElem);
			}
		}

		private void ParseIdentifier(string id, out string ldmlId, out string alt)
		{
			int index = id.IndexOf(IdentifierDelimiter, StringComparison.Ordinal);
			if (index == -1)
			{
				ldmlId = id;
				alt = null;
			}
			else
			{
				ldmlId = id.Substring(0, index);
				alt = id.Substring(index + 1);
			}
		}
	}
}