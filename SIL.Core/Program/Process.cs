// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
//
using System;

namespace SIL.Program
{
	public static class Process
	{
		/// <summary>
		/// Process.Start() obeys the current LD_LIBRARY_PATH on Linux.  This can have unfortunate
		/// effects for some programs, such as preventing web sites from displaying if the default
		/// browser is chromium.  This method ensures that environment variable is cleared (its
		/// normal state) before invoking Process.Start(), then restores it for the sake of the rest
		///  of the program.
		/// </summary>
		/// <remarks>
		/// I suppose this could be fixed by a change to System.Diagnostics.Process.Start(), but
		/// I would argue that the current behavior is probaby by design and a change would be
		/// rejected by the Mono project.
		/// See https://silbloom.myjetbrains.com/youtrack/issue/BL-5993 for the bug report that
		/// triggered this class and method.
		/// </remarks>
		public static void SafeStart(string urlOrCmd)
		{
			var libpath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
			if (!String.IsNullOrEmpty(libpath))
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", null);

			System.Diagnostics.Process.Start(urlOrCmd);

			if (!String.IsNullOrEmpty(libpath))
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", libpath);
		}
	}
}
