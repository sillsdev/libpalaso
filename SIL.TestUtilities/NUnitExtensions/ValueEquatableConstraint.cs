// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework.Constraints;
using SIL.ObjectModel;

namespace SIL.TestUtilities.NUnitExtensions
{
	public class ValueEquatableConstraint<T>: Constraint where T: class
	{
		private readonly IValueEquatable<T> _expected;

		public ValueEquatableConstraint(IValueEquatable<T> expected): base(expected)
		{
			_expected = expected;
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			return new ConstraintResult(this, actual,
				_expected.ValueEquals(actual as T) ? ConstraintStatus.Success : ConstraintStatus.Failure);
		}

		public override string Description => _expected.ToString();
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