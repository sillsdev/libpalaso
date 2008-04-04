using System;
using CookComputing.XmlRpc;

namespace Palaso.Services.Tests.Server
{
	class TestServer
	{
		static void Main(string[] args)
		{
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
			Console.WriteLine("Press Enter to exit");
			Console.ReadLine();
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
