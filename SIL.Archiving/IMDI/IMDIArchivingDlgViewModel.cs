using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Schema;
using SIL.Extensions;
using SIL.WritingSystems;
using static System.String;

namespace SIL.Archiving.IMDI
{
	/// <summary>Implements archiving for IMDI repositories</summary>
	public class IMDIArchivingDlgViewModel : ArchivingDlgViewModel, ISupportMetadataOnly
	{
		private readonly IMDIPackage _imdiData;
		private string _corpusDirectoryName;
		private string _programPreset;
		private string _otherProgramPath;
		private readonly string _configFileName = Path.Combine(ArchivingFileSystem.SilCommonArchivingDataFolder, "IMDIProgram.config");
		private string _outputFolder;

		#region Properties

		public override Standard ArchiveType => Standard.IMDI;

		public override string NameOfProgramToLaunch
		{
			get
			{
				// DO NOT SHOW THE LAUNCH OPTION AT THIS TIME
				return null;

				//if (string.IsNullOrEmpty(PathToProgramToLaunch))
				//    return null;

				//// Arbil
				//if (PathToProgramToLaunch.ToLower().Contains("arbil")) return "Arbil";


				//// if not one of the presets, just return the exe name
				//string exe = Path.GetFileNameWithoutExtension(PathToProgramToLaunch);
				//string dir = Path.GetDirectoryName(PathToProgramToLaunch);
				//if (!string.IsNullOrEmpty(dir))
				//{
				//    dir = Path.GetFileNameWithoutExtension(dir);
				//    if (dir.Length > 0 && exe.ToLowerInvariant().Contains(dir.ToLowerInvariant()))
				//        return dir;
				//}
				//return exe;
			}
		}

		/// ------------------------------------------------------------------------------------
		public override string ArchiveInfoUrl => Properties.Settings.Default.IMDIWebSite;

