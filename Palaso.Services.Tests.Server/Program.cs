using System;
using System.Threading;
using CookComputing.XmlRpc;

namespace Palaso.Services.Tests.Server
{
	class TestServer
	{
		static void Main(string[] args)
		{
			bool createdNew;


			Mutex serverIsStarting = new Mutex(true, "Palaso.Services.Tests.Server.ServerIsStarting", out createdNew);
			if (!createdNew)
			{
				serverIsStarting.WaitOne();
			}

			TestService service = new TestService();
			Console.WriteLine("Starting '{0}' Service...", args[0]);

			IpcSystem.StartingPort = int.Parse(args[1]);

			try
			{
				IpcSystem.StartServingObject(args[0], service);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error occurred: " + e.Message);
			}
			// we're done setting up so let tests know by releasing the mutex
			serverIsStarting.ReleaseMutex();

			// wait until test is done.
			Mutex testIsRunning = Mutex.OpenExisting("Palaso.Services.Tests.Runner.TestIsRunning");
			testIsRunning.WaitOne();
			testIsRunning.ReleaseMutex();
		}

		public interface ITestService
		{
			[XmlRpcMethod("TestService.Ping")]
			bool Ping();
		}

		public interface ITestServiceWithProxy : ITestService, IXmlRpcProxy { }

		[XmlRpcService(Name = "Test Service")]
		public class TestService : MarshalByRefObject, ITestService
		{
			public bool Ping() { return true; }
		}
	}
}
