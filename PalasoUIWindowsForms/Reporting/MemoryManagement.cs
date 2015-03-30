using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using L10NSharp;
using Microsoft.VisualBasic.Devices;
using Palaso.PlatformUtilities;
using Palaso.Reporting;

namespace Palaso.UI.WindowsForms.Reporting
{
	/// <summary>
	/// This class contains methods useful for logging and warning the user about possible problems
	/// of memory shortage.
	/// </summary>
	public static class MemoryManagement
	{
		private static bool s_warningShown;

		/// <summary>
		/// Write to the logger a message containing the event string, the total memory this process is using,
		/// its working set (real memory), .NET heap memory, and the total physical and virtual memory on the system
		/// If botherUser is true and total memory in use is more than 1G, it will display a dialog saying
		/// "This program is using a lot of memory, and may soon slow down or experience other problems.
		/// We recommend that you quit and restart it when convenient." This will happen only once per session.
		/// Enhance: (Do we need a way to turn it off altogether? Should we have an argument whereby the threshold can be controlled?)
		/// </summary>
		/// <param name="minor">True to write a minor event, false to write full event</param>
		/// <param name="eventDescription"></param>
		/// <param name="okToBotherUser">The first time it is called with this true and short of memory, display a dialog.</param>
		/// <returns>true if total memory in use is more than 1G (whether or not we bothered the user)</returns>
		public static bool CheckMemory(bool minor, string eventDescription, bool okToBotherUser)
		{
			var heapMem = GC.GetTotalMemory(true); // first, as it may reduce other numbers
			long memorySize64;
			string message;
			ulong totalPhysicalMemory = 0;
			string totalVirtualMemory = "unknown";
			if (Platform.IsWindows)
			{
				var computerInfo = new Computer().Info;
				totalPhysicalMemory = computerInfo.TotalPhysicalMemory;
				totalVirtualMemory = (computerInfo.TotalVirtualMemory / 1024).ToString("N0");
			}
			else if (Platform.IsLinux)
			{
				var meminfo = File.ReadAllText("/proc/meminfo");
				var match = new Regex(@"MemTotal:\s+(\d+) kB").Match(meminfo);
				if (match.Success)
				{
					totalPhysicalMemory = ulong.Parse(match.Groups[1].Value) * 1024;
				}
				// So far we have no way to get anything corresponding to Windows's idea of the total virtual memory
				// the process can use.
			}
			using (var proc = Process.GetCurrentProcess())
			{
				memorySize64 = proc.PrivateMemorySize64;
				message =
					string.Format(
						"{0}: total memory {1:N0}K, working set {2:N0}K, heap {3:N0}K, system physical {4:N0}K, system virtual {5}K",
						eventDescription,
						memorySize64/1024,
						proc.WorkingSet64/1024,
						heapMem/1024,
						totalPhysicalMemory/1024,
						totalVirtualMemory);
			}
			if (minor)
				Logger.WriteMinorEvent(message);
			else
				Logger.WriteEvent(message);
			var danger = memorySize64 > 1000000000;
			if (danger && okToBotherUser && !s_warningShown)
			{
				s_warningShown = true;
				var warning = LocalizationManager.GetString("MemoryWarning",
					"Unfortunately, {0} is starting to get short of memory, and may soon slow down or experience other problems. We recommend that you quit and restart it when convenient.");
				var caption = LocalizationManager.GetString("Warning", "Warning");
				MessageBox.Show(null, string.Format(warning, Application.ProductName), caption, MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
			return danger;
		}
	}
}
