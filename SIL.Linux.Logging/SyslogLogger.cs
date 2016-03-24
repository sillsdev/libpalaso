using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Unix.Native;
using SIL.Reporting;

namespace SIL.Linux.Logging
{
	public class SyslogLogger
	{
		/// <summary>
		/// Application name to log into syslog
		/// </summary>
		public string AppName { get; set; }

		/// <summary>
		/// Facility to log as (default SyslogFacility.User, which is almost always what you want).
		/// <para />
		/// Change this to send the log to a different syslog facility depending on the system configuration.
		/// E.g., if mail log messages are sent to /var/log/mail.log, then setting this to SyslogFacility.Mail
		/// </summary>
		public SyslogFacility Facility { get; set; }

		/// <summary>
		/// Syslog options (e.g., LOG_PID). Exposed so that callers can, if desired, log to console and/or stderr
		/// in addition to logging to syslog, by setting the appropriate flags.
		/// </summary>
		public SyslogOption Options { get; set; }

		/// <summary>
		/// Build new SyslogLogger with specified application name, facility, and/or options. (All are optional parameters).
		/// </summary>
		/// <param name="appName">Application name to use in syslog (omit to use default value, UsageReporter.AppNameToUseInReporting)</param>
		/// <param name="facility">Syslog facility (can usually omit to use default value, SyslogFacility.User)</param>
		/// <param name="options">Syslog option flags (can usually omit to use default value, SyslogOption.LogPid). If
		/// setting this to values other than the default, including the LogPid flag is HIGHLY recommended.</param>
		public SyslogLogger(string appName = null, SyslogFacility facility = SyslogFacility.User, SyslogOption options = SyslogOption.LogPid)
		{
			AppName = String.IsNullOrEmpty(appName) ? GetDefaultAppName() : appName;
			Facility = facility;
			Options = options;
		}

		/// <summary>
		/// Get application name if none specified (defaults to UsageReporter.AppNameToUseInReporting).
		/// </summary>
		/// <returns>String representing the application name to use in logging to syslog.</returns>
		private string GetDefaultAppName()
		{
			return UsageReporter.AppNameToUseInReporting;
		}

		#region Interop functions (C# calls)

		/// <summary>
		/// Open a connection to /dev/log. IMPORTANT: You must keep the IntPtr handle returned by this function,
		/// and dispose of it by calling Closelog(), otherwise a memory leak or segfault could result.
		/// </summary>
		/// <param name="ident">The application name to use in the syslog messages</param>
		/// <returns></returns>
		private IntPtr Openlog(string ident = null)
		{
			if (String.IsNullOrEmpty(ident))
				ident = AppName;
			IntPtr marshalledAppName = MarshalStringToUtf8WithNullTerminator(ident);
			Syscall.openlog(marshalledAppName, (Mono.Unix.Native.SyslogOptions)Options, (Mono.Unix.Native.SyslogFacility)Facility);
			return marshalledAppName;
		}

		/// <summary>
		/// Sends a message to the syslog previously opened by Openlog(). If called without calling Openlog() first,
		/// the app name in the syslog will be "mono", which is usually not what you want. If you want String.Format()
		/// style parameters, use the Alert/Critical/Error/etc convenience functions.
		/// <para />
		/// Note that the libc_syslog() call expects a UTF-8 encoded string with no BOM, but Syslog() will take care
		/// of encoding your string for you.
		/// </summary>
		/// <param name="priority">Priority of the log message (a SyslogPriority enum value)</param>
		/// <param name="message">The C# string to be logged</param>
		private void Syslog(SyslogPriority priority, string message)
		{
			Syscall.syslog((Mono.Unix.Native.SyslogFacility)Facility, (Mono.Unix.Native.SyslogLevel)priority, message);
		}

		/// <summary>
		/// Closes the logfile and disposes of the handle to previously marshalled AppName string.
		/// </summary>
		/// <param name="marshalledAppName"></param>
		private void Closelog(ref IntPtr marshalledAppName)
		{
			SafelyDisposeOfMarshalledHandle(ref marshalledAppName);
			Syscall.closelog();
		}

		#endregion

		/// <summary>
		/// Send a single message to syslog, handling Openlog() and Closelog() properly.
		/// </summary>
		/// <param name="priority"></param>
		/// <param name="message"></param>
		public void Log(SyslogPriority priority, string message)
		{
			IntPtr handle = Openlog();
			try
			{
				Syslog(priority, message);
			}
			finally
			{
				Closelog(ref handle);
			}
		}

		/// <summary>
		/// For efficiency, if you have many messages to log with the same priority, LogMany() will log them all
		/// with only a single Openlog() / Closelog() call pair.
		/// </summary>
		/// <param name="priority"></param>
		/// <param name="messages"></param>
		public void LogMany(SyslogPriority priority, IEnumerable<string> messages)
		{
			IntPtr handle = Openlog();
			try
			{ 
				foreach (string message in messages)
					Syslog(priority, message);
			}
			finally
			{
				Closelog(ref handle);
			}
		}

		#region Convenience functions

		/// <summary>
		/// Log a message with severity SyslogPriority.Alert, using String.Format to create the message.
		/// </summary>
		/// <param name="messageFormat">Format string, as in String.Format</param>
		/// <param name="messageParts">Remainder of arguments to String.Format</param>
		public void Alert(string messageFormat, params object[] messageParts)
		{
			Log(SyslogPriority.Alert, String.Format(messageFormat, messageParts));
		}

