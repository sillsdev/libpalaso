// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
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

namespace SIL.WindowsForms.Reporting
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
			var is64BitProcess = IntPtr.Size == 8; // according to MSDN
			long memorySize64;
			long workingSet64;
			string message;
			ulong totalPhysicalMemory = 0;
			string totalVirtualMemory = "unknown";

#if !__MonoCS__ // There are completely different dependencies and code per platform for finding the memory information
				var computerInfo = new Computer().Info;
				totalPhysicalMemory = computerInfo.TotalPhysicalMemory;
				totalVirtualMemory = (computerInfo.TotalVirtualMemory / 1024).ToString("N0");
#else
				var meminfo = File.ReadAllText("/proc/meminfo");
				var match = new Regex(@"MemTotal:\s+(\d+) kB").Match(meminfo);
				if (match.Success)
					totalPhysicalMemory = ulong.Parse(match.Groups[1].Value) * 1024;
				ulong totalSwapMemory = 0;
				var match2 = new Regex(@"SwapTotal:\s+(\d+) kB").Match(meminfo);
				if (match2.Success)
					totalSwapMemory = ulong.Parse(match2.Groups [1].Value) * 1024;
				var availableMemory = totalPhysicalMemory + totalSwapMemory;
				if (is64BitProcess)
				{
					totalVirtualMemory = (availableMemory / 1024).ToString ("N0");
				}
				else
				{
					// Googling indicates that 32-bit Mono programs attempting to allocate more than
					// about 1.4 GB start running into OutOfMemory errors.  So 2GB is probably the
					// right virtual memory limit for 32-bit processes.
					ulong twoGB = 2147483648L;
					totalVirtualMemory = ((availableMemory > twoGB ? twoGB : availableMemory) / 1024).ToString("N0");
				}
#endif
			using (var proc = Process.GetCurrentProcess())
			{
				memorySize64 = proc.PrivateMemorySize64;
				workingSet64 = proc.WorkingSet64;
				message =
					string.Format(
						"{0}: total memory {1:N0}K, working set {2:N0}K, heap {3:N0}K, system physical {4:N0}K, system virtual {5}K; {6}-bit process",
						eventDescription,
						memorySize64/1024,
						workingSet64/1024,
						heapMem/1024,
						totalPhysicalMemory/1024,
						totalVirtualMemory,
						is64BitProcess ? 64 : 32);
			}
			if (minor)
				Logger.WriteMinorEvent(message);
			else
				Logger.WriteEvent(message);
			// Limit memory below 1GB unless we have a 64-bit process with lots of physical memory.
			// In that case, still limit memory to well under 2GB before warning.
			long safelimit = 1000000000;
			if (is64BitProcess && totalPhysicalMemory >= 8192000000L)
				safelimit = 2000000000;
			bool danger = false;
			// In Windows/.Net, Process.PrivateMemorySize64 seems to give a reasonable value for current
			// memory usage.  In Linux/Mono, Process.PrivateMemorySize64 gives what seems to be a virtual
			// memory limit or some some value.  In that context, Process.WorkingSet64 appears to be the
			// best bet for approximating how much memory the process is currently using.
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
	}
}
