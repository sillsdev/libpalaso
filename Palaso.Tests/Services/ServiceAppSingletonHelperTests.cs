using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Palaso.Services;

namespace Palaso.Tests.Services
{
	[TestFixture]
	public class ServiceAppSingletonHelperTests
	{
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
			ServiceAppSingletonHelper helper = Palaso.Services.ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo2", false);
			Assert.IsFalse(helper.InServerMode);
		}
		[Test]
		public void InitiallyNotInServerModeWhenRequested()
		{
			ServiceAppSingletonHelper helper = Palaso.Services.ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo3", true);
			Assert.IsTrue(helper.InServerMode);
		}
		[Test]
		public void SecondAttemptCallsBringToFront()
		{
			ServiceAppSingletonHelper helper = Palaso.Services.ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("foo4", true);
			helper.BringToFrontRequest
		}
	}
}