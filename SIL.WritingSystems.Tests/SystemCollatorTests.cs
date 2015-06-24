using System;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class SystemCollatorTests
	{
		private SystemCollator _collator;

		[Test]
		public void NullId_ProducesCollator()
		{
			_collator = new SystemCollator(null);
			Assert.IsNotNull(_collator);
		}

		[Test]
		public void EmptyId_ProducesCollator()
		{
			_collator = new SystemCollator(String.Empty);
			Assert.IsNotNull(_collator);
		}

		[Test]
		public void InvalidId_ProducesCollator()
		{
			_collator = new SystemCollator("This shouldn't be a valid culture ID.");
			Assert.IsNotNull(_collator);
		}

		[Test]
		public void English_ProducesCollator()
		{
			_collator = new SystemCollator("en");
			Assert.IsNotNull(_collator);
		}

		[Test]
		public void ValidateLanguageTag_Valid_ReturnsTrue()
		{
			string message;
			Assert.That(SystemCollator.ValidateLanguageTag("en-US", out message), Is.True);
		}

		[Test]
		public void ValidateLanguageTag_InValid_ReturnsFalse()
		{
			string message;
			Assert.That(SystemCollator.ValidateLanguageTag("invalid", out message), Is.False);
		}

		[Test]
		public void Compare_EnglishCultureEnglishStrings_AreNotSame()
		{
			string a = "an english sentence";
			string b = "some words";

			_collator = new SystemCollator("en");
			var order = _collator.Compare(a, b);
			Assert.That(order, Is.Not.EqualTo(0));
		}

		[Test]
		public void Compare_InvariantCultureKhmerStrings_AreNotSame()
		{
			string a = "សង្ឃនៃអំបូរអឺរ៉ុន";
			string b = "បូជាចារ្យនៃអំបូរអឺរ៉ុន";

			_collator = new SystemCollator("");
			var order = _collator.Compare(a, b);
			Assert.That(order, Is.Not.EqualTo(0));
		}
	}
}
