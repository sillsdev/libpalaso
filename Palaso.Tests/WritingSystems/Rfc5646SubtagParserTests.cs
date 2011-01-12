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
		public void GetParts_RfcSubtagConsistsOfSimplePart_ReturnsThatPart()
		{
			Rfc5646SubtagParser parser = new Rfc5646SubtagParser("variant");
			List<string> parts = parser.Getparts();
			Assert.AreEqual(1, parts.Count);
			Assert.AreEqual("variant", parts[0]);
		}

		[Test]
		public void GetParts_RfcSubtagConsistsOfTwoSimplePartsSepearatedByDash_ReturnsThatPart()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void GetParts_RfcSubtagConsistsOfTwoSimplePartsSepearatedByUnderscore_ReturnsThatPart()
		{
			throw new NotImplementedException();
		}
	}
}
