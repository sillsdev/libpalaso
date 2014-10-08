// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;

namespace Palaso.Extensions
{
	public static class ProcessExtensions
	{
		/// <summary>
		/// Run program with arguments, calling errorHandler() if there was an error such as
		/// the program not being found to run.
		/// </summary>
		/// <returns>launched Process or null</returns>
		public static Process RunProcess(this Process process, string program, string arguments,
			Action<Exception> errorHandler)
		{
			try
			{
				var processInfo = new ProcessStartInfo(program, arguments);
				processInfo.UseShellExecute = false;
				processInfo.RedirectStandardOutput = true;
				process.StartInfo = processInfo;
				process.Start();
			}
			catch (Exception e)
			{
				if (errorHandler != null)
					errorHandler(e);
			}
			return process;
		}
	}
}

