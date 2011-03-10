using System;
using System.Collections.Generic;

namespace Palaso.WritingSystems
{
	//This is basically a copy of Rfc5646V1 but with all the checking removed.
	//It is used as a convenient place with convenient methods to hold temporary data during migration.
	public class RFC5646TagV0 : Object
	{
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
				if(_validIso15924Scripts.Count == 0)
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
				if (_validIso639LanguageCodes.Count == 0)
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
			_validIso639LanguageCodes.Add( new Iso639LanguageCode("qaa", "Unknown language", "qaa"));
			foreach (IanaSubtag ianaSubtag in _ianaSubtags)
			{
				if (ianaSubtag.Type == "language")
				{
					var language = new Iso639LanguageCode(ianaSubtag.Subtag,ianaSubtag.Description,string.Empty);
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
				if(ianaSubtag.Type == "script")
				{
					var script = new Iso15924Script(ianaSubtag.Description, ianaSubtag.Subtag);
					_validIso15924Scripts.Add(script);
				}
			}
			_validIso15924Scripts.Sort(Iso15924Script.CompareScriptOptions);
		}

		private static void LoadIanaSubtags()
		{
			string[] ianaSubtagsAsStrings = Resource.IanaSubtags.Split(new []{"%%"}, StringSplitOptions.None);
			foreach (string ianaSubtagAsString in ianaSubtagsAsStrings)
			{
				string[] subTagComponents = ianaSubtagAsString.Split(new []{"\r\n"},StringSplitOptions.RemoveEmptyEntries);

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
			if(!subTagComponents[0].Contains("Type:"))
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

		public RFC5646TagV0(string language, string script, string region, string variant, string privateUse)
		{
			LoadIanaSubtags();

			Language = language;
			Script = script;
			Region = region;
			Variant = variant;
			PrivateUse = privateUse;
		}

		///<summary>
		/// Copy constructor
		///</summary>
		///<param name="rhs"></param>
		public RFC5646TagV0(RFC5646Tag rhs):this(rhs.Language,rhs.Script,rhs.Region,rhs.Variant, rhs.PrivateUse)
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
				_language = ParseSubtagForParts(value);
			}
		}

		private static bool IsValidIso3166Region(string regionCodeToCheck)
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

		private static bool IsValidRegisteredVariant(string subtagPartToCheck)
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

		private static bool IsValidIso639LanguageCode(string languageCodeToCheck)
		{
			if(languageCodeToCheck.Equals("qaa",StringComparison.OrdinalIgnoreCase)){return true;}

			bool partIsValidIso639LanguageCode = false;
			foreach (Iso639LanguageCode code in ValidIso639LanguageCodes)
			{
				partIsValidIso639LanguageCode =
					languageCodeToCheck.Equals(code.Code, StringComparison.OrdinalIgnoreCase) ||
					languageCodeToCheck.Equals(code.ISO3Code, StringComparison.OrdinalIgnoreCase);
				if(partIsValidIso639LanguageCode) break;
			}
			return partIsValidIso639LanguageCode;
		}

		private static bool IsValidIso15924ScriptCode(string languageCodeToCheck)
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

		public string Script
		{
			get { return AssembleSubtag(_script); }
			set
			{
				_script = ParseSubtagForParts(value);
			}
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
				_privateUse = new List<string>();
				AddToPrivateUse(value);
			}
		}

		private static bool StringContainsNonAlphaNumericCharacters(string stringToSearch)
		{
			foreach (char c in stringToSearch.ToCharArray())
			{
				if(!Char.IsLetterOrDigit(c))
				{
					return true;
				}
			}
			return false;
		}

		private void CheckIfSubtagContainsDuplicates(List<string> partsOfSubtag)
		{
			foreach (string partToTestForDuplicate in partsOfSubtag)
			{
				if (partToTestForDuplicate.Equals("-") || partToTestForDuplicate.Equals("_")) {continue; }
				if(partsOfSubtag.FindAll(part => part.Equals(partToTestForDuplicate, StringComparison.OrdinalIgnoreCase)).Count > 1)
				{
					throw new ArgumentException(String.Format("Subtags may never contain duplicate parts. The duplicate part was: {0}", partToTestForDuplicate));
				}
			}
		}

		public string Region
		{
			get { return AssembleSubtag(_region); }
			set
			{
				_region = ParseSubtagForParts(value);
			}
		}

		public string Variant
		{
			get { return AssembleSubtag(_variant); }
			set
			{
				_variant.Clear();
				_variant = ParseSubtagForParts(value);
			}
		}

