// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
//
using System;

namespace SIL.Program
{
	/// <summary>
	/// Process.Start() obeys the current environment, which includes LD_LIBRARY_PATH on Linux.
	/// This can have unfortunate effects for programs that use geckofx and associated mozilla
	/// code, possibly preventing the program from displaying web sites if firefox is not the
	/// default browser.  The methods in this class ensure that LD_LIBRARY_PATH is cleared (its
	/// normal state) before invoking Process.Start(), then restored for the sake of the rest of
	/// the program.
	/// </summary>
	/// <remarks>
	/// I suppose this could be fixed by a change to System.Diagnostics.Process.Start(), but I
	/// would argue that the current behavior is probably by design and a change would be rejected
	/// by the Mono project.
	/// See https://silbloom.myjetbrains.com/youtrack/issue/BL-5993 for the bug report that
	/// triggered this class and methods.
	/// </remarks>
	public static class Process
	{
		/// <summary>
		/// Safely start the process when the program code merely supplies the URL (or a command).
		/// </summary>
		public static void SafeStart(string urlOrCmd)
		{
			var libpath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
			if (!String.IsNullOrEmpty(libpath))
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", null);

			System.Diagnostics.Process.Start(urlOrCmd);

			if (!String.IsNullOrEmpty(libpath))
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", libpath);
		}

		/// <summary>
		/// Safely start the process when the program code explicitly invokes "xdg-open" (on Linux)
		/// or another command.
		/// </summary>
		public static void SafeStart(string command, string arguments)
		{
			var libpath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
			if (!String.IsNullOrEmpty(libpath))
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", null);

			System.Diagnostics.Process.Start(command, arguments);

			if (!String.IsNullOrEmpty(libpath))
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", libpath);
		}
	}
}
