// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;

namespace Palaso.IO
{
	public static class PathUtilities
	{

		// On Unix there are more characters valid in file names, but we
		// want the result to be identical on both platforms, so we want
		// to use the larger invalid Windows list for both platforms
		public static char[] GetInvalidOSIndependentFileNameChars()
		{
			return new char[]
			{
				'\0',
				'\u0001',
				'\u0002',
				'\u0003',
				'\u0004',
				'\u0005',
				'\u0006',
				'\a',
				'\b',
				'\t',
				'\n',
				'\v',
				'\f',
				'\r',
				'\u000e',
				'\u000f',
				'\u0010',
				'\u0011',
				'\u0012',
				'\u0013',
				'\u0014',
				'\u0015',
				'\u0016',
				'\u0017',
				'\u0018',
				'\u0019',
				'\u001a',
				'\u001b',
				'\u001c',
				'\u001d',
				'\u001e',
				'\u001f',
				'"',
				'<',
				'>',
				'|',
				':',
				'*',
				'?',
				'\\',
				'/'
			};
		}

		public static int GetDeviceNumber(string filePath)
		{
			if (Palaso.PlatformUtilities.Platform.IsWindows)
			{
				var driveInfo = new DriveInfo(Path.GetPathRoot(filePath));
				return driveInfo.Name.ToUpper()[0] - 'A' + 1;
			}

			var process = new Process() { StartInfo = new ProcessStartInfo {
					FileName = "stat",
					Arguments = string.Format("-c %d {0}", filePath),
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};
			process.Start();
			process.WaitForExit();
			var output = process.StandardOutput.ReadToEnd();
			return Convert.ToInt32(output);
		}
	}
}

