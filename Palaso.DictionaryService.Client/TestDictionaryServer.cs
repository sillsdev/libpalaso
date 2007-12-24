using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;

namespace Palaso.DictionaryService.Client
{
	internal class TestDictionaryServer
	{
		private static System.Threading.AutoResetEvent stopFlag = new System.Threading.AutoResetEvent(false);
		public static string writingSystemIdToUseWhenStarting;
		public static void Main()
		{
		   // IDictionary dictionary = new TestDictionary(writingSystemIdToUseWhenStarting);

			ServiceHost serviceHost = new ServiceHost(typeof(TestDictionary));
			serviceHost.AddServiceEndpoint(
				typeof(IDictionary),
				new NetTcpBinding(),
				"net.tcp://localhost:8000");
			serviceHost.Open();


			Debug.WriteLine("SERVER - Running...");
			stopFlag.WaitOne();

			Debug.WriteLine("SERVER - Shutting down...");
			serviceHost.Close();

			Debug.WriteLine("SERVER - Shut down!");
		}

		public static void Stop()
		{
			stopFlag.Set();
		}
	}
}
