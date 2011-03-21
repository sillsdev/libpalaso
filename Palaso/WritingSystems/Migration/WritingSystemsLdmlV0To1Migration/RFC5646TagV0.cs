using System;
using System.Collections;
using System.Collections.Generic;

namespace Palaso.WritingSystems
{
	//This is basically a copy of Rfc5646V1 but with all the checking removed.
	//It is used as a convenient place with convenient methods to hold temporary data during migration.
	public class RFC5646TagV0 : Object
	{
		public enum Subtags
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

		private List<string> _language = new List<string>();
		private List<string> _script = new List<string>();
		private List<string> _region = new List<string>();
		private List<string> _variant = new List<string>();
		private List<string> _privateUse = new List<string>();

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
				_privateUse.AddRange(ParseSubtagForParts(value));
				//AddToPrivateUse(value);
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
				subtagToAddTo.Add(part);
			}
		}

		private void SetSubtag(List<string> subtagToSet, string stringToSetTo)
		{
			subtagToSet.Clear();
			subtagToSet.AddRange(ParseSubtagForParts(stringToSetTo));
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
				//This construct is needed because we can't simpy partsOfSubtagToRemovePartFrom.Remove a string due to casing issues;
				while (SubtagContainsPart(partsOfSubtagToRemovePartFrom, partToRemove))
				{
					int indexOfPartToRemove =
						partsOfSubtagToRemovePartFrom.FindIndex(
							partInSubtag => partInSubtag.Equals(partToRemove, StringComparison.OrdinalIgnoreCase));
					partsOfSubtagToRemovePartFrom.RemoveAt(indexOfPartToRemove);
				}
			}
		}

		private void RemoveFromSubtagAtIndex(List<string> partsOfSubtagToRemovePartFrom, int indexToRemove)
		{
			partsOfSubtagToRemovePartFrom.RemoveAt(indexToRemove);
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

		public List<string> GetPartsOfSubtag(Subtags subtag)
		{
			switch (subtag)
			{
				case Subtags.Language:
					return _language;
				case Subtags.Script:
					return _script;
				case Subtags.Region:
					return _region;
				case Subtags.Variant:
					return _variant;
				case Subtags.PrivateUse:
					return _privateUse;
				default:
					throw new ApplicationException(String.Format("{0} is an invalid subtag.", subtag));
			}
		}

		public void SetSubtag(string part, Subtags subtag)
		{
			SetSubtag(GetPartsOfSubtag(subtag), part);
		}

		public void AddToSubtag(string subtagToAdd, Subtags subtag)
		{
			AddToSubtag(GetPartsOfSubtag(subtag), subtagToAdd);
		}

		public void RemoveFromSubtag(string partToRemove, Subtags subtag)
		{
			RemoveFromSubtag(GetPartsOfSubtag(subtag), partToRemove);
		}

		public void RemoveFromSubtagAtIndex(int indexToRemove, Subtags subtag)
		{
			RemoveFromSubtagAtIndex(GetPartsOfSubtag(subtag), indexToRemove);
		}

		public bool SubtagContainsPart(string partToFind, Subtags subtag)
		{
			return SubtagContainsPart(GetPartsOfSubtag(subtag), partToFind);
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