using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Extensions;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using static SIL.WritingSystems.WellKnownSubtags;

namespace SIL.WritingSystems.Migration
{
	/// <summary>
	/// The IETF language tag cleaner.
	/// </summary>
	public class IetfLanguageTagCleaner
	{
		private SubTag _languageSubTag;
		private SubTag _scriptSubTag;
		private SubTag _regionSubTag;
		private SubTag _variantSubTag;
		private SubTag _privateUseSubTag;

		/// <summary>
		/// Initializes a new instance of the <see cref="IetfLanguageTagCleaner"/> class.
		/// </summary>
		public IetfLanguageTagCleaner(string language, string script, string region, string variant, string privateUse)
		{
			Language = language;
			Script = script;
			Region = region;
			Variant = variant;
			PrivateUse = privateUse;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IetfLanguageTagCleaner"/> class.
		/// </summary>
		public IetfLanguageTagCleaner(string completeTag)
		{
			Language = completeTag;
			Script = "";
			Region = "";
			Variant = "";
			PrivateUse = "";
		}

		/// <summary>
		/// Gets the language.
		/// </summary>
		public string Language
		{
			get { return _languageSubTag.CompleteTag.ToLower(); }
			private set { _languageSubTag = new SubTag(value); }
		}

		/// <summary>
		/// Gets the script.
		/// </summary>
		public string Script
		{
			get { return _scriptSubTag.CompleteTag.ToUpperFirstLetter(); }
			private set { _scriptSubTag = new SubTag(value); }
		}

		/// <summary>
		/// Gets the region.
		/// </summary>
		public string Region
		{
			get { return _regionSubTag.CompleteTag.ToUpper(); }
			private set { _regionSubTag = new SubTag(value); }
		}

		/// <summary>
		/// Gets the variant.
		/// </summary>
		public string Variant
		{
			get { return _variantSubTag.CompleteTag; }
			private set { _variantSubTag = new SubTag(value); }
		}

		/// <summary>
		/// Gets the private use.
		/// </summary>
		public string PrivateUse
		{
			get { return _privateUseSubTag.CompleteTag; }
			private set { _privateUseSubTag = new SubTag(value); }
		}

		/// <summary>
		/// Gets the complete tag.
		/// </summary>
		public string GetCompleteTag()
		{
			var rfcTag = new Rfc5646Tag(Language, Script, Region, Variant, PrivateUse);
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
					for(var i = 0; i < _subTagParts.Count; ++i)
					{
						var cleanedPart = _subTagParts[i];
						// force audio case to lower for audio writing systems.
						if (i == 0 && cleanedPart.Equals(WellKnownSubtags.AudioPrivateUse, StringComparison.InvariantCultureIgnoreCase))
						{
							cleanedPart = WellKnownSubtags.AudioPrivateUse;
						}
						if (!string.IsNullOrEmpty(subtagAsString))
						{
							subtagAsString = subtagAsString + "-";
						}
						subtagAsString = subtagAsString + cleanedPart;
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

		/// <summary>
		/// Cleans the tag.
		/// </summary>
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
					if (part.Equals("x", StringComparison.OrdinalIgnoreCase))
						break; // don't migrate language code parts already explicitly marked private-use.

					if (string.IsNullOrEmpty(migrateFrom))
					{
						LanguageSubtag language;
						if (StandardSubtags.TryGetLanguageFromIso3Code(part, out language) && language.Code != language.Iso3Code)
						{
							migrateFrom = part;
							migrateTo = language.Code;
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
			MoveFirstPartToPrivateUseIfNecessary(_languageSubTag, StandardSubtags.IsValidIso639LanguageCode, UnlistedLanguage, true);
			MoveFirstPartToPrivateUseIfNecessary(_scriptSubTag, StandardSubtags.IsValidIso15924ScriptCode, "Qaaa", false);
			MoveFirstPartToPrivateUseIfNecessary(_regionSubTag, StandardSubtags.IsValidIso3166RegionCode, "QM", false);
			//This fixes a bug where the LdmlAdaptorV1 was writing out Zxxx as part of the variant to mark an audio writing system
			if (_variantSubTag.Contains(WellKnownSubtags.AudioScript))
			{
				MoveTagsMatching(_variantSubTag, _scriptSubTag, tag => tag.Equals(WellKnownSubtags.AudioScript));
				_privateUseSubTag.AddToSubtag(WellKnownSubtags.AudioPrivateUse);
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
			MoveTagsMatching(_languageSubTag, _scriptSubTag, StandardSubtags.IsValidIso15924ScriptCode, StandardSubtags.IsValidIso639LanguageCode);
			MoveTagsMatching(_languageSubTag, _regionSubTag, StandardSubtags.IsValidIso3166RegionCode, StandardSubtags.IsValidIso639LanguageCode);
			MoveTagsMatching(_languageSubTag, _variantSubTag, StandardSubtags.IsValidRegisteredVariantCode, StandardSubtags.IsValidIso639LanguageCode);

			// Move all other tags that don't belong to the private use subtag.

			//keep track of everything that we moved
			var tempSubTag = new SubTag();

			MoveTagsMatching(_languageSubTag, tempSubTag, tag => !StandardSubtags.IsValidIso639LanguageCode(tag));
			//place all the moved parts in private use.
			foreach (var part in tempSubTag.AllParts)
			{
				_privateUseSubTag.AddToSubtag(part);
				//if it looks like we moved a custom script set the subtag to mark that we've moved it
				if(_scriptSubTag.IsEmpty
					&& part.Length == 4 //potential custom script tag
					&& !WellKnownSubtags.IpaPhonemicPrivateUse.EndsWith(part)
					&& !WellKnownSubtags.IpaPhoneticPrivateUse.EndsWith(part))
				{
					_scriptSubTag = new SubTag("Qaaa");
				}
			}

			MoveTagsMatching(_scriptSubTag, _privateUseSubTag, tag => !StandardSubtags.IsValidIso15924ScriptCode(tag));
			MoveTagsMatching(_regionSubTag, _privateUseSubTag, tag => !StandardSubtags.IsValidIso3166RegionCode(tag));
			MoveTagsMatching(_variantSubTag, _privateUseSubTag, tag => !StandardSubtags.IsValidRegisteredVariantCode(tag));

			_languageSubTag.KeepFirstAndMoveRemainderTo(_privateUseSubTag);
			_scriptSubTag.KeepFirstAndMoveRemainderTo(_privateUseSubTag);
			_regionSubTag.KeepFirstAndMoveRemainderTo(_privateUseSubTag);

			if (_privateUseSubTag.Contains(WellKnownSubtags.AudioPrivateUse))
			{
				// Move every tag that's not a Zxxx to private use
				if (!_scriptSubTag.IsEmpty && !_scriptSubTag.Contains(WellKnownSubtags.AudioScript))
				{
					MoveTagsMatching(_scriptSubTag, _privateUseSubTag, tag => !_privateUseSubTag.Contains(tag));
				}
				// If we don't have a Zxxx already, set it. This protects tags already present, but with unusual case
				if (!_scriptSubTag.Contains(WellKnownSubtags.AudioScript))
				{
					_scriptSubTag = new SubTag(WellKnownSubtags.AudioScript);
				}
			}

			//These two methods may produce duplicates that will subsequently be removed. Do we care? - TA 29/3/2011
			_privateUseSubTag.RemoveNonAlphaNumericCharacters();
			_privateUseSubTag.TruncatePartsToNumCharacters(8);

			_variantSubTag.RemoveDuplicates();
			_privateUseSubTag.RemoveDuplicates();
			// Any 'x' in the other tags will have arrived in the privateUse tag, so remove them.
			_privateUseSubTag.RemoveParts("x");

			// if language is empty, we need to add qaa, unless only a privateUse is present (e.g. x-blah is a valid rfc5646 tag)
			if ((_languageSubTag.IsEmpty && (!_scriptSubTag.IsEmpty || !_regionSubTag.IsEmpty || !_variantSubTag.IsEmpty)) ||
				(_languageSubTag.IsEmpty && _scriptSubTag.IsEmpty && _regionSubTag.IsEmpty && _variantSubTag.IsEmpty && _privateUseSubTag.IsEmpty))
			{
				_languageSubTag.AddToSubtag(UnlistedLanguage);
			}

			// Two more legacy problems. We don't allow -etic or -emic without fonipa, so insert if needed.
			// If it has some other standard variant we won't be able to fix it...not sure what the right answer would be.
			// At least we catch the more common case.
			foreach (string part in _privateUseSubTag.AllParts)
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
		private void MoveFirstPartToPrivateUseIfNecessary(SubTag from, Func<string, bool> test, string standardPrivatePart,
			bool keepStandardPartInPrivateUse)
		{
			string part = from.AllParts.FirstOrDefault();
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
		private bool MoveStandardPartToStart(SubTag from, Func<string, bool> test, bool keepStandardPartInPrivateUse)
		{
			foreach (string goodPart in from.AllParts)
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

		/// <summary>
		/// If the given subtag has an "x" part move all subsequent parts into private use and remove the x
		/// and all subsequent parts from the from SubTag.
		/// </summary>
		/// <param name="from"></param>
		private void MovePartsToPrivateUseIfNecessary(SubTag from)
		{
			string movedParts = null;
			foreach (var part in from.AllParts)
			{
				if(movedParts == null && part.ToLowerInvariant().Equals("x"))
				{
					movedParts = "x";
				}
				else if(movedParts != null)
				{
					movedParts += "-";
					movedParts += part;
					_privateUseSubTag.AddToSubtag(part);
				}
			}
			if(movedParts != null)
				from.RemoveParts(movedParts);
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

		/// <summary>
		/// This method should move all subtag parts in the 'from' subtag which match the moveAllMatching predicate into the 'to' subtag.
		/// Because some parts of a subtag may match in more than one language tag area care must be taken to prevent emptying all parts of
		/// one subtag into another so the first part that matches the keepFirstMatching predicate will not be moved.
		/// i.e. if the languageTag is 'from' and the regionTag is 'to' and keepFirstMatching matches language codes and moveAllMatching
		/// matches region codes, all region looking parts would be placed in 'to' with the possible exception of the first language looking
		/// part.
		/// </summary>
		/// <param name="from">SubTag to move parts from</param>
		/// <param name="to">SubTag to move matching parts to</param>
		/// <param name="moveAllMatching">predicate matching parts to move</param>
		/// <param name="keepFirstMatching">predicate matching part to keep</param>
		private static void MoveTagsMatching(SubTag from, SubTag to, Predicate<string> moveAllMatching, Predicate<string> keepFirstMatching)
		{
			bool haveFirstMatching = false;
			var allParts = new List<string>(from.AllParts);
			foreach (var part in allParts)
			{
				if (!haveFirstMatching && keepFirstMatching(part))
				{
					haveFirstMatching = true;
					continue;
				}
				if (!moveAllMatching(part))
					continue;
				to.AddToSubtag(part);
				from.RemoveParts(part);
			}
		}
	}
}
