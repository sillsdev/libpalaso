using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using L10NSharp;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Schema;
using SIL.Archiving.Properties;
using System.Windows.Forms;

namespace SIL.Archiving.IMDI
{
	/// <summary>Implements archiving for IMDI repositories</summary>
	public class IMDIArchivingDlgViewModel : ArchivingDlgViewModel
	{
		private readonly IMDIPackage _imdiData;
		private readonly string _outputFolder;
		private string _corpusDirectoryName;
		private bool _workerException;

		#region Properties
		/// ------------------------------------------------------------------------------------
		internal override string ArchiveType
		{
			get
			{
				return LocalizationManager.GetString("DialogBoxes.ArchivingDlg.IMDIArchiveType", "IMDI",
					"This is the abbreviation for Isle Metadata Initiative (http://www.mpi.nl/imdi/). " +
						"Typically this probably does not need to be localized.");
			}
		}

		/// ------------------------------------------------------------------------------------
		public override string NameOfProgramToLaunch
		{
			get
			{
				if (string.IsNullOrEmpty(PathToProgramToLaunch))
					return null;
				string exe = Path.GetFileNameWithoutExtension(PathToProgramToLaunch);
				string dir = Path.GetDirectoryName(PathToProgramToLaunch);
				if (!string.IsNullOrEmpty(dir))
				{
					dir = Path.GetFileNameWithoutExtension(dir);
					if (dir.Length > 0 && exe.ToLowerInvariant().Contains(dir.ToLowerInvariant()))
						return dir;
				}
				return exe;
			}
		}

