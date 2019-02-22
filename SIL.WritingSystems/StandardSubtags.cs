using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SIL.ObjectModel;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class parses the IANA subtag registry in order to provide a list of valid language, script, region and variant subtags
	/// for use by the IetfLanguageTagHelper and other classes.
	///
	/// Languages and scripts that are newer than the subtag registry may be added with AddScript and AddLanguage
	/// </summary>
	public static class StandardSubtags
	{
		static StandardSubtags()
		{	
			InitialiseIanaSubtags(LanguageRegistryResources.TwoToThreeCodes, LanguageRegistryResources.ianaSubtagRegistry);
			Iso3Languages = RegisteredLanguages.Where(l => !string.IsNullOrEmpty(l.Iso3Code)).ToDictionary(l => l.Iso3Code, StringComparer.InvariantCultureIgnoreCase);
		}

		internal static void InitialiseIanaSubtags(string twotothreecodes, string subtagregistry)
		{
			// JohnT: can't find anywhere else to document this, so here goes: TwoToThreeMap is a file adapted from
			// FieldWorks Ethnologue\Data\iso-639-3_20080804.tab, by discarding all but the first column (3-letter
			// ethnologue codes) and the fourth (two-letter IANA codes), and all the rows where the fourth column is empty.
			// I then swapped the columns. So, in this resource, the string before the tab in each line is a 2-letter
			// Iana code, and the string after it is the one we want to return as the corresponding ISO3Code.
			// The following block of code assembles these lines into a map we can use to fill this slot properly
			// when building the main table.
			var twoToThreeMap = TwoAndThreeMap(twotothreecodes, false);
			string[] ianaSubtagsAsStrings = subtagregistry.Split(new[] { "%%" }, StringSplitOptions.None);

			var languages = new List<LanguageSubtag>();
			var scripts = new List<ScriptSubtag>();
			var regions = new List<RegionSubtag>();
			var variants = new List<VariantSubtag>();
			foreach (string ianaSubtagAsString in ianaSubtagsAsStrings)
			{
				string[] subTagComponents = ianaSubtagAsString.Replace("\r\n", "\n").Split(new[] { "\n" },
					StringSplitOptions.RemoveEmptyEntries);

				if (subTagComponents[0].Contains("File-Date"))
				{
					continue;   //This is the first line of the file.
				}

				CheckIfIanaSubtagFromFileHasExpectedForm(subTagComponents);
				var descriptions = new List<string>();
				bool macrolanguage = false, deprecated = false, comment = false, collection = false;
				string type = null, subtag = null, description = null;

				foreach (string component in subTagComponents)
				{
					if (comment || String.IsNullOrEmpty(component.Trim()))
						continue;
					if (component.Split(':').Length < 2) // the description for ia (Interlingua) is spread over 2 lines
					{
						if (descriptions.Count() > 0)
						{
							description = description + component.Substring(1);
							descriptions.Clear();
							descriptions.Add(description);
						}
						continue;
					}
					string field = component.Split(':')[0];
					string value = component.Split(':')[1].Trim();

					switch (field)
					{
						case "Type":
							type = value;
							break;
						case "Subtag":
							subtag = value;
							break;
						case "Tag":
							subtag = value;
							break;
						case "Description":
							// so that the description spread over 2 lines can be appended to
							description = SubTagComponentDescription(component);
							descriptions.Add(description);
							break;
						case "Deprecated":
							deprecated = true;
							break;
						case "Scope":
							if (String.Equals(value, "macrolanguage"))
								macrolanguage = true;
							if (String.Equals(value, "collection"))
								collection = true;

							break;
						case "Comments":
							comment = true;
							break;
					}
				}
				description = descriptions.First();

				if (String.IsNullOrEmpty(subtag) || String.IsNullOrEmpty(description) || String.IsNullOrEmpty(type))
				{
					continue;
				}

				if (subtag.Contains("..") || collection) // do not add private use subtags or collections to the list
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
						if (!twoToThreeMap.TryGetValue(subtag, out iso3Code))
							iso3Code = subtag;
						languages.Add(new LanguageSubtag(subtag, description, false, iso3Code, descriptions, macrolanguage, deprecated));
						break;
					case "script":
						scripts.Add(new ScriptSubtag(subtag, description, false, deprecated));
						break;
					case "region":
						regions.Add(new RegionSubtag(subtag, description, false, deprecated));
						break;
					case "variant":
						variants.Add(new VariantSubtag(subtag, description, false, deprecated, GetVariantPrefixes(subTagComponents)));
						break;
				}
			}

			IEnumerable<LanguageSubtag> sortedLanguages = languages.OrderBy(l => Regex.Replace(l.Name, @"[^\w]", ""))
				.Concat(new[] {new LanguageSubtag(WellKnownSubtags.UnlistedLanguage, "Language Not Listed", true, string.Empty)});
			RegisteredLanguages = new KeyedList<string, LanguageSubtag>(sortedLanguages, l => l.Code, StringComparer.InvariantCultureIgnoreCase);
			RegisteredScripts = new KeyedList<string, ScriptSubtag>(scripts.OrderBy(s => s.Name), s => s.Code, StringComparer.InvariantCultureIgnoreCase);
			RegisteredRegions = new ReadOnlyKeyedCollection<string, RegionSubtag>(new KeyedList<string, RegionSubtag>(regions.OrderBy(r => r.Name), r => r.Code, StringComparer.InvariantCultureIgnoreCase));
			RegisteredVariants = new ReadOnlyKeyedCollection<string, VariantSubtag>(new KeyedList<string, VariantSubtag>(variants.OrderBy(v => v.Name), v => v.Code, StringComparer.InvariantCultureIgnoreCase));
			CommonPrivateUseVariants = new ReadOnlyKeyedCollection<string, VariantSubtag>(new KeyedList<string, VariantSubtag>(new[]
			{
				new VariantSubtag(WellKnownSubtags.IpaPhoneticPrivateUse, "Phonetic"),
				new VariantSubtag(WellKnownSubtags.IpaPhonemicPrivateUse, "Phonemic"),
				new VariantSubtag(WellKnownSubtags.AudioPrivateUse, "Audio")
			}, v => v.Code, StringComparer.InvariantCultureIgnoreCase));
		}

		private static Dictionary<string, LanguageSubtag> Iso3Languages;

		public static IKeyedCollection<string, ScriptSubtag> RegisteredScripts { get; private set; }

		public static IKeyedCollection<string, LanguageSubtag> RegisteredLanguages { get; private set; }

		public static IReadOnlyKeyedCollection<string, RegionSubtag> RegisteredRegions { get; private set; }

		public static IReadOnlyKeyedCollection<string, VariantSubtag> RegisteredVariants { get; private set; }

		public static IReadOnlyKeyedCollection<string, VariantSubtag> CommonPrivateUseVariants { get; private set; }

		private static IEnumerable<string> GetVariantPrefixes(string[] subTagComponents)
		{
			foreach (string line in subTagComponents)
			{
				if (line.StartsWith("Prefix: "))
					yield return line.Substring("Prefix: ".Length).Trim();
			}
		}

		public static void AddScript(string script, string description)
		{
			var scriptTag = new ScriptSubtag(script, description, IsPrivateUseScriptCode(script), false);
			RegisteredScripts.Add(scriptTag);
		}

		public static void AddLanguage(string code, string name, bool isPrivateUse, string iso3Code)
		{
			var languageTag = new LanguageSubtag(code, name, isPrivateUse, iso3Code);
			RegisteredLanguages.Add(languageTag);
			Iso3Languages = RegisteredLanguages.Where(l => !string.IsNullOrEmpty(l.Iso3Code)).ToDictionary(l => l.Iso3Code, StringComparer.InvariantCultureIgnoreCase);
		}

		internal static string SubTagComponentDescription(string component)
		{
			string description = component.Substring(component.IndexOf(" ", StringComparison.Ordinal) + 1);
			description = Regex.Replace(description, @"\(alias for ", "(");
			description = Regex.Replace(description, @" \(individual language\)", "");
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

		public static bool TryGetLanguageFromIso3Code(string iso3Code, out LanguageSubtag languageSubtag)
		{
			return Iso3Languages.TryGetValue(iso3Code, out languageSubtag);
		}

		public static bool IsValidIso639LanguageCode(string languageCodeToCheck)
		{
			return RegisteredLanguages.Contains(languageCodeToCheck);
		}

		public static bool IsValidIso15924ScriptCode(string scriptTagToCheck)
		{
			return RegisteredScripts.Contains(scriptTagToCheck) || IsPrivateUseScriptCode(scriptTagToCheck);
		}

		public static bool IsPrivateUseScriptCode(string scriptCode)
		{
			return scriptCode.Length == 4 && string.Compare(scriptCode, "QAAA", StringComparison.InvariantCultureIgnoreCase) >= 0
				&& string.Compare(scriptCode, "QABX", StringComparison.InvariantCultureIgnoreCase) <= 0;
		}

		public static bool IsValidIso3166RegionCode(string regionCodeToCheck)
		{
			return RegisteredRegions.Contains(regionCodeToCheck) || IsPrivateUseRegionCode(regionCodeToCheck);
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
			return regionCode.Length == 2
				&& (regionCode.Equals("AA", StringComparison.InvariantCultureIgnoreCase) || regionCode.Equals("ZZ", StringComparison.InvariantCultureIgnoreCase)
					|| (string.Compare(regionCode, "QM", StringComparison.InvariantCultureIgnoreCase) >= 0 && string.Compare(regionCode, "QZ", StringComparison.InvariantCultureIgnoreCase) <= 0)
					|| (string.Compare(regionCode, "XA", StringComparison.InvariantCultureIgnoreCase) >= 0 && string.Compare(regionCode, "XZ", StringComparison.InvariantCultureIgnoreCase) <= 0));
		}

		public static bool IsValidRegisteredVariantCode(string variantToCheck)
		{
			return RegisteredVariants.Contains(variantToCheck);
		}

		public static IDictionary<string, string> TwoAndThreeMap(string twotothreecodes, bool reverse)
		{
			var twoAndThreeLetter = new Dictionary<string, string>();
			foreach (string line in twotothreecodes.Replace("\r\n", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split('\t');
				if (reverse)
				{
					twoAndThreeLetter[items[1].Trim()] = items[0].Trim();
				}
				else
				{
					twoAndThreeLetter[items[0].Trim()] = items[1].Trim();
				}
			}
			return twoAndThreeLetter;
		}
	}
}
