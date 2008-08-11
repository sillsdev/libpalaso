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
using Spart.Actions;
using Spart.Parsers;
using Spart.Parsers.NonTerminal;
using Spart.Parsers.Primitives;
using Spart.Scanners;
using Debugger = Spart.Debug.Debugger;

namespace Palaso.WritingSystems.Collation
{
	class IcuRulesParser
	{
		private XmlNode _parentNode;
		private XmlDocument _dom;
		private Debugger _debugger;
		private bool _useDebugger;
		private XmlNamespaceManager _nameSpaceManager;

		// You will notice that the xml tags used don't always match my parser/rule variable name.  We have
		// the creators of ldml and icu to thank for that.  Collation in ldml is based off of icu and is pretty
		// much a straight conversion of icu operators into xml tags.  Unfortunately, ICU refers to some constructs
		// with one name (which I used for my variable names), but ldml uses a different name for the actual
		// xml tag.
		// http://unicode.org/reports/tr35/#Collation_Elements - LDML collation element spec
		// http://icu-project.org/userguide/Collate_Customization.html - ICU collation spec
		// http://www.unicode.org/reports/tr10/ - UCA (Unicode Collation Algorithm) spec
		public IcuRulesParser() : this(false) {}

		public IcuRulesParser(bool useDebugger)
		{
			_useDebugger = useDebugger;
		}

		public void AddIcuRulesToNode(XmlNode parentNode, string icuRules, XmlNamespaceManager nameSpaceManager)
		{
			if (parentNode == null)
			{
				throw new ArgumentNullException("parentNode");
			}
			_parentNode = parentNode;
			_nameSpaceManager = nameSpaceManager;
			// OwnerDocument will be null if the node is an XmlDocument
			_dom = _parentNode.OwnerDocument ?? (XmlDocument)_parentNode;
			DefineParsingRules();
			AssignSemanticActions();
			InitializeDataObjects();
			StringScanner sc = new StringScanner(icuRules);
			ParserMatch match = _icuRules.Parse(sc);
			ClearDataObjects();
			if (!match.Success || !sc.AtEnd)
			{
				throw new ApplicationException("Invalid ICU rules.");
			}
		}

		public bool ValidateIcuRules(string icuRules, out string message)
		{
			// I was going to run with no semantic actions and therefore no dom needed,
			// but some actions can throw (OnOptionVariableTop for one),
			// so we need to run the actions as well for full validation.
			_dom = new XmlDocument();
			_nameSpaceManager = new XmlNamespaceManager(_dom.NameTable);
			_parentNode = _dom;
			DefineParsingRules();
			AssignSemanticActions();
			InitializeDataObjects();
			StringScanner sc = new StringScanner(icuRules);
			ParserMatch match;
			try
			{
				match = _icuRules.Parse(sc);
			}
			catch (Exception e)
			{
				message = e.Message;
				return false;
			}
			finally
			{
				ClearDataObjects();
			}
			if (!match.Success || !sc.AtEnd)
			{
				message = "Invalid ICU rules.";
				return false;
			}
			message = string.Empty;
			return true;
		}

		private Parser _someWhiteSpace;
		private Parser _optionalWhiteSpace;
		private Parser _octalDigit;
		private Parser _hexDigitExpect;
		private Parser _hex4Group;
		private Parser _singleCharacterEscape;
		private Parser _hex4Escape;
		private Parser _hex8Escape;
		private Parser _hex2Escape;
		private Parser _hexXEscape;
		private Parser _octalEscape;
		private Parser _controlEscape;
		private Parser _escapeSequence;
		private Parser _singleQuoteLiteral;
		private Parser _quotedStringCharacter;
		private Parser _quotedString;
		private Parser _normalCharacter;
		private Parser _dataCharacter;
		private Rule _dataString;
		private Parser _firstOrLast;
		private Parser _primarySecondaryTertiary;
		private Parser _indirectOption;
		private Rule _indirectPosition;
		private Parser _top;
		private Rule _simpleElement;
		private Rule _expansion;
		private Rule _prefix;
		private Parser _extendedElement;
		private Parser _beforeOption;
		private Parser _beforeSpecifier;
		private Parser _differenceOperator;
		private Rule _simpleDifference;
		private Rule _extendedDifference;
		private Parser _difference;
		private Rule _reset;
		private Rule _oneRule;
		private Parser _optionAlternate;
		private Parser _optionBackwards;
		private Parser _optionNormalization;
		private Parser _optionOnOff;
		private Parser _optionCaseLevel;
		private Parser _optionCaseFirst;
		private Parser _optionStrength;
		private Parser _optionHiraganaQ;
		private Parser _optionNumeric;
		private Parser _optionVariableTop;
		private Rule _option;
		private Rule _icuRules;

