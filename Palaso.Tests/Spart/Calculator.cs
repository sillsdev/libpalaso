using System;
using System.Collections.Generic;
using Spart.Actions;
using Spart.Debug;
using Spart.Parsers;
using Spart.Parsers.NonTerminal;
using Spart.Scanners;

namespace Palaso.Tests.Spart
{
	public class Calculator
	{
		private Rule group;
		private Rule term;
		private Rule factor;
		private Rule expression;
		private Rule integer;
		private Debugger debug;

		/// <summary>
		/// A very simple calculator parser
		/// </summary>
		public Calculator()
		{
			// creating rules and assigning names (for debugging)
			group = new Rule("Group");
			term = new Rule("Term");
			factor = new Rule("Factor");
			expression = new Rule("Expression");
			integer = new Rule("Integer", Prims.Digit) [OnInteger];

			// creating sub parsers
			Parser add = Ops.Sequence('+', term);
			// attaching semantic action
			add.Act += OnAdd;

			// creating sub parsers and attaching semantic action in one swoop
			Parser sub = Ops.Sequence('-', term)[OnSub];
			Parser mult = Ops.Sequence('*', factor)[OnMult];
			Parser div = Ops.Sequence('/', factor)[OnDiv];

			// assigning parsers to rules
			group.Parser = Ops.Sequence('(', expression, ')');
			factor.Parser = group | integer;
			term.Parser = Ops.Sequence(factor, Ops.ZeroOrMore(mult | div));
			expression.Parser = Ops.Sequence(term, Ops.ZeroOrMore(add | sub)) [OnExpression];

			// debuggger
			debug = new Debugger(Console.Out);
			debug += factor;
			debug += term;
			debug += group;
			debug += expression;
			debug += integer;
		}

		/// <summary>
		/// Parse a string and return parse match
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public ParserMatch Parse(String s)
		{
			StringScanner sc = new StringScanner(s);
			ParserMatch match = this.expression.Parse(sc);
			if(!sc.AtEnd)
			{
				return ParserMatch.CreateFailureMatch(sc,0);
			}
			return match;
		}

		#region Semantic Actions

		private Stack<int> numberStack = new Stack<int>();

		private void OnAdd(Object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("add");
			int second = numberStack.Pop();
			int first = numberStack.Pop();
			numberStack.Push(first + second);
		}

		private void OnSub(Object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("sub");
			int second = numberStack.Pop();
			int first = numberStack.Pop();
			numberStack.Push(first - second);
		}

		private void OnMult(Object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("mult");
			int second = numberStack.Pop();
			int first = numberStack.Pop();
			numberStack.Push(first * second);
		}

		private void OnDiv(Object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("div");
			int second = numberStack.Pop();
			int first = numberStack.Pop();
			numberStack.Push(first / second);
		}

		private void OnExpression(object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("expression: {0} = {1}", args.Value, numberStack.Peek());
		}

		private void OnInteger(object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("integer: {0}", args.Value);
			numberStack.Push(int.Parse(args.Value));
		}


		#endregion
	}
}