using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palaso.Code;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	/// <summary>
	/// This class is used to migrate an LdmlFile from LDML palaso version 0 to 1. It takes any LDML file and transforms
	/// a non-conformant rfc5646 tag contained therein into a conformant one. Note that the constructor expects a callback
	/// to help a consumer perform changes to its own files where necassary.
	/// Also note that the files are not written until all writing systems have been migrated in order to deal correctly
	/// with duplicate Rfc5646 tags that might result from migration.
	/// </summary>
	public class LdmlVersion0MigrationStrategy : MigrationStrategyBase
	{
		private static class WellKnownSubTags
		{
			private static class Unwritten
			{
				public const string Script = "Zxxx";
			}

			public static class Audio
			{
				public const string PrivateUseSubtag = "audio";
				public const string Script = Unwritten.Script;
			}
		}

		public class MigrationInfo
		{
			public string FileName;
			public string RfcTagBeforeMigration;
			public string RfcTagAfterMigration;
		}

		public delegate void OnMigrationFn(IEnumerable<MigrationInfo> migrationInfo);

		private readonly List<MigrationInfo> _migrationInfo;
		private readonly Dictionary<string, WritingSystemDefinitionV1> _writingSystemsV1;
		private readonly OnMigrationFn _onMigrationCallback;
		private IAuditTrail _auditLog;

		public LdmlVersion0MigrationStrategy(OnMigrationFn onMigrationCallback, IAuditTrail auditLog) :
			base(0, 1)
		{
			Guard.AgainstNull(onMigrationCallback, "onMigrationCallback must be set");
			_migrationInfo = new List<MigrationInfo>();
			_writingSystemsV1 = new Dictionary<string, WritingSystemDefinitionV1>();
			_onMigrationCallback = onMigrationCallback;
			_auditLog = auditLog;
		}

		public override void Migrate(string sourceFilePath, string destinationFilePath)
		{
			string sourceFileName = Path.GetFileName(sourceFilePath);

			var writingSystemDefinitionV0 = new WritingSystemDefinitionV0();
			new LdmlAdaptorV0().Read(sourceFilePath, writingSystemDefinitionV0);

			var rfcHelper = new Rfc5646TagCleaner(
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
					IsLegacyEncoded = writingSystemDefinitionV0.IsLegacyEncoded,
					Keyboard = writingSystemDefinitionV0.Keyboard,
					LanguageName = writingSystemDefinitionV0.LanguageName,
					RightToLeftScript = writingSystemDefinitionV0.RightToLeftScript,
					SortRules = writingSystemDefinitionV0.SortRules,
					SortUsing = (WritingSystemDefinitionV1.SortRulesType)writingSystemDefinitionV0.SortUsing,
					SpellCheckingId = writingSystemDefinitionV0.SpellCheckingId,
					VersionDescription = writingSystemDefinitionV0.VersionDescription,
					DateModified = DateTime.Now
				};

			writingSystemDefinitionV1.SetAllRfc5646LanguageTagComponents(
				rfcHelper.Language,
				rfcHelper.Script,
				rfcHelper.Region,
				ConcatenateVariantAndPrivateUse(rfcHelper.Variant, rfcHelper.PrivateUse)
			);
			_writingSystemsV1[sourceFileName] = writingSystemDefinitionV1;
			//_migratedWs.VerboseDescription //not written out by LdmlAdaptorV1 - flex?
			//_migratedWs.NativeName //not written out by LdmlAdaptorV1 - flex?);

			// Record the details for use in PostMigrate where we change the file name to match the rfc tag where we can.
			var migrationInfo = new MigrationInfo
				{
					FileName = sourceFileName,
					RfcTagBeforeMigration = writingSystemDefinitionV0.Rfc5646,
					RfcTagAfterMigration = writingSystemDefinitionV1.RFC5646
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
				string destinationFilePath = Path.Combine(destinationPath, migrationInfo.RfcTagAfterMigration + ".ldml");
				if (migrationInfo.RfcTagBeforeMigration != migrationInfo.RfcTagAfterMigration)
				{
					_auditLog.LogChange(migrationInfo.RfcTagBeforeMigration, migrationInfo.RfcTagAfterMigration);
				}
				WriteLdml(writingSystemDefinitionV1, sourceFilePath, destinationFilePath);
			}
			if (_onMigrationCallback != null)
			{
				_onMigrationCallback(_migrationInfo);
			}
		}

		private static void WriteLdml(WritingSystemDefinitionV1 writingSystemDefinitionV1, string sourceFilePath, string destinationFilePath)
		{
			using (Stream sourceStream = new FileStream(sourceFilePath, FileMode.Open))
			{
				var ldmlDataMapper = new LdmlAdaptorV1();
				ldmlDataMapper.Write(destinationFilePath, writingSystemDefinitionV1, sourceStream);
				sourceStream.Close();
			}
		}

		internal void EnsureRfcTagsUnique(IEnumerable<MigrationInfo> migrationInfo)
		{
			var uniqueRfcTags = new HashSet<string>();
			foreach (var info in migrationInfo)
			{
				MigrationInfo currentInfo = info;
				if (uniqueRfcTags.Any(rfcTag => rfcTag.Equals(currentInfo.RfcTagAfterMigration, StringComparison.OrdinalIgnoreCase)))
				{
					if (currentInfo.RfcTagBeforeMigration.Equals(currentInfo.RfcTagAfterMigration, StringComparison.OrdinalIgnoreCase))
					{
						// We want to change the other, because we are the same. Even if the other is the same, we'll change it anyway.
						MigrationInfo otherInfo = _migrationInfo.First(
							i => i.RfcTagAfterMigration.Equals(currentInfo.RfcTagAfterMigration, StringComparison.OrdinalIgnoreCase)
						);
						otherInfo.RfcTagAfterMigration = UniqueTagForDuplicate(otherInfo.RfcTagAfterMigration, uniqueRfcTags);
						uniqueRfcTags.Add(otherInfo.RfcTagAfterMigration);
						var writingSystemV1 = _writingSystemsV1[otherInfo.FileName];
						writingSystemV1.SetRfc5646FromString(otherInfo.RfcTagAfterMigration);
					}
					else
					{
						currentInfo.RfcTagAfterMigration = UniqueTagForDuplicate(currentInfo.RfcTagAfterMigration, uniqueRfcTags);
						uniqueRfcTags.Add(currentInfo.RfcTagAfterMigration);
						var writingSystemV1 = _writingSystemsV1[currentInfo.FileName];
						writingSystemV1.SetRfc5646FromString(currentInfo.RfcTagAfterMigration);
					}
				}
				else
				{
					uniqueRfcTags.Add(currentInfo.RfcTagAfterMigration);
				}
			}
		}

		private static string UniqueTagForDuplicate(string rfcTag, IEnumerable<string> uniqueRfcTags)
		{
			RFC5646Tag tag = RFC5646Tag.Parse(rfcTag);
			string originalPrivateUse = tag.PrivateUse;
			int duplicateNumber = 0;
			do
			{
				duplicateNumber++;
				tag.PrivateUse = originalPrivateUse;
				tag.AddToPrivateUse(String.Format("dupl{0}", duplicateNumber));
			} while (uniqueRfcTags.Any(s => s.Equals(tag.CompleteTag, StringComparison.OrdinalIgnoreCase)));
			return tag.CompleteTag;
		}

		#endregion


	}
}
