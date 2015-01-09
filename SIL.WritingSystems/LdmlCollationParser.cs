using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SIL.WritingSystems
{
	public class LdmlCollationParser
	{
		private const string NewLine = "\r\n";
		private static readonly Regex UnicodeEscape4Digit = new Regex(@"\\[u]([0-9A-F]{4})", RegexOptions.IgnoreCase);
		private static readonly Regex UnicodeEscape8Digit = new Regex(@"\\[U]([0-9A-F]{8})", RegexOptions.IgnoreCase);
		private static readonly Dictionary<string, string> StrengthValues = new Dictionary<string, string>
		{
			{"primary", "1"},
			{"secondary", "2"},
			{"tertiary", "3"},
			{"quaternary", "4"},
			{"identical", "I"}
		};

		/// <summary>
		/// This method will replace any unicode escapes in rules with their actual unicode characters
		/// and return the resulting string.
		/// Method created since IcuRulesCoallator does not appear to interpret unicode escapes.
		/// </summary>
		/// <param name="rules"></param>
		public static string ReplaceUnicodeEscapesForIcu(string rules)
		{
			if (!string.IsNullOrEmpty(rules))
			{
				//replace all unicode escapes in the rules string with the unicode character they represent.
				rules = UnicodeEscape8Digit.Replace(rules, match => ((char) int.Parse(match.Groups[1].Value,
																					  NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));
				rules = UnicodeEscape4Digit.Replace(rules, match => ((char) int.Parse(match.Groups[1].Value,
																					  NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));
			}
			return rules;
		}

		public static string GetIcuRulesFromCollationNode(XElement collationElem)
		{
			if (collationElem == null)
				throw new ArgumentNullException("collationElem");

			var icuRules = new StringBuilder();
			XElement settingsElem = collationElem.Element("settings");
			if (settingsElem != null)
				GetIcuSettingsFromSettingsNode(settingsElem, icuRules);
			XElement suppressElem = collationElem.Element("suppress_contractions");
			if (suppressElem != null)
				GetIcuOptionFromNode(suppressElem, icuRules);
			XElement optimizeElem = collationElem.Element("optimize");
			if (optimizeElem != null)
				GetIcuOptionFromNode(optimizeElem, icuRules);
			XElement rulesElem = collationElem.Element("cr");
			if (rulesElem != null)
				icuRules.Append((string) rulesElem);

			return TrimUnescapedWhitespace(icuRules.ToString());
		}

		/// <summary>
		/// Trim all whitespace from the beginning, and all unescaped whitespace from the end.
		/// </summary>
		/// <param name="icuRules"></param>
		/// <returns></returns>
		private static string TrimUnescapedWhitespace(string icuRules)
		{
			int lastEscapeIndex = icuRules.LastIndexOf('\\');
			if (lastEscapeIndex + 2 == icuRules.Length)
				return icuRules.TrimStart();
			return icuRules.Trim();
		}

		/// <summary>
		/// This method will escape necessary characters while avoiding escaping characters that are already escaped
		/// and leave unicode escape sequences alone.
		/// </summary>
		/// <param name="unescapedData"></param>
		/// <returns></returns>
		private static string EscapeForIcu(string unescapedData)
		{
			const int longEscapeLen = 10; //length of a \UFFFFFFFF escape
			const int shortEscLen = 6; //length of a \uFFFF escape
			string result = string.Empty;
			bool escapeNeedsClosing = false;
			for (int i = 0; i < unescapedData.Length; i++)
			{
				//if we are looking at an backslash check if the following character needs escaping, if it does
				//we do not need to escape it again
				if ((unescapedData[i] == '\\' || unescapedData[i] == '\'') && i + 1 < unescapedData.Length
					&& NeedsEscaping(Char.ConvertToUtf32(unescapedData, i + 1), "" + unescapedData[i + 1]))
				{
					result += unescapedData[i++]; //add the backslash and advance
					result += unescapedData[i]; //add the already escaped character
				} //handle long unicode escapes
				else if (i + longEscapeLen <= unescapedData.Length &&
						 UnicodeEscape8Digit.IsMatch(unescapedData.Substring(i, longEscapeLen)))
				{
					result += unescapedData.Substring(i, longEscapeLen);
					i += longEscapeLen - 1;
				} //handle short unicode escapes
				else if (i + shortEscLen <= unescapedData.Length &&
						 UnicodeEscape4Digit.IsMatch(unescapedData.Substring(i, shortEscLen)))
				{
					result += unescapedData.Substring(i, shortEscLen);
					i += shortEscLen - 1;
				}
				else
				{
					//handle everything else
					result += EscapeForIcu(Char.ConvertToUtf32(unescapedData, i), ref escapeNeedsClosing);
					if (Char.IsSurrogate(unescapedData, i))
					{
						i++;
					}
				}
			}
			return escapeNeedsClosing ? result + "'" : result;
		}

		private static string EscapeForIcu(int code, ref bool alreadyEscaping)
		{
			string result = String.Empty;
			string ch = Char.ConvertFromUtf32(code);
			// ICU only requires escaping all whitespace and any ASCII character that is not a letter or digit
			// Honestly, there shouldn't be any whitespace that is a surrogate, but we're checking
			// to maintain the highest compatibility with future Unicode code points.
			if (NeedsEscaping(code, ch))
			{
				if (!alreadyEscaping)
				{
					//Escape a single quote ' with single quote '', but don't start a sequence.
					if (ch != "'")
					{
						alreadyEscaping = true;
					}
					//begin the escape sequence.
					result += "'";
				}
				result += ch;
			}
			else
			{
				if (alreadyEscaping)
				{
					alreadyEscaping = false;
					result = "'";
				}
				result += ch;
			}
			return result;
		}

		private static bool NeedsEscaping(int code, string ch)
		{
			return (code < 0x7F && !Char.IsLetterOrDigit(ch, 0)) || Char.IsWhiteSpace(ch, 0);
		}

		private static void GetIcuSettingsFromSettingsNode(XElement settingsElem, StringBuilder icuRules)
		{
			foreach (XAttribute att in settingsElem.Attributes())
			{
				switch (att.Name.ToString())
				{
					case "alternate":
					case "normalization":
					case "caseLevel":
					case "caseFirst":
					case "numeric":
					case "reorder":
					case "maxVariable":
						icuRules.Append(string.Format(NewLine + "[{0} {1}]", att.Name, att.Value));
						break;
					case "strength":
						string strength;
						if (!StrengthValues.TryGetValue(att.Value, out strength))
						{
							throw new ApplicationException("Invalid collation strength setting in LDML");
						}
						icuRules.Append(string.Format(NewLine + "[strength {0}]", strength));
						break;
					case "backwards":
						if (att.Value != "off" && att.Value != "on")
						{
							throw new ApplicationException("Invalid backwards setting in LDML collation.");
						}
						icuRules.Append(string.Format(NewLine + "[backwards {0}]", att.Value == "off" ? "1" : "2"));
						break;
					case "hiraganaQuaternary":
						icuRules.Append(string.Format(NewLine + "[hiraganaQ {0}]", att.Value));
						break;
					case "variableTop":
						icuRules.Append(string.Format(NewLine + "& {0} < [variable top]", EscapeForIcu(UnescapeVariableTop(att.Value))));
						break;
				}
			}
		}

		private static string UnescapeVariableTop(string variableTop)
		{
			string result = string.Empty;
			foreach (string hexCode in variableTop.Split('u'))
			{
				if (String.IsNullOrEmpty(hexCode))
				{
					continue;
				}
				result += Char.ConvertFromUtf32(int.Parse(hexCode, NumberStyles.AllowHexSpecifier));
			}
			return result;
		}

		private static void GetIcuOptionFromNode(XElement collationElem, StringBuilder icuRules)
		{
			switch (collationElem.Name.ToString())
			{
				case "suppress_contractions":
				case "optimize":
					icuRules.Append(string.Format(NewLine + "[{0} {1}]", collationElem.Name.ToString().Replace('_', ' '), collationElem.Value));
					break;
				default:
					throw new ApplicationException(String.Format("Invalid LDML collation option element: {0}", collationElem.Name));
			}
		}
	}
}
