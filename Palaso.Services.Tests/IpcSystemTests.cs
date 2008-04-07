using System;
using System.Diagnostics;
using System.Threading;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace Palaso.Services.Tests
{
	[TestFixture]
	public class IpcSystemTests
	{
		[TearDown]
		public void TearDown()
		{
			IpcSystem.StartingPort += 10;  // so tests don't interfer with each other
		}

		[Test]
		public void CanServeUpOneServiceInThread()
		{
			using (new TestServerRunner("only_thread"))
			{
				TestGettingProcess("only_thread");
			}
		}

		[Test]
		public void CanServeUpTwoServicesInThreads()
		{
			using (new TestServerRunner("threadone"))
			{
				using (new TestServerRunner("threadtwo"))
				{
					TestGettingProcess("threadone");
					TestGettingProcess("threadtwo");
				 }
			}
		}

		[Test]
		public void CanServeServiceInSeperateProcess()
		{
			Process server = StartServerProcess("only");
			try
			{
				TestGettingProcess("only");
			}
			finally
			{
				server.Kill();
			}
		}

		[Test]
		public void CanServeTwoServicesInSeperateProcesses()
		{
			Process one = StartServerProcess("one");
			try
			{
				Process two = StartServerProcess("two");
				try
				{
					TestGettingProcess("one");
					TestGettingProcess("two");
				}
				finally
				{
					two.Kill();
				}
			}
			finally
			{
				one.Kill();
			}
		}

		private static void TestGettingProcess(string serviceName)
		{
			ITestService service = IpcSystem.GetExistingService<ITestServiceWithProxy>(serviceName);
			Assert.IsNotNull(service);
			Assert.IsTrue(service.Ping());
		}

		private static Process StartServerProcess(string serviceName)
		{
			Process server = Process.Start("Palaso.Services.Tests.Server.exe", serviceName+" "+IpcSystem.StartingPort);
			bool processIdle = false;
			TimeSpan lastProcessorTime = TimeSpan.Zero;
			while(!processIdle)
			{
				TimeSpan processorTime = server.TotalProcessorTime;

				processIdle = processorTime != TimeSpan.Zero
							  && lastProcessorTime == processorTime;
				lastProcessorTime = processorTime;
			}

			return server;
		}

		public interface ITestService
		{
			[XmlRpcMethod("TestService.Ping")]
			bool Ping();
		}

		public interface ITestServiceWithProxy : ITestService, IXmlRpcProxy, IPingable { }

		[XmlRpcService(Name = "Test Service")]
		public class TestService : MarshalByRefObject, ITestService
		{
			public bool Ping() { return true; }


		}

		class TestServerRunner : IDisposable
		{
			private readonly Semaphore _testDone;
			private readonly Thread _serverThread;
			public TestService Service;
			private readonly string _serviceName;

			public TestServerRunner(string serviceName)
			{
				_serviceName = serviceName;
				_testDone = new Semaphore(0, 1, "testDone:"+serviceName);
				Semaphore serverReady = new Semaphore(0, 1, "serversReady");
				_serverThread = new Thread(StartServer);

				_serverThread.Start();
				serverReady.WaitOne(3000, true);
				Thread.Sleep(200);
			}

			private void StartServer()
			{
				Service = new TestService();
				IpcSystem.StartServingObject(_serviceName, Service);

				//tell the unit test that the service is up, and it can more forward
				Semaphore.OpenExisting("serversReady").Release(1);

				//wait here until the unit test is over
				Semaphore.OpenExisting("testDone:" + _serviceName).WaitOne();
			}


			public void Dispose()
			{
				_testDone.Release(1);
			}
		}
	}
}
