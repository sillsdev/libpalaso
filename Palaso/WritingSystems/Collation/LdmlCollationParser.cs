using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Palaso.WritingSystems.Collation
{
	public class LdmlCollationParser
	{
		private const string NewLine = "\r\n";

		public static string GetIcuRulesFromCollationNode(string collationXml)
		{
			if (collationXml == null)
			{
				throw new ArgumentNullException("collationXml");
			}

			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.CloseInput = true;
			readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			string icuRules = string.Empty;
			string variableTop = null;
			int variableTopPositionIfNotUsed = 0;
			using (XmlReader collationReader = XmlReader.Create(new StringReader(collationXml), readerSettings))
			{
				if (XmlHelpers.FindElement(collationReader, "settings", LdmlNodeComparer.CompareElementNames))
				{
					icuRules += GetIcuSettingsFromSettingsNode(collationReader, out variableTop);
					variableTopPositionIfNotUsed = icuRules.Length;
				}
				if (XmlHelpers.FindElement(collationReader, "suppress_contractions", LdmlNodeComparer.CompareElementNames))
				{
					icuRules += GetIcuOptionFromNode(collationReader);
				}
				if (XmlHelpers.FindElement(collationReader, "optimize", LdmlNodeComparer.CompareElementNames))
				{
					icuRules += GetIcuOptionFromNode(collationReader);
				}
				if (XmlHelpers.FindElement(collationReader, "rules", LdmlNodeComparer.CompareElementNames))
				{
					icuRules += GetIcuRulesFromRulesNode(collationReader, ref variableTop);
				}
			}

			if (variableTop != null)
			{
				string variableTopRule = String.Format(NewLine + "&{0} < [variable top]", EscapeForIcu(variableTop));
				if (variableTopPositionIfNotUsed == icuRules.Length)
				{
					icuRules += variableTopRule;
				}
				else
				{
					icuRules = String.Format("{0}{1}{2}", icuRules.Substring(0, variableTopPositionIfNotUsed),
						variableTopRule, icuRules.Substring(variableTopPositionIfNotUsed));
				}
			}
			return icuRules.Trim();
		}

		public static bool TryGetSimpleRulesFromCollationNode(string collationXml, out string rules)
		{
			if (collationXml == null)
			{
				throw new ArgumentNullException("collationXml");
			}

			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.CloseInput = true;
			readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			rules = null;
			using (XmlReader collationReader = XmlReader.Create(new StringReader(collationXml), readerSettings))
			{
				// simple rules can't deal with any non-default settings
				if (XmlHelpers.FindElement(collationReader, "settings", LdmlNodeComparer.CompareElementNames))
				{
					return false;
				}
				if (!XmlHelpers.FindElement(collationReader, "rules", LdmlNodeComparer.CompareElementNames))
				{
					rules = string.Empty;
					return true;
				}
				rules = GetSimpleRulesFromRulesNode(collationReader);
			}
			return rules != null;
		}

		private static string GetSimpleRulesFromRulesNode(XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				return string.Empty;
			}
			bool first = true;
			bool inGroup = false;
			string simpleRules = string.Empty;
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement && !reader.EOF)
			{
				if (reader.NodeType != XmlNodeType.Element)
				{
					reader.Read();
					continue;
				}
				// First child node MUST BE <reset before="primary"><first_non_ignorable /></reset>
				if (first && (reader.Name != "reset" || GetBeforeOption(reader) != "[before 1] " || GetIcuData(reader) != "[first regular]"))
				{
					return null;
				}
				if (first)
				{
					first = false;
					reader.Read();
					continue;
				}
				switch (reader.Name)
				{
					case "p":
						simpleRules += EndSimpleGroupIfNeeded(ref inGroup) + NewLine + GetTextData(reader);
						break;
					case "s":
						simpleRules += EndSimpleGroupIfNeeded(ref inGroup) + " " + GetTextData(reader);
						break;
					case "t":
						BeginSimpleGroupIfNeeded(ref inGroup, ref simpleRules);
						simpleRules += " " + GetTextData(reader);
						break;
					case "pc":
						simpleRules += EndSimpleGroupIfNeeded(ref inGroup) +
									   BuildSimpleRulesFromConcatenatedData(NewLine, GetTextData(reader));
						break;
					case "sc":
						simpleRules += EndSimpleGroupIfNeeded(ref inGroup) +
									   BuildSimpleRulesFromConcatenatedData(" ", GetTextData(reader));
						break;
					case "tc":
						BeginSimpleGroupIfNeeded(ref inGroup, ref simpleRules);
						simpleRules += BuildSimpleRulesFromConcatenatedData(" ", GetTextData(reader));
						break;
					default:    // element name not allowed for simple rules conversion
						return null;
				}
				reader.ReadEndElement();
			}
			simpleRules += EndSimpleGroupIfNeeded(ref inGroup);
			return simpleRules.Trim();
		}

		private static string EndSimpleGroupIfNeeded(ref bool inGroup)
		{
			if (!inGroup)
			{
				return string.Empty;
			}
			inGroup = false;
			return ")";
		}

		private static void BeginSimpleGroupIfNeeded(ref bool inGroup, ref string rules)
		{
			if (inGroup)
			{
				return;
			}
			inGroup = true;
			rules = rules.Insert(rules.Length - 1, "(");
			return;
		}

		private static string EscapeForIcu(string unescapedData)
		{
			string result = string.Empty;
			for (int i = 0; i < unescapedData.Length; i++)
			{
				result += EscapeForIcu(Char.ConvertToUtf32(unescapedData, i));
				if (Char.IsSurrogate(unescapedData, i))
				{
					i++;
				}
			}
			return result;
		}

		private static string BuildSimpleRulesFromConcatenatedData(string op, string data)
		{
			string rule = string.Empty;
			bool surrogate = false;
			for (int i = 0; i < data.Length; i++)
			{
				if (surrogate)
				{
					rule += data[i];
					surrogate = false;
					continue;
				}
				rule += op + data[i];
				if (Char.IsSurrogate(data, i))
				{
					surrogate = true;
				}
			}
			return rule;
		}

		private static string EscapeForIcu(int code)
		{
			string result;
			string ch = Char.ConvertFromUtf32(code);
			// ICU only requires escaping all whitespace and any ASCII character that is not a letter or digit
			// Honestly, there shouldn't be any whitespace that is a surrogate, but we're checking
			// to maintain the highest compatibility with future Unicode code points.
			if ((code < 0x7F && !Char.IsLetterOrDigit(ch, 0)) || Char.IsWhiteSpace(ch, 0))
			{
				result = "\\" + ch;
			}
			else
			{
				result = ch;
			}
			return result;
		}

		private static string GetIcuSettingsFromSettingsNode(XmlReader reader, out string variableTop)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			Debug.Assert(reader.Name == "settings");
			variableTop = null;
			Dictionary<string, string> strengthValues = new Dictionary<string, string>();
			strengthValues["primary"] = "1";
			strengthValues["secondary"] = "2";
			strengthValues["tertiary"] = "3";
			strengthValues["quaternary"] = "4";
			strengthValues["identical"] = "I";
			string icuSettings = string.Empty;
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "alternate":
					case "normalization":
					case "caseLevel":
					case "caseFirst":
					case "numeric":
						icuSettings += String.Format(NewLine + "[{0} {1}]", reader.Name, reader.Value);
						break;
					case "strength":
						if (!strengthValues.ContainsKey(reader.Value))
						{
							throw new ApplicationException("Invalid collation strength setting in LDML");
						}
						icuSettings += String.Format(NewLine + "[strength {0}]", strengthValues[reader.Value]);
						break;
					case "backwards":
						if (reader.Value != "off" && reader.Value != "on")
						{
							throw new ApplicationException("Invalid backwards setting in LDML collation.");
						}
						icuSettings += String.Format(NewLine + "[backwards {0}]", reader.Value == "off" ? "1" : "2");
						break;
					case "hiraganaQuaternary":
						icuSettings += String.Format(NewLine + "[hiraganaQ {0}]", reader.Value);
						break;
					case "variableTop":
						variableTop = EscapeForIcu(UnescapeVariableTop(reader.Value));
						break;
				}
			}
			return icuSettings;
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

		private static string GetIcuOptionFromNode(XmlReader reader)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			string result;
			switch (reader.Name)
			{
				case "suppress_contractions":
				case "optimize":
					result = String.Format(NewLine + "[{0} {1}]", reader.Name.Replace('_', ' '), reader.ReadElementString());
					break;
				default:
					throw new ApplicationException(String.Format("Invalid LDML collation option element: {0}", reader.Name));
			}
			return result;
		}

		private static string GetIcuRulesFromRulesNode(XmlReader reader, ref string variableTop)
		{
			string rules = string.Empty;
			using (XmlReader rulesReader = reader.ReadSubtree())
			{
				// skip initial "rules" element
				rulesReader.Read();
				while (rulesReader.Read())
				{
					string icuData;
					if (rulesReader.NodeType != XmlNodeType.Element)
					{
						continue;
					}
					switch (rulesReader.Name)
					{
						case "reset":
							string beforeOption = GetBeforeOption(rulesReader);
							icuData = GetIcuData(rulesReader);
							// I added a space after the ampersand to increase readability with situations where the first
							// character following a reset may be a combining character or some other character that would be
							// rendered around the ampersand
							rules += String.Format(NewLine + "& {2}{0}{1}", icuData, GetVariableTopString(icuData, ref variableTop),
								beforeOption);
							break;
						case "p":
							icuData = GetIcuData(rulesReader);
							rules += String.Format(" < {0}{1}", icuData, GetVariableTopString(icuData, ref variableTop));
							break;
						case "s":
							icuData = GetIcuData(rulesReader);
							rules += String.Format(" << {0}{1}", icuData, GetVariableTopString(icuData, ref variableTop));
							break;
						case "t":
							icuData = GetIcuData(rulesReader);
							rules += String.Format(" <<< {0}{1}", icuData, GetVariableTopString(icuData, ref variableTop));
							break;
						case "i":
							icuData = GetIcuData(rulesReader);
							rules += String.Format(" = {0}{1}", icuData, GetVariableTopString(icuData, ref variableTop));
							break;
						case "pc":
							rules += BuildRuleFromConcatenatedData("<", rulesReader, ref variableTop);
							break;
						case "sc":
							rules += BuildRuleFromConcatenatedData("<<", rulesReader, ref variableTop);
							break;
						case "tc":
							rules += BuildRuleFromConcatenatedData("<<<", rulesReader, ref variableTop);
							break;
						case "ic":
							rules += BuildRuleFromConcatenatedData("=", rulesReader, ref variableTop);
							break;
						case "x":
							rules += GetRuleFromExtendedNode(rulesReader);
							break;
						default:
							throw new ApplicationException(String.Format("Invalid LDML collation rule element: {0}", rulesReader.Name));
					}
				}
			}
			return rules;
		}

		private static string GetBeforeOption(XmlReader reader)
		{
			switch (reader.GetAttribute("before"))
			{
				case "primary":
					return "[before 1] ";
				case "secondary":
					return "[before 2] ";
				case "tertiary":
					return "[before 3] ";
				case "":
				case null:
					return string.Empty;
				default:
					throw new ApplicationException("Invalid before specifier on reset collation element.");
			}
		}

		private static string GetIcuData(XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				throw new ApplicationException(String.Format("Empty LDML collation rule: {0}", reader.Name));
			}
			string data = reader.ReadString();
			if (reader.NodeType == XmlNodeType.Element && reader.Name != "cp")
			{
				return GetIndirectPosition(reader);
			}
			data += GetTextData(reader);
			return EscapeForIcu(data);
		}

		private static string GetTextData(XmlReader reader)
		{
			string data = reader.ReadString();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Comment:
					case XmlNodeType.ProcessingInstruction:
						reader.Read();
						break;
					case XmlNodeType.CDATA:
					case XmlNodeType.EntityReference:
					case XmlNodeType.Text:
					case XmlNodeType.Whitespace:
					case XmlNodeType.SignificantWhitespace:
						data += reader.ReadString();
						break;
					case XmlNodeType.Element:
						data += GetCPData(reader);
						break;
					default:
						throw new ApplicationException("Unexpected XML node type inside LDML collation data element.");
				}
			}
			return data;
		}

		private static string GetCPData(XmlReader reader)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			if (reader.Name != "cp")
			{
				throw new ApplicationException(string.Format("Unexpected element '{0}' in text data node", reader.Name));
			}
			string hex = reader.GetAttribute("hex");
			string result = string.Empty;
			if (!string.IsNullOrEmpty(hex))
			{
				int code;
				if (!int.TryParse(hex, NumberStyles.AllowHexSpecifier, null, out code))
				{
					throw new ApplicationException("Invalid non-hexadecimal character code in LDML 'cp' element.");
				}
				try
				{
					result = Char.ConvertFromUtf32(code);
				}
				catch (ArgumentOutOfRangeException e)
				{
					throw new ApplicationException("Invalid Unicode code point in LDML 'cp' element.", e);
				}
			}
			reader.Skip();
			return result;
		}

		private static string GetIndirectPosition(XmlReader reader)
		{
			string result;
			switch (reader.Name)
			{
				case "first_non_ignorable":
					result = "[first regular]";
					break;
				case "last_non_ignorable":
					result = "[last regular]";
					break;
				default:
					result = "[" + reader.Name.Replace('_', ' ') + "]";
					break;
			}
			reader.Skip();
			return result;
		}

		private static string BuildRuleFromConcatenatedData(string op, XmlReader reader, ref string variableTop)
		{
			string data = GetTextData(reader);
			string rule = string.Empty;
			for (int i=0; i < data.Length; i++)
			{
				string icuData = EscapeForIcu(Char.ConvertToUtf32(data, i));
				rule += String.Format(" {0} {1}{2}", op, icuData, GetVariableTopString(icuData, ref variableTop));
				if (Char.IsSurrogate(data, i))
				{
					i++;
				}
			}
			return rule;
		}

		private static string GetVariableTopString(string icuData, ref string variableTop)
		{
			if (variableTop == null || variableTop != icuData)
			{
				return string.Empty;
			}
			variableTop = null;
			return " < [variable top]";
		}

		private static string GetRuleFromExtendedNode(XmlReader reader)
		{
			string rule = string.Empty;
			if (reader.IsEmptyElement)
			{
				return rule;
			}
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement && !reader.EOF)
			{
				if (reader.NodeType != XmlNodeType.Element)
				{
					reader.Read();
					continue;
				}
				switch (reader.Name)
				{
					case "context":
						rule += String.Format("{0} | ", GetIcuData(reader));
						break;
					case "extend":
						rule += String.Format(" / {0}", GetIcuData(reader));
						break;
					case "p":
						rule = String.Format(" < {0}{1}", rule, GetIcuData(reader));
						break;
					case "s":
						rule = String.Format(" << {0}{1}", rule, GetIcuData(reader));
						break;
					case "t":
						rule = String.Format(" <<< {0}{1}", rule, GetIcuData(reader));
						break;
					case "i":
						rule = String.Format(" = {0}{1}", rule, GetIcuData(reader));
						break;
					default:
						throw new ApplicationException(String.Format("Invalid node in extended LDML collation rule: {0}", reader.Name));
				}
				reader.Read();
			}
			return rule;
		}
	}
}
