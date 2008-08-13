using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Palaso.WritingSystems.Collation
{
	public class LdmlCollationParser
	{
		public static string GetIcuRulesFromCollationNode(XmlNode collationNode, XmlNamespaceManager nameSpaceManager)
		{
			if (collationNode == null)
			{
				throw new ArgumentNullException("collationNode");
			}

			XmlNode settingsNode = collationNode.SelectSingleNode("settings", nameSpaceManager);
			XmlNode rulesNode = collationNode.SelectSingleNode("rules", nameSpaceManager);
			string icuRules = string.Empty;
			string variableTop = null;
			int variableTopPositionIfNotUsed = 0;

			if (settingsNode != null)
			{
				icuRules += GetIcuSettingsFromSettingsNode(settingsNode, out variableTop);
				variableTopPositionIfNotUsed = icuRules.Length;
			}
			if (rulesNode != null)
			{
				icuRules += GetIcuRulesFromRulesNode(rulesNode, ref variableTop);
			}
			if (variableTop != null)
			{
				string variableTopRule = String.Format("\n&{0} < [variable top]", EscapeForIcu(variableTop));
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

		public static bool TryGetSimpleRulesFromCollationNode(XmlNode collationNode, XmlNamespaceManager namespaceManager, out string rules)
		{
			rules = null;
			// simple rules can't deal with any non-default settings
			if (collationNode.SelectSingleNode("settings", namespaceManager) != null)
			{
				return false;
			}
			XmlNode rulesNode = collationNode.SelectSingleNode("rules", namespaceManager);
			if (rulesNode == null)
			{
				rules = string.Empty;
				return true;
			}
			bool first = true;
			bool inGroup = false;
			string simpleRules = string.Empty;
			foreach (XmlNode node in rulesNode.ChildNodes)
			{
				// First child node MUST BE <reset before="primary"><first_non_ignorable /></reset>
				if (first && node.Name != "reset" && GetIcuData(node) != "[first regular]" && GetBeforeOption(node) != "[before 1]")
				{
					return false;
				}
				if (first)
				{
					first = false;
					continue;
				}
				if (HasIndirectPosition(node))
				{
					return false;
				}
				switch (node.Name)
				{
					case "p":
						simpleRules += EndSimpleGroupIfNeeded(ref inGroup) + "\n" + GetIcuData(node);
						break;
					case "s":
						simpleRules += EndSimpleGroupIfNeeded(ref inGroup) + " " + GetIcuData(node);
						break;
					case "t":
						BeginSimpleGroupIfNeeded(ref inGroup, ref simpleRules);
						simpleRules += " " + GetIcuData(node);
						break;
					case "pc":
						simpleRules += EndSimpleGroupIfNeeded(ref inGroup) +
									   BuildSimpleRulesFromConcatenatedData("\n", node.InnerText);
						break;
					case "sc":
						simpleRules += EndSimpleGroupIfNeeded(ref inGroup) +
									   BuildSimpleRulesFromConcatenatedData(" ", node.InnerText);
						break;
					case "tc":
						BeginSimpleGroupIfNeeded(ref inGroup, ref simpleRules);
						simpleRules += BuildSimpleRulesFromConcatenatedData(" ", node.InnerText);
						break;
					default:    // node type not allowed for simple rules conversion
						return false;
				}
			}
			simpleRules += EndSimpleGroupIfNeeded(ref inGroup);
			rules = simpleRules.Trim();
			return true;
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
			for (int i = 0; i < data.Length; i++)
			{
				string icuData = EscapeForIcu(Char.ConvertToUtf32(data, i));
				rule += op + icuData;
				if (Char.IsSurrogate(data, i))
				{
					i++;
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
				if (Char.IsSurrogate(ch, 0))
				{
					result = "\\U" + code.ToString("X8");
				}
				else
				{
					result = "\\u" + code.ToString("X4");
				}
			}
			else
			{
				result = ch;
			}
			return result;
		}

		private static string GetIcuSettingsFromSettingsNode(XmlNode settingsNode, out string variableTop)
		{
			variableTop = null;
			Dictionary<string, string> strengthValues = new Dictionary<string, string>();
			strengthValues["primary"] = "1";
			strengthValues["secondary"] = "2";
			strengthValues["tertiary"] = "3";
			strengthValues["quaternary"] = "4";
			strengthValues["identical"] = "I";
			string icuSettings = string.Empty;
			foreach (XmlAttribute attr in settingsNode.Attributes)
			{
				switch (attr.Name)
				{
					case "alternate":
					case "normalization":
					case "caseLevel":
					case "caseFirst":
					case "numeric":
						icuSettings += String.Format("\n[{0} {1}]", attr.Name, attr.Value);
						break;
					case "strength":
						if (!strengthValues.ContainsKey(attr.Value))
						{
							throw new ApplicationException("Invalid collation strength setting in LDML");
						}
						icuSettings += String.Format("\n[strength {0}]", strengthValues[attr.Value]);
						break;
					case "backwards":
						if (attr.Value != "off" && attr.Value != "on")
						{
							throw new ApplicationException("Invalid backwards setting in LDML collation.");
						}
						icuSettings += String.Format("\n[backwards {0}]", attr.Value == "off" ? "1" : "2");
						break;
					case "hiraganaQuaternary":
						icuSettings += String.Format("\n[hiraganaQ {0}]", attr.Value);
						break;
					case "variableTop":
						variableTop = EscapeForIcu(UnescapeVariableTop(attr.Value));
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

		private static string GetIcuRulesFromRulesNode(XmlNode rulesNode, ref string variableTop)
		{
			string rules = string.Empty;
			foreach (XmlNode node in rulesNode.ChildNodes)
			{
				string icuData;
				if (node.NodeType != XmlNodeType.Element)
				{
					throw new ApplicationException("Invalid XML node type in LDML collation rules.");
				}
				switch (node.Name)
				{
					case "reset":
						icuData = GetIcuData(node);
						rules += String.Format("\n&{2}{0}{1}", icuData, GetVariableTopString(icuData, ref variableTop), GetBeforeOption(node));
						break;
					case "p":
						icuData = GetIcuData(node);
						rules += String.Format(" < {0}{1}", icuData, GetVariableTopString(icuData, ref variableTop));
						break;
					case "s":
						icuData = GetIcuData(node);
						rules += String.Format(" << {0}{1}", icuData, GetVariableTopString(icuData, ref variableTop));
						break;
					case "t":
						icuData = GetIcuData(node);
						rules += String.Format(" <<< {0}{1}", icuData, GetVariableTopString(icuData, ref variableTop));
						break;
					case "i":
						icuData = GetIcuData(node);
						rules += String.Format(" = {0}{1}", icuData, GetVariableTopString(icuData, ref variableTop));
						break;
					case "pc":
						rules += BuildRuleFromConcatenatedData("<", node.InnerText, ref variableTop);
						break;
					case "sc":
						rules += BuildRuleFromConcatenatedData("<<", node.InnerText, ref variableTop);
						break;
					case "tc":
						rules += BuildRuleFromConcatenatedData("<<<", node.InnerText, ref variableTop);
						break;
					case "ic":
						rules += BuildRuleFromConcatenatedData("=", node.InnerText, ref variableTop);
						break;
					case "x":
						rules += GetRuleFromExtendedNode(node);
						break;
					default:
						throw new ApplicationException(String.Format("Invalid LDML collation rule element: {0}", node.Name));
				}
			}
			return rules;
		}

		private static string GetBeforeOption(XmlNode node)
		{
			switch (XmlHelpers.GetAttributeValue(node, "before"))
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

		private static bool HasIndirectPosition(XmlNode node)
		{
			return node.HasChildNodes && node.FirstChild.NodeType == XmlNodeType.Element;
		}

		private static string GetIcuData(XmlNode node)
		{
			if (node.ChildNodes.Count > 1)
			{
				throw new ApplicationException(String.Format("Invalid data in LDML collation rule: {0}", node.InnerXml));
			}
			if (HasIndirectPosition(node))
			{
				return GetIndirectPosition(node.FirstChild);
			}
			string data = string.Empty;
			for (int i = 0; i < node.InnerText.Length; i++)
			{
				data += EscapeForIcu(Char.ConvertToUtf32(node.InnerText, i));
				if (Char.IsSurrogate(node.InnerText, i))
				{
					i++;
				}
			}
			return data;
		}

		private static string GetIndirectPosition(XmlNode indirectNode)
		{
			switch (indirectNode.Name)
			{
				case "first_non_ignorable":
					return "[first regular]";
				case "last_non_ignorable":
					return "[last regular]";
				default:
					return "[" + indirectNode.Name.Replace('_', ' ') + "]";
			}
		}

		private static string BuildRuleFromConcatenatedData(string op, string data, ref string variableTop)
		{
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

		private static string GetRuleFromExtendedNode(XmlNode extendedNode)
		{
			string rule = string.Empty;
			foreach (XmlNode node in extendedNode.ChildNodes)
			{
				switch (node.Name)
				{
					case "context":
						rule += String.Format("{0} | ", GetIcuData(node));
						break;
					case "extend":
						rule += String.Format(" / {0}", GetIcuData(node));
						break;
					case "p":
						rule = String.Format(" < {0}{1}", rule, GetIcuData(node));
						break;
					case "s":
						rule = String.Format(" << {0}{1}", rule, GetIcuData(node));
						break;
					case "t":
						rule = String.Format(" <<< {0}{1}", rule, GetIcuData(node));
						break;
					case "i":
						rule = String.Format(" = {0}{1}", rule, GetIcuData(node));
						break;
					default:
						throw new ApplicationException(String.Format("Invalid node in extended LDML collation rule: {0}", node.Name));
				}
			}
			return rule;
		}
	}
}