		private void DefineParsingRules()
		{
			// someWhiteSpace ::= WS+
			// optionalWhiteSpace ::= WS*
			_someWhiteSpace = Ops.OneOrMore(Prims.WhiteSpace);
			_optionalWhiteSpace = Ops.ZeroOrMore(Prims.WhiteSpace);

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
			_octalDigit = Prims.Range('0', '7');
			_hexDigitExpect = Ops.Expect("icu0001", "Invalid hexadecimal character in escape sequence.", Prims.HexDigit);
			_hex4Group = Ops.Sequence(_hexDigitExpect, _hexDigitExpect, _hexDigitExpect, _hexDigitExpect);
			_singleCharacterEscape = Prims.AnyChar - Ops.Choice('u', 'U', 'x', _octalDigit, 'c');
			_hex4Escape = Ops.Sequence('u', _hex4Group);
			_hex8Escape = Ops.Sequence('U', _hex4Group, _hex4Group);
			_hex2Escape = Ops.Sequence('x', _hexDigitExpect, !Prims.HexDigit);
			_hexXEscape = Ops.Sequence('x', '{', _hexDigitExpect, !Prims.HexDigit, !Prims.HexDigit, !Prims.HexDigit,
												 !Prims.HexDigit, !Prims.HexDigit, !Prims.HexDigit, !Prims.HexDigit, '}');
			_octalEscape = Ops.Sequence(_octalDigit, !_octalDigit, !_octalDigit);
			_controlEscape = Ops.Sequence('c', Prims.AnyChar);
			_escapeSequence = Ops.Sequence('\\', Ops.Expect("icu0002", "Invalid escape sequence.",
				_singleCharacterEscape | _hex4Escape | _hex8Escape | _hex2Escape | _hexXEscape | _octalEscape | _controlEscape));

			// singleQuoteLiteral ::= "''"
			// quotedStringCharacter ::= AllChars - "'"
			// quotedString ::= "'" (singleQuoteLiteral | quotedStringCharacter)+ "'"
			_singleQuoteLiteral = Prims.Str("''");
			_quotedStringCharacter = Prims.AnyChar - '\'';
			_quotedString = Ops.Sequence('\'', Ops.OneOrMore(_singleQuoteLiteral | _quotedStringCharacter),
				Ops.Expect("icu0003", "Quoted string without matching end-quote.", '\''));

			// Any alphanumeric ASCII character and all characters above the ASCII range are valid data characters
			// normalCharacter ::= [A-Za-z0-9] | [U+0080-U+1FFFFF]
			// dataCharacter ::= normalCharacter | singleQuoteLiteral | escapeSequence
			// dataString ::= (dataCharacter | quotedString) (WS? (dataCharacter | quotedString))*
			_normalCharacter = Prims.LetterOrDigit | Prims.Range('\u0080', char.MaxValue);
			_dataCharacter = _normalCharacter | _singleQuoteLiteral | _escapeSequence;
			_dataString = new Rule(Ops.List(_dataCharacter | _quotedString, _optionalWhiteSpace));

			// firstOrLast ::= 'first' | 'last'
			// primarySecondaryTertiary ::= 'primary' | 'secondary' | 'tertiary'
			// indirectOption ::= (primarySecondaryTertiary WS 'ignorable') | 'variable' | 'regular' | 'implicit' | 'trailing'
			// indirectPosition ::= '[' WS? firstOrLast WS indirectOption WS? ']'
			// According to the LDML spec, "implicit" should not be allowed in a reset element, but we're not going to check that
			_firstOrLast = Ops.Choice("first", "last");
			_primarySecondaryTertiary = Ops.Choice("primary", "secondary", "tertiary");
			_indirectOption = Ops.Choice(Ops.Sequence(_primarySecondaryTertiary, _someWhiteSpace, "ignorable"),
										 "variable", "regular", "implicit", "trailing");
			_indirectPosition = new Rule(Ops.Sequence('[', _optionalWhiteSpace,
				Ops.Expect("icu0004", "Invalid indirect position specifier: unknown option",
				Ops.Sequence(_firstOrLast, _someWhiteSpace, _indirectOption)), _optionalWhiteSpace,
				Ops.Expect("icu0005", "Indirect position specifier missing closing ']'", ']')));

			// top ::= '[' WS? 'top' WS? ']'
			// [top] is a deprecated element in ICU and should be replaced by indirect positioning.
			_top = Ops.Sequence('[', _optionalWhiteSpace, "top", _optionalWhiteSpace, ']');

			// simpleElement ::= indirectPosition | dataString
			_simpleElement = new Rule("simpleElement", _indirectPosition | _dataString);

			// expansion ::= WS? '/' WS? simpleElement
			_expansion = new Rule("extend", Ops.Sequence(_optionalWhiteSpace, '/', _optionalWhiteSpace,
				Ops.Expect("icu0007", "Invalid expansion: Data missing after '/'", _simpleElement)));
			// prefix ::= simpleElement WS? '|' WS?
			_prefix = new Rule("context", Ops.Sequence(_simpleElement, _optionalWhiteSpace, '|', _optionalWhiteSpace));
			// extendedElement ::= (prefix simpleElement expansion?) | (prefix? simpleElement expansion)
			_extendedElement = Ops.Sequence(_prefix, _simpleElement, !_expansion) |
							   Ops.Sequence(!_prefix, _simpleElement, _expansion);

			// beforeOption ::= '1' | '2' | '3'
			// beforeSpecifier ::= '[' WS? 'before' WS beforeOption WS? ']'
			_beforeOption = Ops.Choice('1', '2', '3');
			_beforeSpecifier = Ops.Sequence('[', _optionalWhiteSpace, "before", _someWhiteSpace,
				Ops.Expect("icu0010", "Invalid 'before' specifier: Invalid or missing option", _beforeOption), _optionalWhiteSpace,
				Ops.Expect("icu0011", "Invalid 'before' specifier: Missing closing ']'", ']'));

			// The difference operator initially caused some problems with parsing.  The spart library doesn't
			// handle situations where the first choice is the beginning of the second choice.
			// Ex:  differenceOperator = "<" | "<<" | "<<<" | "="     DOES NOT WORK!
			// That will fail to parse bothe the << and <<< operators because it always thinks it should match <.
			// However, differenceOperator = "<<<" | "<<" | "<" | "=" will work because it tries to match <<< first.
			// I'm using this strange production with the option '<' characters because it also works and doesn't
			// depend on order.  It is less likely for someone to change it and unknowingly mess it up.
			// differenceOperator ::=  ('<' '<'? '<'?) | '='
			_differenceOperator = Ops.Sequence('<', !Prims.Ch('<'), !Prims.Ch('<')) | Prims.Ch('=');

			// simpleDifference ::= differenceOperator WS? simpleElement
			// extendedDifference ::= differenceOperator WS? extendedElement
			// difference ::= simpleDifference | extendedDifference
			// NOTE: Due to issues in the parser, extendedDifference MUST COME BEFORE simpleDifference in the difference definition
			_simpleDifference = new Rule("simpleDifference", Ops.Sequence(_differenceOperator, _optionalWhiteSpace, _simpleElement));
			_extendedDifference = new Rule("x", Ops.Sequence(_differenceOperator, _optionalWhiteSpace, _extendedElement));
			_difference = _extendedDifference | _simpleDifference;

			// reset ::= '&' WS? ((beforeSpecifier? WS? simpleElement) | top)
			_reset = new Rule("reset", Ops.Sequence('&', _optionalWhiteSpace,
				Ops.Sequence(!_beforeSpecifier, _optionalWhiteSpace, _simpleElement) | _top));

			// This option is a weird one, as it can come at any place in a rule and sets the preceding
			// dataString as the variable top option in the settings element.  So, it has to look at the
			// data for the preceding element to know its own value, but leaves the preceding and any
			// succeeding elements as if the variable top option wasn't there.  Go figure.
			// Also, it's really probably only valid following a simpleDifference or reset with a dataString
			// and not an indirect position, but checking for all that in the grammar would be very convoluted, so
			// we'll do it in the semantic action and throw.  Yuck.
			// optionVariableTop ::= '<' WS? '[' WS? 'variable' WS? 'top' WS? ']'
			_optionVariableTop = Ops.Sequence('<', _optionalWhiteSpace, '[', _optionalWhiteSpace, "variable",
											  _optionalWhiteSpace, "top", _optionalWhiteSpace, ']');

			// oneRule ::= reset (WS? (optionVariableTop | difference))*
			_oneRule = new Rule("oneRule", Ops.Sequence(_reset, Ops.ZeroOrMore(Ops.Sequence(_optionalWhiteSpace,
								_difference))));

			// Option notes:
			// * The 'strength' option is specified in ICU as having valid values 1-4 and 'I'.  In the LDML spec, it
			//   seems to indicate that valid values in ICU are 1-5, so I am accepting both and treating 'I' and '5'
			//   as the same.  I'm also accepting 'I' and 'i', although my approach is, in general, to be case-sensitive.
			// * The 'numeric' option is not mentioned on the ICU website, but it is implied as being acceptable ICU
			//   in the LDML spec, so I am supporting it here.
			// * There are LDML options 'match-boundaries' and 'match-style' that are not in ICU, so they are not listed here.
			// * The UCA spec seems to indicate that there is a 'locale' option which is not mentioned in either the
			//   LDML or ICU specs, so I am not supporting it here.  It could be referring to the 'base' element that
			//   is an optional part of the 'collation' element in LDML.
			// optionOnOff ::= 'on' | 'off'
			// optionAlternate ::= 'alternate' WS ('non-ignorable' | 'shifted')
			// optionBackwards ::= 'backwards' WS ('1' | '2')
			// optionNormalization ::= 'normalization' WS optionOnOff
			// optionCaseLevel ::= 'caseLevel' WS optionOnOff
			// optionCaseFirst ::= 'caseFirst' WS ('off' | 'upper' | 'lower')
			// optionStrength ::= 'strength' WS ('1' | '2' | '3' | '4' | 'I' | 'i' | '5')
			// optionHiraganaQ ::= 'hiraganaQ' WS optionOnOff
			// optionNumeric ::= 'numeric' WS optionOnOff
			// option ::= '[' WS? (optionAlternate | optionBackwards | optionNormalization | optionCaseLevel
			//            optionCaseFirst | optionStrength | optionHiraganaQ | optionNumeric) WS? ']'
			_optionOnOff = Ops.Choice("on", "off");
			_optionAlternate = Ops.Sequence("alternate", _someWhiteSpace, Ops.Choice("non-ignorable", "shifted"));
			_optionBackwards = Ops.Sequence("backwards", _someWhiteSpace, Ops.Choice('1', '2'));
			_optionNormalization = Ops.Sequence("normalization", _someWhiteSpace, _optionOnOff);
			_optionCaseLevel = Ops.Sequence("caseLevel", _someWhiteSpace, _optionOnOff);
			_optionCaseFirst = Ops.Sequence("caseFirst", _someWhiteSpace, Ops.Choice("off", "upper", "lower"));
			_optionStrength = Ops.Sequence("strength", _someWhiteSpace, Ops.Choice('1', '2', '3', '4', 'I', 'i', '5'));
			_optionHiraganaQ = Ops.Sequence("hiraganaQ", _someWhiteSpace, _optionOnOff);
			_optionNumeric = Ops.Sequence("numeric", _someWhiteSpace, _optionOnOff);
			_option = new Rule("option", Ops.Sequence('[', _optionalWhiteSpace, _optionAlternate |
				_optionBackwards | _optionNormalization | _optionCaseLevel | _optionCaseFirst |
				_optionStrength | _optionHiraganaQ | _optionNumeric, _optionalWhiteSpace, ']'));

			// I don't know if ICU requires all options first (it's unclear), but I am. :)
			// icuRules ::= WS? (option WS?)* (oneRule WS?)+ EOF
			_icuRules = new Rule("icuRules", Ops.Sequence(_optionalWhiteSpace,
				Ops.ZeroOrMore(Ops.Sequence(_option, _optionalWhiteSpace)),
				Ops.OneOrMore(Ops.Sequence(_oneRule, _optionalWhiteSpace)), Prims.End));

			if (_useDebugger)
			{
				_debugger = new Debugger(Console.Out);
				_debugger += _option;
				_debugger += _oneRule;
				_debugger += _reset;
				_debugger += _simpleElement;
				_debugger += _simpleDifference;
				_debugger += _extendedDifference;
				_debugger += _dataString;
			}
		}

