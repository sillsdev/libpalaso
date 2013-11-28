using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemChangeLogTests
	{
		[Test]
		public void HasChangeFor_InLogHasChange_ReturnsTrue()
		{
			var log = new WritingSystemChangeLog();
			log.LogChange("aab", "bba");
			Assert.That(log.HasChangeFor("aab"), Is.True);
		}

		[Test]
		public void HasChangeFor_InLogNoChange_ReturnsFalse()
		{
			var log = new WritingSystemChangeLog();
			log.LogChange("aaa", "bbb");
			log.LogChange("bbb", "aaa");
			Assert.That(log.HasChangeFor("aaa"), Is.False);
		}

		[Test]
		public void HasChangeFor_NotInLog_ReturnsFalse()
		{
			var log = new WritingSystemChangeLog();
			log.LogChange("aab", "bba");
			Assert.That(log.HasChangeFor("fff"), Is.False);
		}

		[Test]
		public void GetChangeFor_HasChange_ReturnsCorrectWsId()
		{
			var log = new WritingSystemChangeLog();
			log.LogChange("aab", "bba");
			Assert.That(log.GetChangeFor("aab"), Is.EqualTo("bba"));
		}

		[Test]
		public void GetChangeFor_NotInLog_ReturnsNull()
		{
			var log = new WritingSystemChangeLog();
			log.LogChange("aab", "bba");
			Assert.That(log.GetChangeFor("fff"), Is.EqualTo(null));
		}

		[Test]
		public void GetChangeFor_InLogButNoChange_ReturnsSameId()
		{
			var log = new WritingSystemChangeLog();
			log.LogChange("aaa", "bbb");
			log.LogChange("bbb", "aaa");
			Assert.That(log.GetChangeFor("aaa"), Is.EqualTo("aaa"));
		}

		[Test]
		public void LogChange_FromSameAsTo_DoesNotLogChange()
		{
			var log = new WritingSystemChangeLog();
			log.LogChange("aaa", "aaa");
			Assert.That(log.GetChangeFor("aaa"), Is.EqualTo(null));
		}
	}
}