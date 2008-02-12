using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Palaso.Services.ForClients;
using Palaso.Services.ForServers;

namespace Palaso.Services.Tests.ForServers
{
	[TestFixture]
	public class ServiceAppSingletonHelperTests
	{
		private bool _bringToFrontRequestCalled;
		private bool _startUICalled;

		[SetUp]
		public void Setup()
		{
			_bringToFrontRequestCalled = false;
			_startUICalled = false;
		}

		[Test]
		public void FirstStartReturnsService()
		{
			Assert.IsNotNull(ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo", false));
		}
		[Test]
		public void SecondReturnsNull()
		{
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo", false);
			Assert.IsNull(ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo", false));
		}
//
//        [Test]
//        public void ServiceClosesAfterSingleClientCloses()
//        {
//           .base.base----- ServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo10", false);
//
//           // helper.ClientDetach("foo10");
//        }

		[Test]
		public void StartWithDifferentNameReturnsService()
		{
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo", false);
			Assert.IsNotNull(ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("blah", false));
		}

		[Test]
		public void StateIsStartingBeforeRunIsCalled()
		{
			ServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo2", false);
			Assert.AreEqual(ServiceAppSingletonHelper.State.Starting, helper.CurrentState);
		}

		[Test]
		public void SecondAttemptCallsBringToFront()
		{
			ServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo4", false);
			helper.BringToFrontRequest += new EventHandler(helper_BringToFrontRequest);
			Assert.IsFalse(_bringToFrontRequestCalled);
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo4", false);
			Assert.IsTrue(_bringToFrontRequestCalled);
		}
		[Test]
		public void SecondAttemptDoesNotCallBringToFrontIfServerModeRequested()
		{
			ServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo5", false);
			helper.BringToFrontRequest += new EventHandler(helper_BringToFrontRequest);
			Assert.IsFalse(_bringToFrontRequestCalled);
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo5", true);
			Assert.IsFalse(_bringToFrontRequestCalled);
		}
		void helper_BringToFrontRequest(object sender, EventArgs e)
		{
			_bringToFrontRequestCalled = true;
		}

		[Test]
		public void AppStartedAsServerLaterOpensUIWhenLaunchedForUI()
		{
			ServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo6", true);
			Assert.IsFalse(_startUICalled);

			System.Threading.Thread giveUpThread = new Thread(new ThreadStart(GiveUpAfter1Second));
			System.Threading.Thread simulateFutureOpeningInUIMode = new Thread(new ThreadStart(MakeCall));
			simulateFutureOpeningInUIMode.Start();
			giveUpThread.Start();
			helper.HandleEventsUntilExit(On_StartUI);
			giveUpThread.Abort();
			Assert.IsTrue(_startUICalled);
		}

		[Test]
		public void Startup_WithoutWCF_OK()
		{
			IPCUtils.IsWcfAvailable = false;
			ServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo6", false);
			helper.HandleEventsUntilExit(On_StartUI);
			Assert.IsTrue(_startUICalled);
		}

		private void MakeCall()
		{
			Thread.Sleep(100);
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo6", false);
		}
		private void GiveUpAfter1Second()
		{
			Thread.Sleep(1000);
			Assert.Fail("Time to give up on test.");
		}

		private void On_StartUI()
		{
			_startUICalled = true;
		}
	}
}