using NUnit.Framework;
using Spart.Actions;
using Spart.Parsers;
using Spart.Parsers.Composite;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace Palaso.Tests.Spart.Parsers.Composite
{
	[TestFixture]
	public class DifferenceParserTests
	{
		private CharParser _letterOrDigit;
		private CharParser _letter;
		private CharParser _symbol;
		private DifferenceParser _LetterOrDigitButNotLetterDifferenceParser;
		private bool _symbolParserSuccess;
		private bool _letterParserSuccess;
		private bool _letterOrDigitParserSuccess;
		private bool _differenceParserSuccess;

		[SetUp]
		public void Setup() {
			this._letterOrDigit = Prims.LetterOrDigit;
			this._letterOrDigit.Act += OnLetterOrDigitParserSuccess;
			this._letter = Prims.Letter;
			this._letter.Act += OnLetterParserSuccess;
			this._symbol = Prims.Symbol;
			this._symbol.Act += OnSymbolParserSuccess;

			this._LetterOrDigitButNotLetterDifferenceParser = new DifferenceParser(this._letterOrDigit, this._letter);
			this._LetterOrDigitButNotLetterDifferenceParser.Act += OnDifferenceParserSuccess;

			this._symbolParserSuccess = false;
			this._letterParserSuccess = false;
			this._letterOrDigitParserSuccess = false;
			this._differenceParserSuccess = false;

		}

		private void OnSymbolParserSuccess(object sender, ActionEventArgs args)
		{
			this._symbolParserSuccess = true;
		}

		private void OnLetterParserSuccess(object sender, ActionEventArgs args)
		{
			this._letterParserSuccess = true;
		}

		void OnLetterOrDigitParserSuccess(object sender, ActionEventArgs args)
		{
			this._letterOrDigitParserSuccess = true;
		}

		private void OnDifferenceParserSuccess(object sender, ActionEventArgs args)
		{
			this._differenceParserSuccess = true;
		}


		[Test]
		public void DifferenceParser()
		{
			DifferenceParser parser = new DifferenceParser(this._letterOrDigit, this._symbol);
			Assert.AreEqual(this._letterOrDigit, parser.FirstParser);
			Assert.AreEqual(this._symbol, parser.SecondParser);
		}


		[Test]
		public void InputMatchesFirstParserDoesNotMatchSecondParser_Success()
		{
			IScanner digit = new StringScanner("1");
			Assert.IsTrue(this._LetterOrDigitButNotLetterDifferenceParser.Parse(digit).Success);
		}

		[Test]
		public void InputMatchesFirstParserAndSecondParser_SameLength_Fail()
		{
			IScanner letter = new StringScanner("abc");
			Assert.IsFalse(this._LetterOrDigitButNotLetterDifferenceParser.Parse(letter).Success);
		}

		[Test]
		public void InputMatchesFirstParserAndSecondParser_SecondParserLonger_Fail()
		{
			IScanner letter = new StringScanner("abc");
			DifferenceParser parser = new DifferenceParser(this._letterOrDigit,
											Ops.Sequence(this._letterOrDigit, this._letterOrDigit));

			Assert.IsFalse(parser.Parse(letter).Success);
		}

		[Test]
		public void InputMatchesFirstParserAndSecondParser_SecondParserShorter_Success()
		{
			IScanner letter = new StringScanner("abc");
			DifferenceParser parser = new DifferenceParser(Ops.Sequence(this._letterOrDigit, this._letterOrDigit),
																this._letterOrDigit);
			Assert.IsTrue(parser.Parse(letter).Success);
		}

		[Test]
		public void InputDoesNotMatchFirstParserButMatchesSecondParser_Fail()
		{
			IScanner symbol = new StringScanner("+");
			DifferenceParser parser = new DifferenceParser(this._letterOrDigit, this._symbol);
			parser.Act += OnDifferenceParserSuccess;
			Assert.IsFalse(parser.Parse(symbol).Success);
		}

		[Test]
		public void InputDoesNotMatchFirstParserAndDoesNotMatchSecondParser_Fail()
		{
			IScanner symbol = new StringScanner("+");
			Assert.IsFalse(this._LetterOrDigitButNotLetterDifferenceParser.Parse(symbol).Success);
		}



		[Test]
		public void InputMatchesFirstParserDoesNotMatchSecondParser_Actions_LetterOrDigitAndDiff()
		{
			IScanner digit = new StringScanner("1");
			this._LetterOrDigitButNotLetterDifferenceParser.Parse(digit);
			Assert.IsTrue(_differenceParserSuccess);
			Assert.IsTrue(_letterOrDigitParserSuccess);
			Assert.IsFalse(_letterParserSuccess);
		}

		[Test]
		public void InputMatchesFirstParserAndSecondParser_Actions_None()
		{
			IScanner letter = new StringScanner("a");
			this._LetterOrDigitButNotLetterDifferenceParser.Parse(letter);
			Assert.IsFalse(_differenceParserSuccess);
			Assert.IsFalse(_letterOrDigitParserSuccess); // should not be called since it is ultimately not accepted
			Assert.IsFalse(_letterParserSuccess); // should not be called since the difference ultimately not accepted
		}

		[Test]
		public void InputDoesNotMatchFirstParserButMatchesSecondParser_Actions_None()
		{
			IScanner symbol = new StringScanner("+");
			DifferenceParser parser = new DifferenceParser(this._letterOrDigit, this._symbol);
			parser.Act += OnDifferenceParserSuccess;
			parser.Parse(symbol);
			Assert.IsFalse(_differenceParserSuccess);
			Assert.IsFalse(_letterOrDigitParserSuccess);
			Assert.IsFalse(_symbolParserSuccess); // should never call since first parser doesn't match
		}

		[Test]
		public void InputDoesNotMatchFirstParserAndDoesNotMatchSecondParser_Actions_None()
		{
			IScanner symbol = new StringScanner("+");
			this._LetterOrDigitButNotLetterDifferenceParser.Parse(symbol);
			Assert.IsFalse(_differenceParserSuccess);
			Assert.IsFalse(_letterOrDigitParserSuccess);
			Assert.IsFalse(_letterParserSuccess);

		}

		[Test]
		public void InputMatchesFirstParserDoesNotMatchSecondParser_ScanPosition_ForwardByMatchLength()
		{
			IScanner digit = new StringScanner("1 hello");
			ParserMatch match = this._LetterOrDigitButNotLetterDifferenceParser.Parse(digit);
			Assert.AreEqual(1, match.Length);
			Assert.AreEqual(1, digit.Offset);
		}

		[Test]
		public void InputMatchesFirstParserAndSecondParser_ScanPosition_NoChange()
		{
			IScanner letter = new StringScanner("a hello");
			this._LetterOrDigitButNotLetterDifferenceParser.Parse(letter);
			Assert.AreEqual(0, letter.Offset);
		}

		[Test]
		public void InputDoesNotMatchFirstParserButMatchesSecondParser_ScanPosition_NoChange()
		{
			IScanner symbol = new StringScanner("+hello");
			DifferenceParser parser = new DifferenceParser(this._letterOrDigit, this._symbol);
			parser.Parse(symbol);
			Assert.AreEqual(0, symbol.Offset);
		}

		[Test]
		public void InputDoesNotMatchFirstParserAndDoesNotMatchSecondParser_ScanPosition_NoChange()
		{
			IScanner symbol = new StringScanner("+hello");
			this._LetterOrDigitButNotLetterDifferenceParser.Parse(symbol);
			Assert.AreEqual(0, symbol.Offset);
		}

	}

}