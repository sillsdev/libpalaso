using System;
using SIL.Lift.Parsing;

namespace SIL.Lift.Tests.Parsing
{
	class TraitMatcher : Trait
	{
		private readonly string _expectedName;
		private readonly string _expectedValue;
		private readonly int _expectedNumberofAnnotations;

		public TraitMatcher(string expectedName, string expectedValue, int expectedNumberOfAnnotations) : base(expectedName, expectedValue)
		{
			_expectedNumberofAnnotations = expectedNumberOfAnnotations;
			_expectedValue = expectedValue;
			_expectedName = expectedName;
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				throw new NullReferenceException();
			}

			Trait trait = (Trait)o;
			return (trait.Annotations.Count == _expectedNumberofAnnotations)
				   &&(trait.Name == _expectedName)
				   && (trait.Value == _expectedValue);
		}
	}
}