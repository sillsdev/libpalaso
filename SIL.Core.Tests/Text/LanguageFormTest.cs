using System.Collections.Generic;
using NUnit.Framework;
using SIL.Annotations;
using SIL.TestUtilities;
using SIL.Text;

namespace SIL.Tests.Text
{
	[TestFixture]
	public class LanguageFormCloneableTests:CloneableTests<Annotatable>
	{
		public override Annotatable CreateNewCloneable()
		{
			return new LanguageForm();
		}

		public override string ExceptionList =>
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			"|Parent|";

		public override string EqualsExceptionList =>
			//_spans: is a List<T> which doesn't compare well with Equals (two separate empty lists are deemed different for example)
			"|Spans|";

		protected override List<ValuesToSet> DefaultValuesForTypes =>
			new List<ValuesToSet>
			{
				new ValuesToSet("string", "not string"),
				new ValuesToSet(new Annotation{IsOn = false}, new Annotation{IsOn = true}),
				new ValuesToSet(new List<LanguageForm.FormatSpan>(), new List<LanguageForm.FormatSpan>{new LanguageForm.FormatSpan()})
			};
	}

	[TestFixture]
	public class LanguageFormTest
	{
		LanguageForm _languageFormToCompare;
		LanguageForm _languageForm;

		[SetUp]
		public void Setup()
		{
			_languageFormToCompare = new LanguageForm();
			_languageForm = new LanguageForm();
		}

