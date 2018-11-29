// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework.Constraints;
using SIL.ObjectModel;

namespace SIL.TestUtilities.NUnitExtensions
{
	public class ValueEquatableConstraint<T>: Constraint where T: class
	{
		private IValueEquatable<T> _expected;

		public ValueEquatableConstraint(IValueEquatable<T> expected): base(expected)
		{
			_expected = expected;
		}

		public override bool Matches(object actual)
		{
			this.actual = actual;
			return _expected.ValueEquals(actual as T);
		}

		public override void WriteDescriptionTo(MessageWriter writer)
		{
			writer.WriteExpectedValue(_expected);
		}
	}

	public static class ValueEquatableConstraintExtensionMethods
	{
		public static ValueEquatableConstraint<T> ValueEqualTo<T>(
			this ConstraintExpression expression, IValueEquatable<T> expected) where T: class
		{
			var constraint = new ValueEquatableConstraint<T>(expected);
			expression.Append(constraint);
			return constraint;
		}
	}

}