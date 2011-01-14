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
		public void GetParts_RfcSubtagIsEmpty_ReturnsEmptyList()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("");
			Assert.AreEqual(0, parser.GetParts().Count);
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
		public void GetParts_RfcSubtagConsistsOfCapitalExtensionPart_ReturnsThatPart()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("X-AUDIO");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("X-AUDIO", parts[0]);
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
		public void GetParts_RfcSubtagContainsExtensionWithUnderScore_Throws()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("x_audio");
			Assert.Throws<ArgumentException>(() => parser.GetParts());
		}

		[Test]
		public void GetParts_RfcSubtagContainsOnlyDash_ReturnsEmptyPartsList()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("-");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(0, parts.Count);
		}

		[Test]
		public void GetParts_RfcSubtagEndsWithDash_DashIsStripped()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("variant-");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("variant", parts[0]);
		}

		[Test]
		public void GetParts_RfcSubtagBeginsWithDash_DashIsStripped()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("-variant");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("variant", parts[0]);
		}

		[Test]
		public void GetParts_RfcSubtagContainsOnlyUnderScore_ReturnsEmptyPartsList()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("_");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(0, parts.Count);
		}

		[Test]
		public void GetParts_RfcSubtagEndsWithUnderScore_UnderscoreIsStripped()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("variant_");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("variant", parts[0]);
		}

		[Test]
		public void GetParts_RfcSubtagBeginsWithUnderScore_UnderscoreIsStripped()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("_variant");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("variant", parts[0]);
		}
	}
}
