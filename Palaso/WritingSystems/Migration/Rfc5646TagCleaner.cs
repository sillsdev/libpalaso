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
			//This fixes a bug where the LdmlAdaptorV1 was writing out Zxxx as part of the variant to mark an audio writing system
			if (_variantSubTag.Contains(WellKnownSubTags.Audio.Script))
			{
				MoveTagsMatching(_variantSubTag, _scriptSubTag, tag => tag.Equals(WellKnownSubTags.Audio.Script));
				_privateUseSubTag.AddToSubtag(WellKnownSubTags.Audio.PrivateUseSubtag);
			}

			//if (Language.StartsWith("x-"))
			//{
			//    MoveTagsMatching(_languageSubTag, _privateUseSubTag, s => true);
			//}
			// If the language tag contains an x- , then move the string behind the x- to private use
			MovePartsToPrivateUseIfNecessary(_languageSubTag);

			// Move script, region, and variant present in the langauge tag to their proper subtag.
			MoveTagsMatching(_languageSubTag, _scriptSubTag, StandardTags.IsValidIso15924ScriptCode, StandardTags.IsValidIso639LanguageCode);
			MoveTagsMatching(_languageSubTag, _regionSubTag, StandardTags.IsValidIso3166Region, StandardTags.IsValidIso639LanguageCode);
			MoveTagsMatching(_languageSubTag, _variantSubTag, StandardTags.IsValidRegisteredVariant, StandardTags.IsValidIso639LanguageCode);

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
