using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Palaso.BuildTasks
{
	public class FileUpdate : Task
	{
		[Required]
		public string File { get; set; }

		[Required]
		public string Regex { get; set; }

		[Required]
		public string ReplacementText { get; set; }


		public override bool Execute()
		{
			var content = System.IO.File.ReadAllText(File);
			var newContents = System.Text.RegularExpressions.Regex.Replace(content, this.Regex, ReplacementText);
			if(!newContents.Contains(ReplacementText))
			{
				SafeLogError("Did not manage to replace '{0}' with '{1}'", this.Regex, ReplacementText);
				return false;//we didn't actually replace anything
			}
			System.IO.File.WriteAllText(File,
				newContents);
			return true;
		}



		private void SafeLogError(string msg, params object[] args)
		{
			try
			{
				Debug.WriteLine(string.Format(msg, args));
				Log.LogError(msg, args);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}
	}
}