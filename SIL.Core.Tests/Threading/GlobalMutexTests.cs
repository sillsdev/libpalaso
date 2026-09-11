using System;
using System.Threading;
using NUnit.Framework;
using SIL.Threading;

namespace SIL.Tests.Threading
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class GlobalMutexTests
	{
		private const string LocalOnlyVariable = "SIL_CORE_MAKE_GLOBAL_MUTEX_LOCAL_ONLY";

		private const string NotAbandonableReason =
			"A Monitor is released when its owning thread exits, so it cannot be abandoned.";

		private readonly bool _localOnly;
		private readonly string _previousLocalOnlyValue;

		public GlobalMutexTests(bool localOnly)
		{
			_localOnly = localOnly;
			_previousLocalOnlyValue = Environment.GetEnvironmentVariable(LocalOnlyVariable);
			if (localOnly)
			{
				Environment.SetEnvironmentVariable(LocalOnlyVariable, "true");
			}
			else
			{
				Environment.SetEnvironmentVariable(LocalOnlyVariable, null);
			}
		}

		[OneTimeTearDown]
		public void RestoreLocalOnlyVariable()
		{
			// The variable is read once per GlobalMutex construction and is process-global, so leaving
			// it set would hand a different adapter to every later test in this process.
			Environment.SetEnvironmentVariable(LocalOnlyVariable, _previousLocalOnlyValue);
		}

		/// <summary>
		/// Produces a name unique to this run. A mutex name is visible across the whole login session,
		/// so a fixed one lets concurrent test processes interfere with each other's expectations.
		/// </summary>
		private static string UniqueMutexName()
		{
			return $"test-{Guid.NewGuid():N}";
		}

		/// <summary>
		/// Leaves the mutex abandoned. A named mutex is owned by a thread, so a thread that exits without
		/// releasing it abandons it exactly as a process that dies while holding it does.
		/// </summary>
		private static void AbandonOnAnotherThread(GlobalMutex mutex)
		{
			var abandoner = new Thread(() => mutex.Lock()) { IsBackground = true };
			abandoner.Start();
			Assert.That(abandoner.Join(TimeSpan.FromSeconds(2)), Is.True, "the abandoning thread did not exit");
		}

		[Test]
		public void Initialize_CreatedNew_ReturnsTrue()
		{
			using (var mutex = new GlobalMutex("test"))
			{
				mutex.Unlink();
				Assert.That(mutex.Initialize(), Is.True);
			}
		}

		[Test]
		public void Initialize_Existing_ReturnsFalse()
		{
			using (var mutex1 = new GlobalMutex("test"))
			{
				mutex1.Initialize();
				using (var mutex2 = new GlobalMutex("test"))
					Assert.That(mutex2.Initialize(), Is.False);
			}
		}

		[Test]
		public void InitializeAndLock_CreatedNew_ReturnsTrue()
		{
			using (var mutex = new GlobalMutex("test"))
			{
				mutex.Unlink();
				bool createdNew;
				using (mutex.InitializeAndLock(out createdNew)) {}
				Assert.That(createdNew, Is.True);
			}
		}

		[Test]
		public void InitializeAndLock_Existing_ReturnsFalse()
		{
			using (var mutex1 = new GlobalMutex("test"))
			{
				mutex1.Initialize();
				using (var mutex2 = new GlobalMutex("test"))
				{
					bool createdNew;
					using (mutex2.InitializeAndLock(out createdNew)) {}
					Assert.That(createdNew, Is.False);
				}
			}
		}

		[Test, Timeout(1000)]
		public void InitializeAndLock_Reentrancy_DoesNotBlock()
		{
			using (var mutex = new GlobalMutex("test"))
			{
				using (mutex.InitializeAndLock())
				{
					using (mutex.Lock()) {}
				}
			}
		}

		[Test, Timeout(1000)]
		public void Lock_Reentrancy_DoesNotBlock()
		{
			using (var mutex = new GlobalMutex("test"))
			{
				mutex.Initialize();
				using (mutex.Lock())
				{
					using (mutex.Lock()) {}
				}
			}
		}

		[Test, Timeout(1000)]
		public void Lock_OwnerReleasedNormally_ReportsNotAbandoned()
		{
			using (var mutex = new GlobalMutex(UniqueMutexName()))
			{
				mutex.Initialize();
				using (mutex.Lock(out bool wasAbandoned))
					Assert.That(wasAbandoned, Is.False);
			}
		}

		/// <summary>
		/// The acquisition must succeed and report the abandonment rather than throwing, which is what
		/// made the crash in LT-21834 repeat on every subsequent launch.
		/// </summary>
		[Test, Timeout(5000)]
		public void Lock_PreviousOwnerAbandonedMutex_AcquiresAndReportsAbandonment()
		{
			if (_localOnly)
				Assert.Ignore(NotAbandonableReason);

			using (var mutex = new GlobalMutex(UniqueMutexName()))
			{
				mutex.Initialize();
				AbandonOnAnotherThread(mutex);

				using (mutex.Lock(out bool wasAbandoned))
					Assert.That(wasAbandoned, Is.True);
			}
		}

		/// <summary>
		/// The stack trace reported in LT-21834 reaches the wait through InitializeAndLock rather than
		/// Lock: the mutex already exists, so ownership is taken after construction instead of with it.
		/// </summary>
		[Test, Timeout(5000)]
		public void InitializeAndLock_PreviousOwnerAbandonedMutex_AcquiresAndReportsAbandonment()
		{
			if (_localOnly)
				Assert.Ignore(NotAbandonableReason);

			string name = UniqueMutexName();
			using (var mutex = new GlobalMutex(name))
			{
				mutex.Initialize();
				AbandonOnAnotherThread(mutex);

				bool createdNew;
				bool wasAbandoned;
				using (var other = new GlobalMutex(name))
					using (other.InitializeAndLock(out createdNew, out wasAbandoned)) {}

				Assert.That(createdNew, Is.False);
				Assert.That(wasAbandoned, Is.True);
			}
		}

		/// <summary>
		/// Callers that do not ask whether the mutex was abandoned must simply carry on. This is the
		/// shape of every existing caller, and the guarantee that keeps them from crashing.
		/// </summary>
		[Test, Timeout(5000)]
		public void Lock_PreviousOwnerAbandonedMutex_OverloadWithoutReportRecovers()
		{
			if (_localOnly)
				Assert.Ignore(NotAbandonableReason);

			using (var mutex = new GlobalMutex(UniqueMutexName()))
			{
				mutex.Initialize();
				AbandonOnAnotherThread(mutex);

				Assert.That(() => { using (mutex.Lock()) {} }, Throws.Nothing);
			}
		}

		/// <summary>
		/// The counterpart of <see cref="Lock_PreviousOwnerAbandonedMutex_OverloadWithoutReportRecovers"/>
		/// for the route that acquires the mutex while initializing it.
		/// </summary>
		[Test, Timeout(5000)]
		public void InitializeAndLock_PreviousOwnerAbandonedMutex_OverloadWithoutReportRecovers()
		{
			if (_localOnly)
				Assert.Ignore(NotAbandonableReason);

			string name = UniqueMutexName();
			using (var mutex = new GlobalMutex(name))
			{
				mutex.Initialize();
				AbandonOnAnotherThread(mutex);

				using (var other = new GlobalMutex(name))
					Assert.That(() => { using (other.InitializeAndLock()) {} }, Throws.Nothing);
			}
		}

		/// <summary>
		/// Abandonment describes one acquisition, not the mutex. Once it has been reported and the lock
		/// released normally, the next caller must be told the state it protects is sound again.
		/// </summary>
		[Test, Timeout(5000)]
		public void Lock_AfterRecoveringFromAbandonment_ReportsNotAbandoned()
		{
			if (_localOnly)
				Assert.Ignore(NotAbandonableReason);

			using (var mutex = new GlobalMutex(UniqueMutexName()))
			{
				mutex.Initialize();
				AbandonOnAnotherThread(mutex);

				bool firstWasAbandoned;
				using (mutex.Lock(out firstWasAbandoned)) {}
				bool secondWasAbandoned;
				using (mutex.Lock(out secondWasAbandoned)) {}

				Assert.That(firstWasAbandoned, Is.True);
				Assert.That(secondWasAbandoned, Is.False);
			}
		}
	}
}
