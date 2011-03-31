using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	public class LdmlVersion0MigrationStrategy : MigrationStrategyBase
	{
		internal class SubTag
		{
			private List<string> _subTagParts;

			public SubTag()
			{
				_subTagParts = new List<string>();
			}

			public SubTag(string partsToAdd)
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
				//This does not deal with duplicates introduced through truncation. should it?
				var subtagsContainingIllegalCharacters =
					_subTagParts.Where(part => part.Any(c => !Char.IsLetterOrDigit(c)));
				foreach (var subtag in subtagsContainingIllegalCharacters)
				{
					// got this from here: http://stackoverflow.com/questions/449513/removing-characters-from-strings-with-linq
					string subtagWithoutIllegalCharacters = new String(subtag.Where(Char.IsLetterOrDigit).ToArray());
					_subTagParts.Remove(subtag);
					_subTagParts.Add(subtagWithoutIllegalCharacters);
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
				//This does not deal with duplicates introduced through truncation. should it?
				var partsToTruncate = _subTagParts.Where(part => part.Length > size);
				foreach (var partToTruncate in partsToTruncate)
				{
					_subTagParts.Remove(partToTruncate);
					_subTagParts.Add(partToTruncate.Substring(0, size));
				}
			}

			public void AddToSubtag(string partsToAdd)
			{
				List<string> partsOfStringToAdd = ParseSubtagForParts(partsToAdd);
				foreach (string part in partsOfStringToAdd)
				{
					if (StringContainsNonAlphaNumericCharacters(part))
					{
						throw new ArgumentException(String.Format("Rfc5646 tags may only contain alphanumeric characters. '{0}' can not be added to the Rfc5646 tag.", part));
					}
					if (Contains(part))
					{
						throw new ArgumentException(String.Format("Subtags may not contain duplicates. The subtag '{0}' was already contained.", part));
					}
					_subTagParts.Add(part);
				}
			}

			private static bool StringContainsNonAlphaNumericCharacters(string stringToSearch)
			{
				return stringToSearch.Any(c => !Char.IsLetterOrDigit(c));
			}

			public void RemoveAllParts(string partsToRemove)
			{
				List<string> partsOfStringToRemove = ParseSubtagForParts(partsToRemove);

				foreach (string partToRemove in partsOfStringToRemove)
				{
					if (!Contains(partToRemove))
					{
						continue;
					}
					int indexOfPartToRemove = _subTagParts.FindIndex(partInSubtag => partInSubtag.Equals(partToRemove, StringComparison.OrdinalIgnoreCase));
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
		}



		public LdmlVersion0MigrationStrategy() :
			base(0, 1)
		{
		}

		public override void Migrate(string sourceFilePath, string destinationFilePath)
		{
			var writingSystemDefinitionToMigrate = new WritingSystemDefinitionV0();
			new LdmlAdaptorV0().Read(sourceFilePath, writingSystemDefinitionToMigrate);

			var language = new SubTag(writingSystemDefinitionToMigrate.ISO639);
			var script = new SubTag(writingSystemDefinitionToMigrate.Script);
			var region = new SubTag(writingSystemDefinitionToMigrate.Region);
			var variant = new SubTag(writingSystemDefinitionToMigrate.Variant);
			var privateUse = new SubTag();

			//This fixes a bug where the ldmladaptor was writing out Zxxx as part of the variant to mark an audio writing system
			if(variant.Contains(WellKnownSubTags.Audio.Script))
			{
				MoveTagsMatching(variant, script, tag => tag.Equals(WellKnownSubTags.Audio.Script));
				privateUse.AddToSubtag(WellKnownSubTags.Audio.PrivateUseSubtag);
			}

			// Move script, region, and variants present in the langauge tag to their proper subtag.
			MoveTagsMatching(language, script, StandardTags.IsValidIso15924ScriptCode);
			MoveTagsMatching(language, region, StandardTags.IsValidIso3166Region);
			MoveTagsMatching(language, variant, StandardTags.IsValidRegisteredVariant);

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

			// Any 'x' in the other tags will have arrived in the privateUse tag, so remove them.
			privateUse.RemoveAllParts("x");

			language.KeepFirstAndMoveRemainderTo(privateUse);
			script.KeepFirstAndMoveRemainderTo(privateUse);
			region.KeepFirstAndMoveRemainderTo(privateUse);

			if(privateUse.Contains(WellKnownSubTags.Audio.PrivateUseSubtag))
			{
				if(!script.IsEmpty && !script.Contains(WellKnownSubTags.Audio.Script))
				{
					MoveTagsMatching(script, privateUse, tag => !privateUse.Contains(tag));
				}
				script = new SubTag(WellKnownSubTags.Audio.PrivateUseSubtag);
			}

			//These two methods may produce duplicates that will subsequently be removed. Do we care? - TA 29/3/2011
			privateUse.TruncatePartsToNumCharacters(8);
			privateUse.RemoveNonAlphaNumericCharacters();

			variant.RemoveDuplicates();
			privateUse.RemoveDuplicates();

			if ((language.IsEmpty && (!script.IsEmpty || !region.IsEmpty || !variant.IsEmpty)) ||
				(language.IsEmpty && script.IsEmpty && region.IsEmpty && variant.IsEmpty && privateUse.IsEmpty))
			{
				language.AddToSubtag("qaa");
			}

			//MapDataFromWsV0ToWsV1();
			var migratedWritingSystemDefinition = new WritingSystemDefinition
				{
					ISO639 = language.CompleteTag,
					Script = script.CompleteTag,
					Region = region.CompleteTag,
					Variant = ConcatenateVariantAndPrivateUse(variant, privateUse),
					DefaultFontName = writingSystemDefinitionToMigrate.DefaultFontName,
					Abbreviation = writingSystemDefinitionToMigrate.Abbreviation,
					DefaultFontSize = writingSystemDefinitionToMigrate.DefaultFontSize,
					IsLegacyEncoded = writingSystemDefinitionToMigrate.IsLegacyEncoded,
					Keyboard = writingSystemDefinitionToMigrate.Keyboard,
					LanguageName = writingSystemDefinitionToMigrate.LanguageName,
					RightToLeftScript = writingSystemDefinitionToMigrate.RightToLeftScript,
					SortRules = writingSystemDefinitionToMigrate.SortRules,
					SortUsing = (WritingSystemDefinition.SortRulesType)writingSystemDefinitionToMigrate.SortUsing,
					SpellCheckingId = writingSystemDefinitionToMigrate.SpellCheckingId,
					VersionDescription = writingSystemDefinitionToMigrate.VersionDescription,
					DateModified = DateTime.Now
				};
			//_migratedWs.VerboseDescription //not written out by ldmladaptor - flex?
			//_migratedWs.StoreID = ??? //what to do?
			//_migratedWs.NativeName //not written out by ldmladaptor - flex?

			using (Stream streamOfOldFile = new FileStream(sourceFilePath, FileMode.Open))
			{
				var adaptorToWriteLdmlV1 = new LdmlAdaptor();
				adaptorToWriteLdmlV1.Write(destinationFilePath, migratedWritingSystemDefinition, streamOfOldFile);
				streamOfOldFile.Close();
			}
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

		private static void MoveTagsMatching(SubTag from, SubTag to, Predicate<string> predicate)
		{
			var list = new List<string>(from.AllParts.Where(part => predicate(part)));
			foreach (var part in list)
			{
				to.AddToSubtag(part);
				from.RemoveAllParts(part);
			}
		}

		public override void PostMigrate()
		{

		}
	}
}
