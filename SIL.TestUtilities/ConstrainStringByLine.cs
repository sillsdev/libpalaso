// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

// This file is a copy (with different namespace and minus the public class Is) of SIL.BuildTasks.Tests\ConstrainStringByLine.cs
// These two files should be kept in synch (to the extent that their assemblies are using compatible
// versions of NUnit.

using System.IO;
using NUnit.Framework.Constraints;

namespace SIL.TestUtilities
{
	public static class ConstrainStringByLineExtensions
	{
		public static ConstrainStringByLine MultilineString(this ConstraintExpression expression, string expected)
		{
			var constraint = new ConstrainStringByLine(expected);
			expression.Append(constraint);
			return constraint;
		}
	}

	/// <inheritdoc />
	/// <summary>
	///  Matches the text under test with the given text to match. Evaluating it line by line (including
	///  regular expressions).
	/// </summary>
	public class ConstrainStringByLine : Constraint
	{
		private string _actualLine;
		private string _expectedLine;

		public ConstrainStringByLine(string expectedString)
		{
			ExpectedString = expectedString;
		}

		private string ExpectedString { get; }

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			var actualString = actual as string;
			if (string.IsNullOrEmpty(actualString) || string.IsNullOrEmpty(ExpectedString))
			{
				return new ConstraintResult(this, actual, ConstraintStatus.Failure);
			}
			using (var actualReader = new StringReader(actualString))
			{
				using (var expectedReader = new StringReader(ExpectedString))
				{
					while ((_actualLine = actualReader.ReadLine()) != null)
					{
						_expectedLine = expectedReader.ReadLine();
						if (_expectedLine == null)
							return new ConstraintResult(this, _actualLine, ConstraintStatus.Failure);

						if (_actualLine == _expectedLine)
							continue;

						if (_expectedLine.Contains("*"))
						{
							var regEx = new System.Text.RegularExpressions.Regex(_expectedLine);
							if (regEx.IsMatch(_actualLine))
								continue;
						}

						return new ConstraintResult(this, _actualLine, ConstraintStatus.Failure);
					}

					_expectedLine = expectedReader.ReadLine();
					if (_expectedLine != null)
						return new ConstraintResult(this, _actualLine, ConstraintStatus.Failure);
				}
			}
			return new ConstraintResult(this, GetStringRepresentation(actualString), ConstraintStatus.Success);
		}

		private static string GetStringRepresentation(string value)
		{
			return string.Join("\\n", value?.Split('\n'));
		}

		public override string Description => _expectedLine != null ? $"\"{_expectedLine}\"" : "end of string (null)";

		/*
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
		*/
	}
}