		private void AssignSemanticActions()
		{
			_singleCharacterEscape.Act += OnSingleCharacterEscape;
			_hex4Escape.Act += OnHexEscape;
			_hex8Escape.Act += OnHexEscape;
			_hex2Escape.Act += OnHexEscape;
			_hexXEscape.Act += OnHexEscape;
			_octalEscape.Act += OnOctalEscape;
			_controlEscape.Act += OnControlEscape;
			_singleQuoteLiteral.Act += OnSingleQuoteLiteral;
			_quotedStringCharacter.Act += OnDataCharacter;
			_normalCharacter.Act += OnDataCharacter;
			_dataString.Act += OnDataString;
			_firstOrLast.Act += OnIndirectPiece;
			_primarySecondaryTertiary.Act += OnIndirectPiece;
			_indirectOption.Act += OnIndirectPiece;
			_indirectPosition.Act += OnIndirectPosition;
			_top.Act += OnTop;
			_expansion.Act += OnElementWithData;
			_prefix.Act += OnElementWithData;
			_differenceOperator.Act += OnDifferenceOperator;
			_simpleDifference.Act += OnSimpleDifference;
			_extendedDifference.Act += OnExtendedDifference;
			_beforeOption.Act += OnBeforeOption;
			_reset.Act += OnReset;
			_optionAlternate.Act += OnOptionNormal;
			_optionBackwards.Act += OnOptionBackwards;
			_optionNormalization.Act += OnOptionNormal;
			_optionCaseLevel.Act += OnOptionNormal;
			_optionCaseFirst.Act += OnOptionNormal;
			_optionStrength.Act += OnOptionStrength;
			_optionHiraganaQ.Act += OnOptionHiraganaQ;
			_optionNumeric.Act += OnOptionNormal;
			_optionVariableTop.Act += OnOptionVariableTop;
			_icuRules.Act += OnIcuRules;
		}

