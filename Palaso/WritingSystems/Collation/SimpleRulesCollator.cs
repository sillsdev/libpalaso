using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Icu.Collation;
using Spart.Actions;
using Spart.Parsers;
using Spart.Parsers.NonTerminal;
using Spart.Parsers.Primitives;
using Spart.Scanners;
using Debugger=Spart.Debug.Debugger;

namespace Palaso.WritingSystems.Collation
{
	public class SimpleRulesCollator : ICollator
	{
		private RuleBasedCollator _collator;

		public SimpleRulesCollator(string rules)
		{
			string icuRules = ConvertToIcuRules(rules);
			try
			{
				_collator = new RuleBasedCollator(icuRules);
			}
			catch (DllNotFoundException e)
			{
				throw new DllNotFoundException("Currently SimpleRulesCollator uses Icu and thus requires the ICU dlls to be present", e);
			}
		}

		static public string ConvertToIcuRules(string rules)
		{
			SimpleCollationRuleParser ruleConverter = new SimpleCollationRuleParser();
			return ruleConverter.ConvertToIcuTailoringRule(rules);
		}

		public SortKey GetSortKey(string source)
		{
			return _collator.GetSortKey(source);
		}

		///<summary>Compares two strings and returns a value indicating whether one is less than,
		/// equal to, or greater than the other.</summary>
		///
		///<returns>Less than zero when string1 is less than string2.
		///  Zero when string1 equals string2.
		///  Greater than zero when string1 is greater than string2.
		///</returns>
		///
		///<param name="string1">The first string to compare.</param>
		///<param name="string2">The second object to compare.</param>
		public int Compare(string string1, string string2)
		{
			return _collator.Compare(string1, string2);
		}

		private class SimpleCollationRuleParser
		{
			private Rule _collationElement;
			private Rule _collationGroup;
			private Rule _collationLine;
			private Rule _collationRules;

			private Debugger debug;

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
			public SimpleCollationRuleParser() : this(false) {}

			public SimpleCollationRuleParser(bool useDebugger)
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

				character.Act += new ActionHandler(OnCharacter);
				unicodeEscapeCharacter.Act += OnUnicodeEscapeCharacter;
				_collationElement.Act += new ActionHandler(OnCollationElement);
				_collationGroup.Act += new ActionHandler(OnCollationGroup);
				_collationGroup.PreParse += new PreParseEventHandler(OnEnterCollationGroup);
				_collationGroup.PostParse += new PostParseEventHandler(OnExitCollationGroup);
				_collationLine.Act += new ActionHandler(OnCollationLine);
				_collationRules.Act += new ActionHandler(OnCollationRules);
				if (useDebugger)
				{
					// debuggger
					debug = new Debugger(Console.Out);
					debug += _collationRules;
					debug += _collationLine;
					debug += _collationGroup;
					debug += _collationElement;
				}
			}

			private void OnExitCollationGroup(object sender, PostParseEventArgs args)
			{
				inCollationGroup = false;
			}

			private bool inCollationGroup = false;

			private void OnEnterCollationGroup(object sender, PreParseEventArgs args)
			{
				inCollationGroup = true;
			}

			/// <summary>
			/// Parse a string and return parse match
			/// </summary>
			/// <param name="s"></param>
			/// <returns></returns>
			public string ConvertToIcuTailoringRule(String s)
			{
				result = string.Empty;
				_currentCollationElement = new StringBuilder();
				_currentCollationLines = new Queue<string>();
				_currentCollationGroups = new Queue<string>();
				_currentCollationElements = new Queue<string>();
				_usedCollationElements = new List<string>();

				StringScanner sc = new StringScanner(s);
				ParserMatch match = _collationRules.Parse(sc);
				Debug.Assert(match.Success);
				Debug.Assert(sc.AtEnd);
				return result;
			}

			private StringBuilder _currentCollationElement;
			private Queue<string> _currentCollationElements;
			private Queue<string> _currentCollationGroups;
			private Queue<string> _currentCollationLines;

			private List<string> _usedCollationElements;

			private string result;

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
				StringBuilder result = new StringBuilder(s);
				foreach (char c in s)
				{
					if (c <= 0x7f) // ASCII Character
					{
						if (!Char.IsLetterOrDigit(c))
						{
							result.Replace(c.ToString(), "\\u" + ((int) c).ToString("X4"));
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
				if (inCollationGroup)
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
				StringBuilder sb = new StringBuilder();
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
				StringBuilder sb = new StringBuilder();
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
				StringBuilder sb = new StringBuilder();
				if (_currentCollationLines.Count > 0)
				{
					sb.Append("&[before 1] first regular < ");
				}
				while (_currentCollationLines.Count != 0)
				{
					sb.Append(_currentCollationLines.Dequeue());
					if (_currentCollationLines.Count > 0)
					{
						sb.Append(" < "); // primary distinction
					}
				}

				result = sb.ToString();
			}

			#endregion
		}
	}
}