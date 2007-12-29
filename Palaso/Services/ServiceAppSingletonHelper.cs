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
	public class ServiceAppSingletonHelper
	{
		private bool _inServerMode;
		private static ServiceAppConnector _connector;
		private static ServiceHost _singletonAppHost;
		private readonly string _pipeName;
		private bool _exitRequested=false;
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

		public void OnExitIfInServerMode(object sender, EventArgs e)
		{
			_exitRequested = true;
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
			IServiceAppConnector alreadyExistingInstance = IPCUtils.GetExistingService<IServiceAppConnector>(SingletonAppAddress);
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
			_connector = new    ServiceAppConnector();
			_connector.BringToFrontRequest+=On_BringToFrontRequest;
			_singletonAppHost = new ServiceHost(_connector, new Uri[] { new Uri(SingletonAppAddress), });

			_singletonAppHost.AddServiceEndpoint(typeof(IServiceAppConnector), new NetNamedPipeBinding(),
												 SingletonAppAddress);
			_singletonAppHost.Open();
		}


		private void On_BringToFrontRequest(object sender, EventArgs args)
		{
			_inServerMode = false;

			//now pass it on

			if (BringToFrontRequest != null)
			{
				BringToFrontRequest.Invoke(this, null);
			}
			_inServerMode = false;

		}


		public delegate void StartUI();
		public void HandleRequestsUntilExitOrUIStart(StartUI uiStarter)
		{
			bool someoneHasAttached=false;
			while (!_exitRequested)
			{
				if (_connector.ClientIds.Count > 1)
				{
					someoneHasAttached = true;
				}
				//once at least one client has registered (attached), quit
				//when none are attached anymore
				if (someoneHasAttached && _connector.ClientIds.Count == 0)
				{
					break;
				}
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


}