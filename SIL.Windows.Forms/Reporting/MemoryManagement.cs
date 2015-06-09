// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using L10NSharp;
using SIL.PlatformUtilities;
using SIL.Reporting;
#if !__MonoCS__ // This is not needed on Linux, and can causes extra dependancy trouble if it is compiled in
using Microsoft.VisualBasic.Devices;
#else
using System.IO;
using System.Text.RegularExpressions;
#endif

namespace SIL.Windows.Forms.Reporting
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
			// the best available approximation of the number of bytes currently allocated in managed memory.
			var heapMem = GC.GetTotalMemory(true); // first, as it may reduce other numbers
			var is64BitProcess = IntPtr.Size == 8; // according to MSDN
			long memorySize64;
			long workingSet64;
			string message;

			var memInfo = GetMemoryInformation();

			using (var proc = Process.GetCurrentProcess())
			{
				// the current size of the process memory that cannot be shared with other processes.
				memorySize64 = proc.PrivateMemorySize64;
				// the current size of the process memory currently in physical RAM memory.
				workingSet64 = proc.WorkingSet64;
				// the current size of all memory used by the process, split between pages loaded in physical memory and pages stored on disk.
				long virtualMemory64 = proc.VirtualMemorySize64;
				// the maximum amount of physical memory allocated for the process since it was started.
				long peakWorkingSet64 = proc.PeakWorkingSet64;
				// the maximum amount of virtual memory allocated for the process since it was started.
				long peakVirtualMemory64 = proc.PeakVirtualMemorySize64;
				StringBuilder bldr = new StringBuilder();
				bldr.AppendFormat("{0}:", eventDescription);
				bldr.AppendFormat(" Memory Use ({0}-bit process): private {1:N0}K, virtual {2:N0}K,", is64BitProcess ? 64 : 32, memorySize64/1024, virtualMemory64/1024);
				bldr.AppendFormat(" physical {0:N0}K,", workingSet64/1024);
				bldr.AppendFormat(" managed heap {0:N0}K,", heapMem/1024);
				bldr.AppendLine();
				bldr.AppendFormat("        peak virtual {0:N0}K, peak physical {1:N0}K;", peakVirtualMemory64/1024, peakWorkingSet64/1024);
				bldr.AppendFormat(" system virtual {0}K, system physical (RAM) {1:N0}K", (memInfo.TotalVirtualMemory / 1024).ToString("N0"), memInfo.TotalPhysicalMemory / 1024);
				bldr.AppendLine();
				message = bldr.ToString();
			}
			if (minor)
				Logger.WriteMinorEvent(message);
			else
				Logger.WriteEvent(message);
			Debug.Write("DEBUG: " + message);
			// Limit memory below 1GB unless we have a 64-bit process with lots of physical memory.
			// In that case, still limit memory to well under 2GB before warning.
			long safelimit = 1000000000;
			if (is64BitProcess && memInfo.TotalPhysicalMemory >= 8192000000L)
				safelimit = 2000000000;
			bool danger;
			// In Windows/.Net, Process.PrivateMemorySize64 seems to give a reasonable value for current
			// memory usage.  In Linux/Mono, Process.PrivateMemorySize64 seems to include a large amount
			// of virtual memory.  In that context, Process.WorkingSet64 appears to be the best bet for
			// approximating how much memory the process is currently using.  (It may be that this would
			// be a better value for Windows as well, but we'll leave it as it is for now.)
			if (Platform.IsWindows)
				danger = memorySize64 > safelimit;
			else
				danger = workingSet64 > safelimit;
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

		public static MemoryInformation GetMemoryInformation()
		{
			var returnVal = new MemoryInformation();

#if !__MonoCS__ // There are completely different dependencies and code per platform for finding the memory information
			var computerInfo = new Computer().Info;
			returnVal.TotalPhysicalMemory = computerInfo.TotalPhysicalMemory;
			returnVal.TotalVirtualMemory = computerInfo.TotalVirtualMemory;
#else
			var meminfo = File.ReadAllText("/proc/meminfo");
			var match = new Regex(@"MemTotal:\s+(\d+) kB").Match(meminfo);
			if (match.Success)
				returnVal.TotalPhysicalMemory = ulong.Parse(match.Groups[1].Value) * 1024;
			ulong totalSwapMemory = 0;
			var match2 = new Regex(@"SwapTotal:\s+(\d+) kB").Match(meminfo);
			if (match2.Success)
				totalSwapMemory = ulong.Parse(match2.Groups [1].Value) * 1024;
			var availableMemory = returnVal.TotalPhysicalMemory + totalSwapMemory;
			var is64BitProcess = IntPtr.Size == 8; // according to MSDN
			if (is64BitProcess)
			{
				returnVal.TotalVirtualMemory = availableMemory;
			}
			else
			{
				// Googling indicates that 32-bit Mono programs attempting to allocate more than
				// about 1.4 GB start running into OutOfMemory errors.  So 2GB is probably the
				// right virtual memory limit for 32-bit processes.
				const ulong twoGB = 2147483648L;
				returnVal.TotalVirtualMemory = availableMemory > twoGB ? twoGB : availableMemory;
			}
#endif
			return returnVal;
		}
	}

	public struct MemoryInformation
	{
		public ulong TotalPhysicalMemory;
		public ulong TotalVirtualMemory;
	}
}
