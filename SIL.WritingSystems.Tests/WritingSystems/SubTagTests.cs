using System.Collections.Generic;
using NUnit.Framework;
using SIL.WritingSystems.WritingSystems;

namespace SIL.WritingSystems.Tests.WritingSystems
{
	[TestFixture]
	public class SubTagTests
	{
		[Test]
		public void ParseSubtagForParts_SubtagContainsMultipleParts_PartsAreReturned()
		{
			List<string> parts = RFC5646Tag.SubTag.ParseSubtagForParts("en-Latn-x-audio");
			Assert.AreEqual(4, parts.Count);
			Assert.AreEqual("en", parts[0]);
			Assert.AreEqual("Latn", parts[1]);
			Assert.AreEqual("x", parts[2]);
			Assert.AreEqual("audio", parts[3]);
		}

		[Test]
		public void ParseSubtagForParts_SubtagContainsOnePart_PartIsReturned()
		{
			List<string> parts = RFC5646Tag.SubTag.ParseSubtagForParts("en");
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("en", parts[0]);
		}

		[Test]
		public void ParseSubtagForParts_SubtagIsEmpty_ListisEmpty()
		{
			List<string> parts = RFC5646Tag.SubTag.ParseSubtagForParts("");
			Assert.IsTrue(parts.Count == 0);
		}

		[Test]
		public void ParseSubtagForParts_SubtagcontainsOnlyDashes_ListisEmpty()
		{
			List<string> parts = RFC5646Tag.SubTag.ParseSubtagForParts("-------");
			Assert.IsTrue(parts.Count == 0);
		}

		[Test]
		public void ParseSubtagForParts_SubtagContainsMultipleConsecutiveDashes_DashesAreTreatedAsSingleDashes()
		{
			List<string> parts = RFC5646Tag.SubTag.ParseSubtagForParts("-en--Latn-x---audio--");
			Assert.AreEqual(4, parts.Count);
			Assert.AreEqual("en", parts[0]);
			Assert.AreEqual("Latn", parts[1]);
			Assert.AreEqual("x", parts[2]);
			Assert.AreEqual("audio", parts[3]);
		}

	}
}