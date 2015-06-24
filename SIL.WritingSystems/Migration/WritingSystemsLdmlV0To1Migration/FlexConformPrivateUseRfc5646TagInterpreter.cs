﻿using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Data;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	/// <summary>
	/// This is different from the algorithm used by the RfcTagCleaner because Flex puts Wellknown Scripts, Regions and Properties behind
	/// the private use "x-" marker and expects them to be treated as if non-private use. So while the Rfc5646TagCleaner would simply move
	/// "x-en-Zxxx-US-fonipa-private" to private use, this class converts it to "qaa-Zxxx-US-fonipa-x-private-en".
	/// Also this class tries to move any private use language subtag (starts with "x-" as per flex) to be the first private use tag when
	/// rearranged
	/// </summary>
	internal class FlexConformPrivateUseRfc5646TagInterpreter
	{
		private string _language = String.Empty;
		private string _script = String.Empty;
		private string _region = String.Empty;
		private string _variant = String.Empty;

		public void ConvertToPalasoConformPrivateUseRfc5646Tag(string language, string script, string region, string variant)
		{
			string newVariant = "";
			string newPrivateUse = "";

			_script = script;
			_region = region;

			if (!String.IsNullOrEmpty(variant))
			{
				IetfLanguageTag.SplitVariantAndPrivateUse(variant, out newVariant, out newPrivateUse);
			}
			IEnumerable<string> privateUseSubtagsWithoutXs = StripXs(newPrivateUse);
			IEnumerable<string> languageSubtagsWithoutXs = StripXs(language);
			newPrivateUse = String.Join("-", (languageSubtagsWithoutXs.Union(privateUseSubtagsWithoutXs)).Where(str=>!String.IsNullOrEmpty(str)).ToArray());

			_variant = IetfLanguageTag.ConcatenateVariantAndPrivateUse(newVariant, newPrivateUse);

			if (!string.IsNullOrEmpty(script) || !string.IsNullOrEmpty(region) || !string.IsNullOrEmpty(newVariant))
				_language = "qaa";
		}

		private IEnumerable<string> StripXs(string newPrivateUse)
		{
			return newPrivateUse.Split('-').Where(str => !str.Equals("x", StringComparison.OrdinalIgnoreCase)).ToArray();
		}

		public void ConvertToPalasoConformPrivateUseRfc5646Tag(string flexConformPrivateUseRfc5646Tag)
		{
			string language = String.Empty;
			string script = String.Empty;
			string region = String.Empty;
			string variant = String.Empty;
			string privateUse = String.Empty;

			var tokens = flexConformPrivateUseRfc5646Tag.Split(new[] { '-' });
			for (int position = 0; position < tokens.Length; ++position)
			{
				string currentToken = tokens[position];
				if (position == 0)
				{
					if (!currentToken.Equals("x", StringComparison.OrdinalIgnoreCase))
					{
						throw new ValidationException(String.Format("The rfctag {0} does not start with 'x-' or 'X-'.",
																	flexConformPrivateUseRfc5646Tag));
					}
					language = currentToken;
				}
				else if (position == 1 && !StandardSubtags.IsValidIso15924ScriptCode(currentToken))
				{
					language = language + '-' + currentToken;
				}
				else if (StandardSubtags.IsValidIso15924ScriptCode(currentToken))
				{
					if (!String.IsNullOrEmpty(region) || !String.IsNullOrEmpty(variant))
					{
						throw new ValidationException(
							String.Format(
								"The rfctag '{0}' contains a misplaced Script subtag (i.e. it was preceded by a region or variant subtag.",
								flexConformPrivateUseRfc5646Tag));
					}
					script = currentToken;
				}
				else if (StandardSubtags.IsValidIso3166RegionCode(currentToken))
				{
					if (!String.IsNullOrEmpty(variant))
					{
						throw new ValidationException(
							String.Format(
								"The rfctag '{0}' contains a misplaced Region subtag (i.e. it was preceded by a variant subtag.",
								flexConformPrivateUseRfc5646Tag));
					}
					region = currentToken;
				}
				else if (StandardSubtags.IsValidRegisteredVariantCode(currentToken))
				{
					variant = variant + currentToken;
				}
				else
				{
					privateUse = String.IsNullOrEmpty(privateUse) ? currentToken : privateUse + '-' + currentToken;
				}
			}
			variant = IetfLanguageTag.ConcatenateVariantAndPrivateUse(variant, privateUse);
			ConvertToPalasoConformPrivateUseRfc5646Tag(language, script, region, variant);
		}

		public string Rfc5646Tag
		{
			get { return string.Join("-", new[] { Language, Script, Region, Variant }.Select(t => t).Where(str => !String.IsNullOrEmpty(str)).ToArray()); }
		}

		public string Language
		{
			get { return _language; }
		}

		public string Script
		{
			get { return _script; }
		}

		public string Region
		{
			get { return _region; }
		}

		public string Variant
		{
			get { return _variant; }
		}
	}

}
