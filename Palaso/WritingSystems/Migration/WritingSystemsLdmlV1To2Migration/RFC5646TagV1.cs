using System;
using System.Collections.Generic;

namespace Palaso.WritingSystems.Migration.WritingSystemsLdmlV1To2Migration
{
	public class RFC5646TagV1 : Object
	{
		public enum SubTag
		{
			Language,
			Script,
			Region,
			Variant,
			PrivateUse
		}

		public class IanaSubtag
		{
			private readonly string _type;
			private readonly string _subtag;
			private readonly string _description;

			public IanaSubtag(string type, string subtag, string description)
			{
				_type = type;
				_subtag = subtag;
				_description = description;
			}

			public string Type
			{
				get { return _type; }
			}

			public string Subtag
			{
				get { return _subtag; }
			}

			public string Description
			{
				get { return _description; }
			}

			public static int CompareByDescription(IanaSubtag x, IanaSubtag y)
			{
				if (x == null)
				{
					if (y == null)
					{
						return 0;
					}
					else
					{
						return -1;
					}
				}
				else
				{
					if (y == null)
					{
						return 1;
					}
					else
					{
						return x.Description.CompareTo(y.Description);
					}
				}
			}
		}

		private static readonly List<Iso639LanguageCode> _validIso639LanguageCodes = new List<Iso639LanguageCode>();
		private static readonly List<Iso15924Script> _validIso15924Scripts = new List<Iso15924Script>();
		private static readonly List<IanaSubtag> _validIso3166Regions = new List<IanaSubtag>();
		private static readonly List<IanaSubtag> _validRegisteredVariants = new List<IanaSubtag>();
		private static readonly List<IanaSubtag> _ianaSubtags = new List<IanaSubtag>();
		private List<string> _language = new List<string>();
		private List<string> _script = new List<string>();
		private List<string> _region = new List<string>();
		private List<string> _variant = new List<string>();
		private List<string> _privateUse = new List<string>();

		public static List<Iso15924Script> ValidIso15924Scripts
		{
			get
			{
				if (_validIso15924Scripts.Count == 0)
				{
					LoadScriptOptionsIfNeeded();
				}
				return _validIso15924Scripts;
			}
		}

		public static IList<Iso639LanguageCode> ValidIso639LanguageCodes
		{
			get
			{
				if (_validIso639LanguageCodes.Count == 0)
				{
					LoadValidIso639LanguageCodes();
				}
				return _validIso639LanguageCodes;
			}
		}

		public static IList<IanaSubtag> ValidIso3166Regions
		{
			get
			{
				if (_validIso3166Regions.Count == 0)
				{
					LoadValidIso3166Regions();
				}
				return _validIso3166Regions;
			}
		}

		private static void LoadValidIso3166Regions()
		{
			foreach (IanaSubtag ianaSubtag in _ianaSubtags)
			{
				if (ianaSubtag.Type == "region")
				{
					_validIso3166Regions.Add(ianaSubtag);
				}
			}
			_validIso3166Regions.Sort(IanaSubtag.CompareByDescription);
		}

		public static IList<IanaSubtag> ValidRegisteredVariants
		{
			get
			{
				if (_validRegisteredVariants.Count == 0)
				{
					LoadValidRegisteredVariants();
				}
				return _validRegisteredVariants;
			}
		}

		private static void LoadValidRegisteredVariants()
		{
			foreach (IanaSubtag ianaSubtag in _ianaSubtags)
			{
				if (ianaSubtag.Type == "variant")
				{
					_validRegisteredVariants.Add(ianaSubtag);
				}
			}
			_validRegisteredVariants.Sort(IanaSubtag.CompareByDescription);
		}

		private static void LoadValidIso639LanguageCodes()
		{
			_validIso639LanguageCodes.Add(new Iso639LanguageCode("qaa", "Unknown language", "qaa"));
			foreach (IanaSubtag ianaSubtag in _ianaSubtags)
			{
				if (ianaSubtag.Type == "language")
				{
					var language = new Iso639LanguageCode(ianaSubtag.Subtag, ianaSubtag.Description, string.Empty);
					_validIso639LanguageCodes.Add(language);
				}
			}
			_validIso639LanguageCodes.Sort(Iso639LanguageCode.CompareByName);
		}

