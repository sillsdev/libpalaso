using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class RegionSubtagTests
	{
		[Test]
		public void PrivateUseRegion_DisplayLabelContainsCode()
		{
			const string code = "AA";
			var regionTag = new RegionSubtag(code, "PU region name", true, false);
			StringAssert.Contains(code, regionTag.DisplayLabel);
		}

		[Test]
		public void BlankRegionPlaceholder_DisplayLabelIsBlank()
		{
			Assert.AreEqual(string.Empty, new RegionSubtag("blank").DisplayLabel, "Blank placeholder should be blank");
		}

		[Test]
		public void NonPrivateUseRegion_DisplayLabelIsName()
		{
			const string name = "United States";
			var regionTag = new RegionSubtag("US", name, false, false);
			Assert.AreEqual(name, regionTag.DisplayLabel, "non-private use subtags do not need their codes in the label");
		}

	}
}
