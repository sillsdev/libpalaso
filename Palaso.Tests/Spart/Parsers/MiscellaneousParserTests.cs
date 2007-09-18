using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spart.Debug;
using Spart.Parsers;
using Spart.Parsers.NonTerminal;
using Spart.Scanners;

namespace Spart.Tests
{
	[TestFixture]
	public class MiscellaneousParserTests
	{
		[SetUp]
		public void Setup()
		{

		}

		[TearDown]
		public void TearDown()
		{

		}

		[Test]
		public void pbWithKleene() // http://www.codeproject.com/csharp/spart.asp?df=100&forumid=30315&select=1812016#xx1812016xx
		{
			Rule opQt = new Rule();
			Rule cnd_qt = new Rule();
			//([aA-zZ0-9 ]+)
			opQt.Parser = Ops.OneOrMore(Ops.Choice(Prims.LetterOrDigit, ' '));


			//CND_QT -> [OPQT] *(
			cnd_qt.Parser = Ops.Sequence(
			'[',
			opQt,
			']',
			Ops.ZeroOrMore(' '),
			'('
			);

			Assert.IsTrue(cnd_qt.Parse(new StringScanner("[all] (")).Success);
			Assert.IsTrue(cnd_qt.Parse(new StringScanner("[all](")).Success);
		}

		[Test]
		public void List() //http://www.codeproject.com/csharp/spart.asp?df=100&forumid=30315&select=797847#xx797847xx
		{
			Parser real = Ops.Sequence(Ops.OneOrMore(Prims.Digit),
									   Ops.Optional(Ops.Sequence('.',
									   Ops.OneOrMore(Prims.Digit))));
			Rule numList = new Rule();
			numList.Parser = Ops.Sequence(
			real,
			Ops.ZeroOrMore(Ops.Sequence(Prims.Ch(','), real)),
			Prims.End);

			Assert.IsTrue(numList.Parse(new StringScanner("100")).Success); // THROWS INDEX EXCEPTION

			Assert.IsFalse(numList.Parse(new StringScanner("88,d,88,9.090,")).Success); // PARSES SUCCESSFULLY!

			Assert.IsFalse(numList.Parse(new StringScanner("88,88,9.090,")).Success); // PARSES SUCCESSFULLY!
		}

		[Test]
		public void KleeneStar() //http://www.codeproject.com/csharp/spart.asp?df=100&forumid=30315&select=797678#xx797678xx
		{
			Rule integer = new Rule("integer");
			Rule number = new Rule("number");
			Rule group = new Rule("group");
			Rule term = new Rule("term");
			Rule expression = new Rule("expression");
			Parser add = Ops.Sequence('+', term);

			integer.Parser = Ops.Sequence(Prims.Digit, Ops.ZeroOrMore(Prims.Digit));
			number.Parser = Ops.Sequence(Ops.Optional(integer), Prims.Ch('.'), integer);
			group.Parser = Ops.Sequence('(', expression, ')');
			term.Parser = group | number | integer;
			expression.Parser = Ops.Sequence(term, Ops.ZeroOrMore(add));

			Assert.IsTrue(expression.Parse(new StringScanner("(.99+100)")).Success);
			Assert.IsTrue(expression.Parse(new StringScanner("(5.99+100)")).Success);
		}

	}

}