		/// <summary>
		/// parse in the text of the script registry we get from http://unicode.org/iso15924/iso15924-text.html
		/// </summary>
		private static void LoadScriptOptionsIfNeeded()
		{
			if (_validIso15924Scripts.Count != 0)
				return;

			//to help people find Latin
			_validIso15924Scripts.Add(new Iso15924Script("Roman (Latin)", "Latn"));

			LoadIanaSubtags();

			foreach (IanaSubtag ianaSubtag in _ianaSubtags)
			{
				if (ianaSubtag.Type == "script")
				{
					var script = new Iso15924Script(ianaSubtag.Description, ianaSubtag.Subtag);
					_validIso15924Scripts.Add(script);
				}
			}
			_validIso15924Scripts.Sort(Iso15924Script.CompareScriptOptions);
		}

		private static void LoadIanaSubtags()
		{
			string[] ianaSubtagsAsStrings = Resource.IanaSubtags.Split(new[] { "%%" }, StringSplitOptions.None);
			foreach (string ianaSubtagAsString in ianaSubtagsAsStrings)
			{
				string[] subTagComponents = ianaSubtagAsString.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

				if (subTagComponents[0].Contains("File-Date"))
				{
					continue;   //This is the first line of the file.
				}

				CheckIfIanaSubtagFromFileHasExpectedForm(subTagComponents);

				string type = subTagComponents[0].Split(' ')[1];
				string subtag = subTagComponents[1].Split(' ')[1];
				string description = subTagComponents[2].Split(' ')[1];

				var ianaSubtag = new IanaSubtag(type, subtag, description);
				_ianaSubtags.Add(ianaSubtag);
			}
		}

		private static void CheckIfIanaSubtagFromFileHasExpectedForm(string[] subTagComponents)
		{
			if (!subTagComponents[0].Contains("Type:"))
			{
				throw new ApplicationException(
					String.Format(
						"Unable to parse IANA subtag. First line was '{0}' when it should have denoted the type of subtag.",
						subTagComponents[0]));
			}
			if (!subTagComponents[1].Contains("Subtag:") && !subTagComponents[1].Contains("Tag:"))
			{
				throw new ApplicationException(
					String.Format(
						"Unable to parse IANA subtag. Second line was '{0}' when it should have denoted the subtag code.",
						subTagComponents[1]));
			}
			if (!subTagComponents[2].Contains("Description:"))
			{
				throw new ApplicationException(
					String.Format(
						"Unable to parse IANA subtag. Second line was '{0}' when it should have contained a description.",
						subTagComponents[2]));
			}
		}

		public RFC5646TagV1(string language, string script, string region, string variant, string privateUse)
		{
			LoadIanaSubtags();

			SetLanguageSubtags(language);
			SetScriptSubtags(script);
			SetRegionSubtags(region);
			SetVariantSubtags(variant);
			SetPrivateUseSubtags(privateUse);
			CheckIfEntireTagIsValid();
		}

		private void CheckIfEntireTagIsValid()
		{
			CheckIfLanguageTagIsValid();
			CheckIfScriptTagIsValid();
			CheckIfRegionTagIsValid();
			CheckIfVariantTagIsValid();
			CheckIfPrivateUseTagIsValid();
			bool languageIsEmpty = String.IsNullOrEmpty(Language);
			bool scriptIsEmpty = String.IsNullOrEmpty(Script);
			bool regionIsEmpty = String.IsNullOrEmpty(Region);
			bool variantIsEmpty = String.IsNullOrEmpty(Variant);
			bool privateUseEmpty = String.IsNullOrEmpty(PrivateUse);
			bool onlyPrivateUseIsNotSet = (languageIsEmpty && scriptIsEmpty && regionIsEmpty && variantIsEmpty && !privateUseEmpty);
			bool languageTagIsSetOrOnlyPrivateUseTagIsSet = !languageIsEmpty || onlyPrivateUseIsNotSet;
			if (!languageTagIsSetOrOnlyPrivateUseTagIsSet)
			{
				throw new ArgumentException("An Rfc5646 tag must have a language subtag or consist entirely of private use subtags.");
			}
		}

