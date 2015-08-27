// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
// Parts based on code by MJ Hutchinson http://mjhutchinson.com/journal/2010/01/25/integrating_gtk_application_mac
using System;

namespace Palaso.PlatformUtilities
{
	public static class Platform
	{
		private static readonly string UnixNameMac = "Darwin";
		private static readonly string UnixNameLinux = "Linux";
		private static bool? m_isMono;
		private static string m_unixName;
		private static string m_sessionManager;

		public static bool IsUnix
		{
			get { return Environment.OSVersion.Platform == PlatformID.Unix; }
		}

		public static bool IsLinux
		{
			get { return IsUnix && (UnixName == UnixNameLinux); }
		}

		public static bool IsMac
		{
			get { return IsUnix && (UnixName == UnixNameMac); }
		}

		public static bool IsWindows
		{
			get { return !IsUnix; }
		}

		public static bool IsMono
		{
			get
			{
				if (m_isMono == null)
					m_isMono = Type.GetType("Mono.Runtime") != null;

				return (bool)m_isMono;
			}
		}

		public static bool IsDotNet
		{
			get { return !IsMono; }
		}

		private static string UnixName
		{
			get
			{
				if (m_unixName == null)
				{
					IntPtr buf = IntPtr.Zero;
					try
					{
						buf = System.Runtime.InteropServices.Marshal.AllocHGlobal (8192);
						// This is a hacktastic way of getting sysname from uname ()
						if (uname (buf) == 0)
							m_unixName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi (buf);
					}
					catch
					{
						m_unixName = String.Empty;
					}
					finally {
						if (buf != IntPtr.Zero)
							System.Runtime.InteropServices.Marshal.FreeHGlobal (buf);
					}
				}

				return m_unixName;
			}
		}


		public static bool IsWasta
		{
			get { return IsUnix && System.IO.File.Exists("/etc/wasta-release"); }
		}

		public static bool IsCinnamon
		{
			get { return IsUnix && SessionManager.StartsWith("/usr/bin/cinnamon-session"); }
		}

		/// <summary>
		/// On a Unix machine this gets the current desktop environment (gnome/xfce/...), on
		/// non-Unix machines the platform name.
		/// </summary>
		public static string DesktopEnvironment
		{
			get
			{
				if (!Platform.IsUnix)
					return Environment.OSVersion.Platform.ToString();

				// see http://unix.stackexchange.com/a/116694
				// and http://askubuntu.com/a/227669
				var currentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
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
				return currentDesktop.ToLowerInvariant();
			}
		}

		/// <summary>
		/// Get the currently running desktop environment (like Unity, Gnome shell etc)
		/// </summary>
		public static string DesktopEnvironmentInfoString
		{
			get
			{
				if (!Platform.IsUnix)
					return string.Empty;

				// see http://unix.stackexchange.com/a/116694
				// and http://askubuntu.com/a/227669
				var currentDesktop = Platform.DesktopEnvironment;
				var mirSession = Environment.GetEnvironmentVariable("MIR_SERVER_NAME");
				var additionalInfo = string.Empty;
				if (!string.IsNullOrEmpty(mirSession))
					additionalInfo = " [display server: Mir]";
				var gdmSession = Environment.GetEnvironmentVariable("GDMSESSION") ?? "not set";
				return string.Format("{0} ({1}{2})", currentDesktop, gdmSession, additionalInfo);
			}
		}

		private static string SessionManager
		{
			get
			{
				if (m_sessionManager == null)
				{
					IntPtr buf = IntPtr.Zero;
					try
					{
						// This is the only way I've figured out to get the session manager.
						buf = System.Runtime.InteropServices.Marshal.AllocHGlobal(8192);
						var len = readlink("/etc/alternatives/x-session-manager", buf, 8192);
						if (len > 0)
							m_sessionManager = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buf);
						else
							m_sessionManager = String.Empty;
					}
					catch
					{
						m_sessionManager = String.Empty;
					}
					finally
					{
						if (buf != IntPtr.Zero)
							System.Runtime.InteropServices.Marshal.FreeHGlobal(buf);
					}
				}
				return m_sessionManager;
			}
		}

		[System.Runtime.InteropServices.DllImport ("libc")]
		static extern int uname (IntPtr buf);

		[System.Runtime.InteropServices.DllImport ("libc")]
		static extern int readlink(string path, IntPtr buf, int bufsiz);
	}
}
