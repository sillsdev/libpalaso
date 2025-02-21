// Copyright (c) 2017-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using static System.String;

namespace SIL.ExtractCopyright
{
	class Program
	{
		static int Main(string[] args)
		{
			string debianFolder = null;
			string prefixFolder = null;
			for (int i = 0; i < args.Length; ++i)
			{
				switch (args[i])
				{
				case "-d":
				case "--debian":
					if (++i < args.Length)
						debianFolder = args[i];
					else
						return ShowUsage(CopyrightFile.ExitValue.NoDebianFolder);
					break;
				case "-?":
				case "-h":
				case "--help":
					return ShowUsage(CopyrightFile.ExitValue.Okay);
				case "-p":
				case "--prefix":
					if (++i < args.Length)
						prefixFolder = args[i];
					else
						return ShowUsage(CopyrightFile.ExitValue.NoPrefixFolder);
					break;
				}
			}
			// If the user didn't supply the debian folder, try to find one at the current
			// location or at a parent location in the directory tree.
			if (debianFolder == null)
			{
				var current = Directory.GetCurrentDirectory();
				debianFolder = Path.Combine(current, "debian");
				while (!IsNullOrEmpty(current) && !Directory.Exists(debianFolder))
				{
					current = Path.GetDirectoryName(current);
					if (!IsNullOrEmpty(current))
						debianFolder = Path.Combine(current, "debian");
				}
				if (Directory.Exists(debianFolder))
					Console.WriteLine("ExtractCopyright: found debian folder at \"{0}\"", debianFolder);
			}
			if (!Directory.Exists(debianFolder))
			{
				return ShowUsage(CopyrightFile.ExitValue.NoDebianFolder);
			}
			if (!File.Exists(Path.Combine(debianFolder, "control")) ||
				!File.Exists(Path.Combine(debianFolder, "changelog")))
			{
				Console.WriteLine("ExtractCopyright: the debian folder does not contain both the control and changelog files.");
				return ShowUsage(CopyrightFile.ExitValue.NoDebianFolder);
			}

			// If the user didn't supply the prefix folder, use an empty string.
			prefixFolder ??= Empty;

			return CopyrightFile.CreateOrUpdateCopyrightFile(debianFolder, prefixFolder);
		}

		private static int ShowUsage(CopyrightFile.ExitValue retval)
		{
			Console.WriteLine("ExtractCopyright creates or updates the Debian copyright file from the");
			Console.WriteLine("acknowledgements embedded in the .Net assemblies.  It is highly recommended");
			Console.WriteLine("that a human review and edit the output from this program!");
			Console.WriteLine("Usage: ExtractCopyright [options]");
			Console.WriteLine("-d (--debian)   provide the path to the debian folder.  If this is not given, the");
			Console.WriteLine("                program will search the current directory and its parents up the tree.");
			Console.WriteLine("-h (--help)     display this message");
			Console.WriteLine("-p (--prefix)   provide a directory prefix to add to DLL filenames in copyright file");
			return (int)retval;
		}
	}

}
