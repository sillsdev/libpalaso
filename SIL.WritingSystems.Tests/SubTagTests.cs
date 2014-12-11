using System.Collections.Generic;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class SubTagTests
	{
		[Test]
		public void ParseSubtagForParts_SubtagContainsMultipleParts_PartsAreReturned()
		{
			List<string> parts = Rfc5646Tag.Subtag.ParseSubtagForParts("en-Latn-x-audio");
			Assert.AreEqual(4, parts.Count);
			Assert.AreEqual("en", parts[0]);
			Assert.AreEqual("Latn", parts[1]);
			Assert.AreEqual("x", parts[2]);
			Assert.AreEqual("audio", parts[3]);
		}

		[Test]
		public void ParseSubtagForParts_SubtagContainsOnePart_PartIsReturned()
		{
			List<string> parts = Rfc5646Tag.Subtag.ParseSubtagForParts("en");
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("en", parts[0]);
		}

		[Test]
		public void ParseSubtagForParts_SubtagIsEmpty_ListisEmpty()
		{
			List<string> parts = Rfc5646Tag.Subtag.ParseSubtagForParts("");
			Assert.IsTrue(parts.Count == 0);
		}

		[Test]
		public void ParseSubtagForParts_SubtagcontainsOnlyDashes_ListisEmpty()
		{
			List<string> parts = Rfc5646Tag.Subtag.ParseSubtagForParts("-------");
			Assert.IsTrue(parts.Count == 0);
		}

		[Test]
		public void ParseSubtagForParts_SubtagContainsMultipleConsecutiveDashes_DashesAreTreatedAsSingleDashes()
		{
			List<string> parts = Rfc5646Tag.Subtag.ParseSubtagForParts("-en--Latn-x---audio--");
			Assert.AreEqual(4, parts.Count);
			Assert.AreEqual("en", parts[0]);
			Assert.AreEqual("Latn", parts[1]);
			Assert.AreEqual("x", parts[2]);
			Assert.AreEqual("audio", parts[3]);
		}

	}
}