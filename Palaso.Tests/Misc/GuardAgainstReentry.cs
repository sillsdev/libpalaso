using System;
using NUnit.Framework;
using Palaso.Misc;

namespace Palaso.Tests.Misc
{
	[TestFixture]
	public class GuardAgainstReentryTests
	{

		private class TestSession
		{
			private int _workCount;

			public TestSession()
			{

			}

			public void DoWork()
			{
				_workCount++;
			}

			public int WorkCount
			{
				get { return _workCount; }
			}
		}

		private GuardAgainstReentry _sentry;

		[SetUp]
		public void SetUp()
		{
			_sentry = null;
		}

		[Test]
		public void GuardAgainstReentry_ReturnsInstance()
		{
			GuardAgainstReentry sentry = null;
			sentry = Guard.AgainstReEntry(sentry);
			Assert.IsNotNull(sentry);
		}

		private void WorkWithoutReentry(TestSession session)
		{
			using (_sentry = Guard.AgainstReEntry(_sentry))
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
			using (_sentry = Guard.AgainstReEntry(_sentry))
			{
				session.DoWork();
				if (count == 0)
				{
					WorkWithReentry(session, count + 1);
				}
			}
		}

		[Test]
		[ExpectedException(typeof(ApplicationException))]
		public void GuardAgainstReentry_WithReentry_Throws()
		{
			var session = new TestSession();
			WorkWithReentry(session, 0);
		}

		private void WorkWithReentryExpected(TestSession session, int count)
		{
			using (_sentry = Guard.AgainstReEntryExpected(_sentry))
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