using System;
using System.Collections.Generic;
using System.Text;

namespace Palaso.WritingSystems
{
	public class RFC5646Tag : Object
	{
		public enum SubTag
		{
			Language,
			Script,
			Region,
			Variant
		}

		private List<string> _language;
		private List<string> _script;
		private List<string> _region;
		private List<string> _variant;

		private string[] seperators = new string[]{"-", "_"};

		public RFC5646Tag(string language, string script, string region, string variant)
		{
			Language = language;
			Script = script;
			Region = region;
			Variant = variant;
		}

		///<summary>
		/// Copy constructor
		///</summary>
		///<param name="rhs"></param>
		public RFC5646Tag(RFC5646Tag rhs)
		{
			_language = rhs._language;
			_script = rhs._script;
			_region = rhs._region;
			_variant = rhs._variant;
		}

		public string CompleteTag
		{
			get
			{
				string id = String.IsNullOrEmpty(Language) ? string.Empty : Language;
				if (!String.IsNullOrEmpty(Script))
				{
					id += "-" + Script;
				}
				if (!String.IsNullOrEmpty(Region))
				{
					id += "-" + Region;
				}
				if (!String.IsNullOrEmpty(Variant))
				{
					id += "-" + Variant;
				}
				return id;
			}
		}

		public string Language
		{
			get { return AssembleLanguageSubtag(_language); }
			set
			{
				_language = ParseSubtagForParts(value);
				if (!LanguageTagIsValid)
				{
					throw new ArgumentException("Language codes may not contain private use extensions.");
				}
			}
		}

		private bool LanguageTagIsValid
		{
			get
			{
				foreach (string part in _language)
				{
					if (PartIsExtension(part))
					{
						return false;
					}
				}
				return true;
			}
		}

		private bool PartIsExtension(string part)
		{
			return part.StartsWith("x-",StringComparison.OrdinalIgnoreCase) ? true : false;
		}

		public string Script
		{
			get { return AssembleLanguageSubtag(_script); }
			set
			{
				_script = ParseSubtagForParts(value);
				if (!ScriptTagIsValid)
				{
					throw new ArgumentException();
				}
			}
		}

		private bool ScriptTagIsValid
		{
			get { return true; }
		}

		public string Region
		{
			get { return AssembleLanguageSubtag(_region); }
			set
			{
				_region = ParseSubtagForParts(value);
				if (!RegionTagIsValid)
				{
					throw new ArgumentException();
				}
			}
		}

		private bool RegionTagIsValid
		{
			get { return true; }
		}

		public string Variant
		{
			get { return AssembleLanguageSubtag(_variant); }
			set
			{
				_variant = ParseSubtagForParts(value);
				if (!VariantTagIsValid)
				{
					throw new ArgumentException();
				}
			}
		}

		private bool VariantTagIsValid
		{
			get { return true; }
		}

		public void AddToSubtag(SubTag subTag, string stringToAppend)
		{
			List<string> subtagToAddTo = GetSubtag(subTag);
			if(subtagToAddTo.Count != 0) {AddSeparatorToSubtag(subtagToAddTo);}
			List<string> partsOfStringToAdd = ParseSubtagForParts(stringToAppend);
			foreach (string part in partsOfStringToAdd)
			{
				 bool subTagAlreadyContainsAtLeastOnePartOfStringToAdd = !Rfc5646SubtagParser.StringIsSeperator(part) && SubtagContainsPart(subTag, part);
				if (subTagAlreadyContainsAtLeastOnePartOfStringToAdd)
				{
						throw new ArgumentException();
				}
				subtagToAddTo.Add(part);
			}
		}

		private void AddSeparatorToSubtag(List<string> subtagToAddTo)
		{
			subtagToAddTo.Add("-");
		}

		private List<string> GetSubtag(SubTag subTag)
		{
			List<string> SubtagToAddTo = new List<string>();
			switch (subTag)
			{
				case SubTag.Language:
					SubtagToAddTo = _language;
					break;
				case SubTag.Script:
					SubtagToAddTo = _script;
					break;
				case SubTag.Region:
					SubtagToAddTo = _region;
					break;
				case SubTag.Variant:
					SubtagToAddTo = _variant;
					break;
				default: throw new ApplicationException();
			}
			return SubtagToAddTo;
		}

		public static RFC5646Tag GetValidTag(RFC5646Tag tagToConvert)
		{

			RFC5646Tag validRfc5646Tag = null;

			if (IsBadAudioTag(tagToConvert))
			{
				string newLanguageTag = tagToConvert.Language.Split('-')[0];
				validRfc5646Tag = RFC5646TagForVoiceWritingSystem(newLanguageTag, tagToConvert.Region);
			}
			return validRfc5646Tag;
		}

