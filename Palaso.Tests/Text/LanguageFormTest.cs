using System;
using System.Collections.Generic;
using NUnit.Framework;
using Palaso.Annotations;
using Palaso.Tests.Code;
using Palaso.Text;

namespace Palaso.Tests.Text
{
	[TestFixture]
	public class LanguageFormIClonableGenericTests:IClonableGenericTests<LanguageForm>
	{
		public override LanguageForm CreateNewClonable()
		{
			return new LanguageForm();
		}

		public override string ExceptionList
		{
			get {  return "|_parent|"; }
		}

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get { return new Dictionary<Type, object>
							 {
								 {typeof(string), "string"},
								 {typeof(Annotation), new Annotation()}
							 }; }
		}
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
		public void CompareTo_AlpahbeticallyLaterWritingSystemIdWithIdenticalForm_ReturnsGreater()
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
	}
}
