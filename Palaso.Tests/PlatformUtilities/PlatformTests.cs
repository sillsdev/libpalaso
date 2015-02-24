// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using Palaso.PlatformUtilities;

// For code shipped with products, please try to use runtime checks to select code to run.
// There are cases that require compile-time conditionals (e.g. when testing the runtime or
// referencing platform specific imported assemblies like MonoMac.dll).  In this case, do
// The following:
// 1) include the following PropertyGroup in the .csproj after all of the DefineConstants
//    elements.  [Note: _system_name and _system_type are defined by xbuild in later 
//    versions of Mono.  We tested with Mono 3.12 that comes with Xamarin Studio for 
//    Mac.  To verify, add /verbosity:diag to xbuild command-line. YMMV.]
//
// <PropertyGroup Condition= " '$(_system_name)' != '' ">
//   <DefineConstants>$(DefineConstants);SYSTEM_$(_system_name)</DefineConstants>
// </PropertyGroup>
//
// 2) In code use a conditional based on the desired platform.
// 
// #if SYSTEM_OSX
// using MonoMac.Foundation;
// #endif

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

#if SYSTEM_OSX
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

#if SYSTEM_OSX
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

#if SYSTEM_OSX
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

#if SYSTEM_OSX
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