		///<summary>
		/// Copy constructor
		///</summary>
		///<param name="rhs"></param>
		public RFC5646TagV1(RFC5646TagV1 rhs)
			: this(rhs.Language, rhs.Script, rhs.Region, rhs.Variant, rhs.PrivateUse)
		{
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
				if (!String.IsNullOrEmpty(PrivateUse))
				{
					id += "-" + PrivateUse;
				}
				return id;
			}
		}

		public string Language
		{
			get { return AssembleSubtag(_language); }
			set
			{
				SetLanguageSubtags(value);
				CheckIfEntireTagIsValid();
			}
		}

		private void SetLanguageSubtags(string value)
		{
			_language = ParseSubtagForParts(value);
		}

		private void CheckIfLanguageTagIsValid()
		{
			if (_language.Count == 0) { return; }
			if (_language.Count > 1) { throw new ArgumentException("The language tag may not contain dashes or underscores. I.e. there may only be a single iso 639 tag in this subtag"); }
			if (!IsValidIso639LanguageCode(_language[0]))
			{
				throw new ArgumentException(String.Format("\"{0}\" is not a valid Iso-639 language code.", _language[0]));
			}
		}

		public static bool IsValidIso639LanguageCode(string languageCodeToCheck)
		{
			if (languageCodeToCheck.Equals("qaa", StringComparison.OrdinalIgnoreCase)) { return true; }

			bool partIsValidIso639LanguageCode = false;
			foreach (Iso639LanguageCode code in ValidIso639LanguageCodes)
			{
				partIsValidIso639LanguageCode =
					languageCodeToCheck.Equals(code.Code, StringComparison.OrdinalIgnoreCase) ||
					languageCodeToCheck.Equals(code.ISO3Code, StringComparison.OrdinalIgnoreCase);
				if (partIsValidIso639LanguageCode) break;
			}
			return partIsValidIso639LanguageCode;
		}

		public static bool IsValidIso15924ScriptCode(string languageCodeToCheck)
		{
			bool isValidIso15924ScriptCode = false;
			foreach (Iso15924Script script in ValidIso15924Scripts)
			{
				isValidIso15924ScriptCode =
					languageCodeToCheck.Equals(script.Code, StringComparison.OrdinalIgnoreCase);
				if (isValidIso15924ScriptCode) break;
			}
			return isValidIso15924ScriptCode;
		}

		//this is not implmented correctly as there should only be a single "x" in a language tag, so all subtags after the "x" are private use but are not indivudally prefixed with an "x"
		private bool PartIsPrivateUse(string part)
		{
			//return part.StartsWith("x-",StringComparison.OrdinalIgnoreCase) ? true : false;
			throw new NotImplementedException();
		}

		public string Script
		{
			get { return AssembleSubtag(_script); }
			set
			{
				SetScriptSubtags(value);
				CheckIfEntireTagIsValid();
			}
		}

		private void SetScriptSubtags(string value)
		{
			_script = ParseSubtagForParts(value);
		}

		public string PrivateUse
		{
			get
			{
				if (_privateUse.Count != 0)
				{
					return "x-" + AssembleSubtag(_privateUse);
				}
				return String.Empty;
			}
			set
			{
				SetPrivateUseSubtags(value);
			}
		}

		private void SetPrivateUseSubtags(string value)
		{
			_privateUse = new List<string>();
			AddToPrivateUse(value);
		}

		private void CheckIfPrivateUseTagIsValid()
		{
			bool partInMiddleOfStringStartsWithxDash = (_privateUse.FindLastIndex(part => part.StartsWith("x-")) > 0);
			if (partInMiddleOfStringStartsWithxDash) { throw new ArgumentException("A Private Use subtag may not contain a singleton 'x' anywhere but at the beginning of the subtag."); }
			CheckIfSubtagContainsDuplicates(_privateUse);
		}

		private void CheckIfSubtagContainsDuplicates(List<string> partsOfSubtag)
		{
			foreach (string partToTestForDuplicate in partsOfSubtag)
			{
				if (partToTestForDuplicate.Equals("-") || partToTestForDuplicate.Equals("_")) { continue; }
				if (partsOfSubtag.FindAll(part => part.Equals(partToTestForDuplicate, StringComparison.OrdinalIgnoreCase)).Count > 1)
				{
					throw new ArgumentException(String.Format("Subtags may never contain duplicate parts. The duplicate part was: {0}", partToTestForDuplicate));
				}
			}
		}

