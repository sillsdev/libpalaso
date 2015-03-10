using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Migration;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	/// <summary>
	/// This class is used to migrate an LdmlFile from LDML palaso version 0 (or 1) to 2. It takes any LDML file and transforms
	/// a non-conformant rfc5646 tag contained therein into a conformant one. Note that the constructor expects a callback
	/// to help a consumer perform changes to its own files where necassary.
	/// Also note that the files are not written until all writing systems have been migrated in order to deal correctly
	/// with duplicate Rfc5646 tags that might result from migration.
	/// </summary>
	internal class LdmlVersion0MigrationStrategy : MigrationStrategyBase
	{
		private readonly List<LdmlMigrationInfo> _migrationInfo;
		private readonly Dictionary<string, WritingSystemDefinitionV1> _writingSystemsV1;
		private readonly Action<int, IEnumerable<LdmlMigrationInfo>> _migrationHandler;
		private readonly IAuditTrail _auditLog;

		public LdmlVersion0MigrationStrategy(Action<int, IEnumerable<LdmlMigrationInfo>> migrationHandler, IAuditTrail auditLog, int fromVersion) :
			base(fromVersion, 2)
		{
			_migrationInfo = new List<LdmlMigrationInfo>();
			_writingSystemsV1 = new Dictionary<string, WritingSystemDefinitionV1>();
			_migrationHandler = migrationHandler;
			_auditLog = auditLog;
		}

		public override void Migrate(string sourceFilePath, string destinationFilePath)
		{
			string sourceFileName = Path.GetFileName(sourceFilePath);

			var writingSystemDefinitionV0 = new WritingSystemDefinitionV0();
			new LdmlAdaptorV0().Read(sourceFilePath, writingSystemDefinitionV0);

			var rfcHelper = new IetfLanguageTagCleaner(
				writingSystemDefinitionV0.ISO639,
				writingSystemDefinitionV0.Script,
				writingSystemDefinitionV0.Region,
				writingSystemDefinitionV0.Variant,
				"");

			rfcHelper.Clean();

			var writingSystemDefinitionV1 = new WritingSystemDefinitionV1
				{
					DefaultFontName = writingSystemDefinitionV0.DefaultFontName,
					Abbreviation = writingSystemDefinitionV0.Abbreviation,
					DefaultFontSize = writingSystemDefinitionV0.DefaultFontSize,
					IsUnicodeEncoded = !writingSystemDefinitionV0.IsLegacyEncoded,
					Keyboard = writingSystemDefinitionV0.Keyboard,
					LanguageName = writingSystemDefinitionV0.LanguageName,
					RightToLeftScript = writingSystemDefinitionV0.RightToLeftScript,
					SortRules = writingSystemDefinitionV0.SortRules,
					SortUsing = (WritingSystemDefinitionV1.SortRulesType) writingSystemDefinitionV0.SortUsing,
					SpellCheckingId = writingSystemDefinitionV0.SpellCheckingId,
					VersionDescription = writingSystemDefinitionV0.VersionDescription,
					DateModified = DateTime.Now
				};

			writingSystemDefinitionV1.SetAllComponents(
				rfcHelper.Language,
				rfcHelper.Script,
				rfcHelper.Region,
				ConcatenateVariantAndPrivateUse(rfcHelper.Variant, rfcHelper.PrivateUse)
			);
			_writingSystemsV1[sourceFileName] = writingSystemDefinitionV1;
			//_migratedWs.VerboseDescription //not written out by LdmlAdaptorV1 - flex?
			//_migratedWs.NativeName //not written out by LdmlAdaptorV1 - flex?);

			// Record the details for use in PostMigrate where we change the file name to match the rfc tag where we can.
			var migrationInfo = new LdmlMigrationInfo(sourceFileName)
				{
					IetfLanguageTagBeforeMigration = writingSystemDefinitionV0.Rfc5646,
					IetfLanguageTagAfterMigration = writingSystemDefinitionV1.Bcp47Tag
				};
			_migrationInfo.Add(migrationInfo);
		}

		private static string ConcatenateVariantAndPrivateUse(string variant, string privateUse)
		{
			string concatenatedTags = variant;
			if (!String.IsNullOrEmpty(concatenatedTags))
			{
				concatenatedTags += "-";
			}
			if (!String.IsNullOrEmpty(privateUse))
			{
				concatenatedTags += "x-";
				concatenatedTags += privateUse;
			}
			return concatenatedTags;
		}



#region FolderMigrationCode

		public override void PostMigrate(string sourcePath, string destinationPath)
		{
			EnsureRfcTagsUnique(_migrationInfo);

			// Write them back, with their new file name.
			foreach (var migrationInfo in _migrationInfo)
			{
				var writingSystemDefinitionV1 = _writingSystemsV1[migrationInfo.FileName];
				string sourceFilePath = Path.Combine(sourcePath, migrationInfo.FileName);
				string destinationFilePath = Path.Combine(destinationPath, migrationInfo.IetfLanguageTagAfterMigration + ".ldml");
				if (migrationInfo.IetfLanguageTagBeforeMigration != migrationInfo.IetfLanguageTagAfterMigration)
					_auditLog.LogChange(migrationInfo.IetfLanguageTagBeforeMigration, migrationInfo.IetfLanguageTagAfterMigration);
				WriteLdml(writingSystemDefinitionV1, sourceFilePath, destinationFilePath);
			}
			if (_migrationHandler != null)
				_migrationHandler(ToVersion, _migrationInfo);
		}

		private void WriteLdml(WritingSystemDefinitionV1 writingSystemDefinitionV1, string sourceFilePath, string destinationFilePath)
		{
			using (Stream sourceStream = new FileStream(sourceFilePath, FileMode.Open))
			{
				var ldmlDataMapper = new LdmlAdaptorV1();
				ldmlDataMapper.Write(destinationFilePath, writingSystemDefinitionV1, sourceStream);
				sourceStream.Close();
			}
		}

		internal void EnsureRfcTagsUnique(IEnumerable<LdmlMigrationInfo> migrationInfo)
		{
			var uniqueRfcTags = new HashSet<string>();
			foreach (var info in migrationInfo)
			{
				LdmlMigrationInfo currentInfo = info;
				if (uniqueRfcTags.Any(rfcTag => rfcTag.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.OrdinalIgnoreCase)))
				{
					if (currentInfo.IetfLanguageTagBeforeMigration.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.OrdinalIgnoreCase))
					{
						// We want to change the other, because we are the same. Even if the other is the same, we'll change it anyway.
						LdmlMigrationInfo otherInfo = _migrationInfo.First(
							i => i.IetfLanguageTagAfterMigration.Equals(currentInfo.IetfLanguageTagAfterMigration, StringComparison.OrdinalIgnoreCase)
						);
						otherInfo.IetfLanguageTagAfterMigration = UniqueTagForDuplicate(otherInfo.IetfLanguageTagAfterMigration, uniqueRfcTags);
						uniqueRfcTags.Add(otherInfo.IetfLanguageTagAfterMigration);
						var writingSystemV1 = _writingSystemsV1[otherInfo.FileName];
						writingSystemV1.SetTagFromString(otherInfo.IetfLanguageTagAfterMigration);
					}
					else
					{
						currentInfo.IetfLanguageTagAfterMigration = UniqueTagForDuplicate(currentInfo.IetfLanguageTagAfterMigration, uniqueRfcTags);
						uniqueRfcTags.Add(currentInfo.IetfLanguageTagAfterMigration);
						var writingSystemV1 = _writingSystemsV1[currentInfo.FileName];
						writingSystemV1.SetTagFromString(currentInfo.IetfLanguageTagAfterMigration);
					}
				}
				else
				{
					uniqueRfcTags.Add(currentInfo.IetfLanguageTagAfterMigration);
				}
			}
		}

		private static string UniqueTagForDuplicate(string rfcTag, IEnumerable<string> uniqueRfcTags)
		{
			Rfc5646Tag tag = Rfc5646Tag.Parse(rfcTag);
			string originalPrivateUse = tag.PrivateUse;
			int duplicateNumber = 0;
			do
			{
				tag.PrivateUse = originalPrivateUse;
				tag.AddToPrivateUse(String.Format("dupl{0}", duplicateNumber));
				duplicateNumber++;
			} while (uniqueRfcTags.Any(s => s.Equals(tag.CompleteTag, StringComparison.OrdinalIgnoreCase)));
			return tag.CompleteTag;
		}

		#endregion


	}
}
