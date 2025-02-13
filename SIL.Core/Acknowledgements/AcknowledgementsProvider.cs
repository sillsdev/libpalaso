// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SIL.PlatformUtilities;

namespace SIL.Acknowledgements
{
	public static class AcknowledgementsProvider
	{
		private static readonly string[] Exclusions =
		{
			"Interop.WIA.dll", "vshost.exe", "CommandLine.dll",
			"nunit.framework.dll", ".Tests.dll", ".Tests.exe", "TestApp.dll", "TestApp.exe",
			"oleaut32.dll", "ole32.dll"

			// Use this version to see the TestApp dependencies in SIL.Windows.Forms.TestApp.AssemblyInfo.cs
			//"Interop.WIA.dll", "vshost.exe", "CommandLine.dll",
			//"nunit.framework.dll", ".Tests.dll", ".Tests.exe"
		};

		public static Dictionary<string, AcknowledgementAttribute> CollectAcknowledgements(bool includeSystemLibraries = false)
		{
			var ackAttrDict = new Dictionary<string, AcknowledgementAttribute>();

			// Load all project assemblies, collecting their AcknowledgementAttributes.
			var execAssembly = Assembly.GetExecutingAssembly(); // SIL.Core.dll, since that's where this class lives

			// Start with self, since Assembly.LoadFile() in the foreach below doesn't work for the executing assembly,
			// probably because it's already loaded.
			var myAcknowledgments =
				execAssembly.GetCustomAttributes(typeof(AcknowledgementAttribute), false).Cast<AcknowledgementAttribute>();
			CollectAcknowledgementsAndRemoveDuplicates(ackAttrDict, myAcknowledgments);

			// Now find all the other .exe and .dll files we want to examine.
			var entryAssembly = Assembly.GetEntryAssembly(); // This should be the project that is running the SILAboutBox.
			if (entryAssembly != null) // Can happen in tests
			{
				var assemblyName = GetNonUriExecutingAssemblyName(execAssembly);
				var codeBase = Path.GetDirectoryName(GetFullNonUriFileName(entryAssembly.CodeBase));
				var components = Directory.EnumerateFiles(codeBase).Where(
					file => Path.HasExtension(file) &&
					(Path.GetExtension(file).ToLowerInvariant() == ".exe" || Path.GetExtension(file).ToLowerInvariant() == ".dll") &&
					file != assemblyName && !Exclusions.Any(file.EndsWith) &&
					(includeSystemLibraries || !Path.GetFileName(file).StartsWith("System.")));

				foreach (var execFile in components)
				{
					try
					{
						var assembly = Assembly.LoadFile(execFile);
						var acknowledgements =
							assembly.GetCustomAttributes(typeof(AcknowledgementAttribute), false).Cast<AcknowledgementAttribute>();
						CollectAcknowledgementsAndRemoveDuplicates(ackAttrDict, acknowledgements);
					}
					catch (Exception ex)
					{
						if (ex is BadImageFormatException || ex is FileLoadException)
							Debug.WriteLine(
								$"{ex.GetType().Name} on file '{execFile}': {ex.Message}");
						else throw;
					}
				}
			}
			return ackAttrDict;
		}

		public static string AssembleAcknowledgements()
		{
			var ackAttrDict = CollectAcknowledgements();
			return SortByNameAndConcatenateHtml(ackAttrDict);
		}

		// internal for testing
		internal static void CollectAcknowledgementsAndRemoveDuplicates(
			IDictionary<string, AcknowledgementAttribute> ackAttrDict, IEnumerable<AcknowledgementAttribute> ackAttrCollection)
		{
			foreach (var acknowledgement in ackAttrCollection)
			{
				if (ackAttrDict.TryGetValue(acknowledgement.Key, out var ackRetained))
				{
					Debug.WriteLine("Duplicate Acknowledgement key skipped. Key="+ acknowledgement.Key + " Html=" + acknowledgement.Html);
					Debug.WriteLine("  Kept first Acknowledgement Key=" + ackRetained.Key + " Html=" + ackRetained.Html);
				}
				else
					ackAttrDict.Add(acknowledgement.Key, acknowledgement);
			}
		}

		// internal for testing
		internal static string SortByNameAndConcatenateHtml(Dictionary<string, AcknowledgementAttribute> ackAttrDict)
		{
			return ackAttrDict.OrderBy(current => current.Value.Name)
				.Aggregate(string.Empty, (current, keyValuePair) => current + keyValuePair.Value.Html);
		}

		/// <summary>
		/// Returns a filename safe for using to retrieve files in non-Uri format.
		/// Thanks to Eberhard for this version that should work on Linux too.
		/// </summary>
		public static string GetFullNonUriFileName(string uriFilename)
		{
			return Uri.UnescapeDataString(new Uri(uriFilename).AbsolutePath);
		}

		private static string GetNonUriExecutingAssemblyName(Assembly execAssembly)
		{
			var assemblyName = GetFullNonUriFileName(execAssembly.CodeBase);
			// On Windows, GetExecutingAssembly().CodeBase puts the file extension in UPPERCASE! Why!?
			if (Platform.IsWindows)
			{
				assemblyName = Path.GetDirectoryName(assemblyName) + Path.DirectorySeparatorChar +
					Path.GetFileNameWithoutExtension(assemblyName) + ".dll";
			}
			return assemblyName;
		}
	}
}
