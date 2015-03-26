using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Icu;
using SIL.Extensions;
using SIL.Migration;
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
			IetfLanguageTagHelper.SplitVariantAndPrivateUse(writingSystemDefinitionV1.Variant, out variant, out privateUse);
			var langTagCleaner = new IetfLanguageTagCleaner(writingSystemDefinitionV1.Language, writingSystemDefinitionV1.Script, writingSystemDefinitionV1.Region,
				variant, privateUse);
			langTagCleaner.Clean();
			string langTag = IetfLanguageTagHelper.Canonicalize(langTagCleaner.GetCompleteTag());
			bool isGraphiteEnabled = false;
			string legacyMapping = string.Empty;
			string scriptName = string.Empty;
			string regionName = string.Empty;
			string variantName = string.Empty;

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
					IetfLanguageTagBeforeMigration = writingSystemDefinitionV1.Bcp47Tag,
					IetfLanguageTagAfterMigration = langTag,
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
				// Parse language tag to obtain variant subtags
				string language, script, region, variants;
				IetfLanguageTagHelper.TryGetParts(migrationInfo.IetfLanguageTagAfterMigration, out language, out script, out region,
					out variants);
				IEnumerable<VariantSubtag> variantSubtags;
				IetfLanguageTagHelper.TryGetVariantSubtags(variants, out variantSubtags);
				int index = IetfLanguageTagHelper.GetIndexOfFirstNonCommonPrivateUseVariant(variantSubtags);
				if (index > -1)
					staging.VariantName = variantName;
			}

			// known keyboards
			foreach (KeyboardDefinitionV1 keyboardV1 in writingSystemDefinitionV1.KnownKeyboards)
			{
				string id = string.IsNullOrEmpty(keyboardV1.Locale) ? keyboardV1.Layout : string.Format("{0}_{1}", keyboardV1.Locale, keyboardV1.Layout);
				staging.KnownKeyboardIds.Add(id);
			}

			// IETF language tag
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
						// Move on if fw:validChars contains invalid XML
					}
				}
			}

			_staging[sourceFileName] = staging;
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

			XElement elem = validCharsElem.Element(elementName);
			if ((elem != null) && !string.IsNullOrEmpty(type)) 
			{
				var characterString = (string)elem;
				var characters = characterString.Split(fwDelimiter).ToList();
				String characterSet;
				if (type != "numeric")
					characterSet = UnicodeSet.ToPattern(characters);
				else
					characterSet = string.Join("", characters);
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
			IetfLanguageTagHelper.TryGetParts(languageTag, out language, out script, out region, out variant);

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

		private void WriteCollationsElement(XElement collationsElem, Staging s, string langTag)
		{
			var defaultCollationElem = new XElement("defaultCollation", "standard");
			collationsElem.Add(defaultCollationElem);

			WriteCollationElement(collationsElem, s, langTag);
		}

		private void WriteCollationElement(XElement collationsElem, Staging s, string langTag)
		{
			var collationElem = new XElement("collation", new XAttribute("type", "standard"));
			collationsElem.Add(collationElem);

			// Convert sort rules to collation definition of standard type
			switch (s.SortUsing)
			{
				case WritingSystemDefinitionV1.SortRulesType.CustomSimple:
					WriteCollationRulesFromCustomSimple(collationElem, s.SortRules);
					break;
				case WritingSystemDefinitionV1.SortRulesType.OtherLanguage:
					// SortRules will contain the language tag to import
					if (!string.IsNullOrEmpty(s.SortRules) && s.SortRules != langTag)
						WriteImportElement(collationElem, s.SortRules);
					break;
				case WritingSystemDefinitionV1.SortRulesType.CustomICU:
					WriteCollationRulesFromCustomIcu(collationElem, s.SortRules);
					break;
				case WritingSystemDefinitionV1.SortRulesType.DefaultOrdering:
					break;
			}
		}

		private void WriteImportElement(XElement collationElem, string tag)
		{
			var importElem = new XElement("import", new XAttribute("source", tag));
			// Leave type blank.  Implied to be "standard"
			collationElem.Add(importElem);
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
			WriteKeyboardElement(externalResourcesElem, s);
		}

		private void WriteFontElement(XElement externalResourcesElem, Staging s)
		{
			var fontElem = new XElement(Sil + "font");
			fontElem.SetAttributeValue("name", s.DefaultFontName);
			fontElem.SetOptionalAttributeValue("features", s.DefaultFontFeatures);

			externalResourcesElem.Add(fontElem);
		}

		private void WriteKeyboardElement(XElement externalResourcesElem, Staging s)
		{
			foreach (string id in s.KnownKeyboardIds)
			{
				var kbdElem = new XElement(Sil + "kbd");
				// id required
				kbdElem.SetAttributeValue("id", id);
				externalResourcesElem.Add(kbdElem);
			}
		}

		public override void PostMigrate(string sourcePath, string destinationPath)
		{
			EnsureIeftLanguageTagsUnique(_migrationInfo);

			// Write them back, with their new file name.
			foreach (LdmlMigrationInfo migrationInfo in _migrationInfo)
			{
				Staging staging = _staging[migrationInfo.FileName];
				string sourceFilePath = Path.Combine(sourcePath, migrationInfo.FileName);
				string destinationFilePath = Path.Combine(destinationPath, migrationInfo.IetfLanguageTagAfterMigration + ".ldml");

				XElement ldmlElem = XElement.Load(sourceFilePath);
				// Remove legacy palaso namespace from sourceFilePath
				ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "palaso"))).Remove();
				ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "palaso2"))).Remove();
				ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "fw"))).Remove();

				// Remove collations to repopulate later
				ldmlElem.Elements("collations").Remove();

				// Write out the elements.
				XElement identityElem = ldmlElem.Element("identity");
				WriteIdentityElement(identityElem, staging, migrationInfo.IetfLanguageTagAfterMigration);

				if (staging.CharacterSets.ContainsKey("numeric"))
				{
					XElement numbersElem = ldmlElem.GetOrCreateElement("numbers");
					WriteNumbersElement(numbersElem, staging);
				}

				if (staging.CharacterSets.ContainsKey("main") || staging.CharacterSets.ContainsKey("punctuation"))
				{
					XElement charactersElem = ldmlElem.GetOrCreateElement("characters");
					WriteCharactersElement(charactersElem, staging);
				}

				XElement collationsElem = ldmlElem.GetOrCreateElement("collations");
				WriteCollationsElement(collationsElem, staging, migrationInfo.IetfLanguageTagAfterMigration);

				// If needed, create top level special for external resources
				if (!string.IsNullOrEmpty(staging.DefaultFontName) || (staging.KnownKeyboardIds.Count > 0))
				{
					// Create special element
					XElement specialElem = CreateSpecialElement(ldmlElem);
					WriteTopLevelSpecialElements(specialElem, staging);
				}

				var writerSettings = CanonicalXmlSettings.CreateXmlWriterSettings();
				writerSettings.NewLineOnAttributes = false;
				using (var writer = XmlWriter.Create(destinationFilePath, writerSettings))
					ldmlElem.WriteTo(writer);

				if (migrationInfo.IetfLanguageTagBeforeMigration != migrationInfo.IetfLanguageTagAfterMigration)
					_auditLog.LogChange(migrationInfo.IetfLanguageTagBeforeMigration, migrationInfo.IetfLanguageTagAfterMigration);
			}
			if (_migrationHandler != null)
				_migrationHandler(ToVersion, _migrationInfo);
		}

		internal void EnsureIeftLanguageTagsUnique(IEnumerable<LdmlMigrationInfo> migrationInfo)
		{
			var uniqueIeftLanguageTags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (LdmlMigrationInfo info in migrationInfo)
			{
				LdmlMigrationInfo currentInfo = info;
				if (uniqueIeftLanguageTags.Contains(currentInfo.IetfLanguageTagAfterMigration))
				{
					if (currentInfo.IetfLanguageTagBeforeMigration.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.InvariantCultureIgnoreCase))
					{
						// We want to change the other, because we are the same. Even if the other is the same, we'll change it anyway.
						LdmlMigrationInfo otherInfo = _migrationInfo.First(
							i => i.IetfLanguageTagAfterMigration.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.InvariantCultureIgnoreCase)
						);
						otherInfo.IetfLanguageTagAfterMigration = IetfLanguageTagHelper.ToUniqueIetfLanguageTag(
							otherInfo.IetfLanguageTagAfterMigration, uniqueIeftLanguageTags);
						uniqueIeftLanguageTags.Add(otherInfo.IetfLanguageTagAfterMigration);
					}
					else
					{
						currentInfo.IetfLanguageTagAfterMigration = IetfLanguageTagHelper.ToUniqueIetfLanguageTag(
							currentInfo.IetfLanguageTagAfterMigration, uniqueIeftLanguageTags);
						uniqueIeftLanguageTags.Add(currentInfo.IetfLanguageTagAfterMigration);
					}
				}
				else
				{
					uniqueIeftLanguageTags.Add(currentInfo.IetfLanguageTagAfterMigration);
				}
			}
		}

		#endregion
	}
}
