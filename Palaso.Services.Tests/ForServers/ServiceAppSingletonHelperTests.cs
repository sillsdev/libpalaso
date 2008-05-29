using NUnit.Framework;
using Palaso.Services.ForServers;

namespace Palaso.Services.Tests.ForServers
{
	[TestFixture]
	public class ServiceAppSingletonHelperTests
	{
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
		   IpcSystem.StartingPort++;
		   // ServiceAppSingletonHelper.DisposeForNextTest();
		}



		[Test]
	   // [Category("Long Running")]
		public void FirstStartReturnsService()
		{
			using (ServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), false))
			{
				Assert.IsNotNull(helper);
			}
		}

		[Test]
	//    [Category("Long Running")]
		public void CanGetAtServiceFromTests()
		{
			using(new ServerRunner(ServerRunner.uiState.dontBotherWithEvents, GetServiceName(), false))
			{
				IServiceAppConnectorWithProxy service =
					IpcSystem.GetExistingService<IServiceAppConnectorWithProxy>(GetServiceName());
				Assert.IsNotNull(service);
				Assert.IsTrue(service.Ping());
			}
		}



		public static string GetServiceName()
		{
			//this is intentionally a messy service name
			return "c:/onetwo.three"+IpcSystem.StartingPort;//give a different name each test, so they don't interfere
		}

//        private void RunServerThreadAndWaitUntilReady(ThreadStart threadMethod)
//        {
//            Semaphore serverReady = new Semaphore(0, 1, "serversReady");
//            Thread simluteServer = new Thread(threadMethod);
//            simluteServer.Start();
//            serverReady.WaitOne(3000, true);
//        }



		[Test]
		//[Category("Long Running")]
		public void CreateServiceAppSingletonHelperIfNeeded_SameService_ReturnsNull()
		{
			using (ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), false))
			{
				using (ServiceAppSingletonHelper singletonHelper2 = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), false, true))
				{
					Assert.IsNull(singletonHelper2);
				}
			}
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
	 //   [Category("Long Running")]
		public void StartWithDifferentNameReturnsService()
		{
			using (new ServerRunner(ServerRunner.uiState.initiallyAsServer, GetServiceName(), true))
			{
				using(ServiceAppSingletonHelper singletonHelper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName() + "blah", false))
				{
					Assert.IsNotNull(singletonHelper);
				}
			}
		}

		[Test]
	   // [Category("Long Running")]
		public void StateIsStartingBeforeRunIsCalled()//review: is this right?
		{
			using (ServerRunner runner = new ServerRunner(ServerRunner.uiState.dontBotherWithEvents, GetServiceName(), false))
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
	 //   [Category("Long Running")]
		public void ServerRunningInServerMode_SecondAttempAsksForServerMode_DoesNothing()
		{
			using (ServerRunner runner = new ServerRunner(ServerRunner.uiState.initiallyAsServer, GetServiceName(), false))
			{
				Assert.IsFalse(runner._bringToFrontRequestCalled);
				using(ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), true, true))
				{
					Assert.IsFalse(runner._bringToFrontRequestCalled);
				}
			}
		}

		[Test]
   //     [Category("Long Running")]
		public void ServerRunningInServerMode_SecondAttempAsksForUIMode_CallsStartUI()
		{
			using (ServerRunner runner = new ServerRunner(ServerRunner.uiState.initiallyAsServer, GetServiceName(), true))
			{
				Assert.IsFalse(runner._startUICalled);
				using(ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded(GetServiceName(), true, true))
				{
					Assert.IsFalse(runner._startUICalled);
				}
			}
		}

		[Test]
	  //  [Category("Long Running")]
		public void CanLaunchTwoDifferntServersInThreads()
		{
			using (new ServerRunner(ServerRunner.uiState.initiallyWithUI, GetServiceName(), true))
			{
				using (new ServerRunner(ServerRunner.uiState.initiallyWithUI, GetServiceName() + "-NumberTwo", true))
				{
					IServiceAppConnectorWithProxy service =
						IpcSystem.GetExistingService<IServiceAppConnectorWithProxy>(GetServiceName()+"-NumberTwo");
					Assert.IsNotNull(service);
					Assert.IsTrue(service.Ping());
				 }
			}
		}
	}
}