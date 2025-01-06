// Copyright (c) 2014-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;

namespace SIL.Extensions
{
	public static class ProcessExtensions
	{
		/// <summary>
		/// Run program with arguments, calling errorHandler() if there was an error such as
		/// the program not being found to run.
		/// </summary>
		/// <returns>launched Process or null</returns>
		public static Process RunProcess(this Process process, string program, string arguments,
			Action<Exception> errorHandler, bool redirectStdError = false)
		{
			try
			{
				var processInfo = new ProcessStartInfo(program, arguments) {
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = redirectStdError
				};
				process.StartInfo = processInfo;
				process.Start();
			}
			catch (Exception e)
			{
				errorHandler?.Invoke(e);
			}
			return process;
		}

	}
}

