using NUnit.Framework;
using SIL.WritingSystems;

namespace SIL.TestUtilities.Tests
{
	[TestFixture]
	public class ValueEquatableComparerTests
	{
		[TestCase(1, 1, TestName = "integers equal")]
		[TestCase(1.0, 1.0)]
		[TestCase(1, 1.0, TestName = "same value, different types")]
		[TestCase("test", "test")]
		[TestCase(null, null)]
		[TestCase(new [] { 1, 2, 3 }, new[] { 1, 2, 3 })]
		public void Compare_Equal(object x, object y) => Assert.That(x, Is.EqualTo(y).Using(ValueEquatableComparer.Instance));

		[TestCase(1, 2)]
		[TestCase(1, 1.1)]
		[TestCase(null, true)]
		[TestCase("to", null)]
		public void Compare_Unequal(object x, object y) => Assert.That(x, Is.Not.EqualTo(y).Using(ValueEquatableComparer.Instance));

		[Test]
		public void Compare_IValueEquatable()
		{
			Assert.That(new CharacterSetDefinition("type"), Is.EqualTo(new CharacterSetDefinition("type")).Using(ValueEquatableComparer.Instance));
			Assert.That(new CharacterSetDefinition("type"), Is.Not.EqualTo(new CharacterSetDefinition("!")).Using(ValueEquatableComparer.Instance));
			Assert.That(new CharacterSetDefinition("type"), Is.Not.EqualTo(null).Using(ValueEquatableComparer.Instance));
			Assert.That(null!, Is.Not.EqualTo(new CharacterSetDefinition("type")).Using(ValueEquatableComparer.Instance));

			Sldr.Initialize(true);
			Assert.That(new WritingSystemDefinition("th"), Is.EqualTo(new WritingSystemDefinition("th")).Using(ValueEquatableComparer.Instance));
			Assert.That(new WritingSystemDefinition("th"), Is.Not.EqualTo(new WritingSystemDefinition("en")).Using(ValueEquatableComparer.Instance));

			Assert.That(new WritingSystemDefinition("th"), Is.Not.EqualTo(new CharacterSetDefinition("th")).Using(ValueEquatableComparer.Instance));
		}
	}
}
