using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks
{
	public class FileUpdate : Task
	{
		private string _dateFormat;

		[Required]
		public string File { get; set; }

		[Required]
		public string Regex { get; set; }

		[Required]
		public string ReplacementText { get; set; }

		/// <summary>
		/// The string pattern to replace with the current date (UTC, dd/MMM/yyyy)
		/// </summary>
		public string DatePlaceholder { get; set; }

		/// <summary>
		/// The date format to output (default is dd/MMM/yyyy)
		/// </summary>
		public string DateFormat
		{
			get { return _dateFormat ?? "dd/MMM/yyyy"; }
			set { _dateFormat = value; }
		}

		public override bool Execute()
		{
			var content = System.IO.File.ReadAllText(File);
			var newContents = System.Text.RegularExpressions.Regex.Replace(content, Regex, ReplacementText);

			if(!string.IsNullOrEmpty(DatePlaceholder))
			{
				newContents = newContents.Replace(DatePlaceholder, DateTime.UtcNow.Date.ToString(DateFormat));
			}
			if(!newContents.Contains(ReplacementText))
			{
				SafeLogError("Did not manage to replace '{0}' with '{1}'", Regex, ReplacementText);
				return false;//we didn't actually replace anything
			}
			System.IO.File.WriteAllText(File, newContents);
			return true;
		}



		private void SafeLogError(string msg, params object[] args)
		{
			try
			{
				Debug.WriteLine(msg, args);
				Log.LogError(msg, args);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}
	}
}