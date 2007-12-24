using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Palaso.Services;

namespace Palaso.Tests.Services
{
	[TestFixture]
	public class IServiceAppSingletonHelperTests
	{
		private bool _bringToFrontRequestCalled;

		[SetUp]
		public void Setup()
		{
			_bringToFrontRequestCalled = false;
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
		[Test]
		public void StartWithDifferentNameReturnsService()
		{
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo", false);
			Assert.IsNotNull(ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("blah", false));
		}

		[Test]
		public void InitiallyInServerModeWhenRequested()
		{
			IServiceAppSingletonHelper helper = Palaso.Services.ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo2", false);
			Assert.IsFalse(helper.InServerMode);
		}
		[Test]
		public void InitiallyNotInServerModeWhenRequested()
		{
			IServiceAppSingletonHelper helper = Palaso.Services.ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo3", true);
			Assert.IsTrue(helper.InServerMode);
		}
		[Test]
		public void SecondAttemptCallsBringToFront()
		{
			IServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo4", false);
			helper.BringToFrontRequest += new EventHandler(helper_BringToFrontRequest);
			Assert.IsFalse(_bringToFrontRequestCalled);
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo4", false);
			Assert.IsTrue(_bringToFrontRequestCalled);
		}
		[Test]
		public void SecondAttemptDoesNotCallBringToFrontIfServerModeRequested()
		{
			IServiceAppSingletonHelper helper = ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo5", false);
			helper.BringToFrontRequest += new EventHandler(helper_BringToFrontRequest);
			Assert.IsFalse(_bringToFrontRequestCalled);
			ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo5", true);
			Assert.IsFalse(_bringToFrontRequestCalled);
		}
		void helper_BringToFrontRequest(object sender, EventArgs e)
		{
			_bringToFrontRequestCalled = true;
		}
	}
}