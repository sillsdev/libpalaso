using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SIL.Archiving.Generic;

namespace SIL.Archiving.IMDI
{
	/// <summary>Implements archiving for IMDI repositories</summary>
	public class IMDIArchivingDlgViewModel : ArchivingDlgViewModel
	{
		private IMDIPackage _imdiData;
		private readonly bool _corpus;
		private readonly string _outputFolder;
		private string _corpusDirectoryName;

		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission</param>
		/// <param name="id">Identifier (used as filename) for the package being created</param>
		/// <param name="corpus">Indicates whether this is for an entire project corpus or a
		/// single session</param>
		/// <param name="setFilesToArchive">Delegate to request client to call methods to set
		/// which files should be archived (this is deferred to allow display of progress message)</param>
		/// <param name="outputFolder">Base folder where IMDI file structure is to be created</param>
		/// ------------------------------------------------------------------------------------
		public IMDIArchivingDlgViewModel(string appName, string title, string id, bool corpus,
			Action<ArchivingDlgViewModel> setFilesToArchive, string outputFolder)
			: base(appName, title, id, setFilesToArchive)
		{
			_corpus = corpus;
			_outputFolder = outputFolder;
		}

		/// ------------------------------------------------------------------------------------
		protected override bool DoArchiveSpecificInitialization()
		{
			_imdiData = new IMDIPackage(_corpus)
			{
				Title = _titles[_id],
				Name = _id
			};

			return true;
		}

		/// ------------------------------------------------------------------------------------
		public override int CalculateMaxProgressBarValue()
		{
			// One for processing each list and one for copying each file
			return _fileLists.Count + _fileLists.SelectMany(kvp => kvp.Value.Item1).Count();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets a description for the specified session in a single language
		/// </summary>
		/// <param name="sessionId"></param>
		/// <param name="description">The abstract description</param>
		/// <param name="language">ISO 639-2 3-letter language code</param>
		/// ------------------------------------------------------------------------------------
		public void SetSessionDescription(string sessionId, string description, string language)
		{
			if (description == null)
				throw new ArgumentNullException("description");
			if (language == null)
				throw new ArgumentNullException("language");
			if (language.Length != 3)
				throw new ArgumentException("ISO 639-2 3-letter language code required.", "language");

			PreventDuplicateMetadataProperty(sessionId, MetadataProperties.AbstractDescription);

			throw new NotImplementedException("Need to add description element to session IMDI package");
		}

		/// <summary></summary>
		/// <param name="descriptions"></param>
		protected override void SetAbstract_Impl(IDictionary<string, string> descriptions)
		{
			foreach (var desc in descriptions)
				_imdiData.AddDescription(new LanguageString(desc.Value, desc.Key));
		}

		/// <summary></summary>
		/// <returns></returns>
		public override string GetMetadata()
		{
			return _imdiData.BaseImdiFile.ToString();
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
					var test = NormalizeDirectoryName(_id);
					var i = 1;

					while (Directory.Exists(Path.Combine(_outputFolder, test)))
					{
						test = NormalizeDirectoryName(_id) + "_" + i.ToString("000");
						i++;
					}
					_corpusDirectoryName = test;
				}
				return _corpusDirectoryName;
			}
		}
	}
}
