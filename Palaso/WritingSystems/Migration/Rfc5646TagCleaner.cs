using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems.Migration
{
	public class Rfc5646TagCleaner
	{
		private SubTag _languageSubTag;
		private SubTag _scriptSubTag;
		private SubTag _regionSubTag;
		private SubTag _variantSubTag;
		private SubTag _privateUseSubTag;

		public Rfc5646TagCleaner(string language, string script, string region, string variant, string privateUse)
		{
			Language = language;
			Script = script;
			Region = region;
			Variant = variant;
			PrivateUse = privateUse;
		}

		public Rfc5646TagCleaner(string completeTag)
		{
			Language = completeTag;
			Script = "";
			Region = "";
			Variant = "";
			PrivateUse = "";
		}

		public string Language
		{
			get { return _languageSubTag.CompleteTag; }
			private set { _languageSubTag = new SubTag(value); }
		}
		public string Script
		{
			get { return _scriptSubTag.CompleteTag; }
			private set { _scriptSubTag = new SubTag(value); }
		}
		public string Region
		{
			get { return _regionSubTag.CompleteTag; }
			private set { _regionSubTag = new SubTag(value); }
		}
		public string Variant
		{
			get { return _variantSubTag.CompleteTag; }
			private set { _variantSubTag = new SubTag(value); }
		}
		public string PrivateUse
		{
			get { return _privateUseSubTag.CompleteTag; }
			private set { _privateUseSubTag = new SubTag(value); }
		}

		public string GetCompleteTag()
		{
			var rfcTag = new RFC5646Tag(Language, Script, Region, Variant, PrivateUse);
			return rfcTag.CompleteTag;
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

			internal void InsertAtStartOfSubtag(string partToAdd)
			{
				_subTagParts.Insert(0, partToAdd);
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
				if (partToFind.StartsWith("x-")) // special case for well-known private use subtags that have an x- in front of them
				{
					partToFind = partToFind.Substring(2);
				}
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

		public void Clean()
		{
			// Migrate legacy ISO3 language codes to IANA 2 letter language codes, if there's a match.
			// Do this before we look for valid codes, otherwise the 3-letter ones come up as invalid and
			// get moved to private use. However, only do this to languages not identified as private-use.
			if (!Language.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
			{
				string migrateFrom = "";
				string migrateTo = "";
				foreach (string part in _languageSubTag.AllParts)
				{
					if (String.IsNullOrEmpty(migrateFrom))
					{
						foreach (var code in StandardTags.ValidIso639LanguageCodes)
						{
							if (String.IsNullOrEmpty(migrateFrom) && code.ISO3Code.Equals(part))
							{
								migrateFrom = part;
								migrateTo = code.Code;
							}
						}
					}
				}
				if (!String.IsNullOrEmpty(migrateFrom))
				{
					_languageSubTag.RemoveParts(migrateFrom);
					_languageSubTag.AddToSubtag(migrateTo);
				}
			}
			// The very next thing, before anything else gets moved to private use, is to move the parts whose position we
			// care about to the appropriate position in the private use section.
			// In the process we may remove anything non-alphanumeric, since otherwise we may move a marker that later
			// disappears (pathologically).
			MoveFirstPartToPrivateUseIfNecessary(_languageSubTag, StandardTags.IsValidIso639LanguageCode, "qaa", true);
			MoveFirstPartToPrivateUseIfNecessary(_scriptSubTag, StandardTags.IsValidIso15924ScriptCode, "Qaaa", false);
			MoveFirstPartToPrivateUseIfNecessary(_regionSubTag, StandardTags.IsValidIso3166Region, "QM", false);
			//This fixes a bug where the LdmlAdaptorV1 was writing out Zxxx as part of the variant to mark an audio writing system
			if (_variantSubTag.Contains(WellKnownSubTags.Audio.Script))
			{
				MoveTagsMatching(_variantSubTag, _scriptSubTag, tag => tag.Equals(WellKnownSubTags.Audio.Script));
				_privateUseSubTag.AddToSubtag(WellKnownSubTags.Audio.PrivateUseSubtag);
			}
			// Fixes various legacy problems.
			if (Language.Equals("cmn", StringComparison.OrdinalIgnoreCase))
				Language = "zh";
			if (Language.Equals("pes", StringComparison.OrdinalIgnoreCase))
				Language = "fa";
			if (Language.Equals("arb", StringComparison.OrdinalIgnoreCase))
				Language = "ar";
			if (Language.Equals("zh", StringComparison.OrdinalIgnoreCase) && String.IsNullOrEmpty(Region))
				Region = "CN";

			// If the language tag contains an x- , then move the string behind the x- to private use
			MovePartsToPrivateUseIfNecessary(_languageSubTag);

			// Move script, region, and variant present in the langauge tag to their proper subtag.
			MoveTagsMatching(_languageSubTag, _scriptSubTag, StandardTags.IsValidIso15924ScriptCode, StandardTags.IsValidIso639LanguageCode);
			MoveTagsMatching(_languageSubTag, _regionSubTag, StandardTags.IsValidIso3166Region, StandardTags.IsValidIso639LanguageCode);
			MoveTagsMatching(_languageSubTag, _variantSubTag, StandardTags.IsValidRegisteredVariant, StandardTags.IsValidIso639LanguageCode);

			// This didn't really work out
			//foreach (string part in _languageSubTag.AllParts.Where(languagecode => StandardTags.ValidIso639LanguageCodes.Any(code => code.ISO3Code.Equals(languagecode))))
			//{
			//    _languageSubTag.AddToSubtag(StandardTags.ValidIso639LanguageCodes.First(code => code.ISO3Code.Equals(part)).Code);
			//    _languageSubTag.RemoveParts(part);
			//}

			// Move all other tags that don't belong to the private use subtag.
			MoveTagsMatching(
				_languageSubTag, _privateUseSubTag, tag => !StandardTags.IsValidIso639LanguageCode(tag)
			);
			MoveTagsMatching(
				_scriptSubTag, _privateUseSubTag, tag => !StandardTags.IsValidIso15924ScriptCode(tag)
			);
			MoveTagsMatching(
				_regionSubTag, _privateUseSubTag, tag => !StandardTags.IsValidIso3166Region(tag)
			);
			MoveTagsMatching(
				_variantSubTag, _privateUseSubTag, tag => !StandardTags.IsValidRegisteredVariant(tag)
			);

			_languageSubTag.KeepFirstAndMoveRemainderTo(_privateUseSubTag);
			_scriptSubTag.KeepFirstAndMoveRemainderTo(_privateUseSubTag);
			_regionSubTag.KeepFirstAndMoveRemainderTo(_privateUseSubTag);

			if (_privateUseSubTag.Contains(WellKnownSubTags.Audio.PrivateUseSubtag))
			{
				// Move every tag that's not a Zxxx to private use
				if (!_scriptSubTag.IsEmpty && !_scriptSubTag.Contains(WellKnownSubTags.Audio.Script))
				{
					MoveTagsMatching(_scriptSubTag, _privateUseSubTag, tag => !_privateUseSubTag.Contains(tag));
				}
				// If we don't have a Zxxx already, set it. This protects tags already present, but with unusual case
				if (!_scriptSubTag.Contains(WellKnownSubTags.Audio.Script))
				{
					_scriptSubTag = new SubTag(WellKnownSubTags.Audio.Script);
				}
			}

			//These two methods may produce duplicates that will subsequently be removed. Do we care? - TA 29/3/2011
			_privateUseSubTag.TruncatePartsToNumCharacters(8);
			_privateUseSubTag.RemoveNonAlphaNumericCharacters();

			_variantSubTag.RemoveDuplicates();
			_privateUseSubTag.RemoveDuplicates();
			// Any 'x' in the other tags will have arrived in the privateUse tag, so remove them.
			_privateUseSubTag.RemoveParts("x");

			// if language is empty, we need to add qaa, unless only a privateUse is present (e.g. x-blah is a valid rfc5646 tag)
			if ((_languageSubTag.IsEmpty && (!_scriptSubTag.IsEmpty || !_regionSubTag.IsEmpty || !_variantSubTag.IsEmpty)) ||
				(_languageSubTag.IsEmpty && _scriptSubTag.IsEmpty && _regionSubTag.IsEmpty && _variantSubTag.IsEmpty && _privateUseSubTag.IsEmpty))
			{
				_languageSubTag.AddToSubtag("qaa");
			}

			// Two more legacy problems. We don't allow -etic or -emic without fonipa, so insert if needed.
			// If it has some other standard variant we won't be able to fix it...not sure what the right answer would be.
			// At least we catch the more common case.
			foreach (var part in _privateUseSubTag.AllParts)
			{
				if (string.IsNullOrEmpty(Variant)
					&& (part.Equals("etic",StringComparison.OrdinalIgnoreCase) || part.Equals("emic", StringComparison.OrdinalIgnoreCase)))
				{
					Variant = "fonipa";
				}
			}
		}

		private string FirstNonXPart(IEnumerable<string> input)
		{
			foreach (var part in input)
				if (!part.Equals("x", StringComparison.OrdinalIgnoreCase))
					return part;
			return null;
		}

		/// <summary>
		/// This is used to move one part if appropriate from the 'from' subtag to private use.
		/// Alternatively, if any part is appropriate for the tag according to the test predicate,
		/// it is moved to the first position (unless it follows an x- and keepStandardPartInPrivateUse is true).
		/// If we didn't find a valid part, but did move something, insert standardPrivateCode at the START of "from".
		/// As a side effect, this method may remove non-alphanumeric characters from the from tag.
		/// (I don't like having such a side effect, but it seems necessary to produce the desired behavior).
		/// </summary>
		private void MoveFirstPartToPrivateUseIfNecessary(SubTag from, Func<bool, string> test, string standardPrivatePart,
			bool keepStandardPartInPrivateUse)
		{
			var part = from.AllParts.FirstOrDefault();
			if (part == null)
				return; // nothing to move.
			if (test(part))
				return; // no need to move, it is a valid code for its slot.
			if (MoveStandardPartToStart(from, test, keepStandardPartInPrivateUse))
				return;
			// If we exit this loop we need to move the first part to private use.
			// But first strip illegal characters since that may leave nothing to move,
			// or at least nothing of the first part we would otherwise move.
			// We do NOT want to do this BEFORE looking for good parts, because (for example) if we have a
			// region code like U!S-gb, we want to detect 'gb' as a good region code and keep that,
			// rather than fixing U!S to US and then choosing to keep that.
			from.RemoveNonAlphaNumericCharacters();
			// But, now we should scan again. If cleaning out bad characters resulted in a good code,
			// let's put it in the main part of the tag rather than private-use.
			if (MoveStandardPartToStart(from, test, keepStandardPartInPrivateUse))
				return;
			// OK, no good parts left. We will move the first part that is not an X.
			part = FirstNonXPart(from.AllParts);
			if (part == null)
				return;
			_privateUseSubTag.AddToSubtag(part);
			from.RemoveParts(part);
			from.InsertAtStartOfSubtag(standardPrivatePart);
		}

		/// <summary>
		/// If there is a standard part (that passes test) in the parts of the subtag, move it to the start and return true.
		/// If keepStandardPartInPrivateUse is true, only a part before the first 'x' may be moved.
		/// Return true if an acceptable part was found.
		/// </summary>
		private bool MoveStandardPartToStart(SubTag from, Func<bool, string> test, bool keepStandardPartInPrivateUse)
		{
			foreach (var goodPart in from.AllParts)
			{
				if (keepStandardPartInPrivateUse && goodPart.Equals("x", StringComparison.OrdinalIgnoreCase))
					return false;
				if (test(goodPart))
				{
					from.RemoveParts(goodPart);
					from.InsertAtStartOfSubtag(goodPart);
					return true;
				}
			}
			return false;
		}

		private void MovePartsToPrivateUseIfNecessary(SubTag from)
		{
			string completeTag = from.CompleteTag;
			if (completeTag.Contains("x-"))
			{
				string privateUseParts = completeTag.Substring(completeTag.IndexOf("x-") + 2);
				from.RemoveParts("x");
				from.RemoveParts(privateUseParts);
				_privateUseSubTag.AddToSubtag(privateUseParts);
			}
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
	}
}
