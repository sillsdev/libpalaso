using System;
using System.ServiceModel;
using System.Threading;
using SampleDictionaryServicesApplication;

namespace Palaso.Services
{
	/// <summary>
	/// Use this class to export services from a user application.  It helps
	/// with handling the states of being invisible, becoming visible, and
	/// ensuring that only one instance of the application is running.
	/// </summary>
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class ServiceAppSingletonHelper : IServiceAppSingletonHelper
	{
		private bool _inServerMode;
		private static ServiceHost _singletonAppHost;
		private readonly string _pipeName;
		public event EventHandler BringToFrontRequest;

		public static ServiceAppSingletonHelper CreateServiceAppSingletonHelperIfNeeded(string pipeName, bool startInServerMode)
		{
			ServiceAppSingletonHelper helper = new ServiceAppSingletonHelper(pipeName, startInServerMode);
			if (!helper.StartupIfAppropriate())
			{
				return null;
			}
			else
			{
				return helper;
			}
		}


		private ServiceAppSingletonHelper(string pipeName, bool startInServerMode)
		{
			if (string.IsNullOrEmpty(pipeName))
			{
				throw new ArgumentException("pipeName");
			}

			_pipeName = pipeName;
			_inServerMode = startInServerMode;
		}

		/// <summary>
		/// Decides whether there is already an app using this named pipe, and claims the pipe if not.
		/// </summary>
		/// <returns>false if this application should just exit</returns>
		private bool StartupIfAppropriate()
		{
			IServiceAppSingletonHelper alreadyExistingInstance = IPCUtils.GetExistingService<IServiceAppSingletonHelper>(SingletonAppAddress);
			if (alreadyExistingInstance != null)
			{
				if (!InServerMode)
				{
					alreadyExistingInstance.BringToFront();
				}
				return false;
			}

			StartServeAppSingletonService();
			return true;
		}


		private string SingletonAppAddress
		{
			get
			{
				return "net.pipe://localhost/" + _pipeName;
			}
		}

		public bool InServerMode
		{
			get { return _inServerMode; }
		}

		/// <summary>
		/// create the service any future versions of this (with the same pipeName) will use
		/// to find out we're already running and tell us other stuff (like BringToFront)
		/// </summary>
		private  void StartServeAppSingletonService()
		{
			BringToFrontRequest += On_BringToFrontRequest;

			_singletonAppHost = new ServiceHost(this, new Uri[] { new Uri(SingletonAppAddress), });

			_singletonAppHost.AddServiceEndpoint(typeof(IServiceAppSingletonHelper), new NetNamedPipeBinding(),
												 SingletonAppAddress);
			_singletonAppHost.Open();
		}

		private void On_BringToFrontRequest(object sender, EventArgs args)
		{
			_inServerMode = false;

		}
		public void BringToFront()
		{
			if (BringToFrontRequest != null)
			{
				BringToFrontRequest.Invoke(this, null);
			}
			_inServerMode = false;
		}

		public delegate void StartUI();
		public void HandleRequestsUntilExitOrUIStart(StartUI uiStarter)
		{
			while (true)
			{
				if (!InServerMode)
				{
					uiStarter();
					break;
				}
				//want to exit the app when the client closes us
				Thread.Sleep(10);
			}
		}
	}

	[ServiceContract]
	public interface IServiceAppSingletonHelper
	{
		[OperationContract]
		void BringToFront();
	}
}