using System;
using System.Threading;
using NUnit.Framework;
using SIL.PlatformUtilities;
using SIL.Threading;

namespace SIL.Tests.Threading
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class GlobalMutexTests
	{
		public GlobalMutexTests(bool localOnly)
		{
			if (localOnly)
			{
				Environment.SetEnvironmentVariable("SIL_CORE_MAKE_GLOBAL_MUTEX_LOCAL_ONLY", "true");
			}
			else
			{
				Environment.SetEnvironmentVariable("SIL_CORE_MAKE_GLOBAL_MUTEX_LOCAL_ONLY", null);
			}
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

		[Test, Timeout(5000)]
		public void Lock_AfterMutexAbandonedByExitedThread_RecoversWithoutThrowing()
		{
			if (!Platform.IsWindows)
				Assert.Ignore("AbandonedMutexException only surfaces through the Windows named Mutex; flock and Monitor adapters cannot reach it.");

			var envVar = Environment.GetEnvironmentVariable("SIL_CORE_MAKE_GLOBAL_MUTEX_LOCAL_ONLY");
			if (!string.IsNullOrEmpty(envVar))
				Assert.Ignore("LocalOnlyMutexAdapter uses Monitor; AbandonedMutexException is unreachable through it.");

			const string name = "libpalaso-test-abandoned-mutex";
			using (var mutex = new GlobalMutex(name))
			{
				mutex.Initialize();

				// Acquire and intentionally don't release: thread exit marks the mutex abandoned.
				var worker = new Thread(() =>
				{
					var raw = new Mutex(false, name);
					raw.WaitOne();
				}) { IsBackground = true };
				worker.Start();
				Assert.That(worker.Join(TimeSpan.FromSeconds(2)), Is.True, "Worker should exit promptly");

				// Nested Lock verifies we actually own the mutex, not just that we swallowed the exception.
				Assert.DoesNotThrow(() =>
				{
					using (mutex.Lock())
					using (mutex.Lock()) { }
				});
			}
		}
	}
}
