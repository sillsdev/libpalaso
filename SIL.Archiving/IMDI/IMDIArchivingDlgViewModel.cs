using System;
using System.IO;
using System.Text;

namespace SIL.Archiving.IMDI
{
	/// <summary>Implements archiving for IMDI repositories</summary>
	public class IMDIArchivingDlgViewModel : ArchivingDlgViewModel
	{
		private readonly string _corpusName;
		private IMDIPackage _imdiData;
		private readonly string _outputFolder;
		private string _corpusDirectoryName;

		/// <summary>Constructor</summary>
		/// <param name="appName"></param>
		/// <param name="corpusName"></param>
		/// <param name="title"></param>
		/// <param name="id"></param>
		/// <param name="outputFolder"></param>
		public IMDIArchivingDlgViewModel(string appName, string corpusName, string title, string id, string outputFolder) : base(appName, title, id)
		{
			_corpusName = corpusName;
			_outputFolder = outputFolder;
		}

		/// <summary>Initialization</summary>
		protected override bool DoArchiveSpecificInitialization()
		{
			_imdiData = new IMDIPackage
			{
				Title = _title,
				Name = _corpusName
			};

			return true;
		}

		/// <summary>Launch Arbil or Lamus</summary>
		public override bool LaunchArchivingProgram()
		{
			throw new NotImplementedException();
		}

		/// <summary></summary>
		public override bool CreatePackage()
		{
			throw new NotImplementedException();
		}

		/// <summary>Only Latin characters, URL compatible</summary>
		protected override StringBuilder DoArchiveSpecificFilenameNormalization(string key, string fileName)
		{
			return new StringBuilder(NormalizeFileName(fileName));
		}

		/// <summary>Only Latin characters, URL compatible</summary>
		internal static string NormalizeFileName(string fileName)
		{
			return fileName.ToLatinOnly("_", "+", ".");
		}

		/// <summary>Only Latin characters, URL compatible</summary>
		internal static string NormalizeDirectoryName(string dirName)
		{
			return dirName.ToLatinOnly("_", "_", ".-");
		}

		/// <summary>Performs clean-up for the class</summary>
		public void CleanUp()
		{
			// delete temp files, etc
		}

		/// <summary>Returns the normalized name to use for the output corpus folder. A sub-directory of <c>_outputFolder</c></summary>
		public string CorpusDirectoryName
		{
			get
			{
				if (!Directory.Exists(_outputFolder))
					throw new DirectoryNotFoundException(string.Format("The path {0} was not found.", _outputFolder));

				if (string.IsNullOrEmpty(_corpusDirectoryName))
				{
					var test = NormalizeDirectoryName(_corpusName);
					var i = 1;

					while (Directory.Exists(Path.Combine(_outputFolder, test)))
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
