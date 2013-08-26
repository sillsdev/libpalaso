using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using L10NSharp;
using Palaso.UI.WindowsForms;
using Palaso.IO;
using Palaso.UI.WindowsForms.Miscellaneous;
using Palaso.UI.WindowsForms.Progress;
using SIL.Archiving.Properties;
using Timer = System.Threading.Timer;

namespace SIL.Archiving
{
	public class ArchivingDlgViewModel
	{
#if !__MonoCS__
		[DllImport("User32.dll")]
		private static extern IntPtr SetForegroundWindow(int hWnd);

		[DllImport("User32.dll")]
		private static extern bool BringWindowToTop(int hWnd);
#endif

		#region RAMP and METS constants
		public const string kRampProcessName = "RAMP";
		public const string kRampFileExtension = ".ramp";
		public const string kPackageTitle = "dc.title";
		public const string kSourceFilesForMets = "files";
		public const string kImageExtent = "format.extent.images";
		public const string kFileTypeModeList = "dc.type.mode";

		public const string kModeSpeech = "Speech";
		public const string kModeVideo = "Video";
		public const string kModeText = "Text";
		public const string kModePhotograph = "Photograph";
		public const string kModeMusicalNotation = "Musical notation";
		public const string kModeDataset = "Dataset";
		public const string kModeSoftwareOrFont = "Software application";
		public const string kModePresentation = "Presentation";

		public const string kDefaultKey = " ";
		public const string kSeparator = ",";
		public const string kFileDescription = "description";
		public const string kFileRelationship = "relationship";
		public const string kRelationshipSource = "Source";
		public const string kRelationshipPresentation = "Presentation";
		public const string kRelationshipSupporting = "Supporting";
		#endregion

		private readonly string _title;
		private readonly string _id;
		private readonly IEnumerable<string> _appSpecificMetsPairs;
		private readonly Func<string, string, string> _getFileDescription; // first param is filelist key, second param is filename
		private string _metsFilePath;
		private string _tempFolder;
		private BackgroundWorker _worker;
		private Timer _timer;
		private bool _cancelProcess;
		private bool _workerException;
		private readonly Dictionary<string, string> _progressMessages = new Dictionary<string, string>();
		private string _rampProgramPath;
		private Action _incrementProgressBarAction;
		private int _imageCount = -1;
		private IDictionary<string, Tuple<IEnumerable<string>, string>> _fileLists;

