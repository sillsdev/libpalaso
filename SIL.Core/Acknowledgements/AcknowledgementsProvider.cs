using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SIL.Acknowledgements
{
	public class AcknowledgementsProvider
	{
		private const string UriFilePrefix = "file:///";

		private static string[] exclusions = {"vshost.exe", "CommandLine.exe", "nunit.framework.dll", ".Tests.dll", ".Tests.exe", ".TestApp.dll", ".TestApp.exe" };

		public static string AssembleAcknowledgements()
		{
			var ackAttrDict = new Dictionary<string, AcknowledgementAttribute>();

			// Load all project assemblies, collecting their AcknowledgementAttributes.
			var execAssembly = Assembly.GetExecutingAssembly();

			// Start with self, since the foreach below doesn't work for the executing assembly, for some unknown reason.
			var myAcknowledgments =
				execAssembly.GetCustomAttributes(typeof(AcknowledgementAttribute), false).Cast<AcknowledgementAttribute>();
			CollectAcknowledgementsAndRemoveDuplicates(ackAttrDict, myAcknowledgments);

			// Now find all the other .exes and .dlls we want to examine.
			var fullExecutingAssyName = GetFullNonUriFileName(execAssembly.CodeBase);
			var codeBase = Path.GetDirectoryName(fullExecutingAssyName);
			var assyFile = Path.GetFileName(fullExecutingAssyName);
			var executables = Directory.EnumerateFiles(codeBase).Where(file => Path.HasExtension(file) &&
				(Path.GetExtension(file) == ".exe" || Path.GetExtension(file) == ".dll") && file != assyFile &&
				!exclusions.Any(ex => file.EndsWith(ex)));

			foreach (var execFile in executables)
			{
				try
				{
					var assembly = Assembly.LoadFile(execFile);
					var acknowledgements =
						assembly.GetCustomAttributes(typeof(AcknowledgementAttribute), false).Cast<AcknowledgementAttribute>();
					CollectAcknowledgementsAndRemoveDuplicates(ackAttrDict, acknowledgements);
				}
				// What can happen here?
				catch (Exception)
				{
				}
			}

			return "<ul>" + SortByNameAndConcatenateHtml(ackAttrDict) + "</ul>";
		}

		// internal for testing
		internal static void CollectAcknowledgementsAndRemoveDuplicates(
			IDictionary<string, AcknowledgementAttribute> ackAttrDict, IEnumerable<AcknowledgementAttribute> aaCollection)
		{
			AcknowledgementAttribute dummy;
			foreach (var acknowledgement in aaCollection)
			{
				if (!ackAttrDict.TryGetValue(acknowledgement.Key, out dummy))
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
		/// </summary>
		public static string GetFullNonUriFileName(string uriFilename)
		{
			return uriFilename.StartsWith(UriFilePrefix) ? uriFilename.Substring(UriFilePrefix.Length) : uriFilename;
		}
	}
}
