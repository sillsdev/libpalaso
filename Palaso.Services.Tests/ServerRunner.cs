using System;
using System.Threading;
using NUnit.Framework;
using Palaso.Services.ForServers;

namespace Palaso.Services.Tests.ForServers
{
	class ServerRunner : IDisposable
	{
		private readonly uiState _initialUiState;
		private readonly string _serviceName;
		private Semaphore testDone;
		private Thread serverThread;

		public bool _bringToFrontRequestCalled;
		public bool _startUICalled;
		public ServiceAppSingletonHelper _helper;
		private bool _couldHaveTwinsInProcess;

		public enum uiState
		{
			dontBotherWithEvents,
			initiallyAsServer,
			initiallyWithUI
		}

		public ServerRunner(uiState initialUiState, string serviceName, bool couldHaveTwinsInProcess)
		{
			_initialUiState = initialUiState;
			_couldHaveTwinsInProcess = couldHaveTwinsInProcess;
			_serviceName = serviceName;

			testDone = new Semaphore(0, 1, "testDone_" + serviceName);
			Semaphore serverReady = new Semaphore(0, 1, "serversReady_"+serviceName);
			_helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(
				_serviceName,
				_initialUiState ==
				uiState.initiallyAsServer,
				_couldHaveTwinsInProcess
			);
			if (initialUiState == uiState.dontBotherWithEvents)
			{
				serverThread = new Thread(StartServer);
			}
			else
			{
				serverThread =
					new Thread(StartServerAndHandleEvents);
			}
			serverThread.Start();
			if (!serverReady.WaitOne(5000 * IpcSystem.NumberOfPortsToTry, true))
			{
				Assert.Fail("ServerRunner Timed out waiting for server thread to complete startup.  There is a constant here in the test code which you can increase if need be.");
			}
		}

		public void Dispose()
		{
			if (_initialUiState == uiState.dontBotherWithEvents)
			{
				testDone.Release(1);
				Thread.Sleep(10);
			}
			else
			{
				_helper.TestRequestsExitFromServerMode();
				_helper.Dispose();
			}
		}

		private void StartServer()
		{
			_helper.BringToFrontRequest += delegate { _bringToFrontRequestCalled = true; };

			Semaphore.OpenExisting("serversReady_" + _serviceName).Release(1);

			Semaphore.OpenExisting("testDone_"+_serviceName).WaitOne();
		}

		private void StartServerAndHandleEvents()
		{
			_helper.BringToFrontRequest += delegate { _bringToFrontRequestCalled = true; };
			Semaphore.OpenExisting("serversReady_" + _serviceName).Release(1);
			_helper.HandleEventsUntilExit(delegate { _startUICalled = true; });
			//   Semaphore.OpenExisting("testDone").WaitOne();
		}

	}
}