		/// <summary>
		/// Log a message with severity SyslogPriority.Critical, using String.Format to create the message.
		/// </summary>
		/// <param name="messageFormat">Format string, as in String.Format</param>
		/// <param name="messageParts">Remainder of arguments to String.Format</param>
		public void Critical(string messageFormat, params object[] messageParts)
		{
			Log(SyslogPriority.Critical, String.Format(messageFormat, messageParts));
		}

		/// <summary>
		/// Log a message with severity SyslogPriority.Debug, using String.Format to create the message.
		/// </summary>
		/// <param name="messageFormat">Format string, as in String.Format</param>
		/// <param name="messageParts">Remainder of arguments to String.Format</param>
		public void Debug(string messageFormat, params object[] messageParts)
		{
			Log(SyslogPriority.Debug, String.Format(messageFormat, messageParts));
		}

		/// <summary>
		/// Log a message with severity SyslogPriority.Emergency, using String.Format to create the message.
		/// </summary>
		/// <param name="messageFormat">Format string, as in String.Format</param>
		/// <param name="messageParts">Remainder of arguments to String.Format</param>
		public void Emergency(string messageFormat, params object[] messageParts)
		{
			Log(SyslogPriority.Emergency, String.Format(messageFormat, messageParts));
		}

		/// <summary>
		/// Log a message with severity SyslogPriority.Error, using String.Format to create the message.
		/// </summary>
		/// <param name="messageFormat">Format string, as in String.Format</param>
		/// <param name="messageParts">Remainder of arguments to String.Format</param>
		public void Error(string messageFormat, params object[] messageParts)
		{
			Log(SyslogPriority.Error, String.Format(messageFormat, messageParts));
		}

		/// <summary>
		/// Log a message with severity SyslogPriority.Info, using String.Format to create the message.
		/// </summary>
		/// <param name="messageFormat">Format string, as in String.Format</param>
		/// <param name="messageParts">Remainder of arguments to String.Format</param>
		public void Info(string messageFormat, params object[] messageParts)
		{
			Log(SyslogPriority.Info, String.Format(messageFormat, messageParts));
		}

		/// <summary>
		/// Log a message with severity SyslogPriority.Notice, using String.Format to create the message.
		/// </summary>
		/// <param name="messageFormat">Format string, as in String.Format</param>
		/// <param name="messageParts">Remainder of arguments to String.Format</param>
		public void Notice(string messageFormat, params object[] messageParts)
		{
			Log(SyslogPriority.Notice, String.Format(messageFormat, messageParts));
		}

		/// <summary>
		/// Log a message with severity SyslogPriority.Warning, using String.Format to create the message.
		/// </summary>
		/// <param name="messageFormat">Format string, as in String.Format</param>
		/// <param name="messageParts">Remainder of arguments to String.Format</param>
		public void Warning(string messageFormat, params object[] messageParts)
		{
			Log(SyslogPriority.Warning, String.Format(messageFormat, messageParts));
		}

		#endregion

		/// <summary>
		/// Marshal a string to UTF-8, with a null terminating byte (a '\0' character)
		/// </summary>
		/// <param name="s">C# string to marshal to a UTF-8 encoded</param>
		/// <returns>IntPtr handle to the null-terminated UTF-8 string</returns>
		private static IntPtr MarshalStringToUtf8WithNullTerminator(string s)
		{
			// Can't use Marshal.StringToHGlobalAnsi as it's hardcoded to use the current codepage (which will mangle UTF-8)
			byte[] encodedBytes = GetUtf8BytesWithNullTerminator(s);
			IntPtr handle = Marshal.AllocHGlobal(encodedBytes.Length);
			Marshal.Copy(encodedBytes, 0, handle, encodedBytes.Length);
			return handle;
		}

		/// <summary>
		/// Free a marshalled handle if (and only if) it hasn't been freed before.
		/// Enforces the "only free a handle once" by setting it to IntPtr.Zero after
		/// freeing it, and requiring callers to pass the handle as a ref parameter so
		/// that they know it's going to be set to IntPtr.Zero.
		/// </summary>
		/// <param name="marshalledHandle"></param>
		private static void SafelyDisposeOfMarshalledHandle(ref IntPtr marshalledHandle)
		{
			if (marshalledHandle != IntPtr.Zero)
				Marshal.FreeHGlobal(marshalledHandle);
			marshalledHandle = IntPtr.Zero;
		}

		/// <summary>
		/// Convert a string to UTF-8 with a null terminating byte (a '\0' character) and no BOM.
		/// </summary>
		/// <param name="s">The string to encode</param>
		/// <returns>Byte array containing the UTF-8 encoding of s, with a \0 terminator</returns>
		private static byte[] GetUtf8BytesWithNullTerminator(string s)
		{
			UTF8Encoding utf8 = new UTF8Encoding(false);
			int byteCount = utf8.GetByteCount(s) + 1; // Need 1 extra byte for the '\0' terminator
			byte[] encodedBytes = new byte[byteCount];
			encodedBytes[byteCount - 1] = 0; // Not actually needed since C# zero-fills allocated memory, but be safe anyway
			utf8.GetBytes(s, 0, s.Length, encodedBytes, 0);
			return encodedBytes;
		}
	}
}
