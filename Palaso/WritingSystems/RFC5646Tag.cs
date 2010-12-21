using System;

namespace Palaso.WritingSystems
{
	public class RFC5646Tag : Object
	{
		private string _language;
		private string _script;
		private string _region;
		private string _variant;

		public RFC5646Tag(string language, string script, string region, string variant)
		{
			_language = language;
			_script = script;
			_region = region;
			_variant = variant;
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
			get { return _language; }
			set { _language = value; }
		}

		public string Script
		{
			get { return _script; }
			set { _script = value; }
		}

		public string Region
		{
			get { return _region; }
			set { _region = value; }
		}

		public string Variant
		{
			get { return _variant; }
			set { _variant = value; }
		}

		///<summary>
		// This method defines what is currently considered a valid RFC 5646 language tag by palaso.
		// At the moment this is almost anything.
		///</summary>
		///<returns></returns>
		public bool IsValid()
		{
			if (IsBadAudioTag(this))
			{
				return false;
			}
			return true;
		}

		public static RFC5646Tag GetValidTag(RFC5646Tag tagToConvert)
		{
			if (tagToConvert.IsValid()) { return tagToConvert; }

			RFC5646Tag validRfc5646Tag = null;

			if (IsBadAudioTag(tagToConvert))
			{
				string newLanguageTag = tagToConvert.Language.Split('-')[0];
				validRfc5646Tag = RFC5646TagForVoiceWritingSystem(newLanguageTag, tagToConvert.Region);
			}
			if (validRfc5646Tag == null || !validRfc5646Tag.IsValid())
			{
				throw new InvalidOperationException("The palaso library is not able to covert this tag to a valid form.");
			}
			return validRfc5646Tag;
		}

		private static bool IsBadAudioTag(RFC5646Tag tagToConvert)
		{
			return (tagToConvert.Language.Contains("x-audio")) ||
				   (tagToConvert.Variant == "x-audio" && tagToConvert.Script != "Zxxx") ||
				   (tagToConvert.Variant == "x-audio" && tagToConvert.Language.Contains("-"));
		}

		public static RFC5646Tag RFC5646TagForVoiceWritingSystem(string language, string region)
		{
			return new RFC5646Tag(language, "Zxxx", region, "x-audio");
		}

		public static bool IsRFC5646TagForVoiceWritingSystem(RFC5646Tag rfcTag)
		{
			if(rfcTag.Script == "Zxxx" && rfcTag.Variant == "x-audio")
			{
				return true;
			}
			return false;
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
	}
}
