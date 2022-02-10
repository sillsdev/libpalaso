// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
// Parts based on code by MJ Hutchinson http://mjhutchinson.com/journal/2010/01/25/integrating_gtk_application_mac
// Parts based on code by bugsnag-dotnet (https://github.com/bugsnag/bugsnag-dotnet/blob/v1.4/src/Bugsnag/Diagnostics.cs)

using System;
using System.Diagnostics;
#if !NETSTANDARD2_0
using System.Management;
#endif
using System.Runtime.InteropServices;

namespace SIL.PlatformUtilities
{
	public static class Platform
	{
		private static bool? _isMono;
		private static string _sessionManager;

		public static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix;
		public static bool IsWasta => IsUnix && System.IO.File.Exists("/etc/wasta-release");

		public static bool IsCinnamon => IsUnix &&
			(SessionManager.StartsWith("/usr/bin/cinnamon-session") ||
			Environment.GetEnvironmentVariable("GDMSESSION") == "cinnamon");

		public static bool IsMono
		{
			get
			{
				if (_isMono == null)
					_isMono = Type.GetType("Mono.Runtime") != null;

				return (bool)_isMono;
			}
		}
		public static bool IsDotNet => !IsMono;

#if NETSTANDARD2_0
		public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		public static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		public static bool IsDotNetCore => RuntimeInformation.FrameworkDescription == ".NET Core";
		public static bool IsDotNetFramework => IsDotNet && RuntimeInformation.FrameworkDescription == ".NET Framework";
#elif NET461
		private static readonly string UnixNameMac = "Darwin";
		private static readonly string UnixNameLinux = "Linux";

		public static bool IsLinux => IsUnix && UnixName == UnixNameLinux;
		public static bool IsMac => IsUnix && UnixName == UnixNameMac;
		public static bool IsWindows => !IsUnix;

		public static bool IsDotNetCore => false;
		public static bool IsDotNetFramework => IsDotNet;

		private static string _unixName;
		private static string UnixName
		{
			get
			{
				if (_unixName == null)
				{
					IntPtr buf = IntPtr.Zero;
					try
					{
						buf = Marshal.AllocHGlobal(8192);
						// This is a hacktastic way of getting sysname from uname ()
						if (uname(buf) == 0)
							_unixName = Marshal.PtrToStringAnsi(buf);
					}
					catch
					{
						_unixName = String.Empty;
					}
					finally
					{
						if (buf != IntPtr.Zero)
							Marshal.FreeHGlobal(buf);
					}
				}

				return _unixName;
			}
		}

		[DllImport("libc")]
		private static extern int uname(IntPtr buf);
#endif

		/// <summary>
		/// On a Unix machine this gets the current desktop environment (gnome/xfce/...), on
		/// non-Unix machines the platform name.
		/// </summary>
		public static string DesktopEnvironment
		{
			get
			{
				if (!IsUnix)
					return Environment.OSVersion.Platform.ToString();

				// see http://unix.stackexchange.com/a/116694
				// and http://askubuntu.com/a/227669
				string currentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
				if (string.IsNullOrEmpty(currentDesktop))
				{
					var dataDirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
					if (dataDirs != null)
					{
						dataDirs = dataDirs.ToLowerInvariant();
						if (dataDirs.Contains("xfce"))
							currentDesktop = "XFCE";
						else if (dataDirs.Contains("kde"))
							currentDesktop = "KDE";
						else if (dataDirs.Contains("gnome"))
							currentDesktop = "Gnome";
					}
					if (string.IsNullOrEmpty(currentDesktop))
						currentDesktop = Environment.GetEnvironmentVariable("GDMSESSION") ?? string.Empty;
				}
				// Special case for Wasta 12
				else if (currentDesktop == "GNOME" && Environment.GetEnvironmentVariable("GDMSESSION") == "cinnamon")
					currentDesktop = Environment.GetEnvironmentVariable("GDMSESSION");
				else if (currentDesktop.ToLowerInvariant() == "ubuntu:gnome")
					currentDesktop = "gnome";
				else if (currentDesktop.ToLowerInvariant() == "gnome-classic:gnome")
				{
					currentDesktop = "gnome";
				}
				else if (currentDesktop.ToLowerInvariant() == "gnome-flashback:gnome")
				{
					currentDesktop = "gnome";
				}
				return currentDesktop?.ToLowerInvariant();
			}
		}

