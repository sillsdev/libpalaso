// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework.Constraints;
using SIL.ObjectModel;

namespace SIL.TestUtilities
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
}