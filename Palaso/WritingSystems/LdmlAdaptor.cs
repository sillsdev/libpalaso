using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Collation;
using Spart.Actions;
using Spart.Parsers;
using Spart.Parsers.NonTerminal;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace Palaso.WritingSystems
{
	public class LdmlAdaptor
	{
		private XmlNamespaceManager _nameSpaceManager;

		public LdmlAdaptor()
		{
			_nameSpaceManager = MakeNameSpaceManager();
		}

		public void Read(string filePath, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.Load(filePath);
			Read(doc, ws);
		}

		public void Read(XmlReader xmlReader, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.Load(xmlReader);
			Read(doc, ws);
		}

		public void Read(XmlDocument doc, WritingSystemDefinition ws)
		{
			ws.ISO = GetIdentityValue(doc, "language", "type");
			ws.Variant = GetIdentityValue(doc, "variant", "type");
			ws.Region = GetIdentityValue(doc, "territory", "type");
			ws.Script = GetIdentityValue(doc, "script", "type");
			string dateTime = GetIdentityValue(doc, "generation", "date");
			ws.DateModified = DateTime.Parse(dateTime);
			XmlNode node = doc.SelectSingleNode("ldml/identity/version");
			ws.VersionNumber = XmlHelpers.GetOptionalAttributeValue(node, "number");
			ws.VersionDescription = node.InnerText;

			ws.Abbreviation = GetSpecialValue(doc, "abbreviation");
			ws.LanguageName = GetSpecialValue(doc, "languageName");
			ws.DefaultFontName = GetSpecialValue(doc, "defaultFontFamily");
			ws.Keyboard = GetSpecialValue(doc, "keyboard");
			string rtl = GetSpecialValue(doc, "rightToLeft");
			ws.RightToLeftScript = rtl == "true";
			ws.StoreID = "";
			ws.Modified = false;
		}

		public void Write(string filePath, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			doc.CreateXmlDeclaration("1.0", "", "no");
			XmlHelpers.GetOrCreateElement(doc, ".", "ldml", null, _nameSpaceManager);
			XmlHelpers.GetOrCreateElement(doc, "ldml", "identity", null, _nameSpaceManager);
			UpdateDOM(doc, ws);
			doc.Save(filePath);
		}

		public void Write(XmlWriter xmlWriter, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument(_nameSpaceManager.NameTable);
			XmlHelpers.GetOrCreateElement(doc, ".", "ldml", null, _nameSpaceManager);
			XmlHelpers.GetOrCreateElement(doc, "ldml", "identity", null, _nameSpaceManager);
			UpdateDOM(doc, ws);
			doc.Save(xmlWriter); //??? Not sure about this, does this need to be Write(To) or similar?
		}

		public void FillWithDefaults(string rfc4646, WritingSystemDefinition ws)
		{
			string id = rfc4646.ToLower();
			switch (id)
			{
				case "en-latn":
					ws.ISO = "en";
					ws.LanguageName = "English";
					ws.Abbreviation = "eng";
					ws.Script = "Latn";
					break;
				 default:
					ws.Script = "Latn";
					break;
			}
		}

		private string GetSpecialValue(XmlDocument doc, string field)
		{
			XmlNode node = doc.SelectSingleNode("ldml/special/palaso:"+field, _nameSpaceManager);
			return XmlHelpers.GetOptionalAttributeValue(node, "value", string.Empty);
		}

		private string GetIdentityValue(XmlDocument doc, string field, string attributeName)
		{
			XmlNode node = doc.SelectSingleNode("ldml/identity/" + field);
			return XmlHelpers.GetOptionalAttributeValue(node, attributeName, string.Empty);
		}

		public static XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNamespaceManager m = new XmlNamespaceManager(new NameTable());
			m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			return m;
		}

		private void UpdateDOM(XmlDocument dom, WritingSystemDefinition ws)
		{
			SetSubIdentityNode(dom, "language", "type", ws.ISO);
			SetSubIdentityNode(dom, "script", "type", ws.Script);
			SetSubIdentityNode(dom, "territory", "type", ws.Region);
			SetSubIdentityNode(dom, "variant", "type", ws.Variant);
			SetSubIdentityNode(dom, "generation", "date", String.Format("{0:s}", ws.DateModified));
			XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml/identity", "version", null, _nameSpaceManager);
			XmlHelpers.AddOrUpdateAttribute(node, "number", ws.VersionNumber);
			node.InnerText = ws.VersionDescription;

			SetTopLevelSpecialNode(dom, "languageName", ws.LanguageName);
			SetTopLevelSpecialNode(dom, "abbreviation", ws.Abbreviation);
			SetTopLevelSpecialNode(dom, "defaultFontFamily", ws.DefaultFontName);
			SetTopLevelSpecialNode(dom, "keyboard", ws.Keyboard);
			SetTopLevelSpecialNode(dom, "rightToLeft", ws.RightToLeftScript ? "true": "false");
		}

		private void SetSubIdentityNode(XmlDocument dom, string field, string attributeName, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml/identity", field, null, _nameSpaceManager);
				XmlHelpers.AddOrUpdateAttribute(node, attributeName, value);
			}
			else
			{
				XmlHelpers.RemoveElement(dom, "ldml/identity/" + field, _nameSpaceManager);
			}
		}

		private void SetTopLevelSpecialNode(XmlDocument dom, string field, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				XmlHelpers.GetOrCreateElement(dom, "ldml", "special", null, _nameSpaceManager);
				XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml/special", field, "palaso", _nameSpaceManager);
				Palaso.XmlHelpers.AddOrUpdateAttribute(node, "value", value);
			}
			else
			{
				XmlHelpers.RemoveElement(dom, "ldml/special/palaso:" + field, _nameSpaceManager);
			}
		}

		private void UpdateCollationElement(XmlDocument dom, WritingSystemDefinition ws)
		{
			Debug.Assert(dom != null);
			Debug.Assert(ws != null);
			if (string.IsNullOrEmpty(ws.SortUsing) || !Enum.IsDefined(typeof (WritingSystemDefinition.SortRulesType), ws.SortUsing))
			{
				return;
			}
			XmlNode parentNode = XmlHelpers.GetOrCreateElement(dom, "ldml", "collations", null, _nameSpaceManager);
			XmlNode node = parentNode.SelectSingleNode("collation[@type='']", _nameSpaceManager);
			if (node == null)
			{
				node = parentNode.OwnerDocument.CreateElement("collation");
				parentNode.AppendChild(node);
			}
			switch ((WritingSystemDefinition.SortRulesType)Enum.Parse(typeof(WritingSystemDefinition.SortRulesType), ws.SortUsing))
			{
				case WritingSystemDefinition.SortRulesType.OtherLanguage:
					UpdateCollationRulesFromOtherLanguage(node, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomSimple:
					UpdateCollationRulesFromCustomSimple(node, ws);
					break;
				case WritingSystemDefinition.SortRulesType.CustomICU:
					UpdateCollationRulesFromCustomICU(node, ws);
					break;
				default:
					string message = string.Format("Unhandled SortRulesType '{0}' while writing LDML definition file.", ws.SortUsing);
					throw new ApplicationException(message);
					break;
			}
		}

		private void UpdateCollationRulesFromOtherLanguage(XmlNode parentNode, WritingSystemDefinition ws)
		{
			Debug.Assert(parentNode != null);
			Debug.Assert(ws.SortUsing == "OtherLanguage");
			// Since the alias element gets all information from another source,
			// we should remove all other elements in this collation element.  We
			// leave "special" elements as they are custom data from some other app.
			List<XmlNode> nodesToRemove = new List<XmlNode>();
			foreach (XmlNode child in parentNode.ChildNodes)
			{
				if (child.NodeType != XmlNodeType.Element || child.Name == "special")
				{
					continue;
				}
				nodesToRemove.Add(child);
			}
			foreach (XmlNode node in nodesToRemove)
			{
				parentNode.RemoveChild(node);
			}

			XmlNode alias = XmlHelpers.GetOrCreateElement(parentNode, string.Empty, "alias", string.Empty, _nameSpaceManager);
			XmlHelpers.AddOrUpdateAttribute(alias, "source", ws.SortRules);
		}

		private void UpdateCollationRulesFromCustomSimple(XmlNode parentNode, WritingSystemDefinition ws)
		{
			Debug.Assert(parentNode != null);
			Debug.Assert(ws.SortUsing == "CustomSimple");
			string icu = SimpleRulesCollator.ConvertToIcuRules(ws.SortRules ?? string.Empty);
			UpdateCollationRulesFromICUString(parentNode, icu);
		}

		private void UpdateCollationRulesFromCustomICU(XmlNode parentNode, WritingSystemDefinition ws)
		{
			Debug.Assert(parentNode != null);
			Debug.Assert(ws.SortUsing == "CustomICU");
			UpdateCollationRulesFromICUString(parentNode, ws.SortRules);
		}

		private void UpdateCollationRulesFromICUString(XmlNode parentNode, string icu)
		{
			Debug.Assert(parentNode != null);
			icu = icu ?? string.Empty;
			// remove any alias that would override our rules
			XmlHelpers.RemoveElement(parentNode, "alias", _nameSpaceManager);
			XmlNode rulesNode = XmlHelpers.GetOrCreateElement(parentNode, string.Empty, "rules", string.Empty, _nameSpaceManager);
			XmlHelpers.RemoveElement(rulesNode, "alias", _nameSpaceManager);
		}

		private class ICURulesParser
		{
			private XmlDocument _dom;
			private Rule _primaryDifference;
			private Rule _secondaryDifference;
			private Rule _tertiaryDifference;
			private Rule _noDifference;
			private Rule _reset;
			private Rule _oneRule;
			private Rule _icuRules;

			public ICURulesParser(XmlDocument dom) : this(dom, false) {}

			public ICURulesParser(XmlDocument dom, bool useDebugger)
			{
				_dom = dom;

				// someWhiteSpace ::= WS+
				// optionalWhiteSpace ::= WS*
				Parser someWhiteSpace = Ops.OneOrMore(Prims.WhiteSpace);
				Parser optionalWhiteSpace = Ops.ZeroOrMore(Prims.WhiteSpace);

				// Valid escaping formats (from ICU source code u_unescape):
				// \uhhhh - exactly 4 hex digits to specify character
				// \Uhhhhhhhh - exactly 8 hex digits to specify character
				// \xhh - 1 or 2 hex digits to specify character
				// \x{hhhhhhhh} - 1 to 8 hex digits to specify character
				// \ooo - 1 to 3 octal digits to specify character
				// \cX - masked control character - take value of X and bitwise AND with 0x1F
				// \a - U+0007
				// \b - U+0008
				// \t - U+0009
				// \n - U+000A
				// \v - U+000B
				// \f - U+000C
				// \r - U+000D
				// Any other character following '\' means that literal character (e.g. '\<' ::= '<', '\\' ::= '\')
				Parser octalDigit = Prims.Range('0', '7');
				Parser hexDigitExpect = Ops.Expect("icu0001", "Invalid hexadecimal character in escape sequence.", Prims.HexDigit);
				Parser hex4Group = Ops.Sequence(hexDigitExpect, hexDigitExpect, hexDigitExpect, hexDigitExpect);
				Parser singleCharacterEscape = Ops.Sequence(Prims.AnyChar - Ops.Choice('u', 'U', 'x', octalDigit, 'c'));
				Parser hex4Escape = Ops.Sequence('u', hex4Group);
				Parser hex8Escape = Ops.Sequence('U', hex4Group, hex4Group);
				Parser hex2Escape = Ops.Sequence('x', hexDigitExpect, !Prims.HexDigit);
				Parser hexXEscape = Ops.Sequence('x', '{', hexDigitExpect, !hexDigitExpect, !hexDigitExpect, !hexDigitExpect,
														  !hexDigitExpect, !hexDigitExpect, !hexDigitExpect, !hexDigitExpect, '}');
				Parser octalEscape = Ops.Sequence(octalDigit, !octalDigit, !octalDigit);
				Parser controlEscape = Ops.Sequence('c', Prims.AnyChar);
				Parser escapeSequence = Ops.Sequence('\\', Ops.Expect("icu0002", "Invalid escape sequence.",
					singleCharacterEscape | hex4Escape | hex8Escape | hex2Escape | hexXEscape | octalEscape | controlEscape));

				// singleQuoteLiteral ::= "''"
				// quotedStringCharacter ::= AllChars - "'"
				// quotedString ::= "'" (singleQuoteLiteral | quotedStringCharacter)+ "'"
				Parser singleQuoteLiteral = Ops.Sequence('\'', '\'');
				Parser quotedStringCharacter = Prims.AnyChar - '\'';
				Parser quotedString = Ops.Sequence('\'', Ops.OneOrMore(singleQuoteLiteral | quotedStringCharacter),
					Ops.Expect("icu0003", "Quoted string without matching end-quote.", '\''));

				// Any alphanumeric ASCII character and all characters above the ASCII range are valid data characters
				// dataCharacter ::= [A-Za-z0-9] | [U+0080-U+1FFFFF]
				Parser dataCharacter = Prims.LetterOrDigit | Prims.Range('\u0080', char.MaxValue);
				// dataString ::= (escapeSequence | singleQuoteLiteral | quotedString | dataCharacter)
				//           (WS? (escapeSequence | singleQuoteLiteral | quotedString | dataCharacter))*
				Parser dataString = Ops.List(escapeSequence | singleQuoteLiteral | quotedString | dataCharacter, optionalWhiteSpace);

				// firstOrLast ::= 'first' | 'last'
				// primarySecondaryTertiary ::= 'primary' | 'secondary' | 'tertiary'
				// indirectOption ::= (primarySecondaryTertiary WS 'ignorable') | 'variable' | 'regular' | 'implicit' | 'trailing'
				// indirectPosition ::= '[' firstOrLast WS indirectOption ']'
				Parser firstOrLast = Ops.Choice("first", "last");
				Parser primarySecondaryTertiary = Ops.Choice("primary", "secondary", "tertiary");
				Parser indirectOption = Ops.Choice(Ops.Sequence(primarySecondaryTertiary, someWhiteSpace, "ignorable"),
													"variable", "regular", "implicit", "trailing");
				Parser indirectPosition = Ops.Sequence('[', Ops.Expect("icu0004", "Invalid indirect position specifier: unknown option",
					Ops.Sequence(firstOrLast, someWhiteSpace, indirectOption)),
					Ops.Expect("icu0005", "Indirect position specifier missing closing ']'", ']'));

				// expansion ::= dataString WS? '/' WS? dataString
				Parser expansion = Ops.Sequence(Ops.Expect("icu0006", "Invalid expansion: Data missing before '/'", dataString),
					optionalWhiteSpace, '/', optionalWhiteSpace,
					Ops.Expect("icu0007", "Invalid expansion: Data missing after '/'", dataString));

				// prefix ::= dataString WS? '|' WS? dataString
				Parser prefix = Ops.Sequence(Ops.Expect("icu0008", "Invalid prefix: Data missing before '|'", dataString),
					optionalWhiteSpace, '|', optionalWhiteSpace,
					Ops.Expect("icu0009", "Invalid prefix: Data missing after '|'", dataString));

				// dataElement ::= indirectPosition | expansion | prefix | dataString
				Parser dataElement = indirectPosition | expansion | prefix | dataString;

				// beforeOption ::= '[before' WS ('1' | '2' | '3') ']'
				Parser beforeOption = Ops.Sequence("[before", someWhiteSpace,
					Ops.Expect("icu0010", "Invalid 'before' specifier: Missing '1', '2', or '3'", Ops.Choice('1', '2', '3')),
					Ops.Expect("icu0011", "Invalid 'before' specifier: Missing closing ']'", ']'));

				_primaryDifference = new Rule("primaryDifference");
				_secondaryDifference = new Rule("secondaryDifference");
				_tertiaryDifference = new Rule("tertiaryDifference");
				_noDifference = new Rule("noDifference");
				_reset = new Rule("reset");
				_oneRule = new Rule("oneRule");
				_icuRules = new Rule("icuRules");

				// primaryDifference ::= '<' WS? dataElement
				_primaryDifference.Parser = Ops.Sequence('<', optionalWhiteSpace, dataElement);

				// secondaryDifference ::= '<<' WS? dataElement
				_secondaryDifference.Parser = Ops.Sequence("<<", optionalWhiteSpace, dataElement);

				// tertiaryDifference ::= '<<<' WS? dataElement
				_tertiaryDifference.Parser = Ops.Sequence("<<<", optionalWhiteSpace, dataElement);

				// noDifference ::= '=' WS? dataElement
				_noDifference.Parser = Ops.Sequence('=', optionalWhiteSpace, dataElement);

				// reset ::= '&' WS? beforeOption? WS? dataElement
				_reset.Parser = Ops.Sequence('&', optionalWhiteSpace, !beforeOption, optionalWhiteSpace, dataElement);

				// oneRule ::= reset (WS? (primaryDifference | secondaryDifference | tertiaryDifference | noDifference))*
				_oneRule.Parser = Ops.Sequence(_reset, Ops.ZeroOrMore(Ops.Sequence(optionalWhiteSpace,
											   _primaryDifference | _secondaryDifference | _tertiaryDifference | _noDifference)));

				// icuRules ::= (oneRule | WS)* EOF
				_icuRules.Parser = Ops.ZeroOrMore(_oneRule | optionalWhiteSpace);

				singleCharacterEscape.Act += OnSingleCharacterEscape;
				hex4Escape.Act += OnHexEscape;
				hex8Escape.Act += OnHexEscape;
				hex2Escape.Act += OnHexEscape;
				hexXEscape.Act += OnHexEscape;
				octalEscape.Act += OnOctalEscape;
				controlEscape.Act += OnControlEscape;
				singleQuoteLiteral.Act += OnSingleQuoteLiteral;
				quotedStringCharacter.Act += OnDataCharacter;
				dataCharacter.Act += OnDataCharacter;
			}

			private StringBuilder _currentDataElement;

			private void OnSingleCharacterEscape(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value.Length == 1);
				Dictionary<char, char> substitutes = new Dictionary<char, char>(7);
				substitutes['a'] = '\u0007';
				substitutes['b'] = '\u0008';
				substitutes['t'] = '\u0009';
				substitutes['n'] = '\u000A';
				substitutes['v'] = '\u000B';
				substitutes['f'] = '\u000C';
				substitutes['r'] = '\u000D';
				char newChar = args.Value[0];
				if (substitutes.ContainsKey(newChar))
				{
					newChar = substitutes[newChar];
				}
				_currentDataElement.Append(newChar);
			}

			private void OnHexEscape(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value.Length >= 2 && args.Value.Length <= 11);
				string hex = args.Value.Substring(1);
				if (hex[0] == '{')
				{
					hex = hex.Substring(1, hex.Length - 2);
				}
				AddCharacterFromCode(Int32.Parse(hex, NumberStyles.AllowHexSpecifier));
			}

			private void OnOctalEscape(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value.Length >= 2 && args.Value.Length <= 4);
				// There is no octal number style, so we have to do it manually.
				int code = 0;
				for (int i=1; i < args.Value.Length; i++)
				{
					code = code * 8 + Int32.Parse(args.Value.Substring(i, 1));
				}
				AddCharacterFromCode(code);
			}

			private void OnControlEscape(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value.Length == 2);
				AddCharacterFromCode(args.Value[1] & 0x1F);
			}

			private void AddCharacterFromCode(int code)
			{
				try
				{
					_currentDataElement.Append(Char.ConvertFromUtf32(code));
				}
				catch (ArgumentOutOfRangeException e)
				{
					string message =
						string.Format("Unable to parse ICU: Invalid character code (U+{0:X}) in escape sequence.", code);
					throw new ApplicationException(message, e);
				}
			}

			private void OnSingleQuoteLiteral(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value == "''");
				_currentDataElement.Append('\'');
			}

			private void OnDataCharacter(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value.Length == 1);
				_currentDataElement.Append(args.Value);
			}
		}
	}
}