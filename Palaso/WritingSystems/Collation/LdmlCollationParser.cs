using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Collation;

namespace Palaso.WritingSystems.Collation
{
	class LdmlCollationParser
	{
		public LdmlCollationParser() { }

		public string GetIcuRulesFromCollationNode(XmlNode collationNode, XmlNamespaceManager nameSpaceManager)
		{
			if (collationNode == null)
			{
				throw new ArgumentNullException("collationNode");
			}

			XmlNode settingsNode = collationNode.SelectSingleNode("settings", nameSpaceManager);
			XmlNode rulesNode = collationNode.SelectSingleNode("rules", nameSpaceManager);
			string icuRules = string.Empty;
			_variableTop = null;
			int variableTopPositionIfNotUsed = 0;

			if (settingsNode != null)
			{
				icuRules += GetIcuSettingsFromSettingsNode(settingsNode);
				variableTopPositionIfNotUsed = icuRules.Length;
			}
			if (rulesNode != null)
			{
				icuRules += GetIcuRulesFromRulesNode(rulesNode);
			}
			if (_variableTop != null)
			{
				string variableTopRule = String.Format("\n&{0} < [variable top]", EscapeForIcu(_variableTop));
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

		private string _variableTop;

		private string EscapeForIcu(string unescapedData)
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

		private string EscapeForIcu(int code)
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

		private string GetIcuSettingsFromSettingsNode(XmlNode settingsNode)
		{
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
						_variableTop = attr.Value;
						break;
				}
			}
			return icuSettings;
		}

		private string GetIcuRulesFromRulesNode(XmlNode rulesNode)
		{
			string rules = string.Empty;
			foreach (XmlNode node in rulesNode.ChildNodes)
			{
				if (node.NodeType != XmlNodeType.Element)
				{
					throw new ApplicationException("Invalid XML node type in LDML collation rules.");
				}
				switch (node.Name)
				{
					case "reset":
						rules += String.Format("\n&{0}", GetIcuData(node));
						break;
					case "p":
						rules += String.Format(" < {0}", GetIcuData(node));
						break;
					case "s":
						rules += String.Format(" << {0}", GetIcuData(node));
						break;
					case "t":
						rules += String.Format(" <<< {0}", GetIcuData(node));
						break;
					case "i":
						rules += String.Format(" = {0}", GetIcuData(node));
						break;
					case "pc":
						rules += BuildRuleFromConcatenatedData("<", node.InnerText);
						break;
					case "sc":
						rules += BuildRuleFromConcatenatedData("<<", node.InnerText);
						break;
					case "tc":
						rules += BuildRuleFromConcatenatedData("<<<", node.InnerText);
						break;
					case "ic":
						rules += BuildRuleFromConcatenatedData("=", node.InnerText);
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

		private string GetIcuData(XmlNode node)
		{
			if (node.ChildNodes.Count > 1)
			{
				throw new ApplicationException(String.Format("Invalid data in LDML collation rule: {0}", node.InnerXml));
			}
			if (node.HasChildNodes)
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

		private string GetIndirectPosition(XmlNode indirectNode)
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

		private string BuildRuleFromConcatenatedData(string op, string data)
		{
			string rule = string.Empty;
			for (int i=0; i < data.Length; i++)
			{
				rule += String.Format(" {0} {1}", op, EscapeForIcu(Char.ConvertToUtf32(data, i)));
				if (Char.IsSurrogate(data, i))
				{
					i++;
				}
			}
			return rule;
		}

		private string GetRuleFromExtendedNode(XmlNode extendedNode)
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