		/// <summary>
		/// Get the currently running desktop environment (like Unity, Gnome shell etc)
		/// </summary>
		public static string DesktopEnvironmentInfoString
		{
			get
			{
				if (!IsUnix)
					return string.Empty;

				// see http://unix.stackexchange.com/a/116694
				// and http://askubuntu.com/a/227669
				var currentDesktop = DesktopEnvironment;
				var additionalInfo = string.Empty;
				var mirSession = Environment.GetEnvironmentVariable("MIR_SERVER_NAME");
				if (!string.IsNullOrEmpty(mirSession))
					additionalInfo = " [display server: Mir]";

				var gdmSession = Environment.GetEnvironmentVariable("GDMSESSION") ?? "not set";
				if (gdmSession.ToLowerInvariant().EndsWith("-wayland"))
					return $"{currentDesktop} ({gdmSession.Split('-')[0]} [display server: Wayland])";

				if (Platform.IsFlatpak)
				{
					additionalInfo += " [container: flatpak]";
				}

				return $"{currentDesktop} ({gdmSession}{additionalInfo})";
			}
		}

		private static string SessionManager
		{
			get
			{
				if (_sessionManager == null)
				{
					IntPtr buf = IntPtr.Zero;
					try
					{
						// This is the only way I've figured out to get the session manager: read the
						// symbolic link destination value.
						buf = Marshal.AllocHGlobal(8192);
						var len = readlink("/etc/alternatives/x-session-manager", buf, 8192);
						if (len > 0)
						{
							// For some reason, Marshal.PtrToStringAnsi() sometimes returns null in Mono.
							// Copying the bytes and then converting them to a string avoids that problem.
							// Filenames are likely to be in UTF-8 on Linux if they are not pure ASCII.
							var bytes = new byte[len];
							Marshal.Copy(buf, bytes, 0, len);
							_sessionManager = System.Text.Encoding.UTF8.GetString(bytes);
						}
						if (_sessionManager == null)
							_sessionManager = string.Empty;
					}
					catch
					{
						_sessionManager = string.Empty;
					}
					finally
					{
						if (buf != IntPtr.Zero)
							Marshal.FreeHGlobal(buf);
					}
				}
				return _sessionManager;
			}
		}

		[DllImport("libc")]
		private static extern int readlink(string path, IntPtr buf, int bufsiz);

		[DllImport("__Internal", EntryPoint = "mono_get_runtime_build_info")]
		private static extern string GetMonoVersion();

		/// <summary>
		/// Gets the version of the currently running Mono (e.g.
		/// "5.0.1.1 (2017-02/5077205 Thu May 25 09:16:53 UTC 2017)"), or the empty string
		/// on Windows.
		/// </summary>
		public static string MonoVersion => IsMono ? GetMonoVersion() : string.Empty;

		/// <summary>
		/// Gets a string that describes the OS, e.g. 'Windows 10' or 'Ubuntu 16.04 LTS'
		/// </summary>
		/// <remarks>Note that you might have to add an app.config file to your executable
		/// that lists the supported Windows versions in order to get the correct Windows version
		/// reported (https://msdn.microsoft.com/en-us/library/windows/desktop/aa374191.aspx)!
		/// </remarks>
		public static string OperatingSystemDescription
		{
			get
			{
				switch (Environment.OSVersion.Platform)
				{
					// Platform is Windows 95, Windows 98, Windows 98 Second Edition,
					// or Windows Me.
					case PlatformID.Win32Windows:
						// Platform is Windows 95, Windows 98, Windows 98 Second Edition,
						// or Windows Me.
						switch (Environment.OSVersion.Version.Minor)
						{
							case 0:
								return "Windows 95";
							case 10:
								return "Windows 98";
							case 90:
								return "Windows Me";
							default:
								return "UNKNOWN";
						}
					case PlatformID.Win32NT:
						return GetWin32NTVersion();
					case PlatformID.Unix:
					case PlatformID.MacOSX:
						return UnixOrMacVersion();
					default:
						return "UNKNOWN";
				}
			}
		}

