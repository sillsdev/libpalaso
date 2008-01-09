using System;
using Palaso.DictionaryService.Client;
using Palaso.DictionaryService.SampleClient.Properties;
using SampleDictionaryServicesApplication;

namespace Palaso.DictionaryService.SampleClient
{
	public class ServiceMinder
	{

		public ServiceMinder()
		{
		}

		public IDictionaryService GetDictionaryService()
		{
			IDictionaryService dictionaryService = IPCUtils.GetExistingService<IDictionaryService>(ServiceAddress);
			if (dictionaryService == null)
			{
				string arguments = '"' + Settings.Default.PathToDictionary + '"' + " -server";
				MainWindow.Logger.Log("Starting service as [{0} {1}]...", PathToApp, arguments);
				System.Diagnostics.Process.Start(PathToApp, arguments);
				for (int i = 0; i < 20; i++)
				{
					System.Threading.Thread.Sleep(500);
					dictionaryService = IPCUtils.GetExistingService<IDictionaryService>(ServiceAddress);
					if (dictionaryService != null)
					{
						break;
					}
				}
			}
			if (dictionaryService == null)
			{
				MainWindow.Logger.Log("Failed to locate or start service");
			}
			return dictionaryService;
		}

		private string PathToApp
		{
			get { return Settings.Default.PathToApplication; }
		}

		private string ServiceAddress
		{
			get
			{
				return "net.pipe://localhost/DictionaryServices/"
					   + Uri.EscapeDataString(Settings.Default.PathToDictionary);
			}
		}

	}
}
