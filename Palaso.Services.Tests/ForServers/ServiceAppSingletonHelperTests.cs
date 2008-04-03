using System;
using System.Threading;
using NUnit.Framework;
using Palaso.Services.ForClients;
using Palaso.Services.ForServers;

namespace Palaso.Services.Tests.ForServers
{
	[TestFixture]
	public class ServiceAppSingletonHelperTests
	{

		class ServerRunner : IDisposable
		{
			private readonly uiState _initialUiState;
			private Semaphore testDone;
			private Thread serverThread;

			public bool _bringToFrontRequestCalled;
			public bool _startUICalled;
			public ServiceAppSingletonHelper _helper;

			public enum uiState
			{
				dontBotherWithEvents,
				initiallyAsServer,
				initiallyWithUI
			}

			public ServerRunner(uiState initialUiState)
			{
				_initialUiState = initialUiState;

				testDone = new Semaphore(0, 1, "testDone");
			   Semaphore serverReady = new Semaphore(0, 1, "serversReady");
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
			   serverReady.WaitOne(3000, true);
			   Thread.Sleep(200);//really, what we want to do is wait until is at a steady state, ah well.
		   }

			public void Dispose()
			{
			   if (_initialUiState == uiState.dontBotherWithEvents)
			   {
				   testDone.Release(1);
			   }
			   else
			   {
				   _helper.TestRequestsExitFromServerMode();
			   }
			}

			private void StartServer()
			{
				 _helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(),
					  _initialUiState == uiState.initiallyAsServer);
				_helper.BringToFrontRequest += delegate { _bringToFrontRequestCalled = true; };

				Semaphore.OpenExisting("serversReady").Release(1);
				Semaphore.OpenExisting("testDone").WaitOne();
			}

			private void StartServerAndHandleEvents()
			{
				_helper =
					ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(),
					_initialUiState==uiState.initiallyAsServer);
				_helper.BringToFrontRequest += delegate { _bringToFrontRequestCalled = true; };
				Semaphore.OpenExisting("serversReady").Release(1);
				_helper.HandleEventsUntilExit(delegate { _startUICalled = true; });
			 //   Semaphore.OpenExisting("testDone").WaitOne();
			}

		}


		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void TearDown()
		{
			//we increment the port as the cleanest way to get a new start for each test;
			// it would probably be possible to teardown the existing stuff, but this takes
			// a long time to do (Cambell guesses 4 seconds)
		   IpcSystem._defaultPort++;
		   // ServiceAppSingletonHelper.DisposeForNextTest();
		}



		[Test]
		public void FirstStartReturnsService()
		{
			ServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(),false);
			Assert.IsNotNull(helper);
		}

		[Test]
		public void CanGetAtServiceFromTests()
		{
			using(new ServerRunner(ServerRunner.uiState.dontBotherWithEvents))
			{
				IServiceAppConnectorWithProxy service =
					IpcSystem.GetExistingService<IServiceAppConnectorWithProxy>(GetServiceName());
				Assert.IsNotNull(service);
				Assert.IsTrue(service.IsAlive());
//                TestResult r= service.TestReturnStruct("blah");
//                Assert.AreEqual("hello", r.ones[0]);
//                Assert.AreEqual("bye", r.twos[0]);
			}
		}


		public static string GetServiceName()
		{
			//this is intentionally a messy service name
			return "c:/one\two.three"+IpcSystem._defaultPort;//give a different name each test, so they don't interfere
		}

//        private void RunServerThreadAndWaitUntilReady(ThreadStart threadMethod)
//        {
//            Semaphore serverReady = new Semaphore(0, 1, "serversReady");
//            Thread simluteServer = new Thread(threadMethod);
//            simluteServer.Start();
//            serverReady.WaitOne(3000, true);
//        }



		[Test]
		public void CreateServiceAppSingletonHelperIfNeeded_SameService_ReturnsNull()
		{
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(),false);
			Assert.IsNull(ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), false));
		}
//
//        [Test]
//        public void ServiceClosesAfterSingleClientCloses()
//        {
//            using (new ServerRunner(false))
//            {
//            }
//            // helper.ClientDetach("foo10");
//        }

		[Test]
		public void StartWithDifferentNameReturnsService()
		{
			using (new ServerRunner(ServerRunner.uiState.initiallyAsServer))
			{
				Assert.IsNotNull(
					ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName() + "blah", false));
			}
		}

		[Test]
		public void StateIsStartingBeforeRunIsCalled()//review: is this right?
		{
			using (ServerRunner runner = new ServerRunner(ServerRunner.uiState.dontBotherWithEvents))
			{
				Assert.AreEqual(ServiceAppSingletonHelper.State.Starting, runner._helper.CurrentState);
			}
		}

 /* this turns out to be hard to write a test for, because we don't have a real ui, and the
  * service that we start just calls the startui() delegate (which we have only setting a variable)
  * and then exits.  So by the time we try to run the guts of this test, the helper is already
  * in exitting mode (the startui() returned, so it figures the application is exitting)
  *
		[Test]
		public void ServerRunningInUiMode_SecondAttempAsksForUiMode_CallsBringToFront()
		{
			using (ServerRunner runner = new ServerRunner(ServerRunner.uiState.initiallyWithUI))
			{
				Assert.IsFalse(runner._bringToFrontRequestCalled);
				Assert.AreEqual(ServiceAppSingletonHelper.State.UiMode, runner._helper.CurrentState);

				ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), true);
				Assert.IsTrue(runner._bringToFrontRequestCalled);
			}
		}
		*/

		[Test]
		public void ServerRunningInServerMode_SecondAttempAsksForServerMode_DoesNothing()
		{
			using (ServerRunner runner = new ServerRunner(ServerRunner.uiState.initiallyAsServer))
			{
				Assert.IsFalse(runner._bringToFrontRequestCalled);
				ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), true);
				Assert.IsFalse(runner._bringToFrontRequestCalled);
			}
		}

		[Test]
		public void ServerRunningInServerMode_SecondAttempAsksForUIMode_CallsStartUI()
		{
			using (ServerRunner runner = new ServerRunner(ServerRunner.uiState.initiallyAsServer))
			{
				Assert.IsFalse(runner._startUICalled);
				ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), true);
				Assert.IsFalse(runner._startUICalled);
			}
		}
	}
}