		private StringBuilder _currentDataString;
		private Stack<XmlNode> _currentDataObjects;
		private List<XmlNode> _currentNodes;
		private Stack<string> _currentOperators;
		private StringBuilder _currentIndirectPosition;
		private Queue<XmlNode> _attributesForReset;
		private List<XmlNode> _optionAttributes;

		private void InitializeDataObjects()
		{
			_currentDataString = new StringBuilder();
			_currentDataObjects = new Stack<XmlNode>();
			_currentNodes = new List<XmlNode>();
			_currentOperators = new Stack<string>();
			_currentIndirectPosition = new StringBuilder();
			_attributesForReset = new Queue<XmlNode>();
			_optionAttributes = new List<XmlNode>();
		}

		private void ClearDataObjects()
		{
			_currentDataString = null;
			_currentDataObjects = null;
			_currentNodes = null;
			_currentOperators = null;
			_currentIndirectPosition = null;
			_attributesForReset = null;
		}

		private XmlNode AddXmlNodeWithData(string name)
		{
			XmlNode node = _dom.CreateElement(name);
			node.AppendChild(_currentDataObjects.Pop());
			_currentNodes.Add(node);
			return node;
		}

		private void OnReset(object sender, ActionEventArgs args)
		{
			XmlNode node = AddXmlNodeWithData(((Rule)sender).ID);
			foreach (XmlNode attr in _attributesForReset)
			{
				node.Attributes.SetNamedItem(attr);
			}
			_attributesForReset = new Queue<XmlNode>();
		}