		private void CheckIfScriptTagIsValid()
		{

			if (_script.Count == 0) { return; }
			if (_script.Count > 1) { throw new ArgumentException("The script tag may not contain dashes or underscores. I.e. there may only be a single Iso-15924 tag in this subtag"); }
			if (!IsValidIso15924ScriptCode(_script[0]))
			{
				throw new ArgumentException(String.Format("\"{0}\" is not a valid Iso-15924 script code.", _script[0]));
			}
		}

		public string Region
		{
			get { return AssembleSubtag(_region); }
			set
			{
				SetRegionSubtags(value);
				CheckIfEntireTagIsValid();
			}
		}

		private void SetRegionSubtags(string value)
		{
			_region = ParseSubtagForParts(value);
		}

		private void CheckIfRegionTagIsValid()
		{
			if (_region.Count == 0) { return; }
			if (_region.Count > 1) { throw new ArgumentException("The region tag may not contain dashes or underscores. I.e. there may only be a single Iso-3166 tag in this subtag"); }
			if (!IsValidIso3166Region(_region[0]))
			{
				throw new ArgumentException(String.Format("\"{0}\" is not a valid Iso-3166 region code.", _region[0]));
			}
		}

		public static bool IsValidIso3166Region(string regionCodeToCheck)
		{
			bool isValidIso3166Region = false;
			foreach (IanaSubtag ianaSubtag in ValidIso3166Regions)
			{
				isValidIso3166Region =
					regionCodeToCheck.Equals(ianaSubtag.Subtag, StringComparison.OrdinalIgnoreCase);
				if (isValidIso3166Region) break;
			}
			return isValidIso3166Region;
		}

		public string Variant
		{
			get { return AssembleSubtag(_variant); }
			set
			{
				_variant.Clear();
				AddToVariant(value);
			}
		}

		private void SetVariantSubtags(string value)
		{
			_variant.Clear();
			_variant = ParseSubtagForParts(value);
		}

		private void CheckIfVariantTagIsValid()
		{
			if (_variant.Count == 0) { return; }
			foreach (string subtagPart in _variant)
			{
				if (subtagPart.Equals("-") || subtagPart.Equals("_")) { continue; }
				if (subtagPart.StartsWith("x-")) { return; }    //From here on out it is a private use tag
				if (!IsValidRegisteredVariant(subtagPart))
				{
					throw new ArgumentException(String.Format("\"{0}\" is not a valid registered variant code.",
															  subtagPart));
				}
			}
		}

		public static bool IsValidRegisteredVariant(string subtagPartToCheck)
		{
			bool isValidRegisteredVariant = false;
			foreach (IanaSubtag variant in ValidRegisteredVariants)
			{
				isValidRegisteredVariant =
					subtagPartToCheck.Equals(variant.Subtag, StringComparison.OrdinalIgnoreCase);
				if (isValidRegisteredVariant) break;
			}
			return isValidRegisteredVariant;
		}

		private void AddToSubtag(SubTag subTag, string stringToAppend)
		{
			List<string> subtagToAddTo = GetSubtag(subTag);
			List<string> partsOfStringToAdd = ParseSubtagForParts(stringToAppend);
			foreach (string part in partsOfStringToAdd)
			{
				bool subTagAlreadyContainsAtLeastOnePartOfStringToAdd = SubtagContainsPart(subTag, part);
				if (subTagAlreadyContainsAtLeastOnePartOfStringToAdd)
				{
					throw new ArgumentException(String.Format("Subtags may not contain duplicates. The subtag '{0}' was already contained.", part));
				}
				subtagToAddTo.Add(part);
			}
		}

		private static void AddSeparatorToSubtag(List<string> subtagToAddTo)
		{
			subtagToAddTo.Add("-");
		}

		private List<string> GetSubtag(SubTag subTag)
		{
			List<string> subtagToAddTo;
			switch (subTag)
			{
				case SubTag.Language:
					subtagToAddTo = _language;
					break;
				case SubTag.Script:
					subtagToAddTo = _script;
					break;
				case SubTag.Region:
					subtagToAddTo = _region;
					break;
				case SubTag.Variant:
					subtagToAddTo = _variant;
					break;
				case SubTag.PrivateUse:
					subtagToAddTo = _privateUse;
					break;
				default: throw new ApplicationException();
			}
			return subtagToAddTo;
		}

