// Copyright (c) 2014 SIL International
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

		[Test]
		[Platform(Include = "Win", Reason = "Windows specific test")]
		public void DesktopEnvironment_Windows()
		{
			// SUT
			Assert.That(Platform.DesktopEnvironment, Is.EqualTo("Win32NT"));
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		[TestCase("Unity", null, "ubuntu", Result = "unity", TestName = "Unity")]
		[TestCase("Unity", "/usr/share/ubuntu:/usr/share/gnome:/usr/local/share/:/usr/share/",
			"ubuntu", Result = "unity", TestName = "Unity with dataDir")]
		[TestCase("GNOME", null, "gnome-shell", Result = "gnome",
			TestName = "Gnome shell")]
		[TestCase("GNOME", null, "cinnamon", Result = "cinnamon",
			TestName = "Wasta 12")]
		[TestCase("x-cinnamon", null, "cinnamon", Result = "x-cinnamon",
			TestName = "Wasta 14")]
		[TestCase(null, "/usr/share/ubuntu:/usr/share/kde:/usr/local/share/:/usr/share/",
			"kde-plasma", Result = "kde", TestName = "KDE on Ubuntu 12_04")]
		[TestCase("XFCE", null, "xubuntu", Result = "xfce", TestName = "XFCE")]
		[TestCase("foo", null, null, Result = "foo", TestName = "Only XDG_CURRENT_DESKTOP set")]
		[TestCase(null, "/usr/share/ubuntu:/usr/share/kde:/usr/local/share/:/usr/share/", null,
			Result = "kde", TestName = "Only XDG_DATA_DIRS set")]
		[TestCase(null, null, "something", Result = "something", TestName = "Only GDMSESSION set")]
		[TestCase(null, null, null, Result = "", TestName = "Nothing set")]
		[TestCase("X-Cinnamon",
			"/usr/share/gnome:/usr/share/cinnamon:/home/user/.local/share/flatpak/exports/share:/var/lib/flatpak/exports/share:/usr/local/share:/usr/share",
			"cinnamon", Result = "x-cinnamon",
			TestName = "Wasta 18.04")]
		[TestCase("ubuntu:GNOME",
			"/usr/share/ubuntu:/home/vagrant/.local/share/flatpak/exports/share:/var/lib/flatpak/exports/share:/usr/local/share/:/usr/share/:/var/lib/snapd/desktop",
			"ubuntu", ExpectedResult = "gnome", TestName = "Ubuntu 20.04 (Gnome Shell)")]
		[TestCase("GNOME-Classic:GNOME",
			"/usr/share/gnome-classic:/home/vagrant/.local/share/flatpak/exports/share:/var/lib/flatpak/exports/share:/usr/local/share/:/usr/share/:/var/lib/snapd/desktop",
			"gnome-classic", ExpectedResult = "gnome", TestName = "Ubuntu 20.04 (Gnome Classic)")]
		[TestCase("GNOME-Flashback:GNOME",
			"/usr/share/gnome-flashback-metacity:/home/vagrant/.local/share/flatpak/exports/share:/var/lib/flatpak/exports/share:/usr/local/share/:/usr/share/:/var/lib/snapd/desktop",
			"gnome-flashback-metacity", ExpectedResult = "gnome", TestName = "Ubuntu 20.04 (Gnome Flashback)")]
		[TestCase("ubuntu:GNOME", null, "ubuntu-wayland", ExpectedResult = "gnome",
			TestName = "Ubuntu 20.04 (Gnome + Wayland)")]
		[TestCase("X-Cinnamon", null, "cinnamon", ExpectedResult = "x-cinnamon",
			TestName = "Wasta 20 (Cinnamon)")]
		[TestCase("ubuntu:GNOME", null, "ubuntu", ExpectedResult = "gnome", TestName = "Wasta 20 (Gnome Shell)")]
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
		[TestCase("Unity", null, "ubuntu", null, Result = "unity (ubuntu)", TestName = "Unity")]
		[TestCase("Unity", "/usr/share/ubuntu:/usr/share/gnome:/usr/local/share/:/usr/share/",
			"ubuntu", null, Result = "unity (ubuntu)", TestName = "Unity with dataDir")]
		[TestCase("Unity", null, "ubuntu", "session-1",
			Result = "unity (ubuntu [display server: Mir])", TestName = "Unity with Mir")]
		[TestCase("GNOME", null, "gnome-shell", null, Result = "gnome (gnome-shell)",
			TestName = "Gnome shell")]
		[TestCase("GNOME", null, "cinnamon", null, Result = "cinnamon (cinnamon)",
			TestName = "Wasta 12")]
		[TestCase("x-cinnamon", null, "cinnamon", null, Result = "x-cinnamon (cinnamon)",
			TestName = "Wasta 14")]
		[TestCase(null, "/usr/share/ubuntu:/usr/share/kde:/usr/local/share/:/usr/share/",
			"kde-plasma", null, Result = "kde (kde-plasma)", TestName = "KDE on Ubuntu 12_04")]
		[TestCase(null, null, null, null, Result = " (not set)", TestName = "Nothing set")]
		[TestCase("ubuntu:GNOME", null, "ubuntu", "", ExpectedResult = "gnome (ubuntu)", TestName = "Ubuntu 20.04 (Gnome)")]
		[TestCase("ubuntu:GNOME", null, "ubuntu-wayland", "",
			ExpectedResult = "gnome (ubuntu [display server: Wayland])", TestName = "Ubuntu 20.04 (Gnome + Wayland)")]
		[TestCase("X-Cinnamon", null, "cinnamon", "", ExpectedResult = "x-cinnamon (cinnamon)", TestName = "Wasta 20 (Cinnamon)")]
		[TestCase("ubuntu:GNOME", null, "ubuntu", "", ExpectedResult = "gnome (ubuntu)", TestName = "Wasta 20 (Gnome)")]
		[TestCase("ubuntu:GNOME",
			"/app/share:/usr/share:/usr/share/runtime/share:/run/host/user-share:/run/host/share",
			"ubuntu", null, "org.example.MyApp", ExpectedResult = "gnome (ubuntu [container: flatpak])",
			TestName = "Ubuntu 20.04 (Gnome) in Flatpak")]
		public string DesktopEnvironmentInfoString_SimulateDesktopEnvironments(string currDesktop,
			string dataDirs, string gdmSession, string mirServerName, string flatpakId = null)
		{
			// See http://askubuntu.com/a/227669 for actual values on different systems

			// Setup
			Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", currDesktop);
			Environment.SetEnvironmentVariable("XDG_DATA_DIRS", dataDirs);
			Environment.SetEnvironmentVariable("GDMSESSION", gdmSession);
			Environment.SetEnvironmentVariable("MIR_SERVER_NAME", mirServerName);
			Environment.SetEnvironmentVariable("FLATPAK_ID", flatpakId);

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
		[Platform(Include = "Windows", Reason = "Windows specific test")]
		public void IsCinnamon_Windows()
		{
			Assert.That(Platform.IsCinnamon, Is.False);
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		// In flatpak
		[TestCase("org.example.MyApp", ExpectedResult = true)]
		// Not in flatpak
		[TestCase(null, ExpectedResult = false)]
		public bool IsFlatpak(string flatpakId)
		{
			// Setup
			Environment.SetEnvironmentVariable("FLATPAK_ID", flatpakId);

			// SUT
			return Platform.IsFlatpak;
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		// Ubuntu 20.04 Gnome Shell
		[TestCase("ubuntu:GNOME", ExpectedResult = true)]
		// Ubuntu 20.04 Gnome Classic
		[TestCase("GNOME-Classic:GNOME", ExpectedResult = true)]
		// Ubuntu 20.04 Gnome Flashback
		[TestCase("GNOME-Flashback:GNOME", ExpectedResult = false)]
		public bool IsGnomeShell(string xdgCurrentDesktop)
		{
			Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", xdgCurrentDesktop);
			// SUT
			return Platform.IsGnomeShell;
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		// Ubuntu 20.04 Gnome Shell
		[TestCase("ubuntu:GNOME", ExpectedResult = false)]
		// Ubuntu 20.04 Gnome Classic
		[TestCase("GNOME-Classic:GNOME", ExpectedResult = true)]
		// Ubuntu 20.04 Gnome Flashback
		[TestCase("GNOME-Flashback:GNOME", ExpectedResult = false)]
		public bool IsGnomeClassic(string xdgCurrentDesktop)
		{
			Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", xdgCurrentDesktop);
			// SUT
			return Platform.IsGnomeClassic;
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		// Ubuntu 20.04 Gnome Shell
		[TestCase("ubuntu:GNOME", ExpectedResult = false)]
		// Ubuntu 20.04 Gnome Classic
		[TestCase("GNOME-Classic:GNOME", ExpectedResult = false)]
		// Ubuntu 20.04 Gnome Flashback
		[TestCase("GNOME-Flashback:GNOME", ExpectedResult = true)]
		public bool IsGnomeFlashback(string xdgCurrentDesktop)
		{
			Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", xdgCurrentDesktop);
			// SUT
			return Platform.IsGnomeFlashback;
		}

		[Platform(Include = "Linux", Reason = "Linux specific test")]
		[TestCase("org.example.MyApp", ExpectedResult = true, TestName = "In flatpak")]
		[TestCase(null, ExpectedResult = false, TestName = "Not in flatpak")]
		public bool UnixOrMacVersion_ReportsIfFlatpak(string flatpakIdEnv)
		{
			Environment.SetEnvironmentVariable("FLATPAK_ID", flatpakIdEnv);
			// SUT
			string result = Platform.UnixOrMacVersion();

			return result.Contains("Flatpak");
		}
	}
}

