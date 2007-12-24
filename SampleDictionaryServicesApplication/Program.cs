using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using Palaso.Services;

namespace SampleDictionaryServicesApplication
{
	static class Program
	{
		private static IServiceAppSingletonHelper _serviceAppSingletonHelper;

		[STAThread]
		static void Main(string[] args)
		{
			bool startInServerMode = args.Length > 0 && args[0] == "-server";
			_serviceAppSingletonHelper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("testApp", startInServerMode);
			if (_serviceAppSingletonHelper == null)
			{
				return; // there's already an instance of this app running
			}

			StartMathServices();

		   _serviceAppSingletonHelper.HandleRequestsUntilExitOrUIStart(StartUI);
	   }

		private static void StartMathServices()
		{
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




	[ServiceContract]
	public interface ILookup
	{
		[OperationContract]
		string GetHmtlForWord(string word);
	}
}