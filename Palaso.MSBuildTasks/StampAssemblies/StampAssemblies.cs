using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Palaso.BuildTasks.StampAssemblies
{
	public class StampAssemblies : Task
	{
		public class VersionParts
		{
			public string[] parts = new string[4];
		}

		[Required]
		public ITaskItem[] InputAssemblyPaths { get; set; }

		[Required]
		public string Version { get; set; }

		public override bool Execute()
		{
			foreach (var inputAssemblyPath in InputAssemblyPaths)
			{
				var path = inputAssemblyPath.ItemSpec;

				var contents = File.ReadAllText(path);

				File.WriteAllText(path,  GetModifiedContents(contents, Version));
			}
			return true;
		}

		public string GetModifiedContents(string contents, string incomingVersion)
		{
			var versionTemplateInFile = GetExistingAssemblyVersion(contents);
			var versionTemplateInBuildScript = ParseVersionString(incomingVersion);

			string newVersionString = MergeTemplates(versionTemplateInBuildScript, versionTemplateInFile);

			var replacement = string.Format(
				"[assembly: AssemblyVersion(\"{0}\")]",
				newVersionString);
			contents = Regex.Replace(contents, @"\[assembly: AssemblyVersion\("".*""\)\]", replacement);
			replacement = string.Format(
				"[assembly: AssemblyFileVersion(\"{0}\")]",
				newVersionString);
			contents = Regex.Replace(contents, @"\[assembly: AssemblyFileVersion\("".*""\)\]", replacement);
			return contents;
		}

		public string MergeTemplates(VersionParts incoming, VersionParts existing)
		{
			string result = "";
			for (int i = 0; i < 4; i++)
			{
				if(incoming.parts[i] != "*")
				{
					result += incoming.parts[i] + ".";
				}
				else
				{
					result += existing.parts[i] + ".";
				}
			}
			return result.TrimEnd(new char[] {'.'});
		}

		public VersionParts GetExistingAssemblyVersion(string contents)
		{
			try
			{
				var result = Regex.Match(contents, @"\[assembly\: AssemblyVersion\(""(.+)""");
				return ParseVersionString(result.Groups[1].Value);
			}
			catch (Exception e)
			{
				Log.LogError("Could not parse the AssemblyVersion attribute, which should be something like 0.7.*.* or 1.0.0.0");
				Log.LogErrorFromException(e);
				throw e;
			}
		}

		public VersionParts ParseVersionString(string contents)
		{
			var result = Regex.Match(contents, @"(.+)\.(.+)\.(.+)\.(.+)");
			var v = new VersionParts();
			v.parts[0] = result.Groups[1].Value;
			v.parts[1] = result.Groups[2].Value;
			v.parts[2] = result.Groups[3].Value;
			v.parts[3] = result.Groups[4].Value;

			return v;
		}
	}
}