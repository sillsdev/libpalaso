// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
	// TODO: Remove this class when we start using NUnit 3

	public static class Does
	{
		#region Not

		/// <summary>
		/// Returns a ConstraintExpression that negates any
		/// following constraint.
		/// </summary>
		public static ConstraintExpression Not => new ConstraintExpression().Not;

		#endregion

		#region Contain

		/// <summary>
		/// Returns a new <see cref="SomeItemsConstraint"/> checking for the
		/// presence of a particular object in the collection.
		/// </summary>
		public static SomeItemsConstraint Contain(object expected) =>
			new SomeItemsConstraint(new EqualConstraint(expected));

		/// <summary>
		/// Returns a new <see cref="ContainsConstraint"/>. This constraint
		/// will, in turn, make use of the appropriate second-level
		/// constraint, depending on the type of the actual argument.
		/// This overload is only used if the item sought is a string,
		/// since any other type implies that we are looking for a
		/// collection member.
		/// </summary>
		public static ContainsConstraint Contain(string expected) =>
			new ContainsConstraint(expected);

		#endregion

		#region StartWith

		/// <summary>
		/// Returns a constraint that succeeds if the actual
		/// value starts with the substring supplied as an argument.
		/// </summary>
		public static StartsWithConstraint StartWith(string expected)
		{
			return new StartsWithConstraint(expected);
		}

		#endregion

		#region EndWith

		/// <summary>
		/// Returns a constraint that succeeds if the actual
		/// value ends with the substring supplied as an argument.
		/// </summary>
		public static EndsWithConstraint EndWith(string expected)
		{
			return new EndsWithConstraint(expected);
		}

		#endregion

		#region Match

		/// <summary>
		/// Returns a constraint that succeeds if the actual
		/// value matches the regular expression supplied as an argument.
		/// </summary>
		public static RegexConstraint Match(string pattern)
		{
			return new RegexConstraint(pattern);
		}

		#endregion
	}
}