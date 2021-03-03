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
		public void ValueEquatableConstraint_Equivalent_Succeeds()
		{
			Assert.That(new SomeClass("foo"), new ValueEquatableConstraint<SomeClass>(new SomeClass("foo")));
		}

		[Test]
		public void ValueEquatableConstraint_Different_Fails()
		{
			var actual = new SomeClass("mine");
			var result = (new ValueEquatableConstraint<SomeClass>(new SomeClass("yours"))).ApplyTo(actual);
			Assert.False(result.IsSuccess);
			Assert.AreEqual(actual, result.ActualValue);
			Assert.AreEqual("Encapsulated (yours)", result.Description);
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

		public override string ToString() => $"Encapsulated ({_text})";
	}
	#endregion
}