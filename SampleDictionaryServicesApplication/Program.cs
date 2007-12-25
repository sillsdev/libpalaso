using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using Palaso.DictionaryService.Client;
using Palaso.Services;

namespace SampleDictionaryServicesApplication
{
	static class Program
	{
		private static ServiceAppSingletonHelper _serviceAppSingletonHelper;
		private static TestDictionary _dictionary;

		[STAThread]
		static void Main(string[] args)
		{
			bool startInServerMode = args.Length > 0 && args[0] == "-server";
			_serviceAppSingletonHelper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("testApp", startInServerMode);
			if (_serviceAppSingletonHelper == null)
			{
				return; // there's already an instance of this app running
			}

		   using(_dictionary = new TestDictionary("qTest"))
		   {
			   StartDictionaryServices();

			   _serviceAppSingletonHelper.HandleRequestsUntilExitOrUIStart(StartUI);
		   }
		}

		private static void StartDictionaryServices()
		{

			ServiceHost _dictionaryHost = new ServiceHost(_dictionary, new Uri[] { new Uri(DictionaryServiceAddress), });

			_dictionaryHost.AddServiceEndpoint(typeof(ILookup), new NetNamedPipeBinding(),
												 DictionaryServiceAddress);
			_dictionaryHost.Open();

		}

		private static string DictionaryServiceAddress
		{
			get
			{
				return "net.pipe://localhost/DictionaryServices/" + "qTest";
			}
		}

		private static void StartUI()
		{
			//if this request is made from now on, it isn't to come out of invisible "server" mode;
			//it is to actually come to the front of the window z-order.  The form will need to
			//subscribe to it.
		 //   serviceAppSingletonHelper.BringToFrontRequest -= On_BringToFrontRequest;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1(_serviceAppSingletonHelper));
		}
	}





}