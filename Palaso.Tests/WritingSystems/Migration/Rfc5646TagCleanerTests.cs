using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.WritingSystems.Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class Rfc5646TagCleanerTests
	{

		[Test]
		public void CompleteTagConstructor_HasInvalidLanguageName_MovedToPrivateUse()
		{
			var cleaner = new Rfc5646TagCleaner("234");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("x-234"));
		}

		[Test]
		public void CompleteTagConstructor_HasLanguageNameAndOtherName_OtherNameMovedToPrivateUse()
		{
			var cleaner = new Rfc5646TagCleaner("abc-123");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("abc-x-123"));
		}

		[Test]
		public void CompleteTagConstructor_LanguageNameWithAudio_GetZxxxAdded()
		{
			var cleaner = new Rfc5646TagCleaner("aaa-x-audio");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("aaa-Zxxx-x-audio"));
		}

		[Test]
		public void CompleteTagConstructor_InvalidLanguageNameWithScript_QaaAdded()
		{
			var cleaner = new Rfc5646TagCleaner("wee-Latn");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-Latn-x-wee"));
		}

		[Test]
		public void CompleteTagConstructor_XDashBeforeValidLanguageName_NoChange()
		{
			var cleaner = new Rfc5646TagCleaner("x-de");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("x-de"));
		}

		[Test]
		public void CompleteTagConstructor_XDashBeforeString_NoChange()
		{
			var cleaner = new Rfc5646TagCleaner("x-blah");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("x-blah"));
		}

		[Test]
		public void CompleteTagConstructor_QaaWithXDashBeforeValidLanguageName_NoChange()
		{
			var cleaner = new Rfc5646TagCleaner("qaa-x-th");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-x-th"));
		}

		[Test]
		public void CompleteTagConstructor_TagContainsPrivateUseWithAdditionalXDash_RedundantXDashRemoved()
		{
			var cleaner = new Rfc5646TagCleaner("en-x-some-x-whatever");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("en-x-some-whatever"));
		}

		[Test]
		public void CompleteTagConstructor_TagContainsOnlyPrivateUseWithAdditionalXDash_RedundantXDashRemoved()
		{
			var cleaner = new Rfc5646TagCleaner("x-some-x-whatever");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("x-some-whatever"));
		}

		[Test]
		public void CompleteTagConstructor_PrivateUseWithAudioAndDuplicateX_MakesAudioTag()
		{
			var cleaner = new Rfc5646TagCleaner("x-en-Zxxx-x-audio");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-Zxxx-x-en-Zxxx-audio"));
		}

	}
}
