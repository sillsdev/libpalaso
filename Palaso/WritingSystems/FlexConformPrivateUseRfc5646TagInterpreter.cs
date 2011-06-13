using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;

namespace Palaso.WritingSystems
{
	public class FlexConformPrivateUseRfc5646TagInterpreter
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
				WritingSystemDefinition.SplitVariantAndPrivateUse(variant, out newVariant, out newPrivateUse);
			}
			newPrivateUse = String.Join("-", new[] { language, newPrivateUse }.Select(t => t).Where(str => !String.IsNullOrEmpty(str)).ToArray());

			_variant = WritingSystemDefinition.ConcatenateVariantAndPrivateUse(newVariant, newPrivateUse);

			if (!(String.IsNullOrEmpty(script) &&
				  String.IsNullOrEmpty(region) &&
				  String.IsNullOrEmpty(newVariant))
				)
			{
				_language = "qaa";
			}
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
						throw new ValidationException(String.Format("The rfctag {0} does not start with 'x' or 'X'.",
																	flexConformPrivateUseRfc5646Tag));
					}
					language = currentToken;
				}
				else if (position == 1 && !StandardTags.IsValidIso15924ScriptCode(currentToken))
				{
					language = language + '-' + currentToken;
					continue;
				}
				else if (StandardTags.IsValidIso15924ScriptCode(currentToken))
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
				else if (StandardTags.IsValidIso3166Region(currentToken))
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
				else if (StandardTags.IsValidRegisteredVariant(currentToken))
				{
					variant = variant + currentToken;
				}
				else if (currentToken.Equals("x", StringComparison.OrdinalIgnoreCase))
				{
					privateUse = currentToken;
				}
				else if (!String.IsNullOrEmpty(privateUse))
				{
					privateUse = privateUse + '-' + currentToken;
				}
			}
			variant = WritingSystemDefinition.ConcatenateVariantAndPrivateUse(variant, privateUse);
			ConvertToPalasoConformPrivateUseRfc5646Tag(language, script, region, variant);
		}

		public string RFC5646Tag
		{
			get { return string.Join("-", new string[] { Language, Script, Region, Variant }.Select(t => t).Where(str => !String.IsNullOrEmpty(str)).ToArray()); }
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
