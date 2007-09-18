using System;

namespace Spart.Demo
{
	using Spart.Parsers.NonTerminal;
	using Spart.Parsers;
	using Spart.Scanners;
	using Spart.Actions;
	using Spart.Debug;

	public class Calculator
	{
		Rule group;
		Rule term;
		Rule factor;
		Rule expression;
		Rule integer;
		Debugger debug;

		/// <summary>
		/// A very simple calculator parser
		/// </summary>
		public Calculator()
		{
			// creating rules and assigning names (for debugging)
			group = new Rule(); group.ID = "group";
			term = new Rule(); term.ID ="term";
			factor = new Rule(); factor.ID="factor";
			expression = new Rule(); expression.ID="expression";
			integer  = new Rule(); integer.ID="integer";

			// debuggger
			debug = new Debugger(Console.Out);
			debug += factor;
			debug+=term;
			debug+=group;
			debug += expression;
			debug += integer;

			// creating sub parsers
			Parser add = Ops.Seq('+',term);
			// attaching semantic action
			add.Act += new ActionHandler(this.Add);
			Parser sub = Ops.Seq('-',term);
			sub.Act += new ActionHandler(this.Sub);
			Parser mult = Ops.Seq('*',factor);
			mult.Act += new ActionHandler(this.Mult);
			Parser div = Ops.Seq('/',factor);
			div.Act += new ActionHandler(this.Div);

			// assigning parsers to rules
			integer.Parser = Prims.Digit;

			group.Parser       = Ops.Seq('(',Ops.Seq(expression,')'));
			factor.Parser      = group | integer;
			term.Parser        = Ops.Seq( factor, Ops.Klenee(mult	| div ));
			expression.Parser  = Ops.Seq(term,Ops.Klenee(add | mult ));
		}

		/// <summary>
		/// Parse a string and return parse match
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public ParserMatch Parse(String s)
		{
			StringScanner sc = new StringScanner(s);
			return expression.Parse(sc);
		}

		#region Semantic Actions
		public void Add(Object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("add");
		}


		public void Sub(Object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("sub");
		}


		public void Mult(Object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("mult");
		}

		public void Div(Object sender, ActionEventArgs args)
		{
			Console.Out.WriteLine("div");
		}
		#endregion
	}
}
