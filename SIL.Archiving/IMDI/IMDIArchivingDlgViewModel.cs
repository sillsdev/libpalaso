using System;
using System.IO;
using System.Text;

namespace SIL.Archiving.IMDI
{
	/// <summary>Implements archiving for IMDI repositories</summary>
	public class IMDIArchivingDlgViewModel : ArchivingDlgViewModel
	{
		private readonly string _corpusName;
		private IMDIData _imdiData;
		private readonly string _corpusOutputFolder;
		private string _corpusDirectoryName;

		/// <summary>Constructor</summary>
		/// <param name="appName"></param>
		/// <param name="corpusName"></param>
		/// <param name="title"></param>
		/// <param name="id"></param>
		/// <param name="corpusOutputFolder"></param>
		public IMDIArchivingDlgViewModel(string appName, string corpusName, string title, string id, string corpusOutputFolder) : base(appName, title, id)
		{
			_corpusName = corpusName;
			_corpusOutputFolder = corpusOutputFolder;
		}

		protected override bool DoArchiveSpecificInitialization()
		{
			_imdiData = new IMDIData
			{
				Title = _title,
				Name = _corpusName
			};

			return true;
		}

		public override bool LaunchArchivingProgram()
		{
			throw new NotImplementedException();
		}

		public override bool CreatePackage()
		{
			throw new NotImplementedException();
		}

		protected override StringBuilder DoArchiveSpecificFilenameNormalization(string key, string fileName)
		{
			return new StringBuilder(fileName.ToLatinOnly("_", "+", "."));
		}

		private static string NormalizeDirectoryName(string dirName)
		{
			return dirName.ToLatinOnly("_", "_", ".-");
		}

		/// <summary>Performs clean-up for the class</summary>
		public void CleanUp()
		{
			// delete temp files, etc
		}

		/// <summary>Returns the normalized name to use for the output corpus folder. A sub-directory of <c>_corpusOutputFolder</c></summary>
		public string CorpusDirectoryName
		{
			get
			{
				if (!Directory.Exists(_corpusOutputFolder))
					throw new DirectoryNotFoundException(string.Format("The path {0} was not found.", _corpusOutputFolder));

				if (string.IsNullOrEmpty(_corpusDirectoryName))
				{
					var test = NormalizeDirectoryName(_corpusName);
					var i = 1;

					while (Directory.Exists(Path.Combine(_corpusOutputFolder, test)))
					{
						test = NormalizeDirectoryName(_corpusName) + "_" + i.ToString("000");
						i++;
					}
					_corpusDirectoryName = test;
				}
				return _corpusDirectoryName;
			}
		}
	}
}
