using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Spart;
using Spart.Actions;
using Spart.Parsers;
using Spart.Parsers.NonTerminal;
using Spart.Parsers.Primitives;
using Spart.Scanners;
using Debugger = Spart.Debug.Debugger;

namespace SIL.WritingSystems
{
	public class SimpleRulesParser
	{
		private readonly Rule _collationElement;
		private readonly Rule _collationGroup;
		private readonly Rule _collationLine;
		private readonly Rule _collationRules;
		private bool _inCollationGroup;

		private StringBuilder _currentCollationElement;
		private Queue<string> _currentCollationElements;
		private Queue<string> _currentCollationGroups;
		private Queue<string> _currentCollationLines;

		private List<string> _usedCollationElements;

		private string _result;

		/// <summary>
		/// Check whether the given character should be recognized
		/// </summary>
		/// <param name="c">the character to try to recognize</param>
		/// <returns>true if recognized, false if not</returns>
		private static bool IsCharacter(char c)
		{
			switch (c)
			{
				case ' ':
				case '\t':
				case '(':
				case ')':
				case '\\':
				case '\n':
				case '\r':
					return false;
				default:
					return true;
			}
		}

		/// <summary>
		/// A very simple calculator parser
		/// </summary>
		public SimpleRulesParser()
			: this(false)
		{
		}

		public SimpleRulesParser(bool useDebugger)
		{
			// creating sub parsers
			Parser character = new CharParser(IsCharacter);
			Parser hexDigit = Prims.HexDigit;
			Parser newLine = Prims.Eol;
			Parser whiteSpace = Ops.ZeroOrMore(Prims.WhiteSpace - Prims.Eol);

			Parser unicodeEscapeCharacter = Ops.Sequence('\\',
														 Ops.Expect("scr0001",
																	"Invalid unicode character escape sequence: missing 'u' after '\\'",
																	'u'),
														 Ops.Expect("scr0002",
																	"Invalid unicode character escape sequence: missing hexadecimal digit after '\\u'",
																	hexDigit),
														 Ops.Expect("scr0002",
																	"Invalid unicode character escape sequence: missing hexadecimal digit after '\\u'",
																	hexDigit),
														 Ops.Expect("scr0002",
																	"Invalid unicode character escape sequence: missing hexadecimal digit after '\\u'",
																	hexDigit),
														 Ops.Expect("scr0002",
																	"Invalid unicode character escape sequence: missing hexadecimal digit after '\\u'",
																	hexDigit));

			// creating rules and assigning names (for debugging)
			_collationElement = new Rule("collationElement");
			_collationGroup = new Rule("collationGroup");
			_collationLine = new Rule("collationLine");
			_collationRules = new Rule("collationRules");

			// assigning parsers to rules
			// collationElement ::= (unicodeEscapeCharacter | character)+
			_collationElement.Parser = Ops.Expect(CollationElementIsUnique, "scr0100", "Duplicate collation element",
												  Ops.OneOrMore(unicodeEscapeCharacter | character));

			// collationGroup ::= '(' WS* collationElement WS+ (collationElement WS?)+ ')'
			_collationGroup.Parser = Ops.Sequence('(',
												  whiteSpace,
												  Ops.Expect("scr0003",
															 "Expected: 2 or more collation elements in collation group (nested collation groups are not allowed)",
															 Ops.Sequence(_collationElement,
																		  whiteSpace,
																		  _collationElement,
																		  whiteSpace)),
												  Ops.Expect("scr0004",
															 "Expected: collation element or close group ')'",
															 Ops.ZeroOrMore(
																	 Ops.Sequence(_collationElement, whiteSpace))),
												  Ops.Expect("scr0005", "Expected: group close ')'", ')'));

			// collationLine ::= (collationElement WS? | collationGroup WS?)+
			_collationLine.Parser = Ops.OneOrMore(Ops.Sequence(_collationGroup | _collationElement,
															   whiteSpace));

			// collationRules ::= WS? collationLine? (NewLine WS? collationLine?)* EOF
			_collationRules.Parser = Ops.Expect("scr0006",
												"Invalid character",
												Ops.Sequence(whiteSpace,
															 Ops.Optional(_collationLine),
															 Ops.ZeroOrMore(Ops.Sequence(newLine,
																						 whiteSpace,
																						 Ops.Optional(_collationLine))),
															 Prims.End));

			character.Act += OnCharacter;
			unicodeEscapeCharacter.Act += OnUnicodeEscapeCharacter;
			_collationElement.Act += OnCollationElement;
			_collationGroup.Act += OnCollationGroup;
			_collationGroup.PreParse += OnEnterCollationGroup;
			_collationGroup.PostParse += OnExitCollationGroup;
			_collationLine.Act += OnCollationLine;
			_collationRules.Act += OnCollationRules;
			if (useDebugger)
			{
				// debuggger
				var debug = new Debugger(Console.Out);
				debug += _collationRules;
				debug += _collationLine;
				debug += _collationGroup;
				debug += _collationElement;
			}
		}

		private void OnExitCollationGroup(object sender, PostParseEventArgs args)
		{
			_inCollationGroup = false;
		}

		private void OnEnterCollationGroup(object sender, PreParseEventArgs args)
		{
			_inCollationGroup = true;
		}