		public new string ToString()
		{
			return CompleteTag;
		}

		public override bool Equals(Object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(RFC5646TagV1)) return false;
			return Equals((RFC5646TagV1)obj);
		}

		public bool Equals(RFC5646TagV1 other)
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
				return result;
			}
		}

		private void RemoveFromSubtag(SubTag subTag, string stringToRemove)
		{
			List<string> partsOfSubtagToRemovePartFrom = GetSubtag(subTag);
			List<string> partsOfStringToRemove = ParseSubtagForParts(stringToRemove);

			foreach (string partToRemove in partsOfStringToRemove)
			{
				if (!SubtagContainsPart(subTag, partToRemove)) { continue; }
				int indexOfPartToRemove = partsOfSubtagToRemovePartFrom.FindIndex(partInSubtag => partInSubtag.Equals(partToRemove, StringComparison.OrdinalIgnoreCase));
				partsOfSubtagToRemovePartFrom.RemoveAt(indexOfPartToRemove);
			}
		}

		private static bool SubtagContainsAllPartsOfStringToBeRemoved(List<string> partsOfSubtagToRemovePartFrom, List<string> partsOfStringToRemove)
		{
			foreach (string part in partsOfStringToRemove)
			{
				if (!partsOfSubtagToRemovePartFrom.Contains(part, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}

		private static List<string> ParseSubtagForParts(string subtagToParse)
		{
			var parts = new List<string>();
			parts.AddRange(subtagToParse.Split('-'));
			parts.RemoveAll(str => str == "");
			return parts;
		}

		private static string AssembleSubtag(IEnumerable<string> subtag)
		{
			string subtagAsString = "";
			foreach (string part in subtag)
			{
				if (!String.IsNullOrEmpty(subtagAsString))
				{
					subtagAsString = subtagAsString + "-";
				}
				subtagAsString = subtagAsString + part;
			}
			return subtagAsString;
		}

		private bool SubtagContainsPart(SubTag subtagToCheck, string partToFind)
		{
			List<string> partsOfSubTag = GetSubtag(subtagToCheck);
			List<string> partsOfPart = ParseSubtagForParts(partToFind);
			foreach (string subPart in partsOfPart)
			{
				if (!partsOfSubTag.Contains(subPart, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}

		public void AddToPrivateUse(string subtagToAdd)
		{
			string stringWithoutPrecedingOrTrailingDashes = subtagToAdd.Trim('-');
			if (stringWithoutPrecedingOrTrailingDashes.StartsWith("x-")) { stringWithoutPrecedingOrTrailingDashes = stringWithoutPrecedingOrTrailingDashes.Remove(0, 2); }
			_privateUse.AddRange(ParseSubtagForParts(stringWithoutPrecedingOrTrailingDashes));
			if (_privateUse.Contains("x"))
			{
				throw new ArgumentException(
					"A Private Use subtag may only contain one 'x' at the beginning of the subtag.");
			}
			CheckIfEntireTagIsValid();
		}

		public void AddToVariant(string subtagToAdd)
		{
			AddToSubtag(SubTag.Variant, subtagToAdd);
			CheckIfEntireTagIsValid();
		}

		public void RemoveFromPrivateUse(string subtagToRemove)
		{
			string stringWithoutPrecedingxDash = subtagToRemove.Trim('-', 'x');
			RemoveFromSubtag(SubTag.PrivateUse, stringWithoutPrecedingxDash);
			CheckIfEntireTagIsValid();
		}

		public void RemoveFromVariant(string subtagToRemove)
		{
			RemoveFromSubtag(SubTag.Variant, subtagToRemove);
		}

		public bool PrivateUseContainsPart(string subTagToFind)
		{
			string stringWithoutPrecedingxDash = subTagToFind.Trim('-', 'x');
			return SubtagContainsPart(SubTag.PrivateUse, stringWithoutPrecedingxDash);
		}

		public bool VariantContainsPart(string subTagToFind)
		{
			return SubtagContainsPart(SubTag.Variant, subTagToFind);
		}
	}
}