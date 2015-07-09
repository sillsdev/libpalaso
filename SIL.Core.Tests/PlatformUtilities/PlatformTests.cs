// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.PlatformUtilities;

namespace SIL.Tests.PlatformUtilities
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

#if SYSTEM_MAC
		[Test]
		public void IsLinux_Mac()
		{
			Assert.That(Platform.IsLinux, Is.False);
		}
#else
		[Test]
		[Platform(Include="Linux")]
		public void IsLinux_Linux()
		{
			Assert.That(Platform.IsLinux, Is.True);
		}
#endif

		[Test]
		[Platform(Include="Win")]
		public void IsLinux_Windows()
		{
			Assert.That(Platform.IsLinux, Is.False);
		}

#if SYSTEM_MAC
		[Test]
		public void IsWindows_Mac()
		{
			Assert.That(Platform.IsWindows, Is.False);
		}
#else
		[Test]
		[Platform(Include="Linux")]
		public void IsWindows_Linux()
		{
			Assert.That(Platform.IsWindows, Is.False);
		}
#endif

		[Test]
		[Platform(Include="Win")]
		public void IsWindows_Windows()
		{
			Assert.That(Platform.IsWindows, Is.True);
		}

#if SYSTEM_MAC
		[Test]
		public void IsMac_Mac()
		{
			Assert.That(Platform.IsMac, Is.True);
		}
#else
		[Test]
		[Platform(Include="Linux")]
		public void IsMac_Linux()
		{
			Assert.That(Platform.IsMac, Is.False);
		}
#endif

		[Test]
		[Platform(Include="Win")]
		public void IsMac_Windows()
		{
			Assert.That(Platform.IsMac, Is.False);
		}

#if SYSTEM_MAC
		[Test]
		public void IsUnix_Mac()
		{
			Assert.That(Platform.IsUnix, Is.True);
		}
#else
		[Test]
		[Platform(Include="Linux")]
		public void IsUnix_Linux()
		{
			Assert.That(Platform.IsUnix, Is.True);
		}
#endif

		[Test]
		[Platform(Include="Win")]
		public void IsUnix_Windows()
		{
			Assert.That(Platform.IsUnix, Is.False);
		}
	}
}

