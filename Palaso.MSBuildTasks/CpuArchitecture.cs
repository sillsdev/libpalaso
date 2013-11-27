using System;
using System.Diagnostics;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Palaso.BuildTasks
{
	/// <summary>
	/// Return the CPU architecture of the current system.
	/// </summary>
	public class CpuArchitecture : Task
	{
		public override bool Execute()
		{
			if (Environment.OSVersion.Platform == System.PlatformID.Unix)
			{
				Process proc = new Process();
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.RedirectStandardOutput = true;
				proc.StartInfo.FileName = "/usr/bin/arch";
				proc.Start();
				Value = proc.StandardOutput.ReadToEnd().TrimEnd();
				proc.WaitForExit();
			}
			else
			{
				Value = Environment.Is64BitOperatingSystem ? "x64" : "x86";
			}
			return true;
		}

		[Output]
		public string Value { get; set; }
	}
}
