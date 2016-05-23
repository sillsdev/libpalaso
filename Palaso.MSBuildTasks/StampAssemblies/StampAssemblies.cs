using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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

		public string FileVersion { get; set; }

		public override bool Execute()
		{
			foreach (var inputAssemblyPath in InputAssemblyPaths)
			{
				var path = inputAssemblyPath.ItemSpec;

				SafeLog("StampAssemblies: Reading {0}", path); //investigating mysterious TeamCity failure with "Illegal Characters in path"
				SafeLog("StampAssemblies: If you get 'Illegal Characters in path' and have a wild card in the file specification, check for paths that exceed MAX_PATH. We had this happen when we 'shrinkwrap'-ped our node dependencies. MsBuild just silently gives up when this happens.");
				var contents = File.ReadAllText(path);

				SafeLog("StampAssemblies: Stamping {0}", inputAssemblyPath);
				//SafeLog("StampAssemblies: Contents: {0}",contents);

				// ENHANCE: add property for InformationalVersion
				File.WriteAllText(path,  GetModifiedContents(contents, Version, FileVersion));
			}
			return true;
		}

		private string ExpandTemplate(string whichAttribute, string contents,
			VersionParts incomingVersion, bool allowHashAsRevision)
		{
			try
			{
				var regex = new Regex(string.Format(@"\[assembly\: {0}\(""(.+)""", whichAttribute));
				var result = regex.Match(contents);
				if (result == Match.Empty)
					return contents;
				var versionTemplateInFile = ParseVersionString(result.Groups[1].Value, allowHashAsRevision);
				var newVersion = MergeTemplates(incomingVersion, versionTemplateInFile);

				SafeLog("StampAssemblies: Merging existing {0} with incoming {1} to produce {2}.",
					versionTemplateInFile.ToString(), incomingVersion.ToString(), newVersion);
				return regex.Replace(contents, string.Format(@"[assembly: {0}(""{1}""", whichAttribute, newVersion));
			}
			catch (Exception e)
			{
				Log.LogError("Could not parse the {0} attribute, which should be something like 0.7.*.* or 1.0.0.0",
					whichAttribute);
				Log.LogErrorFromException(e);
				throw e;
			}
		}

		public string GetModifiedContents(string contents, string incomingVersion, string incomingFileVersion)
		{
			var versionTemplateInBuildScript = ParseVersionString(incomingVersion, false);
			var fileVersionTemplateInScript = incomingFileVersion != null ?
				ParseVersionString(incomingFileVersion, false) : versionTemplateInBuildScript;
			var infoVersionTemplateInBuildScript = ParseVersionString(incomingVersion, true);

			contents = ExpandTemplate("AssemblyVersion", contents, versionTemplateInBuildScript, false );
			contents = ExpandTemplate("AssemblyFileVersion", contents, fileVersionTemplateInScript, false);
			contents = ExpandTemplate("AssemblyInformationalVersion", contents, infoVersionTemplateInBuildScript, true);
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
						//SafeLog("StampAssemblies: existing.parts[i] == '*'");
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

		private VersionParts GetExistingAssemblyVersion(string whichAttribute, string contents)
		{
			try
			{
				var result = Regex.Match(contents, string.Format(@"\[assembly\: {0}\(""(.+)""", whichAttribute));
				if (result == Match.Empty)
					return null;
				return ParseVersionString(result.Groups[1].Value);
			}
			catch (Exception e)
			{
				Log.LogError("Could not parse the {0} attribute, which should be something like 0.7.*.* or 1.0.0.0",
					whichAttribute);
				Log.LogErrorFromException(e);
				throw e;
			}
		}

		public VersionParts GetExistingAssemblyVersion(string contents)
		{
			return GetExistingAssemblyVersion("AssemblyVersion", contents);
		}

		public VersionParts GetExistingAssemblyFileVersion(string contents)
		{
			return GetExistingAssemblyVersion("AssemblyFileVersion", contents);
		}

		public VersionParts ParseVersionString(string contents, bool allowHashAsRevision = false)
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

			if(!allowHashAsRevision && v.parts[3].IndexOfAny(new char[]{'a','b','c','d','e','f'}) >-1)
			{
				// zero out hash code which we can't have in numeric version numbers
				v.parts[3] = "0";
			}

			return v;
		}
	}
}