using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Palaso.BuildTasks.Archive
{
	public class Archive : Task
	{
		[Required]
		public ITaskItem[] InputFilePaths { get; set; }

		[Required]
		public string Command { get; set; }

		[Required]
		public string OutputFileName { get; set; }

		public string BasePath { get; set; }

		public string WorkingDir { get; set; }

		public override bool Execute()
		{
			string filePathString = FlattenFilePaths(InputFilePaths, ' ', false);

			var startInfo = new ProcessStartInfo(ExecutableName());
			startInfo.Arguments = Arguments() + " " + filePathString;
			startInfo.WorkingDirectory = String.IsNullOrEmpty(WorkingDir) ? BasePath : WorkingDir;
			Process.Start(ExecutableName(), Arguments() + " " + filePathString);
			return true;
		}

		internal string ExecutableName()
		{
			switch (Command)
			{
				case "Tar":
					return "tar";
			}
			return String.Empty;
		}

		internal string Arguments()
		{
			switch (Command)
			{
				case "Tar":
					return "-cvzf " + OutputFileName;
			}
			return String.Empty;
		}

		internal string TrimBaseFromFilePath(string filePath)
		{
			string result = filePath;
			if (result.StartsWith(BasePath))
			{
				result = filePath.Substring(BasePath.Length);
				if (result.StartsWith("/") || result.StartsWith("\\"))
					result = result.TrimStart(new[] {'/', '\\'});
			}
			return result;
		}

		internal string FlattenFilePaths(ITaskItem[] items, char delimeter, bool withQuotes)
		{
			var sb = new StringBuilder();
			bool haveStarted = false;
			foreach (var item in items)
			{
				if (haveStarted)
				{
					sb.Append(delimeter);
				}
				string filePath = TrimBaseFromFilePath(item.ItemSpec);
				if (filePath.Contains(" ") || withQuotes)
				{
					sb.Append('"');
					sb.Append(filePath);
					sb.Append('"');
				}
				else
				{
					sb.Append(filePath);
				}
				haveStarted = true;
			}
			return sb.ToString();
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

	}
}