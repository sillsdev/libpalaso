using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// This class parses the IANA subtag registry in order to provide a list of valid language, script, region and variant subtags
	/// for use by the Rfc5646Tag and other classes.
	/// </summary>
	public class StandardTags
	{
		public class IanaSubtag
		{
			public IanaSubtag(string type, string subtag, string description)
			{
				Type = type;
				Subtag = subtag;
				Description = description;
			}

			public override string ToString()
			{
				return Description;
			}

			public string Type { get; private set; }

			public string Subtag { get; private set; }

			public string Description { get; private set; }

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

		static StandardTags()
		{
			ValidIso15924Scripts = new List<Iso15924Script>();
			ValidIso639LanguageCodes = new List<Iso639LanguageCode>();
			ValidIso3166Regions = new List<IanaSubtag>();
			ValidRegisteredVariants = new List<IanaSubtag>();
			LoadIanaSubtags();
		}

		public static List<Iso15924Script> ValidIso15924Scripts { get; private set; }

		public static List<Iso639LanguageCode> ValidIso639LanguageCodes { get; private set; }

		public static List<IanaSubtag> ValidIso3166Regions { get; private set; }

		public static List<IanaSubtag> ValidRegisteredVariants { get; private set; }

		private static void LoadIanaSubtags()
		{
			// JohnT: can't find anywhere else to document this, so here goes: TwoToThreeMap is a file adapted from
			// FieldWorks Ethnologue\Data\iso-639-3_20080804.tab, by discarding all but the first column (3-letter
			// ethnologue codes) and the fourth (two-letter IANA codes), and all the rows where the fourth column is empty.
			// I then swapped the columns. So, in this resource, the string before the tab in each line is a 2-letter
			// Iana code, and the string after it is the one we want to return as the corresponding ISO3Code.
			// The following block of code assembles these lines into a map we can use to fill this slot properly
			// when building the main table.
			var TwoToThreeMap = new Dictionary<string, string>();
			string[] encodingPairs = Resource.TwoToThreeCodes.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string pair in encodingPairs)
			{
				var items = pair.Split('\t');
				if (items.Length != 2)
					continue;
				TwoToThreeMap[items[0]] = items[1];
			}

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
				string description = SubTagComponentDescription(subTagComponents[2]);

				if (subtag.Contains("..")) // do not add private use subtags to the list
				{
					continue;
				}

				/* Note: currently we are only using the first "Description:" line in each entry.
				 * A few script entries contain multiple Description: lines, as in the example below:
				 *
				 * Type: script
				 * Subtag: Deva
				 * Description: Devanagari
				 * Description: Nagari
				 * Added: 2005-10-16
				 *
				 * In the future it may be necessary to build a separate iana script entry collection
				 * that contains duplicate script codes, for the purposes of including all possible
				 * script Descriptions.
				 */
				switch (type)
				{
					case "language":

						string iso3Code;
						if (!TwoToThreeMap.TryGetValue(subtag, out iso3Code))
							iso3Code = String.Empty;
						ValidIso639LanguageCodes.Add(
							new Iso639LanguageCode(subtag, description, iso3Code)
							);
						break;
					case "script":
						ValidIso15924Scripts.Add(
							new Iso15924Script(description, subtag)
							);
						break;
					case "region":
						ValidIso3166Regions.Add(
							new IanaSubtag(type, subtag, description)
							);
						break;
					case "variant":
						ValidRegisteredVariants.Add(
							new IanaSubtag(type, subtag, description)
							);
						break;
				}
			}

			ValidIso639LanguageCodes.Sort(Iso639LanguageCode.CompareByName);
			ValidIso15924Scripts.Sort(Iso15924Script.CompareScriptOptions);
			ValidIso3166Regions.Sort(IanaSubtag.CompareByDescription);
			ValidRegisteredVariants.Sort(IanaSubtag.CompareByDescription);

			// Add Unlisted Language
			ValidIso639LanguageCodes.Insert(0, new Iso639LanguageCode("qaa", "Language Not Listed", String.Empty));

			// To help people find Latin as a script tag
			ValidIso15924Scripts.Insert(0, new Iso15924Script("Roman (Latin)", "Latn"));
		}

		internal static string SubTagComponentDescription(string component)
		{
			string description = component.Substring(component.IndexOf(" ") + 1);
			description = Regex.Replace(description, @"\(alias for ", "(");
			if (description[0] == '(')
			{
				// remove parens if the description begins with an open parenthesis
				description = Regex.Replace(description, @"[\(\)]", "");
			}
			description = Regex.Replace(description, @"/", "|");
			return description;
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
						subTagComponents[1]
						)
					);
			}
			if (!subTagComponents[2].Contains("Description:"))
			{
				throw new ApplicationException(
					String.Format(
						"Unable to parse IANA subtag. Second line was '{0}' when it should have contained a description.",
						subTagComponents[2]));
			}
		}

		public static bool IsValidIso639LanguageCode(string languageCodeToCheck)
		{
			if (languageCodeToCheck.Equals("qaa", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return ValidIso639LanguageCodes.Any(code => languageCodeToCheck.Equals(code.Code, StringComparison.OrdinalIgnoreCase));
		}

		public static bool IsValidIso15924ScriptCode(string scriptTagToCheck)
		{
			return IsStandardIso15924ScriptCode(scriptTagToCheck) || IsPrivateUseScriptCode(scriptTagToCheck);
		}

		public static bool IsPrivateUseScriptCode(string scriptCode)
		{
			var scriptCodeU = scriptCode.ToUpperInvariant();
			return (scriptCodeU.CompareTo("QAAA") >= 0 && scriptCodeU.CompareTo("QABX") <= 0);
		}

		public static bool IsStandardIso15924ScriptCode(string scriptTagToCheck)
		{
			return ValidIso15924Scripts.Any(
				code => scriptTagToCheck.Equals(code.Code, StringComparison.OrdinalIgnoreCase)
				);
		}

		public static bool IsValidIso3166Region(string regionCodeToCheck)
		{
			return IsStandardIso3166Region(regionCodeToCheck) || IsPrivateUseRegionCode(regionCodeToCheck);
		}

		public static bool IsStandardIso3166Region(string regionCodeToCheck)
		{
			return ValidIso3166Regions.Any(
				code => regionCodeToCheck.Equals(code.Subtag, StringComparison.OrdinalIgnoreCase)
				);
		}

		/// <summary>
		/// Determines whether the specified region code is private use. These are considered valid region codes,
		/// but not predefined ones with a known meaning.
		/// </summary>
		/// <param name="regionCode">The region code.</param>
		/// <returns>
		/// 	<c>true</c> if the region code is private use.
		/// </returns>
		public static bool IsPrivateUseRegionCode(string regionCode)
		{
			var regionCodeU = regionCode.ToUpperInvariant();
			return regionCodeU == "AA" || regionCodeU == "ZZ"
				|| (regionCodeU.CompareTo("QM") >= 0 && regionCodeU.CompareTo("QZ") <= 0)
				|| (regionCodeU.CompareTo("XA") >= 0 && regionCodeU.CompareTo("XZ") <= 0);
		}

		public static bool IsValidRegisteredVariant(string variantToCheck)
		{
			return ValidRegisteredVariants.Any(
				code => variantToCheck.Equals(code.Subtag, StringComparison.OrdinalIgnoreCase)
				);
		}
	}
}