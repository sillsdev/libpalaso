// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Reflection;
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

		[Test]
		[Platform(Include="MacOsX,Win")]
		public void IsLinux_MacWindows()
		{
			Assert.That(Platform.IsLinux, Is.False);
		}

		[Test]
		[Platform(Include="Linux")]
		public void IsLinux_Linux()
		{
			Assert.That(Platform.IsLinux, Is.True);
		}

		[Test]
		[Platform(Include="MacOsX,Linux")]
		public void IsWindows_MacLinux()
		{
			Assert.That(Platform.IsWindows, Is.False);
		}

		[Test]
		[Platform(Include="Win")]
		public void IsWindows_Windows()
		{
			Assert.That(Platform.IsWindows, Is.True);
		}

		[Test]
		[Platform(Include="MacOsX")]
		public void IsMac_Mac()
		{
			Assert.That(Platform.IsMac, Is.True);
		}

		[Test]
		[Platform(Include="Linux, Win")]
		public void IsMac_LinuxWin()
		{
			Assert.That(Platform.IsMac, Is.False);
		}

		[Test]
		[Platform(Include="MacOsX, Linux")]
		public void IsUnix_MacLinux()
		{
			Assert.That(Platform.IsUnix, Is.True);
		}

		[Test]
		[Platform(Include="Win")]
		public void IsUnix_Windows()
		{
			Assert.That(Platform.IsUnix, Is.False);
		}

		[Test]
		[Platform(Include = "Win", Reason = "Windows specific test")]
		public void DesktopEnvironment_Windows()
		{
			// SUT
			Assert.That(Platform.DesktopEnvironment, Is.EqualTo("Win32NT"));
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		[TestCase("Unity", null, "ubuntu", ExpectedResult = "unity", TestName = "Unity")]
		[TestCase("Unity", "/usr/share/ubuntu:/usr/share/gnome:/usr/local/share/:/usr/share/",
			"ubuntu", ExpectedResult = "unity", TestName = "Unity with dataDir")]
		[TestCase("GNOME", null, "gnome-shell", ExpectedResult = "gnome",
			TestName = "Gnome shell")]
		[TestCase("GNOME", null, "cinnamon", ExpectedResult = "cinnamon",
			TestName = "Wasta 12")]
		[TestCase("x-cinnamon", null, "cinnamon", ExpectedResult = "x-cinnamon",
			TestName = "Wasta 14")]
		[TestCase(null, "/usr/share/ubuntu:/usr/share/kde:/usr/local/share/:/usr/share/",
			"kde-plasma", ExpectedResult = "kde", TestName = "KDE on Ubuntu 12_04")]
		[TestCase("XFCE", null, "xubuntu", ExpectedResult = "xfce", TestName = "XFCE")]
		[TestCase("foo", null, null, ExpectedResult = "foo", TestName = "Only XDG_CURRENT_DESKTOP set")]
		[TestCase(null, "/usr/share/ubuntu:/usr/share/kde:/usr/local/share/:/usr/share/", null,
			ExpectedResult = "kde", TestName = "Only XDG_DATA_DIRS set")]
		[TestCase(null, null, "something", ExpectedResult = "something", TestName = "Only GDMSESSION set")]
		[TestCase(null, null, null, ExpectedResult = "", TestName = "Nothing set")]
		[TestCase("ubuntu:GNOME", null, "ubuntu", ExpectedResult = "gnome", TestName = "Ubuntu 20.04 (Gnome)")]
		[TestCase("ubuntu:GNOME", null, "ubuntu-wayland", ExpectedResult = "gnome", TestName = "Ubuntu 20.04 (Gnome + Wayland)")]
		[TestCase("X-Cinnamon", null, "cinnamon", ExpectedResult = "x-cinnamon", TestName = "Wasta 20 (Cinnamon)")]
		[TestCase("ubuntu:GNOME", null, "ubuntu", ExpectedResult = "gnome", TestName = "Wasta 20 (Gnome)")]
		public string DesktopEnvironment_SimulateDesktops(string currDesktop,
			string dataDirs, string gdmSession)
		{
			// See http://askubuntu.com/a/227669 for actual values on different systems

			// Setup
			Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", currDesktop);
			Environment.SetEnvironmentVariable("XDG_DATA_DIRS", dataDirs);
			Environment.SetEnvironmentVariable("GDMSESSION", gdmSession);

			// SUT
			return Platform.DesktopEnvironment;
		}

		[Test]
		[Platform(Include = "Win", Reason = "Windows specific test")]
		public void DesktopEnvironmentInfoString_Windows()
		{
			// SUT
			Assert.That(Platform.DesktopEnvironmentInfoString, Is.Empty);
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		[TestCase("Unity", null, "ubuntu", null, ExpectedResult = "unity (ubuntu)", TestName = "Unity")]
		[TestCase("Unity", "/usr/share/ubuntu:/usr/share/gnome:/usr/local/share/:/usr/share/",
			"ubuntu", null, ExpectedResult = "unity (ubuntu)", TestName = "Unity with dataDir")]
		[TestCase("Unity", null, "ubuntu", "session-1",
			ExpectedResult = "unity (ubuntu [display server: Mir])", TestName = "Unity with Mir")]
		[TestCase("GNOME", null, "gnome-shell", null, ExpectedResult = "gnome (gnome-shell)",
			TestName = "Gnome shell")]
		[TestCase("GNOME", null, "cinnamon", null, ExpectedResult = "cinnamon (cinnamon)",
			TestName = "Wasta 12")]
		[TestCase("x-cinnamon", null, "cinnamon", null, ExpectedResult = "x-cinnamon (cinnamon)",
			TestName = "Wasta 14")]
		[TestCase(null, "/usr/share/ubuntu:/usr/share/kde:/usr/local/share/:/usr/share/",
			"kde-plasma", null, ExpectedResult = "kde (kde-plasma)", TestName = "KDE on Ubuntu 12_04")]
		[TestCase(null, null, null, null, ExpectedResult = " (not set)", TestName = "Nothing set")]
		[TestCase("ubuntu:GNOME", null, "ubuntu", "", ExpectedResult = "gnome (ubuntu)", TestName = "Ubuntu 20.04 (Gnome)")]
		[TestCase("ubuntu:GNOME", null, "ubuntu-wayland", "",
			ExpectedResult = "gnome (ubuntu [display server: Wayland])", TestName = "Ubuntu 20.04 (Gnome + Wayland)")]
		[TestCase("X-Cinnamon", null, "cinnamon", "", ExpectedResult = "x-cinnamon (cinnamon)", TestName = "Wasta 20 (Cinnamon)")]
		[TestCase("ubuntu:GNOME", null, "ubuntu", "", ExpectedResult = "gnome (ubuntu)", TestName = "Wasta 20 (Gnome)")]
		public string DesktopEnvironmentInfoString_SimulateDesktopEnvironments(string currDesktop,
			string dataDirs, string gdmSession, string mirServerName)
		{
			// See http://askubuntu.com/a/227669 for actual values on different systems

			// Setup
			Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", currDesktop);
			Environment.SetEnvironmentVariable("XDG_DATA_DIRS", dataDirs);
			Environment.SetEnvironmentVariable("GDMSESSION", gdmSession);
			Environment.SetEnvironmentVariable("MIR_SERVER_NAME", mirServerName);

			// SUT
			return Platform.DesktopEnvironmentInfoString;
		}

		[Test]
		[Platform(Include = "Linux", Reason = "Linux specific test")]
		public void MonoPlatform_Linux()
		{
			Assert.That(Platform.MonoVersion, Is.Not.Empty);
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "Windows specific test")]
		public void MonoPlatform_Windows()
		{
			Assert.That(Platform.MonoVersion, Is.Empty);
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		[TestCase("/usr/bin/gnome-session", "ubuntu", ExpectedResult = false, TestName = "Ubuntu Gnome")]
		[TestCase("/usr/bin/cinnamon-session", "cinnamon", ExpectedResult = true, TestName = "Wasta 12/14")]
		[TestCase("/usr/bin/gnome-session", "cinnamon", ExpectedResult = true, TestName = "Wasta 20 (Cinnamon)")]
		[TestCase("/usr/bin/gnome-session", "ubuntu", ExpectedResult = false, TestName = "Wasta 20 (Gnome)")]
		public bool IsCinnamon_SimulateDesktops(string sessionManager, string gdmSession)
		{
			// See http://askubuntu.com/a/227669 for actual values on different systems

			// Setup
			var field = typeof(Platform).GetField("_sessionManager", BindingFlags.Static | BindingFlags.NonPublic);
			field.SetValue(null, sessionManager);
			Environment.SetEnvironmentVariable("GDMSESSION", gdmSession);

			// SUT
			return Platform.IsCinnamon;
		}

		[Test]
		[Platform(Include = "Win", Reason = "Windows specific test")]
		public void IsCinnamon_Windows()
		{
			Assert.That(Platform.IsCinnamon, Is.False);
		}
	}
}

