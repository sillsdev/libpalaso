using System;
using System.Collections.Generic;
using System.Diagnostics;
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

			public override string ToString()
			{
				return string.Format("{0}.{1}.{2}.{3}", parts[0], parts[1], parts[2], parts[3]);
			}
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

				SafeLog("StampAssemblies: Stamping {0}", inputAssemblyPath);
				//SafeLog("StampAssemblies: Contents: {0}",contents);

				File.WriteAllText(path,  GetModifiedContents(contents, Version));
			}
			return true;
		}

		public string GetModifiedContents(string contents, string incomingVersion)
		{
			var versionTemplateInFile = GetExistingAssemblyVersion(contents);
			var versionTemplateInBuildScript = ParseVersionString(incomingVersion);

			string newVersionString = MergeTemplates(versionTemplateInBuildScript, versionTemplateInFile);

			SafeLog("StampAssemblies: Merging existing {0} with incoming {1} to produce {2}.",
				versionTemplateInFile.ToString(), incomingVersion, newVersionString);


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

		private void SafeLog(string msg, params object[] args)
		{
			try
			{
				Debug.WriteLine(string.Format(msg,args));
				Log.LogMessage(msg,args);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}

		public string MergeTemplates(VersionParts incoming, VersionParts existing)
		{
			string result = "";
			for (int i = 0; i < 4; i++)
			{
				//SafeLog("StampAssemblies: incoming[{0}]={1}", i, incoming.parts[i]);
				//SafeLog("StampAssemblies: existing[{0}]={1}", i, existing.parts[i]);
				if(incoming.parts[i] != "*")
				{
					result += incoming.parts[i] + ".";
				}
				else
				{
					if(existing.parts[i] == "*")
					{
						SafeLog("StampAssemblies: existing.parts[i] == '*'");
						result += "0.";
					}
					else
					{
						result += existing.parts[i] + ".";
					}
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
			if(!result.Success)
			{
				//handle 1.0.*  (I'm not good enough with regex to
				//overcome greediness and get a single pattern to work for both situations).
				result = Regex.Match(contents, @"(.+)\.(.+)\.(\*)");
			}
			if (!result.Success)
			{
				//handle 0.0.12
				result = Regex.Match(contents, @"(.+)\.(.+)\.(.+)");
			}
			var v = new VersionParts();
			v.parts[0] = result.Groups[1].Value;
			v.parts[1] = result.Groups[2].Value;
			v.parts[2] = result.Groups[3].Value;
			v.parts[3] = result.Groups[4].Value;

			for (int i = 0; i < 4; i++)
			{
				if(string.IsNullOrEmpty(v.parts[i]))
				{
					v.parts[i] = "*";
				}
			}
			//can't propogate a hash code, though it's nice (for build server display purposes)
			//to send it through to us. So for now, we just strip it out.
			if(v.parts[3].IndexOfAny(new char[]{'a','b','c','d','e','f'}) >-1)
			{
				v.parts[3] = "0";
			}

			return v;
		}
	}
}