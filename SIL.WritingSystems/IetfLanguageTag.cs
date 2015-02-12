using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Icu;
using Palaso.Data;
using Palaso.Extensions;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This static utility class contains various methods for processing RFC-5646 language tags. The methods
	/// defined in this class can currently only support language tags with a single variant subtag and no
	/// extensions.
	/// </summary>
	public static class IetfLanguageTag
	{
		private const string PrivateUseExpr = "[xX](-" + PrivateUseSubExpr + ")+";
		private const string PrivateUseSubExpr = "[a-zA-Z0-9]{1,8}";
		// according to RFC-5646, a primary language subtag can be anywhere from 2 to 8 characters in length,
		// at this point only ISO 639 codes are allowed, which are all 2 to 3 characters in length, so we
		// use the more practical constraint of 2 to 3 characters, which allows private use ICU locales with
		// only a language defined (i.e. "xkal") to not match the regex.
		private const string LanguageExpr = "[a-zA-Z]{2,8}(-[a-zA-Z]{3}){0,3}";
		private const string ScriptExpr = "[a-zA-Z]{4}";
		private const string RegionExpr = "[a-zA-Z]{2}|[0-9]{3}";
		private const string VariantSubExpr = "[0-9][a-zA-Z0-9]{3}|[a-zA-Z0-9]{5,8}";
		private const string VariantExpr = "(" + VariantSubExpr + ")(-(" + VariantSubExpr + "))*";
		private const string ExtensionExpr = "[a-wyzA-WYZ](-([a-zA-Z0-9]{2,8})+)+";

		private const string IcuTagExpr = "(\\A(?'privateuse'" + PrivateUseExpr + ")\\z)"
			+ "|(\\A(?'language'" + LanguageExpr + ")"
			+ "(-(?'script'" + ScriptExpr + "))?"
			+ "(-(?'region'" + RegionExpr + "))?"
			+ "(-(?'variant'" + VariantExpr + "))?"
			+ "(-(?'extension'" + ExtensionExpr + "))?"
			+ "(-(?'privateuse'" + PrivateUseExpr + "))?\\z)";

		private const string LangTagExpr = "(\\A(?'privateuse'" + PrivateUseExpr + ")\\z)"
			+ "|(\\A(?'language'" + LanguageExpr + ")"
			+ "(-(?'script'" + ScriptExpr + "))?"
			+ "(-(?'region'" + RegionExpr + "))?"
			+ "(-(?'variant'" + VariantExpr + "))?"
			+ "(-(?'extension'" + ExtensionExpr + "))?"
			+ "(-(?'privateuse'" + PrivateUseExpr + "))?\\z)";

		private static readonly Regex IcuTagPattern;
		private static readonly Regex LangTagPattern;
		private static readonly Regex LangPattern;
		private static readonly Regex ScriptPattern;
		private static readonly Regex RegionPattern;
		private static readonly Regex PrivateUsePattern;

		static IetfLanguageTag()
		{
			IcuTagPattern = new Regex(IcuTagExpr, RegexOptions.ExplicitCapture);
			LangTagPattern = new Regex(LangTagExpr, RegexOptions.ExplicitCapture);
			LangPattern = new Regex("\\A(" + LanguageExpr + ")\\z", RegexOptions.ExplicitCapture);
			ScriptPattern = new Regex("\\A(" + ScriptExpr + ")\\z", RegexOptions.ExplicitCapture);
			RegionPattern = new Regex("\\A(" + RegionExpr + ")\\z", RegexOptions.ExplicitCapture);
			PrivateUsePattern = new Regex("\\A(" + PrivateUseSubExpr + ")\\z", RegexOptions.ExplicitCapture);
		}

		public static bool TryGetVariantSubtags(string variantCodes, out IEnumerable<VariantSubtag> variantSubtags)
		{
			if (string.IsNullOrEmpty(variantCodes))
			{
				variantSubtags = Enumerable.Empty<VariantSubtag>();
				return true;
			}

			string standardVariantCodes, privateUseVariantCodes;
			SplitVariantAndPrivateUse(variantCodes, out standardVariantCodes, out privateUseVariantCodes);
			var variantSubtagsList = new List<VariantSubtag>();
			foreach (string standardCode in standardVariantCodes.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries))
			{
				VariantSubtag variantSubtag;
				if (StandardSubtags.RegisteredVariants.TryGetItem(standardCode, out variantSubtag))
				{
					variantSubtagsList.Add(variantSubtag);
				}
				else
				{
					variantSubtags = null;
					return false;
				}
			}

			foreach (string privateUseCode in privateUseVariantCodes.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries))
			{
				VariantSubtag variantSubtag;
				if (!StandardSubtags.CommonPrivateUseVariants.TryGetItem(privateUseCode, out variantSubtag))
					variantSubtag = new VariantSubtag(privateUseCode, true);
				variantSubtagsList.Add(variantSubtag);
			}
			variantSubtags = variantSubtagsList;
			return true;
		}

		public static string GetVariantCodes(IEnumerable<VariantSubtag> variantSubtags)
		{
			VariantSubtag[] variantSubtagsArray = variantSubtags.ToArray();
			if (variantSubtagsArray.Length == 0)
				return null;

			return ConcatenateVariantAndPrivateUse(string.Join("-", variantSubtagsArray.Where(v => !v.IsPrivateUse).Select(v => v.Code)),
				string.Join("-", variantSubtagsArray.Where(v => v.IsPrivateUse).Select(v => v.Code)));
		}

		public static bool IsValidLanguageCode(string code)
		{
			return LangPattern.IsMatch(code);
		}

		public static bool IsValidScriptCode(string code)
		{
			return ScriptPattern.IsMatch(code);
		}

		public static bool IsValidRegionCode(string code)
		{
			return RegionPattern.IsMatch(code);
		}

		public static bool IsValidPrivateUseCode(string code)
		{
			return PrivateUsePattern.IsMatch(code);
		}

		/// <summary>
		/// Converts the specified ICU locale to a language tag. If the ICU locale is already a valid
		/// language tag, it will return it.
		/// </summary>
		/// <param name="icuLocale">The ICU locale.</param>
		/// <returns></returns>
		public static string ToLanguageTag(string icuLocale)
		{
			if (string.IsNullOrEmpty(icuLocale))
				throw new ArgumentNullException("icuLocale");

			if (icuLocale.Contains("-"))
			{
				Match match = IcuTagPattern.Match(icuLocale);
				if (match.Success)
				{
					// We need to check for mixed case in the language code portion.  This has been
					// observed in user data, and causes crashes later on.  See LT-11288.
					var rgs = icuLocale.Split('-');
					if (rgs[0].ToLowerInvariant() == rgs[0])
						return icuLocale;
					var bldr = new StringBuilder();
					bldr.Append(rgs[0].ToLowerInvariant());
					for (var i = 1; i < rgs.Length; ++i)
					{
						bldr.Append("-");
						bldr.Append(rgs[i].ToLowerInvariant());
					}
					icuLocale = bldr.ToString();
				}
			}

			var locale = new Locale(icuLocale);
			string icuLanguageCode = locale.Language;
			string languageCode;
			if (icuLanguageCode.Length == 4 && icuLanguageCode.StartsWith("x"))
				languageCode = icuLanguageCode.Substring(1);
			else
				languageCode = icuLanguageCode;
			// Some very old projects may have codes with over-long identifiers. In desperation we truncate these.
			// 4-letter codes starting with 'e' are a special case.
			if (languageCode.Length > 3 && !(languageCode.Length == 4 && languageCode.StartsWith("e")))
				languageCode = languageCode.Substring(0, 3);
			// The ICU locale strings in FW 6.0 allowed numbers in the language tag.  The
			// standard doesn't allow this. Map numbers to letters deterministically, even
			// though the resulting code may have no relation to reality.  (It may be a valid
			// ISO 639-3 language code that is assigned to a totally unrelated language.)
			if (languageCode.Contains('0'))
				languageCode = languageCode.Replace('0', 'a');
			if (languageCode.Contains('1'))
				languageCode = languageCode.Replace('1', 'b');
			if (languageCode.Contains('2'))
				languageCode = languageCode.Replace('2', 'c');
			if (languageCode.Contains('3'))
				languageCode = languageCode.Replace('3', 'd');
			if (languageCode.Contains('4'))
				languageCode = languageCode.Replace('4', 'e');
			if (languageCode.Contains('5'))
				languageCode = languageCode.Replace('5', 'f');
			if (languageCode.Contains('6'))
				languageCode = languageCode.Replace('6', 'g');
			if (languageCode.Contains('7'))
				languageCode = languageCode.Replace('7', 'h');
			if (languageCode.Contains('8'))
				languageCode = languageCode.Replace('8', 'i');
			if (languageCode.Contains('9'))
				languageCode = languageCode.Replace('9', 'j');
			LanguageSubtag languageSubtag;
			if (languageCode == icuLanguageCode)
			{
				languageSubtag = (languageCode.Length == 4 && languageCode.StartsWith("e"))
					? languageCode.Substring(1) : languageCode;
			}
			else
			{
				languageSubtag = new LanguageSubtag(languageCode, null, true, null);
			}
			if (icuLanguageCode == icuLocale)
				return ToLanguageTag(languageSubtag, null, null, Enumerable.Empty<VariantSubtag>());

			return ToLanguageTag(languageSubtag, locale.Script, locale.Country, TranslateVariantCode(locale.Variant));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Translates standard variant codes to their expanded (semi-human-readable) format;
		/// all others are translated using the given function.
		/// </summary>
		/// <param name="variantCode">The variant code.</param>
		/// ------------------------------------------------------------------------------------
		private static IEnumerable<VariantSubtag> TranslateVariantCode(string variantCode)
		{
			if (string.IsNullOrEmpty(variantCode))
				yield break;

			switch (variantCode)
			{
				case "IPA":
					yield return WellKnownSubtags.IpaVariant;
					break;
				case "X_ETIC":
					yield return WellKnownSubtags.IpaVariant;
					yield return WellKnownSubtags.IpaPhoneticPrivateUse;
					break;
				case "X_EMIC":
				case "EMC":
					yield return WellKnownSubtags.IpaVariant;
					yield return WellKnownSubtags.IpaPhonemicPrivateUse;
					break;
				case "X_PY":
				case "PY":
					yield return WellKnownSubtags.PinyinVariant;
					break;
				default:
					string[] subcodes = variantCode.Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
					if (subcodes.Length > 1)
					{
						foreach (string subcode in subcodes)
						{
							foreach (string translatedCode in TranslateVariantCode(subcode))
								yield return translatedCode;
						}
					}
					else
					{
						yield return variantCode.ToLowerInvariant();
					}
					break;
			}
		}

		public static int GetIndexOfFirstPrivateUseVariant(IEnumerable<VariantSubtag> variantSubtags)
		{
			int i = 0;
			foreach (VariantSubtag variantSubtag in variantSubtags)
			{
				if (variantSubtag.IsPrivateUse)
					break;
				i++;
			}
			return i;
		}

		/// <summary>
		/// A convenience method to help consumers deal with variant and private use subtags both being stored in the Variant property.
		/// This method will search the Variant part of the BCP47 tag for an "x" extension marker and split the tag into variant and private use sections
		/// Note the complementary method "ConcatenateVariantAndPrivateUse"
		/// </summary>
		/// <param name="variantAndPrivateUse">The string containing variant and private use sections seperated by an "x" private use subtag</param>
		/// <param name="variant">The resulting variant section</param>
		/// <param name="privateUse">The resulting private use section</param>
		public static void SplitVariantAndPrivateUse(string variantAndPrivateUse, out string variant, out string privateUse)
		{
			if (variantAndPrivateUse.StartsWith("x-", StringComparison.OrdinalIgnoreCase)) // Private Use at the beginning
			{
				variantAndPrivateUse = variantAndPrivateUse.Substring(2); // Strip the leading x-
				variant = "";
				privateUse = variantAndPrivateUse;
			}
			else if (variantAndPrivateUse.Contains("-x-", StringComparison.OrdinalIgnoreCase)) // Private Use from the middle
			{
				string[] partsOfVariant = variantAndPrivateUse.Split(new[] { "-x-" }, StringSplitOptions.None);
				if (partsOfVariant.Length == 1)  //Must have been a capital X
				{
					partsOfVariant = variantAndPrivateUse.Split(new[] { "-X-" }, StringSplitOptions.None);
				}
				variant = partsOfVariant[0];
				privateUse = partsOfVariant[1];
			}
			else // No Private Use, it's contains variants only
			{
				variant = variantAndPrivateUse;
				privateUse = "";
			}
		}

		/// <summary>
		/// A convenience method to help consumers deal with registeredVariantSubtags and private use subtags both being stored in the Variant property.
		/// This method will insert a "x" private use subtag between a set of registered BCP47 variants and a set of private use subtags
		/// Note the complementary method "ConcatenateVariantAndPrivateUse"
		/// </summary>
		/// <param name="registeredVariantSubtags">A set of registered variant subtags</param>
		/// <param name="privateUseSubtags">A set of private use subtags</param>
		/// <returns>The resulting combination of registeredVariantSubtags and private use.</returns>
		public static string ConcatenateVariantAndPrivateUse(string registeredVariantSubtags, string privateUseSubtags)
		{
			if (string.IsNullOrEmpty(privateUseSubtags))
			{
				return registeredVariantSubtags;
			}
			if (!privateUseSubtags.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
			{
				privateUseSubtags = string.Concat("x-", privateUseSubtags);
			}

			string variantToReturn = registeredVariantSubtags;
			if (!string.IsNullOrEmpty(privateUseSubtags))
			{
				if (!string.IsNullOrEmpty(variantToReturn))
				{
					variantToReturn += "-";
				}
				variantToReturn += privateUseSubtags;
			}
			return variantToReturn;
		}

		/// <summary>
		/// Generates a language tag from the specified subtags.
		/// </summary>
		public static string ToLanguageTag(LanguageSubtag languageSubtag, ScriptSubtag scriptSubtag, RegionSubtag regionSubtag, IEnumerable<VariantSubtag> variantSubtags, bool validate = true)
		{
			VariantSubtag[] variantSubtagsArray = variantSubtags.ToArray();
			if (validate && languageSubtag == null && (scriptSubtag != null || regionSubtag != null || variantSubtagsArray.Any(v => !v.IsPrivateUse)))
				throw new ValidationException("A language subtag is required.");
			if (validate && languageSubtag == null && variantSubtagsArray.Length == 0)
				throw new ValidationException("A language subtag is required.");

			var sb = new StringBuilder();

			bool isCustomLanguage = false;
			if (languageSubtag != null)
			{
				// Insert non-custom language, script, region into main part of code.
				if (languageSubtag.IsPrivateUse && languageSubtag.Code != WellKnownSubtags.UnlistedLanguage)
				{
					if (validate && !PrivateUsePattern.IsMatch(languageSubtag.Code))
						throw new ValidationException("The private use language code is invalid.");
					sb.Append("qaa");
					isCustomLanguage = true;
				}
				else
				{
					sb.Append(languageSubtag.Code);
				}
			}

			bool isCustomScript = false;
			if (scriptSubtag != null)
			{
				if (sb.Length > 0)
					sb.Append("-");
				// Qaaa is our flag to expect a script in private-use. If the actual value is Qaaa, we need to treat it as custom,
				// so we don't confuse some other private-use tag with a custom script.
				if (scriptSubtag.IsPrivateUse && !StandardSubtags.IsPrivateUseScriptCode(scriptSubtag.Code))
				{
					if (validate && !PrivateUsePattern.IsMatch(scriptSubtag.Code))
						throw new ValidationException("The private use script code is invalid.");
					sb.Append("Qaaa");
					isCustomScript = true;
				}
				else
				{
					sb.Append(scriptSubtag.Code);
				}
			}

			bool isCustomRegion = false;
			if (regionSubtag != null)
			{
				if (sb.Length > 0)
					sb.Append("-");
				// QM is our flag to expect a region in private-use. If the actual value is QM, we need to treat it as custom,
				// so we don't confuse some other private-use tag with a custom region.
				if (regionSubtag.IsPrivateUse && !StandardSubtags.IsPrivateUseRegionCode(regionSubtag.Code))
				{
					if (validate && !PrivateUsePattern.IsMatch(regionSubtag.Code))
						throw new ValidationException("The private use region code is invalid.");
					sb.Append("QM");
					isCustomRegion = true;
				}
				else
				{
					sb.Append(regionSubtag.Code);
				}
			}

			var variants = new HashSet<string>();
			foreach (VariantSubtag variantSubtag in variantSubtagsArray.Where(vs => !vs.IsPrivateUse))
			{
				if (validate && variants.Contains(variantSubtag.Code))
					throw new ValidationException("Duplicate variants are not allowed.");
				if (sb.Length > 0)
					sb.Append("-");
				sb.Append(variantSubtag.Code);
				variants.Add(variantSubtag.Code);
			}

			// Insert custom language, script, or variant into private=use.
			bool inPrivateUse = false;
			if (isCustomLanguage)
			{
				inPrivateUse = true;
				if (sb.Length > 0)
					sb.Append("-");
				sb.Append("x-");
				sb.Append(languageSubtag.Code);
			}

			if (isCustomScript)
			{
				if (sb.Length > 0)
					sb.Append("-");
				if (!inPrivateUse)
				{
					inPrivateUse = true;
					sb.Append("x-");
				}
				sb.Append(scriptSubtag.Code);
			}

			if (isCustomRegion)
			{
				if (sb.Length > 0)
					sb.Append("-");
				if (!inPrivateUse)
				{
					inPrivateUse = true;
					sb.Append("x-");
				}
				sb.Append(regionSubtag.Code);
			}
			else if (languageSubtag != null && languageSubtag.Code == "zh" && languageSubtag.Iso3Code == "cmn" && regionSubtag == null)
			{
				if (sb.Length > 0)
					sb.Append("-");
				sb.Append("CN");
			}

			variants.Clear();
			foreach (VariantSubtag variantSubtag in variantSubtagsArray.Where(vs => vs.IsPrivateUse))
			{
				if (validate && !PrivateUsePattern.IsMatch(variantSubtag.Code))
					throw new ValidationException("The private use subtags contains an invalid subtag.");
				if (validate && variants.Contains(variantSubtag.Code))
					throw new ValidationException("Duplicate private use subtags are not allowed.");

				if (sb.Length > 0)
					sb.Append("-");
				if (!inPrivateUse)
				{
					inPrivateUse = true;
					sb.Append("x-");
				}
				sb.Append(variantSubtag.Code);
				variants.Add(variantSubtag.Code);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Generates a language tag from the specified codes.
		/// </summary>
		public static string ToLanguageTag(string languageCode, string scriptCode, string regionCode, string variantCodes, bool validate = true)
		{
			var sb = new StringBuilder();
			if (!string.IsNullOrEmpty(languageCode))
			{
				if (validate && !StandardSubtags.IsValidIso639LanguageCode(languageCode))
					throw new ValidationException("The language code is invalid.");
				sb.Append(languageCode);
			}

			if (!string.IsNullOrEmpty(scriptCode))
			{
				if (validate && string.IsNullOrEmpty(languageCode))
					throw new ValidationException("A language code is required.");
				if (validate && !StandardSubtags.IsValidIso15924ScriptCode(scriptCode))
					throw new ValidationException("The script code is invalid.");
				if (sb.Length > 0)
					sb.Append("-");
				sb.Append(scriptCode);
			}

			if (!string.IsNullOrEmpty(regionCode))
			{
				if (validate && string.IsNullOrEmpty(languageCode))
					throw new ValidationException("A language code is required.");
				if (validate && !StandardSubtags.IsValidIso3166RegionCode(regionCode))
					throw new ValidationException("The region code is invaild.");
				if (sb.Length > 0)
					sb.Append("-");
				sb.Append(regionCode);
			}

			if (!string.IsNullOrEmpty(variantCodes))
			{
				if (validate && string.IsNullOrEmpty(languageCode) && !variantCodes.StartsWith("x-", StringComparison.InvariantCultureIgnoreCase))
					throw new ValidationException("A language code is required.");
				foreach (string variantCode in variantCodes.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries))
				{
					if (variantCode.Equals("x", StringComparison.InvariantCultureIgnoreCase))
						break;
					if (validate && !StandardSubtags.IsValidRegisteredVariantCode(variantCode))
						throw new ValidationException("A variant code is invalid.");
				}
				if (sb.Length > 0)
					sb.Append("-");
				sb.Append(variantCodes);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Converts the specified language tag to an ICU locale.
		/// </summary>
		/// <param name="langTag">The language tag.</param>
		/// <returns></returns>
		public static string ToIcuLocale(string langTag)
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			if (!TryGetSubtags(langTag, out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags))
				throw new ValidationException("The language tag is invalid.");
			return ToIcuLocale(languageSubtag, scriptSubtag, regionSubtag, variantSubtags);
		}

		/// <summary>
		/// Generates an ICU locale from the specified language tag subtags.
		/// </summary>
		/// <param name="languageSubtag">The language subtag.</param>
		/// <param name="scriptSubtag">The script subtag.</param>
		/// <param name="regionSubtag">The region subtag.</param>
		/// <param name="variantSubtags">The variant subtag.</param>
		/// <returns></returns>
		public static string ToIcuLocale(LanguageSubtag languageSubtag, ScriptSubtag scriptSubtag, RegionSubtag regionSubtag, IEnumerable<VariantSubtag> variantSubtags)
		{
			if (languageSubtag == null)
				throw new ValidationException("A language subtag is required.");

			var sb = new StringBuilder();
			//start with the LanguageCode
			if (languageSubtag.IsPrivateUse)
				sb.Append("x");
			sb.Append(languageSubtag.Code);

			//now add the Script if it exists
			if (scriptSubtag != null)
				sb.AppendFormat("_{0}", scriptSubtag.Code);

			//now add the Region if it exists
			if (regionSubtag != null)
				sb.AppendFormat("_{0}", regionSubtag.Code);

			// convert language tag variants to known ICU variants
			// TODO: are there any more ICU variants?
			var variantCodes = new HashSet<string>(variantSubtags.Select(v => v.Code));
			string icuVariant = null;
			if (variantCodes.Contains(WellKnownSubtags.IpaVariant))
			{
				if (variantCodes.Contains(WellKnownSubtags.IpaPhoneticPrivateUse))
					icuVariant = "X_ETIC";
				else if (variantCodes.Contains(WellKnownSubtags.IpaPhonemicPrivateUse))
					icuVariant = "X_EMIC";
				else
					icuVariant = "IPA";
			}
			else if (variantCodes.Contains(WellKnownSubtags.PinyinVariant))
			{
				icuVariant = "X_PY";
			}
			if (!string.IsNullOrEmpty(icuVariant))
				sb.AppendFormat(regionSubtag == null ? "__{0}" : "_{0}", icuVariant);

			return sb.ToString();
		}

		/// <summary>
		/// Generates an ICU locale from the specified language tag codes.
		/// </summary>
		/// <param name="languageCode">The language code.</param>
		/// <param name="scriptCode">The script code.</param>
		/// <param name="regionCode">The region code.</param>
		/// <param name="variantCodes">The variant code.</param>
		/// <returns></returns>
		public static string ToIcuLocale(string languageCode, string scriptCode, string regionCode, string variantCodes)
		{
			if (string.IsNullOrEmpty(languageCode))
				throw new ValidationException("A language code is required.");

			IEnumerable<VariantSubtag> variantSubtags;
			if (!TryGetVariantSubtags(variantCodes, out variantSubtags))
				throw new ValidationException("A variant code is invalid.");

			return ToIcuLocale(languageCode, scriptCode, regionCode, variantCodes);
		}

		/// <summary>
		/// Gets the codes of the specified language tag.
		/// </summary>
		/// <param name="langTag">The lang tag.</param>
		/// <param name="language">The language part.</param>
		/// <param name="script">The script part.</param>
		/// <param name="region">The region part.</param>
		/// <param name="variant">The variant part.</param>
		/// <returns></returns>
		public static bool GetParts(string langTag, out string language, out string script, out string region, out string variant)
		{
			language = null;
			script = null;
			region = null;
			variant = null;

			Match match = LangTagPattern.Match(langTag);
			if (!match.Success)
				return false;

			Group languageGroup = match.Groups["language"];
			if (languageGroup.Success)
			{
				if (!StandardSubtags.IsValidIso639LanguageCode(languageGroup.Value))
					return false;
				language = languageGroup.Value;
			}

			Group scriptGroup = match.Groups["script"];
			if (scriptGroup.Success)
			{
				if (!StandardSubtags.IsValidIso15924ScriptCode(scriptGroup.Value))
				{
					language = null;
					return false;
				}
				script = scriptGroup.Value;
			}

			Group regionGroup = match.Groups["region"];
			if (regionGroup.Success)
			{
				if (!StandardSubtags.IsValidIso3166RegionCode(regionGroup.Value))
				{
					language = null;
					script = null;
					return false;
				}
				region = regionGroup.Value;
			}

			string variantCodes;
			Group variantGroup = match.Groups["variant"];
			if (variantGroup.Success)
			{
				if (variantGroup.Value.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries).Any(v => !StandardSubtags.IsValidRegisteredVariantCode(v)))
				{
					language = null;
					script = null;
					region = null;
					return false;
				}
				variantCodes = variantGroup.Value;
			}
			else
			{
				variantCodes = string.Empty;
			}

			Group privateUseGroup = match.Groups["privateuse"];
			string privateUseCodes = privateUseGroup.Success ? privateUseGroup.Value : string.Empty;

			if (!string.IsNullOrEmpty(variantCodes) || !string.IsNullOrEmpty(privateUseCodes))
				variant = ConcatenateVariantAndPrivateUse(variantCodes, privateUseCodes);

			return true;
		}

		/// <summary>
		/// Gets the subtags of the specified language tag.
		/// </summary>
		/// <param name="langTag">The language tag.</param>
		/// <param name="languageSubtag">The language subtag.</param>
		/// <param name="scriptSubtag">The script subtag.</param>
		/// <param name="regionSubtag">The region subtag.</param>
		/// <param name="variantSubtags">The variant subtag.</param>
		/// <returns></returns>
		public static bool TryGetSubtags(string langTag, out LanguageSubtag languageSubtag, out ScriptSubtag scriptSubtag,
			out RegionSubtag regionSubtag, out IEnumerable<VariantSubtag> variantSubtags)
		{
			if (string.IsNullOrEmpty(langTag))
				throw new ArgumentNullException("langTag");

			languageSubtag = null;
			scriptSubtag = null;
			regionSubtag = null;
			variantSubtags = null;

			Match match = LangTagPattern.Match(langTag);
			if (!match.Success)
				return false;

			var privateUseCodes = new List<string>();
			Group privateUseGroup = match.Groups["privateuse"];
			if (privateUseGroup.Success)
				privateUseCodes.AddRange(privateUseGroup.Value.Substring(2).Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries));

			Group languageGroup = match.Groups["language"];
			if (languageGroup.Success)
			{
				string languageCode = languageGroup.Value;
				if (languageCode.Equals(WellKnownSubtags.UnlistedLanguage, StringComparison.OrdinalIgnoreCase))
				{
					// In our own WS dialog, we don't allow no language, but if it isn't a standard one, a language like xkal
					// produces an identifier like qaa-x-kal, and we interepret the first thing after the x as a private
					// language code (not allowed as the first three characters according to the standard).
					// If it's NOT a valid language code (e.g., too many characters), probably came from some other
					// program. Treating it as a language code will fail if we try to create such a writing system,
					// since we will detect the invalid language code. So only interpret the first element
					// after the x as a language code if it is a valid one. Otherwise, we just let qaa be the language.
					if (privateUseCodes.Count > 0 && LangPattern.IsMatch(privateUseCodes[0]))
					{
						languageSubtag = new LanguageSubtag(privateUseCodes[0], true);
						privateUseCodes.RemoveAt(0);
					}
					else
					{
						languageSubtag = WellKnownSubtags.UnlistedLanguage; // We do allow just plain qaa.
					}
				}
				else
				{
					if (!StandardSubtags.IsValidIso639LanguageCode(languageCode))
						return false;

					languageSubtag = languageCode;
				}
			}
			else if (LangPattern.IsMatch(privateUseCodes[0]))
			{
				// in this case, the language subtag should always be private use
				languageSubtag = new LanguageSubtag(privateUseCodes[0], true);
				privateUseCodes.RemoveAt(0);
				if (privateUseCodes.Count > 0 && ScriptPattern.IsMatch(privateUseCodes[0]))
				{
					scriptSubtag = privateUseCodes[0];
					privateUseCodes.RemoveAt(0);
				}
				if (privateUseCodes.Count > 0 && RegionPattern.IsMatch(privateUseCodes[0]))
				{
					regionSubtag = privateUseCodes[0];
					privateUseCodes.RemoveAt(0);
				}
			}
			else
			{
				return false;
			}

			Group scriptGroup = match.Groups["script"];
			if (scriptGroup.Success)
			{
				string scriptCode = scriptGroup.Value;
				if (scriptCode.Equals("Qaaa", StringComparison.OrdinalIgnoreCase) && privateUseCodes.Count > 0)
				{
					scriptSubtag = new ScriptSubtag(privateUseCodes[0], true);
					privateUseCodes.RemoveAt(0);
				}
				else
				{
					if (!StandardSubtags.IsValidIso15924ScriptCode(scriptCode))
						return false;

					scriptSubtag = scriptCode;
				}
			}

			Group regionGroup = match.Groups["region"];
			if (regionGroup.Success)
			{
				string regionCode = regionGroup.Value;
				if (regionCode.Equals("QM", StringComparison.OrdinalIgnoreCase) && privateUseCodes.Count > 0)
				{
					regionSubtag = new RegionSubtag(privateUseCodes[0], true);
					privateUseCodes.RemoveAt(0);
				}
				else
				{
					if (!StandardSubtags.IsValidIso3166RegionCode(regionCode))
						return false;

					regionSubtag = regionCode;
				}
			}

			var variantSubtagsList = new List<VariantSubtag>();
			Group variantGroup = match.Groups["variant"];
			if (variantGroup.Success)
			{
				foreach (string variantCode in variantGroup.Value.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries))
				{
					VariantSubtag variantSubtag;
					if (!StandardSubtags.RegisteredVariants.TryGetItem(variantCode, out variantSubtag))
						return false;
					variantSubtagsList.Add(variantSubtag);
				}
			}

			foreach (string privateUseCode in privateUseCodes.Where(c => !c.Equals("x", StringComparison.InvariantCultureIgnoreCase)))
			{
				VariantSubtag variantSubtag;
				if (!StandardSubtags.CommonPrivateUseVariants.TryGetItem(privateUseCode, out variantSubtag))
					variantSubtag = new VariantSubtag(privateUseCode, true);
				variantSubtagsList.Add(variantSubtag);
			}
			variantSubtags = variantSubtagsList;
			return true;
		}

		/// <summary>
		/// Determines whether the specified language tag is valid.
		/// </summary>
		/// <param name="langTag">The language tag.</param>
		/// <returns>
		/// 	<c>true</c> if the specified language tag is valid; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsValid(string langTag)
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			return TryGetSubtags(langTag, out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags);
		}
	}
}
