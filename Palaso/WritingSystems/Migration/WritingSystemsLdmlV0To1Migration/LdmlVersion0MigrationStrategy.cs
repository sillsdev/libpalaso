using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{

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

		internal class SubTag
		{
			private List<string> _subTagParts;

			public SubTag()
			{
				_subTagParts = new List<string>();
			}

			public SubTag(string partsToAdd) :
				this()
			{
				AddToSubtag(partsToAdd);
			}

			public SubTag(SubTag rhs)
			{
				_subTagParts = new List<string>(rhs._subTagParts);
			}

			public bool IsEmpty
			{
				get { return _subTagParts.Count == 0; }
			}

			public int Count
			{
				get { return _subTagParts.Count; }
			}

			public string CompleteTag
			{
				get
				{
					if (_subTagParts.Count == 0)
					{
						return String.Empty;
					}
					string subtagAsString = "";
					foreach (string part in _subTagParts)
					{
						if (!String.IsNullOrEmpty(subtagAsString))
						{
							subtagAsString = subtagAsString + "-";
						}
						subtagAsString = subtagAsString + part;
					}
					return subtagAsString;
				}
			}

			public IEnumerable<string> AllParts
			{
				get { return _subTagParts; }
			}

			public void RemoveNonAlphaNumericCharacters()
			{
				for (int i = _subTagParts.Count - 1; i >= 0; i--)
				{
					if (_subTagParts[i].Any(c => !Char.IsLetterOrDigit(c)))
					{
						_subTagParts[i] = new String(_subTagParts[i].Where(Char.IsLetterOrDigit).ToArray());
					}
					if (String.IsNullOrEmpty(_subTagParts[i]))
					{
						_subTagParts.RemoveAt(i);
					}
				}
			}

			public static List<string> ParseSubtagForParts(string subtagToParse)
			{
				var parts = new List<string>();
				parts.AddRange(subtagToParse.Split('-'));
				parts.RemoveAll(str => str == "");
				return parts;
			}

			public void TruncatePartsToNumCharacters(int size)
			{
				// Note: This does not deal with duplicates introduced through truncation.
				for (int i = 0; i < _subTagParts.Count; i++)
				{
					if (_subTagParts[i].Length > size)
					{
						_subTagParts[i] = _subTagParts[i].Substring(0, size);
					}
				}
			}

			public void AddToSubtag(string partsToAdd)
			{
				List<string> partsOfStringToAdd = ParseSubtagForParts(partsToAdd);
				foreach (string part in partsOfStringToAdd)
				{
					_subTagParts.Add(part);
				}
			}

			public void RemoveParts(string partsToRemove)
			{
				List<string> partsOfStringToRemove = ParseSubtagForParts(partsToRemove);

				foreach (string partToRemove in partsOfStringToRemove)
				{
					string part = partToRemove;
					if (!Contains(part))
					{
						continue;
					}
					int indexOfPartToRemove = _subTagParts.FindLastIndex(partInSubtag => partInSubtag.Equals(part, StringComparison.OrdinalIgnoreCase));
					_subTagParts.RemoveAt(indexOfPartToRemove);
				}
			}

			public bool Contains(string partToFind)
			{
				return _subTagParts.Any(part => part.Equals(partToFind, StringComparison.OrdinalIgnoreCase));
			}

			public void KeepFirstAndMoveRemainderTo(SubTag tagToMoveTo)
			{
				for (int i = _subTagParts.Count - 1; i > 0; i--)
				{
					tagToMoveTo.AddToSubtag(_subTagParts[i]);
					_subTagParts.RemoveAt(i);
				}
			}

			public void RemoveDuplicates()
			{
				var tags = new List<string>();
				foreach (var tag in _subTagParts.Where(tag => !tags.Contains(tag, StringComparison.OrdinalIgnoreCase)))
				{
					tags.Add(tag);
				}
				_subTagParts = tags;
			}

			public override string ToString()
			{
				return CompleteTag;
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

		public LdmlVersion0MigrationStrategy(OnMigrationFn onMigrationCallback) :
			base(0, 1)
		{
			_migrationInfo = new List<MigrationInfo>();
			_writingSystemsV1 = new Dictionary<string, WritingSystemDefinitionV1>();
			_onMigrationCallback = onMigrationCallback;
		}

		public override void Migrate(string sourceFilePath, string destinationFilePath)
		{
			string sourceFileName = Path.GetFileName(sourceFilePath);

			var writingSystemDefinitionV0 = new WritingSystemDefinitionV0();
			new LdmlAdaptorV0().Read(sourceFilePath, writingSystemDefinitionV0);

			var language = new SubTag(writingSystemDefinitionV0.ISO639);
			var script = new SubTag(writingSystemDefinitionV0.Script);
			var region = new SubTag(writingSystemDefinitionV0.Region);
			var variant = new SubTag(writingSystemDefinitionV0.Variant);
			var privateUse = new SubTag();

			//This fixes a bug where the LdmlAdaptorV1 was writing out Zxxx as part of the variant to mark an audio writing system
			if(variant.Contains(WellKnownSubTags.Audio.Script))
			{
				MoveTagsMatching(variant, script, tag => tag.Equals(WellKnownSubTags.Audio.Script));
				privateUse.AddToSubtag(WellKnownSubTags.Audio.PrivateUseSubtag);
			}

			// Move script, region, and variants present in the langauge tag to their proper subtag.
			MoveTagsMatching(language, script, StandardTags.IsValidIso15924ScriptCode, StandardTags.IsValidIso639LanguageCode);
			MoveTagsMatching(language, region, StandardTags.IsValidIso3166Region, StandardTags.IsValidIso639LanguageCode);
			MoveTagsMatching(language, variant, StandardTags.IsValidRegisteredVariant, StandardTags.IsValidIso639LanguageCode);

			// Move all other tags that don't belong to the private use subtag.
			MoveTagsMatching(
				language, privateUse, tag => !StandardTags.IsValidIso639LanguageCode(tag)
			);
			MoveTagsMatching(
				script, privateUse, tag => !StandardTags.IsValidIso15924ScriptCode(tag)
			);
			MoveTagsMatching(
				region, privateUse, tag => !StandardTags.IsValidIso3166Region(tag)
			);
			MoveTagsMatching(
				variant, privateUse, tag => !StandardTags.IsValidRegisteredVariant(tag)
			);

			language.KeepFirstAndMoveRemainderTo(privateUse);
			script.KeepFirstAndMoveRemainderTo(privateUse);
			region.KeepFirstAndMoveRemainderTo(privateUse);

			if(privateUse.Contains(WellKnownSubTags.Audio.PrivateUseSubtag))
			{
				// Move every tag that's not a Zxxx to private use
				if(!script.IsEmpty && !script.Contains(WellKnownSubTags.Audio.Script))
				{
					MoveTagsMatching(script, privateUse, tag => !privateUse.Contains(tag));
				}
				// If we don't have a Zxxx already, set it. This protects tags already present, but with unusual case
				if (!script.Contains(WellKnownSubTags.Audio.Script))
				{
					script = new SubTag(WellKnownSubTags.Audio.Script);
				}
			}

			//These two methods may produce duplicates that will subsequently be removed. Do we care? - TA 29/3/2011
			privateUse.TruncatePartsToNumCharacters(8);
			privateUse.RemoveNonAlphaNumericCharacters();

			variant.RemoveDuplicates();
			privateUse.RemoveDuplicates();
			// Any 'x' in the other tags will have arrived in the privateUse tag, so remove them.
			privateUse.RemoveParts("x");

			if ((language.IsEmpty && (!script.IsEmpty || !region.IsEmpty || !variant.IsEmpty)) ||
				(language.IsEmpty && script.IsEmpty && region.IsEmpty && variant.IsEmpty && privateUse.IsEmpty))
			{
				language.AddToSubtag("qaa");
			}

			//MapDataFromWsV0ToWsV1();
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
				language.CompleteTag,
				script.CompleteTag,
				region.CompleteTag,
				ConcatenateVariantAndPrivateUse(variant, privateUse)
			);
			_writingSystemsV1[sourceFileName] = writingSystemDefinitionV1;
			//_migratedWs.VerboseDescription //not written out by LdmlAdaptorV1 - flex?
			//_migratedWs.StoreID = ??? //what to do?
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

		private static string ConcatenateVariantAndPrivateUse(SubTag variant, SubTag privateUse)
		{
			string concatenatedTags = variant.CompleteTag;
			if (!String.IsNullOrEmpty(concatenatedTags))
			{
				concatenatedTags += "-";
			}
			if (!String.IsNullOrEmpty(privateUse.CompleteTag))
			{
				concatenatedTags += "x-";
				concatenatedTags += privateUse.CompleteTag;
			}
			return concatenatedTags;
		}

		private static void MoveTagsMatching(SubTag from, SubTag to, Predicate<string> moveAllMatching)
		{
			var list = new List<string>(from.AllParts.Where(part => moveAllMatching(part)));
			foreach (var part in list)
			{
				to.AddToSubtag(part);
				from.RemoveParts(part);
			}
		}

		private static void MoveTagsMatching(SubTag from, SubTag to, Predicate<string> moveAllMatching, Predicate<string> keepFirstMatching)
		{
			var list = new List<string>(from.AllParts.Where(part => moveAllMatching(part)));
			bool haveFirstMatching = false;
			foreach (var part in list)
			{
				if (!haveFirstMatching && keepFirstMatching(part))
				{
					haveFirstMatching = true;
					continue;
				}
				to.AddToSubtag(part);
				from.RemoveParts(part);
			}
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
				WriteLdml(writingSystemDefinitionV1, sourceFilePath, destinationFilePath);
			}
			_onMigrationCallback(_migrationInfo);
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
