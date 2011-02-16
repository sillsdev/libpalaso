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
			Assert.AreEqual(2, parts.Count);
			Assert.AreEqual("x", parts[0]);
			Assert.AreEqual("audio", parts[1]);
		}

		[Test]
		public void GetParts_RfcSubtagConsistsOfCapitalExtensionPart_ReturnsThatPart()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("X-AUDIO");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(2, parts.Count);
			Assert.AreEqual("X", parts[0]);
			Assert.AreEqual("AUDIO", parts[1]);
		}

		[Test]
		public void GetParts_RfcSubtagConsistsOfTwoSimplePartsSepearatedByDash_ReturnsThatPart()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("variant-variant2");
			List<string> parts = parser.GetParts();
			Assert.AreEqual(2, parts.Count);
			Assert.AreEqual("variant", parts[0]);
			Assert.AreEqual("variant2", parts[1]);
		}

		[Test]
		public void GetParts_RfcSubtagcontainsInvalidCharacters_Throws()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("variant_variant2");
			Assert.Throws<ArgumentException>(() => parser.GetParts());
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
	}
}
