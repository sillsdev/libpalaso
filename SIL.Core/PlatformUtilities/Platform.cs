// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
// Parts based on code by MJ Hutchinson http://mjhutchinson.com/journal/2010/01/25/integrating_gtk_application_mac
using System;
using System.Runtime.InteropServices;

namespace SIL.PlatformUtilities
{
	public static class Platform
	{
		private static bool? _isMono;
		private static string _sessionManager;

		public static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix;
		public static bool IsWasta => IsUnix && System.IO.File.Exists("/etc/wasta-release");
		public static bool IsCinnamon => IsUnix && SessionManager.StartsWith("/usr/bin/cinnamon-session");

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
		public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
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
				string currentDesktop = DesktopEnvironment;
				string mirSession = Environment.GetEnvironmentVariable("MIR_SERVER_NAME");
				var additionalInfo = string.Empty;
				if (!string.IsNullOrEmpty(mirSession))
					additionalInfo = " [display server: Mir]";
				string gdmSession = Environment.GetEnvironmentVariable("GDMSESSION") ?? "not set";
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
	}
}
