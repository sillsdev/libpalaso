using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.Text;

namespace Palaso.Tests.Text
{
	[TestFixture]
	public class LanguageFormTest
	{
		LanguageForm _languageFormToCompare = null;
		LanguageForm _languageForm = null;

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
	}
}
