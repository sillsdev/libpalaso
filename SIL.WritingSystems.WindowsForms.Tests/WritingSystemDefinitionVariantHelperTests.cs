using NUnit.Framework;

namespace SIL.WritingSystems.WindowsForms.Tests
{
	[TestFixture]
	public class WritingSystemDefinitionVariantHelperTests
	{
		[Test]
		public void ValidVariant_RegisteredVariant_NoChange()
		{
			Assert.AreEqual("1901", WritingSystemDefinitionVariantHelper.ValidVariantString("1901"));
		}

		[Test]
		public void ValidVariant_HasSpace_RemovesSpace()
		{
			Assert.AreEqual("1901", WritingSystemDefinitionVariantHelper.ValidVariantString("1901    "));
		}

		[Test]
		public void ValidVariant_RegisteredVariantAndPrivateUse_AddsX()
		{
			Assert.AreEqual("1901-x-English", WritingSystemDefinitionVariantHelper.ValidVariantString("1901-English"));
		}

		[Test]
		public void ValidVariant_PrivateUseWithX_NoChange()
		{
			Assert.AreEqual("x-English", WritingSystemDefinitionVariantHelper.ValidVariantString("x-English"));
		}

		[Test]
		public void ValidVariant_HasXRegisteredVariant_NoChange()
		{
			Assert.AreEqual("x-1901", WritingSystemDefinitionVariantHelper.ValidVariantString("x-1901"));
		}

		[Test]
		public void ValidVariant_2RegisteredVariants_NoChange()
		{
			Assert.AreEqual("1901-Biske", WritingSystemDefinitionVariantHelper.ValidVariantString("1901-Biske"));
		}

		[Test]
		public void ValidVariant_RegisteredVariantAndPrivateUseOutOfOrder_ReOrdersAddsX()
		{
			Assert.AreEqual("1901-x-English", WritingSystemDefinitionVariantHelper.ValidVariantString("English-1901"));
		}

		[Test]
		public void ValidVariant_HasSpacesInMiddle_ConvertsToDash()
		{
			Assert.AreEqual("x-English-French", WritingSystemDefinitionVariantHelper.ValidVariantString("English   French"));
		}

		[Test]
		public void ValidVariant_HasCommaInMiddle_ConvertsToDash()
		{
			Assert.AreEqual("1901-x-English", WritingSystemDefinitionVariantHelper.ValidVariantString("English, 1901"));
		}

		[Test]
		public void ValidVariant_HasMultipleX_KeepsOneX()
		{
			Assert.AreEqual("x-English-French", WritingSystemDefinitionVariantHelper.ValidVariantString("x-English-x-French"));
		}

		[Test]
		public void ValidVariant_HasPeriodInMiddle_ConvertsToDash()
		{
			Assert.AreEqual("1901-x-ThaiSpecial", WritingSystemDefinitionVariantHelper.ValidVariantString("1901. x-ThaiSpecial"));
		}

		[Test]
		public void ValidVariant_HasxDash_Empty()
		{
			Assert.AreEqual("", WritingSystemDefinitionVariantHelper.ValidVariantString("x-"));
		}

	}
}