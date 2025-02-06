// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Collections.Generic;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	public static class AlternateLanguageCodes
	{
		// ICU uses the ISO 639-3 language codes; xkb has at least some ISO 639-2/B codes.
		// According to http://en.wikipedia.org/wiki/ISO_639-2#B_and_T_codes there are 20 languages
		// that have both B and T codes, so we need to translate those.

		private static readonly Dictionary<string, string> s_AlternateLanguageCodes;

		static AlternateLanguageCodes()
		{
			if (s_AlternateLanguageCodes == null)
			{
				s_AlternateLanguageCodes = new Dictionary<string, string>();
				s_AlternateLanguageCodes["alb"] = "sqi"; // Albanian
				s_AlternateLanguageCodes["arm"] = "hye"; // Armenian
				s_AlternateLanguageCodes["baq"] = "eus"; // Basque
				s_AlternateLanguageCodes["bur"] = "mya"; // Burmese
				s_AlternateLanguageCodes["chi"] = "zho"; // Chinese
				s_AlternateLanguageCodes["cze"] = "ces"; // Czech
				s_AlternateLanguageCodes["dut"] = "nld"; // Dutch, Flemish
				s_AlternateLanguageCodes["fre"] = "fra"; // French
				s_AlternateLanguageCodes["geo"] = "kat"; // Georgian
				s_AlternateLanguageCodes["ger"] = "deu"; // German
				s_AlternateLanguageCodes["gre"] = "ell"; // Modern Greek (1453–)
				s_AlternateLanguageCodes["ice"] = "isl"; // Icelandic
				s_AlternateLanguageCodes["mac"] = "mkd"; // Macedonian
				s_AlternateLanguageCodes["mao"] = "mri"; // Maori
				s_AlternateLanguageCodes["may"] = "msa"; // Malay
				s_AlternateLanguageCodes["per"] = "fas"; // Persian
				s_AlternateLanguageCodes["rum"] = "ron"; // Romanian
				s_AlternateLanguageCodes["slo"] = "slk"; // Slovak
				s_AlternateLanguageCodes["tib"] = "bod"; // Tibetan
				s_AlternateLanguageCodes["wel"] = "cym"; // Welsh
			}
		}

		public static string GetLanguageCode(string langCode3letter)
		{
			return s_AlternateLanguageCodes.TryGetValue(langCode3letter, out var alternateLangCode3letter)
				? alternateLangCode3letter : langCode3letter;
		}
	}
}
