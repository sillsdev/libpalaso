using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Icu;
using JetBrains.Annotations;
using SIL.Code;
using SIL.Extensions;
using SIL.Reporting;
using static System.String;
using static SIL.Unicode.CharacterUtils;
using static SIL.WritingSystems.WellKnownSubtags;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This static utility class contains various methods for processing IETF language tags. Currently,
	/// there are no methods for accessing extended language and extension subtags.
	/// </summary>
	public static class IetfLanguageTag
	{
		private const string kInvalidTagMsg = "The IETF language tag is invalid.";

		private const string PrivateUseExpr = "[xX](-" + PrivateUseSubExpr + ")+";
		// according to RFC-5646 the private use subtag can be up to 8 characters
		// some data in alltags uses longer ones so relaxing this requirement
		private const string PrivateUseSubExpr = "[a-zA-Z0-9]{1,15}";
		// according to RFC-5646, a primary language subtag can be anywhere from 2 to 8 characters in length,
		// at this point only ISO 639 codes are allowed, which are all 2 to 3 characters in length, so we
		// use the more practical constraint of 2 to 3 characters, which allows private use ICU locales with
		// only a language defined (i.e. "xkal") to not match the regex.
		private const string LanguageExpr = "[a-zA-Z]{2,8}(-[a-zA-Z]{3}){0,3}";
		private const string SignLanguageExpr = "sgn-[a-zA-Z]{2,8}";
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
			+ "|((\\A"
		    + "((?'signlanguage'" + SignLanguageExpr + ")"
			+ "|(?'language'" + LanguageExpr + ")))"
			+ "(-(?'script'" + ScriptExpr + "))?"
			+ "(-(?'region'" + RegionExpr + "))?"
			+ "(-(?'variant'" + VariantExpr + "))?"
			+ "(-(?'extension'" + ExtensionExpr + "))?"
			+ "(-(?'privateuse'" + PrivateUseExpr + "))?\\z)";

		private static readonly Regex IcuTagPattern;
		private static readonly Regex LangTagPattern;
		private static readonly Regex LangPattern;
		private static readonly Regex SignLangPattern;
		private static readonly Regex ScriptPattern;
		private static readonly Regex RegionPattern;
		private static readonly Regex PrivateUsePattern;
		private static readonly bool UseICUForLanguageNames;

		static IetfLanguageTag()
		{
			IcuTagPattern = new Regex(IcuTagExpr, RegexOptions.ExplicitCapture);
			LangTagPattern = new Regex(LangTagExpr, RegexOptions.ExplicitCapture);
			LangPattern = new Regex("\\A(" + LanguageExpr + ")\\z", RegexOptions.ExplicitCapture);
			SignLangPattern = new Regex("\\A(" + SignLanguageExpr + ")\\z", RegexOptions.ExplicitCapture);
			ScriptPattern = new Regex("\\A(" + ScriptExpr + ")\\z", RegexOptions.ExplicitCapture);
			RegionPattern = new Regex("\\A(" + RegionExpr + ")\\z", RegexOptions.ExplicitCapture);
			PrivateUsePattern = new Regex("\\A(" + PrivateUseSubExpr + ")\\z", RegexOptions.ExplicitCapture);
			try
			{
				UseICUForLanguageNames = GetLocalizedLanguageNameFromIcu("en", "es") == "inglés";
			} catch (Exception)
			{
				// Since the whole purpose of calling GetLocalizedLanguageNameFromIcu is just to determine
				// if ICU is available, I think it is ok to say that any Exception it might throw
				// is cause for not continuing to try to use ICU.
				// Note that at least in Bloom's case of ICU not being available,
				// what you actually get thrown here is a System.IO.FileLoadException.
				// That then gets propagated out past here as a TypeInitializationException.
				UseICUForLanguageNames = false;
			}
		}

		/// <summary>
		/// Get Variant Subtags based on the code, looking up and returning subtags for standard
		/// codes or creating new ones for custom (private use) codes.
		/// </summary>
		/// <param name="variantCodes">Variant codes (see <see cref="SplitVariantAndPrivateUse")/></param>
		/// <param name="variantSubtags">List of identified variant subtags</param>
		/// <param name="variantNames">Comma-separated list of variant names to use with each
		/// private use code in variantCodes</param>
		/// <returns><c>true</c>if a <see cref="variantSubtags"/> is set to a non-null collection;
		/// <c>false</c> otherwise. (Note that if no</returns>
		[PublicAPI]
		public static bool TryGetVariantSubtags(string variantCodes, out IEnumerable<VariantSubtag> variantSubtags, string variantNames = "")
		{
			if (IsNullOrEmpty(variantCodes))
			{
				variantSubtags = Enumerable.Empty<VariantSubtag>();
				return true;
			}

			SplitVariantAndPrivateUse(variantCodes, out var standardVariantCodes,
				out var privateUseVariantCodes);
			var variantSubtagsList = new List<VariantSubtag>();
			foreach (string standardCode in standardVariantCodes.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries))
			{
				if (StandardSubtags.RegisteredVariants.TryGet(standardCode, out var variantSubtag))
				{
					variantSubtagsList.Add(variantSubtag);
				}
				else
				{
					variantSubtags = null;
					return false;
				}
			}

			var variantName = variantNames.Split(',');
			int index = 0;
			foreach (string privateUseCode in privateUseVariantCodes.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (!StandardSubtags.CommonPrivateUseVariants.TryGet(privateUseCode, out var variantSubtag))
				{
					if (!PrivateUsePattern.IsMatch(privateUseCode))
					{
						variantSubtags = null;
						return false;
					}
					if (!IsNullOrEmpty(variantNames) && index < variantName.Length)
					{
						variantSubtag = new VariantSubtag(privateUseCode, variantName[index]);
						index++;
					}
					else
					{
						variantSubtag = new VariantSubtag(privateUseCode);
					}
				}
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

			return ConcatenateVariantAndPrivateUse(Join("-", variantSubtagsArray.Where(v => !v.IsPrivateUse).Select(v => v.Code)),
				Join("-", variantSubtagsArray.Where(v => v.IsPrivateUse).Select(v => v.Code)));
		}

		[PublicAPI]
		public static bool IsValidLanguageCode(string code)
		{
			return LangPattern.IsMatch(code) || SignLangPattern.IsMatch(code);
		}

		[PublicAPI]
		public static bool IsValidScriptCode(string code)
		{
			return ScriptPattern.IsMatch(code);
		}

		[PublicAPI]
		public static bool IsValidRegionCode(string code)
		{
			return RegionPattern.IsMatch(code);
		}

		[PublicAPI]
		public static bool IsValidPrivateUseCode(string code)
		{
			return PrivateUsePattern.IsMatch(code);
		}

		/// <summary>
		/// Converts the specified ICU locale to a language tag. If the ICU locale is already a valid
		/// language tag, it will return it.
		/// </summary>
		/// <param name="icuLocale">The ICU locale.</param>
		[PublicAPI]
		public static string ToLanguageTag(string icuLocale)
		{
			if (IsNullOrEmpty(icuLocale))
				throw new ArgumentNullException(nameof(icuLocale));

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

			var locale = GetLocale(icuLocale);
			string icuLanguageCode = locale.Language;
			string languageCode;
			if (icuLanguageCode.Length == 4 && icuLanguageCode.StartsWith("x"))
				languageCode = icuLanguageCode.Substring(1);
			else
				languageCode = icuLanguageCode;
			// Some very old projects may have codes with over-long identifiers. In desperation, we truncate these.
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
				languageSubtag = new LanguageSubtag(languageCode);
			}
			if (icuLanguageCode == icuLocale)
				return Create(languageSubtag, null, null, Enumerable.Empty<VariantSubtag>());

			return Create(languageSubtag, locale.Script, locale.Country, TranslateVariantCode(locale.Variant));
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
			if (IsNullOrEmpty(variantCode))
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
					string[] subCodes = variantCode.Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
					if (subCodes.Length > 1)
					{
						foreach (var translatedCode in subCodes.SelectMany(TranslateVariantCode))
							yield return translatedCode;
					}
					else
					{
						yield return variantCode.ToLowerInvariant();
					}
					break;
			}
		}

		internal static int GetIndexOfFirstNonCommonPrivateUseVariant(IEnumerable<VariantSubtag> variantSubtags) =>
			variantSubtags.IndexOf(vst =>
				vst.IsPrivateUse && !StandardSubtags.CommonPrivateUseVariants.Contains(vst.Name));

		/// <summary>
		/// A convenience method to help consumers deal with variant and private use subtags both being stored in the Variant property.
		/// This method will search the Variant part of the BCP47 tag for an "x" extension marker and split the tag into variant and private use sections
		/// Note the complementary method "ConcatenateVariantAndPrivateUse"
		/// </summary>
		/// <param name="variantAndPrivateUse">The string containing variant and private use sections separated by an "x" private use subtag</param>
		/// <param name="variant">The resulting variant section</param>
		/// <param name="privateUse">The resulting private use section</param>
		[PublicAPI]
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
			if (IsNullOrEmpty(privateUseSubtags))
			{
				return registeredVariantSubtags;
			}
			if (!privateUseSubtags.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
			{
				privateUseSubtags = Concat("x-", privateUseSubtags);
			}

			string variantToReturn = registeredVariantSubtags;
			if (!IsNullOrEmpty(privateUseSubtags))
			{
				if (!IsNullOrEmpty(variantToReturn))
				{
					variantToReturn += "-";
				}
				variantToReturn += privateUseSubtags;
			}
			return variantToReturn;
		}

		/// <summary>
		/// Tries to create a language tag from the specified subtags.
		/// </summary>
		public static bool TryCreate(LanguageSubtag languageSubtag, ScriptSubtag scriptSubtag, RegionSubtag regionSubtag, IEnumerable<VariantSubtag> variantSubtags,
			out string langTag)
		{
			string message, paramName;
			if (!TryCreate(languageSubtag, scriptSubtag, regionSubtag, variantSubtags, out langTag, out message, out paramName))
			{
				langTag = null;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Creates a language tag from the specified subtags.
		/// </summary>
		[PublicAPI]
		public static string Create(LanguageSubtag languageSubtag, ScriptSubtag scriptSubtag, RegionSubtag regionSubtag, IEnumerable<VariantSubtag> variantSubtags,
			bool validate = true)
		{
			if (!TryCreate(languageSubtag, scriptSubtag, regionSubtag, variantSubtags,
				out var langTag, out var message, out var paramName))
			{
				if (validate)
					throw new ArgumentException(message, paramName);
			}

			return langTag;
		}

		/// <summary>
		/// Validates the creation of an IETF language tag.
		/// </summary>
		[PublicAPI]
		public static bool Validate(LanguageSubtag languageSubtag, ScriptSubtag scriptSubtag,
			RegionSubtag regionSubtag, IEnumerable<VariantSubtag> variantSubtags, out string message)
		{
			return TryCreate(languageSubtag, scriptSubtag, regionSubtag, variantSubtags, out _, out message, out _);
		}

		private static bool TryCreate(LanguageSubtag languageSubtag, ScriptSubtag scriptSubtag,
			RegionSubtag regionSubtag, IEnumerable<VariantSubtag> variantSubtags,
			out string langTag, out string message, out string paramName)
		{
			message = null;
			paramName = null;
			VariantSubtag[] variantSubtagsArray = variantSubtags.ToArray();
			if (languageSubtag == null && (scriptSubtag != null || regionSubtag != null ||
				variantSubtagsArray.Length == 0 || variantSubtagsArray.Any(v => !v.IsPrivateUse)))
			{
				message = "A language subtag is required.";
				paramName = "languageSubtag";
			}

			var sb = new StringBuilder();

			bool isCustomLanguage = false;
			if (languageSubtag != null)
			{
				// Insert non-custom language, script, region into main part of code.
				if (languageSubtag.IsPrivateUse && languageSubtag.Code != WellKnownSubtags.UnlistedLanguage)
				{
					if (!LangPattern.IsMatch(languageSubtag.Code) && !SignLangPattern.IsMatch((languageSubtag.Code)))
					{
						message = "The private use language code is invalid.";
						paramName = "languageSubtag";
					}
					sb.Append(UnlistedLanguage);
					isCustomLanguage = true;
				}
				else
				{
					sb.Append(languageSubtag.Code);
				}
			}

			bool isCustomScript = false;
			if (scriptSubtag != null && GetImplicitScriptCode(languageSubtag, regionSubtag) != scriptSubtag.Code)
			{
				if (sb.Length > 0)
					sb.Append("-");
				// Qaaa is our flag to expect a script in private-use. If the actual value is Qaaa, we need to treat it as custom,
				// so we don't confuse some other private-use tag with a custom script.
				if (scriptSubtag.IsPrivateUse && !StandardSubtags.IsPrivateUseScriptCode(scriptSubtag.Code))
				{
					if (message == null && !ScriptPattern.IsMatch(scriptSubtag.Code))
					{
						message = "The private use script code is invalid.";
						paramName = "scriptSubtag";
					}
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
					if (message == null && !RegionPattern.IsMatch(regionSubtag.Code))
					{
						message = "The private use region code is invalid.";
						paramName = "regionSubtag";
					}
					sb.Append("QM");
					isCustomRegion = true;
				}
				else
				{
					sb.Append(regionSubtag.Code);
				}
			}

			var variants = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (VariantSubtag variantSubtag in variantSubtagsArray.Where(vs => !vs.IsPrivateUse))
			{
				if (message == null && variants.Contains(variantSubtag.Code))
				{
					message = "Duplicate variants are not allowed.";
					paramName = "variantSubtags";
				}
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

			variants.Clear();
			foreach (VariantSubtag variantSubtag in variantSubtagsArray.Where(vs => vs.IsPrivateUse))
			{
				if (message == null && !PrivateUsePattern.IsMatch(variantSubtag.Code))
				{
					message = "The private use subtags contains an invalid subtag.";
					paramName = "variantSubtags";
				}
				if (message == null && variants.Contains(variantSubtag.Code))
				{
					message = "Duplicate private use subtags are not allowed.";
					paramName = "variantSubtags";
				}

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

			langTag = sb.ToString();
			return message == null;
		}

		/// <summary>
		/// Tries to create a language tag from the specified subtag codes.
		/// </summary>
		[PublicAPI]
		public static bool TryCreate(string languageCode, string scriptCode, string regionCode, string variantCodes, out string langTag)
		{
			return TryCreate(languageCode, scriptCode, regionCode, variantCodes, out langTag, out _, out _);
		}

		/// <summary>
		/// Creates a language tag from the specified subtag codes.
		/// </summary>
		[PublicAPI]
		public static string Create(string languageCode, string scriptCode, string regionCode, string variantCodes, bool validate = true)
		{
			if (!TryCreate(languageCode, scriptCode, regionCode, variantCodes,
				out var langTag, out var message, out var paramName))
			{
				if (validate)
					throw new ArgumentException(message, paramName);
			}
			return langTag;
		}

		/// <summary>
		/// Validates the creation of an IETF language tag.
		/// </summary>
		[PublicAPI]
		public static bool Validate(string languageCode, string scriptCode, string regionCode,
			string variantCodes, out string message)
		{
			return TryCreate(languageCode, scriptCode, regionCode, variantCodes, out _,
				out message, out _);
		}

		private static bool TryCreate(string languageCode, string scriptCode, string regionCode,
			string variantCodes, out string langTag, out string message, out string paramName)
		{
			message = null;
			paramName = null;
			if (IsNullOrEmpty(languageCode) && (!IsNullOrEmpty(scriptCode) || !IsNullOrEmpty(regionCode)
				|| (!IsNullOrEmpty(variantCodes) && !variantCodes.StartsWith("x-", StringComparison.InvariantCultureIgnoreCase))))
			{
				message = "A language code is required.";
				paramName = "languageCode";
			}

			var sb = new StringBuilder();
			if (!IsNullOrEmpty(languageCode))
			{
				if (!StandardSubtags.IsValidIso639LanguageCode(languageCode))
				{
					message = $"The language code [{languageCode}] is invalid.";
					paramName = "languageCode";
				}
				sb.Append(languageCode);
			}

			if (!IsNullOrEmpty(scriptCode))
			{
				if (message == null && !StandardSubtags.IsValidIso15924ScriptCode(scriptCode))
				{
					message = $"The script code [{scriptCode}] is invalid.";
					paramName = "scriptCode";
				}
				// do not include implicit script codes in the language tag
				if (GetImplicitScriptCode(languageCode, regionCode) != scriptCode)
				{
					if (sb.Length > 0)
						sb.Append("-");
					sb.Append(scriptCode);
				}
			}

			if (!IsNullOrEmpty(regionCode))
			{
				if (message == null && !StandardSubtags.IsValidIso3166RegionCode(regionCode))
				{
					message = $"The region code [{regionCode}] is invalid.";
					paramName = "regionCode";
				}
				if (sb.Length > 0)
					sb.Append("-");
				sb.Append(regionCode);
			}

			if (!IsNullOrEmpty(variantCodes))
			{
				if (message == null)
				{
					bool inPrivateUse = false;
					var variants = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
					foreach (string variantCode in variantCodes.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries))
					{
						if (variants.Contains(variantCode))
						{
							message = $"Duplicate private use codes [{variantCode}] are not allowed.";
							paramName = "variantCodes";
							break;
						}
						if (variantCode.Equals("x", StringComparison.InvariantCultureIgnoreCase))
						{
							if (inPrivateUse)
							{
								message = "Duplicate x codes are not allowed.";
								paramName = "variantCodes";
								break;
							}
							inPrivateUse = true;
							variants.Clear();
							continue;
						}
						if (!inPrivateUse && !StandardSubtags.IsValidRegisteredVariantCode(variantCode))
						{
							message = $"A variant code [{variantCode}] is invalid.";
							paramName = "variantCodes";
							break;
						}
						if (inPrivateUse && !PrivateUsePattern.IsMatch(variantCode))
						{
							message = $"A private use code [{variantCode}] is invalid.";
							paramName = "variantCodes";
						}
						variants.Add(variantCode);
					}
				}
				if (sb.Length > 0)
					sb.Append("-");
				sb.Append(variantCodes);
			}

			langTag = sb.ToString();
			return message == null;
		}

		/// <summary>
		/// Converts the specified language tag to an ICU locale.
		/// </summary>
		[PublicAPI]
		public static string ToIcuLocale(string langTag)
		{
			if (!TryGetSubtags(langTag, out var languageSubtag, out var scriptSubtag,
				    out var regionSubtag, out var variantSubtags))
			{
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			}

			var sb = new StringBuilder();
			//start with the LanguageCode
			if (languageSubtag.IsPrivateUse)
				sb.Append("x");
			sb.Append(languageSubtag.Code);

			//now add the Script if it exists
			if (scriptSubtag != null && GetImplicitScriptCode(languageSubtag, regionSubtag) != scriptSubtag.Code)
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
			if (!IsNullOrEmpty(icuVariant))
				sb.AppendFormat(regionSubtag == null ? "__{0}" : "_{0}", icuVariant);

			return sb.ToString();
		}

		/// <summary>
		/// Gets the codes of the specified language tag.
		/// </summary>
		/// <param name="langTag">The lang tag.</param>
		/// <param name="language">The language part.</param>
		/// <param name="script">The script part.</param>
		/// <param name="region">The region part.</param>
		/// <param name="variant">The variant and private-use part.</param>
		/// <returns></returns>
		public static bool TryGetParts(string langTag, out string language, out string script,
			out string region, out string variant)
		{
			if (langTag == null)
				throw new ArgumentNullException(nameof(langTag));

			if (!TryParse(langTag, out language, out script, out region, out variant))
				return false;

			return true;
		}

		private static bool TryParse(string langTag, out string language, out string script,
			out string region, out string variant)
		{
			language = null;
			script = null;
			region = null;
			variant = null;

			Match match = LangTagPattern.Match(langTag);
			if (!match.Success)
				return false;

			Group signLanguageGroup = match.Groups["signlanguage"];
			if (signLanguageGroup.Success)
			{
				language = signLanguageGroup.Value;
			}
			else
			{
				Group languageGroup = match.Groups["language"];
				if (languageGroup.Success)
				{
					// In addition to known valid codes, all the private use language codes are reasonable values.
					if (!StandardSubtags.IsValidIso639LanguageCode(languageGroup.Value) && !LanguageSubtag.IsUnlistedCode(languageGroup.Value))
						return false;
					language = languageGroup.Value;
				}
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

			var variants = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			Group variantGroup = match.Groups["variant"];
			if (variantGroup.Success)
			{
				foreach (string variantCode in variantGroup.Value.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (variants.Contains(variantCode) || !StandardSubtags.IsValidRegisteredVariantCode(variantCode))
					{
						language = null;
						script = null;
						region = null;
						return false;
					}
					variants.Add(variantCode);
				}
			}

			variants.Clear();
			Group privateUseGroup = match.Groups["privateuse"];
			if (privateUseGroup.Success)
			{
				foreach (string privateUseCode in privateUseGroup.Value.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
				{
					if (variants.Contains(privateUseCode) || privateUseCode.Equals("x", StringComparison.InvariantCultureIgnoreCase) || !PrivateUsePattern.IsMatch(privateUseCode))
					{
						language = null;
						script = null;
						region = null;
						return false;
					}
					variants.Add(privateUseCode);
				}
			}

			if (variantGroup.Success || privateUseGroup.Success)
				variant = ConcatenateVariantAndPrivateUse(variantGroup.Value, privateUseGroup.Value);

			return true;
		}

		/// <summary>
		/// Gets the subtags of the specified language tag.
		/// </summary>
		/// <param name="langTag">The language tag string</param>
		/// <param name="languageSubtag">Assigned the LanguageSubtag</param>
		/// <param name="scriptSubtag">Assigned the ScriptSubtag or null</param>
		/// <param name="regionSubtag">Assigned the RegionSubtag or null</param>
		/// <param name="variantSubtags">List of the variant and private-use subtags, or an empty list</param>
		/// <remarks>
		/// The SIL (FieldWorks) convention for private use subtags is followed:
		/// qaa for the language expects the first private use code to be a 3 letter language name abbreviation
		/// Qaaa in the Script expects the first remaining private use code to be a 4 letter script abbreviation
		/// QM in the Region expects the first remaining private use code to be a 2 letter Region abbreviation
		/// Any deviation from this will still parse, but the subtags will not return info from the private use area.
		/// </remarks>
		/// <returns></returns>
		public static bool TryGetSubtags(string langTag, out LanguageSubtag languageSubtag, out ScriptSubtag scriptSubtag,
			out RegionSubtag regionSubtag, out IEnumerable<VariantSubtag> variantSubtags)
		{
			if (langTag == null)
				throw new ArgumentNullException(nameof(langTag));

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
				privateUseCodes.AddRange(privateUseGroup.Value.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries).Skip(1));

			Group languageGroup = match.Groups["language"];
			if (languageGroup.Success)
			{
				string languageCode = languageGroup.Value;
				if (languageCode.Equals(WellKnownSubtags.UnlistedLanguage, StringComparison.OrdinalIgnoreCase))
				{
					// In our own WS dialog, we don't allow no language, but if it isn't a standard one, a language like xkal
					// produces an identifier like qaa-x-kal, and we interpret the first thing after the x as a private
					// language code (not allowed as the first three characters according to the standard).
					// If it's NOT a valid language code (e.g., too many characters), probably came from some other
					// program. Treating it as a language code will fail if we try to create such a writing system,
					// since we will detect the invalid language code. So only interpret the first element
					// after the x as a language code if it is a valid one. Otherwise, we just let qaa be the language.
					if (privateUseCodes.Count > 0 && LangPattern.IsMatch(privateUseCodes[0])
						&& !StandardSubtags.CommonPrivateUseVariants.Contains(privateUseCodes[0]))
					{
						languageSubtag = new LanguageSubtag(privateUseCodes[0]);
						privateUseCodes.RemoveAt(0);
					}
					else
					{
						languageSubtag = WellKnownSubtags.UnlistedLanguage; // We do allow just plain qaa.
					}
				}
				else
				{
					// In addition to known valid codes, all the private use language codes are reasonable values.
					if (!StandardSubtags.IsValidIso639LanguageCode(languageCode) && !LanguageSubtag.IsUnlistedCode(languageCode))
						return false;

					languageSubtag = languageCode;
				}
			}

			Group scriptGroup = match.Groups["script"];
			if (scriptGroup.Success)
			{
				string scriptCode = scriptGroup.Value;
				// Qaaa triggers convention looking for a privateUse abbreviation
				if (scriptCode.Equals("Qaaa", StringComparison.OrdinalIgnoreCase) && privateUseCodes.Count > 0
					&& ScriptPattern.IsMatch(privateUseCodes[0]))
				{
					scriptSubtag = new ScriptSubtag(privateUseCodes[0]);
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
				// QM triggers convention looking for a privateUse abbreviation
				if (regionCode.Equals("QM", StringComparison.OrdinalIgnoreCase) && privateUseCodes.Count > 0
					&& RegionPattern.IsMatch(privateUseCodes[0]))
				{
					regionSubtag = new RegionSubtag(privateUseCodes[0]);
					privateUseCodes.RemoveAt(0);
				}
				else
				{
					if (!StandardSubtags.IsValidIso3166RegionCode(regionCode))
						return false;

					regionSubtag = regionCode;
				}
			}

			if (scriptSubtag == null)
				scriptSubtag = GetImplicitScriptCode(languageSubtag, regionSubtag);

			var variants = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			var variantSubtagsList = new List<VariantSubtag>();
			Group variantGroup = match.Groups["variant"];
			if (variantGroup.Success)
			{
				foreach (string variantCode in variantGroup.Value.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries))
				{
					if (variants.Contains(variantCode))
						return false;
					if (!StandardSubtags.RegisteredVariants.TryGet(variantCode, out var variantSubtag))
						return false;
					variantSubtagsList.Add(variantSubtag);
					variants.Add(variantCode);
				}
			}

			variants.Clear();
			foreach (string privateUseCode in privateUseCodes)
			{
				if (variants.Contains(privateUseCode) || privateUseCode.Equals("x", StringComparison.InvariantCultureIgnoreCase))
					return false;
				if (!StandardSubtags.CommonPrivateUseVariants.TryGet(privateUseCode, out var variantSubtag))
					variantSubtag = new VariantSubtag(privateUseCode);
				variantSubtagsList.Add(variantSubtag);
				variants.Add(privateUseCode);
			}
			variantSubtags = variantSubtagsList;
			return true;
		}

		/// <summary>
		/// Determines whether the script of the specified language tag is implied.
		/// </summary>
		public static bool IsScriptImplied(string langTag)
		{
			if (!TryParse(langTag, out var language, out var script, out var region, out _))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));

			return IsNullOrEmpty(script) || script.Equals(GetImplicitScriptCode(language, region));
		}

		private static string GetImplicitScriptCode(LanguageSubtag languageSubtag, RegionSubtag regionSubtag)
		{
			if (languageSubtag == null || languageSubtag.IsPrivateUse)
				return null;

			string regionCode = null;
			if (regionSubtag != null && !regionSubtag.IsPrivateUse)
				regionCode = regionSubtag;
			return GetImplicitScriptCode(languageSubtag.Code, regionCode);
		}

		private static string GetImplicitScriptCode(string languageCode, string regionCode)
		{
			if (IsNullOrEmpty(languageCode))
				return null;
			string langTag = languageCode;
			if (!IsNullOrEmpty(regionCode))
				langTag += "-" + regionCode;

			if (Sldr.LanguageTags.TryGet(langTag, out var langTagInfo) ||
			    Sldr.LanguageTags.TryGet(languageCode, out langTagInfo))
			{
				return langTagInfo.ImplicitScriptCode;
			}

			return null;
		}

		/// <summary>
		/// Gets the language subtag of the specified language tag.
		/// </summary>
		[PublicAPI]
		public static LanguageSubtag GetLanguageSubtag(string langTag)
		{
			if (!TryGetSubtags(langTag, out var languageSubtag, out _, out _, out _))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			return languageSubtag;
		}

		/// <summary>
		/// Gets the script subtag of the specified language tag.
		/// </summary>
		[PublicAPI]
		public static ScriptSubtag GetScriptSubtag(string langTag)
		{
			if (!TryGetSubtags(langTag, out _, out var scriptSubtag, out _, out _))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			return scriptSubtag;
		}

		/// <summary>
		/// Gets the region subtag of the specified language tag.
		/// </summary>
		[PublicAPI]
		public static RegionSubtag GetRegionSubtag(string langTag)
		{
			if (!TryGetSubtags(langTag, out _, out _, out var regionSubtag, out _))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			return regionSubtag;
		}

		/// <summary>
		/// Gets the variant and private-use subtags of the specified language tag.
		/// </summary>
		[PublicAPI]
		public static IEnumerable<VariantSubtag> GetVariantSubtags(string langTag)
		{
			if (!TryGetSubtags(langTag, out _, out _, out _, out var variantSubtags))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			return variantSubtags;
		}

		/// <summary>
		/// Gets the language part of the specified language tag.
		/// </summary>
		[PublicAPI]
		public static string GetLanguagePart(string langTag)
		{
			if (!TryGetParts(langTag, out var language, out _, out _, out _))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			return language;
		}

		/// <summary>
		/// Gets the script part of the specified language tag.
		/// </summary>
		[PublicAPI]
		public static string GetScriptPart(string langTag)
		{
			if (!TryGetParts(langTag, out _, out var script, out _, out _))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			return script;
		}

		/// <summary>
		/// Gets the region part of the specified language tag.
		/// </summary>
		[PublicAPI]
		public static string GetRegionPart(string langTag)
		{
			if (!TryGetParts(langTag, out _, out _, out var region, out _))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			return region;
		}

		/// <summary>
		/// Gets the variant and private-use part of the specified language tag.
		/// </summary>
		[PublicAPI]
		public static string GetVariantPart(string langTag)
		{
			if (!TryGetParts(langTag, out _, out _, out _, out var variant))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			return variant;
		}

		/// <summary>
		/// Determines whether the specified language tag is valid.
		/// </summary>
		[PublicAPI]
		public static bool IsValid(string langTag)
		{
			return TryGetParts(langTag, out _, out _, out _, out _);
		}

		/// <summary>
		/// Canonicalizes the specified language tag.
		/// </summary>
		[PublicAPI]
		public static string Canonicalize(string langTag)
		{
			if (!TryGetSubtags(langTag, out var languageSubtag, out var scriptSubtag, out var regionSubtag, out var variantSubtags))
				throw new ArgumentException(kInvalidTagMsg, nameof(langTag));
			if (!TryCreate(languageSubtag, scriptSubtag, regionSubtag, variantSubtags, out var newLangTag, out var message, out _))
				throw new ArgumentException(message, nameof(langTag));
			return newLangTag;
		}

		/// <summary>
		/// This method will make the IetfLanguageTag unique compared to list of language tags passed in by
		/// appending dupl# where # is a digit that increases with the number of duplicates found.
		/// </summary>
		/// <param name="ietfLanguageTag"></param>
		/// <param name="otherLangTags"></param>
		[PublicAPI]
		public static string ToUniqueLanguageTag(string ietfLanguageTag, IEnumerable<string> otherLangTags)
		{
			// Parse for variants
			if (!TryGetSubtags(ietfLanguageTag, out var languageSubtag, out var scriptSubtag,
				    out var regionSubtag, out var variantSubtags))
			{
				throw new ArgumentException(kInvalidTagMsg, nameof(ietfLanguageTag));
			}

			List<VariantSubtag> variants = variantSubtags.ToList();

			VariantSubtag lastDuplSubtag = null;
			int duplicateNumber = 0;
			var wsLangTags = new HashSet<string>(otherLangTags, StringComparer.InvariantCultureIgnoreCase);
			while (wsLangTags.Contains(ietfLanguageTag))
			{
				if (lastDuplSubtag != null)
					variants.Remove(lastDuplSubtag);
				var curDuplSubtag = new VariantSubtag($"dupl{duplicateNumber}");
				if (!variants.Contains(curDuplSubtag))
				{
					variants.Add(curDuplSubtag);
					lastDuplSubtag = curDuplSubtag;
				}
				duplicateNumber++;
				ietfLanguageTag = Create(languageSubtag, scriptSubtag, regionSubtag, variants);
			}
			return ietfLanguageTag;
		}

		[PublicAPI]
		public static bool AreTagsEquivalent(string firstTag, string secondTag)
		{
			Guard.AgainstNullOrEmptyString(firstTag, nameof(firstTag));
			Guard.AgainstNullOrEmptyString(secondTag, nameof(secondTag));
			if (IsValid(firstTag) && IsValid(secondTag))
			{
				return Canonicalize(firstTag).Equals(Canonicalize(secondTag));
			}
			// If the tags aren't valid the only way they can be equivalent is if they are equal
			return firstTag.Equals(secondTag, StringComparison.InvariantCultureIgnoreCase);
		}

		#region From Bloom (LanguageLookupModelExtensions)
		/// <summary>
		/// A smarter way to get a name for an iso code. Recent rework on writing systems has
		/// apparently fixed many of our problems as StandardSubtags.TryGetLanguageFromIso3Code()
		/// finds 3-letter entries now. This adds fall-backs for 2-letter codes and strips off
		/// Script/Region/Variant codes. If we can't find ANY name, the out param is set to the
		/// isoCode itself, and we return false.
		/// </summary>
		/// <returns>true if it found a name</returns>
		public static bool GetBestLanguageName(string isoCode, out string name)
		{
			// BL-8081/8096: Perhaps we got in here with Script/Region/Variant tag(s).
			// Try to get a match on the part of the isoCode up to the first hyphen.
			var codeToMatch = GetGeneralCode(isoCode.ToLowerInvariant());
			if (!IsNullOrEmpty(codeToMatch))
			{
				if (StandardSubtags.TryGetLanguageFromIso3Code(codeToMatch, out var match))
				{
					name = match.Name;
					return true;
				}
				// Perhaps we only have a 2-letter code (e.g. 'fr'), in that case, this will likely find it.
				if (StandardSubtags.RegisteredLanguages.TryGet(codeToMatch, out match))
				{
					name = match.Name;
					if (codeToMatch == UnlistedLanguage)
						name += $" ({isoCode})";
					return true;
				}
			}
			name = isoCode; // At this point, the best name we can come up with is the isoCode itself.
			return false;
		}

		private static readonly Dictionary<Tuple<string, string>, string> MapIsoCodesToLanguageName =
			new Dictionary<Tuple<string, string>, string>();

		private static readonly Dictionary<string, Locale> MapCodesToIcuLocale =
			new Dictionary<string, Locale>();

		private static Locale GetLocale(string ietfLanguageTag)
		{
			string icuCode = ietfLanguageTag.Replace("-", "_");
			if (!MapCodesToIcuLocale.TryGetValue(icuCode, out var locale))
			{
				locale = new Locale(icuCode);
				MapCodesToIcuLocale.Add(icuCode, locale);
			}
			return locale;
		}

		private static string GetLocalizedLanguageNameFromIcu(string languageTag, string uiLanguageTag) =>
			GetLocale(languageTag).GetDisplayName(GetLocale(uiLanguageTag));

		/// <summary>
		/// Get the language name in the language of uiLanguageTag if possible
		/// (and if uiLanguageTag is provided).
		/// Otherwise, get the name in the language itself if possible (autonym).
		/// It that doesn't work, return the English name.
		/// If we don't know even that, return the tag as the name.
		/// Note, in most cases, we do not get the language name in uiLanguage unless
		/// using ICU or uiLanguage happens to match the system CurrentCulture.
		/// </summary>
		[PublicAPI]
		public static string GetLocalizedLanguageName(string languageTag, string uiLanguageTag)
		{
			if (IsNullOrEmpty(uiLanguageTag))
				uiLanguageTag = languageTag; // get autonym

			var generalCode = GetGeneralCode(languageTag);
			var uiLanguageCode = GetLanguagePart(uiLanguageTag);

			if (generalCode == ChineseSimplifiedTag && uiLanguageCode == "en")
			{
				// This corresponds to what we (currently) get as the "English Subtitle" in
				// GetNativeLanguageNameWithEnglishSubtitle. Not sure if it really matters here,
				// but the ICU-supplied name (e.g., used on Linux), and the EnglishName and
				// DisplayName supplied via the Windows CultureInfo are all subtly different in
				// unhelpful ways:
				// ICU: Chinese (China)
				// DisplayName: Chinese (Simplified, PRC)
				// EnglishName: Chinese (Simplified, China)
				// Ideally, we should probably either have GetNativeLanguageNameWithEnglishSubtitle
				// use this same constant or factor out the complex logic so that we always compute
				// the same value based on a single well-defined source. But it's not clear whether
				// one of those approaches is superior.
				return "Chinese (Simplified)";
			}

			if (UseICUForLanguageNames)
			{
				var name = GetLocalizedLanguageNameFromIcu(generalCode, uiLanguageCode);
				if (name != generalCode)
					return name;
			}

			var key = new Tuple<string, string>(languageTag, uiLanguageTag);
			if (MapIsoCodesToLanguageName.TryGetValue(key, out var langName))
				return langName;

			// This would be nice but does not actually reflect the way Bloom uses this code.
			//Debug.Assert(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == uiLanguageCode ||
			//	CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName == uiLanguageCode,
			//	$"The current UI language should match {nameof(uiLanguageTag)}. This method " +
			//	"depends on CultureInfo.DisplayName returning the language name in the current " +
			//	"UI language.");

			try
			{
				var ci = CultureInfo.GetCultureInfo(generalCode);
				// CultureInfo.DisplayName is known to be broken in Mono as it always returns EnglishName.
				// I can't tell that Windows behaves any differently, but maybe it does if the system is
				// installed as a Spanish language, French language, or whatever language, system.
				// If DisplayName does not do what we want (but returns the same as EnglishName), then
				// we return just NativeName if it exists.  This seems less objectionable (ethnocentric)
				// than returning the EnglishName value.
				langName = ci.DisplayName;
				if (uiLanguageCode != "en")
				{
					if (generalCode == ChineseSimplifiedTag)
						langName = ci.NativeName;
					else if (langName == ci.EnglishName)
						langName = FixBotchedNativeName(ci.NativeName);
				}
				if (IsNullOrWhiteSpace(langName))
					langName = ci.EnglishName;
				if (!ci.IsUnknownCulture())
				{
					MapIsoCodesToLanguageName.Add(key, langName);
					return langName;
				}
			}
			catch (Exception e)
			{
				// ignore exception, but log on terminal.
				System.Diagnostics.Debug.WriteLine($"GetLocalizedLanguageName ignoring exception: {e.Message}");
			}
			// We get here after either an exception was thrown or the returned CultureInfo
			// helpfully told us it is for an unknown language (instead of throwing).
			// Handle a few languages that we do know the English and native names for,
			// and that are being localized for Bloom.
			langName = GetNativeNameIfKnown(languageTag);
			if (uiLanguageCode == "en" || IsNullOrWhiteSpace(langName))
				langName = GetEnglishNameIfKnown(languageTag);
			if (IsNullOrWhiteSpace(langName))
				langName = languageTag;
			MapIsoCodesToLanguageName.Add(key, langName);
			return langName;
		}

		private static readonly Dictionary<string, string> MapIsoCodeToSubtitledLanguageName =
			new Dictionary<string, string>();

		/// <summary>
		/// Get the language name in its own language and script if possible. If it's not a Latin
		/// script, add an English name suffix.
		/// If we don't know a native name, but do know an English name, return the language code
		/// with an English name suffix.
		/// If we know nothing, return the language code.
		/// </summary>
		/// <remarks>
		/// This might be easier to implement reliably with Full ICU for a larger set of languages,
		/// but if only the Min ICU DLL is included, that information is definitely not
		/// available.
		/// This method is suitable to generate menu labels in the UI language chooser menu, which
		/// are most likely to be major languages known to both Windows and Linux.
		/// GetEnglishNameIfKnown and GetNativeNameIfKnown may need to be updated if localizations
		/// are done into regional (or national) languages of some countries.
		/// </remarks>
		[PublicAPI]
		public static string GetNativeLanguageNameWithEnglishSubtitle(string code)
		{
			if (MapIsoCodeToSubtitledLanguageName.TryGetValue(code, out var langName))
				return langName;
			string nativeName;
			var generalCode = GetGeneralCode(code);
			try
			{
				// englishNameSuffix is always an empty string if we don't need it.
				string englishNameSuffix = Empty;
				var ci = CultureInfo.GetCultureInfo(generalCode); // this may throw or produce worthless empty object
				if (NeedEnglishSuffixForLanguageName(ci))
					englishNameSuffix = $" ({GetManuallyOverriddenEnglishNameIfNeeded(code, ()=>ci.EnglishName)})";

				nativeName = FixBotchedNativeName(ci.NativeName);
				if (IsNullOrWhiteSpace(nativeName))
					nativeName = code;

				if (ci.Name != ChineseSimplifiedTag && ci.Name != ChineseTraditionalTag)
				{
					// Remove any country (or script?) names.
					var idxCountry = englishNameSuffix.LastIndexOf(" (", StringComparison.Ordinal);
					if (englishNameSuffix.Length > 0 && idxCountry > 0)
						englishNameSuffix = englishNameSuffix.Substring(0, idxCountry) + ")";
					idxCountry = nativeName.IndexOf(" (", StringComparison.Ordinal);
					if (idxCountry > 0)
						nativeName = nativeName.Substring(0, idxCountry);
				}
				else if (englishNameSuffix.Length > 0)
				{
					// I have seen more cruft after the country name a few times, so remove that
					// as well. The parenthetical expansion always seems to start "(Simplified",
					// which we want to keep. We need double close parentheses because there's one
					// open parenthesis before "Chinese" and another open parenthesis before
					// "Simplified" (which precedes ", China" or ", PRC"). Also, we don't worry
					// about the parenthetical content of the native Chinese name.
					var idxCountry = englishNameSuffix.IndexOf(", ", StringComparison.Ordinal);
					if (idxCountry > 0)
						englishNameSuffix = englishNameSuffix.Substring(0, idxCountry) + "))";
				}
				langName = nativeName + englishNameSuffix;
				if (!ci.IsUnknownCulture())
				{
					MapIsoCodeToSubtitledLanguageName.Add(code, langName);
					return langName;
				}
			}
			catch (Exception e)
			{
				// ignore exception, but log it.
				Logger.WriteError(e);
				System.Diagnostics.Debug.WriteLine($"GetNativeLanguageNameWithEnglishSubtitle ignoring exception: {e.Message}");
			}
			// We get here after either an exception was thrown or the returned CultureInfo
			// helpfully told us it is for an unknown language (instead of throwing).
			// Handle a few languages that we do know the English and native names for
			// (that are being localized for Bloom).
			if (LanguageSubtag.IsUnlistedCode(generalCode) && GetBestLanguageName(code, out langName))
				return langName;
			var englishName = GetManuallyOverriddenEnglishNameIfNeeded(code, () => GetEnglishNameIfKnown(generalCode));
			nativeName = GetNativeNameIfKnown(generalCode);
			if (IsNullOrWhiteSpace(nativeName) && IsNullOrWhiteSpace(englishName))
				langName = code;
			else if (IsNullOrWhiteSpace(nativeName))
				langName = code + " (" + englishName + ")";
			else if (IsNullOrWhiteSpace(englishName))
			{
				// I don't think this will ever happen...
				if (IsLatinChar(nativeName[0]))
					langName = nativeName;
				else
					langName = nativeName + $" ({code})";
			}
			else
			{
				if (IsLatinChar(nativeName[0]))
					langName = nativeName;
				else
					langName = nativeName + $" ({englishName})";
			}
			MapIsoCodeToSubtitledLanguageName.Add(code, langName);
			return langName;
		}

		public static string GetManuallyOverriddenEnglishNameIfNeeded(string code, Func<string> defaultOtherwise)
		{
			// We used pbu in Crowdin for some reason which is "Northern Pashto,"
			// but we want this label to just be the generic macrolanguage "Pashto."
			return code == "pbu" ? "Pashto" : defaultOtherwise();
		}

		/// <summary>
		/// Check whether we need to add an English suffix to the native language name. This is true if we don't know
		/// the native name at all or if the native name is not in a Latin alphabet.
		/// </summary>
		private static bool NeedEnglishSuffixForLanguageName(CultureInfo ci)
		{
			if (IsNullOrWhiteSpace(ci.NativeName))
				return true;
			var testChar = ci.NativeName[0];
			return ci.EnglishName != ci.NativeName && !IsLatinChar(testChar);
		}

		/// <summary>
		/// Get the language part of the given tag.
		/// </summary>
		public static string GetGeneralCode(string code)
		{
			// Though you might be tempted to simplify this by using GetLanguagePart, don't: this
			// methods works with three-letter codes even if there is a valid 2-letter code that
			// should be used instead.
			var idxCountry = code.IndexOf("-");
			if (idxCountry == -1 || code == ChineseSimplifiedTag || code == ChineseTraditionalTag)
				return code;
			return code.Substring(0, idxCountry);
		}

		/// <summary>
		/// For what languages we know about, return the English name. If we don't know anything, return null.
		/// This is called only when CultureInfo doesn't supply the information we need.
		/// </summary>
		private static string GetEnglishNameIfKnown(string code)
		{
			if (!GetBestLanguageName(code, out var englishName))
			{
				switch (code)
				{
					case "pbu":  englishName = "Pashto";  break;
					case "prs":  englishName = "Dari";             break;
					case "tpi":  englishName = "New Guinea Pidgin English"; break;
					default:     englishName = null;               break;
				}
			}
			return englishName;
		}

		/// <summary>
		/// For the languages we know about, return the native name. If we don't know anything, return null.
		/// (This applies only to languages that CultureInfo doesn't know about on at least one of Linux and
		/// Windows.)
		/// </summary>
		private static string GetNativeNameIfKnown(string code)
		{
			switch (code)
			{
				case "pbu":  return "پښتو";
				case "prs":  return "دری";
				case "tpi":  return "Tok Pisin";
				default:     return null;
			}
		}

		/// <summary>
		/// Fix any native language names that we know either .Net or Mono gets wrong.
		/// </summary>
		private static string FixBotchedNativeName(string name)
		{
			// See http://issues.bloomlibrary.org/youtrack/issue/BL-5223.
			switch (name)
			{
				// .Net gets this one wrong,but Mono gets it right.
				case "Indonesia": return "Bahasa Indonesia";

				// Although these look the same, what Windows supplies as the "Native Name"
				// Wiktionary lists it as a different word and says that the word we have
				// hardcoded here (and above in GetNativeNameIfKnown) is the correct name.
				// Wikipedia seems to agree. Interestingly, Google brings up the Wikipedia
				// info for Dari when you search for either one, even though the presumably
				// incorrect version does not actually appear on that Wikipedia page. It
				// would be nice to find someone who is an authority on this, so we could
				// report it to Microsoft as a bug if it is indeed incorrect.
				case "درى": return "دری";
				// Incorrect capitalization on older Windows OS versions.
				case "Português": return "português";
				// REVIEW: For Chinese, older Windows OS versions return 中文(中华人民共和国) instead
				// of 中文(中国) {i.e., Chinese (People's Republic of China) instead of
				// Chinese (China). Do we consider that "botched"?

				default: return name;
			}
		}
		#endregion
	}
}
