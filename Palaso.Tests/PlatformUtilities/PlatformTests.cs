// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using Palaso.PlatformUtilities;

namespace Palaso.Tests.PlatformUtilities
{
	[TestFixture]
	public class PlatformTests
	{
		[Test]
		[Platform(Exclude="Net")]
		public void IsMono_Mono()
		{
			Assert.That(Platform.IsMono, Is.True);
		}

		[Test]
		[Platform(Include="Net")]
		public void IsMono_Net()
		{
			Assert.That(Platform.IsMono, Is.False);
		}

		[Test]
		[Platform(Exclude="Net")]
		public void IsDotnet_Mono()
		{
			Assert.That(Platform.IsDotNet, Is.False);
		}

		[Test]
		[Platform(Include="Net")]
		public void IsDotnet_Net()
		{
			Assert.That(Platform.IsDotNet, Is.True);
		}

		[Test]
		[Platform(Include="Linux")]
		public void IsLinux_Linux()
		{
			Assert.That(Platform.IsLinux, Is.True);
		}

		[Test]
		[Platform(Exclude="Linux")]
		public void IsLinux_Windows()
		{
			Assert.That(Platform.IsLinux, Is.False);
		}

		[Test]
		[Platform(Include="Linux")]
		public void IsWindows_Linux()
		{
			Assert.That(Platform.IsWindows, Is.False);
		}

		[Test]
		[Platform(Exclude="Linux")]
		public void IsWindows_Windows()
		{
			Assert.That(Platform.IsWindows, Is.True);
		}
	}
}

