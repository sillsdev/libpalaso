using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L10NSharp;

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	public abstract class ArchivingDlgViewModel
	{
		#region Data members
		protected readonly string _title;
		protected readonly string _id;

		protected bool _cancelProcess;
		protected readonly Dictionary<string, string> _progressMessages = new Dictionary<string, string>();
		protected IDictionary<string, Tuple<IEnumerable<string>, string>> _fileLists;
		protected BackgroundWorker _worker;
		protected int _imageCount = -1;
		protected int _audioCount = -1;
		protected int _videoCount = -1;
		#endregion

		#region Delegates and Events
		/// ------------------------------------------------------------------------------------
		public enum MessageType
		{
			/// <summary>Normal (bold) text</summary>
			Normal,
			/// <summary>Blue text, with "Warning:" label (not localizable)</summary>
			Warning,
			/// <summary>Red text, followed by new line</summary>
			Error,
			/// <summary>Non-bold, indented with tab</summary>
			Detail,
			/// <summary>New line</summary>
			Progress,
			/// <summary>Non-bold, indented 8 spaces, with bullet character (U+00B7)</summary>
			Bullet,
			/// <summary>New line, Dark Green text</summary>
			Success,
			/// <summary>New line, indented 4 spaces</summary>
			Indented,
			/// <summary>Normal text, which will cause display to be cleared when the next message is to be displayed</summary>
			Volatile,
		}

		/// <summary>Delegate for OnDisplayMessage event</summary>
		/// <param name="msg">Message to display</param>
		/// <param name="type">Type of message (which handler can use to determine appropriate color, style, indentation, etc.</param>
		public delegate void DisplayMessageEventHandler(string msg, MessageType type);

		/// <summary>
		/// Notifiers subscribers of a message to display.
		/// </summary>
		public event DisplayMessageEventHandler OnDisplayMessage;

		/// <summary>Delegate for DisplayError event</summary>
		/// <param name="msg">Message to display</param>
		/// <param name="packageTitle">Title of package being created</param>
		/// <param name="e">Exception (can be null)</param>
		public delegate void DisplayErrorEventHandler(string msg, string packageTitle, Exception e);

		/// <summary>
		/// Notifiers subscribers of an error message to report.
		/// </summary>
		public event DisplayErrorEventHandler OnDisplayError;

		/// <summary>Action raised when progress happens</summary>
		public Action IncrementProgressBarAction { protected get; set; }
		#endregion

		#region properties
		/// ------------------------------------------------------------------------------------
		public string AppName { get; private set; }
		/// ------------------------------------------------------------------------------------
		public bool IsBusy { get; protected set; }



		#endregion

		#region callbacks
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback function to allow the application to modify the contents of a file rather
		/// than merely copying it. If application performs the "copy" for the given file,
		/// it should return true; otherwise, false.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Func<ArchivingDlgViewModel, string, string, bool> FileCopyOverride { protected get; set; }

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
		/// Callback to allow application to handle display of initial summary in log box. If
		/// the application implements this, then the default summary display will be suppressed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<IDictionary<string, Tuple<IEnumerable<string>, string>>> OverrideDisplayInitialSummary { private get; set; }
		#endregion

		#region construction and initialization
		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission</param>
		/// <param name="id">Identifier (used as filename) for the package being created</param>
		/// ------------------------------------------------------------------------------------
		protected ArchivingDlgViewModel(string appName, string title, string id)
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

		/// ------------------------------------------------------------------------------------
		public bool Initialize(Func<IDictionary<string, Tuple<IEnumerable<string>, string>>> getFilesToArchive,
			out int maxProgBarValue)
		{
			IsBusy = true;

			try
			{
				if (!DoArchiveSpecificInitialization())
				{
					maxProgBarValue = 0;
					return false;
				}

				_fileLists = getFilesToArchive();
				foreach (var fileList in _fileLists.Where(fileList => fileList.Value.Item1.Any()))
				{
					string normalizedName = NormalizeFilename(fileList.Key, Path.GetFileName(fileList.Value.Item1.First()));
					_progressMessages[normalizedName] = fileList.Value.Item2;
				}
				DisplayInitialSummary();

				// One for analyzing each list, one for copying each file, one for saving each file in the zip file
				// and one for the mets.xml file.
				maxProgBarValue = _fileLists.Count + 2 * _fileLists.SelectMany(kvp => kvp.Value.Item1).Count() + 1;

				return true;
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// ------------------------------------------------------------------------------------
		protected abstract bool DoArchiveSpecificInitialization();

		/// ------------------------------------------------------------------------------------
		private void DisplayInitialSummary()
		{
			if (OverrideDisplayInitialSummary != null)
				OverrideDisplayInitialSummary(_fileLists);
			else if (OnDisplayMessage != null)
			{
				OnDisplayMessage(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.PrearchivingStatusMsg",
						"The following files will be added to the archive:"), MessageType.Normal);

				foreach (var kvp in _fileLists)
				{
					if (kvp.Key != string.Empty)
						OnDisplayMessage(kvp.Key, MessageType.Indented);

					foreach (var file in kvp.Value.Item1)
						OnDisplayMessage(Path.GetFileName(file), MessageType.Bullet);
				}
			}
		}
		#endregion

		/// ------------------------------------------------------------------------------------
		public void DisplayMessage(string msg, MessageType type)
		{
			if (OnDisplayMessage != null)
				OnDisplayMessage(msg, type);
		}

		/// ------------------------------------------------------------------------------------
		public abstract bool LaunchArchivingProgram();
		/// ------------------------------------------------------------------------------------
		public abstract bool CreatePackage();

		/// ------------------------------------------------------------------------------------
		protected virtual StringBuilder DoArchiveSpecificFilenameNormalization(string key, string fileName)
		{
			return AppSpecificFilenameNormalization != null ? new StringBuilder(fileName) : null;
		}

		/// ------------------------------------------------------------------------------------
		public virtual string NormalizeFilename(string key, string fileName)
		{
			StringBuilder bldr = DoArchiveSpecificFilenameNormalization(key, fileName);
			if (AppSpecificFilenameNormalization != null)
				AppSpecificFilenameNormalization(key, fileName, bldr);
			return bldr.ToString();
		}

		/// ------------------------------------------------------------------------------------
		const int CopyBufferSize = 64 * 1024;
		/// ------------------------------------------------------------------------------------
		protected static void CopyFile(string src, string dest)
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
		public virtual void Cancel()
		{
			if (_cancelProcess)
				return;

			_cancelProcess = true;

			if (_worker != null)
			{
				DisplayMessage(Environment.NewLine + LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.CancellingMsg", "Canceling..."), MessageType.Error);

				_worker.CancelAsync();
				while (_worker.IsBusy)
					Application.DoEvents();
			}
		}

		/// ------------------------------------------------------------------------------------
		protected void ReportError(Exception e, string msg)
		{
			if (OnDisplayError != null)
				OnDisplayError(msg, _title, e);
			else if (e != null)
				throw e;

			if (HandleNonFatalError != null)
				HandleNonFatalError(e, msg);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The file locations are different on Linux than on Windows
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool IsMono
		{
			get { return (Type.GetType("Mono.Runtime") != null); }
		}
	}
}