		private void OnSimpleDifference(object sender, ActionEventArgs args)
		{
			AddXmlNodeWithData(_currentOperators.Pop());
		}

		// The extended element is the trickiest one becuase the operator gets nested in an extended tag, and
		// can be preceded by another element as well.  Here's a simple rule:
		// &a < b  => <reset>a</reset><p>b</p>
		// And the extended one for comparison:
		// &a < b | c / d => <reset>a</reset><x><context>b</context><p>c</p><extend>d</extend></x>
		// The operator tag is inserted inside the "x" tag, and comes after the "context" tag (represented by '|')
		private void OnExtendedDifference(object sender, ActionEventArgs args)
		{
			// There should always at least be 2 nodes by this point - reset and either prefix or expansion
			Debug.Assert(_currentNodes.Count >= 2);
			XmlNode differenceNode = _dom.CreateElement("x");
			XmlNode dataNode = _dom.CreateElement(_currentOperators.Pop());
			dataNode.AppendChild(_currentDataObjects.Pop());
			XmlNode prefixNode = _currentNodes[_currentNodes.Count - 2];
			XmlNode expansionNode = _currentNodes[_currentNodes.Count - 1];
			int deleteCount = 0;
			if (expansionNode.Name != "extend")
			{
				prefixNode = expansionNode;
				expansionNode = null;
			}
			if (prefixNode.Name == "context")
			{
				differenceNode.AppendChild(prefixNode);
				deleteCount++;
			}
			differenceNode.AppendChild(dataNode);
			if (expansionNode != null)
			{
				differenceNode.AppendChild(expansionNode);
				deleteCount++;
			}
			_currentNodes.RemoveRange(_currentNodes.Count - deleteCount, deleteCount);
			_currentNodes.Add(differenceNode);
		}

