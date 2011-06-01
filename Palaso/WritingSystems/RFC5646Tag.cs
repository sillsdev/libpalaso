using System;
using System.Collections.Generic;
using System.Linq;
using Palaso.Data;

namespace Palaso.WritingSystems
{
	internal class RFC5646Tag : Object
	{
		internal class SubTag
		{
			private List<string> _subTagParts;

			public SubTag()
			{
				_subTagParts = new List<string>();
			}

			public SubTag(SubTag rhs)
			{
				_subTagParts = new List<string>(rhs._subTagParts);
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

			public static List<string> ParseSubtagForParts(string subtagToParse)
			{
				var parts = new List<string>();
				parts.AddRange(subtagToParse.Split('-'));
				parts.RemoveAll(str => str == "");
				return parts;
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

			public void ThrowIfSubtagContainsInvalidContent()
			{
				string offendingSubtag;
				if ((!String.IsNullOrEmpty(offendingSubtag = _subTagParts.Find(StringContainsNonAlphaNumericCharacters))))
				{
					throw new ValidationException(
						String.Format(
							"Private use subtags may not contain non alpha numeric characters. The offending subtag was {0}",
							offendingSubtag
							)
						);
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

			public void ThrowIfSubtagContainsDuplicates()
			{
				foreach (string part in _subTagParts)
				{
					//if (part.Equals("-") || part.Equals("_"))
					//{
					//    continue;
					//}
					if(_subTagParts.FindAll(p => p.Equals(part, StringComparison.OrdinalIgnoreCase)).Count > 1)
					{
						throw new ValidationException(String.Format("Subtags may never contain duplicate parts. The duplicate part was: {0}", part));
					}
				}
			}

		}

		private string _language = "";
		private string _script = "";
		private string _region = "";
		private SubTag _variant = new SubTag();
		private SubTag _privateUse = new SubTag();

		public RFC5646Tag() :
			this("qaa", String.Empty, String.Empty, String.Empty, String.Empty)
		{
		}

		public RFC5646Tag(string language, string script, string region, string variant, string privateUse)
		{
			_language = language ?? "";
			_script = script ?? "";
			_region = region ?? "";
			_variant.AddToSubtag(variant ?? "");
			_privateUse.AddToSubtag(privateUse ?? "");
			Validate();
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
			_variant = new SubTag(rhs._variant);
			_privateUse = new SubTag(rhs._privateUse);
		}

		private void Validate()
		{
			ValidateLanguage();
			ValidateScript();
			ValidateRegion();
			ValidateVariant();
			ValidatePrivateUse();
			if (!(HasLanguage || (!HasLanguage && !HasScript && !HasRegion && !HasVariant && HasPrivateUse)))
			{
				throw new ValidationException("An Rfc5646 tag must have a language subtag or consist entirely of private use subtags.");
			}
		}

		public string CompleteTag
		{
			get
			{
				string id = String.IsNullOrEmpty(Language) ? string.Empty : Language;
				if (!String.IsNullOrEmpty(id))
				{
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
					if (!String.IsNullOrEmpty(PrivateUse))
					{
						id += "-" + PrivateUse;
					}
				}
				else
				{
					id = PrivateUse;
				}
				return id;
			}
		}

		public string Language
		{
			get { return _language; }
			set
			{
				_language = value ?? "";
				Validate();
			}
		}

		private void ValidateLanguage()
		{
			if (String.IsNullOrEmpty(_language))
			{
				return;
			}

			if (_language.Contains("-"))
			{
				throw new ValidationException(
					"The language tag may not contain dashes. I.e. there may only be a single iso 639 tag in this subtag"
				);
			}
			if (!StandardTags.IsValidIso639LanguageCode(_language))
			{
				throw new ValidationException(String.Format("'{0}' is not a valid ISO-639 language code.", _language));
			}
		}

		public string Script
		{
			get { return _script; }
			set
			{
				_script = value ?? "";
				Validate();
			}
		}

		public string PrivateUse
		{
			get
			{
				var result = _privateUse.CompleteTag;
				if (!String.IsNullOrEmpty(result))
				{
					result = "x-" + result;
				}
				return result;
			}
			set
			{
				SetPrivateUseSubtags(value);
			}
		}

		private void SetPrivateUseSubtags(string tags)
		{
			_privateUse = new SubTag();
			AddToPrivateUse(tags);
		}

		private void ValidatePrivateUse()
		{
			if (_privateUse.Contains("x"))
			{
				throw new ValidationException("Private Use tag may not contain 'x'");
			}
			_privateUse.ThrowIfSubtagContainsInvalidContent();
			_privateUse.ThrowIfSubtagContainsDuplicates();
		}

		private void ValidateScript()
		{
			if (String.IsNullOrEmpty(_script))
			{
				return;
			}
			if (_script.Contains("-"))
			{
				throw new ValidationException("The script tag may not contain dashes or underscores. I.e. there may only be a single iso 639 tag in this subtag");
			}
			if(!StandardTags.IsValidIso15924ScriptCode(_script))
			{
				throw new ValidationException(String.Format("'{0}' is not a valid ISO-15924 script code.", _script));
			}
		}

		public string Region
		{
			get { return _region; }
			set
			{
				_region = value ?? "";
				Validate();
			}
		}

		private void ValidateRegion()
		{
			if (String.IsNullOrEmpty(_region))
			{
				return;
			}
			if (_region.Contains("-"))
			{
				throw new ValidationException("The region tag may not contain dashes or underscores. I.e. there may only be a single iso 639 tag in this subtag");
			}
			if (!StandardTags.IsValidIso3166Region(_region))
			{
				throw new ValidationException(String.Format("'{0}' is not a valid ISO-3166 region code.", _region));
			}
		}

		public string Variant
		{
			get
			{
				return _variant.CompleteTag;
			}
			set
			{
				_variant = new SubTag();
				AddToVariant(value);
			}
		}

		public bool HasRegion
		{
			get { return !String.IsNullOrEmpty(_region); }
		}

		public bool HasScript
		{
			get { return !String.IsNullOrEmpty(_script); }
		}

		public bool HasLanguage
		{
			get { return !String.IsNullOrEmpty(_language); }
		}

		public bool HasVariant
		{
			get { return _variant.Count > 0; }
		}

		public bool HasPrivateUse
		{
			get { return _privateUse.Count > 0; }
		}

		private void SetVariantSubtags(string tags)
		{
			_variant = new SubTag();
			AddToVariant(tags);
		}

		private void ValidateVariant()
		{
			var invalidPart = _variant.AllParts.FirstOrDefault(part => !StandardTags.IsValidRegisteredVariant(part));
			if (!String.IsNullOrEmpty(invalidPart))
			{
				throw new ValidationException(
					String.Format("'{0}' is not a valid registered variant code.", invalidPart)
				);
			}
		}

		public new string ToString()
		{
			return CompleteTag;
		}

		///<summary>Constructor method to parse a valid RFC5646 tag as a string
		///</summary>
		///<param name="inputString">valid RFC5646 string</param>
		///<returns>RFC5646Tag object</returns>
		public static RFC5646Tag Parse(string inputString)
		{
			var tokens = inputString.Split(new[] {'-'});

			var rfc5646Tag = new RFC5646Tag();

			bool haveX = false;
			for (int position = 0; position < tokens.Length; ++position)
			{
				var token = tokens[position];
				if (token == "x")
				{
					haveX = true;
					continue;
				}
				if (haveX)
				{
					//This is the case for RfcTags consisting only of a private use subtag
					if(position==1)
					{
						rfc5646Tag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, token);
						continue;
					}
					rfc5646Tag.AddToPrivateUse(token);
					continue;
				}
				if (position == 0)
				{
					rfc5646Tag.Language = token;
					continue;
				}
				if (position <= 1 && StandardTags.IsValidIso15924ScriptCode(token))
				{
					rfc5646Tag.Script = token;
					continue;
				}
				if (position <= 2 && StandardTags.IsValidIso3166Region(token))
				{
					rfc5646Tag.Region = token;
					continue;
				}
				if (StandardTags.IsValidRegisteredVariant(token))
				{
					rfc5646Tag.AddToVariant(token);
					continue;
				}
				throw new ValidationException(String.Format("The RFC tag '{0}' could not be parsed.", inputString));
			}
			return rfc5646Tag;
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
			bool languagesAreEqual = Equals(other.Language, Language);
			bool scriptsAreEqual = Equals(other.Script, Script);
			bool regionsAreEqual = Equals(other.Region, Region);
			bool variantsArEqual = Equals(other.Variant, Variant);
			bool privateUseArEqual = Equals(other.PrivateUse, PrivateUse);
			return languagesAreEqual && scriptsAreEqual && regionsAreEqual && variantsArEqual && privateUseArEqual;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (_language != null ? _language.GetHashCode() : 0);
				result = (result * 397) ^ (_script != null ? _script.GetHashCode() : 0);
				result = (result * 397) ^ (_region != null ? _region.GetHashCode() : 0);
				result = (result * 397) ^ (_variant != null ? _variant.GetHashCode() : 0);
				result = (result * 397) ^ (_privateUse != null ? _privateUse.GetHashCode() : 0);
				return result;
			}
		}

		public void AddToPrivateUse(string tagsToAdd)
		{
			tagsToAdd = tagsToAdd ?? "";
			tagsToAdd = StripLeadingPrivateUseMarker(tagsToAdd);
			_privateUse.AddToSubtag(tagsToAdd);

			Validate();
		}

		public void RemoveFromPrivateUse(string tagsToRemove)
		{
			tagsToRemove = StripLeadingPrivateUseMarker(tagsToRemove);
			_privateUse.RemoveAllParts(tagsToRemove);
			Validate();
		}

		public bool PrivateUseContains(string tagToFind)
		{
			tagToFind = StripLeadingPrivateUseMarker(tagToFind);
			return _privateUse.Contains(tagToFind);
		}

		public static string StripLeadingPrivateUseMarker(string tag)
		{
			if (tag.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
			{
				tag = tag.Substring(2); // strip the leading x-. Ideally we would throw if WritingSystemDefinition exposed the Private Use tags.
				// throw new ArgumentException("RFC Private Use tags may not start with 'x-', try giving the tag only");
			}
			return tag;
		}

		public void AddToVariant(string tagsToAdd)
		{
			tagsToAdd = tagsToAdd ?? "";
			_variant.AddToSubtag(tagsToAdd);
			Validate();
		}

		public void RemoveFromVariant(string tagsToRemove)
		{
			_variant.RemoveAllParts(tagsToRemove);
		}

		public bool VariantContains(string tagToFind)
		{
			return _variant.Contains(tagToFind);
		}

	}
}