		public bool MetadataOnly { get; set; }
		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission.</param>
		/// <param name="id">Identifier for the package being created. Used as the CORPUS name.</param>
		/// <param name="corpus">Indicates whether this is for an entire project corpus or a
		/// single session</param>
		/// <param name="setFilesToArchive">Delegate to request client to call methods to set
		/// which files should be archived (this is deferred to allow display of progress message)</param>
		/// <param name="outputFolder">Base folder where IMDI file structure is to be created</param>
		/// ------------------------------------------------------------------------------------
		public IMDIArchivingDlgViewModel(string appName, string title, string id, bool corpus,
			Action<ArchivingDlgViewModel, CancellationToken> setFilesToArchive, string outputFolder)
			: base(appName, title, id, setFilesToArchive)
		{
			OutputFolder = outputFolder;

			_imdiData = new IMDIPackage(corpus, PackagePath)
			{
				Title = PackageTitle,
				Name = PackageId
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
			return FileLists.Count + FileLists.SelectMany(kvp => kvp.Value.Item1).Count();
		}

		/// ------------------------------------------------------------------------------------
		protected override string FileGroupDisplayMessage(string groupKey)
		{
			if (groupKey == Empty)
				Progress.GetMessage(StringId.IMDIActorsGroup);
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
		[PublicAPI]
		public void SetSessionDescription(string sessionId, string description, string iso3LanguageCode)
		{
			if (description == null)
				throw new ArgumentNullException(nameof(description));
			// This will throw an appropriate exception if the language code is not valid:
			IetfLanguageTag.Create(iso3LanguageCode, null, null, null);

			_imdiData.AddDescription(sessionId, new LanguageString { Value = description, Iso3LanguageId = iso3LanguageCode });
		}

		/// ------------------------------------------------------------------------------------
		protected override void SetAbstract_Impl(IDictionary<string, string> descriptions)
		{
			foreach (var desc in descriptions)
				_imdiData.AddDescription(new LanguageString(desc.Value, desc.Key));
		}


		/// ------------------------------------------------------------------------------------
		public override string GetMetadata()
		{
			return _imdiData.BaseImdiFile.ToString();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>Launch Arbil or Lamus or whatever</summary>
		/// <remarks>Need custom launcher here because Arbil is a java program, with no
		/// executable on linux</remarks>
		/// ------------------------------------------------------------------------------------
		public override void LaunchArchivingProgram()
		{
			if (IsNullOrEmpty(PathToProgramToLaunch) || !File.Exists(PathToProgramToLaunch))
				return;

			// if it is a .jar file, open with java
			var exePath = PathToProgramToLaunch.EndsWith(".jar") ? "java" : PathToProgramToLaunch;
			var args = Empty;
			if (exePath == "java")
			{
				// are there additional command line parameters for this program?
				args = PathToProgramToLaunch.ToLower().Contains("arbil") ?
					Format(ArchivingPrograms.ArbilCommandLineArgs, PathToProgramToLaunch) :
					PathToProgramToLaunch;
			}

			var prs = new Process { StartInfo = { FileName = exePath, Arguments = args } };

			LaunchArchivingProgram(prs);
		}

#region Create IMDI package asynchronously

		/// ------------------------------------------------------------------------------------
		public override async Task<string> CreatePackage(CancellationToken cancellationToken)
		{
			// check for missing data that is required by Arbil
			var success = _imdiData.SetMissingInformation();

			// write the xml files
			if (success)
				success = _imdiData.CreateIMDIPackage(); // REVIEW: Should this also be awaited?

			// copy the content files
			if (success && !MetadataOnly)
				success = await CreateIMDIPackageAsync(cancellationToken);

			CleanUp();

			return success ? _imdiData.MainExportFile : null;
		}

		/// ------------------------------------------------------------------------------------
		private Task<bool> CreateIMDIPackageAsync(CancellationToken cancellationToken)
		{
			try
			{
				var outputDirectory = Path.Combine(_imdiData.PackagePath, NormalizeDirectoryName(_imdiData.Name));

				ReportMajorProgressPoint(StringId.PreparingFiles, cancellationToken);

				var filesToCopy = new Dictionary<string, string>();

				// get files from each session
				foreach (var session in _imdiData.Sessions.OfType<Session>())
				{
					ReportProgress(session.Name, MessageType.Detail, cancellationToken);

					// get files to copy
					foreach (var file in session.Resources.MediaFile)
					{
						// create sub directory
						var fullSessionDirName = Path.Combine(outputDirectory, NormalizeDirectoryName(file.OutputDirectory));
						Directory.CreateDirectory(fullSessionDirName);

						var newFileName = NormalizeFilename(Empty, Path.GetFileName(file.FullPathAndFileName));
						filesToCopy[file.FullPathAndFileName] = Path.Combine(fullSessionDirName, newFileName);
					}

					foreach (var file in session.Resources.WrittenResource)
					{
						// create sub directory
						var fullSessionDirName = Path.Combine(outputDirectory, NormalizeDirectoryName(file.OutputDirectory));
						Directory.CreateDirectory(fullSessionDirName);

						var newFileName = NormalizeFilename(Empty, Path.GetFileName(file.FullPathAndFileName));
						filesToCopy[file.FullPathAndFileName] = Path.Combine(fullSessionDirName, newFileName);
					}
				}

				ReportMajorProgressPoint(StringId.CopyingFiles, cancellationToken);

				// copy the files now
				foreach (var fileToCopy in filesToCopy)
				{
					ReportProgress(Path.GetFileName(fileToCopy.Key), MessageType.Detail, cancellationToken);
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
							var msg = GetFileExcludedMsg(fileToCopy.Value);
							ReportError(error, msg);
						}
					}
					// Don't use File.Copy because it's asynchronous.
					CopyFile(fileToCopy.Key, fileToCopy.Value);
				}

				ReportMajorProgressPoint(StringId.SavingFilesInPackage, cancellationToken, false);

				return Task.FromResult(true);
			}
			catch (OperationCanceledException)
			{
				return Task.FromResult(false);
			}
			catch (Exception exception)
			{
				ReportError(exception, Progress.GetMessage(StringId.ErrorCreatingArchive));
				return Task.FromResult(false);
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

		/// <summary>Returns the normalized name to use for the output corpus folder. A sub-directory of <c>OutputFolder</c></summary>
		public string CorpusDirectoryName
		{
			get
			{
				// create the output base directory if it doesn't already exist
				Directory.CreateDirectory(OutputFolder);
				
				if (IsNullOrEmpty(_corpusDirectoryName))
				{
					var baseName = NormalizeDirectoryName(PackageTitle + " " + DateTime.Today.ToISO8601TimeFormatDateOnlyString());
					var test = baseName;
					var i = 1;

					while (Directory.Exists(Path.Combine(OutputFolder, test)))
					{
						test = NormalizeDirectoryName(baseName + " " + i.ToString("000"));
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
			var session = _imdiData.Sessions.FirstOrDefault(s => s.Name == sessionId);

			if (session != null)
				return session;

			// if not found, add a new session
			session = new Session {Name = sessionId};

			_imdiData.Sessions.Add(session);

			return session;
		}

		public override IArchivingPackage ArchivingPackage => _imdiData;

		/// <summary></summary>
		public new string PathToProgramToLaunch
		{
			get
			{
				switch (ProgramPreset)
				{
					case "Arbil":
						return ArchivingPrograms.GetArbilExeFileLocation();

					default:
						return OtherProgramPath;
				}
			}
			set => _otherProgramPath = value; // this is just for compatibility
		}

		/// <summary></summary>
		public string ProgramPreset
		{
			get
			{
				if (IsNullOrEmpty(_programPreset))
					GetSavedValues();

				return _programPreset;
			}
			set
			{
				_programPreset = value;
				SaveProgramValues();
			}
		}

		/// <summary></summary>
		public string OtherProgramPath
		{
			get
			{
				if (IsNullOrEmpty(_programPreset))
					GetSavedValues();

				return _otherProgramPath;
			}
			set
			{
				_otherProgramPath = value;
				SaveProgramValues();
			}
		}

		public void GetSavedValues()
		{

			if (File.Exists(_configFileName))
			{
				var lines = File.ReadAllLines(_configFileName);

				foreach (var line in lines)
				{
					var kvp = line.Split(new[] { '=' }, 2);
					if (kvp.Length == 2)
					{
						switch (kvp[0])
						{
							case "ProgramPreset":
								_programPreset = kvp[1];
								break;

							case "OtherProgramPath":
								_otherProgramPath = kvp[1];
								break;
						}
					}
				}
			}

			// default to Arbil
			if (IsNullOrEmpty(_programPreset))
				_programPreset = "Arbil";

			_otherProgramPath ??= Empty;
		}

		private void SaveProgramValues()
		{
			List<string> lines = new List<string>
			{
				"ProgramPreset=" + ProgramPreset,
				"OtherProgramPath=" + OtherProgramPath
			};

			File.WriteAllLines(_configFileName, lines);
		}

		/// <summary />
		public string OutputFolder
		{
			get => _outputFolder;
			set
			{
				_outputFolder = value;
				PackagePath = !IsNullOrEmpty(value)?
					Path.Combine(value, CorpusDirectoryName):
					CorpusDirectoryName;
				if (_imdiData != null)
					_imdiData.PackagePath = PackagePath;
			}
		}
	}
}
