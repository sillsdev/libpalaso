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
			UpdateCollationElement(dom, ws);

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

			XmlNode alias = XmlHelpers.GetOrCreateElement(parentNode, ".", "alias", string.Empty, _nameSpaceManager);
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
			// remove existing rules as we're about to write all of those
			XmlHelpers.RemoveElement(parentNode, "rules", _nameSpaceManager);
			XmlNode rulesNode = XmlHelpers.GetOrCreateElement(parentNode, ".", "rules", string.Empty, _nameSpaceManager);
			ICURulesParser parser = new ICURulesParser();
			parser.AddIcuRulesToNode(rulesNode, icu);
		}

		private class ICURulesParser
		{
			private XmlNode _parentNode;
			private XmlDocument _dom;
			private Rule _icuRules;

			// You will notice that the xml tags used don't always match my parser/rule variable name.  We have
			// the creators of ldml and icu to thank for that.  Collation in ldml is based off of icu and is pretty
			// much a straight conversion of icu operators into xml tags.  Unfortunately, ICU refers to some constructs
			// with one name (which I used for my variable names), but ldml uses a different name for the actual
			// xml tag.
			// http://unicode.org/reports/tr35/#Collation_Elements - ldml collation element spec
			// http://icu-project.org/userguide/Collate_Customization.html - ICU collation spec
			public ICURulesParser() : this(false) {}

			public ICURulesParser(bool useDebugger)
			{
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
				Parser singleCharacterEscape = Prims.AnyChar - Ops.Choice('u', 'U', 'x', octalDigit, 'c');
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
				Parser singleQuoteLiteral = Prims.Str("''");
				Parser quotedStringCharacter = Prims.AnyChar - '\'';
				Parser quotedString = Ops.Sequence('\'', Ops.OneOrMore(singleQuoteLiteral | quotedStringCharacter),
					Ops.Expect("icu0003", "Quoted string without matching end-quote.", '\''));

				// Any alphanumeric ASCII character and all characters above the ASCII range are valid data characters
				// normalCharacter ::= [A-Za-z0-9] | [U+0080-U+1FFFFF]
				// dataCharacter ::= normalCharacter | singleQuoteLiteral | escapeSequence
				// dataString ::= (dataCharacter | quotedString) (WS? (dataCharacter | quotedString))*
				Parser normalCharacter = Prims.LetterOrDigit | Prims.Range('\u0080', char.MaxValue);
				Parser dataCharacter = normalCharacter | singleQuoteLiteral | escapeSequence;
				Rule dataString = new Rule(Ops.List(dataCharacter | quotedString, optionalWhiteSpace));

				// firstOrLast ::= 'first' | 'last'
				// primarySecondaryTertiary ::= 'primary' | 'secondary' | 'tertiary'
				// indirectOption ::= (primarySecondaryTertiary WS 'ignorable') | 'variable' | 'regular' | 'implicit' | 'trailing'
				// indirectPosition ::= '[' firstOrLast WS indirectOption ']'
				// According to the LDML spec, "implicit" should not be allowed in a reset element, but we're not going to check that
				Parser firstOrLast = Ops.Choice("first", "last");
				Parser primarySecondaryTertiary = Ops.Choice("primary", "secondary", "tertiary");
				Parser indirectOption = Ops.Choice(Ops.Sequence(primarySecondaryTertiary, someWhiteSpace, "ignorable"),
													"variable", "regular", "implicit", "trailing");
				Rule indirectPosition = new Rule(Ops.Sequence('[', Ops.Expect("icu0004", "Invalid indirect position specifier: unknown option",
					Ops.Sequence(firstOrLast, someWhiteSpace, indirectOption)),
					Ops.Expect("icu0005", "Indirect position specifier missing closing ']'", ']')));

				// top ::= "[top]"
				// [top] is a deprecated element in ICU and should be replaced by indirect positioning.
				Parser top = Prims.Str("[top]");

				// simpleElement ::= indirectPosition | dataString
				Rule simpleElement = new Rule("simpleElement", indirectPosition | dataString);

				// expansion ::= WS? '/' WS? simpleElement
				Rule expansion = new Rule("extend", Ops.Sequence(optionalWhiteSpace, '/', optionalWhiteSpace,
					Ops.Expect("icu0007", "Invalid expansion: Data missing after '/'", simpleElement)));
				// prefix ::= simpleElement WS? '|' WS?
				Rule prefix = new Rule("context", Ops.Sequence(simpleElement, optionalWhiteSpace, '|', optionalWhiteSpace));
				// extendedElement ::= (prefix simpleElement expansion?) | (prefix? simpleElement expansion)
				Rule extendedElement = new Rule("x", Ops.Sequence(prefix, simpleElement, !expansion) |
													 Ops.Sequence(!prefix, simpleElement, expansion));

				// dataElement ::= simpleElement | extendedElement
				Parser dataElement = simpleElement | extendedElement;

				// beforeOption ::= '1' | '2' | '3'
				// beforeSpecifier ::= "[before" WS beforeOption ']'
				Parser beforeOption = Ops.Choice('1', '2', '3');
				Parser beforeSpecifier = Ops.Sequence("[before", someWhiteSpace,
					Ops.Expect("icu0010", "Invalid 'before' specifier: Invalid or missing option", beforeOption),
					Ops.Expect("icu0011", "Invalid 'before' specifier: Missing closing ']'", ']'));

				// primaryDifferenceOperator ::= '<'
				// secondaryDifferenceOperator ::= "<<"
				// tertiaryDifferenceOperator ::= "<<<"
				// noDifferenceOperator ::= '='
				// differenceOperator ::= primaryDifferenceOperator | secondaryDifferenceOperator | tertiaryDifferenceOperator | noDifferenceOperator
				Rule primaryDifferenceOperator = new Rule("p", Prims.Ch('<'));
				Rule secondaryDifferenceOperator = new Rule("s", Prims.Str("<<"));
				Rule tertiaryDifferenceOperator = new Rule("t", Prims.Str("<<<"));
				Rule noDifferenceOperator = new Rule("i", Prims.Ch('='));
				Parser differenceOperator = primaryDifferenceOperator | secondaryDifferenceOperator
					| tertiaryDifferenceOperator | noDifferenceOperator;
				// difference ::= differenceOperator WS? dataElement
				Parser difference = Ops.Sequence(differenceOperator, optionalWhiteSpace, dataElement);

				// reset ::= '&' WS? ((beforeSpecifier? WS? simpleElement) | top)
				Rule reset = new Rule("reset", Ops.Sequence('&', optionalWhiteSpace,
					Ops.Sequence(!beforeSpecifier, optionalWhiteSpace, simpleElement) | top));

				// oneRule ::= reset (WS? difference)*
				Rule oneRule = new Rule("oneRule", Ops.Sequence(reset, Ops.ZeroOrMore(Ops.Sequence(optionalWhiteSpace,
											   difference))));

				// icuRules ::= (oneRule | WS)* EOF
				_icuRules = new Rule("icuRules", Ops.ZeroOrMore(oneRule | optionalWhiteSpace));

				singleCharacterEscape.Act += OnSingleCharacterEscape;
				hex4Escape.Act += OnHexEscape;
				hex8Escape.Act += OnHexEscape;
				hex2Escape.Act += OnHexEscape;
				hexXEscape.Act += OnHexEscape;
				octalEscape.Act += OnOctalEscape;
				controlEscape.Act += OnControlEscape;
				singleQuoteLiteral.Act += OnSingleQuoteLiteral;
				quotedStringCharacter.Act += OnDataCharacter;
				normalCharacter.Act += OnDataCharacter;
				dataString.PreParse += OnStartNewDataString;
				dataString.Act += OnDataString;
				firstOrLast.Act += OnIndirectPiece;
				primarySecondaryTertiary.Act += OnIndirectPiece;
				indirectOption.Act += OnIndirectPiece;
				indirectPosition.PreParse += OnStartNewIndirectPosition;
				indirectPosition.Act += OnIndirectPosition;
				top.Act += OnTop;
				simpleElement.PreParse += OnSimpleElementBegin;
				simpleElement.PostParse += OnSimpleElementEnd;
				expansion.PreParse += OnElementWithDataBegin;
				prefix.PreParse += OnElementWithDataBegin;
				extendedElement.PreParse += OnElementBegin;
				extendedElement.PostParse += OnElementEnd;
				primaryDifferenceOperator.PreParse += OnElementWithDataBegin;
				secondaryDifferenceOperator.PreParse += OnElementWithDataBegin;
				tertiaryDifferenceOperator.PreParse += OnElementWithDataBegin;
				noDifferenceOperator.PreParse += OnElementWithDataBegin;
				beforeOption.Act += OnBeforeOption;
				reset.PreParse += OnElementWithDataBegin;
				_icuRules.PostParse += OnIcuRulesEnd;
			}

			public void AddIcuRulesToNode(XmlNode parentNode, string icuRules)
			{
				if (parentNode == null)
				{
					throw new ArgumentNullException("parentNode");
				}
				_parentNode = parentNode;
				_currentNode = parentNode;
				// OwnerDocument will be null if the node is an XmlDocument
				_dom = _parentNode.OwnerDocument ?? (XmlDocument)_parentNode;
				_currentOperators = new Stack<string>();
				StringScanner sc = new StringScanner(icuRules);
				ParserMatch match = _icuRules.Parse(sc);
				Debug.Assert(match.Success);
				Debug.Assert(sc.AtEnd);
			}

			private StringBuilder _currentDataString;
			private XmlNode _currentNode;
			private Stack<string> _currentOperators;
			private StringBuilder _currentIndirectPosition;

			private void BeginElement(string name)
			{
				XmlNode node = _dom.CreateElement(name);
				_currentNode.AppendChild(node);
				_currentNode = node;
			}

			private void EndElement()
			{
				Debug.Assert(_currentNode != _parentNode);
				_currentNode = _currentNode.ParentNode;
			}

			private void OnElementBegin(object sender, PreParseEventArgs args)
			{
				BeginElement(args.Parser.ID);
			}

			private void OnElementEnd(object sender, PostParseEventArgs args)
			{
				EndElement();
			}

			private void OnElementWithDataBegin(object sender, PreParseEventArgs args)
			{
				_currentOperators.Push(args.Parser.ID);
			}

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
				_currentDataString.Append(newChar.ToString());
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
					_currentDataString.Append(Char.ConvertFromUtf32(code));
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
				_currentDataString.Append("\'");
			}

			private void OnDataCharacter(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value.Length == 1);
				_currentDataString.Append(args.Value);
			}

			private void OnStartNewDataString(object sender, PreParseEventArgs args)
			{
				_currentDataString = new StringBuilder();
			}

			private void OnDataString(object sender, ActionEventArgs args)
			{
				_currentNode.InnerText = _currentDataString.ToString();
				_currentDataString = null;
			}

			private void OnIndirectPiece(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value.Length > 0);
				if (_currentIndirectPosition.Length > 0)
				{
					_currentIndirectPosition.Append('_');
				}
				// due to slight differences between ICU and LDML, regular becomes non_ignorable
				_currentIndirectPosition.Append(args.Value.Replace("regular", "non_ignorable"));
			}

			private void OnStartNewIndirectPosition(object sender, PreParseEventArgs args)
			{
				_currentIndirectPosition = new StringBuilder();
			}

			private void OnIndirectPosition(object sender, ActionEventArgs args)
			{
				BeginElement(_currentIndirectPosition.ToString());
				EndElement();
				_currentIndirectPosition = null;
			}

			private void OnSimpleElementBegin(object sender, PreParseEventArgs args)
			{
				BeginElement(_currentOperators.Pop());
			}

			private void OnSimpleElementEnd(object sender, PostParseEventArgs args)
			{
				EndElement();
			}

			private void OnBeforeOption(object sender, ActionEventArgs args)
			{
				Debug.Assert(args.Value.Length == 1);
				Dictionary<string, string> optionMap = new Dictionary<string, string>(3);
				optionMap["1"] = "primary";
				optionMap["2"] = "secondary";
				optionMap["3"] = "tertiary";
				Debug.Assert(optionMap.ContainsKey(args.Value));
				XmlHelpers.AddOrUpdateAttribute(_currentNode, "before", optionMap[args.Value]);
			}

			private void OnTop(object sender, ActionEventArgs args)
			{
				// [top] is deprecated in ICU and not directly allowed in LDML
				// [top] is probably best rendered the same as [before 1] [first tertiary ignorable]
				XmlHelpers.AddOrUpdateAttribute(_currentNode, "before", "primary");
				BeginElement("first_tertiary_ignorable");
				EndElement();
			}

			// Use this to optimize successive difference elements into one element
			private void OnIcuRulesEnd(object sender, PostParseEventArgs args)
			{
				// we can optimize primary, secondary, tertiary, and identity elements
				List<string> optimizableElementNames = new List<string>(new string[] {"p", "s", "t", "i"});
				List<List<XmlNode>> optimizableNodeGroups = new List<List<XmlNode>>();
				List<XmlNode> currentOptimizableNodeGroup = null;

				// first, build a list of lists of nodes we can optimize
				foreach (XmlNode node in _currentNode.ChildNodes)
				{
					// Current node can be part of an optimized group if it is an allowed element
					// AND it only contains one unicode code point.
					bool nodeOptimizable = (optimizableElementNames.Contains(node.Name) &&
						(node.InnerText.Length == 1 ||
						(node.InnerText.Length == 2 && Char.IsSurrogatePair(node.InnerText, 0))));
					// If we have a group of optimizable nodes, but we can't add the current node to that group
					if (currentOptimizableNodeGroup != null && (!nodeOptimizable || currentOptimizableNodeGroup[0].Name != node.Name))
					{
						// Add our current group to the to-be-optimized list only if it makes sense (i.e. it has more than 1 node)
						if (currentOptimizableNodeGroup.Count > 1)
						{
							optimizableNodeGroups.Add(currentOptimizableNodeGroup);
						}
						currentOptimizableNodeGroup = null;
					}
					if (!nodeOptimizable)
					{
						continue;
					}
					// start a new optimizable group if we're not in one already
					if (currentOptimizableNodeGroup == null)
					{
						currentOptimizableNodeGroup = new List<XmlNode>();
					}
					currentOptimizableNodeGroup.Add(node);
				}
				// add the last group, if any
				if (currentOptimizableNodeGroup != null && currentOptimizableNodeGroup.Count > 1)
				{
					optimizableNodeGroups.Add(currentOptimizableNodeGroup);
				}

				// We've got out groups of optimizable nodes, so we now optimize them
				foreach (List<XmlNode> nodeGroup in optimizableNodeGroups)
				{
					// luckily the optimized names are the same as the unoptimized with 'c' appended
					// so <p> becomes <pc>, <s> to <sc>, et al.
					XmlNode optimizedNode = _dom.CreateElement(nodeGroup[0].Name + "c");
					foreach (XmlNode node in nodeGroup)
					{
						optimizedNode.InnerText += node.InnerText;
					}
					// put it in the correct order in our rules.  THIS IS VERY IMPORTANT!
					_currentNode.InsertBefore(optimizedNode, nodeGroup[0]);
					foreach (XmlNode node in nodeGroup)
					{
						_currentNode.RemoveChild(node);
					}
				}
			}
		}
	}
}