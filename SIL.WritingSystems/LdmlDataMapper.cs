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
using SIL.Extensions;
using SIL.Keyboarding;
using SIL.Xml;

namespace SIL.WritingSystems
{
	/// <summary>
	/// The LdmlDatamapper Reads and Writes WritingSystemDefinitions to LDML files. A typical consuming application should not
	/// need to use the LdmlDataMapper directly but should rather use an IWritingSystemRepository (such as the
	/// LdmlInfolderWritingSystemRepository) to manage it's writing systems.
	/// The LdmlDatamapper is tightly tied to the CLDR version of LDML. If the LdmlDatamapper refuses to Read a
	/// particular Ldml file it may need to be migrated to the latest version. Please use the
	/// LdmlInFolderWritingSystemRepository class for this purpose.
	/// Please note that the LdmlDataMapper.Write method can round trip data that it does not understand if passed an
	/// appropriate stream or xmlreader produced from the old file.
	/// Be aware that as of Jul-5-2011 an exception was made for certain well defined Fieldworks LDML files whose contained
	/// Rfc5646 tag begin with "x-". These will load correctly, albeit in a transformed state, in spite of being "Version 0".
	/// Furthermore writing systems containing RfcTags beginning with "x-" and that have a matching Fieldworks conform LDML file
	/// in the repository will not be changed including no marking with "version 1".
	/// </summary>
	/// <remarks>
	/// LDML reference: http://www.unicode.org/reports/tr35/
	/// </remarks>
	public class LdmlDataMapper
	{
		/// <summary>
		/// This is the current version of the LDML data and is mostly used for migration purposes.
		/// This should not be confused with the version of the locale data contained in this writing system.
		/// That information is stored in the "VersionNumber" property.
		/// </summary>
		public const int CurrentLdmlLibraryVersion = 3;

		private static readonly XNamespace Palaso = "urn://palaso.org/ldmlExtensions/v1";
		private static readonly XNamespace Sil = "urn://www.sil.org/ldml/0.1";

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
			{"kmx", KeyboardFormat.CompiledKeyman},
			{"msklc", KeyboardFormat.Msklc},
			{"ldml", KeyboardFormat.Ldml},
			{"keylayout", KeyboardFormat.Keylayout}
		}; 

