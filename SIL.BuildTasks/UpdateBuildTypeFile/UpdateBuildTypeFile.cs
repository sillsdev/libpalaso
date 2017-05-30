using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.UpdateBuildTypeFile
{
	public class UpdateBuildTypeFile : Task
	{
		[Required]
		public ITaskItem[] BuildTypePaths { get; set; }

		public string BuildType { get; set; }

		public override bool Execute()
		{
			if (string.IsNullOrEmpty(BuildType))
				return true;

			var buildTypeFile = BuildTypePaths.Single();
			var path = buildTypeFile.ItemSpec;
			var contents = File.ReadAllText(path);

			SafeLog("UpdateBuildTypeFile: Updating {0}", buildTypeFile);

			File.WriteAllText(path, GetUpdatedFileContents(contents, BuildType));
			return true;
		}

		public string GetUpdatedFileContents(string contents, string newType)
		{
			StringBuilder bldr = new StringBuilder(@"VersionType\.(");
			bool first = true;
			foreach (var versionType in GetVersionTypes(contents))
			{
				if (!first)
					bldr.Append("|");
				bldr.Append("(");
				bldr.Append(versionType);
				bldr.Append(")");
				first = false;
			}
			bldr.Append(")");
			Regex regex = new Regex(bldr.ToString(), RegexOptions.Compiled);
			return regex.Replace(contents, "VersionType." + newType);
		}

		public List<string> GetVersionTypes(string contents)
		{
			int i = contents.IndexOf("public enum VersionType", StringComparison.Ordinal);
			if (i < 0)
				throw new Exception("File does not contain a public definition for an enum named VersionType!");
			var iStart = contents.IndexOf("{", i, StringComparison.Ordinal) + 1;
			var iEnd = contents.IndexOf("}", iStart, StringComparison.Ordinal);
			var versionTypeEnumBody = contents.Substring(iStart, iEnd - iStart);
			Regex regex = new Regex(@"(?:((?!\d)\w+(?:\.(?!\d)\w+)*)\.)?((?!\d)\w+)", RegexOptions.Compiled);
			return (from object type in regex.Matches(versionTypeEnumBody) select type.ToString()).ToList();
		}

		private void SafeLog(string msg, params object[] args)
		{
			try
			{
				Debug.WriteLine(msg, args);
				Log.LogMessage(msg, args);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}
	}
}