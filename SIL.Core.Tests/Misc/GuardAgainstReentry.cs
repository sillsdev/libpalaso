using System;
using NUnit.Framework;
using SIL.Code;
using Guard = SIL.Code.Guard;

namespace SIL.Tests.Misc
{
	[TestFixture]
	public class GuardAgainstReentryTests
	{

		private class TestSession
		{
			private int _workCount;

			public void DoWork()
			{
				_workCount++;
			}

			public int WorkCount
			{
				get { return _workCount; }
			}
		}

#pragma warning disable 618
		private GuardAgainstReentry _sentry;
#pragma warning restore 618

		[SetUp]
		public void SetUp()
		{
			_sentry = null;
		}

		[Test]
		public void GuardAgainstReentry_ReturnsInstance()
		{
#pragma warning disable 618
			GuardAgainstReentry sentry = null;
			sentry = Guard.AgainstReEntry(sentry);
#pragma warning restore 618
			Assert.IsNotNull(sentry);
		}

		private void WorkWithoutReentry(TestSession session)
		{
#pragma warning disable 618
			using (_sentry = Guard.AgainstReEntry(_sentry))
#pragma warning restore 618
			{
				session.DoWork();
			}
		}

		[Test]
		public void GuardAgainstReentry_WithoutReentryOnce_WorksOk()
		{
			var session = new TestSession();
			WorkWithoutReentry(session);
			Assert.AreEqual(1, session.WorkCount);
		}

		[Test]
		public void GuardAgainstReentry_WithoutReentryTwice_WorksOk()
		{
			var session = new TestSession();
			WorkWithoutReentry(session);
			WorkWithoutReentry(session);
			Assert.AreEqual(2, session.WorkCount);
		}

		private void WorkWithReentry(TestSession session, int count)
		{
#pragma warning disable 618
			using (_sentry = Guard.AgainstReEntry(_sentry))
#pragma warning restore 618
			{
				session.DoWork();
				if (count == 0)
				{
					WorkWithReentry(session, count + 1);
				}
			}
		}

		[Test]
		public void GuardAgainstReentry_WithReentry_Throws()
		{
			var session = new TestSession();
			Assert.Throws<ApplicationException>(
				() => WorkWithReentry(session, 0));
		}

		private void WorkWithReentryExpected(TestSession session, int count)
		{
#pragma warning disable 618
			using (_sentry = Guard.AgainstReEntryExpected(_sentry))
#pragma warning restore 618
			{
				if (_sentry.HasEntered)
					return;
				session.DoWork();
				if (count == 0)
				{
					WorkWithReentryExpected(session, count + 1);
				}
			}
		}

		[Test]
		public void GuardAgainstReentry_Expected_Detected()
		{
			var session = new TestSession();
			WorkWithReentryExpected(session, 0);
			Assert.AreEqual(1, session.WorkCount);
		}

	}
}