using System.IO;
using NMock2;
using SIL.Lift.Parsing;

namespace SIL.Lift.Tests.Parsing
{
	class TraitMatcher : Matcher
	{
		private readonly string _expectedName;
		private readonly string _expectedValue;
		private readonly int _expectedNumberofAnnotations;

		public TraitMatcher(string expectedName, string expectedValue, int expectedNumberOfAnnotations)
		{
			_expectedNumberofAnnotations = expectedNumberOfAnnotations;
			_expectedValue = expectedValue;
			_expectedName = expectedName;
		}

		public override bool Matches(object o)
		{
			Trait trait = (Trait)o;
			return (trait.Annotations.Count == _expectedNumberofAnnotations)
				   &&(trait.Name == _expectedName)
				   && (trait.Value == _expectedValue);
		}

		public override void DescribeTo(TextWriter writer)
		{
			writer.Write(string.Format("TraitMatcher(expectedName={0}, expectedValue={1}, numberOfAnnotations={2})",  _expectedName, _expectedValue, _expectedNumberofAnnotations));
		}
	}
}