		/// <summary>
		/// Parse a string and return parse match
		/// </summary>
		/// <param name="rules"></param>
		/// <returns></returns>
		public string ConvertToIcuRules(String rules)
		{
			_result = string.Empty;
			_currentCollationElement = new StringBuilder();
			_currentCollationLines = new Queue<string>();
			_currentCollationGroups = new Queue<string>();
			_currentCollationElements = new Queue<string>();
			_usedCollationElements = new List<string>();

			var sc = new StringScanner(rules);
			ParserMatch match = _collationRules.Parse(sc);
			Debug.Assert(match.Success);
			Debug.Assert(sc.AtEnd);
			return _result;
		}

		public bool ValidateSimpleRules(string rules, out string message)
		{
			_currentCollationElement = new StringBuilder();
			_currentCollationLines = new Queue<string>();
			_currentCollationGroups = new Queue<string>();
			_currentCollationElements = new Queue<string>();
			_usedCollationElements = new List<string>();

			var sc = new StringScanner(rules);
			message = null;
			try
			{
				ParserMatch match = _collationRules.Parse(sc);
				if (!match.Success || !sc.AtEnd)
				{
					message = "Invalid simple rules.";
					return false;
				}
			}
			catch (ParserErrorException e)
			{
				string[] lines = sc.InputString.Split(new char[] {'\n'});
				if (e.ParserError.Line > 0 && e.ParserError.Line <= lines.Length)
				{
					string errString = lines[e.ParserError.Line - 1];
					int startingPos = Math.Max((int) e.ParserError.Column - 2, 0);
					errString = errString.Substring(startingPos, Math.Min(10, errString.Length - startingPos));
					message = String.Format("{0}: '{1}'", e.ParserError.ErrorText, errString);
				}
				else
					message = e.ParserError.ErrorText;

				return false;
			}
			catch (Exception e)
			{
				message = e.Message;
				return false;
			}
			return true;
		}

		#region Semantic Actions

		// ICU tailoring
		//http://www.icu-project.org/userguide/Collate_Customization.html
		//
		// Most of the characters can be used as parts of rules.
		// However, whitespace characters will be skipped over,
		// and all ASCII characters that are not digits or letters
		// are considered to be part of syntax. In order to use
		// these characters in rules, they need to be escaped.
		// Escaping can be done in several ways:
		// * Single characters can be escaped using backslash \ (U+005C).
		// * Strings can be escaped by putting them between single quotes 'like this'.
		// * Single quote can be quoted using two single quotes ''.
		private static string IcuEscape(string s)
		{
			var result = new StringBuilder(s);
			foreach (char c in s)
			{
				if (c <= 0x7f) // ASCII Character
				{
					if (!Char.IsLetterOrDigit(c))
					{
						result.Replace(c.ToString(), "\\" + c);
					}
				}
			}
			return result.ToString();
		}

		private void OnUnicodeEscapeCharacter(object sender, ActionEventArgs args)
		{
			Debug.Assert(args.Value.Length == 6);
			ushort unicodeCharacterValue = UInt16.Parse(args.Value.Substring(2), NumberStyles.AllowHexSpecifier);
			// is it a surrogate?
			string s;
			if (0xD800 <= unicodeCharacterValue && unicodeCharacterValue <= 0xdfff)
			{
				s = args.Value; // just keep the unicode escape character and let icu handle the error
				// if there isn't a valid one following
			}
			else
			{
				s = Char.ConvertFromUtf32(unicodeCharacterValue);
				s = IcuEscape(s);
			}
			_currentCollationElement.Append(s);
		}

		private void OnCharacter(object sender, ActionEventArgs args)
		{
			_currentCollationElement.Append(IcuEscape(args.Value));
		}

		private bool CollationElementIsUnique(ParserMatch match)
		{
			// we don't want to have an error if it didn't match
			// only if it matched but we have already declared this collation element
			if (match.Success)
			{
				string collationElement = _currentCollationElement.ToString();
				if (_usedCollationElements.Contains(collationElement))
				{
					return false;
				}
			}
			return true;
		}
		private void OnCollationElement(object sender, ActionEventArgs args)
		{
			string collationElement = _currentCollationElement.ToString();
			_usedCollationElements.Add(collationElement);
			if (_inCollationGroup)
			{
				_currentCollationElements.Enqueue(collationElement);
			}
			else
			{
				_currentCollationGroups.Enqueue(collationElement);
			}
			_currentCollationElement = new StringBuilder();
		}

		private void OnCollationGroup(object sender, ActionEventArgs args)
		{
			var sb = new StringBuilder();
			while (_currentCollationElements.Count != 0)
			{
				sb.Append(_currentCollationElements.Dequeue());
				if (_currentCollationElements.Count > 0)
				{
					sb.Append(" <<< "); // tertiary distinction
				}
			}

			_currentCollationGroups.Enqueue(sb.ToString());
		}

		private void OnCollationLine(object sender, ActionEventArgs args)
		{
			var sb = new StringBuilder();
			while (_currentCollationGroups.Count != 0)
			{
				sb.Append(_currentCollationGroups.Dequeue());
				if (_currentCollationGroups.Count > 0)
				{
					sb.Append(" << "); // secondary distinction
				}
			}

			_currentCollationLines.Enqueue(sb.ToString());
		}

		private void OnCollationRules(object sender, ActionEventArgs args)
		{
			var sb = new StringBuilder();
			if (_currentCollationLines.Count > 0)
			{
				sb.Append("&[before 1] [first regular] < ");
			}
			while (_currentCollationLines.Count != 0)
			{
				sb.Append(_currentCollationLines.Dequeue());
				if (_currentCollationLines.Count > 0)
				{
					sb.Append(" < "); // primary distinction
				}
			}

			_result = sb.ToString();
		}

		#endregion
	}
}