		[Test]
		public void CompareTo_Null_ReturnsGreater()
		{
			_languageFormToCompare = null;
			Assert.AreEqual(1, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void CompareTo_AlphabeticallyEarlierWritingSystemIdWithIdenticalForm_ReturnsLess()
		{
			_languageForm.WritingSystemId = "de";
			_languageForm.Form = "Word1";
			_languageFormToCompare.WritingSystemId = "en";
			_languageFormToCompare.Form = "Word1";
			Assert.AreEqual(-1, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void CompareTo_AlphabeticallyLaterWritingSystemIdWithIdenticalForm_ReturnsGreater()
		{
			_languageForm.WritingSystemId = "en";
			_languageForm.Form = "Word1";
			_languageFormToCompare.WritingSystemId = "de";
			_languageFormToCompare.Form = "Word1";
			Assert.AreEqual(1, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void CompareTo_IdenticalWritingSystemIdWithIdenticalForm_Returns_Equal()
		{
			_languageForm.WritingSystemId = "de";
			_languageForm.Form = "Word1";
			_languageFormToCompare.WritingSystemId = "de";
			_languageFormToCompare.Form = "Word1";
			Assert.AreEqual(0, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void CompareTo_AlphabeticallyEarlierFormWithIdenticalWritingSystem_ReturnsLess()
		{
			_languageForm.WritingSystemId = "de";
			_languageForm.Form = "Word1";
			_languageFormToCompare.WritingSystemId = "de";
			_languageFormToCompare.Form = "Word2";
			Assert.AreEqual(-1, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void CompareTo_AlphabeticallyLaterFormWithIdenticalWritingSystem_ReturnsGreater()
		{
			_languageForm.WritingSystemId = "de";
			_languageForm.Form = "Word2";
			_languageFormToCompare.WritingSystemId = "de";
			_languageFormToCompare.Form = "Word1";
			Assert.AreEqual(1, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void CompareTo_IdenticalFormWithIdenticalWritingSystem_ReturnsEqual()
		{
			_languageForm.WritingSystemId = "de";
			_languageForm.Form = "Word1";
			_languageFormToCompare.WritingSystemId = "de";
			_languageFormToCompare.Form = "Word1";
			Assert.AreEqual(0, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void CompareTo_AlphabeticallyEarlierWritingSystemAlphabeticallyLaterForm_ReturnsLess()
		{
			_languageForm.WritingSystemId = "de";
			_languageForm.Form = "Word2";
			_languageFormToCompare.WritingSystemId = "en";
			_languageFormToCompare.Form = "Word1";
			Assert.AreEqual(-1, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void CompareTo_AlphabeticallyEarlierFormAlphabeticallyLaterWritingSystem_ReturnsGreater()
		{
			_languageForm.WritingSystemId = "en";
			_languageForm.Form = "Word1";
			_languageFormToCompare.WritingSystemId = "de";
			_languageFormToCompare.Form = "Word2";
			Assert.AreEqual(1, _languageForm.CompareTo(_languageFormToCompare));
		}

		[Test]
		public void Equals_SameObject_True()
		{
			var form = new LanguageForm();
			Assert.That(form.Equals(form), Is.True);
		}

		[Test]
		public void Equals_OneStarredOtherIsNot_False()
		{
			var form1 = new LanguageForm();
			var form2 = new LanguageForm();
			form1.IsStarred = true;
			Assert.That(form1.Equals(form2), Is.False);
		}

		[Test]
		public void Equals_OneContainsWritingSystemOtherDoesNot_False()
		{
			var form1 = new LanguageForm{WritingSystemId = "en"};
			var form2 = new LanguageForm{WritingSystemId = "de"};
			Assert.That(form1.Equals(form2), Is.False);
		}

		[Test]
		public void Equals_OneContainsFormInWritingSystemOtherDoesNot_False()
		{
			var form1 = new LanguageForm { WritingSystemId = "en", Form = "form1"};
			var form2 = new LanguageForm { WritingSystemId = "en", Form = "form2" };
			Assert.That(form1.Equals(form2), Is.False);
		}

		[Test]
		public void Equals_StarredWritingSystemAndFormAreIdentical_True()
		{
			var form1 = new LanguageForm { IsStarred = true, WritingSystemId = "en", Form = "form1" };
			var form2 = new LanguageForm { IsStarred = true, WritingSystemId = "en", Form = "form1" };
			Assert.That(form1.Equals(form2), Is.True);
		}

		[Test]
		public void Equals_Null_False()
		{
			var form1 = new LanguageForm { IsStarred = true, WritingSystemId = "en", Form = "form1" };
			LanguageForm form2 = null;
			Assert.That(form1.Equals(form2), Is.False);
		}

		[Test]
		public void ObjectEquals_SameObject_True()
		{
			var form = new LanguageForm();
			Assert.That(form.Equals((object) form), Is.True);
		}

		[Test]
		public void ObjectEquals_OneStarredOtherIsNot_False()
		{
			var form1 = new LanguageForm();
			var form2 = new LanguageForm();
			form1.IsStarred = true;
			Assert.That(form1.Equals((object)form2), Is.False);
		}

		[Test]
		public void ObjectEquals_OneContainsWritingSystemOtherDoesNot_False()
		{
			var form1 = new LanguageForm { WritingSystemId = "en" };
			var form2 = new LanguageForm { WritingSystemId = "de" };
			Assert.That(form1.Equals((object)form2), Is.False);
		}

		[Test]
		public void ObjectEquals_OneContainsFormInWritingSystemOtherDoesNot_False()
		{
			var form1 = new LanguageForm { WritingSystemId = "en", Form = "form1" };
			var form2 = new LanguageForm { WritingSystemId = "en", Form = "form2" };
			Assert.That(form1.Equals((object)form2), Is.False);
		}

		[Test]
		public void ObjectEquals_StarredWritingSystemAndFormAreIdentical_True()
		{
			var form1 = new LanguageForm { IsStarred = true, WritingSystemId = "en", Form = "form1" };
			var form2 = new LanguageForm { IsStarred = true, WritingSystemId = "en", Form = "form1" };
			Assert.That(form1.Equals((object)form2), Is.True);
		}

		[Test]
		public void ObjectEquals_Null_False()
		{
			var form1 = new LanguageForm { IsStarred = true, WritingSystemId = "en", Form = "form1" };
			LanguageForm form2 = null;
			Assert.That(form1.Equals((object)form2), Is.False);
		}

		[Test]
		public void UpdateSpans_Insert()
		{
			// In these tests, we're testing (inserting) edits to the formatted string
			// "This is a <span class='Strong'>test</span> of <span class='Weak'>something</span> or other."
			// See http://jira.palaso.org/issues/browse/WS-34799 for a discussion of what we want to achieve.
			List<LanguageForm.FormatSpan> spans = new List<LanguageForm.FormatSpan>();
			string oldString = "This is a test of something or other.";

			// before any spans (add 3 characters)
			var expected = CreateSpans(spans, 3, 0, 3, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This isn't a test of something or other.", spans);
			VerifySpans(expected, spans);

			// before any spans, but right next to the first span (add 5 characters)
			expected = CreateSpans(spans, 5, 0, 5, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a good test of something or other.", spans);
			VerifySpans(expected, spans);

			// after any spans (add 5 characters, but spans don't change)
			expected = CreateSpans(spans, 0, 0, 0, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a test of something else or other.", spans);
			VerifySpans(expected, spans);

			// inside the first span (increase its length by 2)
			expected = CreateSpans(spans, 0, 2, 2, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a tessst of something or other.", spans);
			VerifySpans(expected, spans);

			// after the first span, but right next to it (increase its length by 3)
			expected = CreateSpans(spans, 0, 3, 3, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a testing of something or other.", spans);
			VerifySpans(expected, spans);

			// inside the second span (increase its length by 9)
			expected = CreateSpans(spans, 0, 0, 0, 9);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a test of some kind of thing or other.", spans);
			VerifySpans(expected, spans);

			// between the two spans (effectively add 1 character)
			expected = CreateSpans(spans, 0, 0, 1, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a test for something or other.", spans);
			VerifySpans(expected, spans);
		}

		[Test]
		public void AdjustSpans_Delete()
		{
			// In these tests, we're testing (deleting) edits to the formatted string
			// "This is a <span class='Strong'>test</span> of <span class='Weak'>something</span> or other."
			// See http://jira.palaso.org/issues/browse/WS-34799 for a discussion of what we want to achieve.
			List<LanguageForm.FormatSpan> spans = new List<LanguageForm.FormatSpan>();
			string oldString = "This is a test of something or other.";

			// before any spans (remove 3 characters)
			var expected = CreateSpans(spans, -3, 0, -3, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This a test of something or other.", spans);
			VerifySpans(expected, spans);

			// before any spans, but next to first span (remove 2 characters)
			expected = CreateSpans(spans, -2, 0, -2, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is test of something or other.", spans);
			VerifySpans(expected, spans);

			// start before any spans, but including part of first span (remove 2 before and 2 inside span)
			expected = CreateSpans(spans, -2, -2, -4, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is st of something or other.", spans);
			VerifySpans(expected, spans);

			// start before any spans, but extending past first span (remove 2 before, 4 inside, and 4 after/between)
			// The span length going negative is okay, and the same as going to zero: the span is ignored thereafter.
			expected = CreateSpans(spans, -2, -8, -10, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is something or other.", spans);
			VerifySpans(expected, spans);

			// delete exactly the first span (remove 4 inside)
			expected = CreateSpans(spans, 0, -4, -4, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a  of something or other.", spans);
			VerifySpans(expected, spans);

			// after any spans (effectively no change)
			expected = CreateSpans(spans, 0, 0, 0, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a test of something or other", spans);
			VerifySpans(expected, spans);

			// after any spans, but adjacent to last span (effectively no change)
			expected = CreateSpans(spans, 0, 0, 0, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a test of somethingther.", spans);
			VerifySpans(expected, spans);

			// delete from middle of first span to middle of second span
			expected = CreateSpans(spans, 0, -2, -6, -7);
			LanguageForm.AdjustSpansForTextChange(oldString, "This is a teng or other.", spans);
			VerifySpans(expected, spans);

			// change text without changing length of string
			// (alas, we can't handle this kind of wholesale change, so effectively no change to the spans)
			expected = CreateSpans(spans, 0, 0, 0, 0);
			LanguageForm.AdjustSpansForTextChange(oldString, "That is a joke of some other topic!!!", spans);
			VerifySpans(expected, spans);
		}

		static List<LanguageForm.FormatSpan> CreateSpans(List<LanguageForm.FormatSpan> spans, int deltaloc1, int deltalen1, int deltaloc2, int deltalen2)
		{
			spans.Clear();
			spans.Add(new LanguageForm.FormatSpan {
				Index = 10,
				Length = 4,
				Class = "Strong"
			});
			spans.Add(new LanguageForm.FormatSpan {
				Index = 18,
				Length = 9,
				Class = "Weak"
			});
			var expected = new List<LanguageForm.FormatSpan>();
			expected.Add(new LanguageForm.FormatSpan {
				Index = 10 + deltaloc1,
				Length = 4 + deltalen1,
				Class = "Strong"
			});
			expected.Add(new LanguageForm.FormatSpan {
				Index = 18 + deltaloc2,
				Length = 9 + deltalen2,
				Class = "Weak"
			});
			return expected;
		}

		void VerifySpans (List<LanguageForm.FormatSpan> expected, List<LanguageForm.FormatSpan> spans)
		{
			Assert.AreEqual(expected[0].Index, spans[0].Index, "first span index wrong");
			Assert.AreEqual(expected[0].Length, spans[0].Length, "first span length wrong");
			Assert.AreEqual(expected[0].Class, spans[0].Class, "first span class wrong");
			Assert.AreEqual(expected[1].Index, spans[1].Index, "second span index wrong");
			Assert.AreEqual(expected[1].Length, spans[1].Length, "second span length wrong");
			Assert.AreEqual(expected[1].Class, spans[1].Class, "second span class wrong");
		}
	}
}
