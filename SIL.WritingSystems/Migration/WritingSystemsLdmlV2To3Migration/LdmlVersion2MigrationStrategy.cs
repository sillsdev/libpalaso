using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
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
		private readonly Dictionary<string, WritingSystemDefinitionV3> _writingSystemsV3;
		private readonly Action<int, IEnumerable<LdmlMigrationInfo>> _migrationHandler;
		private readonly IAuditTrail _auditLog;

		private static readonly XNamespace FW = "urn://fieldworks.sil.org/ldmlExtensions/v1";

		public LdmlVersion2MigrationStrategy(Action<int, IEnumerable<LdmlMigrationInfo>> migrationHandler, IAuditTrail auditLog) :
			base(2, 3)
		{
			_migrationInfo = new List<LdmlMigrationInfo>();
			_writingSystemsV3 = new Dictionary<string, WritingSystemDefinitionV3>();
			_migrationHandler = migrationHandler;
			_auditLog = auditLog;
		}

		public override void Migrate(string sourceFilePath, string destinationFilePath)
		{
			string sourceFileName = Path.GetFileName(sourceFilePath);

			var writingSystemDefinitionV1 = new WritingSystemDefinitionV1();
			new LdmlAdaptorV1().Read(sourceFilePath, writingSystemDefinitionV1);
			XElement ldmlElem = XElement.Load(sourceFilePath);

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

			XElement fwElem = ldmlElem.Elements("special").FirstOrDefault(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "fw")));

			// Migrate fields from legacy fw namespace, and then remove fw namespace
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
				// Intentionally only checking the first variant
				XElement variantNameElem = fwElem.Element(FW + "variantName");
				if (variantNameElem != null)
					variantName = (string) variantNameElem.Attribute("value");

				fwElem.Remove();
			}
			
			// Remove legacy palaso namespace from sourceFilePath
			ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string)e.Attribute(XNamespace.Xmlns + "palaso"))).Remove();
			ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string)e.Attribute(XNamespace.Xmlns + "palaso2"))).Remove();

			// Remove empty collations and collations that contain special
			ldmlElem.Elements("collations")
				.Elements("collation")
				.Where(e => e.IsEmpty || (string.IsNullOrEmpty((string) e) && !e.HasElements && !e.HasAttributes))
				.Remove();
			ldmlElem.Elements("collations").Elements("collation").Where(e => e.Descendants("special") != null).Remove();

			var writerSettings = CanonicalXmlSettings.CreateXmlWriterSettings();
			writerSettings.NewLineOnAttributes = false;
			using (var writer = XmlWriter.Create(sourceFilePath, writerSettings))
				ldmlElem.WriteTo(writer);

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
						if (!string.IsNullOrEmpty(variantName))
						{
							int index = ws.Variants.IndexOf(v => v.IsPrivateUse);
							if (index > -1)
								ws.Variants[index] = new VariantSubtag(ws.Variants[index], variantName);
						}
					}
				};

			_migrationInfo.Add(migrationInfo);

			// misc properties
			var writingSystemDefinitionV3 = new WritingSystemDefinitionV3
			{
				RightToLeftScript = writingSystemDefinitionV1.RightToLeftScript,
				VersionDescription = writingSystemDefinitionV1.VersionDescription,
				WindowsLcid = writingSystemDefinitionV1.WindowsLcid,
				DateModified = DateTime.Now,
			};

			// font
			FontDefinition fd = null;
			if (!string.IsNullOrEmpty(defaultFontName))
			{
				fd = new FontDefinition(writingSystemDefinitionV1.DefaultFontName);
				writingSystemDefinitionV3.Fonts.Add(fd);
			}

			// Convert sort rules to collation definition of standard type
			CollationDefinition cd = null;
			switch (writingSystemDefinitionV1.SortUsing)
			{
				case WritingSystemDefinitionV1.SortRulesType.CustomSimple:
					cd = new SimpleCollationDefinition("standard") {SimpleRules = writingSystemDefinitionV1.SortRules};
					break;
				case WritingSystemDefinitionV1.SortRulesType.OtherLanguage:
					if (!string.IsNullOrEmpty(writingSystemDefinitionV1.SortRules) && writingSystemDefinitionV1.SortRules != langTag)
						cd = new IcuCollationDefinition("standard") {Imports = {new IcuCollationImport(writingSystemDefinitionV1.SortRules)}};
					break;
				case WritingSystemDefinitionV1.SortRulesType.CustomICU:
					cd = new IcuCollationDefinition("standard") {IcuRules = writingSystemDefinitionV1.SortRules};
					break;
				case WritingSystemDefinitionV1.SortRulesType.DefaultOrdering:
					cd = new IcuCollationDefinition("standard");
					break;
			}
			if (cd != null)
				writingSystemDefinitionV3.DefaultCollation = cd;

			// IETF language tag
			writingSystemDefinitionV3.IetfLanguageTag = langTag;

			if (fwElem != null)
			{
				// DefaultFontFeatures
				XElement fontFeatsElem = fwElem.Element(FW + "defaultFontFeatures");
				if (fontFeatsElem != null && fd != null)
					fd.Features = (string) fontFeatsElem.Attribute("value");

				//MatchedPairs, PunctuationPatterns, QuotationMarks deprecated

				// Valid Chars
				XElement validCharsElem = fwElem.Element(FW + "validChars");
				if (validCharsElem != null)
				{
					try
					{
						var fwValidCharsElem = XElement.Parse((string) validCharsElem.Attribute("value"));
						AddCharacterSet(fwValidCharsElem, writingSystemDefinitionV3, "WordForming", "main");
						AddCharacterSet(fwValidCharsElem, writingSystemDefinitionV3, "Numeric", "numeric");
						AddCharacterSet(fwValidCharsElem, writingSystemDefinitionV3, "Other", "punctuation");
					}
					catch (XmlException)
					{
						// Move on if fw:validChars contains invalid XML
					}
				}
			}

			_writingSystemsV3[sourceFileName] = writingSystemDefinitionV3;
		}

		/// <summary>
		/// Parse a character set from the Fw:validChars element and add it to the writing system definition
		/// </summary>
		/// <param name="validCharsElem">XElement of Fw:validChars</param>
		/// <param name="ws">writing system definition where the character set definition will be added</param>
		/// <param name="elementName">name of the character set to read</param>
		/// <param name="type">character set definition type</param>
		private void AddCharacterSet(XElement validCharsElem, WritingSystemDefinition ws, string elementName, string type)
		{
			const char fwDelimiter = '\uFFFC';

			XElement elem = validCharsElem.Element(elementName);
			if ((elem != null) && !string.IsNullOrEmpty(type)) 
			{
				var characterString = (string)elem;
				var csd = new CharacterSetDefinition(type);
				foreach (var c in characterString.Split(fwDelimiter))
					csd.Characters.Add(c);
				ws.CharacterSets.Add(csd);
			}
		}

