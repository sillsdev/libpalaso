using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using Palaso.Services.ForClients;
using Palaso.Services.ForServers;

namespace Palaso.Services.ForServers
{
	/// <summary>
	/// Use this class to export services from a user application.  It helps
	/// with handling the states of being invisible, becoming visible, and
	/// ensuring that only one instance of the application is running.
	/// </summary>
	public class ServiceAppSingletonHelper
	{
		public enum State
		{
			Starting,
			ServerMode,
			UiMode,
			Exitting,
			UiModeStarting
		} ;
		private State _state;
		private State _requestedState;
		private static ServiceAppConnector _connector;
		private static ServiceHost _singletonAppHost;
		private readonly string _pipeName;
		public event EventHandler BringToFrontRequest;

		/// <summary>
		/// Attempts to locate an already-running instance of the program with the given pipe
		/// (e.g., open on the same document).  If it finds one, it asks it to come to the front.
		/// If it can't find one, it claims that pipe and returns a new ServiceAppSingletonHelper
		/// </summary>
		/// <param name="pipeName"></param>
		/// <param name="startInServerMode"></param>
		/// <returns>null if an application is already open with that pipe, otherwise a helper object</returns>
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
			_state = State.Starting;
			_requestedState = startInServerMode ? State.ServerMode : State.UiMode;
		}

		public State CurrentState
		{
			get { return _state; }
		}

		public void UiReadyForEvents()
		{
			_state = State.UiMode;
			if (BringToFrontRequest != null)
			{
				BringToFrontRequest.Invoke(this, null);
			}
		}

		public void EnsureUIRunningAndInFront()
		{
			if (_state == State.UiMode)
			{
				On_BringToFrontRequest(this, null);
				return;
			}
			_requestedState = State.UiMode;
			for (int i = 0; i < 100; i++) // wait 5 seconds
			{
				if (_state == State.UiMode || _state == State.Exitting)
				{
					break;
				}
				Thread.Sleep(50);
			}
			//wait another second to allow the apps event hander to truly be running
			Thread.Sleep(1000);
			switch(_state)
			{
				case State.Exitting:
					throw new Exception(Process.GetCurrentProcess().ProcessName + " is in the process of Exitting.");
					break;
				case State.UiMode:
					return;
				case State.Starting:
					throw new Exception(
						string.Format("Gave up trying to get {0} To switch to ui mode (still Starting app).",
									  Process.GetCurrentProcess().ProcessName));
					break;
				case State.UiModeStarting:
					throw new Exception(
						string.Format("Gave up trying to get {0} To switch to ui mode (still Starting UI).",
									  Process.GetCurrentProcess().ProcessName));
				case State.ServerMode:
					throw new Exception(
						string.Format("Gave up trying to get {0} To switch to ui mode (still in server mode).",
									  Process.GetCurrentProcess().ProcessName));
			}
		}

		public void OnExitIfInServerMode(object sender, EventArgs e)
		{
			_requestedState =State.Exitting;
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
				if (_requestedState == State.UiMode)
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
				return "net.tcp://localhost/" + _pipeName;
			}
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

			_singletonAppHost.AddServiceEndpoint(typeof(IServiceAppConnector), new NetTcpBinding(),
												 SingletonAppAddress);
			_singletonAppHost.Open();
		}


		/// <summary>
		/// This is used to bring an already-in-ui-mode app up to the front by sending a message on to the
		/// form.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void On_BringToFrontRequest(object sender, EventArgs args)
		{
			_requestedState = State.UiMode;

			//now tell the form, if it is already existing and has registered for this event

			if (BringToFrontRequest != null)
			{
				BringToFrontRequest.Invoke(this, null);
			}
		}


		public delegate void StartUI();
		public void HandleEventsUntilExit(StartUI uiStarter)
		{
			bool someoneHasAttached=false;
			_state = State.ServerMode;
			while (true)
			{
				if (_connector.ClientIds.Count > 1)
				{
					someoneHasAttached = true;
				}
				//once at least one client has registered (attached), quit
				//when none are attached anymore
				if (someoneHasAttached && _connector.ClientIds.Count == 0)
				{
					_state = State.Exitting;
					break;
				}
				if (_requestedState == State.UiMode)
				{
					_state = State.UiModeStarting;
					uiStarter();//this guy need to call us back with UiReadyForEvents() when ui is fully up.
					// for now, always exit if the user exits the ui (can review this)
					_state = State.Exitting;
					break;
				}
				if (_requestedState == State.Exitting)
				{
					_state = State.Exitting;
					break;
				}
				Thread.Sleep(100);//provide the actual service while we sleep
			}
		}

	}
}