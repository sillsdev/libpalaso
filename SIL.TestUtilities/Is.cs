// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework.Constraints;
using SIL.ObjectModel;

namespace SIL.TestUtilities.Extensions
{
	/// <summary>
	/// Some extensions to NUnits constraints. To use this class add the following line to
	/// the top of your test class:
	/// <c>using Is = SIL.TestUtilities.Extensions.Is;</c>
	/// </summary>
	public class Is : NUnit.Framework.Is
	{
		public static ValueEquatableConstraint<T> ValueEqualTo<T>(IValueEquatable<T> expected)
			where T : class
		{
			return new ValueEquatableConstraint<T>(expected);
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