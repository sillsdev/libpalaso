using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Icu;
using SIL.Extensions;
using SIL.IO;
using SIL.Keyboarding;
using SIL.Migration;
using SIL.Reflection;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using SIL.Xml;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV2To3Migration
{
	/// <summary>
	/// This class is used to migrate an LdmlFile from LDML palaso version 2 to 3. 
	/// Also note that the files are not written until all writing systems have been migrated in order to deal correctly
	/// with duplicate Ieft Language tags that might result from migration.
	/// </summary>
	internal class LdmlVersion2MigrationStrategy : MigrationStrategyBase
	{
		private readonly List<LdmlMigrationInfo> _migrationInfo;
		private readonly Dictionary<string, Staging> _staging;
		private readonly Action<int, IEnumerable<LdmlMigrationInfo>> _migrationHandler;
		private readonly IAuditTrail _auditLog;

		private static readonly XNamespace FW = "urn://fieldworks.sil.org/ldmlExtensions/v1";
		private static readonly XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		public LdmlVersion2MigrationStrategy(Action<int, IEnumerable<LdmlMigrationInfo>> migrationHandler, IAuditTrail auditLog) :
			base(2, 3)
		{
			_migrationInfo = new List<LdmlMigrationInfo>();
			_staging = new Dictionary<string, Staging>();
			_migrationHandler = migrationHandler;
			_auditLog = auditLog;
		}

		public override void Migrate(string sourceFilePath, string destinationFilePath)
		{
			string sourceFileName = Path.GetFileName(sourceFilePath);

			var writingSystemDefinitionV1 = new WritingSystemDefinitionV1();
			new LdmlAdaptorV1().Read(sourceFilePath, writingSystemDefinitionV1);

			string abbreviation = writingSystemDefinitionV1.Abbreviation;
			float defaultFontSize = writingSystemDefinitionV1.DefaultFontSize;
			string keyboard = writingSystemDefinitionV1.Keyboard;
			string spellCheckingId = writingSystemDefinitionV1.SpellCheckingId;
			string defaultFontName = writingSystemDefinitionV1.DefaultFontName;
			string languageName = writingSystemDefinitionV1.LanguageName.IsOneOf("Unknown Language", "Language Not Listed") ? string.Empty : writingSystemDefinitionV1.LanguageName;
			string variant, privateUse;
			IetfLanguageTag.SplitVariantAndPrivateUse(writingSystemDefinitionV1.Variant, out variant, out privateUse);
			var langTagCleaner = new IetfLanguageTagCleaner(writingSystemDefinitionV1.Language, writingSystemDefinitionV1.Script, writingSystemDefinitionV1.Region,
				variant, privateUse);
			langTagCleaner.Clean();
			string langTag = IetfLanguageTag.Canonicalize(langTagCleaner.GetCompleteTag());
			List<string> knownKeyboards = writingSystemDefinitionV1.KnownKeyboards.Select(k => string.IsNullOrEmpty(k.Locale) ? k.Layout : string.Format("{0}_{1}", k.Locale, k.Layout)).ToList();
			bool isGraphiteEnabled = false;
			string legacyMapping = string.Empty;
			string scriptName = string.Empty;
			string regionName = string.Empty;
			string variantName = string.Empty;
			SystemCollationDefinition scd = null;

			// Create system collation definition if applicable
			if ((writingSystemDefinitionV1.SortUsing == WritingSystemDefinitionV1.SortRulesType.OtherLanguage) && (!string.IsNullOrEmpty(writingSystemDefinitionV1.SortRules)))
				scd = new SystemCollationDefinition { LanguageTag = writingSystemDefinitionV1.SortRules };

			// Migrate fields from legacy fw namespace, and then remove fw namespace
			XElement ldmlElem = XElement.Load(sourceFilePath);
			XElement fwElem = ldmlElem.Elements("special").FirstOrDefault(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "fw")));
			if (fwElem != null)
			{
				XElement graphiteEnabledElem = fwElem.Element(FW + "graphiteEnabled");
				if (graphiteEnabledElem != null)
				{
					if (!bool.TryParse((string) graphiteEnabledElem.Attribute("value"), out isGraphiteEnabled))
						isGraphiteEnabled = false;
				}

				// LegacyMapping
				XElement legacyMappingElem = fwElem.Element(FW + "legacyMapping");
				if (legacyMappingElem != null)
					legacyMapping = (string) legacyMappingElem.Attribute("value");

				// ScriptName
				XElement scriptNameElem = fwElem.Element(FW + "scriptName");
				if (scriptNameElem != null)
					scriptName = (string) scriptNameElem.Attribute("value");

				// RegionName
				XElement regionNameElem = fwElem.Element(FW + "regionName");
				if (regionNameElem != null)
					regionName = (string) regionNameElem.Attribute("value");

				// VariantName
				XElement variantNameElem = fwElem.Element(FW + "variantName");
				if (variantNameElem != null)
					variantName = (string) variantNameElem.Attribute("value");
			}

			// Record the details for use in PostMigrate where we change the file name to match the ieft language tag where we can.
			var migrationInfo = new LdmlMigrationInfo(sourceFileName)
				{
					LanguageTagBeforeMigration = writingSystemDefinitionV1.Bcp47Tag,
					LanguageTagAfterMigration = langTag,
					RemovedPropertiesSetter = ws =>
					{
						if (!string.IsNullOrEmpty(abbreviation))
							ws.Abbreviation = abbreviation;
						if (defaultFontSize != 0)
							ws.DefaultFontSize = defaultFontSize;
						if (!string.IsNullOrEmpty(keyboard))
							ws.Keyboard = keyboard;
						if (!string.IsNullOrEmpty(spellCheckingId))
							ws.SpellCheckingId = spellCheckingId;
						if (!string.IsNullOrEmpty(defaultFontName))
							ws.DefaultFont = ws.Fonts[defaultFontName];
						if (!string.IsNullOrEmpty(languageName))
							ws.Language = new LanguageSubtag(ws.Language, languageName);
						ws.IsGraphiteEnabled = isGraphiteEnabled;
						if (!string.IsNullOrEmpty(legacyMapping))
							ws.LegacyMapping = legacyMapping;
						if (!string.IsNullOrEmpty(scriptName) && ws.Script != null && ws.Script.IsPrivateUse)
							ws.Script = new ScriptSubtag(ws.Script, scriptName);
						if (!string.IsNullOrEmpty(regionName) && ws.Region != null && ws.Region.IsPrivateUse)
							ws.Region = new RegionSubtag(ws.Region, regionName);
						if (scd != null)
							ws.DefaultCollation = scd;
						foreach (string keyboardId in knownKeyboards)
						{
							IKeyboardDefinition kd;
							if (!Keyboard.Controller.TryGetKeyboard(keyboardId, out kd))
								kd = Keyboard.Controller.CreateKeyboard(keyboardId, KeyboardFormat.Unknown, Enumerable.Empty<string>());
							ws.KnownKeyboards.Add(kd);
						}
					}
				};

			_migrationInfo.Add(migrationInfo);

			// Store things that stay in ldml but are being moved: WindowsLcid, variantName, font, known keyboards, collations, font features, character sets

			// misc properties
			var staging = new Staging
			{
				WindowsLcid = writingSystemDefinitionV1.WindowsLcid,
				DefaultFontName = writingSystemDefinitionV1.DefaultFontName,
				SortUsing = writingSystemDefinitionV1.SortUsing,
				SortRules = writingSystemDefinitionV1.SortRules,
			};

			// Determine if variantName is non-common private use before preserving it
			if (!string.IsNullOrEmpty(variantName))
			{
				int index = IetfLanguageTag.GetIndexOfFirstNonCommonPrivateUseVariant(IetfLanguageTag.GetVariantSubtags(migrationInfo.LanguageTagAfterMigration));
				if (index > -1)
					staging.VariantName = variantName;
			}

			if (fwElem != null)
			{
				// DefaultFontFeatures
				XElement fontFeatsElem = fwElem.Element(FW + "defaultFontFeatures");
				if (fontFeatsElem != null && !string.IsNullOrEmpty(staging.DefaultFontName))
					staging.DefaultFontFeatures = (string) fontFeatsElem.Attribute("value");

				//MatchedPairs, PunctuationPatterns, QuotationMarks deprecated

				// Valid Chars
				XElement validCharsElem = fwElem.Element(FW + "validChars");
				if (validCharsElem != null)
				{
					try
					{
						var fwValidCharsElem = XElement.Parse((string) validCharsElem.Attribute("value"));
						AddCharacterSet(fwValidCharsElem, staging, "WordForming", "main");
						AddCharacterSet(fwValidCharsElem, staging, "Numeric", "numeric");
						AddCharacterSet(fwValidCharsElem, staging, "Other", "punctuation");
					}
					catch (XmlException)
					{
						ParseLegacyWordformingCharOverridesFile(staging);
					}
				}
			}

			_staging[sourceFileName] = staging;
		}

		private static void ParseLegacyWordformingCharOverridesFile(Staging staging)
		{
			string legacyOverridesFile = GetLegacyWordformingCharOverridesPath();
			if (File.Exists(legacyOverridesFile))
			{
				XElement rootElem = XElement.Load(legacyOverridesFile);
				var characters = new HashSet<string>();
				foreach (XElement charElem in rootElem.Elements("wordForming"))
				{
					var codepointStr = (string)charElem.Attribute("val");
					if (!string.IsNullOrEmpty(codepointStr))
					{
						int codepoint = Convert.ToInt32(codepointStr, 16);
						var c = (char)codepoint;
						characters.Add(c.ToString(CultureInfo.InvariantCulture));
					}
				}
				if (characters.Count > 0)
					staging.CharacterSets.Add("main", UnicodeSet.ToPattern(characters));
			}
		}

		private static string GetLegacyWordformingCharOverridesPath()
		{
			string path = ReflectionHelper.DirectoryOfTheApplicationExecutable;
			int index = path.ToLower().LastIndexOf(Path.DirectorySeparatorChar + "output" + Path.DirectorySeparatorChar,
				StringComparison.Ordinal);
			if (index != -1)
			{
				string parentPath = path.Substring(0, index + 1);
				path = Path.Combine(parentPath, "DistFiles");
			}
			return Path.Combine(path, "WordFormingCharOverrides.xml");
		}

		/// <summary>
		/// Parse a character set from the Fw:validChars element and add it to the staging definition
		/// </summary>
		/// <param name="validCharsElem">XElement of Fw:validChars</param>
		/// <param name="s">staging definition where the character set definition will be added</param>
		/// <param name="elementName">name of the character set to read</param>
		/// <param name="type">character set definition type</param>
		private void AddCharacterSet(XElement validCharsElem, Staging s, string elementName, string type)
		{
			const char fwDelimiter = '\uFFFC';
			const string spaceReplacement = "U+0020";

			XElement elem = validCharsElem.Element(elementName);
			if ((elem != null) && !string.IsNullOrEmpty(type)) 
			{
				var characterString = (string) elem;
				string[] characters = characterString.Replace(spaceReplacement, " ").Split(fwDelimiter);
				string characterSet = type != "numeric" ? UnicodeSet.ToPattern(characters) : string.Join("", characters);
				s.CharacterSets.Add(type, characterSet);
			}
		}

		#region FolderMigrationCode

		/// <summary>
		/// Utility to create the special element with SIL namespace
		/// </summary>
		/// <param name="element">parent element of the special element</param>
		/// <returns>XElement special</returns>
		private XElement CreateSpecialElement(XElement element)
		{
			// Create element
			var specialElem = new XElement("special");
			specialElem.SetAttributeValue(XNamespace.Xmlns + "sil", Sil);
			element.Add(specialElem);
			return specialElem;
		}

		private void WriteIdentityElement(XElement identityElem, Staging s, string ietfLanguageTagAfterMigration)
		{
			WriteLanguageTagElements(identityElem, ietfLanguageTagAfterMigration);

			// Write generation date with UTC so no more ambiguity on timezone
			identityElem.SetAttributeValue("generation", "date", DateTime.UtcNow.ToISO8601TimeFormatWithUTCString());

			// Create special element if data needs to be written
			if (!string.IsNullOrEmpty(s.WindowsLcid) || !string.IsNullOrEmpty(s.VariantName))
			{
				XElement specialElem = CreateSpecialElement(identityElem);
				XElement silIdentityElem = specialElem.GetOrCreateElement(Sil + "identity");

				silIdentityElem.SetOptionalAttributeValue("windowsLCID", s.WindowsLcid);
				silIdentityElem.SetOptionalAttributeValue("variantName", s.VariantName);
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
			else
				identityElem.Elements("script").Remove();
			if (!string.IsNullOrEmpty(region))
				identityElem.SetAttributeValue("territory", "type", region);
			else
				identityElem.Elements("territory").Remove();
			if (!string.IsNullOrEmpty(variant))
				identityElem.SetAttributeValue("variant", "type", variant);
			else
				identityElem.Elements("variant").Remove();
		}

		private void WriteCharactersElement(XElement charactersElem, Staging s)
		{
			// Convert each key value pair into character elements
			foreach (var kvp in s.CharacterSets.Where(kvp => kvp.Key != "numeric"))
			{
				// These character sets go to the normal LDML exemplarCharacters space
				// http://unicode.org/reports/tr35/tr35-general.html#Exemplars
				var exemplarCharactersElem = new XElement("exemplarCharacters", kvp.Value);
				// Assume main set doesn't have an attribute type
				if (kvp.Key != "main")
					exemplarCharactersElem.SetAttributeValue("type", kvp.Key);
				charactersElem.Add(exemplarCharactersElem);
			}
		}

		private void WriteNumbersElement(XElement numbersElem, Staging s)
		{
			// Create defaultNumberingSystem element and add as the first child
			const string defaultNumberingSystem = "standard";
			var defaultNumberingSystemElem = new XElement("defaultNumberingSystem", defaultNumberingSystem);
			numbersElem.AddFirst(defaultNumberingSystemElem);

			// Populate numbering system element
			var numberingSystemsElem = new XElement("numberingSystem");
			numberingSystemsElem.SetAttributeValue("id", defaultNumberingSystem);
			numberingSystemsElem.SetAttributeValue("type", "numeric");
			numberingSystemsElem.SetAttributeValue("digits", s.CharacterSets["numeric"]);
			numbersElem.Add(numberingSystemsElem);
		}

		private void WriteCollationsElement(XElement collationsElem, Staging s)
		{
			var defaultCollationElem = new XElement("defaultCollation", "standard");
			collationsElem.Add(defaultCollationElem);

			WriteCollationElement(collationsElem, s);
		}

		private void WriteCollationElement(XElement collationsElem, Staging s)
		{
			var collationElem = new XElement("collation", new XAttribute("type", "standard"));
			collationsElem.Add(collationElem);

			// Convert sort rules to collation definition of standard type
			switch (s.SortUsing)
			{
				case WritingSystemDefinitionV1.SortRulesType.CustomSimple:
					WriteCollationRulesFromCustomSimple(collationElem, s.SortRules);
					break;
				case WritingSystemDefinitionV1.SortRulesType.CustomICU:
					WriteCollationRulesFromCustomIcu(collationElem, s.SortRules);
					break;
				case WritingSystemDefinitionV1.SortRulesType.DefaultOrdering:
					break;
			}
		}

		private void WriteCollationRulesFromCustomIcu(XElement collationElem, string icuRules)
		{
			// If collation valid and icu rules exist, populate icu rules
			if (!string.IsNullOrEmpty(icuRules))
				collationElem.Add(new XElement("cr", new XCData(icuRules)));
		}

		private void WriteCollationRulesFromCustomSimple(XElement collationElem, string sortRules)
		{
			XElement specialElem = CreateSpecialElement(collationElem);
			// When migrating, set needsCompiling to true
			specialElem.SetAttributeValue(Sil + "needsCompiling", "true");
			specialElem.Add(new XElement(Sil + "simple", new XCData(sortRules)));
		}

		private void WriteTopLevelSpecialElements(XElement specialElem, Staging s)
		{
			XElement externalResourcesElem = specialElem.GetOrCreateElement(Sil + "external-resources");
			if (!string.IsNullOrEmpty(s.DefaultFontName))
				WriteFontElement(externalResourcesElem, s);
		}

		private void WriteFontElement(XElement externalResourcesElem, Staging s)
		{
			var fontElem = new XElement(Sil + "font");
			fontElem.SetAttributeValue("name", s.DefaultFontName);
			fontElem.SetOptionalAttributeValue("features", s.DefaultFontFeatures);

			externalResourcesElem.Add(fontElem);
		}

		public override void PostMigrate(string sourcePath, string destinationPath)
		{
			EnsureIeftLanguageTagsUnique(_migrationInfo);

			// Write them back, with their new file name.
			foreach (LdmlMigrationInfo migrationInfo in _migrationInfo)
			{
				Staging staging = _staging[migrationInfo.FileName];
				string sourceFilePath = Path.Combine(sourcePath, migrationInfo.FileName);
				string destinationFilePath = Path.Combine(destinationPath, migrationInfo.LanguageTagAfterMigration + ".ldml");

				XElement ldmlElem = XElement.Load(sourceFilePath);
				// Remove legacy palaso namespace from sourceFilePath
				ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "palaso"))).Remove();
				ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "palaso2"))).Remove();
				ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "fw"))).Remove();

				// Remove collations to repopulate later
				ldmlElem.Elements("collations").Remove();

				// Write out the elements.
				XElement identityElem = ldmlElem.Element("identity");
				WriteIdentityElement(identityElem, staging, migrationInfo.LanguageTagAfterMigration);

				var layoutElement = ldmlElem.Element("layout");
				WriteLayoutElement(layoutElement);

				if (staging.CharacterSets.ContainsKey("main") || staging.CharacterSets.ContainsKey("punctuation"))
				{
					XElement charactersElem = ldmlElem.GetOrCreateElement("characters");
					WriteCharactersElement(charactersElem, staging);
				}

				if (staging.CharacterSets.ContainsKey("numeric"))
				{
					XElement numbersElem = ldmlElem.GetOrCreateElement("numbers");
					WriteNumbersElement(numbersElem, staging);
				}

				if (staging.SortUsing != WritingSystemDefinitionV1.SortRulesType.OtherLanguage)
				{
					XElement collationsElem = ldmlElem.GetOrCreateElement("collations");
					WriteCollationsElement(collationsElem, staging);
				}

				// If needed, create top level special for external resources
				if (!string.IsNullOrEmpty(staging.DefaultFontName))
				{
					// Create special element
					XElement specialElem = CreateSpecialElement(ldmlElem);
					WriteTopLevelSpecialElements(specialElem, staging);
				}

				var writerSettings = CanonicalXmlSettings.CreateXmlWriterSettings();
				writerSettings.NewLineOnAttributes = false;
				using (var writer = XmlWriter.Create(destinationFilePath, writerSettings))
					ldmlElem.WriteTo(writer);

				if (migrationInfo.LanguageTagBeforeMigration != migrationInfo.LanguageTagAfterMigration)
					_auditLog.LogChange(migrationInfo.LanguageTagBeforeMigration, migrationInfo.LanguageTagAfterMigration);
			}
			if (_migrationHandler != null)
				_migrationHandler(ToVersion, _migrationInfo);
		}

		private void WriteLayoutElement(XElement layoutElement)
		{
			var orientation = layoutElement?.Element("orientation");
			var characterOrientationAttribute = orientation?.Attribute("characters");
			if(characterOrientationAttribute == null)
				return;
			characterOrientationAttribute.Remove();
			var characterOrientationElement = new XElement("characterOrder");
			characterOrientationElement.Value = characterOrientationAttribute.Value;
			orientation.Add(characterOrientationElement);
		}

		internal void EnsureIeftLanguageTagsUnique(IEnumerable<LdmlMigrationInfo> migrationInfo)
		{
			var uniqueLanguageTags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (LdmlMigrationInfo info in migrationInfo)
			{
				LdmlMigrationInfo currentInfo = info;
				if (uniqueLanguageTags.Contains(currentInfo.LanguageTagAfterMigration))
				{
					if (currentInfo.LanguageTagBeforeMigration.Equals(currentInfo.LanguageTagAfterMigration, StringComparison.InvariantCultureIgnoreCase))
					{
						// We want to change the other, because we are the same. Even if the other is the same, we'll change it anyway.
						LdmlMigrationInfo otherInfo = _migrationInfo.First(
							i => i.LanguageTagAfterMigration.Equals(currentInfo.LanguageTagAfterMigration, StringComparison.InvariantCultureIgnoreCase)
						);
						otherInfo.LanguageTagAfterMigration = IetfLanguageTag.ToUniqueLanguageTag(
							otherInfo.LanguageTagAfterMigration, uniqueLanguageTags);
						uniqueLanguageTags.Add(otherInfo.LanguageTagAfterMigration);
					}
					else
					{
						currentInfo.LanguageTagAfterMigration = IetfLanguageTag.ToUniqueLanguageTag(
							currentInfo.LanguageTagAfterMigration, uniqueLanguageTags);
						uniqueLanguageTags.Add(currentInfo.LanguageTagAfterMigration);
					}
				}
				else
				{
					uniqueLanguageTags.Add(currentInfo.LanguageTagAfterMigration);
				}
			}
		}

		#endregion
	}
}
