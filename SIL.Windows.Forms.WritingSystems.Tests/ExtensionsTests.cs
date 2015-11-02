using NUnit.Framework;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	public class ExtensionsTests
	{
		[Test]
		public void ToValidVariantString_RegisteredVariant_NoChange()
		{
			Assert.AreEqual("1901", "1901".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_HasSpace_RemovesSpace()
		{
			Assert.AreEqual("1901", "1901    ".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_RegisteredVariantAndPrivateUse_AddsX()
		{
			Assert.AreEqual("1901-x-English", "1901-English".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_PrivateUseWithX_NoChange()
		{
			Assert.AreEqual("x-English", "x-English".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_HasXRegisteredVariant_NoChange()
		{
			Assert.AreEqual("x-1901", "x-1901".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_2RegisteredVariants_NoChange()
		{
			Assert.AreEqual("1901-Biske", "1901-Biske".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_RegisteredVariantAndPrivateUseOutOfOrder_ReOrdersAddsX()
		{
			Assert.AreEqual("1901-x-English", "English-1901".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_HasSpacesInMiddle_ConvertsToDash()
		{
			Assert.AreEqual("x-English-French", "English   French".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_HasCommaInMiddle_ConvertsToDash()
		{
			Assert.AreEqual("1901-x-English", "English, 1901".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_HasMultipleX_KeepsOneX()
		{
			Assert.AreEqual("x-English-French", "x-English-x-French".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_HasPeriodInMiddle_ConvertsToDash()
		{
			Assert.AreEqual("1901-x-ThaiSpecial", "1901. x-ThaiSpecial".ToValidVariantString());
		}

		[Test]
		public void ToValidVariantString_HasxDash_Empty()
		{
			Assert.AreEqual("", "x-".ToValidVariantString());
		}

	}
}