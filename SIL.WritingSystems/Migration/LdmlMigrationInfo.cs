using System;

namespace SIL.WritingSystems.Migration
{
	/// <summary>
	/// This class keeps track of a file's name change due to updates in the ieft language tag
	/// </summary>
	public class LdmlMigrationInfo
	{
		private readonly string _fileName;

		public LdmlMigrationInfo(string fileName)
		{
			_fileName = fileName;
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public string LanguageTagBeforeMigration { get; set; }

		public string LanguageTagAfterMigration { get; set; }

		internal Action<WritingSystemDefinition> RemovedPropertiesSetter { get; set; } 
	}
}
