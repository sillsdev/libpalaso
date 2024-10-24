using System;
using System.IO;
using SIL.Lift.Parsing;

namespace SIL.Lift.Tests.Parsing
{
	class LiftMultiTextAnnotationMatcher : LiftMultiText
	{
		private readonly string _expectedLanguageOfFirstAnnotation;
		private readonly string _expectedNameOfFirstAnnotation;
		private readonly string _expectedValueOfFirstAnnotation;
		private readonly string _expectedWhoOfFirstAnnotation;
		private readonly DateTime _expectedWhenOfFirstAnnotation;
		private readonly int _expectedCount;

		public LiftMultiTextAnnotationMatcher(int expectedNumberOfAnnotations, string expectedLanguageOfFirstAnnotation, string expectedNameOfFirstAnnotation, string expectedValueOfFirstAnnotation, string expectedWhoOfFirstAnnotation, DateTime expectedWhenOfFirstAnnotation)
		{
			_expectedLanguageOfFirstAnnotation = expectedLanguageOfFirstAnnotation;
			_expectedWhenOfFirstAnnotation = expectedWhenOfFirstAnnotation;
			_expectedWhoOfFirstAnnotation = expectedWhoOfFirstAnnotation;
			_expectedCount = expectedNumberOfAnnotations;
			_expectedValueOfFirstAnnotation = expectedValueOfFirstAnnotation;
			_expectedNameOfFirstAnnotation = expectedNameOfFirstAnnotation;
		}

		public override bool Equals(object o)
		{
			LiftMultiText m = (LiftMultiText)o;
			if (m.Annotations.Count != _expectedCount)
			{
				return false;
			}
			Annotation t = m.Annotations[0];
			if (!string.IsNullOrEmpty(_expectedLanguageOfFirstAnnotation))
			{
//                    if (t.LanguageHint != _expectedLanguageOfFirstAnnotation)
//                    {
//                        return false;
//                    }
			}
			return (t.Name == _expectedNameOfFirstAnnotation
					&& t.Value == _expectedValueOfFirstAnnotation
					&& t.Who == _expectedWhoOfFirstAnnotation
					&& t.When == _expectedWhenOfFirstAnnotation);
		}
	}
}