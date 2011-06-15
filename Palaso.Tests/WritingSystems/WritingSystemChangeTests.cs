using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemChangeTests
	{
		[Test]
		public void Constructor_Data_SetsAllProperties()
		{
			var change = new WritingSystemChange("aaa", "bbb", "WeSay", "1.1");
			Assert.That(change.From, Is.EqualTo("aaa"));
			Assert.That(change.To, Is.EqualTo("bbb"));
			Assert.That(change.Producer, Is.EqualTo("WeSay"));
			Assert.That(change.ProducerVersion, Is.EqualTo("1.1"));
		}
	}
}