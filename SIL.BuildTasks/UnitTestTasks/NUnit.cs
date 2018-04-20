/* Copyright © 2012-2013 SIL International */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace SIL.BuildTasks.UnitTestTasks
{
	/// <summary>
	/// Run NUnit on a test assembly.
	/// </summary>
	public class NUnit : TestTask
	{
		public NUnit()
		{
			// For NUnit, a negative exit code indicates a problem running the tests
			_failTaskIfNegativeExitCode = true;
		}

		/// <summary>
		/// Gets or sets the full path to the NUnit assemblies (test DLLs).
		/// </summary>
		[Required]
		public ITaskItem[] Assemblies { get; set; }

		/// <summary>
		/// Gets or sets the categories to include.
		/// </summary>
		/// <remarks>Multiple values are separated by a comma ","</remarks>
		public string IncludeCategory { get; set; }

		/// <summary>
		/// Gets or sets the categories to exclude.
		/// </summary>
		/// <remarks>Multiple values are separated by a comma ","</remarks>
		public string ExcludeCategory { get; set; }

		/// <summary>
		/// Gets or sets the fixture.
		/// </summary>
		public string Fixture { get; set; }

		/// <summary>
		/// Gets or sets the XSLT transform file.
		/// </summary>
		public string XsltTransformFile { get; set; }

		/// <summary>
		/// Gets or sets the output XML file.
		/// </summary>
		public string OutputXmlFile { get; set; }

		/// <summary>
		/// The file to receive test error details.
		/// </summary>
		public string ErrorOutputFile { get; set; }

		/// <summary>
		/// Gets or sets the working directory.
		/// </summary>
		public string WorkingDirectory { get; set; }

		protected override string GetWorkingDirectory()
		{
			if (!string.IsNullOrEmpty(WorkingDirectory))
			{
				return Path.GetFullPath(WorkingDirectory);
			}
			return Path.GetFullPath(Path.GetDirectoryName(Assemblies[0].ItemSpec));
		}

		/// <summary>
		/// Determines whether assemblies are copied to a shadow folder during testing.
		/// </summary>
		public bool DisableShadowCopy { get; set; }

		/// <summary>
		/// The project configuration to run.
		/// </summary>
		public string ProjectConfiguration { get; set; }

		/// <summary>
		/// Sometimes it is important that the task report a failure if any tests fail.
		/// One example is if we run multiple build targets in the same call and
		/// expect subsequent targets not to be called if tests fail.
		///
		/// REVIEW: it actually seems to me that this should be the default behavior, but
		/// I suppose changing the code in that way now could cause problems for existing callers.
		/// </summary>
		public bool? FailTaskIfAnyTestsFail
		{
			get { return _failTaskIfAnyTestsFail; }
			set
			{
				_failTaskIfAnyTestsFail = value;

				// For NUnit, a positive exit code indicates the number of failed tests
				_failTaskIfPositiveExitCode = value.HasValue && value.Value;
			}
		}

		// make this nullable so we have a third state, not set
		private bool? _testInNewThread;
		private bool? _failTaskIfAnyTestsFail;

		/// <summary>
		/// Allows tests to be run in a new thread, allowing you to take advantage of ApartmentState and ThreadPriority settings in the config file.
		/// </summary>
		public bool TestInNewThread
		{
			get { return !_testInNewThread.HasValue || _testInNewThread.Value; }
			set { _testInNewThread = value; }
		}

		/// <summary>
		/// Determines whether the tests are run in a 32bit process on a 64bit OS.
		/// </summary>
		public bool Force32Bit { get; set; }

		/// <summary>
		/// Determines the framework to run aganist.
		/// </summary>
		public string Framework { get; set; }

		/// <summary>
		/// Gets or sets the path to the NUnit executable assembly.
		/// </summary>
		public string ToolPath { get; set; }

		/// <summary>
		/// Apartment for running tests: MTA (Default), STA
		/// </summary>
		public string Apartment { get; set; }

		/// <summary>
		/// Gets the name (without path) of the NUnit executable. When running on Mono this is
		/// different from ProgramNameAndPath() which returns the executable we'll start.
		/// </summary>
		protected virtual string RealProgramName => Force32Bit ? "nunit-console-x86.exe" : "nunit-console.exe";

		private static bool? _isMono;

		private static bool IsMono
		{
			get
			{
				if (_isMono == null)
					_isMono = Type.GetType("Mono.Runtime") != null;

				return (bool)_isMono;
			}
		}

		/// <summary>
		/// Gets the name of the executable to start.
		/// </summary>
		/// <returns>The name of the NUnit executable when run on .NET, or
		/// the name of the Mono runtime executable when run on Mono.</returns>
		protected override string ProgramNameAndPath
		{
			get
			{
				if (IsMono)
					return "mono";

				EnsureToolPath();
				return Path.Combine(Path.GetFullPath(ToolPath), RealProgramName);
			}
		}

		protected override string ProgramArguments
		{
			get
			{
				var bldr = new StringBuilder();
				if (IsMono)
				{
					EnsureToolPath();
					bldr.Append("--debug "); // cause Mono to show filenames in stack trace
					bldr.Append(Path.Combine(Path.GetFullPath(ToolPath), RealProgramName));
				}
				foreach (var item in Assemblies)
				{
					if (bldr.Length > 0)
						bldr.Append(" ");
					bldr.Append(item.ItemSpec);
				}
				// At least with NUnit 2.8.4 we can use "--" as switch char on both Windows and Linux
				if (DisableShadowCopy)
					bldr.Append(" --noshadow");
				if (!string.IsNullOrEmpty(ProjectConfiguration))
					bldr.AppendFormat(" \"--config={0}\"", ProjectConfiguration);
				if (!string.IsNullOrEmpty(Fixture))
					bldr.AppendFormat(" \"--fixture={0}\"", Fixture);
				if (!string.IsNullOrEmpty(XsltTransformFile))
					bldr.AppendFormat(" \"--transform={0}\"", XsltTransformFile);
				if (!string.IsNullOrEmpty(ErrorOutputFile))
					bldr.AppendFormat(" \"--err={0}\"", ErrorOutputFile);
				if (!string.IsNullOrEmpty(Framework))
					bldr.AppendFormat(" \"--framework={0}\"", Framework);
				if (!string.IsNullOrEmpty(Apartment))
					bldr.AppendFormat(" \"--apartment={0}\"", Apartment);
				bldr.AppendFormat(AddIncludeAndExcludeArguments());
				bldr.AppendFormat(AddAdditionalProgramArguments());
				return bldr.ToString();
			}
		}

		// internal for testing
		internal virtual string AddIncludeAndExcludeArguments()
		{
			var bldr = new StringBuilder();
			if (!string.IsNullOrEmpty(IncludeCategory))
				bldr.AppendFormat(" \"--include={0}\"", IncludeCategory);
			if (!string.IsNullOrEmpty(ExcludeCategory))
				bldr.AppendFormat(" \"--exclude={0}\"", ExcludeCategory);
			return bldr.ToString();
		}

		protected virtual string AddAdditionalProgramArguments()
		{
			var bldr = new StringBuilder();
			bldr.Append(" --nologo");
			if (!TestInNewThread)
				bldr.Append(" --nothread");
			if (!string.IsNullOrEmpty(OutputXmlFile))
				bldr.AppendFormat(" \"--xml={0}\"", OutputXmlFile);
			bldr.Append(" --labels");
			return bldr.ToString();
		}

		private void EnsureToolPath()
		{
			if (!String.IsNullOrEmpty(ToolPath) &&
				File.Exists(Path.Combine(ToolPath, RealProgramName)))
			{
				return;
			}
			foreach (var dir in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
			{
				if (File.Exists(Path.Combine(dir, RealProgramName)))
				{
					ToolPath = dir;
					return;
				}
			}
			foreach (var dir in Directory.EnumerateDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)))
			{
				if (dir.StartsWith("NUnit"))
				{
					if (File.Exists(Path.Combine(dir, Path.Combine("bin", RealProgramName))))
					{
						ToolPath = dir;
						return;
					}
				}
			}
			var keySoftware = Registry.CurrentUser.OpenSubKey("Software");
			if (keySoftware != null && Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				var keyNUnitOrg = keySoftware.OpenSubKey("nunit.org");
				if (keyNUnitOrg != null)
				{
					var keyNUnit = keyNUnitOrg.OpenSubKey("Nunit");
					if (keyNUnit != null)
					{
						foreach (var verName in keyNUnit.GetSubKeyNames())
						{
							var keyVer = keyNUnit.OpenSubKey(verName);
							if (keyVer != null)
							{
								var path = keyVer.GetValue("InstallDir").ToString();
								if (!String.IsNullOrEmpty(path) &&
									File.Exists(Path.Combine(path, RealProgramName)))
								{
									ToolPath = path;
									return;
								}
							}
						}
					}
				}
			}
			ToolPath = ".";
		}

		protected override string TestProgramName => $"{GetType().Name} ({FixturePath})";

		private string FixturePath
		{
			get
			{
				var bldr = new StringBuilder();
				foreach (var item in Assemblies)
				{
					if (bldr.Length > 0)
						bldr.Append(" ");
					bldr.Append(Path.GetFileNameWithoutExtension(item.ItemSpec));
				}
				return bldr.ToString();
			}
		}

		protected override void ProcessOutput(bool fTimedOut, TimeSpan delta)
		{
			var lines = new List<string>();
			foreach (var line in m_TestLog)
			{
				var trimmedLine = line.Trim();
				if (trimmedLine.StartsWith("***** "))
					lines.Add(trimmedLine);
				else if (!Verbose)
				{
					// Print out collected messages of NUnit unless we already printed them in
					// the base class
					Log.LogMessage(MessageImportance.Normal, trimmedLine);
				}
			}
			if (fTimedOut)
			{
				// If the tests time out we have an invalid XML file. Create a valid XML file
				// that contains as much as possible.
				if (File.Exists(OutputXmlFile))
				{
					var fi = new FileInfo(OutputXmlFile);
					if (fi.Length > 0)
						File.Move(OutputXmlFile, OutputXmlFile + "-partial");
					else
						File.Delete(OutputXmlFile);
				}
				using (var writer = new StreamWriter(OutputXmlFile))
				{
					var num = lines.Count;
					writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
					writer.WriteLine("<test-results name=\"{0}\" total=\"{1}\" errors=\"{2}\" failures=\"{3}\" not-run=\"{4}\" inconclusive=\"{5}\" ignored=\"{6}\" skipped=\"{7}\" invalid=\"{8}\" date=\"{9}\" time=\"{10}\">",
										FixturePath, num + 1, 0, 1, 0, num, 0, 0, 0,
										DateTime.Now.ToShortDateString(), DateTime.Now.ToString("HH:mm:ss"));
					writer.WriteLine("  <test-suite type=\"Assembly\" name=\"{0}\" executed=\"True\" result=\"Timeout\" success=\"False\" time=\"{1}\">",
										FixturePath, delta.TotalSeconds.ToString("F3"));
					writer.WriteLine("    <results>");
					writer.WriteLine("      <test-suite name=\"Timeout\">");
					writer.WriteLine("        <results>");
					writer.WriteLine("          <test-case name=\"Timeout\" success=\"False\" time=\"{0}\" asserts=\"0\"/>", ((double)Timeout / 1000.0).ToString("F3"));
					writer.WriteLine("        </results>");
					writer.WriteLine("      </test-suite>");
					writer.WriteLine("    </results>");
					writer.WriteLine("  </test-suite>");
					writer.WriteLine("<!-- tests tried before time ran out:");
					foreach (var line in lines)
						writer.WriteLine(line.Substring(6));
					writer.WriteLine("-->");
					writer.WriteLine("</test-results>");
				}
			}
		}

		protected override ITaskItem[] FailedSuiteNames
		{
			get
			{
				var suites = new ITaskItem[Assemblies.Length];
				for (int i = 0; i < Assemblies.Length; i++)
				{
					suites[i] = new TaskItem(Path.GetFileNameWithoutExtension(Assemblies[i].ItemSpec));
				}
				return suites;
			}
		}
	}
}
