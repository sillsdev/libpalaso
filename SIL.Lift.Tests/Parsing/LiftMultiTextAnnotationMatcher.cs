using System;
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
			if (o == null)
			{
				throw new NullReferenceException();
			}

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

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (_expectedLanguageOfFirstAnnotation != null ? _expectedLanguageOfFirstAnnotation.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_expectedNameOfFirstAnnotation != null ? _expectedNameOfFirstAnnotation.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_expectedValueOfFirstAnnotation != null ? _expectedValueOfFirstAnnotation.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_expectedWhoOfFirstAnnotation != null ? _expectedWhoOfFirstAnnotation.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ _expectedWhenOfFirstAnnotation.GetHashCode();
				hashCode = (hashCode * 397) ^ _expectedCount;
				return hashCode;
			}
		}
	}
}