		/// <summary>
		/// Mapping of KeyboardFormat enumeration to keyboard type attribute
		/// </summary>
		private static readonly Dictionary<KeyboardFormat, string> KeyboardFormatToKeyboard = new Dictionary<KeyboardFormat, string>
		{
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
		private static readonly Dictionary<string, QuotationMarkingSystemType> QuotationToQuotationMarkingSystemTypes = new Dictionary<string, QuotationMarkingSystemType>
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

		/// <summary>
		/// Read the LDML file at the root element and populate the writing system.
		/// </summary>
		/// <param name="element">Root element</param>
		/// <param name="ws">Writing System to populate</param>
		/// <remarks>
		/// For elements that have * annotation in the LDML spec, we use the extensions NonAltElement
		/// and NonAltElements to ignore the elements that have the alt attribute.
		/// </remarks>
		private void ReadLdml(XElement element, WritingSystemDefinition ws)
		{
			Debug.Assert(element != null);
			Debug.Assert(ws != null);
			if (element.Name != "ldml")
				throw new ApplicationException("Unable to load writing system definition: Missing <ldml> tag.");

			ResetWritingSystemDefinition(ws);

			var identityElem = element.Element("identity");
			if (identityElem != null)
			{
				ReadIdentityElement(identityElem, ws);
			}

			// Check for the proper LDML version after reading identity element so that we have the proper language tag if an error occurs.
			foreach (var specialElem in element.NonAltElements("special"))
				CheckVersion(specialElem, ws);

			var charactersElem = element.Element("characters");
			if (charactersElem != null)
				ReadCharactersElement(charactersElem, ws);

			var delimitersElem = element.Element("delimiters");
			if (delimitersElem != null)
				ReadDelimitersElement(delimitersElem, ws);

			var layoutElem = element.Element("layout");
			if (layoutElem != null)
				ReadLayoutElement(layoutElem, ws);

			var numbersElem = element.Element("numbers");
			if (numbersElem != null)
			{
				ReadNumbersElement(numbersElem, ws);
				if (ws.NumberingSystem == NumberingSystemDefinition.Default)
				{
					ReadIntermediateVersion3NumbersElement(numbersElem, ws);
				}
			}

			var collationsElem = element.Element("collations");
			if (collationsElem != null)
			{
				ReadCollationsElement(collationsElem, ws);
			}
			else
			{
				ws.DefaultCollationType = "standard";
				var systemDefinition = new SystemCollationDefinition { LanguageTag = ws.LanguageTag };
				ws.DefaultCollation = systemDefinition.IsValid ? systemDefinition : new SystemCollationDefinition();
			}

			foreach (XElement specialElem in element.NonAltElements("special"))
				ReadTopLevelSpecialElement(specialElem, ws);

			// Validate collations after all of them have been read in (for self-referencing imports)
			foreach (CollationDefinition cd in ws.Collations)
			{
				string message;
				cd.Validate(out message);
			}
			ws.Id = string.Empty;
			ws.AcceptChanges();
		}

		/// <summary>
		/// Resets all of the properties of the writing system definition that might not get set when reading the LDML file.
		/// </summary>
		private void ResetWritingSystemDefinition(WritingSystemDefinition ws)
		{
			ws.VersionNumber = null;
			ws.VersionDescription = null;
			ws.WindowsLcid = null;
			ws.DefaultRegion = null;
			ws.CharacterSets.Clear();
			ws.MatchedPairs.Clear();
			ws.PunctuationPatterns.Clear();
			ws.QuotationMarks.Clear();
			ws.QuotationParagraphContinueType = QuotationParagraphContinueType.None;
			ws.RightToLeftScript = false;
			ws.DefaultCollationType = null;
			ws.Collations.Clear();
			ws.Fonts.Clear();
			ws.SpellCheckDictionaries.Clear();
			ws.KnownKeyboards.Clear();
		}

		private void CheckVersion(XElement specialElem, WritingSystemDefinition ws)
		{
			// Flag invalid versions (0-2 inclusive) from reading legacy LDML files
			// We're intentionally not using WritingSystemLDmlVersionGetter and the
			// cheeck for Flex7V0Compatible because the migrator will have handled that.
			if (!string.IsNullOrEmpty((string) specialElem.Attribute(XNamespace.Xmlns + "fw")) ||
				!string.IsNullOrEmpty((string) specialElem.Attribute(XNamespace.Xmlns + "palaso")))
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
					ws.LanguageTag,
					version,
					CurrentLdmlLibraryVersion
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
			foreach (XElement fontElem in externalResourcesElem.NonAltElements(Sil + "font"))
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
					foreach (XElement urlElem in fontElem.NonAltElements(Sil + "url"))
						fd.Urls.Add(urlElem.Value);

					ws.Fonts.Add(fd);
				}
			}
		}

		private void ReadSpellcheckElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			foreach (XElement scElem in externalResourcesElem.NonAltElements(Sil + "spellcheck"))
			{
				var type = (string) scElem.Attribute("type");
				if (!string.IsNullOrEmpty(type))
				{
					var scd = new SpellCheckDictionaryDefinition(SpellCheckToSpecllCheckDictionaryFormats[type]);

					// URL elements
					foreach (XElement urlElem in scElem.NonAltElements(Sil + "url"))
						scd.Urls.Add(urlElem.Value);
					ws.SpellCheckDictionaries.Add(scd);
				}
			}
		}

		private void ReadKeyboardElement(XElement externalResourcesElem, WritingSystemDefinition ws)
		{
			foreach (XElement kbdElem in externalResourcesElem.NonAltElements(Sil + "kbd"))
			{
				var id = (string) kbdElem.Attribute("id");
				if (!string.IsNullOrEmpty(id))
				{
					KeyboardFormat format = KeyboardToKeyboardFormat[(string) kbdElem.Attribute("type") ?? string.Empty];
					IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboard(id, format, kbdElem.NonAltElements(Sil + "url").Select(u => (string) u));
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
			DateTime modified = DateTime.UtcNow;
			if (generationElem != null)
			{
				string dateTime = (string) generationElem.Attribute("date") ?? string.Empty;
				// Previous versions of LDML Data Mapper allowed generation date to be in CVS format
				// This is deprecated so we only handle ISO 8601 format
				if (DateTimeExtensions.IsISO8601DateTime(dateTime))
					modified = DateTimeExtensions.ParseISO8601DateTime(dateTime);
			}
			ws.DateModified = modified;

			string language = identityElem.GetAttributeValue("language", "type");
			string script = identityElem.GetAttributeValue("script", "type");
			string region = identityElem.GetAttributeValue("territory", "type");
			string variant = identityElem.GetAttributeValue("variant", "type");

			ws.LanguageTag = IetfLanguageTag.Create(language, script, region, variant);

			// TODO: Parse rest of special element.  Currently only handling a subset
			XElement specialElem = identityElem.NonAltElement("special");
			if (specialElem != null)
			{
				XElement silIdentityElem = specialElem.Element(Sil + "identity");
				if (silIdentityElem != null)
				{
					ws.WindowsLcid = (string) silIdentityElem.Attribute("windowsLCID");
					ws.DefaultRegion = (string) silIdentityElem.Attribute("defaultRegion");
					// Update the variant name if a non-wellknown private use variant exists
					var variantName = (string) silIdentityElem.Attribute("variantName") ?? String.Empty;
					int index = IetfLanguageTag.GetIndexOfFirstNonCommonPrivateUseVariant(ws.Variants);
					if (!string.IsNullOrEmpty(variantName) && (index != -1))
					{
						ws.Variants[index] = new VariantSubtag(ws.Variants[index], variantName);
					}
				}
			}
		}

		private void ReadCharactersElement(XElement charactersElem, WritingSystemDefinition ws)
		{
			foreach (XElement exemplarCharactersElem in charactersElem.NonAltElements("exemplarCharacters"))
				ReadExemplarCharactersElem(exemplarCharactersElem, ws);

			XElement specialElem = charactersElem.NonAltElement("special");
			if (specialElem != null)
			{
				foreach (XElement exemplarCharactersElem in specialElem.NonAltElements(Sil + "exemplarCharacters"))
				{
					// Sil:exemplarCharacters are required to have a type
					if (!string.IsNullOrEmpty((string) exemplarCharactersElem.Attribute("type")))
						ReadExemplarCharactersElem(exemplarCharactersElem, ws);
				}
			}
		}

		private void ReadExemplarCharactersElem(XElement exemplarCharactersElem, WritingSystemDefinition ws)
		{
			string type = (string) exemplarCharactersElem.Attribute("type") ?? "main";
			var csd = new CharacterSetDefinition(type);
			var unicodeSet = (string) exemplarCharactersElem;
			csd.Characters.AddRange(csd.IsSequenceType ? unicodeSet.Trim('[', ']').Split(' ').Select(c => c.Trim('{', '}')) : UnicodeSet.ToCharacters(unicodeSet));
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

			XElement specialElem = delimitersElem.NonAltElement("special");
			if (specialElem != null)
			{
				XElement matchedPairsElem = specialElem.Element(Sil + "matched-pairs");
				if (matchedPairsElem != null)
				{
					foreach (XElement matchedPairElem in matchedPairsElem.NonAltElements(Sil + "matched-pair"))
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
					foreach (XElement punctuationPatternElem in punctuationPatternsElem.NonAltElements(Sil + "punctuation-pattern"))
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

					foreach (XElement quotationElem in quotationsElem.NonAltElements(Sil + "quotation"))
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
			open = (string)delimitersElem.NonAltElement("quotationStart");
			close = (string)delimitersElem.NonAltElement("quotationEnd");
			if (!string.IsNullOrEmpty(open) || !string.IsNullOrEmpty(close) || !string.IsNullOrEmpty(level1Continue))
			{
				var qm = new QuotationMark(open, close, level1Continue, 1, QuotationMarkingSystemType.Normal);
				ws.QuotationMarks.Add(qm);
			}

			// level 2: alternateQuotationStart, alternateQuotationEnd
			open = (string)delimitersElem.NonAltElement("alternateQuotationStart");
			close = (string)delimitersElem.NonAltElement("alternateQuotationEnd");
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
			XElement orientationElem = layoutElem.NonAltElement("orientation");
			if (orientationElem != null)
			{
				XElement characterOrderElem = orientationElem.NonAltElement("characterOrder");
				if (characterOrderElem != null)
					ws.RightToLeftScript = ((string) characterOrderElem == "right-to-left");
			}
		}

		/// <summary>
		/// This will read the content written out in the first released version 3 LDML Library (which was used in some limited releases of products)
		/// The content was not valid LDML.
		/// The numbering system is added as a NumberingSystemDefinition
		/// </summary>
		private void ReadIntermediateVersion3NumbersElement(XElement numbersElem, WritingSystemDefinition ws)
		{
			XElement defaultNumberingSystemElem = numbersElem.NonAltElement("defaultNumberingSystem");
			if (defaultNumberingSystemElem != null)
			{
				var id = (string)defaultNumberingSystemElem;
				XElement numberingSystemsElem = numbersElem.NonAltElements("numberingSystem")
					.FirstOrDefault(e => id == (string)e.Attribute("id") && (string)e.Attribute("type") == "numeric");
				if (numberingSystemsElem != null)
				{
					var digits = (string)numberingSystemsElem.Attribute("digits");
					var cldrSysId = CLDRNumberingSystems.FindNumberingSystemID(digits);
					if (cldrSysId == null) // digits don't match a cldr def, create a custom one
					{
						ws.NumberingSystem = NumberingSystemDefinition.CreateCustomSystem(digits);
					}
					else
					{
						ws.NumberingSystem = new NumberingSystemDefinition(cldrSysId);
					}
				}
			}
		}

		/// <summary>
		/// Read the numbers element, expecting only a defaultNumberingSystem element.
		/// The numbering system is added as a NumberingSystemDefinition
		/// </summary>
		private void ReadNumbersElement(XElement numbersElem, WritingSystemDefinition ws)
		{
			XElement defaultNumberingSystemElem = numbersElem.NonAltElement("defaultNumberingSystem");
			if (defaultNumberingSystemElem != null)
			{
				var id = (string)defaultNumberingSystemElem;
				var digits = CLDRNumberingSystems.GetDigitsForID(id);
				if (!string.IsNullOrEmpty(digits) )
				{
					ws.NumberingSystem = new NumberingSystemDefinition(id);
				}
				else if(ParseCustomNumberingSystem(id, out digits))
				{
					ws.NumberingSystem = NumberingSystemDefinition.CreateCustomSystem(digits);
				}
			}
		}

		/// <summary>
		/// Parses text similar to 'other(0123456789)' returns true if valid
		/// </summary>
		private bool ParseCustomNumberingSystem(string id, out string digits)
		{
			var regex = new Regex("other\\((.*)\\)");
			var match = regex.Match(id.Trim());
			if (match.Success)
			{
				digits = match.Groups[1].Value;
				return true;
			}

			digits = null;
			return false;
		}

		private void ReadCollationsElement(XElement collationsElem, WritingSystemDefinition ws)
		{
			var defaultCollationElem = collationsElem.Element("defaultCollation");
			ws.DefaultCollationType = (string) defaultCollationElem ?? "standard";
			foreach (var collationElem in collationsElem.NonAltElements("collation"))
			{
				ReadCollationElement(collationElem, ws);
			}
			if (defaultCollationElem == null)
			{
				var sysColDef = new SystemCollationDefinition { LanguageTag = ws.LanguageTag };
				ws.DefaultCollation = sysColDef.IsValid ? sysColDef : new SystemCollationDefinition();
			}
		}

		private void ReadCollationElement(XElement collationElem, WritingSystemDefinition ws)
		{
			var collationType = (string)collationElem.Attribute("type");
			if (!string.IsNullOrEmpty(collationType))
			{
				CollationDefinition cd = null;
				XElement specialElem = collationElem.NonAltElement("special");
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
					cd = ReadCollationRulesForCustomIcu(collationElem, ws, collationType);
				}

				// Only add collation definition if it's been set
				if (cd != null)
					ws.Collations.Add(cd);
			}
		}

		private SimpleRulesCollationDefinition ReadCollationRulesForCustomSimple(XElement collationElem, XElement specialElem, string collationType)
		{
			XElement simpleElem = specialElem.Element(Sil + "simple");
			bool needsCompiling = (bool?) specialElem.Attribute(Sil + "needscompiling") ?? false;
			var scd = new SimpleRulesCollationDefinition(collationType) {SimpleRules = ((string) simpleElem).Replace("\n", "\r\n")};
			if (!needsCompiling)
			{
				scd.CollationRules = LdmlCollationParser.GetIcuRulesFromCollationNode(collationElem);
				scd.IsValid = true;
			}
			return scd;
		}

		private IcuRulesCollationDefinition ReadCollationRulesForCustomIcu(XElement collationElem, WritingSystemDefinition ws, string collationType)
		{
			var icd = new IcuRulesCollationDefinition(collationType) {WritingSystemFactory = _writingSystemFactory, OwningWritingSystemDefinition = ws};
			icd.Imports.AddRange(collationElem.NonAltElements("import").Select(ie => new IcuCollationImport((string) ie.Attribute("source"), (string) ie.Attribute("type"))));
			icd.IcuRules = LdmlCollationParser.GetIcuRulesFromCollationNode(collationElem);
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
		/// Utility to remove empty elements. Since isEmpty is true for "<element />"
		/// but false for "<element></element>", we have to check both cases
		/// </summary>
		/// <param name="element">XElement to remove if it's empty or has 0 contents/attributes/elements</param>
		private void RemoveIfEmpty(ref XElement element)
		{
			if (element != null)
			{
				if (element.IsEmpty || (string.IsNullOrEmpty((string)element) && !element.HasElements && !element.HasAttributes))
				{
					element.Remove();
					element = null;
				}
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
			string message;
			if (!ws.ValidateLanguageTag(out message))
				throw new ArgumentException(string.Format("The writing system's IETF language tag is invalid: {0}", message), "ws");
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
					try
					{
						element = XElement.Load(reader);
					}
					catch
					{
						// ignore content of old file since it can't be read
						element = new XElement("ldml");
					}
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
					writer.Flush();
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
			string message;
			if (!ws.ValidateLanguageTag(out message))
				throw new ArgumentException(string.Format("The writing system's IETF language tag is invalid: {0}", message), "ws");
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
			RemoveIfEmpty(ref identityElem);

			XElement charactersElem = element.GetOrCreateElement("characters");
			WriteCharactersElement(charactersElem, ws);
			RemoveIfEmpty(ref charactersElem);

			XElement delimitersElem = element.GetOrCreateElement("delimiters");
			WriteDelimitersElement(delimitersElem, ws);
			RemoveIfEmpty(ref delimitersElem);

			XElement layoutElem = element.GetOrCreateElement("layout");
			WriteLayoutElement(layoutElem, ws);
			RemoveIfEmpty(ref layoutElem);

			XElement numbersElem = element.GetOrCreateElement("numbers");
			WriteNumbersElement(numbersElem, ws);
			RemoveIfEmpty(ref numbersElem);

			XElement collationsElem = element.GetOrCreateElement("collations");
			WriteCollationsElement(collationsElem, ws);
			RemoveIfEmpty(ref collationsElem);

			// Can have multiple specials.  Find the one with SIL namespace and external-resources.
			// Also handle case where we create special because writingsystem has entries to write
			XElement specialElem = element.NonAltElements("special").FirstOrDefault(
				e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns+"sil")) && e.NonAltElement(Sil + "external-resources") != null);
			if (specialElem == null && (ws.Fonts.Count > 0 || ws.KnownKeyboards.Count > 0 || ws.SpellCheckDictionaries.Count > 0))
			{
				// Create special element
				specialElem = GetOrCreateSpecialElement(element);
			}
			if (specialElem != null)
			{
				WriteTopLevelSpecialElements(specialElem, ws);
				RemoveIfEmpty(ref specialElem);
			}

			element.WriteTo(writer);
		}

		private void WriteIdentityElement(XElement identityElem, WritingSystemDefinition ws)
		{
			Debug.Assert(identityElem != null);
			Debug.Assert(ws != null);

			// Remove non-special elements to repopulate later
			// Preserve special because we don't recreate all its contents
			identityElem.NonAltElements().Where(e => e.Name != "special").Remove();

			// Version is required.  If VersionNumber is blank, the empty attribute is still written
			XElement versionElem = identityElem.GetOrCreateElement("version");
			versionElem.SetAttributeValue("number", ws.VersionNumber);
			if (!string.IsNullOrEmpty(ws.VersionDescription))
				versionElem.SetValue(ws.VersionDescription);

			// Write generation date with UTC so no more ambiguity on timezone
			identityElem.SetAttributeValue("generation", "date", ws.DateModified.ToISO8601TimeFormatWithUTCString());
			WriteLanguageTagElements(identityElem, ws.LanguageTag);

			// Create special element if data needs to be written
			int index = IetfLanguageTag.GetIndexOfFirstNonCommonPrivateUseVariant(ws.Variants);
			string variantName = string.Empty;
			if (index != -1)
				variantName = ws.Variants[index].Name;
			if (!string.IsNullOrEmpty(ws.WindowsLcid) || !string.IsNullOrEmpty(ws.DefaultRegion) || !string.IsNullOrEmpty(variantName))
			{
				XElement specialElem = GetOrCreateSpecialElement(identityElem);
				XElement silIdentityElem = specialElem.GetOrCreateElement(Sil + "identity");

				// uid and revid attributes are left intact

				silIdentityElem.SetOptionalAttributeValue("windowsLCID", ws.WindowsLcid);
				silIdentityElem.SetOptionalAttributeValue("defaultRegion", ws.DefaultRegion);
				silIdentityElem.SetOptionalAttributeValue("variantName", variantName);
					
				// Move special to the end of the identity block (preserving order)
				specialElem.Remove();
				identityElem.Add(specialElem);
			}
		}

		private void WriteLanguageTagElements(XElement identityElem, string languageTag) 
		{
			string language, script, region, variant;
			IetfLanguageTag.TryGetParts(languageTag, out language, out script, out region, out variant);
			
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
			charactersElem.NonAltElements("exemplarCharacters").Remove();
			XElement specialElem = charactersElem.NonAltElement("special");
			if (specialElem != null)
			{
				specialElem.NonAltElements(Sil + "exemplarCharacters").Remove();
				RemoveIfEmpty(ref specialElem);
			}

			foreach (CharacterSetDefinition csd in ws.CharacterSets)
			{
				XElement exemplarCharactersElem;
				switch (csd.Type)
				{
					// These character sets go to the normal LDML exemplarCharacters space
					// http://unicode.org/reports/tr35/tr35-general.html#Exemplars
					case "main":
					case "auxiliary":
					case "index":
					case "punctuation":
						exemplarCharactersElem = new XElement("exemplarCharacters", UnicodeSet.ToPattern(csd.Characters));
						// Assume main set doesn't have an attribute type
						if (csd.Type != "main")
							exemplarCharactersElem.SetAttributeValue("type", csd.Type);
						charactersElem.Add(exemplarCharactersElem);
						break;
					// Numeric characters will be written in the numbers element
					case "numeric":
						break;
					// All others go to special Sil:exemplarCharacters
					default:
						string unicodeSet = csd.IsSequenceType ? string.Format("[{0}]", string.Join(" ", csd.Characters.Select(c => c.Length > 1 ? string.Format("{{{0}}}", c) : c)))
							: UnicodeSet.ToPattern(csd.Characters);
						exemplarCharactersElem = new XElement(Sil + "exemplarCharacters", unicodeSet);
						exemplarCharactersElem.SetAttributeValue("type", csd.Type);
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
			delimitersElem.NonAltElements().Where(e => e.Name != "special").Remove();

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
			XElement specialElem = delimitersElem.NonAltElement("special");
			XElement matchedPairsElem;
			if (specialElem != null)
			{
				matchedPairsElem = specialElem.NonAltElement(Sil + "matched-pairs");
				if (matchedPairsElem != null)
				{
					matchedPairsElem.NonAltElements(Sil + "matched-pair").Remove();
					RemoveIfEmpty(ref matchedPairsElem);
				}
				RemoveIfEmpty(ref specialElem);
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
				punctuationPatternsElem = specialElem.NonAltElement(Sil + "punctuation-patterns");
				if (punctuationPatternsElem != null)
				{
					punctuationPatternsElem.NonAltElements(Sil + "punctuation-pattern").Remove();
					RemoveIfEmpty(ref punctuationPatternsElem);
				}
				RemoveIfEmpty(ref specialElem);
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

			// Remove sil:quotation elements where type is blank or narrative. Also remove quotation continue elements.
			// These will be repopulated later
			XElement quotationmarksElem = null;
			if (specialElem != null)
			{
				quotationmarksElem = specialElem.NonAltElement(Sil + "quotation-marks");
				if (quotationmarksElem != null)
				{
					quotationmarksElem.NonAltElements(Sil + "quotation").Where(e => string.IsNullOrEmpty((string) e.Attribute("type"))).Remove();
					quotationmarksElem.NonAltElements(Sil + "quotation").Where(e => (string) e.Attribute("type") == "narrative").Remove();
					quotationmarksElem.NonAltElements(Sil + "quotationContinue").Remove();
					quotationmarksElem.NonAltElements(Sil + "alternateQuotationContinue").Remove();
					RemoveIfEmpty(ref quotationmarksElem);
				}
				RemoveIfEmpty(ref specialElem);
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
			XElement orientationElem = layoutElem.NonAltElement("orientation");
			if (orientationElem != null)
			{
				orientationElem.NonAltElements().Where(e => e.Name == "characterOrder").Remove();
				RemoveIfEmpty(ref orientationElem);
			}

			// we generally don't need to write out default values, but SLDR seems to always write characterOrder
			orientationElem = layoutElem.GetOrCreateElement("orientation");
			XElement characterOrderElem = orientationElem.GetOrCreateElement("characterOrder");
			characterOrderElem.SetValue(ws.RightToLeftScript ? "right-to-left" : "left-to-right");
			// Ignore lineOrder
		}

		/// <summary>
		/// Write all the content into the Numbers element cleaning up bad data from our old library and setting the defaultNumberingSystem
		/// for the writing system if there is one defined other than the CLDR default ('latn').
		/// </summary>
		private void WriteNumbersElement(XElement numbersElem, WritingSystemDefinition ws)
		{
			Debug.Assert(numbersElem != null);
			Debug.Assert(ws != null);

			// Remove defaultNumberingSystem and numberingSystems elements of type "numeric", we'll repopulate defaultNumberingSystem if needed
			numbersElem.NonAltElements("defaultNumberingSystem").Remove();
			// Remove any numberingSystem elements created by our intermediate version of the LDMLLibrary
			numbersElem.NonAltElements("numberingSystem").Where(e => (string) e.Attribute("id") == "standard" && (string) e.Attribute("type") == "numeric" && e.Attribute("alt") == null).Remove();

			var numberingSystem = ws.NumberingSystem;
			if (numberingSystem.Equals(NumberingSystemDefinition.Default))
				return;
			// Create defaultNumberingSystem element and add as the first child
			var defaultNumberingSystemElem = new XElement("defaultNumberingSystem", numberingSystem.Id);
			numbersElem.AddFirst(defaultNumberingSystemElem);
		}

		private void WriteCollationsElement(XElement collationsElem, WritingSystemDefinition ws)
		{
			// Preserve exisiting collations since we don't process them all
			// Remove only the collations we can repopulate from the writing system
			collationsElem.NonAltElements("collation").Where(ce => ce.NonAltElements("special").Elements().All(se => se.Name != (Sil + "reordered"))).Remove();

			// if there will be no collation elements, don't write out defaultCollation element
			if (!collationsElem.Elements("collation").Any() && ws.Collations.All(c => c is SystemCollationDefinition))
			{
				return;
			}

			var defaultCollationElem = collationsElem.GetOrCreateElement("defaultCollation");
			defaultCollationElem.SetValue(ws.DefaultCollationType);

			foreach (CollationDefinition collation in ws.Collations)
			{
				WriteCollationElement(collationsElem, collation);
			}
		}

		private void WriteCollationElement(XElement collationsElem, CollationDefinition collation)
		{
			Debug.Assert(collationsElem != null);
			Debug.Assert(collation != null);

			// SystemCollationDefinition is application-specific and not written to LDML
			// REVIEW: This does not handle the 'sort like' situation and that may need to be addressed
			if (collation is SystemCollationDefinition)
			{
				return;
			}

			var collationElem = new XElement("collation", new XAttribute("type", collation.Type));
			collationsElem.Add(collationElem);

			string message;
			collation.Validate(out message);

			var icuCollation = collation as IcuRulesCollationDefinition;
			if (icuCollation != null)
				WriteCollationRulesFromCustomIcu(collationElem, icuCollation);

			var simpleCollation = collation as SimpleRulesCollationDefinition;
			if (simpleCollation != null)
				WriteCollationRulesFromCustomSimple(collationElem, simpleCollation);
		}

		private void WriteCollationRulesFromCustomIcu(XElement collationElem, IcuRulesCollationDefinition icd)
		{
			foreach (IcuCollationImport import in icd.Imports)
			{
				var importElem = new XElement("import", new XAttribute("source", import.LanguageTag));
				if (!string.IsNullOrEmpty(import.Type))
					importElem.Add(new XAttribute("type", import.Type));
				collationElem.Add(importElem);
			}

			// If collation invalid because we couldn't parse the icu rules, write a comment to send back to SLDR
			if (!icd.IsValid)
				collationElem.Add(new XComment(string.Format("Unable to parse the ICU rules with ICU version {0}", Wrapper.IcuVersion)));

			// If collation valid and icu rules exist, populate icu rules
			if (!string.IsNullOrEmpty(icd.IcuRules))
				collationElem.Add(new XElement("cr", new XCData(icd.IcuRules)));
		}

		private void WriteCollationRulesFromCustomSimple(XElement collationElem, SimpleRulesCollationDefinition scd)
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
			externalResourcesElem.NonAltElements(Sil + "font").Remove();
			foreach (FontDefinition font in ws.Fonts)
			{
				var fontElem = new XElement(Sil + "font");
				fontElem.SetAttributeValue("name", font.Name);

				// Generate space-separated list of font roles
				if (font.Roles != FontRoles.None)
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
					fontElem.SetAttributeValue("engines", fontEngineList.Count == 0 ? null : string.Join(" ", fontEngineList));
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
			externalResourcesElem.NonAltElements(Sil + "spellcheck").Remove();
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
			
			// Remove sil:kbd elements to repopulate later
			externalResourcesElem.NonAltElements(Sil + "kbd").Remove();

			// Don't include unknown system keyboard definitions
			foreach (IKeyboardDefinition keyboard in ws.KnownKeyboards.Where(kbd=>kbd.Format != KeyboardFormat.Unknown))
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
	}
}