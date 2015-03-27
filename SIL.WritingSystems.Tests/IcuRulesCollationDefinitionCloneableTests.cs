using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	public class IcuRulesCollationDefinitionCloneableTests : CollationDefinitionCloneableTests
	{
		public override CollationDefinition CreateNewCloneable()
		{
			return new IcuRulesCollationDefinition("standard");
		}

		public override string ExceptionList
		{
			get { return base.ExceptionList + "_imports|WritingSystemFactory|"; }
		}

		public override string EqualsExceptionList
		{
			get { return "|_collationRules|"; }
		}

		/// <summary>
		/// The generic test that clone copies everything can't handle lists.
		/// </summary>
		[Test]
		public void CloneCopiesImports()
		{
			var original = new IcuRulesCollationDefinition("standard");
			original.Imports.Add(new IcuCollationImport("en-US", "standard"));
			original.Imports.Add(new IcuCollationImport("zh-Hans-CN", "pinyin"));
			var copy = (IcuRulesCollationDefinition) original.Clone();
			Assert.That(copy.Imports, Is.EqualTo(new[] {new IcuCollationImport("en-US", "standard"), new IcuCollationImport("zh-Hans-CN", "pinyin")}));
		}

		[Test]
		public void ValueEqualsComparesImports()
		{
			var first = new IcuRulesCollationDefinition("standard");
			first.Imports.Add(new IcuCollationImport("en-US", "standard"));
			first.Imports.Add(new IcuCollationImport("zh-Hans-CN", "pinyin"));
			var second = new IcuRulesCollationDefinition("standard");

			Assert.That(first.ValueEquals(second), Is.False, "collations with empty imports should not equal one with some");
			second.Imports.Add(new IcuCollationImport("en-US", "standard"));
			Assert.That(first.ValueEquals(second), Is.False, "collations with different length import lists should not be equal");
			second.Imports.Add(new IcuCollationImport("zh-Hans-CN", "pinyin"));
			Assert.That(first.ValueEquals(second), Is.True, "collations with same import lists should be equal");

			second.Imports.Clear();
			second.Imports.Add(new IcuCollationImport("en-US", "standard"));
			second.Imports.Add(new IcuCollationImport("fr-FR", "standard"));
			Assert.That(first.ValueEquals(second), Is.False, "collations with same-length lists of different imports should not be equal");
		}

	}
}
