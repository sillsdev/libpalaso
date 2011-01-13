using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class Rfc5646SubtagParserTests
	{
		[Test]
		public void GetParts_RfcSubtagIsEmpty_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Rfc5646SubtagParser(""));
		}

		[Test]
		public void GetParts_RfcSubtagConsistsOfSimplePart_ReturnsThatPart()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("variant");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("variant", parts[0]);
		}

		[Test]
		public void GetParts_RfcSubtagConsistsOfExtensionPart_ReturnsThatPart()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("x-audio");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("x-audio", parts[0]);
		}

		[Test]
		public void GetParts_RfcSubtagConsistsOfTwoSimplePartsSepearatedByDash_ReturnsThatPart()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("variant-variant2");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(3, parts.Count);
			Assert.AreEqual("variant", parts[0]);
			Assert.AreEqual("-", parts[1]);
			Assert.AreEqual("variant2", parts[2]);
		}

		[Test]
		public void GetParts_RfcSubtagConsistsOfTwoSimplePartsSepearatedByUnderscore_ReturnsThatPart()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("variant_variant2");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(3, parts.Count);
			Assert.AreEqual("variant", parts[0]);
			Assert.AreEqual("_", parts[1]);
			Assert.AreEqual("variant2", parts[2]);
		}

		[Test]
		public void GetParts_RfcSubtagContainsOnlyDash_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Rfc5646SubtagParser("-"));
		}

		[Test]
		public void GetParts_RfcSubtagEndsWithDash_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Rfc5646SubtagParser("variant-variant2-"));
		}

		[Test]
		public void GetParts_RfcSubtagBeginsWithDash_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Rfc5646SubtagParser("-variant-variant2"));
		}

		[Test]
		public void GetParts_RfcSubtagContainsOnlyUnderScore_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Rfc5646SubtagParser("_"));
		}

		[Test]
		public void GetParts_RfcSubtagEndsWithUnderScore_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Rfc5646SubtagParser("variant-variant2_"));
		}

		[Test]
		public void GetParts_RfcSubtagBeginsWithUnderScore_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Rfc5646SubtagParser("_variant-variant2"));
		}
	}
}
