using System.Collections.Generic;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	[TestFixture]
	internal class SubtagCloneableTests : CloneableTests<Rfc5646Tag.Subtag>
	{
		public override Rfc5646Tag.Subtag CreateNewCloneable()
		{
			return new Rfc5646Tag.Subtag();
		}

		public override string ExceptionList
		{
			get { return ""; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							{
								new ValuesToSet(new List<string>{"en"}, new List<string>{"de"})
							};
			}
		}
	}

	[TestFixture]
	public class SubtagTests
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