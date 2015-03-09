using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SIL.Code;
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
	public class LdmlVersion2MigrationStrategy : MigrationStrategyBase
	{
		private readonly List<MigrationInfo> _migrationInfo;
		private readonly Dictionary<string, WritingSystemDefinitionV3> _writingSystemsV3;
		private readonly Action<int, IEnumerable<MigrationInfo>> _migrationHandler;
		private readonly IAuditTrail _auditLog;

		private static readonly XNamespace Fw = "urn://fieldworks.sil.org/ldmlExtensions/v1";
		private readonly List<ICustomDataMapper> _customDataMappers;

		public LdmlVersion2MigrationStrategy(Action<int, IEnumerable<MigrationInfo>> migrationHandler, IAuditTrail auditLog, IEnumerable<ICustomDataMapper> customDataMappers) :
			base(2, 3)
		{
			Guard.AgainstNull(migrationHandler, "migrationCallback must be set");
			_migrationInfo = new List<MigrationInfo>();
			_writingSystemsV3 = new Dictionary<string, WritingSystemDefinitionV3>();
			_migrationHandler = migrationHandler;
			_auditLog = auditLog;
			_customDataMappers = customDataMappers.ToList();
		}

		public override void Migrate(string sourceFilePath, string destinationFilePath)
		{
			string sourceFileName = Path.GetFileName(sourceFilePath);

			var writingSystemDefinitionV1 = new WritingSystemDefinitionV1();
			new LdmlAdaptorV1().Read(sourceFilePath, writingSystemDefinitionV1);

			XElement ldmlElem = XElement.Load(sourceFilePath);
			XElement fwElem =
				ldmlElem.Elements("special").FirstOrDefault(e => !string.IsNullOrEmpty((string) e.Attribute(XNamespace.Xmlns + "fw")));
			
			// Remove legacy palaso namespace from sourceFilePath
			ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string)e.Attribute(XNamespace.Xmlns + "palaso"))).Remove();
			ldmlElem.Elements("special").Where(e => !string.IsNullOrEmpty((string)e.Attribute(XNamespace.Xmlns + "palaso2"))).Remove();

			// Remove empty collations and collations that contain special
			ldmlElem.Elements("collations")
				.Elements("collation")
				.Where(e => e.IsEmpty || (string.IsNullOrEmpty((string) e) && !e.HasElements && !e.HasAttributes))
				.Remove();
			ldmlElem.Elements("collations").Elements("collation").Where(e => e.Descendants("special") != null).Remove();

			var writingSystemDefinitionV3 = new WritingSystemDefinitionV3
			{
				Abbreviation = writingSystemDefinitionV1.Abbreviation,
				DefaultFontSize = writingSystemDefinitionV1.DefaultFontSize,
				Keyboard = writingSystemDefinitionV1.Keyboard,
				RightToLeftScript = writingSystemDefinitionV1.RightToLeftScript,
				SpellCheckingId = writingSystemDefinitionV1.SpellCheckingId,
				VersionDescription = writingSystemDefinitionV1.VersionDescription,
				DateModified = DateTime.Now,
				WindowsLcid = writingSystemDefinitionV1.WindowsLcid
			};

			if (!string.IsNullOrEmpty(writingSystemDefinitionV1.DefaultFontName))
			{
				var fd = new FontDefinition(writingSystemDefinitionV1.DefaultFontName);
				writingSystemDefinitionV3.Fonts.Add(fd);
				writingSystemDefinitionV3.DefaultFont = fd;
			}

			// Convert sort rules to collation definition of standard type
			CollationDefinition cd;
			switch (writingSystemDefinitionV1.SortUsing)
			{
				case WritingSystemDefinitionV1.SortRulesType.CustomSimple:
					cd = new SimpleCollationDefinition("standard") {SimpleRules = writingSystemDefinitionV1.SortRules};
					break;
				case WritingSystemDefinitionV1.SortRulesType.OtherLanguage:
					cd = new IcuCollationDefinition("standard") {Imports = {new IcuCollationImport(writingSystemDefinitionV1.Bcp47Tag)}};
					break;
				case WritingSystemDefinitionV1.SortRulesType.CustomICU:
					cd = new IcuCollationDefinition("standard") {IcuRules = writingSystemDefinitionV1.SortRules};
					break;
				default:
					cd = new IcuCollationDefinition("standard");
					break;
			}
			writingSystemDefinitionV3.Collations.Add(cd);

			writingSystemDefinitionV3.IetfLanguageTag = 
				IetfLanguageTagHelper.ToIetfLanguageTag(
				writingSystemDefinitionV1.Language,
				writingSystemDefinitionV1.Script,
				writingSystemDefinitionV1.Region,
				writingSystemDefinitionV1.Variant);

			if (!string.IsNullOrEmpty(writingSystemDefinitionV1.LanguageName))
				writingSystemDefinitionV3.Language = new LanguageSubtag(writingSystemDefinitionV3.Language, writingSystemDefinitionV1.LanguageName);

			// Migrate fields from legacy fw namespace, and then remove fw namespace
			if (fwElem != null)
			{
				ReadFwSpecialElem(fwElem, writingSystemDefinitionV3);
				fwElem.Remove();
			}

			var writerSettings = CanonicalXmlSettings.CreateXmlWriterSettings();
			writerSettings.NewLineOnAttributes = false;
			using (var writer = XmlWriter.Create(sourceFilePath, writerSettings))
				ldmlElem.WriteTo(writer);

			_writingSystemsV3[sourceFileName] = writingSystemDefinitionV3;

			// Record the details for use in PostMigrate where we change the file name to match the ieft language tag where we can.
			var migrationInfo = new MigrationInfo
				{
					FileName = sourceFileName,
					IetfLanguageTagBeforeMigration = writingSystemDefinitionV1.Bcp47Tag,
					IetfLanguageTagAfterMigration = writingSystemDefinitionV3.IetfLanguageTag
				};
			_migrationInfo.Add(migrationInfo);
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

		public void ReadFwSpecialElem(XElement fwElem, WritingSystemDefinition ws)
		{
			// DefaultFontFeatures
			XElement elem = fwElem.Element(Fw + "defaultFontFeatures");
			if ((elem != null) && ws.DefaultFont != null)
				ws.DefaultFont.Features = (string)elem.Attribute("value");

			elem = fwElem.Element(Fw + "graphiteEnabled");
			if (elem != null)
			{
				bool graphiteEnabled;
				if (bool.TryParse((string)elem.Attribute("value"), out graphiteEnabled))
					ws.IsGraphiteEnabled = graphiteEnabled;
			}

			//MatchedPairs, PunctuationPatterns, QuotationMarks deprecated

			// LegacyMapping
			elem = fwElem.Element(Fw + "legacyMapping");
			if (elem != null)
			{
				ws.LegacyMapping = (string) elem.Attribute("value");
			}

			// RegionName
			elem = fwElem.Element(Fw + "regionName");
			if (!string.IsNullOrEmpty(ws.Region) && (elem != null) && ws.Region.IsPrivateUse)
				ws.Region = new RegionSubtag(ws.Region, (string) elem.Attribute("value"));

			// ScriptName
			elem = fwElem.Element(Fw + "scriptName");
			if (!string.IsNullOrEmpty(ws.Script) && (elem != null) && ws.Script.IsPrivateUse)
				ws.Script = new ScriptSubtag(ws.Script, (string) elem.Attribute("value"));

			// VariantName
			// Intentionally only checking the first variant
			elem = fwElem.Element(Fw + "variantName");
			if (ws.Variants.Count > 0 && (elem != null) && ws.Variants[0].IsPrivateUse)
				ws.Variants[0] = new VariantSubtag(ws.Variants[0], (string) elem.Attribute("value"));

			// Valid Chars
			elem = fwElem.Element(Fw + "validChars");
			if (elem != null)
			{
				try
				{
					var validCharsElem = XElement.Parse((string) elem.Attribute("value"));
					AddCharacterSet(validCharsElem, ws, "WordForming", "main");
					AddCharacterSet(validCharsElem, ws, "Numeric", "numeric");
					AddCharacterSet(validCharsElem, ws, "Other", "punctuation");
				}
				catch (XmlException)
				{
					// Move on if fw:validChars contains invalid XML
				}
			}
		}