		/// <summary>
		/// Detects the current operating system version if its Win32 NT
		/// </summary>
		/// <returns>The operation system version</returns>
		private static string GetWin32NTVersion()
		{
			switch (Environment.OSVersion.Version.Major)
			{
				case 3:
					return "Windows NT 3.51";
				case 4:
					return "Windows NT 4.0";
				case 5:
					return Environment.OSVersion.Version.Minor == 0 ? "Windows 2000" : "Windows XP";
				case 6:
					switch (Environment.OSVersion.Version.Minor)
					{
						case 0:
							return "Windows Server 2008";
						case 1:
							return IsWindowsServer ? "Windows Server 2008 R2" : "Windows 7";
						case 2:
							return IsWindowsServer ? "Windows Server 2012" : "Windows 8";
						case 3:
							return IsWindowsServer ? "Windows Server 2012 R2" : "Windows 8.1";
						default:
							return "UNKNOWN";
					}
				case 10:
					return "Windows 10";
				default:
					return "UNKNOWN";
			}
		}

		// https://stackoverflow.com/a/3138781/487503
		private static bool IsWindowsServer => IsOS(OS_ANYSERVER);

		private const int OS_ANYSERVER = 29;

		[DllImport("shlwapi.dll", SetLastError=true)]
		private static extern bool IsOS(int os);

		/// <summary>
		/// Determines the OS version if on a UNIX based system
		/// </summary>
		/// <returns></returns>
		internal static string UnixOrMacVersion()
		{
			if (RunTerminalCommand("uname") == "Darwin")
			{
				var osName = RunTerminalCommand("sw_vers", "-productName");
				var osVersion = RunTerminalCommand("sw_vers", "-productVersion");
				return osName + " (" + osVersion + ")";
			}

			string unameCommand = "uname --kernel-name --kernel-release --kernel-version --machine";
			var distro = RunTerminalCommand("bash", $"-c 'which lsb_release >/dev/null && [ -f /etc/wasta-release ] && echo \"$(lsb_release -d -s) ($(cat /etc/wasta-release | grep DESCRIPTION | cut -d\\\" -f 2))\" || lsb_release -d -s 2>/dev/null || {unameCommand}'");
			if (IsFlatpak)
			{
				// Flatpak may not have lsb_release and can fall back to uname. But try to report the host distro.
				string extra = Environment.GetEnvironmentVariable("XDG_SESSION_DESKTOP")
					?? Environment.GetEnvironmentVariable("DESKTOP_SESSION")
					?? Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
				return $"Flatpak ({extra} {distro})";
			}
			return string.IsNullOrEmpty(distro) ? "UNIX" : distro;
		}

		/// <summary>
		/// Executes a command with arguments, used to send terminal commands in UNIX systems
		/// </summary>
		/// <param name="cmd">The command to send</param>
		/// <param name="args">The arguments to send</param>
		/// <returns>The returned output</returns>
		private static string RunTerminalCommand(string cmd, string args = null)
		{
			var proc = new Process {
				EnableRaisingEvents = false,
				StartInfo = {
					FileName = cmd,
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true
				}
			};
			proc.Start();
			proc.WaitForExit();
			var output = proc.StandardOutput.ReadToEnd();
			return output.Trim();
		}

		/// <summary>
		/// Is the software running in the GNOME Shell desktop environment?
		/// </summary>
		public static bool IsGnomeShell
		{
			get
			{
				if (!IsLinux)
					return false;
				if (Platform.DesktopEnvironment == "gnome" && !IsGnomeClassic && !IsGnomeFlashback)
				{
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Is the software running in the GNOME Classic desktop environment?
		/// </summary>
		public static bool IsGnomeClassic
		{
			get
			{
				return Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") == "GNOME-Classic:GNOME";
			}
		}


		/// <summary>
		/// Is the software running in the GNOME Flashback desktop environment?
		/// </summary>
		public static bool IsGnomeFlashback
		{
			get
			{
				return Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") == "GNOME-Flashback:GNOME";
			}
		}

		/// <summary>
		/// Is the software running in a flatpak container?
		/// </summary>
		public static bool IsFlatpak
		{
			get
			{
				return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FLATPAK_ID"));
			}
		}
	}
}
