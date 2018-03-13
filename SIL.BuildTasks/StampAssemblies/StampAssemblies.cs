using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.StampAssemblies
{
	public class StampAssemblies : Task
	{
		private enum VersionFormat
		{
			File,
			Info,
			Semantic
		}

		public class VersionParts
		{
			public string[] parts = new string[4];
			public string Prerelease { get; set; }

			public override string ToString()
			{
				string str = string.Join(".", parts);
				if (!string.IsNullOrEmpty(Prerelease))
					str += "-" + Prerelease;
				return str;
			}
		}

		[Required]
		public ITaskItem[] InputAssemblyPaths { get; set; }

		[Required]
		public string Version { get; set; }

		public string FileVersion { get; set; }

		public string PackageVersion { get; set; }

		public override bool Execute()
		{
			foreach (var inputAssemblyPath in InputAssemblyPaths)
			{
				var path = inputAssemblyPath.ItemSpec;

				SafeLog("StampAssemblies: Reading {0}", path); //investigating mysterious TeamCity failure with "Illegal Characters in path"
				SafeLog("StampAssemblies: If you get 'Illegal Characters in path' and have a wild card in the file specification, check for paths that exceed MAX_PATH. We had this happen when we 'shrinkwrap'-ped our node dependencies. MsBuild just silently gives up when this happens.");
				var contents = File.ReadAllText(path);

				SafeLog("StampAssemblies: Stamping {0}", inputAssemblyPath);

				bool isCode = Path.GetExtension(path).Equals(".cs", StringComparison.InvariantCultureIgnoreCase);
				// ENHANCE: add property for InformationalVersion
				contents = GetModifiedContents(contents, isCode, Version, FileVersion, PackageVersion);
				File.WriteAllText(path, contents);
			}
			return true;
		}

		private string ExpandTemplate(string regexTemplate, string replaceTemplate, string whichVersion,
			string contents, VersionParts incomingVersion, VersionFormat format = VersionFormat.File)
		{
			try
			{
				var regex = new Regex(string.Format(regexTemplate, whichVersion));
				var result = regex.Match(contents);
				if (result == Match.Empty)
					return contents;
				var versionTemplateInFile = ParseVersionString(result.Groups[1].Value, format);
				var newVersion = MergeTemplates(incomingVersion, versionTemplateInFile);

				SafeLog("StampAssemblies: Merging existing {0} with incoming {1} to produce {2}.",
					versionTemplateInFile.ToString(), incomingVersion.ToString(), newVersion);
				return regex.Replace(contents, string.Format(replaceTemplate, whichVersion, newVersion));
			}
			catch (Exception e)
			{
				Log.LogError("Could not parse the {0} attribute, which should be something like 0.7.*.* or 1.0.0.0",
					whichVersion);
				Log.LogErrorFromException(e);
				throw;
			}
		}

		public string GetModifiedContents(string contents, string versionStr, string fileVersionStr)
		{
			return GetModifiedContents(contents, true, versionStr, fileVersionStr, null);
		}

		internal string GetModifiedContents(string contents, bool isCode, string versionStr, string fileVersionStr,
			string packageVersionStr)
		{
			// ENHANCE: add property for InformationalVersion
			VersionParts version = ParseVersionString(versionStr);
			VersionParts fileVersion = fileVersionStr != null ? ParseVersionString(fileVersionStr) : version;
			VersionParts infoVersion = ParseVersionString(versionStr, VersionFormat.Info);
			VersionParts packageVersion = packageVersionStr != null
				? ParseVersionString(packageVersionStr, VersionFormat.Semantic)
				: version;

			if (isCode)
				return ModifyCodeAttributes(contents, version, fileVersion, infoVersion);
			return ModifyMSBuildProps(contents, version, fileVersion, infoVersion, packageVersion);
		}

		private string ModifyCodeAttributes(string contents, VersionParts version, VersionParts fileVersion,
			VersionParts infoVersion)
		{
			const string regexTemplate = @"\[assembly\: {0}\(""(.+)""";
			const string replaceTemplate = @"[assembly: {0}(""{1}""";

			contents = ExpandTemplate(regexTemplate, replaceTemplate, "AssemblyVersion", contents, version);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "AssemblyFileVersion", contents, fileVersion);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "AssemblyInformationalVersion", contents,
				infoVersion, VersionFormat.Info);
			return contents;
		}

		private string ModifyMSBuildProps(string contents, VersionParts version, VersionParts fileVersion,
			VersionParts infoVersion, VersionParts packageVersion)
		{
			const string regexTemplate = "<{0}>(.+)</{0}>";
			const string replaceTemplate = "<{0}>{1}</{0}>";

			contents = ExpandTemplate(regexTemplate, replaceTemplate, "AssemblyVersion", contents, version);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "FileVersion", contents, fileVersion);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "InformationalVersion", contents, infoVersion,
				VersionFormat.Info);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "Version", contents, packageVersion,
				VersionFormat.Semantic);

			return contents;
		}

		private void SafeLog(string msg, params object[] args)
		{
			try
			{
				Debug.WriteLine(msg, args);
				Log.LogMessage(msg,args);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}

		public string MergeTemplates(VersionParts incoming, VersionParts existing)
		{
			VersionParts result = new VersionParts
			{
				parts = (string[]) existing.parts.Clone(),
				Prerelease = incoming.Prerelease ?? existing.Prerelease
			};
			for (int i = 0; i < result.parts.Length; i++)
			{
				if (incoming.parts[i] != "*")
					result.parts[i] = incoming.parts[i];
				else if (existing.parts[i] == "*")
					result.parts[i] = "0";
			}
			return result.ToString();
		}

		private VersionParts GetExistingAssemblyVersion(string whichAttribute, string contents)
		{
			try
			{
				var result = Regex.Match(contents, $@"\[assembly\: {whichAttribute}\(""(.+)""");
				if (result == Match.Empty)
					return null;
				return ParseVersionString(result.Groups[1].Value);
			}
			catch (Exception e)
			{
				Log.LogError("Could not parse the {0} attribute, which should be something like 0.7.*.* or 1.0.0.0",
					whichAttribute);
				Log.LogErrorFromException(e);
				throw;
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
			return ParseVersionString(contents, allowHashAsRevision ? VersionFormat.Info : VersionFormat.File);
		}

		private static VersionParts ParseVersionString(string contents, VersionFormat format)
		{
			VersionParts v;
			if (format == VersionFormat.Semantic)
			{
				Match result = Regex.Match(contents, @"([\d\*]+)\.([\d\*]+)\.([\d\*]+)(?:\-(.*))?");

				v = new VersionParts
				{
					parts = new[]
					{
						result.Groups[1].Value,
						result.Groups[2].Value,
						result.Groups[3].Value
					},
					Prerelease = result.Groups[4].Value
				};
			}
			else
			{
				Match result = Regex.Match(contents, @"(.+)\.(.+)\.(.+)\.(.+)");
				if (!result.Success)
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

				v = new VersionParts
				{
					parts = new[]
					{
						result.Groups[1].Value,
						result.Groups[2].Value,
						result.Groups[3].Value,
						result.Groups[4].Value
					}
				};

				if (format == VersionFormat.File && v.parts.Length == 4
					&& v.parts[3].IndexOfAny(new[] { 'a', 'b', 'c', 'd', 'e', 'f' }) != -1)
				{
					// zero out hash code which we can't have in numeric version numbers
					v.parts[3] = "0";
				}
			}

			for (int i = 0; i < v.parts.Length; i++)
			{
				if (string.IsNullOrEmpty(v.parts[i]))
					v.parts[i] = "*";
			}

			return v;
		}
	}
}