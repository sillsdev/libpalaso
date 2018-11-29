// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.ObjectModel;
using SIL.TestUtilities.NUnitExtensions;

namespace SIL.TestUtilities.Tests.NUnitExtensions
{
	[TestFixture]
	public class ValueEquatableConstraintTests
	{
		[Test]
		public void ValueEquatableConstraint()
		{
			Assert.That(new SomeClass("foo"), new ValueEquatableConstraint<SomeClass>(new SomeClass("foo")));
		}
	}

	#region Supporting classes for tests

	internal class SomeClass: IValueEquatable<SomeClass>
	{
		private string _text;

		public SomeClass(string text)
		{
			_text = text;
		}

		public bool ValueEquals(SomeClass other)
		{
			return _text == other._text;
		}
	}
	#endregion
}