		private static bool IsBadAudioTag(RFC5646Tag tagToConvert)
		{
			return (tagToConvert.Language.Contains(WellKnownSubTags.Audio.VariantMarker)) ||
				   (tagToConvert.Variant == WellKnownSubTags.Audio.VariantMarker && tagToConvert.Script != WellKnownSubTags.Audio.Script) ||
				   (tagToConvert.Variant == WellKnownSubTags.Audio.VariantMarker && tagToConvert.Language.Contains("-"));
		}

		public static RFC5646Tag RFC5646TagForVoiceWritingSystem(string language, string region)
		{
			return new RFC5646Tag(language, WellKnownSubTags.Audio.Script, region, "x-audio");
		}

		public string ToString()
		{
			return CompleteTag;
		}

		public override bool Equals(Object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (RFC5646Tag)) return false;
			return Equals((RFC5646Tag) obj);
		}

		public bool Equals(RFC5646Tag other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other._language, _language) && Equals(other._script, _script) && Equals(other._region, _region) && Equals(other._variant, _variant);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (_language != null ? _language.GetHashCode() : 0);
				result = (result*397) ^ (_script != null ? _script.GetHashCode() : 0);
				result = (result*397) ^ (_region != null ? _region.GetHashCode() : 0);
				result = (result*397) ^ (_variant != null ? _variant.GetHashCode() : 0);
				return result;
			}
		}

		public void RemoveFromSubtag(SubTag subTag, string stringToRemove)
		{
			List<string> partsOfSubtagToRemovePartFrom = GetSubtag(subTag);
			List<string> partsOfStringToRemove = ParseSubtagForParts(stringToRemove);

			if(!SubtagContainsAllPartsOfStringToBeRemoved(partsOfSubtagToRemovePartFrom, partsOfStringToRemove)){return;}

			bool subtagHasMultipleParts = partsOfSubtagToRemovePartFrom.Count > 1;
			bool subtagHasOnlyOnePart = partsOfSubtagToRemovePartFrom.Count == 1;
			foreach (string partToRemove in partsOfStringToRemove)
			{
				if(!SubtagContainsPart(subTag, partToRemove)){continue;}
				if (Rfc5646SubtagParser.StringIsSeperator(partToRemove)) {continue; }
				bool stringToRemoveIsFirstItemInSubtag = partsOfSubtagToRemovePartFrom[0].Equals(partToRemove,
																								 StringComparison.
																									 OrdinalIgnoreCase);
				bool stringToRemoveIsOnlyPartOfSubtag = (partsOfSubtagToRemovePartFrom.Count == 1) &&
														(partsOfSubtagToRemovePartFrom[0].Equals(partToRemove,
																								 StringComparison.
																									 OrdinalIgnoreCase));
				bool stringToRemoveIsFirstPartOfMultiPartSubtag = subtagHasMultipleParts &&
																  stringToRemoveIsFirstItemInSubtag;

				int indexOfPartToRemove = partsOfSubtagToRemovePartFrom.FindIndex(partInSubtag => partInSubtag.Equals(partToRemove, StringComparison.OrdinalIgnoreCase));
				if (stringToRemoveIsOnlyPartOfSubtag)
				{
					partsOfSubtagToRemovePartFrom.RemoveAt(0);
				}
				else if (stringToRemoveIsFirstPartOfMultiPartSubtag)
				{
					partsOfSubtagToRemovePartFrom.RemoveAt(1); //Removes following seperator
					partsOfSubtagToRemovePartFrom.RemoveAt(0);
				}
				else
				{
					partsOfSubtagToRemovePartFrom.RemoveAt(indexOfPartToRemove);
					partsOfSubtagToRemovePartFrom.RemoveAt(indexOfPartToRemove - 1); //removes preceding seperator
				}
			}
		}

		private bool SubtagContainsAllPartsOfStringToBeRemoved(List<string> partsOfSubtagToRemovePartFrom, List<string> partsOfStringToRemove)
		{
			foreach (string part in partsOfStringToRemove)
			{
				if (Rfc5646SubtagParser.StringIsSeperator(part)) {continue; }
				if (!partsOfSubtagToRemovePartFrom.Contains(part, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}

		private List<string> ParseSubtagForParts(string subtagToParse)
		{
			return new Rfc5646SubtagParser(subtagToParse).GetParts();
		}

		private string AssembleLanguageSubtag(List<string> subtag)
		{
			string subtagAsString = "";
			foreach (string part in subtag)
			{
				subtagAsString = String.Concat(String.Concat(subtagAsString, part));
			}
			return subtagAsString;
		}

		public bool SubtagContainsPart(SubTag subtagToCheck, string partToFind)
		{
			List<string> partsOfSubTag = GetSubtag(subtagToCheck);
			return partsOfSubTag.Contains(partToFind, StringComparison.OrdinalIgnoreCase) ? true : false;
		}
	}
}