#region FolderMigrationCode

		public override void PostMigrate(string sourcePath, string destinationPath)
		{
			EnsureIeftLanguageTagsUnique(_migrationInfo);

			// Write them back, with their new file name.
			foreach (var migrationInfo in _migrationInfo)
			{
				var writingSystemDefinitionV3 = _writingSystemsV3[migrationInfo.FileName];
				string sourceFilePath = Path.Combine(sourcePath, migrationInfo.FileName);
				string destinationFilePath = Path.Combine(destinationPath, migrationInfo.IetfLanguageTagAfterMigration + ".ldml");
				if (migrationInfo.IetfLanguageTagBeforeMigration != migrationInfo.IetfLanguageTagAfterMigration)
				{
					_auditLog.LogChange(migrationInfo.IetfLanguageTagBeforeMigration, migrationInfo.IetfLanguageTagAfterMigration);
				}
				WriteLdml(writingSystemDefinitionV3, sourceFilePath, destinationFilePath);
				foreach (ICustomDataMapper customDataMapper in _customDataMappers)
					customDataMapper.Write(writingSystemDefinitionV3);
			}
			if (_migrationHandler != null)
			{
				_migrationHandler(ToVersion, _migrationInfo);
			}
		}

		private void WriteLdml(WritingSystemDefinitionV3 writingSystemDefinitionV3, string sourceFilePath, string destinationFilePath)
		{
			using (Stream sourceStream = new FileStream(sourceFilePath, FileMode.Open))
			{
				var ldmlDataMapper = new LdmlAdaptorV3();
				ldmlDataMapper.Write(destinationFilePath, writingSystemDefinitionV3, sourceStream);
			}
		}

		internal void EnsureIeftLanguageTagsUnique(IEnumerable<MigrationInfo> migrationInfo)
		{
			var uniqueIeftLanguageTag = new HashSet<string>();
			foreach (var info in migrationInfo)
			{
				MigrationInfo currentInfo = info;
				if (uniqueIeftLanguageTag.Any(ieftLanguageTag => ieftLanguageTag.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.OrdinalIgnoreCase)))
				{
					if (currentInfo.IetfLanguageTagBeforeMigration.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.OrdinalIgnoreCase))
					{
						// We want to change the other, because we are the same. Even if the other is the same, we'll change it anyway.
						MigrationInfo otherInfo = _migrationInfo.First(
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