		private void OnElementWithData(object sender, ActionEventArgs args)
		{
			Debug.Assert(sender is Rule);
			AddXmlNodeWithData(((Rule) sender).ID);
		}

		private void OnDifferenceOperator(object sender, ActionEventArgs args)
		{
			switch (args.Value)
			{
				case "<":
					_currentOperators.Push("p");
					break;
				case "<<":
					_currentOperators.Push("s");
					break;
				case "<<<":
					_currentOperators.Push("t");
					break;
				case "=":
					_currentOperators.Push("i");
					break;
				default:
					Debug.Assert(false, "Unhandled operator");
					break;
			}
		}

		private void AddAttributeForReset(string name, string value)
		{
			XmlNode attr = _dom.CreateAttribute(name);
			attr.Value = value;
			_attributesForReset.Enqueue(attr);
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

		private void OnDataString(object sender, ActionEventArgs args)
		{
			_currentDataObjects.Push(_dom.CreateTextNode(_currentDataString.ToString()));
			_currentDataString = new StringBuilder();
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

		private void OnIndirectPosition(object sender, ActionEventArgs args)
		{
			_currentDataObjects.Push(_dom.CreateElement(_currentIndirectPosition.ToString()));
			_currentIndirectPosition = new StringBuilder();
		}

		private void OnBeforeOption(object sender, ActionEventArgs args)
		{
			Debug.Assert(args.Value.Length == 1);
			Dictionary<string, string> optionMap = new Dictionary<string, string>(3);
			optionMap["1"] = "primary";
			optionMap["2"] = "secondary";
			optionMap["3"] = "tertiary";
			Debug.Assert(optionMap.ContainsKey(args.Value));
			XmlNode attr = _dom.CreateAttribute("before");
			AddAttributeForReset("before", optionMap[args.Value]);
		}

		private void OnTop(object sender, ActionEventArgs args)
		{
			// [top] is deprecated in ICU and not directly allowed in LDML
			// [top] is probably best rendered the same as [before 1] [first tertiary ignorable]
			_currentDataObjects.Push(_dom.CreateElement("first_tertiary_ignorable"));
			AddAttributeForReset("before", "primary");
		}

		private void OnOptionNormal(object sender, ActionEventArgs args)
		{
			Regex regex = new Regex("(\\S+)\\s+(\\S+)");
			Match match = regex.Match(args.Value);
			Debug.Assert(match.Success);
			XmlNode attr = _dom.CreateAttribute(match.Groups[1].Value);
			attr.Value = match.Groups[2].Value;
			_optionAttributes.Add(attr);
		}

		private void OnOptionBackwards(object sender, ActionEventArgs args)
		{
			XmlNode attr = _dom.CreateAttribute("backwards");
			switch (args.Value[args.Value.Length - 1])
			{
				case '1':
					attr.Value = "off";
					break;
				case '2':
					attr.Value = "on";
					break;
				default:
					Debug.Assert(false, "Unhandled backwards option.");
					break;
			}
			_optionAttributes.Add(attr);
		}

		private void OnOptionStrength(object sender, ActionEventArgs args)
		{
			XmlNode attr = _dom.CreateAttribute("strength");
			switch (args.Value[args.Value.Length - 1])
			{
				case '1':
					attr.Value = "primary";
					break;
				case '2':
					attr.Value = "secondary";
					break;
				case '3':
					attr.Value = "tertiary";
					break;
				case '4':
					attr.Value = "quaternary";
					break;
				case '5':
				case 'I':
				case 'i':
					attr.Value = "identical";
					break;
				default:
					Debug.Assert(false, "Unhandled strength option.");
					break;
			}
			_optionAttributes.Add(attr);
		}

		private void OnOptionHiraganaQ(object sender, ActionEventArgs args)
		{
			XmlNode attr = _dom.CreateAttribute("hiraganaQuaternary");
			attr.Value = attr.Value.EndsWith("on") ? "on" : "off";
			_optionAttributes.Add(attr);
		}

		private void OnOptionVariableTop(object sender, ActionEventArgs args)
		{
			XmlNode attr = _dom.CreateAttribute("variableTop");
			XmlNode previousNode = _currentNodes[_currentNodes.Count - 1];
			if (previousNode.Name == "x")
			{
				throw new ApplicationException("[variable top] cannot follow an extended node (prefix, expasion, or both).");
			}
			if (previousNode.InnerText.Length == 0)
			{
				throw new ApplicationException("[variable top] cannot follow an indirect position");
			}
			string unescapedValue = previousNode.InnerText;
			string escapedValue = string.Empty;
			for (int i=0; i < unescapedValue.Length; i++)
			{
				escapedValue += String.Format("u{0:X}", Char.ConvertToUtf32(unescapedValue, i));
				// if we had a surogate, that means that two "char"s were encoding one code point,
				// so skip the next "char" as it was already handled in this iteration
				if (Char.IsSurrogate(unescapedValue, i))
				{
					i++;
				}
			}
			attr.Value = escapedValue;
			_optionAttributes.Add(attr);
		}

		// Use this to optimize successive difference elements into one element
		private void OptimizeNodes()
		{
			// we can optimize primary, secondary, tertiary, and identity elements
			List<string> optimizableElementNames = new List<string>(new string[] { "p", "s", "t", "i" });
			List<XmlNode> currentOptimizableNodeGroup = null;
			List<XmlNode> optimizedNodeList = new List<XmlNode>();

			// first, build a list of lists of nodes we can optimize
			foreach (XmlNode node in _currentNodes)
			{
				// Current node can be part of an optimized group if it is an allowed element
				// AND it only contains one unicode code point.
				bool nodeOptimizable = (optimizableElementNames.Contains(node.Name) &&
					(node.InnerText.Length == 1 ||
					(node.InnerText.Length == 2 && Char.IsSurrogatePair(node.InnerText, 0))));
				// If we have a group of optimizable nodes, but we can't add the current node to that group
				if (currentOptimizableNodeGroup != null && (!nodeOptimizable || currentOptimizableNodeGroup[0].Name != node.Name))
				{
					// optimize the current list and add it, then reset the list
					optimizedNodeList.Add(CreateOptimizedNode(currentOptimizableNodeGroup));
					currentOptimizableNodeGroup = null;
				}
				if (!nodeOptimizable)
				{
					optimizedNodeList.Add(node);
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
			if (currentOptimizableNodeGroup != null)
			{
				optimizedNodeList.Add(CreateOptimizedNode(currentOptimizableNodeGroup));
			}
			_currentNodes = optimizedNodeList;
		}

		private XmlNode CreateOptimizedNode(List<XmlNode> nodeGroup)
		{
			Debug.Assert(nodeGroup != null);
			Debug.Assert(nodeGroup.Count > 0);
			// one node is already optimized
			if (nodeGroup.Count == 1)
			{
				return nodeGroup[0];
			}
			// luckily the optimized names are the same as the unoptimized with 'c' appended
			// so <p> becomes <pc>, <s> to <sc>, et al.
			XmlNode optimizedNode = _dom.CreateElement(nodeGroup[0].Name + "c");
			foreach (XmlNode node in nodeGroup)
			{
				optimizedNode.InnerText += node.InnerText;
			}
			return optimizedNode;
		}

		private void PreserveNonIcuSettings()
		{
			XmlNode oldSettings = _parentNode.SelectSingleNode("settings", _nameSpaceManager);
			if (oldSettings == null)
			{
				return;
			}
			List<string> icuSettings = new List<string>(new string[] {"strength", "alternate", "backwards",
				"normalization", "caseLevel", "caseFirst", "hiraganaQuaternary", "numeric", "variableTop"});
			foreach (XmlAttribute attr in oldSettings.Attributes)
			{
				if (!icuSettings.Contains(attr.Name))
				{
					_optionAttributes.Add(attr.CloneNode(true));
				}
			}
		}

		private void OnIcuRules(object sender, ActionEventArgs args)
		{
			XmlNode afterNode;
			PreserveNonIcuSettings();
			XmlHelpers.RemoveElement(_parentNode, "settings", _nameSpaceManager);
			// add the settings element, if needed.
			if (_optionAttributes.Count > 0)
			{
				XmlNode settings = _dom.CreateElement("settings");
				foreach (XmlNode attr in _optionAttributes)
				{
					settings.Attributes.SetNamedItem(attr);
				}
				// "settings" comes after "base", which should be the first element if it exists
				afterNode = null;
				if (_parentNode.FirstChild != null && _parentNode.FirstChild.Name == "base")
				{
					afterNode = _parentNode.FirstChild;
				}
				_parentNode.InsertAfter(settings, afterNode);
			}
			XmlHelpers.RemoveElement(_parentNode, "rules", _nameSpaceManager);
			XmlNode rulesNode = _dom.CreateElement("rules");
			OptimizeNodes();
			foreach (XmlNode node in _currentNodes)
			{
				rulesNode.AppendChild(node);
			}
			// the "rules" element comes before any "special" elements, which should be the last elements if they exist
			int i;
			for (i = _parentNode.ChildNodes.Count - 1; i >= 0 && _parentNode.ChildNodes[i].Name == "special"; i--);
			afterNode = i < 0 ? null : _parentNode.ChildNodes[i];
			_parentNode.InsertAfter(rulesNode, afterNode);
		}
	}
}
