using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SIL.Data;
using SIL.ObjectModel;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	/// <summary>
	/// The RFC5646Tag class represents a language tag that conforms to Rfc5646. It relies heavily on the StandardTags class for
	/// valid Language, Script, Region and Variant subtags. The RFC5646 class enforces strict adherence to the Rfc5646 spec.
	/// Exceptions are:
	/// - does not support singletons other than "x-"
	/// - does not support grandfathered, regular or irregular tags
	/// </summary>
	internal class Rfc5646Tag : ICloneable<Rfc5646Tag>, IEquatable<Rfc5646Tag>
	{
		internal class Subtag : ICloneable<Subtag>, IEquatable<Subtag>
		{
			private readonly List<string> _subtagParts;

			public Subtag()
			{
				_subtagParts = new List<string>();
			}

			public Subtag(Subtag rhs)
			{
				_subtagParts = new List<string>(rhs._subtagParts);
			}

			public int Count
			{
				get { return _subtagParts.Count; }
			}

			public string CompleteTag
			{
				get
				{
					if (_subtagParts.Count == 0)
					{
						return String.Empty;
					}
					string subtagAsString = "";
					foreach (string part in _subtagParts)
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
				get { return _subtagParts; }
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
					_subtagParts.Add(part);
				}
			}

			public void ThrowIfSubtagContainsInvalidContent()
			{
				string offendingSubtag;
				if ((!String.IsNullOrEmpty(offendingSubtag = _subtagParts.Find(StringContainsNonAlphaNumericCharacters))))
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
					int indexOfPartToRemove = _subtagParts.FindIndex(partInSubtag => partInSubtag.Equals(partToRemove, StringComparison.OrdinalIgnoreCase));
					_subtagParts.RemoveAt(indexOfPartToRemove);
				}
			}

			public bool Contains(string partToFind)
			{
				return _subtagParts.Any(part => part.Equals(partToFind, StringComparison.OrdinalIgnoreCase));
			}

			public void ThrowIfSubtagContainsDuplicates()
			{
				foreach (string part in _subtagParts)
				{
					//if (part.Equals("-") || part.Equals("_"))
					//{
					//    continue;
					//}
					if(_subtagParts.FindAll(p => p.Equals(part, StringComparison.OrdinalIgnoreCase)).Count > 1)
					{
						throw new ValidationException(String.Format("Subtags may never contain duplicate parts. The duplicate part was: {0}", part));
					}
				}
			}

			public Subtag Clone()
			{
				return new Subtag(this);
			}

			public IEnumerable<string> GetPrivateUseSubtagsMatchingRegEx(string pattern)
			{
				var regex = new Regex(pattern);
				return _subtagParts.Where(part => regex.IsMatch(part));
			}

			public override bool Equals(object other)
			{
				if (!(other is Subtag)) return false;
				return Equals((Subtag)other);
			}

			public bool Equals(Subtag other)
			{
				if (other == null) return false;
				if (!_subtagParts.SequenceEqual(other._subtagParts)) return false;
				return true;
			}

			public override int GetHashCode()
			{
				int code = 23;
				foreach (string part in _subtagParts)
					code = code * 31 + part.GetHashCode();
				return code;
			}
		}

		private string _language = "";
		private string _script = "";
		private string _region = "";
		private Subtag _variant = new Subtag();
		private Subtag _privateUse = new Subtag();
		private bool _requiresValidTag = true;

		public Rfc5646Tag() :
			this(WellKnownSubtags.UnlistedLanguage, String.Empty, String.Empty, String.Empty, String.Empty)
		{
		}

		public Rfc5646Tag(string language, string script, string region, string variant, string privateUse)
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
		public Rfc5646Tag(Rfc5646Tag rhs)
		{
			_language = rhs._language;
			_script = rhs._script;
			_region = rhs._region;
			_variant = new Subtag(rhs._variant);
			_privateUse = new Subtag(rhs._privateUse);
			_requiresValidTag = rhs._requiresValidTag;
		}

		private void Validate()
		{
			if (!RequiresValidTag)
				return;
			ValidateLanguage();
			ValidateScript();
			ValidateRegion();
			ValidateVariant();
			ValidatePrivateUse();
			if (!(HasLanguage || (!HasLanguage && !HasScript && !HasRegion && !HasVariant && HasPrivateUse)))
			{
				throw new ValidationException(string.Format("An Rfc5646 tag must have a language subtag or consist entirely of private use subtags (Language={0}  Script={1} Region={2} Variant={3} Private={4})", Language,Script,Region,Variant,PrivateUse));
			}
		}

		/// <summary>
		/// Setting this true will throw unless the tag has previously been put into a valid state.
		/// </summary>
		internal bool RequiresValidTag
		{
			get { return _requiresValidTag; }
			set
			{
				_requiresValidTag = value;
				Validate();
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
			if (!StandardSubtags.IsValidIso639LanguageCode(_language))
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
			_privateUse = new Subtag();
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
			if(!StandardSubtags.IsValidIso15924ScriptCode(_script))
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
			if (!StandardSubtags.IsValidIso3166RegionCode(_region))
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
				_variant = new Subtag();
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

		private void ValidateVariant()
		{
			var invalidPart = _variant.AllParts.FirstOrDefault(part => !StandardSubtags.IsValidRegisteredVariantCode(part));
			if (!String.IsNullOrEmpty(invalidPart))
			{
				throw new ValidationException(
					String.Format("'{0}' is not a valid registered variant code.", invalidPart)
				);
			}
			_variant.ThrowIfSubtagContainsDuplicates();
		}

		public new string ToString()
		{
			return CompleteTag;
		}

		///<summary>Constructor method to parse a valid RFC5646 tag as a string
		///</summary>
		///<param name="inputString">valid RFC5646 string</param>
		///<returns>RFC5646Tag object</returns>
		public static Rfc5646Tag Parse(string inputString)
		{
			var tokens = inputString.Split(new[] {'-'});

			var rfc5646Tag = new Rfc5646Tag();

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
						rfc5646Tag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, token);
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
				if (position <= 1 && StandardSubtags.IsValidIso15924ScriptCode(token))
				{
					rfc5646Tag.Script = token;
					continue;
				}
				if (position <= 2 && StandardSubtags.IsValidIso3166RegionCode(token))
				{
					rfc5646Tag.Region = token;
					continue;
				}
				if (StandardSubtags.IsValidRegisteredVariantCode(token))
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
			if (!(obj is Rfc5646Tag)) return false;
			return Equals((Rfc5646Tag) obj);
		}

		public Rfc5646Tag Clone()
		{
			return new Rfc5646Tag(this);
		}

		public bool Equals(Rfc5646Tag other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			bool languagesAreEqual = Equals(other.Language, Language);
			bool scriptsAreEqual = Equals(other.Script, Script);
			bool regionsAreEqual = Equals(other.Region, Region);
			bool variantsArEqual = Equals(other.Variant, Variant);
			bool privateUseArEqual = Equals(other.PrivateUse, PrivateUse);
			bool requiresValidTagIsEqual = Equals(other._requiresValidTag, _requiresValidTag);
			return languagesAreEqual && scriptsAreEqual && regionsAreEqual && variantsArEqual && privateUseArEqual && requiresValidTagIsEqual;
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

		public IEnumerable<string> GetPrivateUseSubtagsMatchingRegEx(string pattern)
		{
			return _privateUse.GetPrivateUseSubtagsMatchingRegEx(pattern);
		}
	}
}