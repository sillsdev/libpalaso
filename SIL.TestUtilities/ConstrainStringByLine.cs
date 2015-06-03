using System.IO;
using NUnit.Framework.Constraints;

namespace SIL.TestUtilities
{

	///<summary>
	/// Matches the text under test with the given text to match. Evaluating it line by line (including
	/// regular expressions).
	///</summary>
	public class ConstrainStringByLine : Constraint
	{
		private string _actualLine;
		private string _expectedLine;

		///<summary>
		///</summary>
		///<param name="expectedString"></param>
		public static ConstrainStringByLine Matches(string expectedString)
		{
			return new ConstrainStringByLine(expectedString);
		}

		private ConstrainStringByLine(string expectedString)
		{
			ExpectedString = expectedString;
		}

		private string ExpectedString { get; set; }

		public override bool Matches(object actualObject)
		{
			bool result = true;
			string actualString = actualObject as string;
			if (actualString == null)
			{
				return false;
			}
			using (var actualReader = new StringReader(actualString))
			{
				using (var expectedReader = new StringReader(ExpectedString))
				{
					while ((_actualLine = actualReader.ReadLine()) != null)
					{
						_expectedLine = expectedReader.ReadLine();
						if (_expectedLine == null)
						{
							result = false;
						}
						else if (_actualLine != _expectedLine)
						{
							if (_expectedLine.Contains("*"))
							{
								var regEx = new System.Text.RegularExpressions.Regex(_expectedLine);
								result = regEx.IsMatch(_actualLine);
							}
							else
							{
								result = false;
							}
						}
						if (!result)
						{
							break;
						}

					}
				}
			}
			return result;
		}

		public override void WriteDescriptionTo(MessageWriter writer)
		{
		}

		public override void WriteMessageTo(MessageWriter writer)
		{
			if (_expectedLine == null)
			{
				writer.WriteMessageLine("Expected end of string (null), actual was '{0}'", _actualLine);
				return;
			}
			if (_actualLine == null)
			{
				writer.WriteMessageLine("Expected string '{0}', actual was at end (null)", _expectedLine);
				return;
			}
			writer.DisplayStringDifferences(_expectedLine, _actualLine, 0, false, true);
		}
	}
}