#region FolderMigrationCode

		public override void PostMigrate(string sourcePath, string destinationPath)
		{
			EnsureIeftLanguageTagsUnique(_migrationInfo);

			// Write them back, with their new file name.
			foreach (LdmlMigrationInfo migrationInfo in _migrationInfo)
			{
				var writingSystemDefinitionV3 = _writingSystemsV3[migrationInfo.FileName];
				string sourceFilePath = Path.Combine(sourcePath, migrationInfo.FileName);
				string destinationFilePath = Path.Combine(destinationPath, migrationInfo.IetfLanguageTagAfterMigration + ".ldml");
				if (migrationInfo.IetfLanguageTagBeforeMigration != migrationInfo.IetfLanguageTagAfterMigration)
					_auditLog.LogChange(migrationInfo.IetfLanguageTagBeforeMigration, migrationInfo.IetfLanguageTagAfterMigration);
				WriteLdml(writingSystemDefinitionV3, sourceFilePath, destinationFilePath);
			}
			if (_migrationHandler != null)
				_migrationHandler(ToVersion, _migrationInfo);
		}

		private void WriteLdml(WritingSystemDefinitionV3 writingSystemDefinitionV3, string sourceFilePath, string destinationFilePath)
		{
			using (Stream sourceStream = new FileStream(sourceFilePath, FileMode.Open))
			{
				var ldmlDataMapper = new LdmlAdaptorV3();
				ldmlDataMapper.Write(destinationFilePath, writingSystemDefinitionV3, sourceStream);
			}
		}

		internal void EnsureIeftLanguageTagsUnique(IEnumerable<LdmlMigrationInfo> migrationInfo)
		{
			var uniqueIeftLanguageTag = new HashSet<string>();
			foreach (LdmlMigrationInfo info in migrationInfo)
			{
				LdmlMigrationInfo currentInfo = info;
				if (uniqueIeftLanguageTag.Any(ieftLanguageTag => ieftLanguageTag.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.OrdinalIgnoreCase)))
				{
					if (currentInfo.IetfLanguageTagBeforeMigration.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.OrdinalIgnoreCase))
					{
						// We want to change the other, because we are the same. Even if the other is the same, we'll change it anyway.
						LdmlMigrationInfo otherInfo = _migrationInfo.First(
							i => i.IetfLanguageTagAfterMigration.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.OrdinalIgnoreCase)
						);
						var writingSystemV3 = _writingSystemsV3[otherInfo.FileName];
						writingSystemV3.MakeIetfLanguageTagUnique(uniqueIeftLanguageTag);
						otherInfo.IetfLanguageTagAfterMigration = writingSystemV3.IetfLanguageTag;
						uniqueIeftLanguageTag.Add(otherInfo.IetfLanguageTagAfterMigration);
					}
					else
					{
						var writingSystemV3 = _writingSystemsV3[currentInfo.FileName];
						writingSystemV3.MakeIetfLanguageTagUnique(uniqueIeftLanguageTag);
						currentInfo.IetfLanguageTagAfterMigration = writingSystemV3.IetfLanguageTag;
						uniqueIeftLanguageTag.Add(currentInfo.IetfLanguageTagAfterMigration);
					}
				}
				else
				{
					uniqueIeftLanguageTag.Add(currentInfo.IetfLanguageTagAfterMigration);
				}
			}
		}

		#endregion
	}
}