		private void AddToSubtag(List<string> subtagToAddTo, string stringToAppend)
		{
			List<string> partsOfStringToAdd = ParseSubtagForParts(stringToAppend);
			foreach (string part in partsOfStringToAdd)
			{
				bool subTagAlreadyContainsAtLeastOnePartOfStringToAdd = SubtagContainsPart(subtagToAddTo, part);
				if(StringContainsNonAlphaNumericCharacters(part))
				{
					throw new ArgumentException(String.Format("Rfc5646 tags may only contain alphanumeric characters. '{0}' can not be added to the Rfc5646 tag.", part));
				}
				if (subTagAlreadyContainsAtLeastOnePartOfStringToAdd)
				{
						throw new ArgumentException(String.Format("Subtags may not contain duplicates. The subtag '{0}' was already contained.",part));
				}
				subtagToAddTo.Add(part);
			}
		}

		public new string ToString()
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
				result = (result*397) ^ (_script != null ? _script.GetHashCode() : 0);
				result = (result*397) ^ (_region != null ? _region.GetHashCode() : 0);
				result = (result*397) ^ (_variant != null ? _variant.GetHashCode() : 0);
				return result;
			}
		}

		private void RemoveFromSubtag(List<string> partsOfSubtagToRemovePartFrom, string stringToRemove)
		{
			List<string> partsOfStringToRemove = ParseSubtagForParts(stringToRemove);

			foreach (string partToRemove in partsOfStringToRemove)
			{
				if (!SubtagContainsPart(partsOfSubtagToRemovePartFrom, partToRemove)) { continue; }
				int indexOfPartToRemove = partsOfSubtagToRemovePartFrom.FindIndex(partInSubtag => partInSubtag.Equals(partToRemove, StringComparison.OrdinalIgnoreCase));
				partsOfSubtagToRemovePartFrom.RemoveAt(indexOfPartToRemove);
			}
		}

		public static List<string> ParseSubtagForParts(string subtagToParse)
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
				if(!String.IsNullOrEmpty(subtagAsString))
				{
					subtagAsString = subtagAsString + "-";
				}
				subtagAsString = subtagAsString + part;
			}
			return subtagAsString;
		}

		private static bool SubtagContainsPart(List<string> partsOfSubTag, string partToFind)
		{
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

		public bool LanguageSubtagContainsPart(string partToFind)
		{
			return SubtagContainsPart(_language, partToFind);
		}

		public bool ScriptSubtagContainsPart(string partToFind)
		{
			return SubtagContainsPart(_language, partToFind);
		}

		public bool RegionSubtagContainsPart(string partToFind)
		{
			return SubtagContainsPart(_language, partToFind);
		}

		public bool VariantSubtagContainsPart(string partToFind)
		{
			return SubtagContainsPart(_language, partToFind);
		}

		public bool PrivateUseSubtagContainsPart(string partToFind)
		{
			return SubtagContainsPart(_language, partToFind);
		}

		public List<string> GetIso15924CodesInSubtag(List<string> subtag)
		{
			List<string> foundIso15924Codes = new List<string>();
			foreach (var part in subtag)
			{
				if(IsValidIso15924ScriptCode(part))
				{
					foundIso15924Codes.Add(part);
				}
			}
			return foundIso15924Codes;
		}

		public List<string> GetIso15924CodesInLanguageSubtag()
		{
			return GetIso15924CodesInSubtag(_language);
		}

		public List<string> GetIso15924CodesInScriptSubtag()
		{
			return GetIso15924CodesInSubtag(_script);
		}

		public List<string> GetIso15924CodesInRegionSubtag()
		{
			return GetIso15924CodesInSubtag(_region);
		}

		public List<string> GetIso15924CodesInVariantSubtag()
		{
			return GetIso15924CodesInSubtag(_variant);
		}

		public List<string> GetIso15924CodesInPrivateUseSubtag()
		{
			return GetIso15924CodesInSubtag(_privateUse);
		}

		private List<string> GetIso3166RegionCodesInSubtag(List<string> subtag)
		{
			List<string> foundIso3166RegionCodes = new List<string>();
			foreach (var part in subtag)
			{
				if (IsValidIso3166Region(part))
				{
					foundIso3166RegionCodes.Add(part);
				}
			}
			return foundIso3166RegionCodes;
		}

		public List<string> GetIso3166RegionsInLanguageSubtag()
		{
			return GetIso3166RegionCodesInSubtag(_language);
		}

		public List<string> GetIso3166RegionsInScriptSubtag()
		{
			return GetIso3166RegionCodesInSubtag(_script);
		}

		public List<string> GetIso3166RegionsInRegionSubtag()
		{
			return GetIso3166RegionCodesInSubtag(_region);
		}

		public List<string> GetIso3166RegionsInVariantSubtag()
		{
			return GetIso3166RegionCodesInSubtag(_variant);
		}

		public List<string> GetIso3166RegionsInPrivateUseSubtag()
		{
			return GetIso3166RegionCodesInSubtag(_privateUse);
		}

		private List<string> GetRegisteredVariantsInSubtag(List<string> subtag)
		{
			List<string> foundRegisteredVariants = new List<string>();
			foreach (var part in subtag)
			{
				if (IsValidRegisteredVariant(part))
				{
					foundRegisteredVariants.Add(part);
				}
			}
			return foundRegisteredVariants;
		}

		public List<string> GetRegisteredVariantsInLanguageSubtag()
		{
			return GetRegisteredVariantsInSubtag(_language);
		}

		public List<string> GetRegisteredVariantsInScriptSubtag()
		{
			return GetRegisteredVariantsInSubtag(_script);
		}

		public List<string> GetRegisteredVariantsInRegionSubtag()
		{
			return GetRegisteredVariantsInSubtag(_region);
		}

		public List<string> GetRegisteredVariantsInVariantSubtag()
		{
			return GetRegisteredVariantsInSubtag(_variant);
		}

		public List<string> GetRegisteredVariantsInPrivateUseSubtag()
		{
			return GetRegisteredVariantsInSubtag(_privateUse);
		}

		public void AddToPrivateUse(string subtagToAdd)
		{
			string stringWithoutPrecedingOrTrailingDashes = subtagToAdd.Trim('-');
			if (stringWithoutPrecedingOrTrailingDashes.StartsWith("x-")) { stringWithoutPrecedingOrTrailingDashes = stringWithoutPrecedingOrTrailingDashes.Remove(0, 2); }
			_privateUse.AddRange(ParseSubtagForParts(stringWithoutPrecedingOrTrailingDashes));
			if (_privateUse.Contains("x")) {
				throw new ArgumentException(
					"A Private Use subtag may only contain one 'x' at the beginning of the subtag."); }
		}

		public void AddToVariant(string subtagToAdd)
		{
			AddToSubtag(_variant, subtagToAdd);
		}

		public void AddToLanguage(string subtagToAdd)
		{
			AddToSubtag(_language, subtagToAdd);
		}

		public void AddToScript(string subtagToAdd)
		{
			AddToSubtag(_script, subtagToAdd);
		}

		public void AddToRegion(string subtagToAdd)
		{
			AddToSubtag(_region, subtagToAdd);
		}

		public void RemoveFromLanguage(string subtagToRemove)
		{
			string stringWithoutPrecedingxDash = subtagToRemove.Trim('-', 'x');
			RemoveFromSubtag(_language, stringWithoutPrecedingxDash);
		}

		public void RemoveFromScript(string subtagToRemove)
		{
			RemoveFromSubtag(_script, subtagToRemove);
		}

		public void RemoveFromRegion(string subtagToRemove)
		{
			RemoveFromSubtag(_region, subtagToRemove);
		}

		public void RemoveFromPrivateUse(string subtagToRemove)
		{
			string stringWithoutPrecedingxDash = subtagToRemove.Trim('-', 'x');
			RemoveFromSubtag(_privateUse, stringWithoutPrecedingxDash);
		}

		public void RemoveFromVariant(string subtagToRemove)
		{
			RemoveFromSubtag(_variant, subtagToRemove);
		}

		public bool PrivateUseContainsPart(string subTagToFind)
		{
			string stringWithoutPrecedingxDash = subTagToFind.Trim('-', 'x');
			return SubtagContainsPart(_privateUse, stringWithoutPrecedingxDash);
		}

		public bool VariantContainsPart(string subTagToFind)
		{
			return SubtagContainsPart(_variant, subTagToFind);
		}

		public string VariantAndPrivateUse
		{
			get
			{
				bool privateUseIsPopulatedAndVariantIsNot = String.IsNullOrEmpty(Variant) && !String.IsNullOrEmpty(PrivateUse);
				bool variantIsPopulatedAndPrivateUseIsNot = !String.IsNullOrEmpty(Variant) && String.IsNullOrEmpty(PrivateUse);
				bool variantAndPrivateUseAreBothPopulated = !String.IsNullOrEmpty(Variant) && !String.IsNullOrEmpty(PrivateUse);
				string variantToReturn = "";
				if (variantIsPopulatedAndPrivateUseIsNot)
				{
					variantToReturn = Variant;
				}
				else if (privateUseIsPopulatedAndVariantIsNot)
				{
					variantToReturn = PrivateUse;
				}
				else if (variantAndPrivateUseAreBothPopulated)
				{
					variantToReturn = Variant + "-" + PrivateUse;
				}
				return variantToReturn;
			}
		}
	}
}