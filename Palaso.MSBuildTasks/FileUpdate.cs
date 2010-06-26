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
			System.IO.File.WriteAllText(File,
				System.Text.RegularExpressions.Regex.Replace(content, this.Regex, ReplacementText));
			return true;
		}

	}
}