		/// ------------------------------------------------------------------------------------
		public override string InformativeText
		{
			get
			{
				string programInfo = string.IsNullOrEmpty(NameOfProgramToLaunch) ?
					string.Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.NoIMDIProgramInfoText",
					"The {0} package will be created in {1}.",
					"Parameter 0 is 'IMDI'; " +
					"Parameter 1 is the path where the package is created."),
					ArchiveType, PackagePath)
					:
					string.Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.IMDIProgramInfoText",
					"This tool will help you use {0} to archive your {1} data. When the {1} package has been " +
					"created, you can launch {0} and enter any additional information before doing the actual submission.",
					"Parameter 0 is the name of the program that will be launched to further prepare the IMDI data for submission; " +
					"Parameter 1 is the name of the calling (host) program (SayMore, FLEx, etc.)"), NameOfProgramToLaunch, AppName);
				return string.Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.IMDIOverviewText",
					"{0} ({1}) is a metadata standard to describe multi-media and multi-modal language " +
					"resources. The standard provides interoperability for browsable and searchable " +
					"corpus structures and resource descriptions.",
					"Parameter 0  is 'Isle Metadata Initiative' (the first occurrence will be turned into a hyperlink); " +
					"Parameter 1 is 'IMDI'"),
					ArchiveInfoHyperlinkText, ArchiveType) +
					" " + _appSpecificArchivalProcessInfo +
					" " + programInfo;
			}
		}

		/// <summary></summary>
		public override string ArchiveInfoHyperlinkText
		{
			get { return LocalizationManager.GetString("DialogBoxes.ArchivingDlg.IsleMetadataInitiative",
				"Isle Metadata Initiative", "Typically this probably does not need to be localized."); }
		}

		/// ------------------------------------------------------------------------------------
		public override string ArchiveInfoUrl
		{
			get { return Settings.Default.IMDIWebSite; }
		}
		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission.</param>
		/// <param name="id">Identifier for the package being created. Used as the CORPUS name.</param>
		/// <param name="appSpecificArchivalProcessInfo">Application can use this to pass
		/// additional information that will be displayed to the user in the dialog to explain
		/// any application-specific details about the archival process.</param>
		/// <param name="corpus">Indicates whether this is for an entire project corpus or a
		/// single session</param>
		/// <param name="setFilesToArchive">Delegate to request client to call methods to set
		/// which files should be archived (this is deferred to allow display of progress message)</param>
		/// <param name="outputFolder">Base folder where IMDI file structure is to be created</param>
		/// ------------------------------------------------------------------------------------
		public IMDIArchivingDlgViewModel(string appName, string title, string id,
			string appSpecificArchivalProcessInfo, bool corpus,
			Action<ArchivingDlgViewModel> setFilesToArchive, string outputFolder)
			: base(appName, title, id, appSpecificArchivalProcessInfo, setFilesToArchive)
		{
			_outputFolder = outputFolder;

			PackagePath = Path.Combine(_outputFolder, NormalizeDirectoryName(title));

			_imdiData = new IMDIPackage(corpus, PackagePath)
			{
				Title = _titles[_id],
				Name = _id
			};
		}

		/// ------------------------------------------------------------------------------------
		protected override bool DoArchiveSpecificInitialization()
		{
			// no-op
			return true;
		}

		/// ------------------------------------------------------------------------------------
		public override int CalculateMaxProgressBarValue()
		{
			// One for processing each list and one for copying each file
			return _fileLists.Count + _fileLists.SelectMany(kvp => kvp.Value.Item1).Count();
		}

		/// ------------------------------------------------------------------------------------
		protected override string FileGroupDisplayMessage(string groupKey)
		{
			if (groupKey == string.Empty)
				return LocalizationManager.GetString("DialogBoxes.ArchivingDlg.IMDIActorsGroup", "Actors",
					"This is the heading displayed in the Archive Using IMDI dialog box for the files for the actors/participants");
			return base.FileGroupDisplayMessage(groupKey);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets a description for the specified session in a single language
		/// </summary>
		/// <param name="sessionId"></param>
		/// <param name="description">The abstract description</param>
		/// <param name="iso3LanguageCode">ISO 639-3 3-letter language code</param>
		/// ------------------------------------------------------------------------------------
		public void SetSessionDescription(string sessionId, string description, string iso3LanguageCode)
		{
			if (description == null)
				throw new ArgumentNullException("description");
			if (iso3LanguageCode == null)
				throw new ArgumentNullException("iso3LanguageCode");
			if (iso3LanguageCode.Length != 3)
			{
				var msg = LocalizationManager.GetString("DialogBoxes.ArchivingDlg.ISO3CodeRequired",
					"ISO 639-3 3-letter language code required.",
					"Message displayed when an invalid language code is given.");
				throw new ArgumentException(msg, "iso3LanguageCode");
			}

			_imdiData.AddDescription(sessionId, new LanguageString { Value = description, Iso3LanguageId = iso3LanguageCode });
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

		/// ------------------------------------------------------------------------------------
		/// <summary>Launch Arbil or Lamus or whatever</summary>
		/// ------------------------------------------------------------------------------------
		internal override void LaunchArchivingProgram()
		{
			LaunchArchivingProgram(null);
		}

#region Create IMDI package in worker thread

		/// <summary></summary>
		public override bool CreatePackage()
		{
			IsBusy = true;

			// check for missing data that is required by Arbil
			var success = _imdiData.SetMissingInformation();

			// write the xml files
			if (success)
				success = _imdiData.CreateIMDIPackage();

			// copy the content files
			if (success)
				success = CreateIMDIPackage();

			CleanUp();

			if (success)
			{
				DisplayMessage(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.ReadyToCallIMDIMsg",
					"Ready to hand the package to IMDI program"), MessageType.Success);
			}

			IsBusy = false;
			return success;
		}

		/// <summary></summary>
		public bool CreateIMDIPackage()
		{
			try
			{
				using (_worker = new BackgroundWorker())
				{
					_cancelProcess = false;
					_workerException = false;
					_worker.ProgressChanged += HandleBackgroundWorkerProgressChanged;
					_worker.WorkerReportsProgress = true;
					_worker.WorkerSupportsCancellation = true;
					_worker.DoWork += CreateIMDIPackageInWorkerThread;
					_worker.RunWorkerAsync();

					while (_worker.IsBusy)
						Application.DoEvents();
				}
			}
			catch (Exception e)
			{
				ReportError(e, LocalizationManager.GetString(
					"DialogBoxes.ArchivingDlg.CreatingIMDIPackageErrorMsg",
					"There was a problem starting process to create IMDI package."));

				return false;
			}
			finally
			{
				_worker = null;
			}

			return !_cancelProcess && !_workerException;
		}

		public override void Cancel()
		{
			base.Cancel();

			CleanUp();
		}

		/// <summary></summary>
		void HandleBackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.UserState == null || _cancelProcess)
				return;

			if (e.UserState is KeyValuePair<Exception, string>)
			{
				var kvp = (KeyValuePair<Exception, string>)e.UserState;
				ReportError(kvp.Key, kvp.Value);
				return;
			}

			if (!string.IsNullOrEmpty(e.UserState as string))
			{
				if (e.ProgressPercentage == 0)
				{
					DisplayMessage(e.UserState.ToString(), MessageType.Success);
					return;
				}

				DisplayMessage(e.UserState.ToString(), MessageType.Detail);
			}

			if (IncrementProgressBarAction != null)
				IncrementProgressBarAction();
		}

		private void CreateIMDIPackageInWorkerThread(object sender, DoWorkEventArgs e)

		{
			try
			{
				var outputDirectory = Path.Combine(_imdiData.PackagePath, NormalizeDirectoryName(_imdiData.Name));

				if (Thread.CurrentThread.Name == null)
					Thread.CurrentThread.Name = "CreateIMDIPackageInWorkerThread";

				_worker.ReportProgress(0, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.PreparingFilesMsg",
					"Analyzing component files"));

				var filesToCopy = new Dictionary<string, string>();

				// get files from each session
				foreach (var sess in _imdiData.Sessions)
				{
					Session session = (Session) sess;

					_worker.ReportProgress(1 /* actual value ignored, progress just increments */,
						session.Name);

					// get files to copy
					foreach (var file in session.Resources.MediaFile)
					{
						// create sub directory
						var fullSessionDirName = Path.Combine(outputDirectory, NormalizeDirectoryName(file.OutputDirectory));
						Directory.CreateDirectory(fullSessionDirName);

						var newFileName = NormalizeFilename(string.Empty, Path.GetFileName(file.FullPathAndFileName));
						filesToCopy[file.FullPathAndFileName] = Path.Combine(fullSessionDirName, newFileName);
					}

					foreach (var file in session.Resources.WrittenResource)
					{
						// create sub directory
						var fullSessionDirName = Path.Combine(outputDirectory, NormalizeDirectoryName(file.OutputDirectory));
						Directory.CreateDirectory(fullSessionDirName);

						var newFileName = NormalizeFilename(string.Empty, Path.GetFileName(file.FullPathAndFileName));
						filesToCopy[file.FullPathAndFileName] = Path.Combine(fullSessionDirName, newFileName);
					}

					if (_cancelProcess)
						return;
				}

				_worker.ReportProgress(0, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CopyingFilesMsg",
					"Copying files"));

				// copy the files now
				foreach (var fileToCopy in filesToCopy)
				{
					if (_cancelProcess)
						return;
					_worker.ReportProgress(1 /* actual value ignored, progress just increments */,
						Path.GetFileName(fileToCopy.Key));
					if (FileCopyOverride != null)
					{
						try
						{
							if (FileCopyOverride(this, fileToCopy.Key, fileToCopy.Value))
							{
								if (!File.Exists(fileToCopy.Value))
									throw new FileNotFoundException("Calling application claimed to copy file but didn't", fileToCopy.Value);
								continue;
							}
						}
						catch (Exception error)
						{
							var msg = LocalizationManager.GetString("DialogBoxes.ArchivingDlg.FileExcludedFromIMDI",
								"File excluded from IMDI package: " + fileToCopy.Value);
							ReportError(error, msg);
						}
					}
					// Don't use File.Copy because it's asynchronous.
					CopyFile(fileToCopy.Key, fileToCopy.Value);
				}

				_worker.ReportProgress(0, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.SavingFilesInIMDIMsg",
					"Saving files in IMDI package"));
			}
			catch (Exception exception)
			{
				_worker.ReportProgress(0, new KeyValuePair<Exception, string>(exception,
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CreatingIMDIArchiveErrorMsg",
						"There was an error attempting to create the IMDI package.")));

				_workerException = true;
			}
		}

#endregion

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

		/// <summary>Adds a new session and returns it</summary>
		/// <param name="sessionId"></param>
		public override IArchivingSession AddSession(string sessionId)
		{
			// look for existing session
			foreach (var sess in _imdiData.Sessions.Where(sess => sess.Name == sessionId))
				return sess;

			// if not found, add a new session
			Session session = new Session {Name = sessionId};

			_imdiData.Sessions.Add(session);

			return session;
		}

		/// <summary></summary>
		/// <param name="iso3LanguageId"></param>
		public void AddDocumentLanguage(string iso3LanguageId)
		{
			_imdiData.MetadataIso3LanguageIds.Add(iso3LanguageId);
		}

		/// <summary></summary>
		/// <param name="iso3LanguageId"></param>
		public void AddSubjectLanguage(string iso3LanguageId)
		{
			_imdiData.ContentIso3LanguageIds.Add(iso3LanguageId);
		}
	}
}