		#region properties
		public string AppName { get; private set; }
		public bool IsBusy { get; private set; }
		public string RampPackagePath { get; private set; }
		public LogBox LogBox { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The font used in the Log box. Application can set this to ensure a consistent look
		/// in the UI.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Font ProgramDialogFont
		{
			get { return LogBox.Font; }
			set
			{
				if (value != null)
					LogBox.Font = FontHelper.MakeFont(value, FontStyle.Bold);
			}
		}

		#region callbacks
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback function to allow the application to modify the contents of a file rather
		/// than merely copying it. If application performs the "copy" for the given file,
		/// it should return true; otherwise, false.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Func<ArchivingDlgViewModel, string, string, bool> FileCopyOverride { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to do application-specific normalization of filenames to be added to
		/// archive based on the file-list key (param 1) and the filename (param 2).
		/// The StringBuilder (param 3) has the normalized name which the app can alter as needed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<string, string, StringBuilder> AppSpecificFilenameNormalization { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to allow application to do special handling of exceptions or other error
		/// conditions. The exception parameter can be null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<Exception, string> HandleNonFatalError { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to allow application to handly display of initial summary in log box. If
		/// the application implements this, then the default summary display will be suppressed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<IDictionary<string, Tuple<IEnumerable<string>, string>>, LogBox> OverrideDisplayInitialSummary { private get; set; }
		#endregion
		#endregion

		#region construction and initialization

		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission</param>
		/// <param name="id">Identifier (used as filename) for the package being created</param>
		/// <param name="appSpecificMetsPairs">Application-specific strings to be included in
		/// METS file. These need to be formatted correctly as JSON key-value pairs.</param>
		/// <param name="getFileDescription">Callback function to get a file description based
		/// on the file-list key (param 1) and the filename (param 2)</param>
		/// ------------------------------------------------------------------------------------
		public ArchivingDlgViewModel(string appName, string title, string id,
			IEnumerable<string> appSpecificMetsPairs, Func<string, string, string> getFileDescription)
		{
			if (appName == null)
				throw new ArgumentNullException("appName");
			AppName = appName;
			if (title == null)
				throw new ArgumentNullException("title");
			_title = title;
			if (id == null)
				throw new ArgumentNullException("id");
			_id = id;
			_appSpecificMetsPairs = appSpecificMetsPairs;
			if (getFileDescription == null)
				throw new ArgumentNullException("getFileDescription");
			_getFileDescription = getFileDescription;

			LogBox = new LogBox();
			LogBox.TabStop = false;
			LogBox.ShowMenu = false;

			foreach (var orphanedRampPackage in Directory.GetFiles(Path.GetTempPath(), "*" + kRampFileExtension))
			{
				try { File.Delete(orphanedRampPackage); }
				catch { }
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <param name="getFilesToArchive">delegate to retrieve the lists of files of files to
		/// archive, keyed and grouped according to whatever logical grouping makes sense in the
		/// calling application. The key for each group will be supplied back to the calling app
		/// for use in "normalizing" file names. For each group, in addition to the enumerated
		/// files to include (in Item1 of the Tuple), the calling app can provide a progress
		/// message (in Item2 of the Tuple) to be displayed when that group of files is being
		/// zipped and added to the RAMP file.</param>
		/// <param name="maxProgBarValue">Value calculated as the max value for the progress
		/// bar so the dialog can set that correctly</param>
		/// <param name="incrementProgressBarAction">Delegate to inform the dialog box that
		/// the progress should be incremented</param>
		/// ------------------------------------------------------------------------------------
		public bool Initialize(Func<IDictionary<string, Tuple<IEnumerable<string>, string>>> getFilesToArchive,
			out int maxProgBarValue, Action incrementProgressBarAction)
		{
			IsBusy = true;
			_incrementProgressBarAction = incrementProgressBarAction;

			var text = LocalizationManager.GetString("DialogBoxes.ArchivingDlg.SearchingForRampMsg",
				"Searching for the RAMP program...");

			LogBox.WriteMessage(text);
			Application.DoEvents();
			_rampProgramPath = FileLocator.GetFromRegistryProgramThatOpensFileType(kRampFileExtension) ??
				FileLocator.LocateInProgramFiles("ramp.exe", true, "ramp");

			LogBox.Clear();

			if (_rampProgramPath == null)
			{
				text = LocalizationManager.GetString("DialogBoxes.ArchivingDlg.RampNotFoundMsg",
					"The RAMP program cannot be found!");

				LogBox.WriteMessageWithColor("Red", text + Environment.NewLine);
			}

			_fileLists = getFilesToArchive();
			foreach (var fileList in _fileLists.Where(fileList => fileList.Value.Item1.Any()))
			{
				string normalizedName = NormalizeFilenameForRAMP(fileList.Key, Path.GetFileName(fileList.Value.Item1.First()));
				_progressMessages[normalizedName] = fileList.Value.Item2;
			}
			DisplayInitialSummary();
			IsBusy = false;

			// One for analyzing each list, one for copying each file, one for saving each file in the zip file
			// and one for the mets.xml file.
			maxProgBarValue = _fileLists.Count + 2 * _fileLists.SelectMany(kvp => kvp.Value.Item1).Count() + 1;

			return (_rampProgramPath != null);
		}

		/// ------------------------------------------------------------------------------------
		private void DisplayInitialSummary()
		{
			if (OverrideDisplayInitialSummary != null)
				OverrideDisplayInitialSummary(_fileLists, LogBox);
			else
			{
				LogBox.WriteMessage(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.PrearchivingStatusMsg",
					"The following files will be added to the archive:"));

				foreach (var kvp in _fileLists)
				{
					if (kvp.Key != string.Empty)
						LogBox.WriteMessage(Environment.NewLine + "    " + kvp.Key);

					foreach (var file in kvp.Value.Item1)
						LogBox.WriteMessageWithFontStyle(FontStyle.Regular, "          \u00B7 {0}", Path.GetFileName(file));
				}
			}

			LogBox.ScrollToTop();
		}

		#endregion

		#region RAMP calling methods
		/// ------------------------------------------------------------------------------------
		public bool CallRAMP()
		{
			if (!File.Exists(RampPackagePath))
			{
				ReportError(null, string.Format("RAMP package prematurely removed: {0}", RampPackagePath));
				return false;
			}

			try
			{
				var prs = new Process();
				prs.StartInfo.FileName = _rampProgramPath;
				prs.StartInfo.Arguments = "\"" + RampPackagePath + "\"";
				if (!prs.Start())
					return false;

				prs.WaitForInputIdle(8000);
				EnsureRampHasFocusAndWaitForPackageToUnlock();
				return true;
			}
			catch (InvalidOperationException)
			{
				EnsureRampHasFocusAndWaitForPackageToUnlock();
				return true;
			}
			catch (Exception e)
			{
				ReportError(e, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.StartingRampErrorMsg",
					"There was an error attempting to open the archive package in RAMP."));
				return false;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void EnsureRampHasFocusAndWaitForPackageToUnlock()
		{
#if !__MonoCS__
			var processes = Process.GetProcessesByName(kRampProcessName);
			if (processes.Length >= 1)
			{
				// I can't figure out why neither of these work.
				BringWindowToTop(processes[0].MainWindowHandle.ToInt32());
//				SetForegroundWindow(processes[0].MainWindowHandle.ToInt32());
			}
#else
			// Figure out how to do this in MONO
#endif
			// Every 4 seconds we'll check to see if the RAMP package is locked. When
			// it gets unlocked by RAMP, then we'll delete it.
			_timer = new Timer(CheckIfPackageFileIsLocked, RampPackagePath, 2000, 4000);
		}

		/// ------------------------------------------------------------------------------------
		private void CheckIfPackageFileIsLocked(Object packageFile)
		{
			if (!FileUtils.IsFileLocked(packageFile as string))
				CleanUpTempRampPackage();
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		public bool CreatePackage()
		{
			IsBusy = true;
			LogBox.Clear();

			var	success = CreateMetsFile() != null;

			if (success)
				success = CreateRampPackage();

			CleanUp();

			if (success)
			{
				LogBox.WriteMessageWithColor(Color.DarkGreen, Environment.NewLine +
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.ReadyToCallRampMsg",
					"Ready to hand the package to RAMP"));
			}

			IsBusy = false;
			return success;
		}

		#region Methods for creating mets file.
		/// ------------------------------------------------------------------------------------
		public string CreateMetsFile()
		{
			try
			{
				var bldr = new StringBuilder();

				foreach (var value in GetMetsPairs())
					bldr.AppendFormat("{0},", value);

				var jsonData = string.Format("{{{0}}}", bldr.ToString().TrimEnd(','));
				jsonData = JSONUtils.EncodeData(jsonData);
				var metsData = Resources.EmptyMets.Replace("<binData>", "<binData>" + jsonData);
				_tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(_tempFolder);
				_metsFilePath = Path.Combine(_tempFolder, "mets.xml");
				File.WriteAllText(_metsFilePath, metsData);
			}
			catch (Exception e)
			{
				if ((e is IOException) || (e is UnauthorizedAccessException) || (e is SecurityException))
				{
					ReportError(e, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CreatingInternalReapMetsFileErrorMsg",
						"There was an error attempting to create the RAMP/REAP mets file."));
					return null;
				}
				throw;
			}

			if (_incrementProgressBarAction != null)
				_incrementProgressBarAction();

			return _metsFilePath;
		}

		 /// ------------------------------------------------------------------------------------
		private IEnumerable<string> GetMetsPairs()
		{
			yield return JSONUtils.MakeKeyValuePair(kPackageTitle, _title);

			if (_appSpecificMetsPairs != null)
			{
				foreach (string appSpecificPair in _appSpecificMetsPairs)
					yield return appSpecificPair;
			}

			if (_fileLists != null)
			{
				string value = GetMode();
				if (value != null)
					yield return value;

				// Return JSON array of files with their descriptions.
				yield return JSONUtils.MakeArrayFromValues(kSourceFilesForMets,
					GetSourceFilesForMetsData(_fileLists));

				if (ImageCount > 0)
					yield return JSONUtils.MakeKeyValuePair(kImageExtent, ImageCount.ToString(CultureInfo.InvariantCulture));
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the number of image files in the list(s) of files to archive.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public int ImageCount
		{
			get
			{
				if (_fileLists != null && _imageCount < 0)
					GetMode();
				return _imageCount;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a comma-separated list of types found in the files to be archived
		/// (e.g. Text, Video, etc.).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private string GetMode()
		{
			_imageCount = 0;
			return GetMode(_fileLists.SelectMany(f => f.Value.Item1));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a comma-separated list of types found in the files to be archived
		/// (e.g. Text, Video, etc.).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetMode(IEnumerable<string> files)
		{
			if (files == null)
				return null;

			var list = new HashSet<string>();

			AddModesToSet(list, files);

			return JSONUtils.MakeBracketedListFromValues(kFileTypeModeList, list);
		}

		private void AddModesToSet(HashSet<string> list, IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				if (FileUtils.GetIsZipFile(file))
				{
					using (var zipFile = new ZipFile(file))
						AddModesToSet(list, zipFile.EntryFileNames);
					continue;
				}

				if (FileUtils.GetIsAudio(file))
					list.Add(kModeSpeech);
				if (FileUtils.GetIsVideo(file))
					list.Add(kModeVideo);
				if (FileUtils.GetIsText(file))
					list.Add(kModeText);
				if (FileUtils.GetIsImage(file))
					list.Add(kModePhotograph);
				if (FileUtils.GetIsMusicalNotation(file))
					list.Add(kModeMusicalNotation);
				if (FileUtils.GetIsDataset(file))
					list.Add(kModeDataset);
				if (FileUtils.GetIsSoftwareOrFont(file))
					list.Add(kModeSoftwareOrFont);
				if (FileUtils.GetIsPresentation(file))
					list.Add(kModePresentation);
			}
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<string> GetSourceFilesForMetsData(IDictionary<string, Tuple<IEnumerable<string>, string>> fileLists)
		{
			foreach (var kvp in fileLists)
			{
				foreach (var file in kvp.Value.Item1)
				{
					var description = _getFileDescription(kvp.Key, file);

					var fileName = NormalizeFilenameForRAMP(kvp.Key, Path.GetFileName(file));

					yield return JSONUtils.MakeKeyValuePair(kDefaultKey, fileName) + kSeparator +
						JSONUtils.MakeKeyValuePair(kFileDescription, description) + kSeparator +
						JSONUtils.MakeKeyValuePair(kFileRelationship, kRelationshipSource);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		public string NormalizeFilenameForRAMP(string key, string fileName)
		{
			StringBuilder bldr = new StringBuilder(fileName);
			int prevPeriod = -1;
			for (int i = 0; i < bldr.Length; i++)
			{
				if (bldr[i] == ' ')
					bldr[i] = '+';
				else if (bldr[i] == '.')
				{
					if (prevPeriod >= 0)
						bldr[prevPeriod] = '#';
					prevPeriod = i;
				}
			}
			if (AppSpecificFilenameNormalization != null)
				AppSpecificFilenameNormalization(key, fileName, bldr);
			return bldr.ToString();
		}
		#endregion

		#region Creating RAMP package (zip file) in background thread.
		/// ------------------------------------------------------------------------------------
		public bool CreateRampPackage()
		{
			try
			{
				RampPackagePath = Path.Combine(Path.GetTempPath(), _id + kRampFileExtension);

				using (_worker = new BackgroundWorker())
				{
					_cancelProcess = false;
					_workerException = false;
					_worker.ProgressChanged += HandleBackgroundWorkerProgressChanged;
					_worker.WorkerReportsProgress = true;
					_worker.WorkerSupportsCancellation = true;
					_worker.DoWork += CreateZipFileInWorkerThread;
					_worker.RunWorkerAsync();

					while (_worker.IsBusy)
						Application.DoEvents();
				}
			}
			catch (Exception e)
			{
				ReportError(e, LocalizationManager.GetString(
					"DialogBoxes.ArchivingDlg.CreatingZipFileErrorMsg",
					"There was a problem starting process to create zip file."));

				return false;
			}
			finally
			{
				_worker = null;
			}

			if (!File.Exists(RampPackagePath))
			{
				ReportError(null, string.Format("Failed to make the RAMP package: {0}", RampPackagePath));
				return false;
			}

			return !_cancelProcess && !_workerException;
		}

		/// ------------------------------------------------------------------------------------
		private void CreateZipFileInWorkerThread(object sender, DoWorkEventArgs e)
		{
			try
			{
				if (Thread.CurrentThread.Name == null)
					Thread.CurrentThread.Name = "CreateZipFileInWorkerThread";

				// Before adding the files to the RAMP (zip) file, we need to copy all the
				// files to a temp folder, flattening out the directory structure and renaming
				// the files as needed to comply with REAP guidelines.
				// REVIEW: Are multiple periods and/or non-Roman script really a problem?

				_worker.ReportProgress(0, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.PreparingFilesMsg",
					"Analyzing component files"));

				var filesToCopyAndZip = new Dictionary<string, string>();
				foreach (var list in _fileLists)
				{
					_worker.ReportProgress(1 /* actual value ignored, progress just increments */,
						string.IsNullOrEmpty(list.Key) ? _id: list.Key);
					foreach (var file in list.Value.Item1)
					{
						string newFileName = Path.GetFileName(file);
						newFileName = NormalizeFilenameForRAMP(list.Key, newFileName);
						filesToCopyAndZip[file] = Path.Combine(_tempFolder, newFileName);
					}
					if (_cancelProcess)
						return;
				}

				_worker.ReportProgress(0, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CopyingFilesMsg",
					"Copying files"));

				foreach (var fileToCopy in filesToCopyAndZip)
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
							var msg = LocalizationManager.GetString("DialogBoxes.ArchivingDlg.FileExcludedFromRAMP",
								"File excluded from RAMP package: " + fileToCopy.Value);
							ReportError(error, msg);
						}
					}
					// Don't use File.Copy because it's asynchronous.
					CopyFile(fileToCopy.Key, fileToCopy.Value);
				}

				_worker.ReportProgress(0, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.SavingFilesInRAMPMsg",
					"Saving files in RAMP package"));

				using (var zip = new ZipFile())
				{
					// RAMP packages must not be compressed or RAMP can't read them.
					zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
					zip.AddFiles(filesToCopyAndZip.Values, @"\");
					zip.AddFile(_metsFilePath, string.Empty);
					zip.SaveProgress += HandleZipSaveProgress;
					zip.Save(RampPackagePath);

					if (!_cancelProcess && _incrementProgressBarAction != null)
						Thread.Sleep(800);
				}
			}
			catch (Exception exception)
			{
				_worker.ReportProgress(0, new KeyValuePair<Exception, string>(exception,
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CreatingArchiveErrorMsg",
						"There was an error attempting to create the RAMP file.")));

				_workerException = true;
			}
		}

		/// ------------------------------------------------------------------------------------
		const int CopyBufferSize = 64 * 1024;
		static void CopyFile(string src, string dest)
		{
			using (var outputFile = File.OpenWrite(dest))
			{
				using (var inputFile = File.OpenRead(src))
				{
					var buffer = new byte[CopyBufferSize];
					int bytesRead;
					while ((bytesRead = inputFile.Read(buffer, 0, CopyBufferSize)) != 0)
					{
						outputFile.Write(buffer, 0, bytesRead);
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is called by the Save method on the ZipFile class as the zip file is being
		/// saved to the disk.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleZipSaveProgress(object s, SaveProgressEventArgs e)
		{
			if (_cancelProcess || e.EventType != ZipProgressEventType.Saving_BeforeWriteEntry)
				return;

			string msg;
			if (_progressMessages.TryGetValue(e.CurrentEntry.FileName, out msg))
				LogBox.WriteMessage(Environment.NewLine + msg);

			_worker.ReportProgress(e.EntriesSaved + 1, Path.GetFileName(e.CurrentEntry.FileName));
		}

		/// ------------------------------------------------------------------------------------
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
					LogBox.WriteMessageWithColor(Color.DarkGreen, Environment.NewLine + e.UserState);
					return;
				}

				LogBox.WriteMessageWithFontStyle(FontStyle.Regular, "\t" + e.UserState);
			}

			if (!_cancelProcess && _incrementProgressBarAction != null)
				_incrementProgressBarAction();
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		public void Cancel()
		{
			if (_cancelProcess)
				return;

			_cancelProcess = true;

			if (_worker != null)
			{
				LogBox.WriteMessageWithColor(Color.Red, Environment.NewLine +
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CancellingMsg", "Canceling..."));

				_worker.CancelAsync();
				while (_worker.IsBusy)
					Application.DoEvents();
			}

			CleanUp();
			CleanUpTempRampPackage();
		}

		/// ------------------------------------------------------------------------------------
		private void ReportError(Exception e, string msg)
		{
			if (LogBox.IsHandleCreated)
			{
				WaitCursor.Hide();
				LogBox.WriteError(msg, _title);
				if (e != null)
					LogBox.WriteException(e);
			}
			else if (e != null)
			{
				throw e;
			}
			if (HandleNonFatalError != null)
				HandleNonFatalError(e, msg);
		}

		#region Clean-up methods
		/// ------------------------------------------------------------------------------------
		public void CleanUp()
		{
			try { Directory.Delete(_tempFolder, true); }
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		public void CleanUpTempRampPackage()
		{
			// Comment out as a test !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			//try { File.Delete(RampPackagePath); }
			//catch { }

			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}

		#endregion
	}
}
